# Coordinate Systems Reference

## Overview

This document defines ALL coordinate systems used in the macOS window sitting implementation. **Every developer working on this code MUST read and understand this document.**

---

## The Five Coordinate Systems

### 1. Cocoa (NSWindow/NSScreen)
- **Origin**: Bottom-left of screen (0,0)
- **X-axis**: Left → Right (increases →)
- **Y-axis**: Bottom → Top (increases ↑)
- **Units**: **Points** (logical pixels)
- **Scale**: Does NOT include Retina scaling
- **Used by**: `NSWindow.frame`, `NSScreen.frame`, `UniWindowController.windowPosition`

```
Screen (1728x1117 points on Retina):
┌────────────────────────────────┐
│                            (1728,1117)
│                                │
│    Window at (100, 200)        │
│    means 100 pts from left,    │
│    200 pts from BOTTOM         │
│                                │
(0,0)────────────────────────────┘
```

### 2. CoreGraphics/CGWindowList (POINTS, not pixels!)
- **Origin**: Top-left of primary display (0,0)
- **X-axis**: Left → Right (increases →)
- **Y-axis**: Top → Bottom (increases ↓)
- **Units**: **POINTS** (NOT device pixels despite documentation suggesting otherwise)
- **Scale**: Does NOT include Retina scaling
- **Used by**: `CGWindowListCopyWindowInfo`, our native plugin, RECT struct

**CRITICAL DISCOVERY**: Despite Apple documentation suggesting CGWindowList uses "global display coordinates",
it actually returns coordinates in **points**, the same unit as NSWindow/NSScreen. This means:
- On a 1728x1117 point display (Retina 2x = 3456x2234 pixels), CGWindowList returns values in the 1728x1117 range
- NO scaling by backingScaleFactor is needed when converting between CGWindowList and NSWindow coordinates

```
Screen (1728x1117 points):
(0,0)────────────────────────────┐
│                                │
│    Window at (100, 200)        │
│    means 100 pts from left,    │
│    200 pts from TOP            │
│                                │
│                            (1728,1117)
└────────────────────────────────┘
```

### 3. Unity Screen Space (Camera/Input)
- **Origin**: Bottom-left of Unity window (0,0)
- **X-axis**: Left → Right (increases →)
- **Y-axis**: Bottom → Top (increases ↑)
- **Units**: **Logical pixels** (points)
- **Scale**: Does NOT include Retina scaling
- **Used by**: `Screen.width`, `Screen.height`, `Camera.WorldToScreenPoint()`, `Input.mousePosition`

### 4. Unity GUI Space (OnGUI/IMGUI)
- **Origin**: TOP-LEFT of Unity window (0,0) - **DIFFERENT FROM Screen Space!**
- **X-axis**: Left → Right (increases →)
- **Y-axis**: Top → Bottom (increases ↓) - **SAME AS CoreGraphics**
- **Units**: **Logical pixels**
- **Used by**: `GUI.DrawTexture`, `GUILayout`, `OnGUI` methods

**CRITICAL**: Unity has TWO different coordinate systems!
- Screen Space (Camera, Input): Bottom-left origin, Y up
- GUI Space (OnGUI): Top-left origin, Y down

### 5. Desktop Coordinates (Our RECT struct)
- **Origin**: Top-left of screen (0,0) - **SAME AS CoreGraphics/CGWindowList**
- **X-axis**: Left → Right (increases →)
- **Y-axis**: Top → Bottom (increases ↓)
- **Units**: **POINTS** (same as CGWindowList)
- **Scale**: Does NOT include Retina scaling
- **Used by**: `AvatarWindowHandler.RECT`, all hit-testing, snap calculations

---

## Conversion Rules (SIMPLIFIED - All in Points!)

### Rule 1: Cocoa ↔ CoreGraphics (Y-axis flip ONLY, no scaling!)

Since both CGWindowList and NSWindow use **points**, conversion is simple:

```
CoreGraphics_Y = ScreenHeight - Cocoa_Y - WindowHeight
Cocoa_Y = ScreenHeight - CoreGraphics_Y - WindowHeight
```

