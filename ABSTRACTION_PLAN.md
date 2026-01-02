# Windows Code Abstraction Plan
## Platform-Independent Flow with Windows-Only Implementation

**Date:** January 2, 2026  
**Objective:** Abstract Windows-specific code behind interfaces while maintaining exact Windows functionality. Other platforms get no-op implementations.

**Key Principle:** *One feature at a time, test after each change*

---

## Why Previous Attempt Failed

### Root Causes Identified:
1. ❌ **Incomplete implementations** - macOS/Linux stubs had syntax errors
2. ❌ **All-or-nothing approach** - Changed everything at once
3. ❌ **No incremental testing** - Couldn't identify what broke
4. ❌ **Direct WinApi calls remained** - Code bypassed abstraction layer
5. ❌ **Compilation errors** - Missing methods, wrong signatures

### This Time:
1. ✅ **Complete stubs first** - Ensure all platforms compile before refactoring
2. ✅ **Incremental migration** - One file/feature at a time
3. ✅ **Test after each step** - Windows must work, others must compile
4. ✅ **Force abstraction usage** - Make WinApi internal/private where possible
5. ✅ **No behavior changes** - Windows code just moves, doesn't change

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│           Consumer Code (Avatar Handlers)           │
│  - AvatarWindowHandler.cs                          │
│  - AvatarHideHandler.cs                            │
│  - SystemTray.cs                                   │
└────────────────────┬────────────────────────────────┘
                     │ Uses interfaces only
                     ▼
┌─────────────────────────────────────────────────────┐
│              Service Interfaces                      │
│  - IWindowService                                   │
│  - IScreenService                                   │
│  - ISystemTrayService                               │
│  - ITransparencyService                             │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────┐
│          PlatformServiceLocator                      │
│  Selects implementation based on platform           │
└──────┬──────────────────────────────────────────┬───┘
       │                                          │
       ▼                                          ▼
┌──────────────────┐                    ┌──────────────────┐
│ Windows Services │                    │  Stub Services   │
│ (Full logic)     │                    │  (No-op/Empty)   │
│ #if WIN          │                    │  #if !WIN        │
└──────────────────┘                    └──────────────────┘
       │
       ▼
┌──────────────────┐
│ WinApi.cs        │
│ DwmApi.cs        │
│ (Kept internal)  │
└──────────────────┘
```

---

## Phase-by-Phase Implementation

### PHASE 0: Preparation (No Code Changes)
**Goal:** Set up testing strategy and backup

**Tasks:**
1. ✅ Create git branch: `feature/windows-abstraction`
2. ✅ Document current Windows build configuration
3. ✅ Take note of Unity project settings (scripting defines, etc.)
4. ✅ Ensure project builds successfully on Windows before starting
5. ✅ Create rollback procedure

**Success Criteria:**
- Clean Windows build
- All features working
- Git branch created
- Backup strategy confirmed

---

### PHASE 1: Create Interface Definitions
**Goal:** Define all service interfaces WITHOUT breaking existing code

**Location:** `Assets/MATE ENGINE - Scripts/Platform/Interfaces/`

**Files to Create:**

#### 1.1 Core Platform Interface
```csharp
// IPlatformService.cs
using UnityEngine;

namespace MateEngine.Platform
{
    public interface IPlatformService
    {
        string PlatformName { get; }
        bool IsSupported(PlatformFeature feature);
    }

    public enum PlatformFeature
    {
        WindowManagement,
        SystemTray,
        MultiMonitor,
        WindowTransparency,
        ScreenCapture,
        WindowSnapping
    }
}
```

#### 1.2 Window Management Interface
```csharp
// IWindowService.cs
using System;
using UnityEngine;

namespace MateEngine.Platform
{
    /// <summary>
    /// Cross-platform window management
    /// </summary>
    public interface IWindowService
    {
        // Window Handle
        IntPtr GetMainWindowHandle();
        
