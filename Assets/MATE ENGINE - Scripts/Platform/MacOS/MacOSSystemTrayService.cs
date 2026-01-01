/**
 * macOS system tray service implementation (Status Bar)
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    /// <summary>
    /// macOS implementation of system tray service (Status Bar)
    /// </summary>
    public class MacOSSystemTrayService : ISystemTrayService
    {
        private bool _initialized = false;

        public event Action OnLeftClick;
        public event Action OnRightClick;

        public void Initialize(Texture2D icon, string tooltip)
        {
#if UNITY_STANDALONE_OSX
            if (_initialized)
            {
                Debug.LogWarning("[MacOSSystemTrayService] Already initialized");
                return;
            }

            try
            {
                // TODO: Implement via native plugin
                // NativeCreateStatusBarItem(iconPath, tooltip);
                _initialized = true;
                Debug.Log("[MacOSSystemTrayService] Initialized (placeholder)");
            }
            catch (Exception ex)
            {
                Debug.LogError("[MacOSSystemTrayService] Failed to initialize: " + ex.Message);
            }
#else
            Debug.LogWarning("[MacOSSystemTrayService] Status bar only available on macOS");
#endif
        }

        public void SetIcon(Texture2D icon)
        {
#if UNITY_STANDALONE_OSX
            if (!_initialized)
            {
                Debug.LogWarning("[MacOSSystemTrayService] Not initialized");
                return;
            }

            // TODO: Implement via native plugin
            Debug.LogWarning("[MacOSSystemTrayService] SetIcon not yet implemented");
#endif
        }

        public void SetTooltip(string tooltip)
        {
#if UNITY_STANDALONE_OSX
            if (!_initialized)
            {
                Debug.LogWarning("[MacOSSystemTrayService] Not initialized");
                return;
            }

            // TODO: Implement via native plugin
            Debug.LogWarning("[MacOSSystemTrayService] SetTooltip not yet implemented");
#endif
        }

        public void ShowBalloon(string title, string message)
        {
#if UNITY_STANDALONE_OSX
            if (!_initialized)
            {
                Debug.LogWarning("[MacOSSystemTrayService] Not initialized");
                return;
            }

            // macOS uses NSUserNotification instead of balloons
            // TODO: Implement via native plugin
            Debug.LogWarning("[MacOSSystemTrayService] ShowBalloon not yet implemented");
#endif
        }

        public void AddMenuItem(string label, Action callback)
        {
            // TODO: Store menu items and implement via native plugin
            Debug.LogWarning("[MacOSSystemTrayService] AddMenuItem not yet implemented");
        }

        public void AddSeparator()
        {
            // TODO: Implement via native plugin
        }

        public void ClearMenu()
        {
            // TODO: Implement via native plugin
        }

        public void Dispose()
        {
#if UNITY_STANDALONE_OSX
            if (_initialized)
            {
                // TODO: Implement cleanup via native plugin
                _initialized = false;
                Debug.Log("[MacOSSystemTrayService] Disposed");
            }
#endif
        }

#if UNITY_STANDALONE_OSX
        // Native plugin methods (to be implemented in Objective-C)
        // [DllImport("NativeWindowManager")]
        // private static extern void NativeCreateStatusBarItem(string iconPath, string tooltip);
#endif
    }
}
