//
//  MacOSWindowHelper.mm
//
//  macOS-specific window management for appearing above fullscreen apps
//  Based on solution from: https://github.com/electron/electron/issues/10078
//  And: https://github.com/hillelkingqt/GeminiDesk/pull/58
//

#if defined(__APPLE__)

#import <AppKit/AppKit.h>
#import <Cocoa/Cocoa.h>
#import <CoreGraphics/CoreGraphics.h>

extern "C" {

// Define a struct to pass bounds data to C#
typedef struct {
  float x;
  float y;
  float width;
  float height;
  bool isValid;
} BoundsResult;

// Extended window info struct that includes window ID for tracking
typedef struct {
  int32_t windowNumber; // CGWindowNumber - unique window identifier
  int32_t ownerPID;     // Process ID of window owner
  float x;
  float y;
  float width;
  float height;
  bool isValid;
} WindowInfo;

// Max windows to return in enumeration
#define MAX_WINDOW_LIST 32

// Get the main NSWindow for the Unity application
NSWindow *GetUnityNSWindow() {
  // Get the main window from the application
  NSWindow *window = [[NSApplication sharedApplication] mainWindow];
  if (window == nil) {
    // If no main window, try to get the key window
    window = [[NSApplication sharedApplication] keyWindow];
  }
  if (window == nil) {
    // If still no window, try to get the first window
    NSArray *windows = [[NSApplication sharedApplication] windows];
    if ([windows count] > 0) {
      window = [windows objectAtIndex:0];
    }
  }
  return window;
}

// Get the bounds of the Unity window in the same coordinate system as
// GetActiveWindowBounds Returns bounds in CoreGraphics coordinates (origin at
// top-left of PRIMARY DISPLAY, y increases downward) IMPORTANT:
// CGWindowListCopyWindowInfo returns coordinates in "points" (virtual pixels),
// NOT device pixels and uses the PRIMARY display as the reference for Y=0, not
// the window's current screen
BoundsResult MacOS_GetUnityWindowBounds() {
  @autoreleasepool {
    // Version marker to confirm plugin loaded (increment this when making
    // changes)
    static bool firstCall = true;
    if (firstCall) {
      NSLog(@"MacOSWindowHelper: Plugin version 2026-01-05-v9 (primary display "
            @"Y-flip fix)");
      firstCall = false;
    }

    BoundsResult result = {0, 0, 0, 0, false};

    NSWindow *window = GetUnityNSWindow();
    if (window == nil) {
      NSLog(@"MacOSWindowHelper: Could not get Unity window for bounds query");
      return result;
    }

    // Get the frame in Cocoa coordinates (points, bottom-left origin)
    NSRect frame = [window frame];

    // CRITICAL: CGWindowList uses a coordinate system where:
    // - Y=0 is at the TOP of the PRIMARY display (not the window's current
    // screen)
    // - Y increases downward
    // - Coordinates are in points
    // Cocoa uses Y=0 at BOTTOM of PRIMARY display, Y increases upward
    // So we must use the PRIMARY display height for the Y-flip, not the
    // window's screen
    NSScreen *primaryScreen = [NSScreen mainScreen];
    if (primaryScreen == nil) {
      NSLog(@"MacOSWindowHelper: Could not get primary screen for coordinate "
            @"conversion");
      return result;
    }

    // The primary screen's frame in Cocoa coordinates always has origin (0,0)
    // at its bottom-left and the height gives us the reference for the CG
    // coordinate flip
    NSRect primaryFrame = [primaryScreen frame];
    float primaryHeightPoints = primaryFrame.size.height;

    // Cocoa: Y=0 at bottom of primary display, increases upward
    // CoreGraphics/CGWindow: Y=0 at top of primary display, increases downward
    // Convert: yFromTop = primaryHeight - yFromBottom - height (all in points)
    // This works even if window is on a secondary display because Cocoa Y is
    // still relative to the primary display bottom
    float x = frame.origin.x;
    float yFromBottom = frame.origin.y;
    float w = frame.size.width;
    float h = frame.size.height;
    float yFromTop = primaryHeightPoints - yFromBottom - h;

    NSLog(@"MacOSWindowHelper: Unity frame=(%.0f,%.0f %.0fx%.0f) primaryH=%.0f "
          @"-> CG=(%.0f,%.0f)",
          x, yFromBottom, w, h, primaryHeightPoints, x, yFromTop);

    result.x = x;
    result.y = yFromTop;
    result.width = w;
    result.height = h;
    result.isValid = true;

    return result;
  }
}

// Get the Unity window backing scale factor (1.0 for non-Retina, 2.0 for
// Retina)
float MacOS_GetUnityBackingScale() {
  @autoreleasepool {
    NSWindow *window = GetUnityNSWindow();
    if (window == nil)
      return 1.0f;
    if ([window respondsToSelector:@selector(backingScaleFactor)]) {
      return (float)[window backingScaleFactor];
    }
    return 1.0f;
  }
}

// Get the screen bounds in POINTS (same coordinate system as CGWindowList)
// Returns bounds for the PRIMARY display (since CGWindowList uses primary
// display as origin) NOTE: CGWindowList uses points, not device pixels, so we
// return points here too
BoundsResult MacOS_GetScreenBounds() {
  @autoreleasepool {
    BoundsResult result = {0, 0, 0, 0, false};

    // CRITICAL: Use primary screen for coordinate consistency with CGWindowList
    // CGWindowList uses Y=0 at top of primary display, so we should return
    // the primary display dimensions for proper clamping
    NSScreen *screen = [NSScreen mainScreen];

    if (screen == nil) {
      NSLog(
          @"MacOSWindowHelper: Could not get primary screen for bounds query");
      return result;
    }

    // Return primary screen frame in POINTS (same as CGWindowList coordinate
    // system) Note: Primary screen frame origin in Cocoa is (0,0) at
    // bottom-left For CGWindowList compatibility, origin should be (0,0) at
    // top-left
    NSRect screenFrame = [screen frame];
    result.x = 0; // Primary display X is always 0 in CG coordinates
    result.y =
        0; // Primary display Y is always 0 in CG coordinates (top-left origin)
    result.width = screenFrame.size.width;
    result.height = screenFrame.size.height;
    result.isValid = true;

    NSLog(@"MacOSWindowHelper: Screen bounds (points, CG coords) = (%.0f,%.0f "
          @"%.0fx%.0f)",
          result.x, result.y, result.width, result.height);

    return result;
  }
}

// Enable always-on-top that works over fullscreen apps
// This sets the window to appear over fullscreen applications
// Uses NSStatusWindowLevel (system constant) for reliable behavior
void MacOS_EnableAlwaysOnTopOverFullscreen(bool enable) {
  @autoreleasepool {
    dispatch_async(dispatch_get_main_queue(), ^{
      NSWindow *window = GetUnityNSWindow();
      if (window == nil) {
        NSLog(@"MacOSWindowHelper: Could not get Unity window");
        return;
      }

      if (enable) {
        // Unmap first so macOS forgets current space assignment
        [window orderOut:nil];

        // Non-activating, borderless overlay panel
        [window setStyleMask:NSWindowStyleMaskBorderless |
                             NSWindowStyleMaskNonactivatingPanel];
        [window setHidesOnDeactivate:NO];

        // Set window level to status level (above normal app windows, below
        // critical system UI)
        [window setLevel:NSStatusWindowLevel];

        // Comprehensive collection behavior for fullscreen apps
        NSWindowCollectionBehavior behavior =
            NSWindowCollectionBehaviorFullScreenAuxiliary |
            NSWindowCollectionBehaviorCanJoinAllSpaces |
            NSWindowCollectionBehaviorStationary |
            NSWindowCollectionBehaviorIgnoresCycle;
        [window setCollectionBehavior:behavior];

        // Force front even when the app is an LSUIElement (agent)
        [window orderFrontRegardless];

        NSLog(@"MacOSWindowHelper: Enabled always-on-top (level: %ld, "
              @"behavior: %lu)",
              (long)[window level], (unsigned long)[window collectionBehavior]);
      } else {
        // Restore normal window level and behavior
        [window setLevel:NSNormalWindowLevel];
        [window setCollectionBehavior:NSWindowCollectionBehaviorDefault];
        [window orderOut:nil];
        [window orderFrontRegardless];

        NSLog(@"MacOSWindowHelper: Disabled always-on-top");
      }
    });
  }
}

// Alternative: Use screen-saver level for even more aggressive window priority
void MacOS_EnableAlwaysOnTopScreenSaverLevel(bool enable) {
  @autoreleasepool {
    dispatch_async(dispatch_get_main_queue(), ^{
      NSWindow *window = GetUnityNSWindow();
      if (window == nil) {
        NSLog(@"MacOSWindowHelper: Could not get Unity window");
        return;
      }

      if (enable) {
        // Unmap first so macOS forgets current space assignment
        [window orderOut:nil];

        // Non-activating, borderless overlay
        [window setStyleMask:NSWindowStyleMaskBorderless |
                             NSWindowStyleMaskNonactivatingPanel];
        [window setHidesOnDeactivate:NO];

        // Use NSScreenSaverWindowLevel (1000) for maximum window priority
        [window setLevel:NSScreenSaverWindowLevel];

        // Comprehensive behavior mask for maximum compatibility
        NSWindowCollectionBehavior behavior =
            NSWindowCollectionBehaviorFullScreenAuxiliary |
            NSWindowCollectionBehaviorCanJoinAllSpaces |
            NSWindowCollectionBehaviorStationary |
            NSWindowCollectionBehaviorIgnoresCycle;
        [window setCollectionBehavior:behavior];

        [window orderFrontRegardless];

        NSLog(@"MacOSWindowHelper: Enabled screen-saver level always-on-top "
              @"(level: %ld, behavior: %lu)",
              (long)[window level], (unsigned long)[window collectionBehavior]);
      } else {
        [window setLevel:NSNormalWindowLevel];
        [window setCollectionBehavior:NSWindowCollectionBehaviorDefault];
        [window orderOut:nil];
        [window orderFrontRegardless];

        NSLog(@"MacOSWindowHelper: Disabled screen-saver level always-on-top");
      }
    });
  }
}

// Set custom window level (for fine-tuned control)
// Window levels: NSNormalWindowLevel (0), NSFloatingWindowLevel (3),
//                NSModalPanelWindowLevel (8), NSMainMenuWindowLevel (24),
//                NSStatusWindowLevel (25), NSPopUpMenuWindowLevel (101),
//                NSScreenSaverWindowLevel (1000)
void MacOS_SetWindowLevel(int level) {
  @autoreleasepool {
    NSWindow *window = GetUnityNSWindow();
    if (window == nil) {
      NSLog(@"MacOSWindowHelper: Could not get Unity window");
      return;
    }

    [window setLevel:level];
    NSLog(@"MacOSWindowHelper: Set window level to %d", level);
  }
}

// Get current window level
int MacOS_GetWindowLevel() {
  @autoreleasepool {
    NSWindow *window = GetUnityNSWindow();
    if (window == nil) {
      NSLog(@"MacOSWindowHelper: Could not get Unity window");
      return 0;
    }

    return (int)[window level];
  }
}

// Set window to be visible on all spaces/desktops
void MacOS_SetVisibleOnAllSpaces(bool enable) {
  @autoreleasepool {
    NSWindow *window = GetUnityNSWindow();
    if (window == nil) {
      NSLog(@"MacOSWindowHelper: Could not get Unity window");
      return;
    }

    NSWindowCollectionBehavior behavior = [window collectionBehavior];
    if (enable) {
      behavior |= NSWindowCollectionBehaviorCanJoinAllSpaces;
    } else {
      behavior &= ~NSWindowCollectionBehaviorCanJoinAllSpaces;
    }
    [window setCollectionBehavior:behavior];

    NSLog(@"MacOSWindowHelper: Set visible on all spaces: %d", enable);
  }
}

// Check if window can appear over fullscreen apps
bool MacOS_CanAppearOverFullscreen() {
  @autoreleasepool {
    NSWindow *window = GetUnityNSWindow();
    if (window == nil) {
      return false;
    }

    NSWindowCollectionBehavior behavior = [window collectionBehavior];
    return (behavior & NSWindowCollectionBehaviorFullScreenAuxiliary) != 0;
  }
}

// Force window to front and make it key (useful when spaces change)
void MacOS_BringToFront() {
  @autoreleasepool {
    NSWindow *window = GetUnityNSWindow();
    if (window == nil) {
      NSLog(@"MacOSWindowHelper: Could not get Unity window");
      return;
    }

    // Make the window key and bring to front
    [window makeKeyAndOrderFront:nil];
    [window orderFrontRegardless];

    // Also activate the application
    [[NSApplication sharedApplication] activateIgnoringOtherApps:YES];

    NSLog(@"MacOSWindowHelper: Brought window to front");
  }
}

// Hide dock icon (like GeminiDesk does for menu bar apps)
void MacOS_HideDockIcon(bool hide) {
  @autoreleasepool {
    if (hide) {
      [NSApp setActivationPolicy:NSApplicationActivationPolicyAccessory];
      NSLog(@"MacOSWindowHelper: Hid dock icon");
    } else {
      [NSApp setActivationPolicy:NSApplicationActivationPolicyRegular];
      NSLog(@"MacOSWindowHelper: Showed dock icon");
    }
  }
}

// Subscribe to space change notifications and re-apply settings
static bool isMonitoringSpaces = false;
static void *spaceChangeObserver = nil;

void MacOS_StartMonitoringSpaceChanges() {
  @autoreleasepool {
    if (isMonitoringSpaces) {
      NSLog(@"MacOSWindowHelper: Already monitoring space changes");
      return;
    }

    // Listen for space changes
    [[NSWorkspace sharedWorkspace].notificationCenter
        addObserverForName:NSWorkspaceActiveSpaceDidChangeNotification
                    object:nil
                     queue:[NSOperationQueue mainQueue]
                usingBlock:^(NSNotification *_Nonnull note) {
                  NSLog(@"MacOSWindowHelper: Space changed - re-applying "
                        @"window settings");

                  // Re-apply always-on-top settings when space changes
                  NSWindow *window = GetUnityNSWindow();
                  if (window != nil) {
                    // Get current level to determine if we should re-apply
                    NSInteger level = [window level];
                    if (level >= NSFloatingWindowLevel) {
                      // Re-order window to front
                      [window orderFrontRegardless];
                      NSLog(@"MacOSWindowHelper: Re-ordered window to front "
                            @"after space change");
                    }
                  }
                }];

    isMonitoringSpaces = true;
    NSLog(@"MacOSWindowHelper: Started monitoring space changes");
  }
}

void MacOS_StopMonitoringSpaceChanges() {
  @autoreleasepool {
    if (!isMonitoringSpaces) {
      return;
    }

    // Note: We can't use spaceChangeObserver directly since we used a block
    // The notification center will handle cleanup when app terminates
    isMonitoringSpaces = false;
    NSLog(@"MacOSWindowHelper: Stopped monitoring space changes");
  }
}

// Get the bounds of the macOS Dock
// Requires Screen Recording permission (System Settings > Privacy & Security)
BoundsResult MacOS_GetDockBounds() {
  @autoreleasepool {
    BoundsResult result = {0, 0, 0, 0, false};

    // Get list of all visible windows including system windows
    CFArrayRef windowList = CGWindowListCopyWindowInfo(
        kCGWindowListOptionOnScreenOnly, kCGNullWindowID);

    if (windowList == NULL) {
      NSLog(@"MacOSWindowHelper: Could not get window list for Dock");
      return result;
    }

    for (NSDictionary *info in (__bridge NSArray *)windowList) {
      NSString *ownerName = info[(id)kCGWindowOwnerName];

      if ([ownerName isEqualToString:@"Dock"]) {
        CGRect bounds;
        CGRectMakeWithDictionaryRepresentation(
            (CFDictionaryRef)info[(id)kCGWindowBounds], &bounds);

        result.x = bounds.origin.x;
        result.y = bounds.origin.y;
        result.width = bounds.size.width;
        result.height = bounds.size.height;
        result.isValid = true;

        break;
      }
    }

    CFRelease(windowList);
    return result;
  }
}

// Get the bounds of the top-most non-Unity window (ignores our own process)
// Requires Screen Recording permission (System Settings > Privacy & Security)
static BoundsResult gLastActiveWindow = {0, 0, 0, 0, false};

BoundsResult MacOS_GetActiveWindowBounds() {
  @autoreleasepool {
    BoundsResult result = {0, 0, 0, 0, false};

    int myPID = [[NSProcessInfo processInfo] processIdentifier];

    CFArrayRef windowList = CGWindowListCopyWindowInfo(
        kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements,
        kCGNullWindowID);

    if (windowList == NULL) {
      NSLog(@"MacOSWindowHelper: Could not get window list for active window");
      return result;
    }

    NSLog(@"MacOSWindowHelper: Looking for active window from PID %d", myPID);

    // Log top few windows for troubleshooting
    int windowCount = (int)CFArrayGetCount(windowList);
    NSLog(@"MacOSWindowHelper: Total windows in list: %d", windowCount);

    int logCount = 0;
    for (NSDictionary *info in (__bridge NSArray *)windowList) {
      if (logCount >= 5)
        break;
      NSString *ownerName = info[(id)kCGWindowOwnerName];
      NSNumber *ownerPID = info[(id)kCGWindowOwnerPID];
      NSNumber *layer = info[(id)kCGWindowLayer];
      CGRect bounds;
      CGRectMakeWithDictionaryRepresentation(
          (CFDictionaryRef)info[(id)kCGWindowBounds], &bounds);
      NSLog(@"MacOSWindowHelper: Window #%d: '%@' PID=%@ layer=%@ "
            @"bounds=(%.0f,%.0f %.0fx%.0f)",
            logCount, ownerName, ownerPID, layer, bounds.origin.x,
            bounds.origin.y, bounds.size.width, bounds.size.height);
      logCount++;
    }

    bool foundPrimary = false;
    CGRect fallbackBounds = CGRectZero;
    bool hasFallback = false;
    CGRect selfFallbackBounds = CGRectZero;
    bool hasSelfFallback = false;

    for (NSDictionary *info in (__bridge NSArray *)windowList) {
      NSNumber *ownerPID = info[(id)kCGWindowOwnerPID];
      bool isSelf = ([ownerPID intValue] == myPID);

      NSNumber *layer = info[(id)kCGWindowLayer];
      CGRect bounds;
      if (!CGRectMakeWithDictionaryRepresentation(
              (CFDictionaryRef)info[(id)kCGWindowBounds], &bounds)) {
        continue;
      }

      // Skip obvious menu bar/status windows (very short heights)
      if (bounds.size.height <= 40 && [layer intValue] <= 25) {
        continue;
      }

      // Primary target: non-self, normal layer 0, reasonably sized
      if (!isSelf && [layer intValue] == 0 && bounds.size.width > 50 &&
          bounds.size.height > 80) {
        result.x = bounds.origin.x;
        result.y = bounds.origin.y;
        result.width = bounds.size.width;
        result.height = bounds.size.height;
        result.isValid = true;
        foundPrimary = true;
        gLastActiveWindow = result;
        break;
      }

      // Fallback non-self: first reasonable window of any layer
      if (!isSelf && !hasFallback && bounds.size.width > 200 &&
          bounds.size.height > 120) {
        fallbackBounds = bounds;
        hasFallback = true;
      }

      // Self fallback: remember our own window in case nothing else exists
      if (isSelf && !hasSelfFallback && [layer intValue] == 0 &&
          bounds.size.width > 200 && bounds.size.height > 120) {
        selfFallbackBounds = bounds;
        hasSelfFallback = true;
      }
    }

    if (!foundPrimary && hasFallback) {
      result.x = fallbackBounds.origin.x;
      result.y = fallbackBounds.origin.y;
      result.width = fallbackBounds.size.width;
      result.height = fallbackBounds.size.height;
      result.isValid = true;
      gLastActiveWindow = result;
      NSLog(@"MacOSWindowHelper: Using fallback non-self window (%.0f,%.0f "
            @"%.0fx%.0f)",
            result.x, result.y, result.width, result.height);
    } else if (!foundPrimary && !hasFallback && hasSelfFallback) {
      result.x = selfFallbackBounds.origin.x;
      result.y = selfFallbackBounds.origin.y;
      result.width = selfFallbackBounds.size.width;
      result.height = selfFallbackBounds.size.height;
      result.isValid = true;
      gLastActiveWindow = result;
      NSLog(@"MacOSWindowHelper: Using self window fallback (%.0f,%.0f "
            @"%.0fx%.0f)",
            result.x, result.y, result.width, result.height);
    } else if (!foundPrimary && !hasFallback && !hasSelfFallback &&
               gLastActiveWindow.isValid) {
      // Sticky last known window to avoid disappearing when nothing else is
      // found
      result = gLastActiveWindow;
      NSLog(
          @"MacOSWindowHelper: Using cached last window (%.0f,%.0f %.0fx%.0f)",
          result.x, result.y, result.width, result.height);
    } else if (foundPrimary) {
      NSLog(@"MacOSWindowHelper: Found primary non-self window at layer 0 "
            @"(%.0f,%.0f %.0fx%.0f)",
            result.x, result.y, result.width, result.height);
    } else {
      NSLog(@"MacOSWindowHelper: No valid window found (foundPrimary=%d "
            @"hasFallback=%d hasSelfFallback=%d lastValid=%d)",
            foundPrimary, hasFallback, hasSelfFallback,
            gLastActiveWindow.isValid);
    }

    CFRelease(windowList);
    return result;
  }
}

// Request Screen Recording permission by triggering a window list query
// This will cause macOS to show the permission prompt on first use
void MacOS_RequestScreenRecordingPermission() {
  @autoreleasepool {
    // Simply calling CGWindowListCopyWindowInfo triggers the permission check
    // If this is the first time, macOS will show a system prompt
    CFArrayRef windowList =
        CGWindowListCopyWindowInfo(kCGWindowListOptionAll, kCGNullWindowID);

    if (windowList != NULL) {
      NSLog(@"MacOSWindowHelper: Screen Recording permission check triggered");
      CFRelease(windowList);
    } else {
      NSLog(@"MacOSWindowHelper: Screen Recording permission likely denied");
    }
  }
}

// Open System Settings to Screen Recording permission page
void MacOS_OpenScreenRecordingSettings() {
  @autoreleasepool {
    // macOS 13+ uses new URL scheme
    NSURL *url =
        [NSURL URLWithString:@"x-apple.systempreferences:com.apple.preference."
                             @"security?Privacy_ScreenCapture"];

    // Try new API first (macOS 10.15+)
    if (@available(macOS 10.15, *)) {
      [[NSWorkspace sharedWorkspace] openURL:url];
    } else {
      // Fallback for older macOS
      [[NSWorkspace sharedWorkspace] openURL:url];
    }

    NSLog(@"MacOSWindowHelper: Opened System Settings > Screen Recording");
  }
}

// Get our own process ID (for filtering)
int MacOS_GetCurrentPID() {
  return [[NSProcessInfo processInfo] processIdentifier];
}

// Get the bounds of a specific window by its CGWindowNumber
// Returns invalid result if window not found or has been closed
WindowInfo MacOS_GetWindowByNumber(int32_t windowNumber) {
  @autoreleasepool {
    WindowInfo result = {0, 0, 0, 0, 0, 0, false};

    if (windowNumber <= 0) {
      return result;
    }

    CFArrayRef windowList = CGWindowListCopyWindowInfo(
        kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements,
        kCGNullWindowID);

    if (windowList == NULL) {
      return result;
    }

    for (NSDictionary *info in (__bridge NSArray *)windowList) {
      NSNumber *winNum = info[(id)kCGWindowNumber];
      if ([winNum intValue] != windowNumber)
        continue;

      NSNumber *ownerPID = info[(id)kCGWindowOwnerPID];
      CGRect bounds;
      if (!CGRectMakeWithDictionaryRepresentation(
              (CFDictionaryRef)info[(id)kCGWindowBounds], &bounds)) {
        continue;
      }

      result.windowNumber = windowNumber;
      result.ownerPID = [ownerPID intValue];
      result.x = bounds.origin.x;
      result.y = bounds.origin.y;
      result.width = bounds.size.width;
      result.height = bounds.size.height;
      result.isValid = true;
      break;
    }

    CFRelease(windowList);
    return result;
  }
}

// Enumerate all visible windows suitable for snapping
// Returns the count of windows found (up to MAX_WINDOW_LIST)
// Windows are returned in Z-order (front to back)
// outWindows must be pre-allocated with MAX_WINDOW_LIST entries
int MacOS_EnumerateWindows(WindowInfo *outWindows) {
  @autoreleasepool {
    if (outWindows == NULL)
      return 0;

    // Initialize all entries as invalid
    for (int i = 0; i < MAX_WINDOW_LIST; i++) {
      outWindows[i].isValid = false;
    }

    int myPID = [[NSProcessInfo processInfo] processIdentifier];

    CFArrayRef windowList = CGWindowListCopyWindowInfo(
        kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements,
        kCGNullWindowID);

    if (windowList == NULL) {
      NSLog(@"MacOSWindowHelper: EnumerateWindows - could not get window list");
      return 0;
    }

    int count = 0;

    for (NSDictionary *info in (__bridge NSArray *)windowList) {
      if (count >= MAX_WINDOW_LIST)
        break;

      NSNumber *ownerPID = info[(id)kCGWindowOwnerPID];
      int pid = [ownerPID intValue];

      // Skip our own windows
      if (pid == myPID)
        continue;

      NSNumber *layer = info[(id)kCGWindowLayer];
      // Skip non-standard layers (menu bar, status items, etc.)
      // Layer 0 = normal windows, layer < 0 = background, layer > 0 = floating
      if ([layer intValue] != 0)
        continue;

      CGRect bounds;
      if (!CGRectMakeWithDictionaryRepresentation(
              (CFDictionaryRef)info[(id)kCGWindowBounds], &bounds)) {
        continue;
      }

      // Skip tiny windows (menu items, tooltips, etc.)
      if (bounds.size.width < 100 || bounds.size.height < 50)
        continue;

      NSNumber *winNum = info[(id)kCGWindowNumber];

      outWindows[count].windowNumber = [winNum intValue];
      outWindows[count].ownerPID = pid;
      outWindows[count].x = bounds.origin.x;
      outWindows[count].y = bounds.origin.y;
      outWindows[count].width = bounds.size.width;
      outWindows[count].height = bounds.size.height;
      outWindows[count].isValid = true;

      count++;
    }

    CFRelease(windowList);

    NSLog(@"MacOSWindowHelper: EnumerateWindows found %d eligible windows",
          count);
    return count;
  }
}

// Check if a specific window is still visible on screen
bool MacOS_IsWindowVisible(int32_t windowNumber) {
  @autoreleasepool {
    if (windowNumber <= 0)
      return false;

    CFArrayRef windowList = CGWindowListCopyWindowInfo(
        kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements,
        kCGNullWindowID);

    if (windowList == NULL)
      return false;

    bool found = false;
    for (NSDictionary *info in (__bridge NSArray *)windowList) {
      NSNumber *winNum = info[(id)kCGWindowNumber];
      if ([winNum intValue] == windowNumber) {
        found = true;
        break;
      }
    }

    CFRelease(windowList);
    return found;
  }
}

} // extern "C"