        // Position and Size
        bool GetWindowRect(IntPtr hWnd, out WindowRect rect);
        bool SetWindowPosition(IntPtr hWnd, int x, int y, int width, int height, SetWindowFlags flags);
        
        // Window State
        bool ShowWindow(IntPtr hWnd, ShowWindowCommand cmd);
        bool IsWindowVisible(IntPtr hWnd);
        bool IsWindowMinimized(IntPtr hWnd);
        bool IsWindowMaximized(IntPtr hWnd);
        
        // Window Properties
        bool SetTopMost(IntPtr hWnd, bool topmost);
        bool SetWindowStyle(IntPtr hWnd, WindowStyle style);
        ulong GetWindowStyle(IntPtr hWnd);
        
        // Window Enumeration
        void EnumerateWindows(WindowEnumCallback callback);
        
        // Window Information
        string GetWindowText(IntPtr hWnd);
        string GetWindowClassName(IntPtr hWnd);
        uint GetWindowProcessId(IntPtr hWnd);
        
        // Parent/Child Relationships
        IntPtr GetParent(IntPtr hWnd);
        IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags);
    }

    // Supporting types
    public struct WindowRect
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    public enum ShowWindowCommand
    {
        Hide = 0,
        Normal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActive = 7,
        ShowNA = 8,
        Restore = 9
    }

    [Flags]
    public enum SetWindowFlags : uint
    {
        NoSize = 0x0001,
        NoMove = 0x0002,
        NoZOrder = 0x0004,
        NoActivate = 0x0010,
        ShowWindow = 0x0040,
        FrameChanged = 0x0020,
        NoOwnerZOrder = 0x0200,
        NoSendChanging = 0x0400
    }

    [Flags]
    public enum WindowStyle : ulong
    {
        Border = 0x00800000L,
        Caption = 0x00C00000L,
        SysMenu = 0x00080000L,
        ThickFrame = 0x00040000L,
        Popup = 0x80000000L,
        Overlapped = 0x00000000L
    }

    public enum GetAncestorFlags : uint
    {
        Parent = 1,
        Root = 2,
        RootOwner = 3
    }

    public delegate bool WindowEnumCallback(IntPtr hWnd);
}
```

#### 1.3 Screen/Monitor Interface
```csharp
// IScreenService.cs
using System;
using UnityEngine;

namespace MateEngine.Platform
{
    public interface IScreenService
    {
        // Cursor Operations
        bool GetCursorPosition(out Vector2Int position);
        bool SetCursorPosition(int x, int y);
        
        // Monitor Information
        MonitorInfo GetPrimaryMonitor();
        MonitorInfo GetMonitorFromPoint(Vector2Int point);
        MonitorInfo GetMonitorFromWindow(IntPtr hWnd);
        MonitorInfo[] GetAllMonitors();
        
        // Virtual Desktop
        Rect GetVirtualDesktopBounds();
    }

    public struct MonitorInfo
    {
        public Rect WorkArea;      // Excludes taskbar
        public Rect MonitorArea;   // Full screen
        public bool IsPrimary;
        public string DeviceName;
    }
}
```

#### 1.4 Transparency Interface
```csharp
// ITransparencyService.cs
using System;

namespace MateEngine.Platform
{
    public interface ITransparencyService
    {
        bool IsCompositionEnabled();
        bool EnableTransparency(IntPtr hWnd);
        bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom);
        bool SetWindowTransparency(IntPtr hWnd, byte alpha);
    }
}
```

#### 1.5 System Tray Interface
```csharp
// ISystemTrayService.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform
{
    public interface ISystemTrayService
    {
        bool IsSupported { get; }
        bool Initialize(string tooltip, Texture2D icon);
        bool UpdateIcon(Texture2D icon);
        bool UpdateTooltip(string tooltip);
        bool ShowContextMenu(List<TrayMenuItem> items);
        bool Remove();
        
        event Action OnLeftClick;
        event Action OnRightClick;
    }

    public struct TrayMenuItem
    {
        public string Label;
        public Action OnClick;
        public bool IsSeparator;
        public bool IsChecked;
    }
}
```

#### 1.6 Screen Capture Interface
```csharp
// IScreenCaptureService.cs
using UnityEngine;

