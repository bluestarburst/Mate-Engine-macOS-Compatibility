/**
 * Main platform service interface
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Main platform service that provides access to all platform-specific services
    /// </summary>
    public interface IPlatformService
    {
        /// <summary>
        /// Get the window service
        /// </summary>
        IWindowService WindowService { get; }

        /// <summary>
        /// Get the screen service
        /// </summary>
        IScreenService ScreenService { get; }

        /// <summary>
        /// Get the system tray service
        /// </summary>
        ISystemTrayService SystemTrayService { get; }

        /// <summary>
        /// Get the current platform
        /// </summary>
        RuntimePlatform Platform { get; }

        /// <summary>
        /// Check if this platform is supported
        /// </summary>
        bool IsSupported { get; }
    }
}
