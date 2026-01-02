using System;
using UnityEngine;

namespace MateEngine.Platform
{
    public interface IScreenCaptureService
    {
        bool IsSupported { get; }
        bool InitializeCapture(int width, int height);
        bool CaptureScreen(int virtX, int virtY, int virtW, int virtH, out IntPtr dibBits);
        void ReleaseCapture();
    }
}