namespace MateEngine.Platform
{
    public interface IScreenCaptureService
    {
        bool IsSupported { get; }
        bool InitializeCapture(int width, int height);
        bool CaptureScreen(out Color[] pixels);
        void ReleaseCapture();
    }
}
```

**Success Criteria:**
- All interface files compile
- No existing code broken
- Clear, well-documented interfaces
- Covers all Windows API usage identified in analysis

---

### PHASE 2: Create Service Locator
**Goal:** Central access point for platform services

**Location:** `Assets/MATE ENGINE - Scripts/Platform/`

#### 2.1 PlatformServiceLocator.cs
```csharp
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
```

**Success Criteria:**
- ServiceLocator compiles
- Can be called from any script
- Lazy initialization works
- Clear dependency injection point

---

### PHASE 3: Create Stub Implementations
**Goal:** Ensure non-Windows platforms compile WITHOUT any behavior

**Location:** `Assets/MATE ENGINE - Scripts/Platform/Stub/`

**Critical:** These MUST compile on all platforms!

#### 3.1 StubWindowService.cs
```csharp
using System;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubWindowService : IWindowService
    {
        public IntPtr GetMainWindowHandle() => IntPtr.Zero;
        
        public bool GetWindowRect(IntPtr hWnd, out WindowRect rect)
        {
            rect = default;
            return false;
        }
        
        public bool SetWindowPosition(IntPtr hWnd, int x, int y, int width, int height, SetWindowFlags flags) => false;
        public bool ShowWindow(IntPtr hWnd, ShowWindowCommand cmd) => false;
        public bool IsWindowVisible(IntPtr hWnd) => false;
        public bool IsWindowMinimized(IntPtr hWnd) => false;
        public bool IsWindowMaximized(IntPtr hWnd) => false;
        public bool SetTopMost(IntPtr hWnd, bool topmost) => false;
        public bool SetWindowStyle(IntPtr hWnd, WindowStyle style) => false;
        public ulong GetWindowStyle(IntPtr hWnd) => 0;
        public void EnumerateWindows(WindowEnumCallback callback) { }
        public string GetWindowText(IntPtr hWnd) => string.Empty;
        public string GetWindowClassName(IntPtr hWnd) => string.Empty;
        public uint GetWindowProcessId(IntPtr hWnd) => 0;
        public IntPtr GetParent(IntPtr hWnd) => IntPtr.Zero;
        public IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags) => IntPtr.Zero;
    }
}
```

#### 3.2 StubScreenService.cs
```csharp
using System;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubScreenService : IScreenService
    {
        public bool GetCursorPosition(out Vector2Int position)
        {
            // Use Unity's Input as fallback
            position = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            return true;
        }
        
        public bool SetCursorPosition(int x, int y) => false;
        
        public MonitorInfo GetPrimaryMonitor()
        {
            return new MonitorInfo
            {
                WorkArea = new Rect(0, 0, Screen.width, Screen.height),
                MonitorArea = new Rect(0, 0, Screen.width, Screen.height),
                IsPrimary = true,
                DeviceName = "Primary"
            };
        }
        
        public MonitorInfo GetMonitorFromPoint(Vector2Int point) => GetPrimaryMonitor();
        public MonitorInfo GetMonitorFromWindow(IntPtr hWnd) => GetPrimaryMonitor();
        
        public MonitorInfo[] GetAllMonitors()
        {
            return new[] { GetPrimaryMonitor() };
        }
        
        public Rect GetVirtualDesktopBounds()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
    }
}
```

#### 3.3 StubTransparencyService.cs
```csharp
using System;

