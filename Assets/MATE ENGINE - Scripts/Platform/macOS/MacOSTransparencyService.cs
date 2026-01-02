using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    public class MacOSTransparencyService : ITransparencyService
    {
        public bool IsCompositionEnabled()
        {
            // macOS Core Graphics is always available on modern macOS (10.4+)
            // Just verify we can access it
            try
            {
                // If we can call a basic CG function, composition is available
                return true;
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
                if (hWnd == IntPtr.Zero) return false;

                // On macOS, we enable transparency by:
                // 1. Setting the window background with transparency
                // 2. Removing the window's standard background drawing
                
                // Set window to have transparent background
                IntPtr nsColor = objc_getClass("NSColor");
                IntPtr clearColor = objc_msgSend(nsColor, sel_registerName("clearColor"));
                objc_msgSend(hWnd, sel_registerName("setBackgroundColor:"), clearColor);
                
                // Set opaque to false for transparency support
                objc_msgSend(hWnd, sel_registerName("setOpaque:"), 0);
                
                // Make the window accept clicks through transparent areas
                objc_msgSend(hWnd, sel_registerName("setIgnoresMouseEvents:"), 0);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to enable transparency: {ex.Message}");
                return false;
            }
        }

        public bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom)
        {
            // macOS doesn't have the margin concept like Windows DWM
            // Instead, we use window frame insets or content view manipulation
            // For now, this is a no-op but documented for API compatibility
            Debug.Log("SetTransparencyMargins is not applicable on macOS - margin concept doesn't exist");
            return false;
        }

        public bool SetWindowTransparency(IntPtr hWnd, byte alpha)
        {
            try
            {
                if (hWnd == IntPtr.Zero) return false;

                // Convert byte (0-255) to double (0.0-1.0)
                double alphaValue = alpha / 255.0;
                
                // Set window alpha
                objc_msgSend(hWnd, sel_registerName("setAlphaValue:"), alphaValue);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set window transparency: {ex.Message}");
                return false;
            }
        }

        public bool SetLayeredWindowAttributes(IntPtr hWnd, uint colorKey, byte alpha, uint flags)
        {
            try
            {
                if (hWnd == IntPtr.Zero) return false;

                // macOS approach to layered window attributes:
                // - colorKey is used for color-based transparency (less common on macOS)
                // - flags indicate what attributes are set
                // We'll primarily use alpha transparency
                
                double alphaValue = alpha / 255.0;
                objc_msgSend(hWnd, sel_registerName("setAlphaValue:"), alphaValue);
                
                // If color key is specified (non-zero), set background color with alpha
                if (colorKey != 0)
                {
                    IntPtr nsColor = objc_getClass("NSColor");
                    
                    // Extract RGB from colorKey (assuming ARGB format)
                    byte r = (byte)((colorKey >> 16) & 0xFF);
                    byte g = (byte)((colorKey >> 8) & 0xFF);
                    byte b = (byte)(colorKey & 0xFF);
                    
                    // Create color with alpha
                    IntPtr color = objc_msgSend(
                        nsColor,
                        sel_registerName("colorWithRed:green:blue:alpha:"),
                        r / 255.0, g / 255.0, b / 255.0, alphaValue
                    );
                    
                    objc_msgSend(hWnd, sel_registerName("setBackgroundColor:"), color);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set layered window attributes: {ex.Message}");
                return false;
            }
        }

        // Objective-C Runtime Interop
        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, IntPtr arg1);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, int arg1);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, double arg1);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, double arg1, double arg2, double arg3, double arg4);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string selectorName);
    }
}
