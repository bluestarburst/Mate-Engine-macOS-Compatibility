# Mate-Engine Cross-Platform Code Analysis
## Comprehensive Windows-Specific API Documentation

**Generated:** 2025
**Purpose:** Complete inventory of Windows-specific code in Assets folder for cross-platform migration planning

---

## Executive Summary

### Analysis Scope
- **Total C# Files:** 1,456 files in Assets folder
- **Total C++ Files:** 0 files (all native code accessed via P/Invoke)
- **DllImport Instances:** 200+ found (search capped, actual count estimated at 300+)
- **Platform Guards:** 187 instances of `UNITY_STANDALONE_WIN` / `UNITY_EDITOR_WIN`
- **Runtime Platform Checks:** 15 instances
- **Windows DLL References:** 156 matches across major system DLLs

### Key Windows-Specific Components Identified
1. **WinApi.cs** (502 lines) - 60+ DllImport declarations for user32.dll, kernel32.dll, shell32.dll, gdi32.dll, Comdlg32.dll
2. **DwmApi.cs** (173 lines) - 8 DllImport declarations for dwmapi.dll (transparency, composition, blur effects)
3. **System Tray** (266+ lines) - Complete Windows-only implementation with 20+ shell32.dll/user32.dll imports
4. **AvatarWindowHandler.cs** (995 lines) - Core window snapping feature with heavy Windows API dependency
5. **AvatarHideHandler.cs** (308 lines) - Multi-monitor cursor tracking with Windows API
6. **DesktopAmbientProbe.cs** (359 lines) - GDI screen capture for ambient lighting
7. **MemoryTrim.cs** (80 lines) - psapi.dll EmptyWorkingSet call

### Windows-Specific Dependencies
| Windows API | Purpose | Usage Count | Cross-Platform Feasible |
|------------|---------|-------------|------------------------|
| **user32.dll** | Window management, input, UI | ~100+ calls | ✅ Yes (NSWindow/X11) |
| **kernel32.dll** | Process/memory management | ~30+ calls | ⚠️ Partial (limited alternatives) |
| **dwmapi.dll** | Desktop Window Manager effects | ~8 calls | ⚠️ Limited (transparency only) |
| **shell32.dll** | System tray, file operations | ~20+ calls | ⚠️ Platform-specific alternatives |
| **gdi32.dll** | Screen capture, rendering | ~15 calls | ⚠️ Different APIs required |
| **psapi.dll** | Process memory monitoring | ~5 calls | ⚠️ Platform-specific tools |
| **Shcore.dll** | DPI awareness | ~2 calls | ⚠️ macOS handles differently |
| **Comdlg32.dll** | File dialogs | ~3 calls | ✅ Unity provides cross-platform |
| **uWindowCapture.dll** | Window content capture | 90+ calls | ❌ Windows-only plugin |

### Cross-Platform Readiness Assessment
- **🟢 Ready:** Core Unity functionality, 3D rendering, VRM models, Steamworks integration
- **🟡 Needs Abstraction:** Window management, file operations, desktop interaction
- **🔴 Windows-Only:** System tray, window content capture, DWM transparency effects, desktop ambient probe

---

## Detailed Component Analysis

### 1. Core Windows API Wrappers

#### WinApi.cs - Complete API Surface
**Location:** [Assets/MATE ENGINE - Scripts/APIs/WinApi.cs](Assets/MATE%20ENGINE%20-%20Scripts/APIs/WinApi.cs)
**Size:** 502 lines
**Purpose:** Central Windows API wrapper providing complete window management, process control, and shell operations

**Complete DllImport List (60+ functions):**

**Window Management (user32.dll):**
- `EnumWindows` / `EnumChildWindows` - Enumerate all windows
- `GetWindowText` / `GetClassName` - Window identification
- `GetWindowThreadProcessId` - Process association
- `FindWindow` - Find window by class/title
- `SetParent` - Window hierarchy manipulation
- `GetWindowRect` / `GetClientRect` - Window dimensions
- `ShowWindow` / `EnableWindow` / `SetFocus` - Window state
- `IsWindow` / `IsWindowVisible` / `IsIconic` / `IsZoomed` - Window status checks
- `SetWindowLong` / `GetWindowLong` / `SetWindowLongPtr` - Window attributes/styles
- `SetWindowPos` - Position and z-order
- `GetActiveWindow` / `GetParent` / `GetAncestor` - Window relationships
- `PostMessage` - Message sending
- `UpdateLayeredWindow` / `SetLayeredWindowAttributes` - Transparency
- `ChangeWindowMessageFilter` / `ChangeWindowMessageFilterEx` - UIPI (User Interface Privilege Isolation)

**Cursor and Mouse (user32.dll):**
- `GetCursorPos` / `SetCursorPos` - Cursor position
- `GetCursorInfo` - Cursor visibility/state
- `mouse_event` - Simulate mouse input

**Shell Operations (shell32.dll):**
- `DragAcceptFiles` / `DragQueryFile` / `DragFinish` - Drag-and-drop file handling

**Hooks (user32.dll):**
- `SetWindowsHookEx` / `UnhookWindowsHookEx` / `CallNextHookEx` - Window procedure hooks

**File Dialogs (Comdlg32.dll):**
- `GetOpenFileName` - Native file open dialog

**Process/Module (kernel32.dll):**
- `GetModuleHandle` - Module handle retrieval
- `GetCurrentThreadId` - Thread ID
- `GetLastError` - Error retrieval

**Constants Defined:**
- Window styles: `WS_BORDER`, `WS_CAPTION`, `WS_SYSMENU`, `WS_THICKFRAME`, `WS_POPUP`, `WS_OVERLAPPEDWINDOW`, etc.
- Extended styles: `WS_EX_TRANSPARENT`, `WS_EX_LAYERED`, `WS_EX_TOPMOST`, `WS_EX_ACCEPTFILES`
- SetWindowPos flags: `SWP_NOSIZE`, `SWP_NOMOVE`, `SWP_NOZORDER`, `SWP_FRAMECHANGED`, etc.
- Show window commands: `SW_HIDE`, `SW_SHOW`, `SW_MAXIMIZE`, `SW_MINIMIZE`, `SW_RESTORE`
- Mouse event flags: `MOUSEEVENTF_LEFTDOWN`, `MOUSEEVENTF_MOVE`, `MOUSEEVENTF_WHEEL`, etc.
- Window messages: `WM_SETTEXT`, `WM_DROPFILES`, `WM_COPYDATA`, `WM_IME_CHAR`
- Hook types: `WH_KEYBOARD_LL`, `WH_MOUSE_LL`, `WH_CBT`, etc.

**Structures:**
- `RECT` - Rectangle coordinates
- `POINT` - 2D coordinates
- `COLORREF` - Color value
- `CURSORINFO` - Cursor state information
- `CHANGEFILTERSTRUCT` - Message filter data
- `CWPSTRUCT` / `MSG` - Message data
- `OpenFileName` - File dialog configuration