namespace MateEngine.Platform.Stub
{
    public class StubTransparencyService : ITransparencyService
    {
        public bool IsCompositionEnabled() => false;
        public bool EnableTransparency(IntPtr hWnd) => false;
        public bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom) => false;
        public bool SetWindowTransparency(IntPtr hWnd, byte alpha) => false;
    }
}
```

#### 3.4 StubSystemTrayService.cs
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubSystemTrayService : ISystemTrayService
    {
        public bool IsSupported => false;
        public bool Initialize(string tooltip, Texture2D icon) => false;
        public bool UpdateIcon(Texture2D icon) => false;
        public bool UpdateTooltip(string tooltip) => false;
        public bool ShowContextMenu(List<TrayMenuItem> items) => false;
        public bool Remove() => false;
        
        public event Action OnLeftClick;
        public event Action OnRightClick;
    }
}
```

#### 3.5 StubScreenCaptureService.cs
```csharp
using UnityEngine;

namespace MateEngine.Platform.Stub
{
    public class StubScreenCaptureService : IScreenCaptureService
    {
        public bool IsSupported => false;
        public bool InitializeCapture(int width, int height) => false;
        public bool CaptureScreen(out Color[] pixels)
        {
            pixels = null;
            return false;
        }
        public void ReleaseCapture() { }
    }
}
```

#### 3.6 StubPlatformService.cs
```csharp
namespace MateEngine.Platform.Stub
{
    public class StubPlatformService : IPlatformService
    {
        public string PlatformName => "Unsupported Platform";
        
        public bool IsSupported(PlatformFeature feature) => false;
    }
}
```

**Testing Checklist:**
- [ ] Build on Windows (should compile)
- [ ] Build with UNITY_STANDALONE_OSX define (should compile)
- [ ] Build with UNITY_STANDALONE_LINUX define (should compile)
- [ ] No runtime errors when stubs are called

**Success Criteria:**
- All platforms compile successfully
- Stub services return safe defaults
- No exceptions thrown
- Can instantiate all stub services

---

### PHASE 4: Create Windows Implementations
**Goal:** Move existing Windows code into service classes

**Location:** `Assets/MATE ENGINE - Scripts/Platform/Windows/`

**Important:** Keep WinApi.cs and DwmApi.cs, but make them internal to Windows namespace

#### 4.1 Make WinApi.cs Internal
```csharp
// At top of WinApi.cs
namespace MateEngine.Platform.Windows
{
    // Make class internal - only Windows services can use it
    internal class WinApi  // was: public class WinApi
    {
        // ... existing code ...
    }
}
```

#### 4.2 Make DwmApi.cs Internal
```csharp
// At top of DwmApi.cs
namespace MateEngine.Platform.Windows
{
    internal class DwmApi  // was: class DwmApi
    {
        // ... existing code ...
    }
}
```

