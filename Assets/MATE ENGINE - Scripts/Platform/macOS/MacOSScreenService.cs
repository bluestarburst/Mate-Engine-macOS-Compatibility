using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    public class MacOSScreenService : IScreenService
    {
        public bool GetCursorPosition(out Vector2Int position)
        {
            try
            {
                // Get the global mouse location using NSEvent
                IntPtr nsEvent = objc_getClass("NSEvent");
                IntPtr mouseLocation = objc_msgSend(nsEvent, sel_registerName("mouseLocation"));
                
                // Extract x and y from NSPoint structure
                // NSPoint is: {double x; double y;}
                double x = Marshal.ReadInt64(mouseLocation);
                double y = Marshal.ReadInt64(mouseLocation + 8);
                
                position = new Vector2Int((int)x, (int)y);
                return true;
            }
            catch
            {
                position = Vector2Int.zero;
                return false;
            }
        }

        public bool SetCursorPosition(int x, int y)
        {
            try
            {
                // Use CGWarpMouseCursorPosition to move the cursor
                CGPoint point = new CGPoint { x = x, y = y };
                CGWarpMouseCursorPosition(point);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public MonitorInfo GetPrimaryMonitor()
        {
            try
            {
                // Get main screen (primary monitor)
                IntPtr nsScreen = objc_getClass("NSScreen");
                IntPtr mainScreen = objc_msgSend(nsScreen, sel_registerName("mainScreen"));
                
                return ExtractMonitorInfo(mainScreen, true);
            }
            catch
            {
                return default;
            }
        }

        public MonitorInfo GetMonitorFromPoint(Vector2Int point)
        {
            try
            {
                IntPtr nsScreen = objc_getClass("NSScreen");
                CGPoint cgPoint = new CGPoint { x = point.x, y = point.y };
                
                // There's no direct "screenContainingPoint" in public API
                // We'll check all screens and find the one containing the point
                MonitorInfo[] allMonitors = GetAllMonitors();
                foreach (var monitor in allMonitors)
                {
                    if (point.x >= monitor.Left && point.x < monitor.Right &&
                        point.y >= monitor.Top && point.y < monitor.Bottom)
                    {
                        return monitor;
                    }
                }
                
                // Fallback to primary monitor
                return GetPrimaryMonitor();
            }
            catch
            {
                return GetPrimaryMonitor();
            }
        }

        public MonitorInfo GetMonitorFromWindow(IntPtr hWnd)
        {
            try
            {
                // For macOS, hWnd is an NSWindow pointer
                // Get the window's frame and find which screen it's on
                if (hWnd == IntPtr.Zero)
                {
                    return GetPrimaryMonitor();
                }

                // Get window frame
                IntPtr frameObj = objc_msgSend(hWnd, sel_registerName("frame"));
                
                // Extract origin from frame (CGRect)
                double x = 0, y = 0;
                Marshal.Copy(frameObj, new byte[Marshal.SizeOf(typeof(CGRect))], 0, Marshal.SizeOf(typeof(CGRect)));
                
                // Get the center point of the window and use that to find monitor
                Vector2Int windowCenter = new Vector2Int((int)x + 100, (int)y + 100); // Approximate center
                return GetMonitorFromPoint(windowCenter);
            }
            catch
            {
                return GetPrimaryMonitor();
            }
        }

        public MonitorInfo[] GetAllMonitors()
        {
            try
            {
                IntPtr nsScreen = objc_getClass("NSScreen");
                IntPtr screensArray = objc_msgSend(nsScreen, sel_registerName("screens"));
                
                uint count = 0;
                IntPtr countObj = objc_msgSend(screensArray, sel_registerName("count"));
                count = (uint)Marshal.PtrToStructure(countObj, typeof(uint));
                
                MonitorInfo[] monitors = new MonitorInfo[count];
                
                for (int i = 0; i < count; i++)
                {
                    IntPtr screen = objc_msgSend(screensArray, sel_registerName("objectAtIndex:"), (uint)i);
                    monitors[i] = ExtractMonitorInfo(screen, i == 0);
                }
                
                return monitors;
            }
            catch
            {
                // Fallback: return primary monitor only
                return new[] { GetPrimaryMonitor() };
            }
        }

        public Rect GetVirtualDesktopBounds()
        {
            try
            {
                MonitorInfo[] monitors = GetAllMonitors();
                if (monitors.Length == 0)
                {
                    return Rect.zero;
                }

                float minX = monitors[0].Left;
                float minY = monitors[0].Top;
                float maxX = monitors[0].Right;
                float maxY = monitors[0].Bottom;

                for (int i = 1; i < monitors.Length; i++)
                {
                    minX = Mathf.Min(minX, monitors[i].Left);
                    minY = Mathf.Min(minY, monitors[i].Top);
                    maxX = Mathf.Max(maxX, monitors[i].Right);
                    maxY = Mathf.Max(maxY, monitors[i].Bottom);
                }

                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
            catch
            {
                return Rect.zero;
            }
        }

        public int GetSystemMetric(int nIndex)
        {
            // macOS doesn't have direct system metrics like Windows
            // Return reasonable defaults based on common metrics
            try
            {
                // Some common metric indices (from Windows API):
                // SM_CXSCREEN = 0, SM_CYSCREEN = 1
                // SM_CXVIRTUALSCREEN = 78, SM_CYVIRTUALSCREEN = 79
                // etc.
                
                Rect virtualBounds = GetVirtualDesktopBounds();
                
                switch (nIndex)
                {
                    case 0: // SM_CXSCREEN - primary screen width
                        {
                            MonitorInfo primary = GetPrimaryMonitor();
                            return primary.Right - primary.Left;
                        }
                    case 1: // SM_CYSCREEN - primary screen height
                        {
                            MonitorInfo primary = GetPrimaryMonitor();
                            return primary.Bottom - primary.Top;
                        }
                    case 78: // SM_CXVIRTUALSCREEN - virtual screen width
                        return (int)virtualBounds.width;
                    case 79: // SM_CYVIRTUALSCREEN - virtual screen height
                        return (int)virtualBounds.height;
                    default:
                        return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private MonitorInfo ExtractMonitorInfo(IntPtr screen, bool isPrimary)
        {
            try
            {
                // Get the visible frame (excludes dock and menu bar)
                IntPtr visibleFrameObj = objc_msgSend(screen, sel_registerName("visibleFrame"));
                CGRect visibleFrame = Marshal.PtrToStructure<CGRect>(visibleFrameObj);
                
                // Get the full frame
                IntPtr frameObj = objc_msgSend(screen, sel_registerName("frame"));
                CGRect frame = Marshal.PtrToStructure<CGRect>(frameObj);
                
                // Get device name
                IntPtr deviceNameObj = objc_msgSend(screen, sel_registerName("localizedName"));
                string deviceName = Marshal.PtrToStringAnsi(objc_msgSend(deviceNameObj, sel_registerName("UTF8String"))) ?? "Display";
                
                return new MonitorInfo
                {
                    WorkArea = new Rect((float)visibleFrame.origin.x, (float)visibleFrame.origin.y, 
                                       (float)visibleFrame.size.width, (float)visibleFrame.size.height),
                    MonitorArea = new Rect((float)frame.origin.x, (float)frame.origin.y, 
                                          (float)frame.size.width, (float)frame.size.height),
                    IsPrimary = isPrimary,
                    DeviceName = deviceName
                };
            }
            catch
            {
                return default;
            }
        }

        // Objective-C Runtime Interop
        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, uint arg1);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string selectorName);

        // Core Graphics
        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGWarpMouseCursorPosition(CGPoint point);

        // Structure definitions
        [StructLayout(LayoutKind.Sequential)]
        private struct CGPoint
        {
            public double x;
            public double y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CGSize
        {
            public double width;
            public double height;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CGRect
        {
            public CGPoint origin;
            public CGSize size;
        }
    }
}
