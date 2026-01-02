using System;

namespace MateEngine.Platform.Stub
{
    public class StubTransparencyService : ITransparencyService
    {
        public bool IsCompositionEnabled() => false;
        public bool EnableTransparency(IntPtr hWnd) => false;
        public bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom) => false;
        public bool SetWindowTransparency(IntPtr hWnd, byte alpha) => false;
        public bool SetLayeredWindowAttributes(IntPtr hWnd, uint colorKey, byte alpha, uint flags) => false;
    }
}
