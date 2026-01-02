using UnityEngine;

namespace MateEngine.Platform
{
    public interface IPlatformService
    {
        string PlatformName { get; }
        bool IsSupported(PlatformFeature feature);
    }

    public enum PlatformFeature
    {
        WindowManagement,
        SystemTray,
        MultiMonitor,
        WindowTransparency,
        ScreenCapture,
        WindowSnapping,
        MemoryManagement
    }
}
