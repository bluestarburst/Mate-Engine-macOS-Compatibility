/**
 * Linux platform service (placeholder)
 * Main entry point for Linux platform-specific services
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using UnityEngine;

namespace MateEngine.Platform.Linux
{
    /// <summary>
    /// Linux platform service implementation (placeholder)
    /// </summary>
    public class LinuxPlatformService : IPlatformService
    {
        private readonly LinuxWindowService _windowService;
        private readonly LinuxScreenService _screenService;
        private readonly LinuxSystemTrayService _systemTrayService;

        public LinuxPlatformService()
        {
            _windowService = new LinuxWindowService();
            _screenService = new LinuxScreenService();
            _systemTrayService = new LinuxSystemTrayService();
            
            Debug.LogWarning("[LinuxPlatformService] Linux support is currently a placeholder. Full implementation pending.");
        }

        public IWindowService WindowService => _windowService;
        public IScreenService ScreenService => _screenService;
        public ISystemTrayService SystemTrayService => _systemTrayService;
        public RuntimePlatform Platform => RuntimePlatform.LinuxPlayer;

#if UNITY_STANDALONE_LINUX
        public bool IsSupported => false; // Not yet fully implemented
#else
        public bool IsSupported => false;
#endif
    }

    /// <summary>
    /// Linux window service placeholder
    /// </summary>
    public class LinuxWindowService : IWindowService
    {
        public IntPtr GetMainWindowHandle() => IntPtr.Zero;
        public void SetWindowPosition(int x, int y, int width, int height) 
        {
            Debug.LogWarning("[LinuxWindowService] SetWindowPosition not implemented");
        }
        public void SetAlwaysOnTop(bool enabled) 
        {
            Debug.LogWarning("[LinuxWindowService] SetAlwaysOnTop not implemented");
        }
        public void SetWindowLevel(WindowLevel level) 
        {
            Debug.LogWarning("[LinuxWindowService] SetWindowLevel not implemented");
        }
        public void SetClickThrough(bool enabled) 
        {
            Debug.LogWarning("[LinuxWindowService] SetClickThrough not implemented");
        }
        public void SetTransparency(float alpha) 
        {
            Debug.LogWarning("[LinuxWindowService] SetTransparency not implemented");
        }
        public PlatformRect GetWindowRect() => new PlatformRect();
        public PlatformRect GetClientRect() => new PlatformRect();
        public void SetVisibleOnAllWorkspaces(bool visible, bool visibleOnFullScreen = false) 
        {
            Debug.LogWarning("[LinuxWindowService] SetVisibleOnAllWorkspaces not implemented");
        }
        public void HideFromDock() 
        {
            Debug.LogWarning("[LinuxWindowService] HideFromDock not implemented");
        }
        public void ShowInDock() 
        {
            Debug.LogWarning("[LinuxWindowService] ShowInDock not implemented");
        }
        public void MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint) 
        {
            Debug.LogWarning("[LinuxWindowService] MoveWindow not implemented");
        }
        public bool IsWindowVisible(IntPtr hWnd) => false;
        public void ShowWindow(IntPtr hWnd, int cmdShow) 
        {
            Debug.LogWarning("[LinuxWindowService] ShowWindow not implemented");
        }
        public PlatformPoint ClientToScreen(IntPtr hWnd, PlatformPoint clientPoint) => clientPoint;
    }

    /// <summary>
    /// Linux screen service placeholder
    /// </summary>
    public class LinuxScreenService : IScreenService
    {
        public ScreenInfo[] GetAllScreens()
        {
            // Use Unity's fallback
            return new ScreenInfo[]
            {
                new ScreenInfo(
                    new Rect(0, 0, Screen.width, Screen.height),
                    new Rect(0, 0, Screen.width, Screen.height),
                    true,
                    "Primary Display"
                )
            };
        }
        
        public ScreenInfo GetScreenAtPoint(Vector2 point)
        {
            return new ScreenInfo(
                new Rect(0, 0, Screen.width, Screen.height),
                new Rect(0, 0, Screen.width, Screen.height),
                true,
                "Primary Display"
            );
        }
        
        public ScreenInfo GetScreenContainingWindow(IntPtr windowHandle) => GetScreenAtPoint(Vector2.zero);
        public Rect GetVirtualScreenBounds() => new Rect(0, 0, Screen.width, Screen.height);
        public Vector2 GetCursorPosition() => Input.mousePosition;
        public void SetCursorPosition(int x, int y) 
        {
            Debug.LogWarning("[LinuxScreenService] SetCursorPosition not implemented");
        }
        public bool IsCursorOverWindow(IntPtr windowHandle) => false;
        public int GetSystemMetrics(int index) => 0;
    }

    /// <summary>
    /// Linux system tray service placeholder
    /// </summary>
    public class LinuxSystemTrayService : ISystemTrayService
    {
        public event Action OnLeftClick;
        public event Action OnRightClick;

        public void Initialize(Texture2D icon, string tooltip) 
        {
            Debug.LogWarning("[LinuxSystemTrayService] System tray not yet implemented for Linux");
        }
        public void SetIcon(Texture2D icon) { }
        public void SetTooltip(string tooltip) { }
        public void ShowBalloon(string title, string message) { }
        public void AddMenuItem(string label, Action callback) { }
        public void AddSeparator() { }
        public void ClearMenu() { }
        public void Dispose() { }
    }
}
