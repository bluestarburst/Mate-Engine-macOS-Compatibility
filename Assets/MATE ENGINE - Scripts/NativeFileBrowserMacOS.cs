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

        [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void_bool(IntPtr receiver, IntPtr selector, bool arg);

        [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

        [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_ulong(IntPtr receiver, IntPtr selector, ulong arg);

        [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern long objc_msgSend_long(IntPtr receiver, IntPtr selector);

        /// <summary>
        /// Converts an NSString pointer to a C# string using UTF8String method
        /// </summary>
        private static string NSStringToString(IntPtr nsString)
        {
            if (nsString == IntPtr.Zero)
                return null;

            // Get the UTF8String (returns const char*)
            IntPtr utf8Ptr = objc_msgSend(nsString, sel_registerName("UTF8String"));
            if (utf8Ptr == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringUTF8(utf8Ptr);
        }

        public static string[] OpenFilePanel(string title, string directory, string extension, bool multiselect)
        {
            try
            {
                // Create NSOpenPanel
                IntPtr nsOpenPanelClass = objc_getClass("NSOpenPanel");
                IntPtr openPanel = objc_msgSend(nsOpenPanelClass, sel_registerName("openPanel"));

                if (openPanel == IntPtr.Zero)
                {
                    Debug.LogError("[NativeFileBrowserMacOS] Failed to create NSOpenPanel");
                    return new string[0];
                }

                // Set properties
                objc_msgSend_void_bool(openPanel, sel_registerName("setCanChooseFiles:"), true);
                objc_msgSend_void_bool(openPanel, sel_registerName("setCanChooseDirectories:"), false);
                objc_msgSend_void_bool(openPanel, sel_registerName("setAllowsMultipleSelection:"), multiselect);

                // Run the panel
                long result = objc_msgSend_long(openPanel, sel_registerName("runModal"));

                Debug.Log($"[NativeFileBrowserMacOS] runModal result: {result}");

                // NSModalResponseOK = 1
                if (result == 1)
                {
                    // Get URLs
                    IntPtr urls = objc_msgSend(openPanel, sel_registerName("URLs"));
                    if (urls == IntPtr.Zero)
                    {
                        Debug.LogError("[NativeFileBrowserMacOS] Failed to get URLs from panel");
                        return new string[0];
                    }

                    long count = objc_msgSend_long(urls, sel_registerName("count"));
                    Debug.Log($"[NativeFileBrowserMacOS] URL count: {count}");

                    if (count <= 0)
                    {
                        return new string[0];
                    }

                    string[] paths = new string[count];
                    for (ulong i = 0; i < (ulong)count; i++)
                    {
                        IntPtr url = objc_msgSend_ulong(urls, sel_registerName("objectAtIndex:"), i);
                        if (url == IntPtr.Zero)
                        {
                            Debug.LogWarning($"[NativeFileBrowserMacOS] URL at index {i} is null");
                            continue;
                        }

                        // Get path from NSURL - returns NSString
                        IntPtr nsStringPath = objc_msgSend(url, sel_registerName("path"));
                        if (nsStringPath == IntPtr.Zero)
                        {
                            Debug.LogWarning($"[NativeFileBrowserMacOS] Path at index {i} is null");
                            continue;
                        }

                        // Convert NSString to C# string using UTF8String
                        string path = NSStringToString(nsStringPath);
                        if (!string.IsNullOrEmpty(path))
                        {
                            paths[i] = path;
                            Debug.Log($"[NativeFileBrowserMacOS] Selected file: {paths[i]}");
                        }
                        else
                        {
                            Debug.LogWarning($"[NativeFileBrowserMacOS] Empty path at index {i}");
                        }
                    }

                    return paths;
                }
                else
                {
                    Debug.Log("[NativeFileBrowserMacOS] User cancelled file selection");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NativeFileBrowserMacOS] Error: {e.Message}\n{e.StackTrace}");
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
