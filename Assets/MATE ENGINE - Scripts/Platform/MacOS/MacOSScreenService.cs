/**
 * macOS screen service implementation
 * 
 * License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
 */
using System;
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    /// <summary>
    /// macOS implementation of screen service
    /// </summary>
    public class MacOSScreenService : IScreenService
    {
        public ScreenInfo[] GetAllScreens()
        {
#if UNITY_STANDALONE_OSX
            // Use Unity's Display system as fallback
            // TODO: Implement native screen enumeration
            ScreenInfo[] screens = new ScreenInfo[Display.displays.Length];
            for (int i = 0; i < Display.displays.Length; i++)
            {
                screens[i] = new ScreenInfo(
                    new Rect(0, 0, Display.displays[i].systemWidth, Display.displays[i].systemHeight),
                    new Rect(0, 0, Display.displays[i].systemWidth, Display.displays[i].systemHeight),
                    i == 0,
                    $"Display {i}"
                );
            }
            return screens;
#else
            return new ScreenInfo[0];
#endif
        }

        public ScreenInfo GetScreenAtPoint(Vector2 point)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement native screen detection
            // For now, return primary screen
            return new ScreenInfo(
                new Rect(0, 0, Screen.width, Screen.height),
                new Rect(0, 0, Screen.width, Screen.height),
                true,
                "Primary Display"
            );
#else
            return new ScreenInfo();
#endif
        }

        public ScreenInfo GetScreenContainingWindow(IntPtr windowHandle)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement native screen detection for window
            return GetScreenAtPoint(Vector2.zero);
#else
            return new ScreenInfo();
#endif
        }

        public Rect GetVirtualScreenBounds()
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement native virtual screen bounds
            return new Rect(0, 0, Screen.width, Screen.height);
#else
            return new Rect(0, 0, Screen.width, Screen.height);
#endif
        }

        public Vector2 GetCursorPosition()
        {
            // Unity's Input.mousePosition works across platforms
            return Input.mousePosition;
        }

        public void SetCursorPosition(int x, int y)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement native cursor positioning
            Debug.LogWarning("[MacOSScreenService] SetCursorPosition not yet implemented");
#endif
        }

        public bool IsCursorOverWindow(IntPtr windowHandle)
        {
#if UNITY_STANDALONE_OSX
            // TODO: Implement native cursor detection
            return false;
#else
            return false;
#endif
        }

        public int GetSystemMetrics(int index)
        {
#if UNITY_STANDALONE_OSX
            // System metrics don't directly translate to macOS
            return 0;
#else
            return 0;
#endif
        }
    }
}
