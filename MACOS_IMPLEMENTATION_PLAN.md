# macOS Implementation Plan - Difficulty & Priority Analysis

## Overview
The platform abstraction layer is complete with interfaces and Windows implementations. This document outlines implementation priorities for macOS, categorized by difficulty and dependencies.

---

## Service-by-Service Breakdown

### 1. **ISystemTrayService** - EASY WIN ⭐⭐⭐
**Current Status**: Windows fully implemented, Stub exists  
**Difficulty**: Low  
**Time Estimate**: 2-4 hours

#### Why It's Easy:
- Isolated functionality - no dependencies on other services
- macOS has NSStatusBar (Status Bar Items) - well-documented native API
- Similar feature parity to Windows: icon, tooltip, menu, events
- Can be implemented entirely in a single .cs file

#### Implementation Path:
```
MacOSSystemTrayService.cs
├── Uses NSStatusBar native API
├── Create NSStatusItem with icon
├── Build menu via NSMenu
├── Handle left-click events
└── No P/Invoke complexity required (use native plugin or Marshal calls)
```

#### What macOS Has:
- **NSStatusBar**: Equivalent to Windows system tray (menu bar items)
- **NSMenu**: Drop-down menu from status item
- **NSStatusItem**: The actual tray icon
- Events triggered naturally by clicks

#### Challenges:
- Need to load Texture2D → NSImage conversion
- Icon positioning in menu bar (macOS handles this)

**Verdict**: Start here! Quick win, builds confidence.

---

### 2. **IScreenService** - EASY WIN ⭐⭐⭐
**Current Status**: Windows fully implemented, Stub exists  
**Difficulty**: Low  
**Time Estimate**: 3-5 hours

#### Why It's Easy:
- Pure information retrieval (no complex state management)
- macOS has straightforward APIs: NSScreen, NSEvent
- No window manipulation required
- Minimal native interop needed

#### Implementation Path:
```
MacOSScreenService.cs
├── GetCursorPosition() → NSEvent.mouseLocation
├── SetCursorPosition() → CGWarpMouseCursorPosition (native)
├── GetPrimaryMonitor() → NSScreen.mainScreen
├── GetAllMonitors() → NSScreen.screens
└── GetVirtualDesktopBounds() → Aggregate all screens
```

#### What macOS Has:
- **NSScreen**: Monitor information (frame, visibleFrame, backingScaleFactor)
- **NSEvent.mouseLocation**: Current cursor position (global coords)
- **CGWarpMouseCursorPosition**: Move cursor programmatically
- **NSWorkspace.sharedWorkspace.activeSpaceNumber**: Virtual desktop info (10.5+)

#### Implementation Details:
| Feature | Windows | macOS | Difficulty |
|---------|---------|-------|-----------|
| Get cursor position | GetCursorPos() | NSEvent.mouseLocation | Trivial |
| Set cursor position | SetCursorPos() | CGWarpMouseCursorPosition | Trivial |
| Get monitors | EnumDisplayMonitors | NSScreen.screens | Low |
| Get primary monitor | GetPrimaryMonitor | NSScreen.mainScreen | Trivial |
| Virtual desktop bounds | GetSystemMetrics(SM_CXVIRTUALSCREEN) | Aggregate NSScreen frames | Low |

**Verdict**: Second priority! Information-only, no side effects.

---

### 3. **ITransparencyService** - MEDIUM ⭐⭐
**Current Status**: Windows fully implemented (DWM-based)  
**Difficulty**: Medium  
**Time Estimate**: 4-6 hours

#### Why It's Medium:
- macOS transparency is simpler than Windows DWM in some ways
- But macOS has quirks with window levels and fullscreen behavior
- Need native code for some operations

#### Implementation Path:
```
MacOSTransparencyService.cs
├── IsCompositionEnabled() → Always true on modern macOS
├── EnableTransparency() → Set window level & backgroundColor
├── SetTransparencyMargins() → Not applicable (macOS doesn't use margins)
├── SetWindowTransparency() → Set NSWindow.alphaValue
└── SetLayeredWindowAttributes() → Set window's background color alpha
```

#### What macOS Has:
- **NSWindow.alphaValue**: Direct window transparency (0.0-1.0)
- **NSWindow.level**: Floating/modal window levels (replaces Windows WS_EX_LAYERED)
- **NSWindow.backgroundColor**: Color with alpha for transparency
- **NSView.alphaValue**: Per-view transparency
- **CALayer**: For advanced transparency effects

#### macOS Transparency Approach:
1. **For simple alpha**: Set `NSWindow.alphaValue`
2. **For color key transparency**: Set `NSWindow.backgroundColor` with transparency
3. **For DWM-like effects**: Use `NSWindow.styleMask |= NSBorderlessWindowMask` with layers
4. **For fullscreen overlay**: Set appropriate `NSWindow.level` (e.g., `NSScreenSaverWindowLevel`)