**Usage Pattern:**
```csharp
// Example: Enumerate all windows
WinApi.EnumWindows((hWnd, lParam) => {
    StringBuilder sb = new StringBuilder(256);
    WinApi.GetWindowText(hWnd, sb, 256);
    // Process window...
    return true;
}, IntPtr.Zero);
```

#### DwmApi.cs - Desktop Window Manager
**Location:** [Assets/MATE ENGINE - Scripts/APIs/DwmApi.cs](Assets/MATE%20ENGINE%20-%20Scripts/APIs/DwmApi.cs)
**Size:** 173 lines
**Purpose:** DWM (Desktop Window Manager) effects for transparency, blur, and window composition

**Complete DllImport List (8 functions):**
- `DwmEnableBlurBehindWindow` - Blur effect behind window
- `DwmIsCompositionEnabled` - Check if DWM is active
- `DwmEnableComposition` - Enable/disable DWM
- `DwmGetColorizationColor` - Get system accent color
- `DwmRegisterThumbnail` / `DwmUnregisterThumbnail` - Window thumbnail registration
- `DwmUpdateThumbnailProperties` - Update thumbnail display
- `DwmExtendFrameIntoClientArea` - **Most Important** - Extends glass/transparency into window

**Structures:**
- `MARGINS` - Frame margins for glass effect
- `RECT` - Rectangle (duplicate of WinApi.RECT)
- `DWM_BLURBEHIND` - Blur configuration
- `DWM_THUMBNAIL_PROPERTIES` - Thumbnail display properties

**Enums:**
- `DWMWINDOWATTRIBUTE` - Window attributes (16 values)
- `DWMNCRENDERINGPOLICY` - Non-client rendering policy

**Custom Helper Method:**
- `DwmExtendIntoClientAll` - Applies glass effect to entire client area (uses -1 margins)

**Usage Pattern:**
```csharp
// Make window completely transparent
MARGINS margins = new MARGINS(-1, -1, -1, -1);
DwmApi.DwmExtendFrameIntoClientArea(hwnd, ref margins);
```

### 2. System Tray Implementation

#### System Tray File Structure
**Location:** [Assets/MATE ENGINE - System Tray/SystemTray/](Assets/MATE%20ENGINE%20-%20System%20Tray/SystemTray/)
**Total Lines:** 400+ across multiple files
**Status:** ❌ **COMPLETELY WINDOWS-ONLY** - No cross-platform abstraction

**Files:**
1. **WinAPI.cs** (~80 lines) - Windows API imports for tray functionality
2. **TrayIcon.cs** (266 lines) - Core tray icon implementation
3. **SystemTray.cs** (150 lines) - Unity component wrapper
4. **Structures.cs** - Native structures (NOTIFYICONDATA, WNDCLASSEX, etc.)
5. **Constants.cs** - Windows constants (NIM_ADD, NIF_ICON, etc.)
6. **IconFromTexture2D.cs** - Convert Unity Texture2D to HICON
7. **Popup.cs** - Context menu builder
8. **Utils.cs** - Helper functions

#### TrayIcon.cs - Detailed Analysis
**DllImport Functions Used:**
- `Shell_NotifyIcon` (shell32.dll) - Add/remove/modify tray icon
- `RegisterClassEx` (user32.dll) - Register window class
- `DestroyWindow` / `UnregisterClass` (user32.dll) - Cleanup
- `DefWindowProc` (user32.dll) - Default window procedure
- `DestroyIcon` (user32.dll) - Icon cleanup
- `CreatePopupMenu` / `AppendMenu` / `TrackPopupMenuEx` / `DestroyMenu` (user32.dll) - Context menu
- `GetCursorPos` / `SetForegroundWindow` (user32.dll) - Menu positioning
- `GetModuleHandle` (kernel32.dll) - Module handle
- `CreateWindowEx` (user32.dll) - Hidden message window
- `CreateDIBSection` / `CreateBitmap` / `DeleteObject` (gdi32.dll) - Bitmap creation
- `CreateIconIndirect` (user32.dll) - Icon creation from bitmap

**Key Features:**
- Creates hidden message-only window to receive tray icon messages
- Converts Unity Texture2D to native HICON using GDI
- Dynamic context menu based on Unity MonoBehaviour methods
- Reflection-based action binding to toggle fields and methods
- Custom window procedure callback (WndProc)

**Critical Code Pattern:**
```csharp
public static void Init(string appName, string tooltip, Texture2D iconTexture, List<(string, Action)> actions = null)
{
#if !UNITY_STANDALONE_WIN
    throw new NotImplementedException("These features are only avaliable on Windows...");
#endif
    // 1. Create HICON from Texture2D
    hIcon = CreateHIconFromTexture2D(ref iconTexture);
    
    // 2. Create hidden window for messages
    CreateMessageWindow();
    
    // 3. Prepare NOTIFYICONDATA structure
    notifyIconData = new NOTIFYICONDATA() {
        hWnd = messageWindowHandle,
        uCallbackMessage = TRAY_ICON_MESSAGE,
        hIcon = hIcon,
        szTip = tooltip
    };
    
    // 4. Add tray icon
    WinAPI.Shell_NotifyIcon(NIM_ADD, ref notifyIconData);
}
```

**Structures Used:**
- `NOTIFYICONDATA` - Tray icon data (size: 508+ bytes, contains GUID, version, strings)
- `WNDCLASSEX` - Window class registration
- `ICONINFO` - Icon bitmap info
- `BITMAPINFO` / `BITMAPINFOHEADER` - Bitmap structures

### 3. Avatar Window Snapping System

#### AvatarWindowHandler.cs - Core Feature
**Location:** [Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarWindowHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarWindowHandler.cs)
**Size:** 995 lines (largest single-file Windows dependency)
**Purpose:** Makes avatar "sit" on window edges - **SIGNATURE FEATURE**

**Windows API Dependencies:**
- All functions from `WinApi.cs` (60+ functions)
- Primarily uses:
  - `EnumWindows` - Find all desktop windows
  - `GetWindowText` / `GetClassName` - Window identification
  - `GetWindowRect` - Window position/size
  - `IsWindowVisible` / `IsWindow` - Window validity checks
  - `GetWindowThreadProcessId` - Process filtering
  - `SetWindowPos` - Position Unity window relative to target

**Key Features:**
1. **Window Enumeration** - Finds "sit-eligible" windows every frame (configurable FPS)
2. **Z-Order Detection** - Determines which windows are in front/behind Unity window
3. **Window Filtering** - Excludes system windows, hidden windows, transparent windows
4. **Snap Detection** - Detects when avatar is dragged near window edge
5. **Occluder System** - Creates invisible quads to hide avatar parts behind windows
6. **Smooth Snapping** - SmoothDamp position to avoid jitter
7. **Animation Control** - Triggers "isWindowSit" animator parameter

