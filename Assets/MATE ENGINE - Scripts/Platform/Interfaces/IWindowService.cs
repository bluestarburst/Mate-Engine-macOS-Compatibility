using System;
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Cross-platform window management
    /// </summary>
    public interface IWindowService
    {
        // Window Handle
        IntPtr GetMainWindowHandle();
        
        // Position and Size
        bool GetWindowRect(IntPtr hWnd, out WindowRect rect);
        bool GetClientRect(IntPtr hWnd, out WindowRect rect);
        bool SetWindowPosition(IntPtr hWnd, int x, int y, int width, int height, SetWindowFlags flags);
        
        // Window State
        bool ShowWindow(IntPtr hWnd, ShowWindowCommand cmd);
        bool IsWindowVisible(IntPtr hWnd);
        bool IsWindowMinimized(IntPtr hWnd);
        bool IsWindowMaximized(IntPtr hWnd);
        bool IsWindow(IntPtr hWnd);
        
        // Window Properties
        bool SetTopMost(IntPtr hWnd, bool topmost);
        bool SetWindowStyle(IntPtr hWnd, WindowStyle style);
        ulong GetWindowStyle(IntPtr hWnd);
        ulong GetWindowLong(IntPtr hWnd, int nIndex);
        bool SetWindowLong(IntPtr hWnd, int nIndex, ulong value);
        
        // Window Enumeration
        void EnumerateWindows(WindowEnumCallback callback);
        
        // Window Information
        string GetWindowText(IntPtr hWnd);
        string GetWindowClassName(IntPtr hWnd);
        uint GetWindowProcessId(IntPtr hWnd);
        int GetWindowTextLength(IntPtr hWnd);
        
        // Parent/Child Relationships
        IntPtr GetParent(IntPtr hWnd);
        IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags);
        IntPtr GetWindow(IntPtr hWnd, GetWindowCommand cmd);
        
        // Z-Order
        bool IsAboveInZOrder(IntPtr hWnd1, IntPtr hWnd2);
        
        // Window Layering & Transparency
        bool GetLayeredWindowAttributes(IntPtr hWnd, out uint colorKey, out byte alpha, out uint flags);
        
        // DWM Attributes (Windows 7+)
        bool GetWindowCloakingState(IntPtr hWnd, out bool isCloaked);
        
        // Process Information
        uint GetCurrentProcessId();
    }

    // Supporting types
    public struct WindowRect
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        
        public WindowRect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    public enum ShowWindowCommand
    {
        Hide = 0,
        Normal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActive = 7,
        ShowNA = 8,
        Restore = 9
    }

    [Flags]
    public enum SetWindowFlags : uint
    {
        NoSize = 0x0001,
        NoMove = 0x0002,
        NoZOrder = 0x0004,
        NoActivate = 0x0010,
        ShowWindow = 0x0040,
        FrameChanged = 0x0020,
        NoOwnerZOrder = 0x0200,
        NoSendChanging = 0x0400,
        NoCopyBits = 0x0100,
        AsyncWindowPos = 0x4000
    }

    [Flags]
    public enum WindowStyle : ulong
    {
        Border = 0x00800000L,
        Caption = 0x00C00000L,
        SysMenu = 0x00080000L,
        ThickFrame = 0x00040000L,
        Popup = 0x80000000L,
        Overlapped = 0x00000000L,
        Visible = 0x10000000L,
        Iconic = 0x20000000L,
        Minimize = 0x20000000L,
        Maximize = 0x01000000L
    }

    public enum GetAncestorFlags : uint
    {
        Parent = 1,
        Root = 2,
        RootOwner = 3
    }

    public enum GetWindowCommand : uint
    {
        HwndFirst = 0,
        HwndLast = 1,
        HwndNext = 2,
        HwndPrev = 3,
        Owner = 4,
        Child = 5
    }

    public delegate bool WindowEnumCallback(IntPtr hWnd);
}