**NO SCALING NEEDED** - both systems use points!

### Rule 2: Unity Screen Space ↔ CGWindowList

Unity Screen Space is relative to the Unity window, CGWindowList is absolute screen position.
Unity's `Screen.width`/`Screen.height` may differ slightly from window frame size.

```csharp
// Calculate ratio between Unity Screen and window frame
float ratioX = Screen.width / unityWindowWidth;
float ratioY = Screen.height / unityWindowHeight;

// Convert relative position to Unity Screen coordinates
float unityScreenX = relativeX * ratioX;
float unityScreenY = relativeY * ratioY;
```

---

## Data Flow: Native → C# → Window Positioning (CORRECTED)

```
┌─────────────────────────────────────────────────────────────────────┐
│ 1. CGWindowListCopyWindowInfo() returns active window bounds        │
│    Coordinate System: CGWindowList (top-left origin, POINTS)        │
│    Example: {x:480, y:351, width:920, height:436}                   │
│    Units: POINTS (same as NSWindow!)                                │
└───────────────────────────┬─────────────────────────────────────────┘
                            │ Direct copy (no conversion)
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 2. MacOSWindowTracker.GetActiveWindowRectDesktop()                  │
│    Returns: RECT {Left:480, Top:351, Right:1400, Bottom:787}        │
│    Coordinate System: Desktop (top-left, POINTS)                    │
└───────────────────────────┬─────────────────────────────────────────┘
                            │ Same coordinate system
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 3. NSWindow.frame (Unity window position)                           │
│    Coordinate System: Cocoa (bottom-left, POINTS)                   │
│                                                                     │
│    Conversion in MacOS_GetUnityWindowBounds():                      │
│    yTop = screenHeight - yBottom - height (ALL IN POINTS)           │
│    NO SCALING BY backingScaleFactor!                                │
└───────────────────────────┬─────────────────────────────────────────┘
                            │ Now in CGWindowList coordinates
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 4. All calculations in AvatarWindowHandler                          │
│    Use Desktop/CGWindowList coordinates consistently                │
│    - GetUnityClientRect() → RECT in POINTS                          │
│    - ComputeDesktopFromWorld() → position in POINTS                 │
│    - Hit testing, snap detection → all in POINTS                    │
└───────────────────────────┬─────────────────────────────────────────┘
                            │ Convert BACK to Cocoa (just flip Y)
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 5. Setting window position via UniWindowController.windowPosition   │
│    REQUIRES: Cocoa coordinates (bottom-left, POINTS)                │
│                                                                     │
│    Conversion FROM CGWindowList TO Cocoa (Y-flip only!):            │
│    cocoaX = cgX          (no change)                                │
│    cocoaY = screenHeight - cgY - windowHeight                       │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Function Reference (CORRECTED)

### Native Plugin (MacOSWindowHelper.mm)

| Function | Returns | Coord System | Units |
|----------|---------|--------------|-------|
| `MacOS_GetActiveWindowBounds()` | BoundsResult | CGWindowList (top-left) | **POINTS** |
| `MacOS_GetDockBounds()` | BoundsResult | CGWindowList (top-left) | **POINTS** |
| `MacOS_GetUnityWindowBounds()` | BoundsResult | CGWindowList (top-left) | **POINTS** |
| `MacOS_GetScreenBounds()` | BoundsResult | CGWindowList (top-left) | **POINTS** |
| `MacOS_GetUnityBackingScale()` | float | N/A | Scale factor (for Unity Screen) |

### C# Wrappers (MacOSWindowHelper.cs, MacOSWindowTracker.cs)

| Function | Returns | Coord System | Units |
|----------|---------|--------------|-------|
| `GetActiveWindowRectDesktop()` | RECT | Desktop (=CGWindowList) | **POINTS** |
| `GetDockRectDesktop()` | RECT | Desktop (=CGWindowList) | **POINTS** |
| `GetUnityWindowBounds()` | BoundsResult | CGWindowList | **POINTS** |
| `GetScreenBounds()` | BoundsResult | CGWindowList | **POINTS** |

### AvatarWindowHandler.cs

| Function | Input Coord System | Output Coord System |
|----------|-------------------|---------------------|
| `GetUnityClientRect()` | N/A | Desktop (POINTS) |
| `ComputeDesktopFromWorld()` | World position | Desktop (POINTS) |
| `TrySnap()` | Desktop | Desktop |
| `PinToTarget()` | Desktop | Cocoa (via windowPosition) |
| `EnsureWindowOnScreen()` | Desktop | Cocoa (via windowPosition) |

---

## Common Mistakes to Avoid (UPDATED)

### Mistake 1: Scaling by backingScaleFactor when converting CGWindowList ↔ Cocoa
```csharp
// WRONG: Multiplying/dividing by scale
cocoaY = (screenHeight - cgY - height) / scale;  // DON'T DO THIS!