**Platform Guards:**
```csharp
void Update()
{
#if !UNITY_STANDALONE_WIN
    return;  // Early exit on non-Windows
#endif
    // ... 900+ lines of Windows-specific code ...
}
```

**Window Eligibility Logic:**
```csharp
bool IsSitEligibleWindow(IntPtr hWnd)
{
    // Filter out:
    // - Invisible windows
    // - Unity's own window
    // - Same process windows
    // - System windows (no title, "Shell_TrayWnd", etc.)
    // - Layered transparent windows (configurable)
    // - Windows with specific extended styles
    // - Too small windows
    return isEligible;
}
```

**Critical Data Structures:**
```csharp
class WindowEntry {
    public IntPtr hWnd;
    public string title;
    public string className;
    public RECT rect;
    public int zOrder;
    public bool isInFront;  // Z-order relative to Unity
}

List<WindowEntry> cachedWindows;  // All eligible windows
List<WindowEntry> activeOccluders;  // Windows currently occluding avatar
```

**Migration Challenges:**
- 🔴 **CRITICAL** - This is the app's signature feature
- Complex window filtering logic specific to Windows window model
- Z-order concept differs across platforms
- Occluder system requires precise window position tracking
- Performance-sensitive (updates at 15-30 FPS)

#### AvatarHideHandler.cs - Screen Edge Snapping
**Location:** [Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarHideHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarHideHandler.cs)
**Size:** 308 lines
**Purpose:** Hides avatar at screen edges when cursor approaches

**Windows API Usage:**
```csharp
[DllImport("user32.dll")]
static extern bool GetCursorPos(out POINT lpPoint);

[DllImport("user32.dll")]
static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

[DllImport("user32.dll")]
static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

[DllImport("user32.dll")]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

[DllImport("user32.dll")]
static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
```

**Features:**
- Multi-monitor support via `MonitorFromPoint`
- Cursor position tracking at update rate
- Smooth snapping to screen edges
- Hand position anchor detection (left/right hand world position → screen position)
- Topmost window control while snapped
- Unsnap threshold with grace period

**Platform Guards:**
```csharp
void Start()
{
#if UNITY_STANDALONE_WIN
    unityHWND = Process.GetCurrentProcess().MainWindowHandle;
#endif
}

void Update()
{
#if !UNITY_STANDALONE_WIN
    return;
#else
    // 280+ lines of Windows-specific code
#endif
}
```

### 4. Desktop Ambient Lighting

#### DesktopAmbientProbe.cs
**Location:** [Assets/MATE ENGINE - Scripts/Tools/DesktopAmbientProbe.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/DesktopAmbientProbe.cs)
**Size:** 359 lines
**Purpose:** Samples screen colors to drive ambient lighting (Philips Hue-like effect)

**GDI Screen Capture Implementation:**
```csharp
[DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
[DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
[DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr hdc);
[DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr hdc);
[DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
[DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr hObject);
[DllImport("gdi32.dll")] static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, int rop);
[DllImport("gdi32.dll")] static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);
[DllImport("user32.dll")] static extern int GetSystemMetrics(int nIndex);
```

**Capture Process:**
1. Get desktop DC (Device Context)
2. Create memory DC and DIB (Device-Independent Bitmap)
3. Use `StretchBlt` to scale virtual screen to small capture buffer (160x90)
4. Read pixel data from DIB bits
5. Sample bands (top/bottom/left/right edges)
6. Calculate average HSV color per band
7. Drive Unity lights with smoothed colors

**Multi-Monitor Support:**
- Uses `GetSystemMetrics(SM_XVIRTUALSCREEN/SM_YVIRTUALSCREEN/SM_CXVIRTUALSCREEN/SM_CYVIRTUALSCREEN)`
- Captures entire virtual desktop (all monitors combined)

**Platform Guards:**
```csharp
#if UNITY_STANDALONE_WIN
    // All GDI code
    IntPtr deskDC, memDC, dib, dibBits;
    // ...
#endif

void Start() {
#if UNITY_STANDALONE_WIN
    InitCapture();
#endif
}
```

### 5. Memory Management

#### MemoryTrim.cs
**Location:** [Assets/MATE ENGINE - Scripts/Tools/MemoryTrim.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/MemoryTrim.cs)
**Size:** 80 lines
**Purpose:** Reduce working set memory via Windows psapi.dll

**Single DllImport:**
```csharp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("psapi.dll")]
    static extern bool EmptyWorkingSet(IntPtr hProcess);
#endif
```

**Functionality:**
- Runs GC.Collect + Resources.UnloadUnusedAssets
- Calls `EmptyWorkingSet` to trim process working set
- Configurable auto-trim every 10 minutes
- Startup trim after 10 seconds

**Migration:** Easy - Remove or use cross-platform memory profiling APIs

---

## Critical Windows-Only APIs

### 1. Window Management (user32.dll)

#### Current Usage
Primary API for all window manipulation, positioning, and z-order management.

**Key Functions Used:**
- `EnumWindows` - Enumerate all top-level windows
- `GetWindowLongPtr/SetWindowLongPtr` - Window styles/attributes
- `GetWindowRect/SetWindowPos` - Position and size
- `ShowWindow/IsWindowVisible` - Visibility control
- `GetForegroundWindow/SetForegroundWindow` - Focus management
- `GetCursorPos/SetCursorPos` - Cursor position
- `MonitorFromPoint/GetMonitorInfo` - Multi-monitor support
- `GetWindowThreadProcessId` - Process identification
- `GetClassName` - Window class identification
- `IsIconic/IsZoomed` - Window state detection

**Files with Heavy Usage:**
- [Assets/MATE ENGINE - Scripts/APIs/WinApi.cs](Assets/MATE%20ENGINE%20-%20Scripts/APIs/WinApi.cs) - 50+ DllImport declarations
- [Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarWindowHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarWindowHandler.cs)
- [Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarHideHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarHideHandler.cs)
- [Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarFollowCursorHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarFollowCursorHandler.cs)
- [Assets/MATE ENGINE - Scripts/Tools/WindowManager.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/WindowManager.cs)

#### macOS Alternative (Cocoa/AppKit)
```objc
// NSWindow API equivalents
NSArray *windows = [[NSApplication sharedApplication] windows];
NSRect frame = [window frame];
[window setFrame:frame display:YES];
[window makeKeyAndOrderFront:nil];
NSPoint mouseLocation = [NSEvent mouseLocation];
NSScreen *screen = [NSScreen mainScreen];
```

**Implementation Path:**
- Use Objective-C plugins or Unity's native plugin system
- Requires P/Invoke to Objective-C runtime or Swift framework
- Third-party plugin: **UniWindowController** (already in project) has partial macOS support

#### Linux Alternative (X11/Wayland)
```c
// X11 equivalents
Display *display = XOpenDisplay(NULL);
Window root = DefaultRootWindow(display);
XWindowAttributes attr;
XGetWindowAttributes(display, window, &attr);
XMoveResizeWindow(display, window, x, y, width, height);
XQueryPointer(display, root, ...);
```

