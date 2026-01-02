using System;

namespace MateEngine.Platform.Windows
{
    public class WindowsTransparencyService : ITransparencyService
    {
        public bool IsCompositionEnabled()
        {
            try
            {
                return Kirurobo.DwmApi.DwmIsCompositionEnabled();
            }
            catch
            {
                return false;
            }
        }

        public bool EnableTransparency(IntPtr hWnd)
        {
            try
            {
                Kirurobo.DwmApi.DwmExtendIntoClientAll(hWnd);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom)
        {
            try
            {
                Kirurobo.DwmApi.MARGINS margins = new Kirurobo.DwmApi.MARGINS(left, top, right, bottom);
                Kirurobo.DwmApi.DwmExtendFrameIntoClientArea(hWnd, ref margins);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetWindowTransparency(IntPtr hWnd, byte alpha)
        {
            // Set the window as layered first
            ulong exStyle = Kirurobo.WinApi.GetWindowLong(hWnd, Kirurobo.WinApi.GWL_EXSTYLE);
            exStyle |= Kirurobo.WinApi.WS_EX_LAYERED;
            Kirurobo.WinApi.SetWindowLong(hWnd, Kirurobo.WinApi.GWL_EXSTYLE, exStyle);
            
            // Set the alpha
            Kirurobo.WinApi.COLORREF colorKey = new Kirurobo.WinApi.COLORREF(0);
            return Kirurobo.WinApi.SetLayeredWindowAttributes(hWnd, colorKey, alpha, Kirurobo.WinApi.LWA_ALPHA);
        }

        public bool SetLayeredWindowAttributes(IntPtr hWnd, uint colorKey, byte alpha, uint flags)
        {
            Kirurobo.WinApi.COLORREF crKey = new Kirurobo.WinApi.COLORREF(colorKey);
            return Kirurobo.WinApi.SetLayeredWindowAttributes(hWnd, crKey, alpha, flags);
        }
    }
}
