using System;
using UnityEngine;

namespace MateEngine.Platform
{
    public interface IScreenService
    {
        // Cursor Operations
        bool GetCursorPosition(out Vector2Int position);
        bool SetCursorPosition(int x, int y);
        
        // Monitor Information
        MonitorInfo GetPrimaryMonitor();
        MonitorInfo GetMonitorFromPoint(Vector2Int point);
        MonitorInfo GetMonitorFromWindow(IntPtr hWnd);
        MonitorInfo[] GetAllMonitors();
        
        // Virtual Desktop
        Rect GetVirtualDesktopBounds();
        
        // System Metrics
        int GetSystemMetric(int nIndex);
    }

    public struct MonitorInfo
    {
        public Rect WorkArea;      // Excludes taskbar
        public Rect MonitorArea;   // Full screen
        public bool IsPrimary;
        public string DeviceName;
        
        public int Left => (int)MonitorArea.x;
        public int Top => (int)MonitorArea.y;
        public int Right => (int)(MonitorArea.x + MonitorArea.width);
        public int Bottom => (int)(MonitorArea.y + MonitorArea.height);
    }
}
