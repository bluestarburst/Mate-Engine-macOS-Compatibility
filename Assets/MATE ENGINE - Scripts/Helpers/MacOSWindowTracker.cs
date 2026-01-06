using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// macOS Window Tracker - Get positions of other app windows and the Dock
/// Enables window sitting and dock sitting on macOS
/// 
/// REQUIRES: Screen Recording permission (System Settings > Privacy & Security > Screen Recording)
/// </summary>
public static class MacOSWindowTracker
{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    
    /// <summary>
    /// Struct matching the native BoundsResult from MacOSWindowHelper
    /// Contains position and size of a window
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundsResult
    {
        public float x;
        public float y;
        public float width;
        public float height;
        [MarshalAs(UnmanagedType.U1)] public bool isValid;
    }

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetDockBounds")]
    private static extern BoundsResult _GetDockBounds();

    [DllImport("MacOSWindowHelper", EntryPoint = "MacOS_GetActiveWindowBounds")]
    private static extern BoundsResult _GetActiveWindowBounds();

    /// <summary>
    /// Get the screen-space rectangle of the macOS Dock
    /// 
    /// Coordinate System:
    /// - macOS CoreGraphics: (0,0) is top-left, Y increases downward
    /// - Unity Screen Space: (0,0) is bottom-left, Y increases upward
    /// This method returns Unity Screen Space coordinates
    /// 
    /// Returns: Rect with dock position/size, or Rect.zero if invalid or permission denied
    /// </summary>
    public static Rect GetDockRect()
    {
#if UNITY_STANDALONE_OSX
        if (Application.platform != RuntimePlatform.OSXPlayer)
            return Rect.zero;

        BoundsResult res = _GetDockBounds();
        if (!res.isValid || res.width <= 0f || res.height <= 0f)
            return Rect.zero;

        return ConvertToUnityRect(res, "Dock");
#else
        return Rect.zero;
#endif
    }

    /// <summary>
    /// Get the dock rectangle in DESKTOP coordinates (for AvatarWindowHandler)
    /// Returns: RECT struct with desktop coordinates (top-left origin, pixels with Retina scale)
    /// </summary>
    public static RECT GetDockRectDesktop()
    {
#if UNITY_STANDALONE_OSX
        if (Application.platform != RuntimePlatform.OSXPlayer)
            return new RECT();

        BoundsResult res = _GetDockBounds();
        if (!res.isValid || res.width <= 0f || res.height <= 0f)
            return new RECT();

        return ConvertToDesktopRect(res, "Dock");
#else
        return new RECT();
#endif
    }

    /// <summary>
    /// Get the screen-space rectangle of the currently active (focused) window
    /// 
    /// This is the window of the app the user is currently interacting with.
    /// On a Mac with the "Always-On-Top" feature enabled, this lets your overlay 
    /// know where to sit.
    /// 
    /// Coordinate System:
    /// - macOS CoreGraphics: (0,0) is top-left, Y increases downward
    /// - Unity Screen Space: (0,0) is bottom-left, Y increases upward
    /// This method returns Unity Screen Space coordinates
    /// 
    /// Returns: Rect with active window position/size, or Rect.zero if invalid or permission denied
    /// </summary>
    public static Rect GetActiveWindowRect()
    {
#if UNITY_STANDALONE_OSX
        if (Application.platform != RuntimePlatform.OSXPlayer)
            return Rect.zero;

        BoundsResult res = _GetActiveWindowBounds();
        if (!res.isValid || res.width <= 0f || res.height <= 0f)
            return Rect.zero;

        return ConvertToUnityRect(res, "ActiveWindow");
#else
        return Rect.zero;
#endif
    }

    /// <summary>
    /// Get the active window rectangle in DESKTOP coordinates (for AvatarWindowHandler)
    /// Returns: RECT struct with desktop coordinates (top-left origin, pixels with Retina scale)
    /// </summary>
    public static RECT GetActiveWindowRectDesktop()
    {
#if UNITY_STANDALONE_OSX
        if (Application.platform != RuntimePlatform.OSXPlayer)
            return new RECT();

        BoundsResult res = _GetActiveWindowBounds();
        if (!res.isValid || res.width <= 0f || res.height <= 0f)
            return new RECT();

        return ConvertToDesktopRect(res, "ActiveWindow");
#else
        return new RECT();
#endif
    }