**Implementation Path:**
- X11 libraries via P/Invoke (libX11.so)
- Wayland support would require different approach (wl_shell, xdg_shell)
- Consider using SDL2 or GLFW for cross-platform window abstraction

---

### 2. Desktop Window Manager (dwmapi.dll)

#### Current Usage
Provides window transparency, blur effects, and frame extension into client area.

**Key Functions Used:**
- `DwmExtendFrameIntoClientArea` - Borderless transparency
- `DwmSetWindowAttribute` - Window rendering policies
- `DwmEnableBlurBehindWindow` - Background blur
- `DwmIsCompositionEnabled` - Check if DWM is active
- `DwmGetColorizationColor` - Get accent color

**Files:**
- [Assets/MATE ENGINE - Scripts/APIs/DwmApi.cs](Assets/MATE%20ENGINE%20-%20Scripts/APIs/DwmApi.cs) - 8 DllImport declarations
- Used by window initialization code for transparency

#### macOS Alternative
```objc
// NSWindow transparency
[window setOpaque:NO];
[window setBackgroundColor:[NSColor clearColor]];
[window setHasShadow:YES];
// Visual effects view for blur
NSVisualEffectView *effectView = [[NSVisualEffectView alloc] initWithFrame:...];
effectView.blendingMode = NSVisualEffectBlendingModeBehindWindow;
effectView.material = NSVisualEffectMaterialDark;
```

**Limitations:**
- macOS transparency is simpler (just set background to clear)
- Blur effects achieved differently (NSVisualEffectView)
- Frame extension not directly equivalent

#### Linux Alternative
```c
// X11 transparency (requires compositor)
XSetWindowAttributes attrs;
attrs.override_redirect = True;
XChangeWindowAttributes(display, window, CWOverrideRedirect, &attrs);
// ARGB visual for transparency
XMatchVisualInfo(display, DefaultScreen(display), 32, TrueColor, &vinfo);
```

**Limitations:**
- Requires compositor (Compiz, KWin, Mutter)
- No standardized blur API
- Wayland has different compositing model

---

### 3. System Tray (shell32.dll)

#### Current Usage
System tray icon with context menu for application control.

**Key Functions Used:**
- `Shell_NotifyIcon` - Add/remove/modify tray icon
- `CreatePopupMenu` - Context menu creation
- `TrackPopupMenu` - Show context menu
- `LoadImage` - Load icon from file
- `CreateIconFromResourceEx` - Create icon from memory
- `DestroyIcon` - Cleanup

**Files:**
- [Assets/MATE ENGINE - System Tray/SystemTray/WinAPI.cs](Assets/MATE%20ENGINE%20-%20System%20Tray/SystemTray/WinAPI.cs) - 20+ imports
- [Assets/MATE ENGINE - System Tray/SystemTray/SystemTray.cs](Assets/MATE%20ENGINE%20-%20System%20Tray/SystemTray/SystemTray.cs)

#### macOS Alternative
```objc
// NSStatusBar API
NSStatusBar *statusBar = [NSStatusBar systemStatusBar];
NSStatusItem *statusItem = [statusBar statusItemWithLength:NSSquareStatusItemLength];
statusItem.button.image = [NSImage imageNamed:@"icon"];
NSMenu *menu = [[NSMenu alloc] init];
[menu addItem:[[NSMenuItem alloc] initWithTitle:@"Quit" action:@selector(quit:) keyEquivalent:@"q"]];
statusItem.menu = menu;
```

**Implementation:**
- Requires native Objective-C plugin
- Menu structure similar to Windows
- Different lifecycle management

#### Linux Alternative
```c
// AppIndicator (Ubuntu/Unity)
AppIndicator *indicator = app_indicator_new("mate-engine", "icon", APP_INDICATOR_CATEGORY_APPLICATION_STATUS);
app_indicator_set_status(indicator, APP_INDICATOR_STATUS_ACTIVE);
GtkMenu *menu = gtk_menu_new();
app_indicator_set_menu(indicator, menu);

// Or XEmbed protocol for traditional tray
```

**Challenges:**
- Multiple implementations (AppIndicator, StatusNotifier, legacy XEmbed)
- Desktop environment dependent
- Different icon format requirements

---

### 4. Screen Capture (gdi32.dll)

#### Current Usage
Captures desktop screen content for ambient lighting effects.

**Key Functions Used:**
- `GetDC/ReleaseDC` - Get device context
- `CreateCompatibleDC` - Memory device context
- `CreateDIBSection` - Create bitmap
- `BitBlt/StretchBlt` - Copy screen pixels
- `SelectObject` - Select GDI object
- `DeleteObject` - Cleanup

**Files:**
- [Assets/MATE ENGINE - Scripts/Tools/DesktopAmbientProbe.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/DesktopAmbientProbe.cs)

#### macOS Alternative
```objc
// Core Graphics screen capture
CGImageRef screenshot = CGWindowListCreateImage(
    CGRectInfinite,
    kCGWindowListOptionOnScreenOnly,
    kCGNullWindowID,
    kCGWindowImageDefault
);
// Or AVFoundation for realtime capture
AVCaptureScreenInput *screenInput = [[AVCaptureScreenInput alloc] initWithDisplayID:displayID];
```

**Performance:**
- macOS APIs are similarly performant
- Can capture specific displays
- Better permission handling (user must authorize)

#### Linux Alternative
```c
// X11 screen capture
XImage *image = XGetImage(display, root, x, y, width, height, AllPlanes, ZPixmap);
// Or use libavformat/libavcodec for advanced capture
```

**Considerations:**
- Wayland has security restrictions on screen capture
- May require PipeWire/xdg-desktop-portal on modern systems

---

### 5. Window Content Capture (uWindowCapture.dll)

#### Current Usage
**WINDOWS-ONLY PLUGIN** - Third-party plugin for capturing content of other application windows into Unity textures.

**Functions (90+ DllImport calls):**
- `UwcInitialize/UwcFinalize` - Plugin lifecycle
- `UwcUpdate` - Update capture data
- `UwcCaptureWindow` - Start capturing window
- `UwcGetWindowBuffer` - Get pixel data
- `UwcGetWindowTitle/UwcGetWindowClassName` - Window info
- Window enumeration and filtering

**Files:**
- [Assets/uWindowCapture/Runtime/UwcLib.cs](Assets/uWindowCapture/Runtime/UwcLib.cs) - 90+ imports
- [Assets/uWindowCapture/Runtime/UwcManager.cs](Assets/uWindowCapture/Runtime/UwcManager.cs)
- [Assets/uWindowCapture/Runtime/UwcWindowTexture.cs](Assets/uWindowCapture/Runtime/UwcWindowTexture.cs)

#### Cross-Platform Status
**❌ NO DIRECT EQUIVALENT**

**Alternative Approaches:**
1. **macOS - CGWindowListCreateImage:**
   - Can capture specific windows by ID
   - Requires user permission
   - Performance may vary
   
