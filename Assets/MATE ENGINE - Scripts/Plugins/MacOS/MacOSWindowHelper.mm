//
//  MacOSWindowHelper.mm
//  
//  macOS-specific window management for appearing above fullscreen apps
//  Based on solution from: https://github.com/electron/electron/issues/10078
//  And: https://github.com/hillelkingqt/GeminiDesk/pull/58
//

#if defined(__APPLE__)

#import <Cocoa/Cocoa.h>
#import <AppKit/AppKit.h>

extern "C" {

// Get the main NSWindow for the Unity application
NSWindow* GetUnityNSWindow() {
    // Get the main window from the application
    NSWindow* window = [[NSApplication sharedApplication] mainWindow];
    if (window == nil) {
        // If no main window, try to get the key window
        window = [[NSApplication sharedApplication] keyWindow];
    }
    if (window == nil) {
        // If still no window, try to get the first window
        NSArray* windows = [[NSApplication sharedApplication] windows];
        if ([windows count] > 0) {
            window = [windows objectAtIndex:0];
        }
    }
    return window;
}

// Enable always-on-top that works over fullscreen apps
// This sets the window to appear over fullscreen applications
// Uses NSStatusWindowLevel (system constant) for reliable behavior
void MacOS_EnableAlwaysOnTopOverFullscreen(bool enable) {
    @autoreleasepool {
        dispatch_async(dispatch_get_main_queue(), ^{
            NSWindow* window = GetUnityNSWindow();
            if (window == nil) {
                NSLog(@"MacOSWindowHelper: Could not get Unity window");
                return;
            }
            
            if (enable) {
                // Unmap first so macOS forgets current space assignment
                [window orderOut:nil];

                // Non-activating, borderless overlay panel
                [window setStyleMask: NSWindowStyleMaskBorderless | NSWindowStyleMaskNonactivatingPanel];
                [window setHidesOnDeactivate:NO];

                // Set window level to status level (above normal app windows, below critical system UI)
                [window setLevel:NSStatusWindowLevel];
                
                // Comprehensive collection behavior for fullscreen apps
                NSWindowCollectionBehavior behavior = NSWindowCollectionBehaviorFullScreenAuxiliary |
                                                       NSWindowCollectionBehaviorCanJoinAllSpaces |
                                                       NSWindowCollectionBehaviorStationary |
                                                       NSWindowCollectionBehaviorIgnoresCycle;
                [window setCollectionBehavior:behavior];
                
                // Force front even when the app is an LSUIElement (agent)
                [window orderFrontRegardless];

                NSLog(@"MacOSWindowHelper: Enabled always-on-top (level: %ld, behavior: %lu)", 
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
            NSWindow* window = GetUnityNSWindow();
            if (window == nil) {
                NSLog(@"MacOSWindowHelper: Could not get Unity window");
                return;
            }
            
            if (enable) {
                // Unmap first so macOS forgets current space assignment
                [window orderOut:nil];

                // Non-activating, borderless overlay
                [window setStyleMask: NSWindowStyleMaskBorderless | NSWindowStyleMaskNonactivatingPanel];
                [window setHidesOnDeactivate:NO];

                // Use NSScreenSaverWindowLevel (1000) for maximum window priority
                [window setLevel:NSScreenSaverWindowLevel];
                
                // Comprehensive behavior mask for maximum compatibility
                NSWindowCollectionBehavior behavior = NSWindowCollectionBehaviorFullScreenAuxiliary |
                                                       NSWindowCollectionBehaviorCanJoinAllSpaces |
                                                       NSWindowCollectionBehaviorStationary |
                                                       NSWindowCollectionBehaviorIgnoresCycle;
                [window setCollectionBehavior:behavior];

                [window orderFrontRegardless];
                
                NSLog(@"MacOSWindowHelper: Enabled screen-saver level always-on-top (level: %ld, behavior: %lu)", 
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
        NSWindow* window = GetUnityNSWindow();
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
        NSWindow* window = GetUnityNSWindow();
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
        NSWindow* window = GetUnityNSWindow();
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
        NSWindow* window = GetUnityNSWindow();
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
        NSWindow* window = GetUnityNSWindow();
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
static void* spaceChangeObserver = nil;

void MacOS_StartMonitoringSpaceChanges() {
    @autoreleasepool {
        if (isMonitoringSpaces) {
            NSLog(@"MacOSWindowHelper: Already monitoring space changes");
            return;
        }
        
        // Listen for space changes
        [[NSWorkspace sharedWorkspace].notificationCenter addObserverForName:NSWorkspaceActiveSpaceDidChangeNotification
                                                                      object:nil
                                                                       queue:[NSOperationQueue mainQueue]
                                                                  usingBlock:^(NSNotification * _Nonnull note) {
            NSLog(@"MacOSWindowHelper: Space changed - re-applying window settings");
            
            // Re-apply always-on-top settings when space changes
            NSWindow* window = GetUnityNSWindow();
            if (window != nil) {
                // Get current level to determine if we should re-apply
                NSInteger level = [window level];
                if (level >= NSFloatingWindowLevel) {
                    // Re-order window to front
                    [window orderFrontRegardless];
                    NSLog(@"MacOSWindowHelper: Re-ordered window to front after space change");
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

} // extern "C"

#endif // __APPLE__
