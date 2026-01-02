# Phase 5.4: AvatarWindowHandler Refactoring - Implementation Guide

## Overview
`AvatarWindowHandler.cs` (995 lines) is the largest and most complex component. It implements the core window snapping/sitting feature. 

## Windows API Usage Patterns in AvatarWindowHandler

### 1. **Window Enumeration & Properties**
- `EnumWindows()` - enumerate all windows
- `GetWindowRect()` - get window boundaries
- `GetWindowPlacement()` - get placement info
- `IsIconic()` - check if minimized
- `IsWindowVisible()` - check visibility
- `GetWindowThreadProcessId()` - get process ID
- `GetClassName()` - get window class name
- `GetWindowTextLength()` - get title length
- `GetAncestor()` - traverse window hierarchy  
- `GetParent()` - get parent window
- `GetWindow()` - get next/prev window

### 2. **Window Positioning & State**
- `MoveWindow()` - move and resize window
- `SetWindowPos()` - set position with flags
- `GetClientRect()` - get client area
- `ClientToScreen()` - convert coordinates
- `GetWindowLong/GetWindowLongPtr()` - get window style

### 3. **Window Properties & Attributes**
- `GetLayeredWindowAttributes()` - get transparency/alpha
- `DwmGetWindowAttribute()` - get DWM attributes (cloaking)
- `GetCurrentProcessId()` - get current process ID

### 4. **Direct Cursor Access**
- `Kirurobo.WinApi.GetCursorPos()` - called directly (not yet wrapped)

## Mapping to Platform Services

### IWindowService Methods Used:
- `GetWindowRect()` → `windowService.GetWindowRect()`
- `IsWindowVisible()` → `windowService.IsWindowVisible()`
- `IsIconic()` (minimized) → `windowService.IsWindowMinimized()`
- `GetWindowThreadProcessId()` → `windowService.GetWindowProcessId()`
- `GetClassName()` → `windowService.GetWindowClassName()`
- `GetWindowTextLength()` → `windowService.GetWindowTextLength()`
- `GetAncestor()` → `windowService.GetAncestor()`
- `GetParent()` → `windowService.GetParent()`
- `GetWindow()` → `windowService.GetWindow()`
- `MoveWindow()` → `windowService.SetWindowPosition()`
- `GetClientRect()` → `windowService.GetClientRect()`
- `SetWindowPos()` → `windowService.SetWindowPosition()`
- `GetWindowLong()` → `windowService.GetWindowLong()`

### IScreenService Methods Used:
- `Kirurobo.WinApi.GetCursorPos()` → `screenService.GetCursorPosition()`

### Custom API Not Yet in Services:
- `GetLayeredWindowAttributes()` - transparency/alpha detection
- `DwmGetWindowAttribute()` - DWM cloaking state
- `GetCurrentProcessId()` - process identification

These three may need to be added to IWindowService or created in a specialized service.

## Refactoring Strategy

### Step 1: Add Missing Service Methods
Add to `IWindowService`:
```csharp
// Get window transparency/layered attributes
bool GetLayeredWindowAttributes(IntPtr hWnd, out uint colorKey, out byte alpha, out uint flags);

// Get DWM cloaking state (for hidden windows)
bool GetWindowCloakingState(IntPtr hWnd, out uint cloakingState);

// Get current process ID
uint GetCurrentProcessId();
```

### Step 2: Refactor in Sections
Given the file size, refactor in logical sections:

1. **Initialization** - Replace `Process.GetCurrentProcess().MainWindowHandle` with `windowService.GetMainWindowHandle()`
2. **Window Enumeration** - Replace `EnumWindows` loop with `windowService.EnumerateWindows()`
3. **Cursor Access** - Replace `Kirurobo.WinApi.GetCursorPos()` with `screenService.GetCursorPosition()`
4. **Window Property Access** - Replace individual P/Invoke calls with service methods
5. **Window Positioning** - Replace `MoveWindow/SetWindowPos` with `windowService.SetWindowPosition()`

### Step 3: Remove P/Invoke Declarations
Delete all DllImport and struct declarations from lines 948-985

### Step 4: Handle Platform Gating
The `#if !UNITY_STANDALONE_WIN` check at the start of Update() will work fine with null checks on services.

## Implementation Checklist

- [ ] Add missing methods to IWindowService interface
- [ ] Add implementations to WindowsWindowService
- [ ] Add no-op stubs to StubWindowService
- [ ] Replace InitializeOnce() window handle retrieval
- [ ] Replace Update() cursor position calls
- [ ] Replace EnumWindows with windowService.EnumerateWindows()
- [ ] Replace all window property access calls
- [ ] Replace window positioning calls (MoveWindow/SetWindowPos)
- [ ] Remove DllImport and struct declarations
- [ ] Test Windows build thoroughly for window snapping feature parity

## Known Concerns

1. **Enumerate Windows Performance**: The code enumerates windows frequently for occlusion detection. Performance must match original.
2. **Cloaking Detection**: DWM cloaking state detection is used for filtering windows. This is Windows-specific.
3. **Exact Behavior Match**: Window snapping is the flagship feature - any behavioral changes could break user experience.

## Risk Mitigation

- Test after each section refactor
- Compare window snapping behavior before/after
- Monitor for any occlusion detection regressions
- Verify multi-monitor snapping still works
