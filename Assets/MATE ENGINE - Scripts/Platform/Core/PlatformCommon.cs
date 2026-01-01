/**
 * Common data structures and enums for platform abstraction layer
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Window level for z-ordering
    /// </summary>
    public enum WindowLevel
    {
        Normal = 0,
        Floating = 1,
        ModalPanel = 2,
        MainMenu = 3,
        StatusBar = 4,
        PopUpMenu = 5,
        ScreenSaver = 6
    }

    /// <summary>
    /// Screen/monitor information
    /// </summary>
    [Serializable]
    public struct ScreenInfo
    {
        public Rect bounds;           // Full screen bounds
        public Rect workArea;         // Working area (excludes taskbar, etc.)
        public bool isPrimary;
        public string deviceName;
        
        public ScreenInfo(Rect bounds, Rect workArea, bool isPrimary, string deviceName = "")
        {
            this.bounds = bounds;
            this.workArea = workArea;
            this.isPrimary = isPrimary;
            this.deviceName = deviceName;
        }
    }

    /// <summary>
    /// Rectangle structure for platform operations
    /// </summary>
    [Serializable]
    public struct PlatformRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int Width => right - left;
        public int Height => bottom - top;

        public PlatformRect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public Rect ToUnityRect()
        {
            return new Rect(left, top, Width, Height);
        }

        public static PlatformRect FromUnityRect(Rect rect)
        {
            return new PlatformRect(
                (int)rect.x,
                (int)rect.y,
                (int)(rect.x + rect.width),
                (int)(rect.y + rect.height)
            );
        }
    }

    /// <summary>
    /// Point structure for platform operations
    /// </summary>
    [Serializable]
    public struct PlatformPoint
    {
        public int x;
        public int y;

        public PlatformPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public static PlatformPoint FromVector2(Vector2 vec)
        {
            return new PlatformPoint((int)vec.x, (int)vec.y);
        }
    }
}