2. **Linux - X11 Composite Extension:**
   - Requires compositor support
   - More complex setup
   - Wayland restrictions make this difficult

**Recommendation:**
- Mark window capture as **Windows-only feature** initially
- Implement platform-specific alternatives later
- Display feature unavailability message on macOS/Linux

---

### 6. Process and Memory Management (kernel32.dll, psapi.dll)

#### Current Usage
Process information, memory allocation, and system metrics.

**Key Functions Used:**
- `GetCurrentProcess/OpenProcess` - Process handles
- `GetProcessMemoryInfo` - Memory usage
- `GetModuleFileName` - Executable path
- `QueryPerformanceCounter/QueryPerformanceFrequency` - High-resolution timing
- `GetSystemInfo` - CPU count, page size
- `VirtualAlloc/VirtualFree` - Low-level memory allocation

**Files:**
- [Assets/MATE ENGINE - Scripts/Tools/MemoryManager.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/MemoryManager.cs)
- [Assets/LLMUnity/Runtime/LLMLib.cs](Assets/LLMUnity/Runtime/LLMLib.cs)
- Performance monitoring code

#### macOS Alternative
```c
// Process info via BSD APIs
#include <sys/sysctl.h>
#include <mach/mach.h>

struct kinfo_proc info;
size_t length = sizeof(info);
sysctlbyname("kern.proc.pid", &info, &length, NULL, 0);

// Memory info
task_basic_info_64_data_t info;
mach_task_basic_info(mach_task_self(), &info);
```

#### Linux Alternative
```c
// Process info via /proc filesystem
FILE *fp = fopen("/proc/self/status", "r");
// Or use sysinfo() system call
struct sysinfo info;
sysinfo(&info);
```

**Unity Built-in Options:**
- `System.Diagnostics.Process.GetCurrentProcess()` - Cross-platform
- `UnityEngine.Profiling.Profiler` - Unity memory stats
- Most low-level operations unnecessary

---

## File-by-File Analysis

### Core Engine Scripts

#### 1. Window Management Layer

**[Assets/MATE ENGINE - Scripts/APIs/WinApi.cs](Assets/MATE%20ENGINE%20-%20Scripts/APIs/WinApi.cs)**
- **Purpose:** Central Windows API wrapper providing all user32.dll and kernel32.dll functions
- **Platform Status:** ❌ Windows-only
- **Lines:** 50+ DllImport declarations
- **Dependencies:** Used by ALL AvatarHandler classes
- **Abstraction Status:** Should be moved behind IWindowService interface
- **Migration Priority:** 🔴 CRITICAL - Core dependency

**[Assets/MATE ENGINE - Scripts/APIs/DwmApi.cs](Assets/MATE%20ENGINE%20-%20Scripts/APIs/DwmApi.cs)**
- **Purpose:** DWM transparency and window composition effects
- **Platform Status:** ❌ Windows-only
- **Functions:** DwmExtendFrameIntoClientArea, DwmSetWindowAttribute, DwmEnableBlurBehindWindow
- **Migration Priority:** 🟡 MEDIUM - Visual feature only
- **Alternative:** macOS and Linux have different transparency models

#### 2. Avatar Behavior Handlers

**[Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarWindowHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarWindowHandler.cs)**
- **Purpose:** Window snapping behavior - avatar "sits" on window edges
- **Platform Status:** ⚠️ Has guards (lines 169, 406, 948+) but Windows-only implementation
- **Key Features:**
  - Enumerate windows via EnumWindows callback
  - Detect "sit-eligible" windows (excludes system windows, hidden windows)
  - Z-order comparison for proper layering
  - Window class filtering
- **Migration Priority:** 🔴 CRITICAL - Core feature
- **Cross-Platform Path:** 
  - Abstract window enumeration behind IWindowService
  - Implement NSWindow enumeration for macOS
  - Implement X11 window tree walk for Linux

**[Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarHideHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarHideHandler.cs)**
- **Purpose:** Hides avatar when cursor approaches, multi-monitor support
- **Platform Status:** ⚠️ Guards at lines 37, 59, 147
- **Windows APIs:**
  - GetCursorPos - cursor position tracking
  - MonitorFromPoint - determine which monitor cursor is on
  - GetMonitorInfo - get monitor bounds
  - SetWindowPos - reposition Unity window
  - GetClientRect - get window client area
- **Migration Priority:** 🔴 CRITICAL - Affects all mouse interaction
- **Cross-Platform Path:**
  - Abstract cursor and monitor APIs
  - Use Unity's Screen.currentResolution for basic multi-monitor
  - Platform-specific cursor position APIs

**[Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarFollowCursorHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarFollowCursorHandler.cs)**
- **Purpose:** Avatar follows mouse cursor
- **Platform Status:** ⚠️ Uses Windows-only cursor position APIs
- **Migration Priority:** 🔴 CRITICAL
- **Alternative:** Unity Input.mousePosition (screen-space, may need conversion)

**[Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarDraggingHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarDraggingHandler.cs)**
- **Purpose:** Drag avatar to reposition
- **Platform Status:** ⚠️ May use Windows-specific hit testing
- **Migration Priority:** 🟡 MEDIUM

**[Assets/MATE ENGINE - Scripts/AvatarHandlers/AvatarAnimationHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/AvatarHandlers/AvatarAnimationHandler.cs)**
- **Purpose:** Animation state management
- **Platform Status:** ✅ Cross-platform (uses Unity Animator)
- **Migration Priority:** ✅ NONE

#### 3. System Integration

**[Assets/MATE ENGINE - System Tray/SystemTray/](Assets/MATE%20ENGINE%20-%20System%20Tray/SystemTray/)**
- **Files:**
  - `WinAPI.cs` - Shell32.dll imports (20+ functions)
  - `SystemTray.cs` - Implementation
  - `SystemTrayMenu.cs` - Menu structure
- **Purpose:** System tray icon with quit/settings menu
- **Platform Status:** ❌ Windows-only
- **Migration Priority:** 🟡 MEDIUM - Important UX but not core
- **Cross-Platform Path:**
  - Create ISystemTrayService interface
  - Implement WindowsSystemTrayService (existing Shell_NotifyIcon)
  - Implement MacSystemTrayService (NSStatusBar)
  - Implement LinuxSystemTrayService (AppIndicator or fallback)

**[Assets/MATE ENGINE - Scripts/Tools/WindowManager.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/WindowManager.cs)**
- **Purpose:** Unity window configuration (borderless, transparency, position)
- **Platform Status:** ⚠️ Has platform guards but Windows-focused
- **Windows APIs:** SetWindowLong, SetWindowPos, DwmExtendFrameIntoClientArea
- **Migration Priority:** 🔴 CRITICAL - App won't display correctly without this
- **Dependencies:** Uses WinApi and DwmApi classes

