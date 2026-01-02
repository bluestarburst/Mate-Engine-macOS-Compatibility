using System;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubScreenCaptureService : IScreenCaptureService
    {
        public bool IsSupported => false;
        public bool InitializeCapture(int width, int height) => false;
        public bool CaptureScreen(int virtX, int virtY, int virtW, int virtH, out IntPtr dibBits)
        {
            dibBits = IntPtr.Zero;
            return false;
        }
        public void ReleaseCapture() { }
    }
}
