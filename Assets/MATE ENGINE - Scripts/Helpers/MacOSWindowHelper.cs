using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// macOS-specific window management helper
/// Provides functionality to make Unity windows appear above fullscreen applications
/// Based on solution from: https://github.com/electron/electron/issues/10078
/// </summary>
public static class MacOSWindowHelper
{
    /// <summary>
    /// Extended window info including CGWindowNumber for tracking specific windows
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowInfo
    {
        public int windowNumber;   // CGWindowNumber - unique window identifier
        public int ownerPID;       // Process ID of window owner
        public float x;
        public float y;
        public float width;
        public float height;
        [MarshalAs(UnmanagedType.U1)] public bool isValid;

        public RECT ToRECT()
        {
            return new RECT
            {
                Left = (int)x,
                Top = (int)y,
                Right = (int)(x + width),
                Bottom = (int)(y + height)
            };
        }
    }

    public const int MAX_WINDOW_LIST = 32;

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
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_RequestScreenRecordingPermission")]
    private static extern void _RequestScreenRecordingPermission();
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_OpenScreenRecordingSettings")]
    private static extern void _OpenScreenRecordingSettings();
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetUnityWindowBounds")]
    private static extern MacOSWindowTracker.BoundsResult _GetUnityWindowBounds();

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetUnityBackingScale")]
    private static extern float _GetUnityBackingScale();

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetScreenBounds")]
    private static extern MacOSWindowTracker.BoundsResult _GetScreenBounds();

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetCurrentPID")]
    private static extern int _GetCurrentPID();

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetWindowByNumber")]
    private static extern WindowInfo _GetWindowByNumber(int windowNumber);

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_EnumerateWindows")]
    private static extern int _EnumerateWindows([Out] WindowInfo[] outWindows);

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_IsWindowVisible")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static extern bool _IsWindowVisible(int windowNumber);

    // ===================================================================================
    // TRAY ICON IMPORTS
    // ===================================================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TrayCallbackDelegate(int actionId);

    [StructLayout(LayoutKind.Sequential)]
    public struct MenuItemData
    {
        public string text;
        public int id;
    }

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_CreateTrayIcon")]
    private static extern void _CreateTrayIcon(string tooltip, TrayCallbackDelegate callback);

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_SetTrayIconImage")]
    private static extern void _SetTrayIconImage(IntPtr buffer, int length);

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_SetTrayTooltip")]
    private static extern void _SetTrayTooltip(string tooltip);

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_DestroyTrayIcon")]
    private static extern void _DestroyTrayIcon();

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_ShowTrayMenu")]
    private static extern void _ShowTrayMenu(MenuItemData[] items, int count);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_ShowTrayMenuAtMouse")]
    private static extern void _ShowTrayMenuAtMouse(MenuItemData[] items, int count);
    
    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_ShowNotification")]
    private static extern void _ShowNotification(string title, string message);

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
    
    /// <summary>
    /// Trigger the first-time Screen Recording permission prompt.
    /// This will call CGWindowListCopyWindowInfo which causes macOS to show the system dialog.
    /// NOTE: The prompt only appears the FIRST time. If denied, use OpenScreenRecordingSettings() instead.
    /// </summary>
    public static void RequestScreenRecordingPermission()
    {
        try
        {
            _RequestScreenRecordingPermission();
            Debug.Log("MacOSWindowHelper: Triggered Screen Recording permission check. " +
                "If this is the first time, you should see a system prompt.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to request screen recording permission: {e.Message}");
        }
    }