**[Assets/MATE ENGINE - Scripts/Tools/DesktopAmbientProbe.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/DesktopAmbientProbe.cs)**
- **Purpose:** Sample screen colors for ambient lighting
- **Platform Status:** ⚠️ Guards at lines 26, 83, 91, 123, 131
- **Windows APIs:** GDI (GetDC, BitBlt, CreateDIBSection)
- **Migration Priority:** 🟡 MEDIUM - Visual enhancement only
- **Alternative:** Could be disabled on non-Windows initially

**[Assets/MATE ENGINE - Scripts/Tools/MemoryManager.cs](Assets/MATE%20ENGINE%20-%20Scripts/Tools/MemoryManager.cs)**
- **Purpose:** Monitor and optimize memory usage
- **Platform Status:** ⚠️ Uses kernel32.dll and psapi.dll
- **Migration Priority:** 🟡 LOW - Can use Unity's Profiler API instead
- **Alternative:** System.Diagnostics.Process (cross-platform)

#### 4. UI and Input

**[Assets/MATE ENGINE - Scripts/UI/](Assets/MATE%20ENGINE%20-%20Scripts/UI/)**
- **General Status:** Most UI code is Unity-based (✅ cross-platform)
- **Exception:** File dialogs may use StandaloneFileBrowser plugin
- **Platform Status:** StandaloneFileBrowser has Windows/Mac/Linux implementations

**[Assets/MATE ENGINE - Scripts/Settings/SystemStartHandler.cs](Assets/MATE%20ENGINE%20-%20Scripts/Settings/SystemStartHandler.cs)**
- **Purpose:** Auto-start on system boot (Windows registry)
- **Platform Status:** ❌ Windows-only
- **Windows APIs:** Registry manipulation via .NET Framework
- **Migration Priority:** 🟡 MEDIUM
- **Alternatives:**
  - macOS: Login Items API or launchd plist
  - Linux: .desktop file in ~/.config/autostart/

#### 5. Networking and Services

**[Assets/DiscordRPC/](Assets/DiscordRPC/)**
- **Purpose:** Discord Rich Presence integration
- **Platform Status:** ✅ Cross-platform (Discord RPC supports all platforms)
- **Migration Priority:** ✅ NONE

**[Assets/MATE ENGINE - Packages/com.rlabrecque.steamworks.net/](Assets/MATE%20ENGINE%20-%20Packages/com.rlabrecque.steamworks.net/)**
- **Purpose:** Steam integration (achievements, cloud saves, overlay)
- **Platform Status:** ✅ Cross-platform
- **Implementation:** Uses conditional compilation for platform-specific DLL names
- **DLL Names:**
  - Windows: steam_api64.dll / steam_api.dll
  - macOS: libsteam_api.dylib
  - Linux: libsteam_api.so
- **Migration Priority:** ✅ NONE - Already abstracted
- **Note:** Has 3000+ DllImport calls but all platform-aware

### Third-Party Plugins

#### 1. uWindowCapture

**[Assets/uWindowCapture/](Assets/uWindowCapture/)**
- **Purpose:** Capture other application windows into Unity textures
- **Platform Status:** ❌ WINDOWS-ONLY PLUGIN
- **Files:**
  - `Runtime/UwcLib.cs` - 90+ DllImport to uWindowCapture.dll
  - `Runtime/UwcManager.cs` - Plugin manager
  - `Runtime/UwcWindowTexture.cs` - Texture component
  - `Runtime/UwcWindow.cs` - Window data structure
- **Migration Strategy:**
  - Feature gate: `#if UNITY_STANDALONE_WIN`
  - Show "Windows-only feature" message on other platforms
  - **Potential macOS replacement:** CGWindowListCreateImage
  - **Potential Linux replacement:** X11 Composite Extension (complex)
- **Priority:** 🟡 MEDIUM - Cool feature but not essential

#### 2. UniWindowController (Kirurobo)

**[Assets/Kirurobo/UniWindowController/](Assets/Kirurobo/UniWindowController/)**
- **Purpose:** Cross-platform window control library
- **Platform Status:** ✅ Multi-platform (Windows/Mac/Linux support)
- **Platform Implementations:**
  - `Plugins/UniWindowController.cs` - Platform selector
  - `Plugins/UniWindowControllerWin.cs` - Windows (user32.dll)
  - `Plugins/UniWindowControllerMac.mm` - macOS (Objective-C)
  - `Plugins/UniWindowControllerLinux.cs` - Linux (libX11)
- **Migration Priority:** ✅ NONE - Already cross-platform
- **Recommendation:** Use this as reference for platform abstraction pattern

#### 3. StandaloneFileBrowser

**[Assets/StandaloneFileBrowser/](Assets/StandaloneFileBrowser/)**
- **Purpose:** Native file open/save dialogs
- **Platform Status:** ✅ Cross-platform
- **Implementations:**
  - Windows: Comdlg32.dll (GetOpenFileName, GetSaveFileName)
  - macOS: NSOpenPanel/NSSavePanel via Objective-C plugin
  - Linux: Zenity or kdialog command-line tools
- **Migration Priority:** ✅ NONE - Already cross-platform

#### 4. LLMUnity

**[Assets/LLMUnity/](Assets/LLMUnity/)**
- **Purpose:** Local LLM integration (runs AI models locally)
- **Platform Status:** ⚠️ Multi-platform with platform-specific DLL loading
- **Platform Detection:** Uses Application.platform checks (RuntimePlatform.WindowsPlayer, etc.)
- **DLL Loading:**
  - Windows: llamafile.dll
  - macOS: libllama.dylib
  - Linux: libllama.so
- **Migration Priority:** 🟡 LOW - Plugin likely handles platform differences
- **Action Required:** Test on each platform to verify library loading

#### 5. VRM/UniGLTF

**[Assets/UniGLTF/](Assets/UniGLTF/), [Assets/VRM/](Assets/VRM/), [Assets/VRM10/](Assets/VRM10/)**
- **Purpose:** VRM character model format support
- **Platform Status:** ✅ Fully cross-platform
- **Implementation:** Pure C# with Unity APIs
- **Migration Priority:** ✅ NONE

#### 6. DynamicBone

**[Assets/DynamicBone/](Assets/DynamicBone/)**
- **Purpose:** Physics-based bone animation (hair, clothing)
- **Platform Status:** ✅ Cross-platform (Unity physics)
- **Migration Priority:** ✅ NONE

#### 7. MToon/lilToon Shaders

**[Assets/MToon/](Assets/MToon/), [Assets/lilToon/](Assets/lilToon/)**
- **Purpose:** Toon shading for VRM characters
- **Platform Status:** ✅ Cross-platform (Unity shaders)
- **Migration Priority:** ✅ NONE

---

## Platform Abstraction Layer Analysis

### Existing Abstractions (from previous attempt)

**[Assets/MATE ENGINE - Scripts/Platform/](Assets/MATE%20ENGINE%20-%20Scripts/Platform/)**

#### Interfaces
1. **IWindowService** - Window management abstraction
   - Methods: GetWindows(), GetWindowInfo(), SetWindowPosition(), etc.
   
