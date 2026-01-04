# macOS Always-On-Top Over Fullscreen Apps

## Overview

This implementation allows the Unity application to appear on top of fullscreen applications on macOS, which is normally not possible with standard window management APIs.

## Problem

On macOS, fullscreen applications run in a separate "Space" (virtual desktop), and regular windows cannot appear over them by default. This is different from Windows where setting a window to "topmost" makes it appear over everything.

## Solution

Based on solutions from the Electron community and GeminiDesk:
- **Reference 1**: [Electron Issue #10078](https://github.com/electron/electron/issues/10078)
- **Reference 2**: [GeminiDesk PR #58](https://github.com/hillelkingqt/GeminiDesk/pull/58)

The solution uses macOS-specific window properties:
1. **Window Level**: Set to `NSFloatingWindowLevel` (3) or `NSScreenSaverWindowLevel` (1000)
2. **Collection Behavior**: Add `NSWindowCollectionBehaviorFullScreenAuxiliary` flag
3. **All Spaces**: Add `NSWindowCollectionBehaviorCanJoinAllSpaces` flag

## Implementation

### Native Plugin (`MacOSWindowHelper.mm`)

A native Objective-C++ plugin that interfaces with macOS AppKit to set window properties:

```objc
// Key functions:
- MacOS_EnableAlwaysOnTopOverFullscreen(bool enable)
  Uses NSFloatingWindowLevel - less intrusive

- MacOS_EnableAlwaysOnTopScreenSaverLevel(bool enable)
  Uses NSScreenSaverWindowLevel - more aggressive

- MacOS_SetWindowLevel(int level)
  Set custom window level for fine control
```

### C# Wrapper (`MacOSWindowHelper.cs`)

Provides a Unity-friendly API:

```csharp
// Enable always-on-top over fullscreen apps (recommended)
MacOSWindowHelper.EnableAlwaysOnTopOverFullscreen(true);

// More aggressive mode (if needed)
MacOSWindowHelper.EnableAlwaysOnTopScreenSaverLevel(true);

// Custom window level
MacOSWindowHelper.SetWindowLevel(MacOSWindowHelper.NSFloatingWindowLevel);

// Show on all desktops/spaces
MacOSWindowHelper.SetVisibleOnAllSpaces(true);

// Check if window can appear over fullscreen
bool canAppear = MacOSWindowHelper.CanAppearOverFullscreen();
```

### Integration

The handlers `AvatarWindowHandler` and `AvatarHideHandler` have been updated to use the macOS helper:

```csharp
void SetTopMost(bool en)
{
#if UNITY_STANDALONE_WIN
    SetWindowPos(unityHWND, en ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
#elif UNITY_STANDALONE_OSX
    MacOSWindowHelper.EnableAlwaysOnTopOverFullscreen(en);
#endif
}
```

## Window Levels

macOS window levels (from low to high):
- `NSNormalWindowLevel` = 0 (normal windows)
- `NSFloatingWindowLevel` = 3 (floating palettes)
- `NSModalPanelWindowLevel` = 8 (modal dialogs)
- `NSMainMenuWindowLevel` = 24 (menu bar)
- `NSStatusWindowLevel` = 25 (status items)
- `NSPopUpMenuWindowLevel` = 101 (pop-up menus)
- `NSScreenSaverWindowLevel` = 1000 (screen saver)

## Building the Plugin

The native plugin is built using the provided build script:

```bash
cd "Assets/MATE ENGINE - Scripts/Plugins/MacOS"
./build_plugin.sh
```

This creates a universal binary (.bundle) that works on both Intel (x86_64) and Apple Silicon (arm64) Macs.

## Requirements

- macOS 10.13 or later
- Xcode Command Line Tools (for building the plugin)
- Unity 2019.4 or later (for the C# wrapper)

## Limitations

1. **Fullscreen Games**: This solution works for most fullscreen apps (browsers, video players, etc.) but may not work with fullscreen games that use exclusive fullscreen mode (bypasses the window server).

2. **User Experience**: Having a window that appears over fullscreen content can be intrusive. Use this feature judiciously.

3. **System Permissions**: Some macOS security features may affect this behavior depending on system settings.

## Testing

To test if the implementation works:

1. Build and run the Unity application on macOS
2. Make another application fullscreen (e.g., Safari in fullscreen mode)
3. Your Unity app should remain visible on top of the fullscreen application

## Troubleshooting

**Plugin not loading:**
- Check that `MacOSWindowHelper.bundle` exists in the Plugins/MacOS folder
- Verify the bundle's Info.plist is correctly formatted
- Check Unity's Console for any DllNotFoundException errors

**Window not appearing over fullscreen:**
- Verify the window level is set correctly: `MacOSWindowHelper.GetWindowLevel()`
- Check collection behavior: `MacOSWindowHelper.CanAppearOverFullscreen()`
- Try the more aggressive screen-saver level mode

**Build errors:**
- Ensure Xcode Command Line Tools are installed: `xcode-select --install`
- Check that the SDK path is correct: `xcrun --show-sdk-path`

## Credits

- Solution approach from Electron community: [@pchw](https://github.com/pchw), [@mansona](https://github.com/mansona)
- GeminiDesk implementation: [@hillelkingqt](https://github.com/hillelkingqt), [@astron8t-voyagerx](https://github.com/astron8t-voyagerx)
