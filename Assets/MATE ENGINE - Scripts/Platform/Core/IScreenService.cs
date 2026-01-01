/**
 * Screen service interface for cross-platform monitor/display operations
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Interface for screen/monitor operations
    /// </summary>
    public interface IScreenService
    {
        /// <summary>
        /// Get information about all screens/monitors
        /// </summary>
        ScreenInfo[] GetAllScreens();

        /// <summary>
        /// Get the screen containing the specified point
        /// </summary>
        ScreenInfo GetScreenAtPoint(Vector2 point);

        /// <summary>
        /// Get the screen containing the specified window
        /// </summary>
        ScreenInfo GetScreenContainingWindow(IntPtr windowHandle);

        /// <summary>
        /// Get the bounds of the virtual screen (all monitors combined)
        /// </summary>
        Rect GetVirtualScreenBounds();

        /// <summary>
        /// Get current cursor position in screen coordinates
        /// </summary>
        Vector2 GetCursorPosition();

        /// <summary>
        /// Set cursor position in screen coordinates
        /// </summary>
        void SetCursorPosition(int x, int y);

        /// <summary>
        /// Check if cursor is over the specified window
        /// </summary>
        bool IsCursorOverWindow(IntPtr windowHandle);

        /// <summary>
        /// Get system metrics (e.g., screen dimensions)
        /// </summary>
        int GetSystemMetrics(int index);
    }
}
