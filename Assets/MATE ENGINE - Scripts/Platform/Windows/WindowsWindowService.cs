using System;
using System.Text;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsWindowService : IWindowService
    {
        public IntPtr GetMainWindowHandle()
        {
            return Kirurobo.WindowController.GetUnityWindowHandle();
        }

        public IntPtr GetActiveWindow()
        {
            return GetActiveWindowImpl();
        }

        public IntPtr GetForegroundWindow()
        {
            return GetForegroundWindowImpl();
        }

        public bool GetWindowRect(IntPtr hWnd, out WindowRect rect)
        {
            Kirurobo.WinApi.RECT winRect;
            bool result = Kirurobo.WinApi.GetWindowRect(hWnd, out winRect);
            rect = new WindowRect
            {
                Left = winRect.left,
                Top = winRect.top,
                Right = winRect.right,
                Bottom = winRect.bottom
            };
            return result;
        }

        public bool GetClientRect(IntPtr hWnd, out WindowRect rect)
        {
            Kirurobo.WinApi.RECT winRect;
            bool result = Kirurobo.WinApi.GetClientRect(hWnd, out winRect);
            rect = new WindowRect
            {
                Left = winRect.left,
                Top = winRect.top,
                Right = winRect.right,
                Bottom = winRect.bottom
            };
            return result;
        }

        public bool SetWindowPosition(IntPtr hWnd, int x, int y, int width, int height, SetWindowFlags flags)
        {
            return Kirurobo.WinApi.SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, (uint)flags);
        }

        public bool ShowWindow(IntPtr hWnd, ShowWindowCommand cmd)
        {
            return Kirurobo.WinApi.ShowWindow(hWnd, (int)cmd);
        }

        public bool IsWindowVisible(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsWindowVisible(hWnd);
        }

        public bool IsWindowMinimized(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsIconic(hWnd);
        }

        public bool IsWindowMaximized(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsZoomed(hWnd);
        }

        public bool IsWindow(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsWindow(hWnd);
        }

        public bool SetTopMost(IntPtr hWnd, bool topmost)
        {
            IntPtr hWndInsertAfter = topmost ? Kirurobo.WinApi.HWND_TOPMOST : Kirurobo.WinApi.HWND_NOTOPMOST;
            return Kirurobo.WinApi.SetWindowPos(hWnd, hWndInsertAfter, 0, 0, 0, 0, 
                Kirurobo.WinApi.SWP_NOMOVE | Kirurobo.WinApi.SWP_NOSIZE | Kirurobo.WinApi.SWP_NOACTIVATE);
        }

        public bool SetWindowStyle(IntPtr hWnd, WindowStyle style)
        {
            ulong result = Kirurobo.WinApi.SetWindowLong(hWnd, Kirurobo.WinApi.GWL_STYLE, (ulong)style);
            return result != 0;
        }

        public ulong GetWindowStyle(IntPtr hWnd)
        {
            return Kirurobo.WinApi.GetWindowLong(hWnd, Kirurobo.WinApi.GWL_STYLE);
        }

        public ulong GetWindowLong(IntPtr hWnd, int nIndex)
        {
            return Kirurobo.WinApi.GetWindowLong(hWnd, nIndex);
        }

        public bool SetWindowLong(IntPtr hWnd, int nIndex, ulong value)
        {
            ulong result = Kirurobo.WinApi.SetWindowLong(hWnd, nIndex, value);
            return result != 0;
        }

        public void EnumerateWindows(WindowEnumCallback callback)
        {
            Kirurobo.WinApi.EnumWindows((hWnd, lParam) =>
            {
                return callback(hWnd);
            }, IntPtr.Zero);
        }

        public string GetWindowText(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Kirurobo.WinApi.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Kirurobo.WinApi.GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public uint GetWindowProcessId(IntPtr hWnd)
        {
            ulong processId;
            Kirurobo.WinApi.GetWindowThreadProcessId(hWnd, out processId);
            return (uint)processId;
        }

        public int GetWindowTextLength(IntPtr hWnd)
        {
            return GetWindowText(hWnd).Length;
        }

        public IntPtr GetParent(IntPtr hWnd)
        {
            return Kirurobo.WinApi.GetParent(hWnd);
        }

        public IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags)
        {
            return Kirurobo.WinApi.GetAncestor(hWnd, (uint)flags);
        }

        public IntPtr GetWindow(IntPtr hWnd, GetWindowCommand cmd)
        {
            // Use GetAncestor for GW_OWNER
            if (cmd == GetWindowCommand.GW_OWNER)
            {
                return Kirurobo.WinApi.GetAncestor(hWnd, Kirurobo.WinApi.GW_OWNER);
            }
            
            // For other commands, we need to use EnumWindows or similar
            // This is a simplified implementation
            return IntPtr.Zero;
        }

        public bool IsAboveInZOrder(IntPtr hWnd1, IntPtr hWnd2)
        {
            // Walk through windows in Z-order to determine which is above
            IntPtr current = Kirurobo.WinApi.GetAncestor(hWnd1, Kirurobo.WinApi.GW_HWNDFIRST);
            
            while (current != IntPtr.Zero)
            {
                if (current == hWnd1) return true;
                if (current == hWnd2) return false;
                current = Kirurobo.WinApi.GetAncestor(current, Kirurobo.WinApi.GW_HWNDNEXT);
            }
            
            return false;
        }

        public bool GetLayeredWindowAttributes(IntPtr hWnd, out uint colorKey, out byte alpha, out uint flags)
        {
            return NativeGetLayeredWindowAttributes(hWnd, out colorKey, out alpha, out flags);
        }

        public bool GetWindowCloakingState(IntPtr hWnd, out bool isCloaked)
        {
            isCloaked = false;
            int cloaked = 0;
            int result = NativeDwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out cloaked, sizeof(int));
            if (result == 0) // S_OK
            {
                isCloaked = (cloaked != 0);
                return true;
            }
            return false;
        }

        public uint GetCurrentProcessId()
        {
            return NativeGetCurrentProcessId();
        }

        #region P/Invoke Declarations

        private const int DWMWA_CLOAKED = 14;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindowImpl();
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindowImpl();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool NativeGetLayeredWindowAttributes(IntPtr hwnd, out uint pcrKey, out byte pbAlpha, out uint pdwFlags);
        
        [System.Runtime.InteropServices.DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int NativeDwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);
        
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern uint NativeGetCurrentProcessId();

        #endregion
    }
}
