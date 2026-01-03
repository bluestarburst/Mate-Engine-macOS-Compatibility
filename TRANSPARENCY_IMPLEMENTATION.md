# macOS Transparency Implementation Guide

## Summary

This document explains how window transparency works on macOS and how to properly enable it in your Unity build.

## Architecture

The transparency system uses different approaches on Windows vs macOS:

### Windows
- Uses Windows API calls (DWM, layered windows, etc.)
- C# code makes P/Invoke calls to Windows DLLs
- Multiple transparency methods available (Alpha, ColorKey)

### macOS
- Uses native Swift framework (`LibUniWinC.bundle`)
- C# code makes P/Invoke calls to the native bundle
- Swift code handles NSWindow configuration at the OS level
- Only one transparency method (Alpha)

## How It Works

1. **Native Layer** (`LibUniWinC.bundle`):
   - Located at: `Assets/MATE ENGINE - Packages/Kirurobo/UniWindowController/Runtime/Plugins/MacOS/LibUniWinC.bundle`
   - Written in Swift
   - Handles actual window transparency by:
     - Setting window to non-opaque
     - Removing title bar and window decorations
     - Clearing window background color
     - Managing z-order (topmost/bottommost)

2. **C# Wrapper** (`UniWinCore.cs`):
   - Calls native methods via `[DllImport("LibUniWinC")]`
   - Methods used for transparency:
     - `SetTransparent(bool)` - Enable/disable transparency
     - `SetBorderless(bool)` - Remove/restore window border
     - `SetAlphaValue(float)` - Set overall window alpha (0.0-1.0)
     - `SetTopmost(bool)` - Keep window always on top
     - `SetBottommost(bool)` - Keep window always on bottom

3. **High-Level Controller** (`UniWindowController.cs`):
   - Public API for transparency control
   - Automatically manages camera background when transparency changes
   - Property: `isTransparent` (set to `true` to enable)

## Camera Configuration

When transparency is enabled, the camera must render with a transparent background:

```csharp
camera.clearFlags = CameraClearFlags.SolidColor;
camera.backgroundColor = Color.clear;  // (0, 0, 0, 0)
```

**Important**: Do NOT use `CameraClearFlags.Depth` as this causes ghosting/layering artifacts. The color buffer must be cleared to transparent black on each frame.

## How to Enable Transparency

### Method 1: Unity Inspector (Recommended)
1. Find the GameObject with `UniWindowController` component in your scene
2. Check the `Is Transparent` checkbox in the Inspector
3. Build and run your macOS application

### Method 2: Programmatically
```csharp
// Get reference to UniWindowController
var windowController = UniWindowController.current;

// Enable transparency
windowController.isTransparent = true;

// Optional: Also enable topmost
windowController.isTopmost = true;
```

### Method 3: Startup Script
Create a script that runs on startup:

```csharp
using UnityEngine;
using Kirurobo;

public class TransparencyEnabler : MonoBehaviour
{
    void Start()
    {
        #if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        var windowController = UniWindowController.current;
        if (windowController != null)
        {
            windowController.isTransparent = true;
            windowController.isTopmost = true;
        }
        #endif
    }
}
```

## Limitations

1. **Editor**: Transparency does NOT work in the Unity Editor. You must create a macOS build to see it working.

2. **Build Only**: The native Swift bundle is only loaded in standalone builds, not in the editor.

3. **Platform-Specific**: Different native implementations for Windows vs macOS means behavior may differ slightly.

4. **HDR**: HDR rendering should be disabled on the camera for proper transparency.

5. **URP/HDRP**: If using Universal Render Pipeline, ensure "Alpha Processing" is enabled in the renderer settings.

## Troubleshooting

### Black Background Instead of Transparent
**Cause**: Transparency is not enabled  
**Solution**: Set `isTransparent = true` on the UniWindowController component

### Ghosting/Layering Effect
**Cause**: Using `CameraClearFlags.Depth` instead of `SolidColor`  
**Solution**: Ensure camera uses `SolidColor` with `Color.clear` (this is automatic when using UniWindowController)

### DllNotFoundException: LibUniWinC
**Cause**: Native bundle not found or not properly included in build  
**Solution**: 
- Verify `LibUniWinC.bundle` exists at `Assets/MATE ENGINE - Packages/Kirurobo/UniWindowController/Runtime/Plugins/MacOS/`
- Check Unity's Plugin Inspector settings for the bundle (should be enabled for macOS Standalone)
- Make sure the bundle is included in your build

### Window Has Title Bar
**Cause**: Borderless mode not enabled  
**Solution**: Both `SetTransparent()` and `SetBorderless()` are called together by `EnableTransparent()`, so this should be automatic

## Code Changes Made

### UniWinCore.cs (macOS Stub Section)
**Before** (broken implementation):
```csharp
#if UNITY_STANDALONE_OSX
    // Empty stubs that do nothing
    public static void SetTransparent(bool bEnabled) { }
    public static void SetBorderless(bool bEnabled) { }
#endif
```

**After** (correct implementation):
```csharp
#if !UNITY_STANDALONE_WIN
    // Call native LibUniWinC.bundle for macOS
    [DllImport("LibUniWinC")]
    public static extern void SetTransparent([MarshalAs(UnmanagedType.U1)] bool bEnabled);

    [DllImport("LibUniWinC")]
    public static extern void SetBorderless([MarshalAs(UnmanagedType.U1)] bool bEnabled);

    [DllImport("LibUniWinC")]
    public static extern void SetAlphaValue(float alpha);

    [DllImport("LibUniWinC")]
    public static extern void SetTopmost([MarshalAs(UnmanagedType.U1)] bool bEnabled);

    [DllImport("LibUniWinC")]
    public static extern void SetBottommost([MarshalAs(UnmanagedType.U1)] bool bEnabled);

    // Remaining methods are stubs (not implemented in macOS bundle)
    public static bool IsActive() => true;
    public static bool IsTransparent() => false;
    // ... etc
#endif
```

### UniWindowController.cs
- No changes needed - already correct
- Uses `CameraClearFlags.SolidColor` with `Color.clear` for transparency
- `SetCameraBackground()` method automatically manages camera settings

## Testing

1. **Build Settings**:
   - Platform: macOS
   - Architecture: Apple Silicon or Intel (depending on your Mac)
   - Build the application

2. **Run the Build**:
   - Launch the .app from Finder
   - Background should be transparent
   - Window should have no title bar
   - Avatar should be visible against desktop background

3. **Verify Transparency**:
   - Move the window around - you should see through it to the desktop
   - Check that there's no black background
   - Verify no ghosting or layering artifacts

## Additional Resources

- [Official UniWindowController Repository](https://github.com/kirurobo/UniWindowController)
- [UniWindowController Documentation](https://github.com/kirurobo/UniWindowController/blob/main/README.md)
- [Unity macOS Player Settings](https://docs.unity3d.com/Manual/class-PlayerSettingsMacOS.html)

## Summary of Fix

The issue was that the macOS stub implementations were empty no-ops. They needed to call the native LibUniWinC.bundle via P/Invoke (DllImport) just like Windows does. Once this was fixed, the Swift framework could properly handle window transparency at the NSWindow level.