    /// <summary>
    /// Check if the Dock is visible (not auto-hidden)
    /// Returns false if dock height is less than 10 pixels (likely hidden)
    /// </summary>
    public static bool IsDockVisible()
    {
        Rect dockRect = GetDockRect();
        return dockRect != Rect.zero && dockRect.height > 10;
    }

    /// <summary>
    /// Get the world position where a character should sit on top of the Dock
    /// (sitting on the macOS Dock bar at the bottom)
    /// </summary>
    public static Vector3 GetDockSitPosition(Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;

        if (camera == null)
            return Vector3.zero;

        Rect dockRect = GetDockRect();
        if (dockRect == Rect.zero || dockRect.height < 10)
            return Vector3.zero;

        // Position at the top-center of the dock
        Vector3 screenPos = new Vector3(
            dockRect.x + dockRect.width * 0.5f,  // Center X
            dockRect.y + dockRect.height,         // Top edge of dock in Unity Screen Space
            0
        );

        return camera.ScreenToWorldPoint(screenPos);
    }

    /// <summary>
    /// Convert CoreGraphics bounds (pixels, top-left) to Unity screen space (pixels, bottom-left).
    /// </summary>
    private static Rect ConvertToUnityRect(BoundsResult res, string label)
    {
        float unityY = Screen.height - (res.y + res.height);
        Rect rect = new Rect(res.x, unityY, res.width, res.height);

        Debug.Log($"MacOSWindowTracker: {label} rect (pixels, top-left→Unity) = {rect}");
        return rect;
    }

    /// <summary>
    /// Convert CoreGraphics bounds (pixels, top-left) to DESKTOP coordinates (pixels, top-left).
    /// Used by AvatarWindowHandler for coordinate matching with ComputeZoneDesktop.
    /// </summary>
    private static RECT ConvertToDesktopRect(BoundsResult res, string label)
    {
        int left = (int)res.x;
        int top = (int)res.y;
        int width = (int)res.width;
        int height = (int)res.height;

        RECT rect = new RECT { Left = left, Top = top, Right = left + width, Bottom = top + height };
        Debug.Log($"MacOSWindowTracker: {label} desktop rect (pixels) = L:{rect.Left} T:{rect.Top} R:{rect.Right} B:{rect.Bottom}");
        return rect;
    }

    /// <summary>
    /// Get the world position where a character should sit on top of the active window
    /// (sitting on the title bar or top edge of another app's window)
    /// </summary>
    public static Vector3 GetWindowSitPosition(Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;

        if (camera == null)
            return Vector3.zero;

        Rect windowRect = GetActiveWindowRect();
        if (windowRect == Rect.zero)
            return Vector3.zero;

        // Position at the top-center of the active window
        Vector3 screenPos = new Vector3(
            windowRect.x + windowRect.width * 0.5f,  // Center X
            windowRect.y + windowRect.height,         // Top edge in Unity Screen Space
            0
        );

        return camera.ScreenToWorldPoint(screenPos);
    }

#else
    // Stub implementations for non-macOS platforms
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundsResult
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public bool isValid;
    }

    public static Rect GetDockRect()
    {
        Debug.LogWarning("MacOSWindowTracker: Not available on this platform");
        return Rect.zero;
    }

    public static Rect GetActiveWindowRect()
    {
        Debug.LogWarning("MacOSWindowTracker: Not available on this platform");
        return Rect.zero;
    }

    public static bool IsDockVisible()
    {
        return false;
    }

    public static Vector3 GetDockSitPosition(Camera camera = null)
    {
        return Vector3.zero;
    }

    public static Vector3 GetWindowSitPosition(Camera camera = null)
    {
        return Vector3.zero;
    }
#endif
}