    /// <summary>
    /// Open System Settings directly to the Screen Recording permission page.
    /// Use this if the user denied permission or needs to re-enable it.
    /// </summary>
    public static void OpenScreenRecordingSettings()
    {
        try
        {
            _OpenScreenRecordingSettings();
            Debug.Log("MacOSWindowHelper: Opening System Settings > Privacy & Security > Screen Recording");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to open screen recording settings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Get the bounds of the Unity window in CoreGraphics coordinates (origin at top-left)
    /// Used for accurate coordinate transformation when snapping windows on macOS
    /// </summary>
    /// <returns>BoundsResult with window position and size</returns>
    public static MacOSWindowTracker.BoundsResult GetUnityWindowBounds()
    {
        try
        {
            return _GetUnityWindowBounds();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning($"MacOSWindowHelper: Failed to get Unity window bounds: {e.Message}");
            return new MacOSWindowTracker.BoundsResult();
        }
    }

    public static float GetUnityBackingScale()
    {
        try
        {
            return _GetUnityBackingScale();
        }
        catch
        {
            return 1.0f;
        }
    }

    /// <summary>
    /// Get the bounds of the screen containing the Unity window (in device pixels)
    /// Useful for clamping window position to keep it on screen
    /// </summary>
    /// <returns>BoundsResult with screen position and size in device pixels</returns>
    public static MacOSWindowTracker.BoundsResult GetScreenBounds()
    {
        try
        {
            return _GetScreenBounds();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogWarning($"MacOSWindowHelper: Failed to get screen bounds: {e.Message}");
            return new MacOSWindowTracker.BoundsResult();
        }
    }

    /// <summary>
    /// Get our own process ID (for filtering out Unity windows)
    /// </summary>
    public static int GetCurrentPID()
    {
        try
        {
            return _GetCurrentPID();
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get the bounds of a specific window by its CGWindowNumber.
    /// Use this to track a window you've previously snapped to.
    /// </summary>
    /// <param name="windowNumber">The CGWindowNumber from a previous enumeration</param>
    /// <returns>WindowInfo with current bounds, or invalid if window no longer exists</returns>
    public static WindowInfo GetWindowByNumber(int windowNumber)
    {
        try
        {
            return _GetWindowByNumber(windowNumber);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to get window by number: {e.Message}");
            return new WindowInfo();
        }
    }

    /// <summary>
    /// Enumerate all visible windows suitable for snapping.
    /// Windows are returned in Z-order (front to back).
    /// </summary>
    /// <returns>Array of WindowInfo structs, or empty array on error</returns>
    public static WindowInfo[] EnumerateWindows()
    {
        try
        {
            WindowInfo[] windows = new WindowInfo[MAX_WINDOW_LIST];
            int count = _EnumerateWindows(windows);

            // Return only the valid entries
            WindowInfo[] result = new WindowInfo[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = windows[i];
            }
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"MacOSWindowHelper: Failed to enumerate windows: {e.Message}");
            return new WindowInfo[0];
        }
    }

    /// <summary>
    /// Check if a specific window is still visible on screen.
    /// </summary>
    /// <param name="windowNumber">The CGWindowNumber to check</param>
    /// <returns>True if window is still visible, false otherwise</returns>
    public static bool IsWindowVisible(int windowNumber)
    {
        try
        {
            return _IsWindowVisible(windowNumber);
        }
        catch
        {
            return false;
        }
    }

    // ===================================================================================
    // TRAY ICON HELPERS
    // ===================================================================================

    public static void CreateTrayIcon(string tooltip, TrayCallbackDelegate callback)
    {
        try
        {
            _CreateTrayIcon(tooltip, callback);
        }
        catch (System.Exception e) { Debug.LogWarning($"[MacOS] CreateTrayIcon failed: {e.Message}"); }
    }

    public static void SetTrayIconImage(byte[] pngData)
    {
        try
        {
            if (pngData == null || pngData.Length == 0) return;
            // Pin data or use unsafe context?
            // Actually, we can use Marshal.AllocHGlobal or just pass byte[] if DllImport supported it.
            // But we defined it as IntPtr buffer.
            
            GCHandle pinnedArray = GCHandle.Alloc(pngData, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            _SetTrayIconImage(pointer, pngData.Length);
            pinnedArray.Free();
        }
        catch (System.Exception e) { Debug.LogWarning($"[MacOS] SetTrayIconImage failed: {e.Message}"); }
    }

    public static void SetTrayTooltip(string tooltip)
    {
        try { _SetTrayTooltip(tooltip); } catch {}
    }

    public static void DestroyTrayIcon()
    {
        try { _DestroyTrayIcon(); } catch {}
    }

    public static void ShowTrayMenuAtMouse(List<(string text, int id)> items)
    {
        try
        {
            if (items == null || items.Count == 0) return;
            MenuItemData[] data = new MenuItemData[items.Count];
            for (int i=0; i<items.Count; i++)
            {
                data[i].text = items[i].text;
                data[i].id = items[i].id;
            }
            _ShowTrayMenuAtMouse(data, items.Count);
        }
        catch (System.Exception e) { Debug.LogWarning($"[MacOS] ShowTrayMenuAtMouse failed: {e.Message}"); }
    }

    public static void ShowNotification(string title, string message)
    {
        try { _ShowNotification(title, message); } catch {}
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
    
    public static void RequestScreenRecordingPermission()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static void OpenScreenRecordingSettings()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
    }
    
    public static MacOSWindowTracker.BoundsResult GetUnityWindowBounds()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
        return new MacOSWindowTracker.BoundsResult();
    }

    public static MacOSWindowTracker.BoundsResult GetScreenBounds()
    {
        Debug.LogWarning("MacOSWindowHelper: Not available on this platform");
        return new MacOSWindowTracker.BoundsResult();
    }

    public static int GetCurrentPID()
    {
        return 0;
    }

    public static WindowInfo GetWindowByNumber(int windowNumber)
    {
        return new WindowInfo();
    }

    public static WindowInfo[] EnumerateWindows()
    {
        return new WindowInfo[0];
    }

    public static bool IsWindowVisible(int windowNumber)
    {
        return false;
    }

    public static float GetUnityBackingScale()
    {
        return 1.0f;
    }
    
    // Stub implementations for Tray Icon on non-macOS
    public delegate void TrayCallbackDelegate(int actionId);
    public static void CreateTrayIcon(string tooltip, TrayCallbackDelegate callback) {}
    public static void SetTrayIconImage(byte[] pngData) {}
    public static void SetTrayTooltip(string tooltip) {}
    public static void DestroyTrayIcon() {}
    public static void ShowTrayMenuAtMouse(List<(string text, int id)> items) {}
    public static void ShowNotification(string title, string message) {}
#endif
}
