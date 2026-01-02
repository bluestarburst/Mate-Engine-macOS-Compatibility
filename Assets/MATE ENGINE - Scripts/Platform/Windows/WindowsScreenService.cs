using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsScreenService : IScreenService
    {
        public bool GetCursorPosition(out Vector2Int position)
        {
            Kirurobo.WinApi.POINT point;
            bool result = Kirurobo.WinApi.GetCursorPos(out point);
            position = new Vector2Int(point.x, point.y);
            return result;
        }

        public bool SetCursorPosition(int x, int y)
        {
            return Kirurobo.WinApi.SetCursorPos(x, y);
        }

        public MonitorInfo GetPrimaryMonitor()
        {
            MonitorInfo primary = new MonitorInfo();
            
            // Enumerate all monitors and find primary
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, 
                (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
                {
                    MONITORINFO mi = new MONITORINFO();
                    mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        // Get device name
                        StringBuilder sb = new StringBuilder(32);
                        MONITORINFOEX miex = new MONITORINFOEX();
                        miex.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                        
                        string deviceName = "Unknown";
                        if (GetMonitorInfo(hMonitor, ref miex))
                        {
                            deviceName = miex.szDevice;
                        }
                        
                        // Check if this is the primary monitor
                        bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                        
                        if (isPrimary)
                        {
                            primary = new MonitorInfo
                            {
                                WorkArea = new Rect(mi.rcWork.left, mi.rcWork.top, 
                                    mi.rcWork.right - mi.rcWork.left, mi.rcWork.bottom - mi.rcWork.top),
                                MonitorArea = new Rect(mi.rcMonitor.left, mi.rcMonitor.top,
                                    mi.rcMonitor.right - mi.rcMonitor.left, mi.rcMonitor.bottom - mi.rcMonitor.top),
                                IsPrimary = true,
                                DeviceName = deviceName
                            };
                        }
                    }
                    return true; // Continue enumeration
                }, IntPtr.Zero);
            
            return primary;
        }

        public MonitorInfo GetMonitorFromPoint(Vector2Int point)
        {
            POINT pt = new POINT { x = point.x, y = point.y };
            IntPtr hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            
            if (hMonitor != IntPtr.Zero)
            {
                MONITORINFO mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    // Get device name
                    MONITORINFOEX miex = new MONITORINFOEX();
                    miex.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                    
                    string deviceName = "Unknown";
                    if (GetMonitorInfo(hMonitor, ref miex))
                    {
                        deviceName = miex.szDevice;
                    }
                    
                    bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                    
                    return new MonitorInfo
                    {
                        WorkArea = new Rect(mi.rcWork.left, mi.rcWork.top,
                            mi.rcWork.right - mi.rcWork.left, mi.rcWork.bottom - mi.rcWork.top),
                        MonitorArea = new Rect(mi.rcMonitor.left, mi.rcMonitor.top,
                            mi.rcMonitor.right - mi.rcMonitor.left, mi.rcMonitor.bottom - mi.rcMonitor.top),
                        IsPrimary = isPrimary,
                        DeviceName = deviceName
                    };
                }
            }
            
            return GetPrimaryMonitor();
        }

        public MonitorInfo GetMonitorFromWindow(IntPtr windowHandle)
        {
            IntPtr hMonitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
            
            if (hMonitor != IntPtr.Zero)
            {
                MONITORINFO mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    // Get device name
                    MONITORINFOEX miex = new MONITORINFOEX();
                    miex.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                    
                    string deviceName = "Unknown";
                    if (GetMonitorInfo(hMonitor, ref miex))
                    {
                        deviceName = miex.szDevice;
                    }
                    
                    bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                    
                    return new MonitorInfo
                    {
                        WorkArea = new Rect(mi.rcWork.left, mi.rcWork.top,
                            mi.rcWork.right - mi.rcWork.left, mi.rcWork.bottom - mi.rcWork.top),
                        MonitorArea = new Rect(mi.rcMonitor.left, mi.rcMonitor.top,
                            mi.rcMonitor.right - mi.rcMonitor.left, mi.rcMonitor.bottom - mi.rcMonitor.top),
                        IsPrimary = isPrimary,
                        DeviceName = deviceName
                    };
                }
            }
            
            return GetPrimaryMonitor();
        }

        public MonitorInfo[] GetAllMonitors()
        {
            List<MonitorInfo> monitors = new List<MonitorInfo>();
            
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
                {
                    MONITORINFO mi = new MONITORINFO();
                    mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        // Get device name
                        MONITORINFOEX miex = new MONITORINFOEX();
                        miex.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                        
                        string deviceName = "Unknown";
                        if (GetMonitorInfo(hMonitor, ref miex))
                        {
                            deviceName = miex.szDevice;
                        }
                        
                        bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                        
                        monitors.Add(new MonitorInfo
                        {
                            WorkArea = new Rect(mi.rcWork.left, mi.rcWork.top,
                                mi.rcWork.right - mi.rcWork.left, mi.rcWork.bottom - mi.rcWork.top),
                            MonitorArea = new Rect(mi.rcMonitor.left, mi.rcMonitor.top,
                                mi.rcMonitor.right - mi.rcMonitor.left, mi.rcMonitor.bottom - mi.rcMonitor.top),
                            IsPrimary = isPrimary,
                            DeviceName = deviceName
                        });
                    }
                    return true; // Continue enumeration
                }, IntPtr.Zero);
            
            return monitors.ToArray();
        }

        public Rect GetVirtualDesktopBounds()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            
            MonitorInfo[] monitors = GetAllMonitors();
            
            foreach (var monitor in monitors)
            {
                minX = Mathf.Min(minX, (int)monitor.MonitorArea.x);
                minY = Mathf.Min(minY, (int)monitor.MonitorArea.y);
                maxX = Mathf.Max(maxX, (int)(monitor.MonitorArea.x + monitor.MonitorArea.width));
                maxY = Mathf.Max(maxY, (int)(monitor.MonitorArea.y + monitor.MonitorArea.height));
            }
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public int GetSystemMetric(int nIndex)
        {
            // Map high-level indices to Windows metric constants
            return nIndex switch
            {
                0 => GetSystemMetrics(SM_XVIRTUALSCREEN),      // X offset
                1 => GetSystemMetrics(SM_YVIRTUALSCREEN),      // Y offset
                2 => GetSystemMetrics(SM_CXVIRTUALSCREEN),     // Width
                3 => GetSystemMetrics(SM_CYVIRTUALSCREEN),     // Height
                _ => GetSystemMetrics(nIndex)                  // Direct pass-through for other constants
            };
        }

        #region P/Invoke Declarations
        
        private const uint MONITOR_DEFAULTTONULL = 0x00000000;
        private const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const uint MONITORINFOF_PRIMARY = 1;
        
        // System metrics constants
        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }
        
        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);
        
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);
        
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
        
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);
        
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);
        
        #endregion
    }
}
