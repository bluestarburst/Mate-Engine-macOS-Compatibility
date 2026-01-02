using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsSystemTrayService : ISystemTrayService
    {
        public bool IsSupported => true;
        
        public event Action OnLeftClick
        {
            add { Utils.TrayIcon.SetLeftClickAction(value); }
            remove { /* Not supported by existing implementation */ }
        }
        
        public event Action OnRightClick
        {
            add { /* Right-click shows menu, not an event in existing implementation */ }
            remove { /* Not supported */ }
        }

        public bool Initialize(string appName, string tooltip, Texture2D icon, Func<List<(string, Action)>> buildMenu)
        {
            try
            {
                // Set the menu builder
                Utils.TrayIcon.OnBuildMenu = buildMenu;
                
                // Initialize with null actions since we're using the OnBuildMenu callback
                Utils.TrayIcon.Init(appName, tooltip, icon, null);
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
            try
            {
                Utils.TrayIcon.UpdateIcon(icon);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateTooltip(string tooltip)
        {
            try
            {
                Utils.TrayIcon.UpdateTooltip(tooltip);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Remove()
        {
            try
            {
                Utils.TrayIcon.Remove();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
