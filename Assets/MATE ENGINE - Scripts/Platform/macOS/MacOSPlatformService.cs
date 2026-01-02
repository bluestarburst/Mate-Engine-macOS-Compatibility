using System;
using UnityEngine;
using MateEngine.Platform.Stub;

namespace MateEngine.Platform.MacOS
{
    public class MacOSPlatformService : IPlatformService
    {
        private StubScreenCaptureService _screenCaptureService;

        public string PlatformName => "macOS";

        public IScreenCaptureService ScreenCaptureService
        {
            get { return _screenCaptureService ??= new StubScreenCaptureService(); }
        }

        public bool IsSupported(PlatformFeature feature)
        {
            // Indicate what features are supported on macOS
            switch (feature)
            {
                case PlatformFeature.WindowManagement:
                    return true; // Fully supported via UniWindowController
                case PlatformFeature.SystemTray:
                    return true; // Fully supported
                case PlatformFeature.MultiMonitor:
                    return true; // Fully supported
                case PlatformFeature.WindowTransparency:
                    return true; // Fully supported via UniWindowController
                case PlatformFeature.ScreenCapture:
                    return false; // Not yet implemented
                case PlatformFeature.WindowSnapping:
                    return false; // Not applicable on macOS
                case PlatformFeature.MemoryManagement:
                    return true; // Supported
                default:
                    return false;
            }
        }

        public bool Initialize()
        {
            try
            {
                Debug.Log("Initializing macOS Platform Service");
                
                // Verify we can access basic macOS APIs
                var screenService = PlatformServiceLocator.ScreenService;
                var systemTrayService = PlatformServiceLocator.SystemTrayService;
                var windowService = PlatformServiceLocator.WindowService;
                var transparencyService = PlatformServiceLocator.TransparencyService;
                
                Debug.Log("macOS Platform Service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize macOS Platform Service: {ex.Message}");
                return false;
            }
        }
    }
}
