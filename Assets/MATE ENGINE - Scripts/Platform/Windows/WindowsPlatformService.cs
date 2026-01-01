/**
 * Windows platform service
 * Main entry point for Windows platform-specific services
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    /// <summary>
    /// Windows platform service implementation
    /// </summary>
    public class WindowsPlatformService : IPlatformService
    {
        private readonly WindowsWindowService _windowService;
        private readonly WindowsScreenService _screenService;
        private readonly WindowsSystemTrayService _systemTrayService;

        public WindowsPlatformService()
        {
            _windowService = new WindowsWindowService();
            _screenService = new WindowsScreenService();
            _systemTrayService = new WindowsSystemTrayService();
        }

        public IWindowService WindowService => _windowService;
        public IScreenService ScreenService => _screenService;
        public ISystemTrayService SystemTrayService => _systemTrayService;
        public RuntimePlatform Platform => RuntimePlatform.WindowsPlayer;

#if UNITY_STANDALONE_WIN
        public bool IsSupported => true;
#else
        public bool IsSupported => false;
#endif
    }
}
