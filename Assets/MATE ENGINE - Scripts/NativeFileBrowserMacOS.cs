using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SFB
{
    /// <summary>
    /// Native macOS file browser using NSOpenPanel for ARM64 compatibility
    /// This is a replacement for StandaloneFileBrowser that works on Apple Silicon
    /// </summary>
    public class NativeFileBrowserMacOS
    {
        #if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        
        [DllImport("libobjc.dylib")]
        private static extern IntPtr objc_getClass(string className);
        
        [DllImport("libobjc.dylib")]
        private static extern IntPtr sel_registerName(string selectorName);
        
        [DllImport("libobjc.dylib")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
        
        [DllImport("libobjc.dylib")]
        private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, bool arg);
        
        [DllImport("libobjc.dylib")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);
        
        public static string[] OpenFilePanel(string title, string directory, string extension, bool multiselect)
        {
            try
            {
                // Create NSOpenPanel
                IntPtr nsOpenPanelClass = objc_getClass("NSOpenPanel");
                IntPtr openPanel = objc_msgSend(nsOpenPanelClass, sel_registerName("openPanel"));
                
                // Set properties
                objc_msgSend(openPanel, sel_registerName("setCanChooseFiles:"), true);
                objc_msgSend(openPanel, sel_registerName("setCanChooseDirectories:"), false);
                objc_msgSend(openPanel, sel_registerName("setAllowsMultipleSelection:"), multiselect);
                
                // Run the panel
                IntPtr runModalSelector = sel_registerName("runModal");
                long result = (long)objc_msgSend(openPanel, runModalSelector);
                
                // NSModalResponseOK = 1
                if (result == 1)
                {
                    // Get URLs
                    IntPtr urls = objc_msgSend(openPanel, sel_registerName("URLs"));
                    long count = (long)objc_msgSend(urls, sel_registerName("count"));
                    
                    string[] paths = new string[count];
                    for (long i = 0; i < count; i++)
                    {
                        IntPtr url = objc_msgSend(urls, sel_registerName("objectAtIndex:"), new IntPtr(i));
                        IntPtr pathPtr = objc_msgSend(url, sel_registerName("path"));
                        paths[i] = Marshal.PtrToStringAuto(pathPtr);
                    }
                    
                    return paths;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"NativeFileBrowserMacOS error: {e.Message}");
            }
            
            return new string[0];
        }
        
        #else
        
        public static string[] OpenFilePanel(string title, string directory, string extension, bool multiselect)
        {
            Debug.LogWarning("NativeFileBrowserMacOS only works on macOS standalone builds");
            return new string[0];
        }
        
        #endif
    }
}