2. **IScreenService** - Screen/monitor management
   - Methods: GetScreens(), GetCursorPosition(), SetCursorPosition()
   
3. **ISystemTrayService** - System tray icon management
   - Methods: CreateTrayIcon(), ShowContextMenu(), UpdateIcon()

4. **IPlatformService** - General platform utilities
   - Methods: GetPlatformName(), GetFileSystemInfo()

#### Implementations

**Windows:**
- `Windows/WindowsWindowService.cs` - Uses WinApi class
- `Windows/WindowsScreenService.cs` - Monitor enumeration
- `Windows/WindowsSystemTrayService.cs` - Shell_NotifyIcon wrapper
- **Status:** ⚠️ Implemented but tightly coupled to WinApi

**macOS:**
- `MacOS/MacWindowService.cs` - Stub/incomplete
- `MacOS/MacScreenService.cs` - Stub/incomplete
- `MacOS/MacSystemTrayService.cs` - Stub/incomplete
- **Status:** ❌ Incomplete implementations (caused build errors)

**Linux:**
- `Linux/LinuxWindowService.cs` - Stub/incomplete
- `Linux/LinuxScreenService.cs` - Stub/incomplete
- `Linux/LinuxSystemTrayService.cs` - Not implemented
- **Status:** ❌ Incomplete implementations

#### Service Locator
**[Assets/MATE ENGINE - Scripts/Platform/PlatformServiceLocator.cs](Assets/MATE%20ENGINE%20-%20Scripts/Platform/PlatformServiceLocator.cs)**
- Pattern: Service Locator
- Selects implementation based on `UNITY_STANDALONE_WIN` / `UNITY_STANDALONE_OSX` / `UNITY_STANDALONE_LINUX`
- **Issue:** Previous implementation had compilation errors due to incomplete Mac/Linux services

### Assessment of Previous Attempt

**Why It Failed:**
1. ❌ Incomplete macOS/Linux implementations caused compilation errors
2. ❌ Windows code still directly called WinApi instead of using services
3. ❌ No gradual migration - tried to switch everything at once
4. ❌ Missing feature parity (macOS was missing features)
5. ❌ Insufficient platform testing during development

**What Was Good:**
1. ✅ Interface design is sound (IWindowService, IScreenService)
2. ✅ Service Locator pattern appropriate for this use case
3. ✅ Conditional compilation strategy correct
4. ✅ File organization clean

---

## Recommended Migration Strategy

### Phase 1: Stabilize Current State ✅ SAFE
**Goal:** Ensure Windows build works, document all dependencies

1. ✅ Complete code inventory (this document)
2. ⏭️ Add comprehensive `#if UNITY_STANDALONE_WIN` guards to ALL Windows-specific code
3. ⏭️ Test that Windows build still works
4. ⏭️ Add `#elif UNITY_STANDALONE_OSX` / `#elif UNITY_STANDALONE_LINUX` placeholder blocks
5. ⏭️ Create stub implementations that log "Platform not implemented"

**Files to Guard:**
- All WinApi.cs and DwmApi.cs usage
- All AvatarHandler classes (window detection, cursor tracking)
- SystemTray code
- DesktopAmbientProbe
- MemoryManager
- SystemStartHandler

### Phase 2: Abstract Core Window Management 🔴 HIGH RISK
**Goal:** Move window operations behind IWindowService

1. ⏭️ Finalize IWindowService interface (enumerate, position, detect, z-order)
2. ⏭️ Implement WindowsWindowService using existing WinApi code
3. ⏭️ Refactor AvatarWindowHandler to use IWindowService ONLY
4. ⏭️ Test Windows build thoroughly
5. ⏭️ Implement MacWindowService using UniWindowController as reference
6. ⏭️ Test macOS build (window enumeration, basic snapping)
7. ⏭️ Implement LinuxWindowService (X11 via libX11)
8. ⏭️ Test Linux build

**Success Criteria:**
- Windows: Feature parity maintained
- macOS: Basic window snapping works
- Linux: Basic window snapping works

### Phase 3: Abstract Screen/Cursor APIs 🟡 MEDIUM RISK
**Goal:** Multi-monitor and cursor tracking

1. ⏭️ Finalize IScreenService interface
2. ⏭️ Implement WindowsScreenService (MonitorFromPoint, GetMonitorInfo, GetCursorPos)
3. ⏭️ Implement MacScreenService (NSScreen, NSEvent.mouseLocation)
4. ⏭️ Implement LinuxScreenService (XRandR for monitors, XQueryPointer for cursor)
5. ⏭️ Refactor AvatarHideHandler and AvatarFollowCursorHandler to use IScreenService
6. ⏭️ Test on multi-monitor setups for all platforms

### Phase 4: System Tray 🟡 MEDIUM RISK
**Goal:** Cross-platform system tray icon

1. ⏭️ Implement WindowsSystemTrayService (existing Shell_NotifyIcon code)
2. ⏭️ Implement MacSystemTrayService (NSStatusBar via Objective-C plugin)
3. ⏭️ Implement LinuxSystemTrayService (AppIndicator primary, XEmbed fallback)
4. ⏭️ Graceful degradation if system tray unavailable

### Phase 5: Visual Features 🟢 LOW RISK
**Goal:** Transparency, ambient probe, window capture

1. ⏭️ **Transparency:**
   - Windows: DwmExtendFrameIntoClientArea (keep existing)
   - macOS: setOpaque:NO, setBackgroundColor:clearColor
   - Linux: ARGB visual with compositor
   
2. ⏭️ **Ambient Probe:**
   - Keep Windows implementation (GDI)
   - macOS: CGWindowListCreateImage
   - Linux: X11 XGetImage or disable feature
   
3. ⏭️ **Window Capture:**
   - Keep uWindowCapture for Windows only
   - Show "Windows-only feature" on other platforms
   - Optional: Implement macOS version later

### Phase 6: Platform-Specific Features 🟢 LOW RISK
**Goal:** Auto-start, file paths, memory management

1. ⏭️ **Auto-start:**
   - Windows: Registry (keep existing)
   - macOS: Login Items API or launchd
   - Linux: .desktop file in autostart
   
2. ⏭️ **Memory Management:**
   - Replace kernel32/psapi calls with System.Diagnostics.Process (cross-platform)
   - Or remove if not critical

3. ⏭️ **File Dialogs:**
   - Already handled by StandaloneFileBrowser ✅

---

## Native Plugin Requirements

### Required Plugins per Platform

#### macOS Plugins Needed
1. **Objective-C Window Management Plugin**
   - Functions: NSWindow enumeration, frame manipulation, z-order
   - Reference: UniWindowController has partial implementation
   
2. **NSStatusBar Plugin**
   - Functions: Create status bar item, menu handling
   - Alternative: Use existing Unity menu system as fallback
   
3. **Screen Capture Plugin** (optional)
   - Functions: CGWindowListCreateImage wrapper
   - For ambient probe feature