#### 4.3 WindowsWindowService.cs
```csharp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Text;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsWindowService : IWindowService
    {
        public IntPtr GetMainWindowHandle()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        }

        public bool GetWindowRect(IntPtr hWnd, out WindowRect rect)
        {
            Kirurobo.WinApi.RECT winRect;
            bool success = Kirurobo.WinApi.GetWindowRect(hWnd, out winRect);
            
            rect = new WindowRect
            {
                Left = winRect.left,
                Top = winRect.top,
                Right = winRect.right,
                Bottom = winRect.bottom
            };
            
            return success;
        }

        public bool SetWindowPosition(IntPtr hWnd, int x, int y, int width, int height, SetWindowFlags flags)
        {
            return Kirurobo.WinApi.SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, (uint)flags);
        }

        public bool ShowWindow(IntPtr hWnd, ShowWindowCommand cmd)
        {
            return Kirurobo.WinApi.ShowWindow(hWnd, (int)cmd);
        }

        public bool IsWindowVisible(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsWindowVisible(hWnd);
        }

        public bool IsWindowMinimized(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsIconic(hWnd);
        }

        public bool IsWindowMaximized(IntPtr hWnd)
        {
            return Kirurobo.WinApi.IsZoomed(hWnd);
        }

        public bool SetTopMost(IntPtr hWnd, bool topmost)
        {
            IntPtr flag = topmost ? Kirurobo.WinApi.HWND_TOPMOST : Kirurobo.WinApi.HWND_NOTOPMOST;
            return Kirurobo.WinApi.SetWindowPos(hWnd, flag, 0, 0, 0, 0, 
                Kirurobo.WinApi.SWP_NOMOVE | Kirurobo.WinApi.SWP_NOSIZE | Kirurobo.WinApi.SWP_NOACTIVATE);
        }

        public bool SetWindowStyle(IntPtr hWnd, WindowStyle style)
        {
            Kirurobo.WinApi.SetWindowLong(hWnd, Kirurobo.WinApi.GWL_STYLE, (ulong)style);
            return true;
        }

        public ulong GetWindowStyle(IntPtr hWnd)
        {
            return Kirurobo.WinApi.GetWindowLong(hWnd, Kirurobo.WinApi.GWL_STYLE);
        }

        public void EnumerateWindows(WindowEnumCallback callback)
        {
            Kirurobo.WinApi.EnumWindows((hWnd, lParam) => callback(hWnd), IntPtr.Zero);
        }

        public string GetWindowText(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Kirurobo.WinApi.GetWindowText(hWnd, sb, 256);
            return sb.ToString();
        }

        public string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Kirurobo.WinApi.GetClassName(hWnd, sb, 256);
            return sb.ToString();
        }

        public uint GetWindowProcessId(IntPtr hWnd)
        {
            ulong pid;
            Kirurobo.WinApi.GetWindowThreadProcessId(hWnd, out pid);
            return (uint)pid;
        }

        public IntPtr GetParent(IntPtr hWnd)
        {
            return Kirurobo.WinApi.GetParent(hWnd);
        }

        public IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags)
        {
            return Kirurobo.WinApi.GetAncestor(hWnd, (uint)flags);
        }
    }
}
#endif
```

#### 4.4 WindowsScreenService.cs
```csharp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MateEngine.Platform.Windows
{
    public class WindowsScreenService : IScreenService
    {
        [DllImport("user32.dll")]
        private static extern bool MonitorFromPoint(POINT pt, uint dwFlags);
        
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x, y; }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left, top, right, bottom; }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }
        
        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const uint MONITORINFOF_PRIMARY = 1;

        public bool GetCursorPosition(out Vector2Int position)
        {
            Kirurobo.WinApi.POINT pt;
            bool success = Kirurobo.WinApi.GetCursorPos(out pt);
            position = new Vector2Int(pt.x, pt.y);
            return success;
        }

        public bool SetCursorPosition(int x, int y)
        {
            return Kirurobo.WinApi.SetCursorPos(x, y);
        }

        public MonitorInfo GetPrimaryMonitor()
        {
            // Implementation using EnumDisplayMonitors
            // Return primary monitor info
            throw new NotImplementedException();
        }

        public MonitorInfo GetMonitorFromPoint(Vector2Int point)
        {
            POINT pt = new POINT { x = point.x, y = point.y };
            IntPtr hMonitor = MonitorFromWindow(IntPtr.Zero, MONITOR_DEFAULTTONEAREST);
            
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf(mi);
            
            if (GetMonitorInfo(hMonitor, ref mi))
            {
                return ConvertMonitorInfo(mi);
            }
            
            return default;
        }

        public MonitorInfo GetMonitorFromWindow(IntPtr hWnd)
        {
            IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf(mi);
            
            if (GetMonitorInfo(hMonitor, ref mi))
            {
                return ConvertMonitorInfo(mi);
            }
            
            return default;
        }

        public MonitorInfo[] GetAllMonitors()
        {
            // Implementation using EnumDisplayMonitors
            throw new NotImplementedException();
        }

        public Rect GetVirtualDesktopBounds()
        {
            // Use GetSystemMetrics
            throw new NotImplementedException();
        }

        private MonitorInfo ConvertMonitorInfo(MONITORINFO mi)
        {
            return new MonitorInfo
            {
                WorkArea = new Rect(mi.rcWork.left, mi.rcWork.top, 
                    mi.rcWork.right - mi.rcWork.left, mi.rcWork.bottom - mi.rcWork.top),
                MonitorArea = new Rect(mi.rcMonitor.left, mi.rcMonitor.top,
                    mi.rcMonitor.right - mi.rcMonitor.left, mi.rcMonitor.bottom - mi.rcMonitor.top),
                IsPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0,
                DeviceName = "Monitor"
            };
        }
    }
}
#endif
```