#### Challenges:
- macOS doesn't have exact DWM equivalent
- Margin concept (SetTransparencyMargins) doesn't apply
- Fullscreen behavior differs significantly
- Need to set window level appropriately for overlay behavior

#### Windows vs macOS Transparency:
| Feature | Windows | macOS |
|---------|---------|-------|
| Basic alpha | SetLayeredWindowAttributes() | NSWindow.alphaValue |
| Color key | LWA_COLORKEY flag | NSColor with alpha |
| Margins | DwmExtendFrameIntoClientArea | Not applicable |
| Always on top | SetWindowPos with HWND_TOPMOST | NSWindow.level = NSFloatingWindowLevel |
| Performance | DWM accelerated | CALayer-backed |

**Verdict**: Medium priority. Wait until window service basics work.

---

### 4. **IWindowService** - HARD ⭐
**Current Status**: Windows fully implemented (40+ methods)  
**Difficulty**: Hard  
**Time Estimate**: 10-15 hours

#### Why It's Hard:
- Largest interface (50+ methods)
- Core to application functionality
- macOS window model fundamentally differs from Windows
- Many Windows concepts don't translate 1:1 to macOS
- Requires deep understanding of macOS window hierarchy
- Heavy use of native code required

#### Implementation Path:
```
MacOSWindowService.cs (with native plugin support)
├── Simple Methods (Trivial):
│   ├── GetMainWindowHandle() → Get Unity player window NSWindow*
│   ├── GetActiveWindow() → NSApplication.sharedApplication.keyWindow
│   └── GetForegroundWindow() → NSApplication.sharedApplication.mainWindow
│
├── Window State Methods (Easy):
│   ├── ShowWindow() → [window orderFront:nil]
│   ├── IsWindowVisible() → [window isVisible]
│   ├── IsWindowMinimized() → [window isMiniaturized]
│   └── IsWindowMaximized() → [window isZoomed]
│
├── Position/Size Methods (Medium):
│   ├── GetWindowRect() → [window frame]
│   ├── GetClientRect() → [[window contentView] bounds]
│   ├── SetWindowPosition() → [window setFrame:display:animate:]
│   ├── MoveWindow() → [window setFrame:display:]
│   └── ClientToScreen() → Convert view coordinates to screen
│
├── Window Properties (Hard):
│   ├── SetTopMost() → Adjust [window level] appropriately
│   ├── SetWindowStyle() → Modify styleMask (no exact equivalent)
│   ├── GetWindowStyle() → Extract styleMask bits
│   ├── GetWindowLong() → No macOS equivalent - must stub
│   └── SetWindowLong() → No macOS equivalent - must stub
│
├── Window Enumeration (Very Hard):
│   ├── EnumerateWindows() → CGWindowListCopyWindowInfo
│   ├── GetWindow() → Navigate sibling windows via private APIs
│   └── GetParent() → Use private accessibility APIs or CGWindowList
│
├── Z-Order & Layering (Very Hard):
│   ├── IsAboveInZOrder() → Compare window orders via CGWindowList
│   ├── GetWindowCloakingState() → Check window visibility flags
│   └── GetLayeredWindowAttributes() → Extract from window properties
│
└── Process Info (Medium):
    └── GetWindowProcessId() → Get NSRunningApplication from window
```

#### The Problem with macOS:
1. **No Window Handles**: macOS uses NSWindow objects, not numeric handles
   - Windows gives you an IntPtr (HWND)
   - macOS windows are objects - huge API difference
   - Workaround: Store a registry of window pointers

2. **No Direct Window Enumeration**: 
   - Windows: EnumWindows() callback is straightforward
   - macOS: Must use CGWindowListCopyWindowInfo (private API) or accessibility API
   - Much slower and less reliable

3. **No Z-Order Management**:
   - Windows: GetWindow(GW_HWNDPREV), SetWindowPos with insertion order
   - macOS: Window ordering is via NSWindow.level (discrete levels, not continuous)
   - Can't get "window below this one" easily

4. **No Window Styles**:
   - Windows: GetWindowLong(GWL_STYLE) extracts flags
   - macOS: styleMask has some flags, but many Windows concepts don't exist
   - GetWindowLong/SetWindowLong need stubs or must be implemented differently

5. **No Layered Windows**:
   - Windows: WS_EX_LAYERED + SetLayeredWindowAttributes for per-pixel alpha
   - macOS: Use NSWindow.alphaValue or CALayer transparency
   - Different paradigm entirely

#### Required Native Code:
You'll need a native plugin for several operations:
```objective-c
// Get all windows in order (macOS doesn't expose this easily)
CFArrayRef windows = CGWindowListCopyWindowInfo(...);

// Monitor window creation/destruction
NSNotificationCenter for NSWindowDidCreateNotification

// Access window properties
KVC on private properties like _windowLevel
```

