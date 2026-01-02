using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsScreenCaptureService : IScreenCaptureService
    {
        public bool IsSupported => true;

        private IntPtr deskDC;
        private IntPtr memDC;
        private IntPtr dib;
        private IntPtr dibBitsInternal;
        private IntPtr oldObj;
        private int captureWidth;
        private int captureHeight;

        public bool InitializeCapture(int width, int height)
        {
            try
            {
                ReleaseCapture();
                
                captureWidth = width;
                captureHeight = height;

                deskDC = GetDC(IntPtr.Zero);
                if (deskDC == IntPtr.Zero) return false;

                memDC = CreateCompatibleDC(deskDC);
                if (memDC == IntPtr.Zero)
                {
                    ReleaseDC(IntPtr.Zero, deskDC);
                    return false;
                }

                BITMAPINFO bmi = new BITMAPINFO();
                bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bmi.bmiHeader.biWidth = width;
                bmi.bmiHeader.biHeight = -height; // Negative for top-down bitmap
                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = 0; // BI_RGB

                dib = CreateDIBSection(memDC, ref bmi, 0, out dibBitsInternal, IntPtr.Zero, 0);
                if (dib == IntPtr.Zero)
                {
                    DeleteDC(memDC);
                    ReleaseDC(IntPtr.Zero, deskDC);
                    return false;
                }

                oldObj = SelectObject(memDC, dib);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CaptureScreen(int virtX, int virtY, int virtW, int virtH, out IntPtr dibBits)
        {
            dibBits = IntPtr.Zero;
            
            if (memDC == IntPtr.Zero || dib == IntPtr.Zero || dibBitsInternal == IntPtr.Zero)
                return false;

            try
            {
                bool success = StretchBlt(
                    memDC, 0, 0, captureWidth, captureHeight,
                    deskDC, virtX, virtY, virtW, virtH,
                    SRCCOPY
                );

                if (success)
                {
                    dibBits = dibBitsInternal;
                }

                return success;
            }
            catch
            {
                return false;
            }
        }

        public void ReleaseCapture()
        {
            if (memDC != IntPtr.Zero && oldObj != IntPtr.Zero)
            {
                SelectObject(memDC, oldObj);
                oldObj = IntPtr.Zero;
            }

            if (dib != IntPtr.Zero)
            {
                DeleteObject(dib);
                dib = IntPtr.Zero;
            }

            if (memDC != IntPtr.Zero)
            {
                DeleteDC(memDC);
                memDC = IntPtr.Zero;
            }

            if (deskDC != IntPtr.Zero)
            {
                ReleaseDC(IntPtr.Zero, deskDC);
                deskDC = IntPtr.Zero;
            }

            dibBitsInternal = IntPtr.Zero;
        }

        #region P/Invoke Declarations

        private const int SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool StretchBlt(
            IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
            IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, int rop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(
            IntPtr hdc, ref BITMAPINFO pbmi, uint iUsage,
            out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        #endregion
    }
}
