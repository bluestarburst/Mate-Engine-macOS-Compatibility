#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

public class MoveToPrimaryScreen : MonoBehaviour
{
    private IntPtr unityHWND = IntPtr.Zero;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTOPRIMARY = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    void Start()
    {
        unityHWND = Process.GetCurrentProcess().MainWindowHandle;
    }

    public void MoveToPrimary()
    {
        if (unityHWND == IntPtr.Zero) return;

        if (!GetWindowRect(unityHWND, out RECT rect)) return;

        int currentWidth = rect.Right - rect.Left;
        int currentHeight = rect.Bottom - rect.Top;

        var monitor = MonitorFromWindow(unityHWND, MONITOR_DEFAULTTOPRIMARY);
        var info = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
        if (!GetMonitorInfo(monitor, ref info))
        {
            Debug.LogWarning("[MoveToPrimaryScreen] GetMonitorInfo failed; falling back to current window position");
            return;
        }

        int x = info.rcMonitor.Left + (info.rcMonitor.Right - info.rcMonitor.Left - currentWidth) / 2;
        int y = info.rcMonitor.Top + (info.rcMonitor.Bottom - info.rcMonitor.Top - currentHeight) / 2;

        MoveWindow(unityHWND, x, y, currentWidth, currentHeight, true);

        Debug.Log($"[MoveToPrimaryScreen] moved window {currentWidth}x{currentHeight} to {x},{y}");
    }
}
#endif
