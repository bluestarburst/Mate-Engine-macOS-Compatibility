using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// macOS-specific window management helper
/// Provides functionality to make Unity windows appear above fullscreen applications
/// Based on solution from: https://github.com/electron/electron/issues/10078
/// </summary>
public static class MacOSWindowHelper
{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    
    // Window level constants from macOS (from AppKit/NSWindow.h)
    // These correspond to system-defined window levels
    public const int NSNormalWindowLevel = 0;           // Normal application windows
    public const int NSFloatingWindowLevel = 3;         // Floating palette windows
    public const int NSModalPanelWindowLevel = 8;       // Modal dialog windows
    public const int NSMainMenuWindowLevel = 24;        // Main menu bar (not usually used)
    public const int NSStatusWindowLevel = 25;          // Status bar and menus (RECOMMENDED for always-on-top)
    public const int NSPopUpMenuWindowLevel = 101;      // Pop-up menus
    public const int NSScreenSaverWindowLevel = 1000;   // Highest normal window level (more aggressive)
    
    // Collection behavior flags for comprehensive control
    public const int NSWindowCollectionBehaviorDefault = 0;
    public const int NSWindowCollectionBehaviorCanJoinAllSpaces = 1;
    public const int NSWindowCollectionBehaviorStationary = 16;
    public const int NSWindowCollectionBehaviorFullScreenAuxiliary = 256;
    public const int NSWindowCollectionBehaviorIgnoresCycle = 64;
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_EnableAlwaysOnTopOverFullscreen")]
    private static extern void _EnableAlwaysOnTopOverFullscreen(bool enable);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_EnableAlwaysOnTopScreenSaverLevel")]
    private static extern void _EnableAlwaysOnTopScreenSaverLevel(bool enable);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_SetWindowLevel")]
    private static extern void _SetWindowLevel(int level);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetWindowLevel")]
    private static extern int _GetWindowLevel();
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_SetVisibleOnAllSpaces")]
    private static extern void _SetVisibleOnAllSpaces(bool enable);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_CanAppearOverFullscreen")]
    private static extern bool _CanAppearOverFullscreen();
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_BringToFront")]
    private static extern void _BringToFront();
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_HideDockIcon")]
    private static extern void _HideDockIcon(bool hide);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_StartMonitoringSpaceChanges")]
    private static extern void _StartMonitoringSpaceChanges();
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_StopMonitoringSpaceChanges")]
    private static extern void _StopMonitoringSpaceChanges();
    
    /// <summary>
    /// Enable or disable always-on-top behavior that works over fullscreen applications.
    /// This uses NSScreenSaverWindowLevel (1000) for maximum reliability like GeminiDesk.
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public static void EnableAlwaysOnTopOverFullscreen(bool enable)
    {
        try
        {
            _EnableAlwaysOnTopOverFullscreen(enable);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to enable always-on-top over fullscreen: {e.Message}");
        }
    }
    
    /// <summary>
    /// Enable or disable always-on-top with screen-saver level priority.
    /// This is more aggressive and will appear above almost everything.
    /// Use this only if floating level is not sufficient.
    /// </summary>
    /// <param name="enable">True to enable, false to disable</param>
    public static void EnableAlwaysOnTopScreenSaverLevel(bool enable)
    {
        try
        {
            _EnableAlwaysOnTopScreenSaverLevel(enable);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to enable screen-saver level always-on-top: {e.Message}");
        }
    }
    
    /// <summary>
    /// Set a custom window level. See the NSWindowLevel constants for common values.
    /// </summary>
    /// <param name="level">Window level (0 = normal, 3 = floating, 1000 = screen-saver)</param>
    public static void SetWindowLevel(int level)
    {
        try
        {
            _SetWindowLevel(level);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to set window level: {e.Message}");
        }
    }
    
    /// <summary>
    /// Get the current window level
    /// </summary>
    /// <returns>Current window level</returns>
    public static int GetWindowLevel()
    {
        try
        {
            return _GetWindowLevel();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to get window level: {e.Message}");
            return 0;
        }
    }
    
    /// <summary>
    /// Make the window visible on all spaces/desktops
    /// </summary>
    /// <param name="enable">True to show on all spaces, false to show only on current space</param>
    public static void SetVisibleOnAllSpaces(bool enable)
    {
        try
        {
            _SetVisibleOnAllSpaces(enable);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to set visible on all spaces: {e.Message}");
        }
    }
    
    /// <summary>
    /// Check if the window can currently appear over fullscreen applications
    /// </summary>
    /// <returns>True if window can appear over fullscreen apps</returns>
    public static bool CanAppearOverFullscreen()
    {
        try
        {
            return _CanAppearOverFullscreen();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to check fullscreen capability: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Force the window to the front. Useful when switching spaces or when the window
    /// needs to be brought forward after system events.
    /// </summary>
    public static void BringToFront()
    {
        try
        {
            _BringToFront();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to bring window to front: {e.Message}");
        }
    }
    
    /// <summary>
    /// Hide or show the dock icon. Like GeminiDesk, hiding the dock icon can help
    /// the window behave more like a menu bar app when always-on-top is enabled.
    /// </summary>
    /// <param name="hide">True to hide the dock icon, false to show it</param>
    public static void HideDockIcon(bool hide)
    {
        try
        {
            _HideDockIcon(hide);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to hide/show dock icon: {e.Message}");
        }
    }
    
    /// <summary>
    /// Start monitoring for macOS space/desktop changes. When a space change is detected,
    /// the window will automatically be brought to front if it's in always-on-top mode.
    /// Call this once when enabling always-on-top functionality.
    /// </summary>
    public static void StartMonitoringSpaceChanges()
    {
        try
        {
            _StartMonitoringSpaceChanges();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to start monitoring space changes: {e.Message}");
        }
    }
    
    /// <summary>
    /// Stop monitoring for space changes.
    /// </summary>
    public static void StopMonitoringSpaceChanges()
    {
        try
        {
            _StopMonitoringSpaceChanges();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to stop monitoring space changes: {e.Message}");
        }
    }
    
#else
    // Stub implementations for non-macOS platforms
    public const int NSNormalWindowLevel = 0;
    public const int NSFloatingWindowLevel = 3;
    public const int NSModalPanelWindowLevel = 8;
    public const int NSMainMenuWindowLevel = 24;
    public const int NSStatusWindowLevel = 25;
    public const int NSPopUpMenuWindowLevel = 101;
    public const int NSScreenSaverWindowLevel = 1000;
    
    public static void EnableAlwaysOnTopOverFullscreen(bool enable)
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static void EnableAlwaysOnTopScreenSaverLevel(bool enable)
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static void SetWindowLevel(int level)
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static int GetWindowLevel()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
        return 0;
    }
    
    public static void SetVisibleOnAllSpaces(bool enable)
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static bool CanAppearOverFullscreen()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
        return false;
    }
    
    public static void BringToFront()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static void HideDockIcon(bool hide)
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static void StartMonitoringSpaceChanges()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static void StopMonitoringSpaceChanges()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
#endif
}