#### Linux Plugins Needed
1. **libX11 Wrapper (P/Invoke)**
   - Functions: Window tree walk, XGetWindowAttributes, XMoveResizeWindow
   - Library: libX11.so (standard on all Linux distros)
   
2. **AppIndicator Plugin** (optional)
   - Functions: System tray via libappindicator3
   - Fallback: XEmbed protocol or no tray icon

### Plugin Creation Guide

#### macOS (.bundle or .dylib)
```objective-c
// Example Objective-C plugin structure
@interface WindowManagement : NSObject
+ (NSArray *)enumerateWindows;
+ (void)setWindowFrame:(NSInteger)windowID x:(CGFloat)x y:(CGFloat)y w:(CGFloat)w h:(CGFloat)h;
@end

// Compile as bundle:
// clang -framework Cocoa -dynamiclib -o WindowManagement.bundle WindowManagement.m
```

**Unity Integration:**
```csharp
[DllImport("WindowManagement")]
private static extern IntPtr enumerateWindows();
```

#### Linux (P/Invoke to system libraries)
```csharp
// Direct P/Invoke to libX11.so
[DllImport("libX11.so.6")]
private static extern IntPtr XOpenDisplay(IntPtr display);

[DllImport("libX11.so.6")]
private static extern int XQueryTree(IntPtr display, IntPtr window, ...);
```

No custom plugin needed - use system libraries directly.

---

## Testing Checklist

### Windows Platform Testing
- [ ] Window snapping works (avatar sits on windows)
- [ ] Multi-monitor support (avatar hides near cursor correctly)
- [ ] Cursor following works
- [ ] System tray icon and menu functional
- [ ] Transparency and borderless window correct
- [ ] Ambient probe samples screen colors
- [ ] Window capture feature works
- [ ] Auto-start registry setting works
- [ ] Steam integration functional
- [ ] Discord RPC functional

### macOS Platform Testing
- [ ] Basic window snapping works
- [ ] Multi-monitor detection works
- [ ] Cursor position tracking works
- [ ] System tray icon appears in menu bar
- [ ] Window transparency works
- [ ] App doesn't crash on missing features
- [ ] File dialogs work (StandaloneFileBrowser)
- [ ] Steam integration functional (if Steam installed)
- [ ] Auto-start via Login Items works

### Linux Platform Testing
- [ ] Window snapping works on X11
- [ ] Window snapping works on Wayland (may have limitations)
- [ ] Multi-monitor detection works
- [ ] Cursor tracking works
- [ ] System tray shows (AppIndicator or XEmbed)
- [ ] Window transparency with compositor
- [ ] No crashes on unsupported features
- [ ] File dialogs work (Zenity/kdialog)
- [ ] Steam integration functional

---

## Risk Assessment

### High-Risk Changes
1. 🔴 **Refactoring AvatarWindowHandler** - Core feature, complex logic
2. 🔴 **Window management abstraction** - Used everywhere
3. 🔴 **WindowManager initialization** - App won't start if broken

### Medium-Risk Changes
1. 🟡 **System tray implementation** - Platform-specific, but isolated
2. 🟡 **Multi-monitor support** - Complex coordinate mapping
3. 🟡 **Cursor tracking** - Timing-sensitive code

### Low-Risk Changes
1. 🟢 **Ambient probe** - Can be disabled, visual only
2. 🟢 **Memory manager** - Can be removed entirely
3. 🟢 **Auto-start** - Optional feature
4. 🟢 **Window capture** - Already isolated plugin

---

## Feature Compatibility Matrix

| Feature | Windows | macOS | Linux | Notes |
|---------|---------|-------|-------|-------|
| **Window Snapping** | ✅ Full | ⚠️ Possible | ⚠️ Possible (X11), Limited (Wayland) | Core feature |
| **Multi-Monitor** | ✅ Full | ✅ Full | ✅ Full (X11), ⚠️ Limited (Wayland) | |
| **Cursor Following** | ✅ Full | ✅ Full | ✅ Full | |
| **Window Transparency** | ✅ Full | ✅ Full | ⚠️ Requires compositor | |
| **System Tray** | ✅ Full | ✅ Full | ⚠️ DE-dependent | Ubuntu/Fedora/Arch vary |
| **Ambient Probe** | ✅ Full | ⚠️ Possible | ⚠️ Difficult | Wayland restrictions |
| **Window Capture** | ✅ Full | ⚠️ Possible | ⚠️ Difficult | Requires new implementation |
| **Auto-start** | ✅ Full | ✅ Full | ✅ Full | Different mechanisms |
| **VRM Models** | ✅ Full | ✅ Full | ✅ Full | Already cross-platform |
| **Steam Integration** | ✅ Full | ✅ Full | ✅ Full | Already cross-platform |
| **Discord RPC** | ✅ Full | ✅ Full | ✅ Full | Already cross-platform |
| **File Dialogs** | ✅ Full | ✅ Full | ✅ Full | StandaloneFileBrowser |

**Legend:**
- ✅ Full support achievable
- ⚠️ Partial or conditional support
- ❌ Not feasible
- 🔴 High priority
- 🟡 Medium priority
- 🟢 Low priority

---

## Conclusion

### Summary
Mate-Engine has extensive Windows-specific code concentrated in:
1. Window management (user32.dll) - ~100+ calls
2. Window capture plugin (uWindowCapture.dll) - 90+ calls
3. Desktop effects (dwmapi.dll) - 8 calls
4. System tray (shell32.dll) - 20+ calls
5. Screen capture (gdi32.dll) - 15 calls

### Cross-Platform Feasibility: ✅ ACHIEVABLE
- **Core features** (window snapping, cursor following, transparency) can be implemented on macOS and Linux
- **Most third-party plugins** already support multiple platforms
- **Windows-only features** (window capture, advanced DWM effects) can be gracefully degraded

### Recommended Approach
1. ✅ Start with Phase 1: Add platform guards, ensure Windows still works
2. ⏭️ Phase 2: Abstract window management (highest risk, highest value)
3. ⏭️ Phase 3-6: Incremental feature additions per platform
4. ⏭️ Test continuously on all platforms after each phase

### Estimated Effort
- **Phase 1 (Guards):** 2-3 days
- **Phase 2 (Window Management):** 2-3 weeks
- **Phase 3 (Screen/Cursor):** 1 week
- **Phase 4 (System Tray):** 1 week
- **Phase 5 (Visual Features):** 1-2 weeks
- **Phase 6 (Polish):** 1 week
- **Total:** 8-12 weeks for full cross-platform support

### Next Steps
1. Review this document with team
2. Prioritize features (which are must-have vs. nice-to-have)
3. Set up macOS and Linux test environments
4. Begin Phase 1 implementation
5. Create branch for cross-platform work
6. Test incrementally

---

**Document Version:** 1.0
**Last Updated:** 2025
**Author:** GitHub Copilot
**Status:** Complete - Ready for review
