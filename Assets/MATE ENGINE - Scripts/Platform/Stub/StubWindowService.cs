using System;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubWindowService : IWindowService
    {
        public IntPtr GetMainWindowHandle() => IntPtr.Zero;
        
        public bool GetWindowRect(IntPtr hWnd, out WindowRect rect)
        {
            rect = default;
            return false;
        }
        
        public bool GetClientRect(IntPtr hWnd, out WindowRect rect)
        {
            rect = default;
            return false;
        }
        
        public bool SetWindowPosition(IntPtr hWnd, int x, int y, int width, int height, SetWindowFlags flags) => false;
        public bool ShowWindow(IntPtr hWnd, ShowWindowCommand cmd) => false;
        public bool IsWindowVisible(IntPtr hWnd) => false;
        public bool IsWindowMinimized(IntPtr hWnd) => false;
        public bool IsWindowMaximized(IntPtr hWnd) => false;
        public bool IsWindow(IntPtr hWnd) => false;
        public bool SetTopMost(IntPtr hWnd, bool topmost) => false;
        public bool SetWindowStyle(IntPtr hWnd, WindowStyle style) => false;
        public ulong GetWindowStyle(IntPtr hWnd) => 0;
        public ulong GetWindowLong(IntPtr hWnd, int nIndex) => 0;
        public bool SetWindowLong(IntPtr hWnd, int nIndex, ulong value) => false;
        public void EnumerateWindows(WindowEnumCallback callback) { }
        public string GetWindowText(IntPtr hWnd) => string.Empty;
        public string GetWindowClassName(IntPtr hWnd) => string.Empty;
        public uint GetWindowProcessId(IntPtr hWnd) => 0;
        public int GetWindowTextLength(IntPtr hWnd) => 0;
        public IntPtr GetParent(IntPtr hWnd) => IntPtr.Zero;
        public IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags) => IntPtr.Zero;
        public IntPtr GetWindow(IntPtr hWnd, GetWindowCommand cmd) => IntPtr.Zero;
        public bool IsAboveInZOrder(IntPtr hWnd1, IntPtr hWnd2) => false;
    }
}
