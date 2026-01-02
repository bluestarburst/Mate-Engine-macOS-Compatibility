using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Provides access to platform-specific services
    /// </summary>
    public static class PlatformServiceLocator
    {
        private static IWindowService _windowService;
        private static IScreenService _screenService;
        private static ITransparencyService _transparencyService;
        private static ISystemTrayService _systemTrayService;
        private static IScreenCaptureService _screenCaptureService;
        private static IPlatformService _platformService;

        public static IWindowService WindowService => _windowService ??= CreateWindowService();
        public static IScreenService ScreenService => _screenService ??= CreateScreenService();
        public static ITransparencyService TransparencyService => _transparencyService ??= CreateTransparencyService();
        public static ISystemTrayService SystemTrayService => _systemTrayService ??= CreateSystemTrayService();
        public static IScreenCaptureService ScreenCaptureService => _screenCaptureService ??= CreateScreenCaptureService();
        public static IPlatformService PlatformService => _platformService ??= CreatePlatformService();

        private static IWindowService CreateWindowService()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new Windows.WindowsWindowService();
#else
            return new Stub.StubWindowService();
#endif
        }

        private static IScreenService CreateScreenService()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new Windows.WindowsScreenService();
#else
            return new Stub.StubScreenService();
#endif
        }

        private static ITransparencyService CreateTransparencyService()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new Windows.WindowsTransparencyService();
#else
            return new Stub.StubTransparencyService();
#endif
        }

        private static ISystemTrayService CreateSystemTrayService()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new Windows.WindowsSystemTrayService();
#else
            return new Stub.StubSystemTrayService();
#endif
        }

        private static IScreenCaptureService CreateScreenCaptureService()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new Windows.WindowsScreenCaptureService();
#else
            return new Stub.StubScreenCaptureService();
#endif
        }

        private static IPlatformService CreatePlatformService()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new Windows.WindowsPlatformService();
#else
            return new Stub.StubPlatformService();
#endif
        }

        /// <summary>
        /// Reset all services (for testing)
        /// </summary>
        public static void ResetServices()
        {
            _windowService = null;
            _screenService = null;
            _transparencyService = null;
            _systemTrayService = null;
            _screenCaptureService = null;
            _platformService = null;
        }
    }
}
