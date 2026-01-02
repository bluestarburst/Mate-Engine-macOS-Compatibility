/**
 * Windows window service implementation
 * Wraps existing Windows API calls from WinApi.cs
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    /// <summary>
    /// Windows implementation of window service
    /// </summary>
    public class WindowsWindowService : IWindowService
    {
        private IntPtr _mainWindowHandle = IntPtr.Zero;

        public WindowsWindowService()
        {
#if UNITY_STANDALONE_WIN
            _mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
#endif
        }

        public IntPtr GetMainWindowHandle()
        {
#if UNITY_STANDALONE_WIN
            if (_mainWindowHandle == IntPtr.Zero)
            {
                _mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            return _mainWindowHandle;
#else
            return IntPtr.Zero;
#endif
        }

        public void SetWindowPosition(int x, int y, int width, int height)
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                MoveWindowNative(hwnd, x, y, width, height, true);
            }
#endif
        }

        public void SetAlwaysOnTop(bool enabled)
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                IntPtr hWndInsertAfter = enabled ? HWND_TOPMOST : HWND_NOTOPMOST;
                SetWindowPos(hwnd, hWndInsertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
#endif
        }

        public void SetWindowLevel(WindowLevel level)
        {
            // Windows doesn't have the same concept as macOS window levels
            // Just use always on top for elevated levels
            SetAlwaysOnTop(level > WindowLevel.Normal);
        }

        public void SetClickThrough(bool enabled)
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                if (enabled)
                {
                    exStyle |= (int)WS_EX_TRANSPARENT;
                }
                else
                {
                    exStyle &= ~(int)WS_EX_TRANSPARENT;
                }
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
            }
#endif
        }

        public void SetTransparency(float alpha)
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                exStyle |= (int)WS_EX_LAYERED;
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
                
                byte alphaValue = (byte)(Mathf.Clamp01(alpha) * 255);
                SetLayeredWindowAttributes(hwnd, 0, alphaValue, LWA_ALPHA);
            }
#endif
        }

        public PlatformRect GetWindowRect()
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                RECT rect;
                if (GetWindowRect(hwnd, out rect))
                {
                    return new PlatformRect(rect.left, rect.top, rect.right, rect.bottom);
                }
            }
#endif
            return new PlatformRect();
        }

        public PlatformRect GetClientRect()
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                RECT rect;
                if (GetClientRect(hwnd, out rect))
                {
                    return new PlatformRect(rect.left, rect.top, rect.right, rect.bottom);
                }
            }
#endif
            return new PlatformRect();
        }

        public void SetVisibleOnAllWorkspaces(bool visible, bool visibleOnFullScreen = false)
        {
            // Windows doesn't have virtual desktops in the same way as macOS
            // This is a no-op on Windows
        }

        public void HideFromDock()
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW);
            }
#endif
        }

        public void ShowInDock()
        {
#if UNITY_STANDALONE_WIN
            IntPtr hwnd = GetMainWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_TOOLWINDOW);
            }
#endif
        }

        public void MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint)
        {
#if UNITY_STANDALONE_WIN
            MoveWindowNative(hWnd, x, y, width, height, repaint);
#endif
        }

        public bool IsWindowVisible(IntPtr hWnd)
        {
#if UNITY_STANDALONE_WIN
            return IsWindowVisibleNative(hWnd);
#else
            return false;
#endif
        }

        public void ShowWindow(IntPtr hWnd, int cmdShow)
        {
#if UNITY_STANDALONE_WIN
            ShowWindowNative(hWnd, cmdShow);
#endif
        }

        public PlatformPoint ClientToScreen(IntPtr hWnd, PlatformPoint clientPoint)
        {
#if UNITY_STANDALONE_WIN
            POINT pt = new POINT { x = clientPoint.x, y = clientPoint.y };
            if (ClientToScreen(hWnd, ref pt))
            {
                return new PlatformPoint(pt.x, pt.y);
            }
#endif
            return clientPoint;
        }

#if UNITY_STANDALONE_WIN
        // Windows API constants
        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_TRANSPARENT = 0x00000020;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const uint LWA_ALPHA = 0x00000002;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

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

        // Windows API imports
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        private static extern bool MoveWindowNative(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        private static extern bool IsWindowVisibleNative(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern bool ShowWindowNative(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
#endif
    }
}
