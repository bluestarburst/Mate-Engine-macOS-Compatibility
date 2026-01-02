using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsSystemTrayService : ISystemTrayService
    {
        public bool IsSupported => true;
        
        private Action onLeftClick;
        public event Action OnLeftClick
        {
            add { onLeftClick += value; }
            remove { onLeftClick -= value; }
        }
        
        public event Action OnRightClick
        {
            add { /* Right-click shows menu, not an event in existing TrayIcon implementation */ }
            remove { /* Not supported */ }
        }

        public bool Initialize(string appName, string tooltip, Texture2D icon, Func<List<(string, Action)>> buildMenu)
        {
            try
            {
                // Set the menu builder callback
                Utils.TrayIcon.OnBuildMenu = buildMenu;
                
                // Create the menu items from the builder
                List<(string, Action)> menuItems = buildMenu?.Invoke() ?? new List<(string, Action)>();
                
                // Initialize the tray icon with the menu items
                Utils.TrayIcon.Init(appName, tooltip, icon, menuItems);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize system tray: {ex.Message}");
                return false;
            }
        }

        public bool UpdateIcon(Texture2D icon)
        {
            // TrayIcon doesn't support dynamic icon updates after Init
            // This is a limitation of the underlying API
            Debug.LogWarning("TrayIcon does not support UpdateIcon after initialization");
            return false;
        }

        public bool UpdateTooltip(string tooltip)
        {
            // TrayIcon doesn't support dynamic tooltip updates after Init
            // This is a limitation of the underlying API
            Debug.LogWarning("TrayIcon does not support UpdateTooltip after initialization");
            return false;
        }

        public bool Remove()
        {
            // TrayIcon doesn't provide a public Remove method
            // The tray icon will be cleaned up when the app closes
            Debug.LogWarning("TrayIcon does not support Remove - cleanup happens on application exit");
            return false;
        }
    }
}
