//
//  NativeWindowManager.mm
//  Native macOS window management plugin for Unity
//
//  License: CC0, https://creativecommons.org/publicdomain/zero/1.0/
//

#import <Cocoa/Cocoa.h>
#import <AppKit/AppKit.h>

extern "C" {
    
    // Get the main Unity window
    NSWindow* GetMainWindow() {
        return [[NSApplication sharedApplication] mainWindow];
    }
    
    // Set window level for z-ordering
    // Levels: 0=Normal, 3=Floating, 8=ModalPanel, 24=MainMenu, 25=StatusBar, 101=PopUpMenu, 1000=ScreenSaver
    void NativeSetWindowLevel(int level) {
        NSWindow* window = GetMainWindow();
        if (window) {
            [window setLevel:level];
        }
    }
    
    // Set window collection behavior for fullscreen overlay support
    void NativeSetCollectionBehavior(bool fullScreenAuxiliary, bool canJoinAllSpaces) {
        NSWindow* window = GetMainWindow();
        if (window) {
            NSWindowCollectionBehavior behavior = 0;
            
            if (fullScreenAuxiliary) {
                behavior |= NSWindowCollectionBehaviorFullScreenAuxiliary;
            }
            if (canJoinAllSpaces) {
                behavior |= NSWindowCollectionBehaviorCanJoinAllSpaces;
            }
            
            [window setCollectionBehavior:behavior];
        }
    }
    
    // Set window click-through (ignores mouse events)
    void NativeSetClickThrough(bool enabled) {
        NSWindow* window = GetMainWindow();
        if (window) {
            if (enabled) {
                [window setIgnoresMouseEvents:YES];
            } else {
                [window setIgnoresMouseEvents:NO];
            }
        }
    }
    
    // Set window alpha (transparency)
    void NativeSetWindowAlpha(float alpha) {
        NSWindow* window = GetMainWindow();
        if (window) {
            [window setAlphaValue:alpha];
        }
    }
    
    // Hide dock icon (makes app run as accessory)
    void NativeHideDock() {
        [NSApp setActivationPolicy:NSApplicationActivationPolicyAccessory];
    }
    
    // Show dock icon (makes app run as regular app)
    void NativeShowDock() {
        [NSApp setActivationPolicy:NSApplicationActivationPolicyRegular];
    }
    
    // Create a status bar item (macOS menu bar icon)
    // Note: This is a simplified version. A full implementation would need
    // more complex management of the status item and menu
    static NSStatusItem* g_statusItem = nil;
    
    void NativeCreateStatusBarItem(const char* iconPath, const char* tooltip) {
        if (g_statusItem != nil) {
            return; // Already created
        }
        
        g_statusItem = [[NSStatusBar systemStatusBar] statusItemWithLength:NSSquareStatusItemLength];
        
        if (iconPath != nullptr && strlen(iconPath) > 0) {
            NSString* path = [NSString stringWithUTF8String:iconPath];
            NSImage* icon = [[NSImage alloc] initWithContentsOfFile:path];
            if (icon) {
                [g_statusItem.button setImage:icon];
            }
        }
        
        if (tooltip != nullptr && strlen(tooltip) > 0) {
            NSString* tooltipStr = [NSString stringWithUTF8String:tooltip];
            [g_statusItem.button setToolTip:tooltipStr];
        }
    }
    
    // Remove status bar item
    void NativeRemoveStatusBarItem() {
        if (g_statusItem != nil) {
            [[NSStatusBar systemStatusBar] removeStatusItem:g_statusItem];
            g_statusItem = nil;
        }
    }
    
    // Set non-activating panel style (window doesn't steal focus)
    void NativeSetNonActivatingPanel() {
        NSWindow* window = GetMainWindow();
        if (window) {
            [window setStyleMask:[window styleMask] | NSWindowStyleMaskNonactivatingPanel];
        }
    }
}
