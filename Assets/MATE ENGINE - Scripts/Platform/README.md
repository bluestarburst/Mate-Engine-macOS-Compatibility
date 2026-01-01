# Platform Abstraction Layer

This directory contains the platform abstraction layer for Mate Engine, providing cross-platform support for Windows, macOS, and Linux.

## Architecture

The platform layer uses a service-oriented architecture with the following components:

### Core Interfaces (`Core/`)

- **`IPlatformService.cs`** - Main entry point for platform-specific services
- **`IWindowService.cs`** - Window management operations (positioning, transparency, z-order, etc.)
- **`IScreenService.cs`** - Monitor/display operations (screen enumeration, cursor position, etc.)
- **`ISystemTrayService.cs`** - System tray/status bar operations
- **`PlatformCommon.cs`** - Common data structures (ScreenInfo, PlatformRect, etc.)
- **`PlatformServiceLocator.cs`** - Service locator for accessing platform services

### Platform Implementations

#### Windows (`Windows/`)
- **`WindowsPlatformService.cs`** - Windows service implementation
- **`WindowsWindowService.cs`** - Windows window management via Win32 API
- **`WindowsScreenService.cs`** - Windows monitor/screen operations
- **`WindowsSystemTrayService.cs`** - Windows system tray wrapper

#### macOS (`MacOS/`)
- **`MacOSPlatformService.cs`** - macOS service implementation
- **`MacOSWindowService.cs`** - macOS window management via native plugin
- **`MacOSScreenService.cs`** - macOS screen operations
- **`MacOSSystemTrayService.cs`** - macOS status bar (menu bar icon)

#### Linux (`Linux/`)
- **`LinuxPlatformService.cs`** - Linux service implementation (placeholder)
- Currently contains stub implementations for future development

### Native Plugins (`../Plugins/macOS/`)

- **`NativeWindowManager.mm`** - Objective-C plugin for macOS-specific window operations
  - Window level management (for overlay above fullscreen apps)
  - Collection behavior (fullscreen auxiliary, all spaces)
  - Dock visibility control
  - Status bar item creation
  - Window transparency and click-through

## Usage

### Basic Usage

```csharp
using MateEngine.Platform;

// Get the platform services
var windowService = PlatformServiceLocator.GetWindowService();
var screenService = PlatformServiceLocator.GetScreenService();

// Set window always on top
windowService.SetAlwaysOnTop(true);

// Get cursor position
Vector2 cursorPos = screenService.GetCursorPosition();

// Get all screens
ScreenInfo[] screens = screenService.GetAllScreens();
```

### Advanced Usage

```csharp
// For macOS-specific features
#if UNITY_STANDALONE_OSX
    // Set window to appear above fullscreen apps
    windowService.SetWindowLevel(WindowLevel.StatusBar);
    windowService.SetVisibleOnAllWorkspaces(true, visibleOnFullScreen: true);
    windowService.HideFromDock();
#endif
```

## Platform-Specific Features

### Windows
- Full window management via Win32 API
- System tray with notifications
- Multiple monitor support
- Taskbar visibility control

### macOS
- Window levels for precise z-ordering
- Fullscreen overlay support (appears above fullscreen apps)
- Multiple workspace/Spaces support
- Status bar (menu bar) icon
- Dock visibility control
- Native transparency and effects

### Linux
- Placeholder implementation
- Future support planned for X11 and Wayland

## macOS Fullscreen Overlay

One of the key features for macOS is the ability to appear above fullscreen applications. This is achieved through:

1. **NSWindowLevel** - Set to `NSStatusWindowLevel` (25) or higher
2. **NSWindowCollectionBehavior** - Use `NSWindowCollectionBehaviorFullScreenAuxiliary`
3. **Dock Hiding** - Use `NSApplicationActivationPolicyAccessory` to hide from dock

Example:
```csharp
var windowService = PlatformServiceLocator.GetWindowService();
windowService.SetWindowLevel(WindowLevel.StatusBar);
windowService.SetVisibleOnAllWorkspaces(true, visibleOnFullScreen: true);
windowService.HideFromDock();
```

## Building the macOS Plugin

The native macOS plugin needs to be compiled as a `.bundle` for Unity:

```bash
cd Assets/Plugins/macOS
clang -dynamiclib -o NativeWindowManager.bundle NativeWindowManager.mm \
    -framework Cocoa -framework AppKit
```

For Unity to recognize it, create a `.meta` file with:
- Platform: macOS
- CPU: x86_64 and ARM64

## Migration Guide

When migrating existing code to use the platform layer:

### Before:
```csharp
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(...);
    
    SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, flags);
#endif
```

### After:
```csharp
var windowService = PlatformServiceLocator.GetWindowService();
windowService.SetAlwaysOnTop(true);
```

## Testing

To test the platform layer:

1. **Windows**: Run the application on Windows and verify window operations work
2. **macOS**: Run the application on macOS and verify:
   - Window appears above other windows
   - Window appears above fullscreen apps
   - Dock icon can be hidden
   - Window transparency works
3. **Linux**: Verify the application runs without errors (stub implementation)

## Future Improvements

- [ ] Complete Linux implementation with X11/Wayland support
- [ ] Add window snapping APIs
- [ ] Add drag-and-drop support
- [ ] Enhanced system tray menu management
- [ ] Multi-monitor DPI awareness
- [ ] Window shadow control for macOS
- [ ] Window vibrancy/blur effects

## References

- **Linux Port Reference**: [Marksonthegamer/Mate-Engine-Linux-Port](https://github.com/Marksonthegamer/Mate-Engine-Linux-Port)
- **macOS Fullscreen Overlay**: [ahkohd/tauri-nspanel](https://github.com/ahkohd/tauri-nspanel)
- **Electron macOS Solution**: [electron/electron#10078](https://github.com/electron/electron/issues/10078)
- **GeminiDesk Implementation**: [hillelkingqt/GeminiDesk PR #58](https://github.com/hillelkingqt/GeminiDesk/pull/58)

## License

CC0 - https://creativecommons.org/publicdomain/zero/1.0/
