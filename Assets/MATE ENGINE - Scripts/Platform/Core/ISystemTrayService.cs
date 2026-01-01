/**
 * System tray service interface for cross-platform tray icon/status bar
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Interface for system tray / status bar operations
    /// </summary>
    public interface ISystemTrayService
    {
        /// <summary>
        /// Initialize the system tray icon
        /// </summary>
        void Initialize(Texture2D icon, string tooltip);

        /// <summary>
        /// Set the tray icon image
        /// </summary>
        void SetIcon(Texture2D icon);

        /// <summary>
        /// Set the tooltip text
        /// </summary>
        void SetTooltip(string tooltip);

        /// <summary>
        /// Show a notification balloon (Windows) or notification (macOS/Linux)
        /// </summary>
        void ShowBalloon(string title, string message);

        /// <summary>
        /// Add a menu item to the tray icon menu
        /// </summary>
        void AddMenuItem(string label, Action callback);

        /// <summary>
        /// Add a separator to the menu
        /// </summary>
        void AddSeparator();

        /// <summary>
        /// Remove all menu items
        /// </summary>
        void ClearMenu();

        /// <summary>
        /// Dispose of the tray icon
        /// </summary>
        void Dispose();

        /// <summary>
        /// Event fired when left-clicking the tray icon
        /// </summary>
        event Action OnLeftClick;

        /// <summary>
        /// Event fired when right-clicking the tray icon
        /// </summary>
        event Action OnRightClick;
    }
}
