/**
 * Service locator for platform services
 * Provides a single entry point to access platform-specific implementations
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Service locator pattern implementation for platform services
    /// </summary>
    public static class PlatformServiceLocator
    {
        private static IPlatformService _platformService;
        private static bool _initialized = false;

        /// <summary>
        /// Get or create the platform service instance
        /// </summary>
        public static IPlatformService GetPlatformService()
        {
            if (!_initialized)
            {
                Initialize();
            }
            return _platformService;
        }

        /// <summary>
        /// Get the window service
        /// </summary>
        public static IWindowService GetWindowService()
        {
            return GetPlatformService().WindowService;
        }

        /// <summary>
        /// Get the screen service
        /// </summary>
        public static IScreenService GetScreenService()
        {
            return GetPlatformService().ScreenService;
        }

        /// <summary>
        /// Get the system tray service
        /// </summary>
        public static ISystemTrayService GetSystemTrayService()
        {
            return GetPlatformService().SystemTrayService;
        }

        /// <summary>
        /// Initialize the platform service based on current platform
        /// </summary>
        private static void Initialize()
        {
            if (_initialized)
                return;

            RuntimePlatform platform = Application.platform;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _platformService = new Windows.WindowsPlatformService();
            Debug.Log("[PlatformServiceLocator] Initialized Windows platform service");
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            _platformService = new MacOS.MacOSPlatformService();
            Debug.Log("[PlatformServiceLocator] Initialized macOS platform service");
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            _platformService = new Linux.LinuxPlatformService();
            Debug.Log("[PlatformServiceLocator] Initialized Linux platform service");
#else
            Debug.LogWarning("[PlatformServiceLocator] Unsupported platform: " + platform);
            _platformService = new UnsupportedPlatformService();
#endif

            if (!_platformService.IsSupported)
            {
                Debug.LogWarning("[PlatformServiceLocator] Platform service is not fully supported on " + platform);
            }

            _initialized = true;
        }

        /// <summary>
        /// Reset the service locator (useful for testing)
        /// </summary>
        public static void Reset()
        {
            if (_platformService != null)
            {
                // Cleanup if needed
                var systemTray = _platformService.SystemTrayService;
                if (systemTray != null)
                {
                    systemTray.Dispose();
                }
            }

            _platformService = null;
            _initialized = false;
        }

        /// <summary>
        /// Set a custom platform service (useful for testing or custom implementations)
        /// </summary>
        public static void SetCustomPlatformService(IPlatformService customService)
        {
            Reset();
            _platformService = customService;
            _initialized = true;
        }
    }

    /// <summary>
    /// Fallback service for unsupported platforms
    /// </summary>
    internal class UnsupportedPlatformService : IPlatformService
    {
        public IWindowService WindowService => new UnsupportedWindowService();
        public IScreenService ScreenService => new UnsupportedScreenService();
        public ISystemTrayService SystemTrayService => new UnsupportedSystemTrayService();
        public RuntimePlatform Platform => Application.platform;
        public bool IsSupported => false;
    }

    internal class UnsupportedWindowService : IWindowService
    {
        public IntPtr GetMainWindowHandle() => System.IntPtr.Zero;
        public void SetWindowPosition(int x, int y, int width, int height) { }
        public void SetAlwaysOnTop(bool enabled) { }
        public void SetWindowLevel(WindowLevel level) { }
        public void SetClickThrough(bool enabled) { }
        public void SetTransparency(float alpha) { }
        public PlatformRect GetWindowRect() => new PlatformRect();
        public PlatformRect GetClientRect() => new PlatformRect();
        public void SetVisibleOnAllWorkspaces(bool visible, bool visibleOnFullScreen = false) { }
        public void HideFromDock() { }
        public void ShowInDock() { }
        public void MoveWindow(System.IntPtr hWnd, int x, int y, int width, int height, bool repaint) { }
        public bool IsWindowVisible(System.IntPtr hWnd) => false;
        public void ShowWindow(System.IntPtr hWnd, int cmdShow) { }
        public PlatformPoint ClientToScreen(System.IntPtr hWnd, PlatformPoint clientPoint) => clientPoint;
    }

    internal class UnsupportedScreenService : IScreenService
    {
        public ScreenInfo[] GetAllScreens() => new ScreenInfo[0];
        public ScreenInfo GetScreenAtPoint(Vector2 point) => new ScreenInfo();
        public ScreenInfo GetScreenContainingWindow(System.IntPtr windowHandle) => new ScreenInfo();
        public Rect GetVirtualScreenBounds() => new Rect(0, 0, Screen.width, Screen.height);
        public Vector2 GetCursorPosition() => Input.mousePosition;
        public void SetCursorPosition(int x, int y) { }
        public bool IsCursorOverWindow(System.IntPtr windowHandle) => false;
        public int GetSystemMetrics(int index) => 0;
    }

    internal class UnsupportedSystemTrayService : ISystemTrayService
    {
        public event System.Action OnLeftClick;
        public event System.Action OnRightClick;

        public void Initialize(Texture2D icon, string tooltip) { }
        public void SetIcon(Texture2D icon) { }
        public void SetTooltip(string tooltip) { }
        public void ShowBalloon(string title, string message) { }
        public void AddMenuItem(string label, System.Action callback) { }
        public void AddSeparator() { }
        public void ClearMenu() { }
        public void Dispose() { }
    }
}
