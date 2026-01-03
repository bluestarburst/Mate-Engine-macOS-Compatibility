/**
 * Windows screen service implementation
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    /// <summary>
    /// Windows implementation of screen service
    /// </summary>
    public class WindowsScreenService : IScreenService
    {
        public ScreenInfo[] GetAllScreens()
        {
#if UNITY_STANDALONE_WIN
            List<ScreenInfo> screens = new List<ScreenInfo>();
            
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, 
                (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
                {
                    MONITORINFO mi = new MONITORINFO();
                    mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        Rect bounds = new Rect(
                            mi.rcMonitor.left,
                            mi.rcMonitor.top,
                            mi.rcMonitor.right - mi.rcMonitor.left,
                            mi.rcMonitor.bottom - mi.rcMonitor.top
                        );
                        
                        Rect workArea = new Rect(
                            mi.rcWork.left,
                            mi.rcWork.top,
                            mi.rcWork.right - mi.rcWork.left,
                            mi.rcWork.bottom - mi.rcWork.top
                        );
                        
                        bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                        
                        screens.Add(new ScreenInfo(bounds, workArea, isPrimary, ""));
                    }
                    
                    return true;
                }, IntPtr.Zero);
            
            return screens.ToArray();
#else
            return new ScreenInfo[0];
#endif
        }

        public ScreenInfo GetScreenAtPoint(Vector2 point)
        {
#if UNITY_STANDALONE_WIN
            POINT pt = new POINT { x = (int)point.x, y = (int)point.y };
            IntPtr hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            
            if (hMonitor != IntPtr.Zero)
            {
                MONITORINFO mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    Rect bounds = new Rect(
                        mi.rcMonitor.left,
                        mi.rcMonitor.top,
                        mi.rcMonitor.right - mi.rcMonitor.left,
                        mi.rcMonitor.bottom - mi.rcMonitor.top
                    );
                    
                    Rect workArea = new Rect(
                        mi.rcWork.left,
                        mi.rcWork.top,
                        mi.rcWork.right - mi.rcWork.left,
                        mi.rcWork.bottom - mi.rcWork.top
                    );
                    
                    bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                    
                    return new ScreenInfo(bounds, workArea, isPrimary, "");
                }
            }
#endif
            return new ScreenInfo();
        }

        public ScreenInfo GetScreenContainingWindow(IntPtr windowHandle)
        {
#if UNITY_STANDALONE_WIN
            IntPtr hMonitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
            
            if (hMonitor != IntPtr.Zero)
            {
                MONITORINFO mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    Rect bounds = new Rect(
                        mi.rcMonitor.left,
                        mi.rcMonitor.top,
                        mi.rcMonitor.right - mi.rcMonitor.left,
                        mi.rcMonitor.bottom - mi.rcMonitor.top
                    );
                    
                    Rect workArea = new Rect(
                        mi.rcWork.left,
                        mi.rcWork.top,
                        mi.rcWork.right - mi.rcWork.left,
                        mi.rcWork.bottom - mi.rcWork.top
                    );
                    
                    bool isPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                    
                    return new ScreenInfo(bounds, workArea, isPrimary, "");
                }
            }
#endif
            return new ScreenInfo();
        }

        public Rect GetVirtualScreenBounds()
        {
#if UNITY_STANDALONE_WIN
            int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);
            
            return new Rect(left, top, width, height);
#else
            return new Rect(0, 0, Screen.width, Screen.height);
#endif
        }

        public Vector2 GetCursorPosition()
        {
#if UNITY_STANDALONE_WIN
            POINT pt;
            if (GetCursorPos(out pt))
            {
                return new Vector2(pt.x, pt.y);
            }
#endif
            return Input.mousePosition;
        }

        public void SetCursorPosition(int x, int y)
        {
#if UNITY_STANDALONE_WIN
            SetCursorPos(x, y);
#endif
        }

        public bool IsCursorOverWindow(IntPtr windowHandle)
        {
#if UNITY_STANDALONE_WIN
            POINT pt;
            if (GetCursorPos(out pt))
            {
                IntPtr hwnd = WindowFromPoint(pt);
                return hwnd == windowHandle;
            }
#endif
            return false;
        }

        public int GetSystemMetrics(int index)
        {
#if UNITY_STANDALONE_WIN
            return GetSystemMetrics(index);
#else
            return 0;
#endif
        }

#if UNITY_STANDALONE_WIN
        // System metrics constants
        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;
        
        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const uint MONITORINFOF_PRIMARY = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x, y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
#endif
    }
}
