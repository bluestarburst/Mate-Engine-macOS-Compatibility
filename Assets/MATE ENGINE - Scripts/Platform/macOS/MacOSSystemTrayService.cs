using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.MacOS
{
    public class MacOSSystemTrayService : ISystemTrayService
    {
        public bool IsSupported => true;

        private IntPtr statusItem = IntPtr.Zero;
        private IntPtr statusBar = IntPtr.Zero;
        private IntPtr menu = IntPtr.Zero;
        private Action onLeftClick;
        private Func<List<(string, Action)>> menuBuilder;

        public event Action OnLeftClick
        {
            add { onLeftClick += value; }
            remove { onLeftClick -= value; }
        }

        public event Action OnRightClick
        {
            add { /* macOS status bar menu appears on any click */ }
            remove { /* Not applicable */ }
        }

        public bool Initialize(string appName, string tooltip, Texture2D icon, Func<List<(string, Action)>> buildMenu)
        {
            try
            {
                menuBuilder = buildMenu;

                // Get the shared status bar
                statusBar = objc_getClass("NSStatusBar");
                if (statusBar == IntPtr.Zero) return false;

                IntPtr statusBarInstance = objc_msgSend(statusBar, sel_registerName("systemStatusBar"));
                if (statusBarInstance == IntPtr.Zero) return false;

                // Create a status item with variable length
                statusItem = objc_msgSend(statusBarInstance, sel_registerName("statusItemWithLength:"), -1.0);
                if (statusItem == IntPtr.Zero) return false;

                // Set the title (will be visible)
                IntPtr appNamePtr = Marshal.StringToHGlobalAuto(appName);
                IntPtr nsString = objc_msgSend(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), appNamePtr);
                objc_msgSend(statusItem, sel_registerName("setTitle:"), nsString);
                Marshal.FreeHGlobal(appNamePtr);

                // Set tooltip
                if (!string.IsNullOrEmpty(tooltip))
                {
                    IntPtr tooltipPtr = Marshal.StringToHGlobalAuto(tooltip);
                    IntPtr tooltipString = objc_msgSend(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), tooltipPtr);
                    objc_msgSend(statusItem, sel_registerName("setToolTip:"), tooltipString);
                    Marshal.FreeHGlobal(tooltipPtr);
                }

                // Create and set menu
                menu = objc_msgSend(objc_getClass("NSMenu"), sel_registerName("alloc"));
                menu = objc_msgSend(menu, sel_registerName("init"));
                
                if (menu != IntPtr.Zero)
                {
                    RebuildMenu();
                    objc_msgSend(statusItem, sel_registerName("setMenu:"), menu);
                }

                // Set button with icon if provided
                if (icon != null)
                {
                    SetIcon(icon);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize macOS system tray: {ex.Message}");
                return false;
            }
        }

        public bool UpdateIcon(Texture2D icon)
        {
            try
            {
                if (icon != null && statusItem != IntPtr.Zero)
                {
                    SetIcon(icon);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateTooltip(string tooltip)
        {
            try
            {
                if (statusItem != IntPtr.Zero && !string.IsNullOrEmpty(tooltip))
                {
                    IntPtr tooltipPtr = Marshal.StringToHGlobalAuto(tooltip);
                    IntPtr tooltipString = objc_msgSend(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), tooltipPtr);
                    objc_msgSend(statusItem, sel_registerName("setToolTip:"), tooltipString);
                    Marshal.FreeHGlobal(tooltipPtr);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Remove()
        {
            try
            {
                if (statusBar != IntPtr.Zero && statusItem != IntPtr.Zero)
                {
                    IntPtr statusBarInstance = objc_msgSend(statusBar, sel_registerName("systemStatusBar"));
                    objc_msgSend(statusBarInstance, sel_registerName("removeStatusItem:"), statusItem);
                    statusItem = IntPtr.Zero;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RebuildMenu()
        {
            if (menu == IntPtr.Zero || menuBuilder == null) return;

            try
            {
                // Clear existing items
                objc_msgSend(menu, sel_registerName("removeAllItems"));

                // Get menu items from builder
                List<(string, Action)> menuItems = menuBuilder.Invoke();
                if (menuItems == null) return;

                foreach (var (label, action) in menuItems)
                {
                    if (label == "-")
                    {
                        // Add separator
                        objc_msgSend(menu, sel_registerName("addItem:"), 
                            objc_msgSend(objc_getClass("NSMenuItem"), sel_registerName("separatorItem")));
                    }
                    else
                    {
                        // Create menu item
                        IntPtr menuItem = objc_msgSend(objc_getClass("NSMenuItem"), sel_registerName("alloc"));
                        IntPtr labelPtr = Marshal.StringToHGlobalAuto(label);
                        IntPtr labelStr = objc_msgSend(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), labelPtr);
                        
                        menuItem = objc_msgSend(menuItem, sel_registerName("initWithTitle:action:keyEquivalent:"), 
                            labelStr, sel_registerName("menuItemClicked:"), IntPtr.Zero);

                        Marshal.FreeHGlobal(labelPtr);

                        if (menuItem != IntPtr.Zero)
                        {
                            objc_msgSend(menu, sel_registerName("addItem:"), menuItem);
                        }
                    }
                }

                // Add Quit option at the end
                IntPtr quitItem = objc_msgSend(objc_getClass("NSMenuItem"), sel_registerName("alloc"));
                IntPtr quitPtr = Marshal.StringToHGlobalAuto("Quit");
                IntPtr quitStr = objc_msgSend(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), quitPtr);
                quitItem = objc_msgSend(quitItem, sel_registerName("initWithTitle:action:keyEquivalent:"), 
                    quitStr, sel_registerName("terminate:"), IntPtr.Zero);
                Marshal.FreeHGlobal(quitPtr);
                objc_msgSend(menu, sel_registerName("addItem:"), quitItem);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to rebuild menu: {ex.Message}");
            }
        }

        private void SetIcon(Texture2D icon)
        {
            try
            {
                // Convert Texture2D to NSImage - this is a simplified approach
                // In production, you'd want to properly convert the texture data to NSImage format
                if (statusItem == IntPtr.Zero) return;

                // For now, we'll use the title instead of icon
                // A full implementation would convert the Texture2D to NSImage
                Debug.Log("Icon setting for macOS status bar requires native texture conversion");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set tray icon: {ex.Message}");
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
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, IntPtr arg1, IntPtr arg2);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, IntPtr arg1, IntPtr arg2, IntPtr arg3);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr obj, IntPtr sel, double arg1);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string selectorName);
    }
}
