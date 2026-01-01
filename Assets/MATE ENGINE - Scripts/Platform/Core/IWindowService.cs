/**
 * Window service interface for cross-platform window management
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Interface for window management operations
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Get the main window handle
        /// </summary>
        IntPtr GetMainWindowHandle();

        /// <summary>
        /// Set window position and size
        /// </summary>
        void SetWindowPosition(int x, int y, int width, int height);

        /// <summary>
        /// Set whether window is always on top
        /// </summary>
        void SetAlwaysOnTop(bool enabled);

        /// <summary>
        /// Set window level (for more granular control than always on top)
        /// </summary>
        void SetWindowLevel(WindowLevel level);

        /// <summary>
        /// Set whether window is click-through (transparent to input)
        /// </summary>
        void SetClickThrough(bool enabled);

        /// <summary>
        /// Set window transparency
        /// </summary>
        void SetTransparency(float alpha);

        /// <summary>
        /// Get window rectangle
        /// </summary>
        PlatformRect GetWindowRect();

        /// <summary>
        /// Get client rectangle (interior of window)
        /// </summary>
        PlatformRect GetClientRect();

        /// <summary>
        /// Set whether window is visible on all workspaces/desktops
        /// </summary>
        void SetVisibleOnAllWorkspaces(bool visible, bool visibleOnFullScreen = false);

        /// <summary>
        /// Hide window from dock/taskbar
        /// </summary>
        void HideFromDock();

        /// <summary>
        /// Show window in dock/taskbar
        /// </summary>
        void ShowInDock();

        /// <summary>
        /// Move window to new position
        /// </summary>
        void MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        /// <summary>
        /// Check if window is visible
        /// </summary>
        bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Show or hide window
        /// </summary>
        void ShowWindow(IntPtr hWnd, int cmdShow);

        /// <summary>
        /// Convert client coordinates to screen coordinates
        /// </summary>
        PlatformPoint ClientToScreen(IntPtr hWnd, PlatformPoint clientPoint);
    }
}
