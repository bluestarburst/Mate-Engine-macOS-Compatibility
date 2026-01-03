/**
 * Windows system tray service implementation
 * Wraps existing TrayIcon implementation
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    /// <summary>
    /// Windows implementation of system tray service
    /// This wraps the existing Utils.TrayIcon implementation
    /// </summary>
    public class WindowsSystemTrayService : ISystemTrayService
    {
        private bool _initialized = false;
        private List<(string, Action)> _menuItems = new List<(string, Action)>();

        public event Action OnLeftClick;
        public event Action OnRightClick;

        public void Initialize(Texture2D icon, string tooltip)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (_initialized)
            {
                Debug.LogWarning("[WindowsSystemTrayService] Already initialized");
                return;
            }

            try
            {
                // Build menu callback function
                Utils.TrayIcon.OnBuildMenu = () => _menuItems;
                
                // Initialize the existing TrayIcon
                Utils.TrayIcon.Init("MateEngine", tooltip, icon, _menuItems);
                
                _initialized = true;
                Debug.Log("[WindowsSystemTrayService] Initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError("[WindowsSystemTrayService] Failed to initialize: " + ex.Message);
            }
#else
            Debug.LogWarning("[WindowsSystemTrayService] System tray only available on Windows standalone builds");
#endif
        }

        public void SetIcon(Texture2D icon)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!_initialized)
            {
                Debug.LogWarning("[WindowsSystemTrayService] Not initialized");
                return;
            }

            // The existing TrayIcon doesn't expose a method to change the icon dynamically
            // This would require modifying the existing implementation
            Debug.LogWarning("[WindowsSystemTrayService] SetIcon not implemented in current TrayIcon");
#endif
        }

        public void SetTooltip(string tooltip)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!_initialized)
            {
                Debug.LogWarning("[WindowsSystemTrayService] Not initialized");
                return;
            }

            // The existing TrayIcon doesn't expose a method to change tooltip dynamically
            Debug.LogWarning("[WindowsSystemTrayService] SetTooltip not implemented in current TrayIcon");
#endif
        }

        public void ShowBalloon(string title, string message)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (!_initialized)
            {
                Debug.LogWarning("[WindowsSystemTrayService] Not initialized");
                return;
            }

            // The existing TrayIcon doesn't expose a ShowBalloon method
            Debug.LogWarning("[WindowsSystemTrayService] ShowBalloon not implemented in current TrayIcon");
#endif
        }

        public void AddMenuItem(string label, Action callback)
        {
            _menuItems.Add((label, callback));
            
            // If already initialized, we'd need to rebuild the menu
            // The current TrayIcon implementation doesn't support dynamic menu updates
        }

        public void AddSeparator()
        {
            _menuItems.Add(("---", null));
        }

        public void ClearMenu()
        {
            _menuItems.Clear();
        }

        public void Dispose()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (_initialized)
            {
                try
                {
                    // The TrayIcon class automatically cleans up on Application.quitting
                    // We just need to mark ourselves as no longer initialized
                    _initialized = false;
                    Debug.Log("[WindowsSystemTrayService] Disposed successfully");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[WindowsSystemTrayService] Error during disposal: " + ex.Message);
                }
            }
#endif
        }
    }
}
