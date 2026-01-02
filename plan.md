Implement platform abstraction layer for macOS and cross-platform compatibility #1
Completed
7 hours ago
·
bluestarburst/Mate-Engine-macOS-Compatibility
main
copilot/implement-macos-compatibility
Summary
Anthropic logo
Claude Sonnet 4.5
· 2 sessions
· 2 premium requests
· Last updated 7 hours ago
macOS and Cross-Platform Compatibility Implementation - COMPLETE
Phase 1-5: Platform Abstraction Layer ✅
Core interfaces, Windows/macOS/Linux implementations
Native Objective-C plugin for macOS
Service locator pattern
Phase 6: Code Refactoring ✅

RemoveTaskbarApp.cs, AvatarHideHandler.cs

AvatarSwayController.cs, AvatarBigScreenTimer.cs

AvatarWindowHandler.cs - Added platform guards for Windows-specific code
All using platform abstraction layer where applicable
Phase 7: Documentation & Unity Integration ✅
Platform README.md
MACOS_COMPATIBILITY.md
All Unity .meta files created
Latest Fix: AvatarWindowHandler.cs
Added proper #if UNITY_STANDALONE_WIN guards around Windows-specific methods and API calls to prevent compilation errors on macOS/Linux:

Methods Guarded:

IsEffectivelyTransparentWindow - Uses GetWindowLongPtr, GetLayeredWindowAttributes
IsSameProcessWindow - Uses GetWindowThreadProcessId
UpdateCachedWindows - Uses EnumWindows, IsWindowVisible, GetWindowRect, GetClassName
RebuildActiveOccluders - Uses GetClassName, IsAboveInZOrder
IsSitEligibleWindow - Uses GetParent, GetAncestor, IsIconic, GetWindowTextLength, IsCloaked
IsLikelyUniWindowMascot - Uses GetWindowLongPtr, GetLayeredWindowAttributes, GetWindowTextLength
IsAboveInZOrder - Uses GetWindow
GetUnityWindowPosition, GetUnityClientRect, SetTopMost, IsWindowMaximized, IsWindowFullscreen - Various Windows APIs
Structure:

Line 119-122: Guard GetCurrentProcessId in Start()
Line 135-137: Guard SetTopMost in Start()
Line 173-175: Early return from Update() on non-Windows
Line 333-954: Main Windows-specific methods section
Line 366-368: Guard SetTopMost in ClearSnapAndHide()
Line 413-417: Guard IsCloaked method
Line 956-1002: Windows API DllImport declarations
The file now compiles on all platforms without errors.

Original prompt
macOS and Cross-Platform Compatibility Implementation
Overview
Make Mate-Engine compatible with macOS while maintaining Windows functionality and providing extensibility for Linux. This is a significant architectural change that introduces a platform abstraction layer.

Background & References
Linux Port Reference: Marksonthegamer/Mate-Engine-Linux-Port - Shows patterns for platform detection using RuntimePlatform
macOS Fullscreen Overlay: ahkohd/tauri-nspanel fullscreen example - Native macOS techniques for windows above fullscreen apps
Electron macOS Solution: electron/electron#10078 - Discussion on alwaysOnTop over fullscreen apps
GeminiDesk Implementation: hillelkingqt/GeminiDesk PR #58 - Merged implementation using setVisibleOnAllWorkspaces and dock hiding
Current Windows-Specific Code Analysis
The following files contain Windows-specific P/Invoke calls that need cross-platform alternatives:

Critical Priority (Core Functionality)
Assets/MATE ENGINE - Scripts/APIs/WinApi.cs - Core Windows API wrapper with 50+ DllImports
Assets/MATE ENGINE - Scripts/APIs/DwmApi.cs - Desktop Window Manager effects (transparency, blur)
Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarHideHandler.cs - Window positioning, topmost, monitor detection
Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarWindowHandler.cs - Window snapping, z-order management
Assets/MATE ENGINE - System Tray/SystemTray/WinAPI.cs - System tray functionality
Assets/MATE ENGINE - System Tray/RemoveTaskbarApp.cs - Taskbar visibility control
High Priority (User Experience)
Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarTaskbarController.cs - Taskbar interaction
Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarSwayController.cs - Window velocity tracking
Assets/MATE ENGINE - Scripts/AvatarHandlers/PetVoiceReactionHandler.cs - Cursor/window detection
Assets/MATE ENGINE - Scripts/Tools/DesktopAmbientProbe.cs - Screen capture for ambient lighting
Assets/MATE ENGINE - Scripts/Settings/SettingsMenuPosition.cs - Monitor enumeration
Medium Priority (Features)
Assets/uWindowCapture/ - Window capture plugin (Windows-only DLL)
Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarBigScreenTimer.cs - Cursor position detection
Implementation Plan
Phase 1: Platform Abstraction Layer Foundation
Create the core abstraction architecture:

Assets/MATE ENGINE - Scripts/Platform/
├── Core/
│   ├── IPlatformService.cs              # Main interface for all platform operations
│   ├── IWindowService.cs                # Window management interface
│   ├── ISystemTrayService.cs            # System tray interface
│   ├── IScreenService.cs                # Screen/monitor interface
│   ├── ICursorService.cs                # Cursor/input interface
│   └── PlatformServiceLocator.cs        # Service locator/factory
├── Windows/
│   ├── WindowsPlatformService.cs        # Existing Windows implementation
│   ├── WindowsWindowService.cs
│   ├── WindowsSystemTrayService.cs
│   └── WindowsScreenService.cs
├── MacOS/
│   ├── MacOSPlatformService.cs
│   ├── MacOSWindowService.cs
│   ├── MacOSSystemTrayService.cs        # Status bar item implementation
│   └── MacOSScreenService.cs
├── Linux/
│   ├── LinuxPlatformService.cs
│   ├── LinuxWindowService.cs            # X11/Wayland abstraction
│   └── LinuxScreenService.cs
└── Plugins/
    └── macOS/
        ├── NativeWindowManager.mm       # Objective-C native plugin
        └── NativeWindowManager.bundle   # Compiled plugin
Phase 2: Interface Definitions
// IWindowService.cs - Core window operations
public interface IWindowService
{
    IntPtr GetMainWindowHandle();
    void SetWindowPosition(int x, int y, int width, int height);
    void SetAlwaysOnTop(bool enabled);
    void SetWindowLevel(WindowLevel level);
    void SetClickThrough(bool enabled);
    void SetTransparency(float alpha);
    Rect GetWindowRect();
    void SetVisibleOnAllWorkspaces(bool visible, bool visibleOnFullScreen = false);
    void HideFromDock();
    void ShowInDock();
}

public enum WindowLevel
{
    Normal,
    Floating,
    ModalPanel,
    MainMenu,
    StatusBar,
    PopUpMenu,
    ScreenSaver
}

// ISystemTrayService.cs - System tray/status bar
public interface ISystemTrayService
{
    void Initialize(Texture2D icon, string tooltip);
    void SetIcon(Texture2D icon);
    void SetTooltip(string tooltip);
    void ShowBalloon(string title, string message);
    void AddMenuItem(string label, Action callback);
    void AddSeparator();
    void Dispose();
    event Action OnLe...

</details>