// RIGHT: Just flip Y, no scaling (both are in points)
cocoaY = screenHeight - cgY - height;
```

### Mistake 2: Assuming CGWindowList returns device pixels
```csharp
// WRONG: Treating CGWindowList values as device pixels
float devicePixelX = cgWindowBounds.x;  // These are POINTS, not pixels!

// RIGHT: CGWindowList returns points
float pointX = cgWindowBounds.x;  // Already in points
```

### Mistake 3: Double-flipping Y
```csharp
// WRONG: Data is already in CGWindowList (top-left), flipping again
py = windowTop + (cameraHeight - screenY);

// RIGHT: Data is in CGWindowList, use directly
py = windowTop + screenY;
```

### Mistake 4: Forgetting to flip Y when setting windowPosition
```csharp
// WRONG: Setting CGWindowList coordinates directly
winController.windowPosition = new Vector2(cgX, cgY);

// RIGHT: Convert to Cocoa (flip Y only)
float cocoaY = screenHeight - cgY - windowHeight;
winController.windowPosition = new Vector2(cgX, cocoaY);
```

---

## Quick Reference Card (SIMPLIFIED)

| From → To | X Transform | Y Transform |
|-----------|-------------|-------------|
| Cocoa → CGWindowList | (same) | `screenH - y - h` |
| CGWindowList → Cocoa | (same) | `screenH - y - h` |
| CGWindowList → Unity GUI | `× ratio` | `× ratio` (both top-left origin) |
| Unity Screen → Unity GUI | (same) | `screenH - y` (flip Y) |

**Note**: backingScaleFactor is only needed when converting between Unity Screen coordinates
and CGWindowList coordinates, as Unity may render at a different resolution than the window frame.

---

## Debug Visualizer Coordinate Flow

To draw a green overlay showing where another window is relative to the Unity window:

```csharp
// 1. Get active window bounds (CGWindowList coords: top-left, points)
RECT active = MacOSWindowTracker.GetActiveWindowRectDesktop();

// 2. Get Unity window bounds (CGWindowList coords: top-left, points)
var unity = MacOSWindowHelper.GetUnityWindowBounds();

// 3. Calculate relative position (still in points, top-left origin)
float relX = active.Left - unity.x;
float relY = active.Top - unity.y;
float relW = active.Right - active.Left;
float relH = active.Bottom - active.Top;

// 4. Scale to Unity GUI coordinates
// (GUI uses Screen.width/height which may differ from window frame)
float ratioX = Screen.width / unity.width;
float ratioY = Screen.height / unity.height;

float guiX = relX * ratioX;
float guiY = relY * ratioY;  // NO Y-FLIP! Both GUI and CGWindowList use top-left origin
float guiW = relW * ratioX;
float guiH = relH * ratioY;

// 5. Draw
GUI.DrawTexture(new Rect(guiX, guiY, guiW, guiH), Texture2D.whiteTexture);
```

---

## Testing Checklist

When making coordinate changes, verify:

1. [ ] Green debug rectangle overlays exactly on the active window
2. [ ] Character snaps to window top edge correctly
3. [ ] Window sitting works when Unity window is at screen edges
4. [ ] Window sitting works on Retina (2x) and non-Retina (1x) displays
5. [ ] Character stays on screen after app restart
