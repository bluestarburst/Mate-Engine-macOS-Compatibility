using System;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubScreenService : IScreenService
    {
        public bool GetCursorPosition(out Vector2Int position)
        {
            // Use Unity's Input as fallback
            position = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            return true;
        }
        
        public bool SetCursorPosition(int x, int y) => false;
        
        public MonitorInfo GetPrimaryMonitor()
        {
            return new MonitorInfo
            {
                WorkArea = new Rect(0, 0, Screen.width, Screen.height),
                MonitorArea = new Rect(0, 0, Screen.width, Screen.height),
                IsPrimary = true,
                DeviceName = "Primary"
            };
        }
        
        public MonitorInfo GetMonitorFromPoint(Vector2Int point) => GetPrimaryMonitor();
        public MonitorInfo GetMonitorFromWindow(IntPtr hWnd) => GetPrimaryMonitor();
        
        public MonitorInfo[] GetAllMonitors()
        {
            return new[] { GetPrimaryMonitor() };
        }
        
        public Rect GetVirtualDesktopBounds()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }

        public int GetSystemMetric(int nIndex)
        {
            // Return default stub values based on metric index
            return nIndex switch
            {
                0 => 0,                    // X offset
                1 => 0,                    // Y offset
                2 => Screen.width,         // Width
                3 => Screen.height,        // Height
                _ => 0                     // Other metrics
            };
        }
    }
}