#### What Maps Reasonably:
| Windows | macOS | Notes |
|---------|-------|-------|
| GetWindowRect() | [window frame] | Direct mapping |
| GetClientRect() | [[window contentView] bounds] | Direct mapping |
| IsWindowVisible() | [window isVisible] | Direct mapping |
| ShowWindow() | [window orderFront/Back/Out:] | Direct mapping |
| IsWindowMinimized() | [window isMiniaturized] | Direct mapping |
| SetTopMost() | [window setLevel:NSFloatingWindowLevel] | Different mechanism |
| GetMainWindowHandle() | Get NSWindow pointer | Must return as IntPtr |

#### What Doesn't Map:
| Windows | macOS | Workaround |
|---------|-------|-----------|
| GetWindowLong() | N/A | Return 0 or stored properties |
| SetWindowLong() | N/A | Stub or custom properties dict |
| GetWindow(GW_HWNDPREV) | N/A | Use CGWindowList (slow) or stub |
| EnumWindows() | CGWindowListCopyWindowInfo | Slower, requires private APIs |
| IsAboveInZOrder() | Compare levels | Approximate via levels |

#### Stub Strategy for Hard Parts:
Some methods will need to be effectively no-ops or stubs:
- `GetWindowLong()` → Return 0
- `SetWindowLong()` → Return false (or log warning)
- `GetWindow()` → Return IntPtr.Zero (sibling enumeration not supported)
- `EnumerateWindows()` → Use private APIs or stub

**Verdict**: Hardest service. Do last. Many methods will be partial implementations or stubs.

---

### 5. **IScreenCaptureService** - HARD ⭐
**Current Status**: Windows fully implemented (GDI screen capture)  
**Difficulty**: Hard  
**Time Estimate**: 8-12 hours

#### Why It's Hard:
- Requires low-level graphics API knowledge
- Windows uses GDI (GetDC, CreateCompatibleDC, StretchBlt, DIB)
- macOS uses different paradigms: IOSurface, CG, Metal
- Performance-critical code - must be optimized
- Pixel format conversion needed

#### Implementation Path:
```
MacOSScreenCaptureService.cs
├── InitializeCapture() → Setup CG/IOSurface buffers
├── CaptureScreen() → Copy pixels from screen to buffer
└── ReleaseCapture() → Clean up resources
```

#### macOS Capture Options:

**Option 1: Core Graphics (Simple, Slow)**
```objective-c
CGImageRef screenshot = CGDisplayCreateImage(displayId);
CGDataProviderRef provider = CGImageGetDataProvider(screenshot);
CFDataRef data = CGDataProviderCopyData(provider);
```
- Pros: Simple API, straightforward
- Cons: Slow (CPU copy), allocates new memory each frame
- Performance: ~15-30ms per frame on 1920x1080

**Option 2: IOSurface (Medium, Moderate Speed)**
- Pros: Supports hardware accelerated transfers
- Cons: Complex API, requires Metal/OpenGL integration
- Performance: ~5-10ms per frame

**Option 3: Metal Compute (Hard, Fast)**
- Pros: GPU-accelerated, very fast
- Cons: Complex to set up, overkill if not rendering
- Performance: <1ms for data transfer (GPU-side work)

#### Challenges:
1. **Pixel Format**: Windows GDI uses BGRA, need to match or convert
2. **Memory Management**: IOSurface vs malloc vs Core Graphics
3. **Multiple Monitors**: Must handle all displays (similar to Windows)
4. **Performance**: Screen capture is CPU-intensive on macOS
5. **Permissions**: macOS 12.5+ requires Screen Recording permission

#### Implementation Recommendation:
Start with Core Graphics (Option 1) for simplicity, optimize later if needed:
```csharp
public bool CaptureScreen(int virtX, int virtY, int virtW, int virtH, out IntPtr dibBits)
{
    // 1. Get display ID from coordinates
    // 2. Create CGImage from display
    // 3. Extract pixel data
    // 4. Copy to allocated buffer (match Windows buffer layout)
    // 5. Return buffer pointer
}
```

#### Key Differences from Windows:
| Aspect | Windows | macOS |
|--------|---------|-------|
| API | GDI (GetDC, StretchBlt) | Core Graphics (CGDisplay) |
| Memory | DIB Section (device memory) | IOSurface or malloc |
| Performance | Hardware accelerated | CPU-based (variable) |
| Multiple displays | Enumerate via virtual screen | CGDisplayCopyAllDisplayModes |
| Permissions | Built-in | macOS 12.5+ requires opt-in |

**Verdict**: Hard but doable. Medium priority - needed for DesktopAmbientProbe.

---

## Implementation Order (Recommended)

