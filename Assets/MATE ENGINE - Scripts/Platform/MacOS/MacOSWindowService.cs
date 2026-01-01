/**
 * macOS window service implementation
 * Uses native plugin for macOS-specific window management
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    /// <summary>
    /// macOS implementation of window service
    /// </summary>
    public class MacOSWindowService : IWindowService
    {
        private IntPtr _mainWindowHandle = IntPtr.Zero;

        public IntPtr GetMainWindowHandle()
        {
            // On macOS, we'll use the Unity window handle
            // This would typically be obtained through native code
            return _mainWindowHandle;
        }

        public void SetWindowPosition(int x, int y, int width, int height)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement via native plugin
            Debug.LogWarning("[MacOSWindowService] SetWindowPosition not yet implemented");
#endif
        }

        public void SetAlwaysOnTop(bool enabled)
        {
#if UNITY_STANDALONE_OSX
            SetWindowLevel(enabled ? WindowLevel.Floating : WindowLevel.Normal);
#endif
        }

        public void SetWindowLevel(WindowLevel level)
        {
#if UNITY_STANDALONE_OSX
            // Map WindowLevel to NSWindowLevel
            int nsLevel = MapWindowLevel(level);
            NativeSetWindowLevel(nsLevel);
#endif
        }

        public void SetClickThrough(bool enabled)
        {
#if UNITY_STANDALONE_OSX
            NativeSetClickThrough(enabled);
#endif
        }

        public void SetTransparency(float alpha)
        {
#if UNITY_STANDALONE_OSX
            NativeSetWindowAlpha(alpha);
#endif
        }

        public PlatformRect GetWindowRect()
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement via native plugin
            return new PlatformRect();
#else
            return new PlatformRect();
#endif
        }

        public PlatformRect GetClientRect()
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement via native plugin
            return new PlatformRect();
#else
            return new PlatformRect();
#endif
        }

        public void SetVisibleOnAllWorkspaces(bool visible, bool visibleOnFullScreen = false)
        {
#if UNITY_STANDALONE_OSX
            NativeSetCollectionBehavior(visibleOnFullScreen, visible);
#endif
        }

        public void HideFromDock()
        {
#if UNITY_STANDALONE_OSX
            NativeHideDock();
#endif
        }

        public void ShowInDock()
        {
#if UNITY_STANDALONE_OSX
            NativeShowDock();
#endif
        }

        public void MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint)
        {
#if UNITY_STANDALONE_OSX
            SetWindowPosition(x, y, width, height);
#endif
        }

        public bool IsWindowVisible(IntPtr hWnd)
        {
#if UNITY_STANDALONE_OSX
            return true; // TODO: Implement properly
#else
            return false;
#endif
        }

        public void ShowWindow(IntPtr hWnd, int cmdShow)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement via native plugin
#endif
        }

        public PlatformPoint ClientToScreen(IntPtr hWnd, PlatformPoint clientPoint)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement via native plugin
            return clientPoint;
#else
            return clientPoint;
#endif
        }

#if UNITY_STANDALONE_OSX
        private int MapWindowLevel(WindowLevel level)
        {
            // NSWindowLevel values
            // NSNormalWindowLevel = 0
            // NSFloatingWindowLevel = 3
            // NSModalPanelWindowLevel = 8
            // NSMainMenuWindowLevel = 24
            // NSStatusWindowLevel = 25
            // NSPopUpMenuWindowLevel = 101
            // NSScreenSaverWindowLevel = 1000

            switch (level)
            {
                case WindowLevel.Normal:
                    return 0; // NSNormalWindowLevel
                case WindowLevel.Floating:
                    return 3; // NSFloatingWindowLevel
                case WindowLevel.ModalPanel:
                    return 8; // NSModalPanelWindowLevel
                case WindowLevel.MainMenu:
                    return 24; // NSMainMenuWindowLevel
                case WindowLevel.StatusBar:
                    return 25; // NSStatusWindowLevel
                case WindowLevel.PopUpMenu:
                    return 101; // NSPopUpMenuWindowLevel
                case WindowLevel.ScreenSaver:
                    return 1000; // NSScreenSaverWindowLevel
                default:
                    return 0;
            }
        }

        // Native plugin methods (to be implemented in Objective-C)
        [DllImport("NativeWindowManager")]
        private static extern void NativeSetWindowLevel(int level);

        [DllImport("NativeWindowManager")]
        private static extern void NativeSetCollectionBehavior(bool fullScreenAuxiliary, bool canJoinAllSpaces);

        [DllImport("NativeWindowManager")]
        private static extern void NativeSetClickThrough(bool enabled);

        [DllImport("NativeWindowManager")]
        private static extern void NativeSetWindowAlpha(float alpha);

        [DllImport("NativeWindowManager")]
        private static extern void NativeHideDock();

        [DllImport("NativeWindowManager")]
        private static extern void NativeShowDock();
#endif
    }
}