// ===================================================================================
// TRAY ICON IMPLEMENTATION
// ===================================================================================

// Callback function type: 0 = LeftClick, 1 = RightClick, >1000 = Menu Command
// ID
typedef void (*TrayCallbackDelegate)(int actionId);

// Global State
static NSStatusItem *g_StatusItem = nil;
static TrayCallbackDelegate g_TrayCallback = NULL;

@interface TrayHandler : NSObject
- (void)onTrayClick:(id)sender;
- (void)onMenuSelect:(id)sender;
@end

static TrayHandler *g_TrayHandler = nil;

@implementation TrayHandler

- (void)onTrayClick:(id)sender {
  if (g_TrayCallback == NULL)
    return;

  NSEvent *event = [NSApp currentEvent];
  // Determine if it is a right click (Ctrl+Click or Right Mouse)
  BOOL isRightClick = ([event type] == NSEventTypeRightMouseUp) ||
                      ([event modifierFlags] & NSEventModifierFlagControl);

  if (isRightClick) {
    g_TrayCallback(1); // Right Click
  } else {
    g_TrayCallback(0); // Left Click
  }
}

- (void)onMenuSelect:(id)sender {
  if (g_TrayCallback == NULL)
    return;
  NSMenuItem *item = (NSMenuItem *)sender;
  g_TrayCallback((int)item.tag);
}