### Phase 1: Quick Wins (Week 1)
1. **ISystemTrayService** (2-4h)
   - Isolated, highest ROI
   - Users see tray icon immediately
   - Good confidence builder

2. **IScreenService** (3-5h)
   - Straightforward APIs
   - Needed for monitor detection
   - Sets up foundation for window service

### Phase 2: Core Functionality (Week 2-3)
3. **ITransparencyService** (4-6h)
   - Depends on window basics
   - Needed for avatar rendering
   - Medium complexity, good learning

4. **IWindowService - Partial** (6-8h of 10-15h)
   - Start with simple methods (GetMainWindowHandle, ShowWindow, etc.)
   - Leave stubbed: GetWindowLong, SetWindowLong, EnumWindows
   - Focus on position/size operations
   - Can iterate and improve later

### Phase 3: Optional Enhancements (Week 4+)
5. **IScreenCaptureService** (8-12h)
   - Needed for ambient lighting
   - Can defer if not critical
   - Higher complexity, lower impact

---

## Service Dependencies

```
┌─────────────────────────────────────┐
│   ISystemTrayService (Easy)         │ ✓ No dependencies
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   IScreenService (Easy)             │ ✓ No dependencies
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   ITransparencyService (Medium)     │
│   Depends on: IWindowService        │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   IWindowService (Hard)             │ ✓ No dependencies
│   Used by: ITransparencyService,    │
│             Avatar handlers         │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   IScreenCaptureService (Hard)      │ ✓ No dependencies
│   Used by: DesktopAmbientProbe      │
└─────────────────────────────────────┘
```

---

## Implementation Strategy: Stub & Iterate

Rather than trying to implement everything perfectly, use this strategy:

### Tier 1: Full Implementation
Methods that have straightforward macOS equivalents:
- `GetMainWindowHandle()`, `GetActiveWindow()`
- `GetWindowRect()`, `SetWindowPosition()`
- `ShowWindow()`, `IsWindowVisible()`
- `IsWindowMinimized()`, `IsWindowMaximized()`

### Tier 2: Partial Implementation
Methods that work for common cases but have limitations:
- `SetTopMost()` - Set level, but macOS levels are different
- `GetWindowProcessId()` - Works but slower on macOS
- `SetWindowPosition()` - Works but no animation support

### Tier 3: Stubs (Log & Return)
Methods with no macOS equivalent:
- `GetWindowLong()` → Return 0, log warning
- `SetWindowLong()` → Return false, log warning
- `GetWindow()` → Return IntPtr.Zero, log warning
- `EnumerateWindows()` → Return false, log warning

This allows the app to run on macOS while acknowledging limitations.

---

## Technical Debt & Notes

### Things to Watch Out For:

1. **Coordinate Systems**:
   - Windows: Origin at top-left, Y increases downward
   - macOS: Origin at bottom-left, Y increases upward
   - Must convert consistently (especially for mouse/window positions)

2. **IntPtr for Window Handles**:
   - Windows: HWND is numeric (IntPtr from int)
   - macOS: NSWindow is a pointer (IntPtr is just the object pointer)
   - Add abstraction layer to make this transparent

3. **Event Handling**:
   - Windows: Messages in message queue
   - macOS: Event responder chain
   - Different models, but events wrapper can hide this

4. **Memory Management**:
   - Windows: Handles are managed by OS
   - macOS: Objects can be deallocated, must track lifetime
   - Keep strong references to window objects

5. **Permissions**:
   - macOS requires user approval for:
     - Screen recording (Accessibility)
     - Screen capture (Screen Recording)
   - Must handle gracefully when denied

### Future Optimization Points:
- IOSurface for fast screen capture
- Metal compute for GPU-accelerated capture
- AVFoundation for video recording
- Accessibility APIs for window enumeration
- Private APIs for advanced window control (use carefully)

---

## Effort Summary

| Service | Difficulty | Time | Priority | Status |
|---------|----------|------|----------|--------|
| ISystemTrayService | Easy | 2-4h | 1st | Not started |
| IScreenService | Easy | 3-5h | 2nd | Not started |
| IWindowService | Hard | 10-15h | 3rd | Not started |
| ITransparencyService | Medium | 4-6h | 4th | Not started |
| IScreenCaptureService | Hard | 8-12h | 5th | Not started |
| **TOTAL** | **Mixed** | **27-42h** | - | - |

**Realistic Timeline (full team)**:
- Week 1: Systems 1-2 (quick wins)
- Week 2: System 3 + partial System 4
- Week 3-4: Complete System 4, add System 5
- **Total: 3-4 weeks for full cross-platform support**

**Minimum Viable macOS (Weeks 1-2)**:
- Systems 1-2 + partial System 4 = Working avatar with basic windowing
- Would satisfy most use cases for macOS port
