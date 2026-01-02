using System;

namespace MateEngine.Platform
{
    public interface ITransparencyService
    {
        bool IsCompositionEnabled();
        bool EnableTransparency(IntPtr hWnd);
        bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom);
        bool SetWindowTransparency(IntPtr hWnd, byte alpha);
        bool SetLayeredWindowAttributes(IntPtr hWnd, uint colorKey, byte alpha, uint flags);
    }
}