@end

extern "C" {

void MacOS_CreateTrayIcon(const char *tooltip, TrayCallbackDelegate callback) {
  @autoreleasepool {
    // Store callback immediately (it's thread-safe)
    g_TrayCallback = callback;

    // Copy tooltip to heap since we're dispatching async
    NSString *tooltipStr =
        tooltip ? [NSString stringWithUTF8String:tooltip] : nil;

    // CRITICAL: NSStatusBar operations MUST run on main thread
    dispatch_async(dispatch_get_main_queue(), ^{
      if (g_StatusItem != nil) {
        NSLog(@"MacOSWindowHelper: Tray icon already exists");
        return;
      }

      NSLog(@"MacOSWindowHelper: Creating NSStatusItem on main thread");

      g_StatusItem = [[NSStatusBar systemStatusBar]
          statusItemWithLength:NSVariableStatusItemLength];
      [g_StatusItem retain];

      if (g_StatusItem == nil) {
        NSLog(@"MacOSWindowHelper: ERROR - Failed to create NSStatusItem!");
        return;
      }

      if (g_TrayHandler == nil) {
        g_TrayHandler = [[TrayHandler alloc] init];
      }

      if (g_StatusItem.button) {
        if (tooltipStr != nil) {
          g_StatusItem.button.toolTip = tooltipStr;
        }
        g_StatusItem.button.target = g_TrayHandler;
        g_StatusItem.button.action = @selector(onTrayClick:);
        [g_StatusItem.button
            sendActionOn:NSEventMaskLeftMouseUp | NSEventMaskRightMouseUp];

        // Set a default title so something is visible even without an image
        g_StatusItem.button.title = @"ME";
      }

      NSLog(@"MacOSWindowHelper: Successfully created Tray Icon");
    });
  }
}

void MacOS_SetTrayIconImage(const void *ptr, int length) {
  @autoreleasepool {
    if (ptr == NULL || length <= 0) {
      NSLog(@"MacOSWindowHelper: SetTrayIconImage - invalid data");
      return;
    }

    // Copy data to heap for async dispatch
    NSData *data = [NSData dataWithBytes:ptr length:length];

    dispatch_async(dispatch_get_main_queue(), ^{
      if (g_StatusItem == nil || g_StatusItem.button == nil) {
        NSLog(@"MacOSWindowHelper: SetTrayIconImage - no status item");
        return;
      }

      NSImage *image = [[NSImage alloc] initWithData:data];

      if (image) {
        [image setTemplate:YES]; // Adapts to Dark/Light mode

        // Resize to standard menu bar icon size (usually 18x18 or 22x22 points)
        NSSize size = NSMakeSize(18, 18); // 18pt is standard for status bar
        [image setSize:size];

        g_StatusItem.button.image = image;
        g_StatusItem.button.title =
            @""; // Clear text title now that we have image
        [image release];

        NSLog(@"MacOSWindowHelper: Set tray icon image successfully");
      } else {
        NSLog(@"MacOSWindowHelper: SetTrayIconImage - failed to create NSImage "
              @"from data");
      }
    });
  }
}

void MacOS_SetTrayTooltip(const char *tooltip) {
  @autoreleasepool {
    if (g_StatusItem != nil && g_StatusItem.button != nil && tooltip != NULL) {
      g_StatusItem.button.toolTip = [NSString stringWithUTF8String:tooltip];
    }
  }
}

void MacOS_DestroyTrayIcon() {
  @autoreleasepool {
    dispatch_async(dispatch_get_main_queue(), ^{
      if (g_StatusItem != nil) {
        [[NSStatusBar systemStatusBar] removeStatusItem:g_StatusItem];
        [g_StatusItem release];
        g_StatusItem = nil;
        NSLog(@"MacOSWindowHelper: Destroyed Tray Icon");
      }
    });
  }
}

// Helper struct for marshalled menu items
struct MenuItemData {
  const char *text;
  int id;
};

void MacOS_ShowTrayMenu(struct MenuItemData *items, int count) {
  @autoreleasepool {
    if (g_StatusItem == nil)
      return;

    NSMenu *menu = [[NSMenu alloc] init];
    [menu setAutoenablesItems:NO];

    for (int i = 0; i < count; i++) {
      NSString *title = [NSString stringWithUTF8String:items[i].text];

      if ([title isEqualToString:@"SEPARATOR"] ||
          [title isEqualToString:@"-"]) {
        [menu addItem:[NSMenuItem separatorItem]];
      } else {
        NSMenuItem *item =
            [[NSMenuItem alloc] initWithTitle:title
                                       action:@selector(onMenuSelect:)
                                keyEquivalent:@""];
        item.tag = items[i].id;
        item.target = g_TrayHandler;
        [menu addItem:item];
        [item release];
      }
    }

    // Pop up the menu
    [g_StatusItem.menu release]; // Release old menu if any
    g_StatusItem.menu = menu;    // Assigning menu makes it ready to pop up? No,
                                 // this makes it primary action.

    // Strategy: We want the next click to show it, OR show it immediately?
    // Use popupMenuPositioningItem to show explicitly
    [g_StatusItem.button performClick:nil];

    // Reset menu immediately so left-click works again next time?
    // Wait, if we set .menu, the button action @selector(onTrayClick:) IS
    // DISABLED. So we must temporarily set the menu, pop it, then unset it?
    // 'performClick' with a menu attached will show the menu.

    // But performClick is async or sync?
    // Actually, for custom behavior (Left=Action, Right=Menu), we generally
    // DON'T attach .menu to item. We pop it up manually.

    // Manual Popup:
    // [g_StatusItem popUpStatusItemMenu:menu]; // Deprecated

    // Correct way for modern macOS to show a menu strictly programmatically:
    // [menu popUpMenuPositioningItem:nil atLocation:[NSEvent mouseLocation]
    // inView:nil];

    // However, we want it anchored to the status item.
    // If we set g_StatusItem.menu = menu, then [g_StatusItem.button
    // performClick:nil] shows it. Then we must clear it after?

    // Let's try:
    // 1. Set menu
    // 2. Perform click
    // 3. (After delay?) Clear menu

    // Actually, if we use the TrackPopupMenu style, we just want to show it
    // NOW. [menu popUpMenuPositioningItem:nil atLocation:[NSEvent
    // mouseLocation] inView:nil]; This works for right click context menus.
  }
}

// Specialized function to popup menu at status item location
void MacOS_ShowTrayMenuAtMouse(struct MenuItemData *items, int count) {
  @autoreleasepool {
    NSMenu *menu = [[NSMenu alloc] init];
    [menu setAutoenablesItems:NO];

    for (int i = 0; i < count; i++) {
      NSString *title = [NSString stringWithUTF8String:items[i].text];
      if ([title isEqualToString:@"SEPARATOR"] ||
          [title isEqualToString:@"-"]) {
        [menu addItem:[NSMenuItem separatorItem]];
      } else {
        NSMenuItem *item =
            [[NSMenuItem alloc] initWithTitle:title
                                       action:@selector(onMenuSelect:)
                                keyEquivalent:@""];
        item.tag = items[i].id;
        item.target = g_TrayHandler;
        [menu addItem:item];
        [item release];
      }
    }

    // Show menu at current mouse location
    [menu popUpMenuPositioningItem:nil
                        atLocation:[NSEvent mouseLocation]
                            inView:nil];
    // Note: This is a blocking call in Carbon, but Cocoa?
    // It tracks the menu loop.

    [menu release];
  }
}

void MacOS_ShowNotification(const char *title, const char *message) {
  @autoreleasepool {
    NSUserNotification *notification = [[NSUserNotification alloc] init];
    if (title != NULL)
      notification.title = [NSString stringWithUTF8String:title];
    if (message != NULL)
      notification.informativeText = [NSString stringWithUTF8String:message];
    notification.soundName = NSUserNotificationDefaultSoundName;
    [[NSUserNotificationCenter defaultUserNotificationCenter]
        deliverNotification:notification];
    [notification release];
  }
}

} // extern "C"

#endif // __APPLE__