#### 4.5 WindowsTransparencyService.cs
```csharp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;

namespace MateEngine.Platform.Windows
{
    public class WindowsTransparencyService : ITransparencyService
    {
        public bool IsCompositionEnabled()
        {
            return Kirurobo.DwmApi.DwmIsCompositionEnabled();
        }

        public bool EnableTransparency(IntPtr hWnd)
        {
            try
            {
                Kirurobo.DwmApi.DwmExtendIntoClientAll(hWnd);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetTransparencyMargins(IntPtr hWnd, int left, int top, int right, int bottom)
        {
            try
            {
                var margins = new Kirurobo.DwmApi.MARGINS(left, top, right, bottom);
                Kirurobo.DwmApi.DwmExtendFrameIntoClientArea(hWnd, ref margins);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetWindowTransparency(IntPtr hWnd, byte alpha)
        {
            var colorKey = new Kirurobo.WinApi.COLORREF(0);
            return Kirurobo.WinApi.SetLayeredWindowAttributes(hWnd, colorKey, alpha, Kirurobo.WinApi.LWA_ALPHA);
        }
    }
}
#endif
```

**Success Criteria:**
- Windows services compile
- All services properly wrap existing WinApi/DwmApi calls
- WinApi/DwmApi are now internal (can't be accessed outside Windows namespace)
- No behavioral changes to Windows functionality

---

### PHASE 5: Refactor One Component at a Time

**Critical:** Do NOT refactor all files at once. One component → Test → Next component

#### 5.1 Test Component: MemoryTrim.cs (Simplest)
**Why first:** Single file, single API call, easy to verify

**Original Code:**
```csharp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("psapi.dll")]
    static extern bool EmptyWorkingSet(IntPtr hProcess);
#endif

static void TrimWorkingSet()
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    EmptyWorkingSet(Process.GetCurrentProcess().Handle);
#endif
}
```

**Refactored (keep in same file, add service interface):**
```csharp
using MateEngine.Platform;

// No changes to public interface
// Just use service internally

static void TrimWorkingSet()
{
    // Try platform-specific trim
    if (PlatformServiceLocator.PlatformService.IsSupported(PlatformFeature.MemoryManagement))
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        EmptyWorkingSet(Process.GetCurrentProcess().Handle);
#endif
    }
}

// Keep existing DllImport for now - will remove later
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("psapi.dll")]
    static extern bool EmptyWorkingSet(IntPtr hProcess);
#endif
```

**Test:**
- [ ] Windows: Memory trim still works
- [ ] macOS/Linux: No errors, simply skips trim

#### 5.2 AvatarHideHandler.cs
**Complexity:** Medium - uses cursor position, window positioning, monitor detection

**Refactoring Strategy:**
1. Add service references at top of class
2. Replace direct WinApi calls with service calls
3. Keep platform guards around Windows-specific features
4. Test thoroughly

**Before:**
```csharp
void Update()
{
#if !UNITY_STANDALONE_WIN
    return;
#else
    if (GetCursorPos(out POINT cp)) {
        // ...
    }
#endif
}

[DllImport("user32.dll")]
static extern bool GetCursorPos(out POINT lpPoint);
```

**After:**
```csharp
using MateEngine.Platform;

private IScreenService _screenService;
private IWindowService _windowService;

void Start()
{
    _screenService = PlatformServiceLocator.ScreenService;
    _windowService = PlatformServiceLocator.WindowService;
    // ... rest of Start
}

void Update()
{
    // No platform guard needed - service handles it
    if (_screenService.GetCursorPosition(out Vector2Int cursorPos))
    {
        // Use cursorPos.x, cursorPos.y
        // Service returns false on unsupported platforms
    }
}

// Remove all DllImport declarations
```

**Test:**
- [ ] Windows: Cursor hiding works at screen edges
- [ ] Multi-monitor: Works correctly
- [ ] macOS/Linux: Compiles, doesn't crash (no functionality)

#### 5.3 System Tray
**Complexity:** High - complete subsystem, multiple files

**Refactoring Strategy:**
1. Create WindowsSystemTrayService that wraps existing TrayIcon.cs logic
2. Keep TrayIcon.cs as internal implementation detail
3. Refactor SystemTray.cs to use ISystemTrayService

**New File: WindowsSystemTrayService.cs**
```csharp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils; // TrayIcon namespace

namespace MateEngine.Platform.Windows
{
    public class WindowsSystemTrayService : ISystemTrayService
    {
        public bool IsSupported => true;

        public bool Initialize(string tooltip, Texture2D icon)
        {
            try
            {
                TrayIcon.Init("MateEngine", tooltip, icon);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ShowContextMenu(List<TrayMenuItem> items)
        {
            var winItems = new List<(string, Action)>();
            foreach (var item in items)
            {
                if (item.IsSeparator)
                {
                    winItems.Add(("---", null));
                }
                else
                {
                    string label = item.IsChecked ? "✔ " + item.Label : item.Label;
                    winItems.Add((label, item.OnClick));
                }
            }
            
            TrayIcon.OnBuildMenu = () => winItems;
            return true;
        }

        // ... implement other methods
    }
}
#endif
```

**Refactor SystemTray.cs:**
```csharp
using MateEngine.Platform;

public class SystemTray : MonoBehaviour
{
    private ISystemTrayService _trayService;

    void Awake()
    {
        _trayService = PlatformServiceLocator.SystemTrayService;
        
        if (!_trayService.IsSupported)
        {
            Debug.LogWarning("System tray not supported on this platform");
            return;
        }

        _trayService.Initialize(iconName, icon);
        // ... rest of logic
    }
}
```

**Test:**
- [ ] Windows: Tray icon appears, menu works
- [ ] macOS/Linux: No tray icon, no errors

#### 5.4 AvatarWindowHandler.cs (MOST CRITICAL)
**Complexity:** EXTREME - 995 lines, core feature

**Refactoring Strategy:**
1. **DON'T rush this** - most complex refactoring
2. Keep ALL platform guards
3. Add service layer but keep guards
4. Test extensively

**Approach:**
```csharp
using MateEngine.Platform;

public class AvatarWindowHandler : MonoBehaviour
{
    private IWindowService _windowService;
    private IScreenService _screenService;

    void Start()
    {
        _windowService = PlatformServiceLocator.WindowService;
        _screenService = PlatformServiceLocator.ScreenService;
        
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Windows-specific initialization
        unityHWND = _windowService.GetMainWindowHandle();
#endif
    }

    void Update()
    {
#if !UNITY_STANDALONE_WIN
        return; // Early exit for non-Windows
#endif
        // Rest of Windows-specific code uses _windowService
        // Keep all existing logic, just replace API calls
    }

    // Keep Windows-specific methods with guards
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    bool IsSitEligibleWindow(IntPtr hWnd)
    {
        if (!_windowService.IsWindowVisible(hWnd)) return false;
        // ... rest of logic using _windowService
    }
#endif
}
```

**Test Plan:**
- [ ] Windows: Window snapping works identically
- [ ] Windows: Z-order detection works
- [ ] Windows: Occluders work
- [ ] Windows: Performance unchanged
- [ ] macOS/Linux: Compiles, avatar just doesn't snap

---

### PHASE 6: Testing and Validation

**For Each Refactored Component:**

#### Windows Testing:
1. [ ] Feature works identically to before
2. [ ] No performance regression
3. [ ] No visual glitches
4. [ ] Memory usage unchanged
5. [ ] No exceptions in console

#### Cross-Platform Testing:
1. [ ] macOS build compiles
2. [ ] Linux build compiles (if possible)
3. [ ] No exceptions at runtime on stub platforms
4. [ ] Graceful degradation (features disabled, not crashing)

#### Regression Testing:
1. [ ] All original features still work on Windows
2. [ ] Save/load system works
3. [ ] Settings persist
4. [ ] VRM loading works
5. [ ] Steam integration works

---

## Rollback Strategy

If anything breaks at ANY phase:

1. **Git Revert:**
   ```bash
   git checkout main
   git branch -D feature/windows-abstraction
   ```

2. **Identify Problem:**
   - Which component broke?
   - What error occurred?
   - Windows or cross-platform issue?

3. **Incremental Fix:**
   - Don't try to fix everything
   - Fix one component
   - Test
   - Continue

---

## Success Criteria

### Phase 1-3 Success:
- [ ] All interfaces defined
- [ ] Service locator created
- [ ] All stub implementations compile
- [ ] Windows build still works
- [ ] macOS/Linux builds compile

### Phase 4 Success:
- [ ] All Windows services implemented
- [ ] WinApi/DwmApi made internal
- [ ] Windows functionality unchanged
- [ ] No performance regression

### Phase 5 Success:
- [ ] All components refactored
- [ ] Tests pass for each component
- [ ] No direct WinApi calls outside Windows namespace
- [ ] Clean separation of concerns

### Final Success:
- [ ] Windows build: 100% feature parity
- [ ] macOS build: Compiles, runs, no crashes
- [ ] Linux build: Compiles, runs, no crashes
- [ ] Code is maintainable and extensible
- [ ] Ready for Phase 2: macOS implementations

---

## Timeline Estimate

| Phase | Duration | Risk Level |
|-------|----------|------------|
| Phase 0: Preparation | 1 hour | 🟢 Low |
| Phase 1: Interfaces | 2-3 hours | 🟢 Low |
| Phase 2: Service Locator | 1 hour | 🟢 Low |
| Phase 3: Stubs | 2-3 hours | 🟡 Medium |
| Phase 4: Windows Services | 4-6 hours | 🟡 Medium |
| Phase 5.1: MemoryTrim | 30 min | 🟢 Low |
| Phase 5.2: AvatarHideHandler | 2-3 hours | 🟡 Medium |
| Phase 5.3: System Tray | 3-4 hours | 🟡 Medium |
| Phase 5.4: AvatarWindowHandler | 6-8 hours | 🔴 High |
| Phase 6: Testing | 4-6 hours | 🟡 Medium |
| **Total** | **25-35 hours** | |

---

## Key Principles to Remember

1. **One Thing at a Time** - Don't refactor multiple components simultaneously
2. **Test After Each Change** - Windows must work after every change
3. **Keep Platform Guards** - Don't remove `#if` guards prematurely
4. **No Behavior Changes** - Windows code just moves, doesn't change
5. **Stubs Must Compile** - Non-Windows platforms must build
6. **Git Commits After Each Phase** - Easy rollback points
7. **Document Changes** - Update this plan as you go

---

## Next Steps After Completion

Once this abstraction is complete:

1. **Phase 2:** Implement macOS native services (Objective-C plugin)
2. **Phase 3:** Implement Linux services (X11/Wayland)
3. **Phase 4:** Feature parity testing on all platforms
4. **Phase 5:** Remove platform guards where possible
5. **Phase 6:** Optimize platform-specific code

---

**Ready to Begin?** Start with Phase 0: Preparation
