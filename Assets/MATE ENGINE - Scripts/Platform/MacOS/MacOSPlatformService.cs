/**
 * macOS platform service
 * Main entry point for macOS platform-specific services
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    /// <summary>
    /// macOS platform service implementation
    /// </summary>
    public class MacOSPlatformService : IPlatformService
    {
        private readonly MacOSWindowService _windowService;
        private readonly MacOSScreenService _screenService;
        private readonly MacOSSystemTrayService _systemTrayService;

        public MacOSPlatformService()
        {
            _windowService = new MacOSWindowService();
            _screenService = new MacOSScreenService();
            _systemTrayService = new MacOSSystemTrayService();
        }

        public IWindowService WindowService => _windowService;
        public IScreenService ScreenService => _screenService;
        public ISystemTrayService SystemTrayService => _systemTrayService;
        public RuntimePlatform Platform => RuntimePlatform.OSXPlayer;

#if UNITY_STANDALONE_OSX
        public bool IsSupported => true;
#else
        public bool IsSupported => false;
#endif
    }
}
