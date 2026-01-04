#!/bin/bash
# Script to add LSUIElement to Info.plist for built macOS app
# This makes the app a "background" app that can hover over other apps
# Without this, the app participates in normal window layering

USAGE="Usage: $0 <path-to-built-app-bundle>"

if [ -z "$1" ]; then
    echo "$USAGE"
    exit 1
fi

APP_BUNDLE="$1"
PLIST_PATH="$APP_BUNDLE/Contents/Info.plist"

if [ ! -f "$PLIST_PATH" ]; then
    echo "Error: Info.plist not found at $PLIST_PATH"
    echo "Make sure you provide the path to the .app bundle"
    exit 1
fi

echo "Modifying $PLIST_PATH..."

# Backup the original
cp "$PLIST_PATH" "$PLIST_PATH.bak"
echo "Backup created at $PLIST_PATH.bak"

# Add LSUIElement=YES to make it a background/agent app
# This allows the window to float above other apps more aggressively
/usr/libexec/PlistBuddy -c "Add :LSUIElement bool true" "$PLIST_PATH" 2>/dev/null

# Check if it was already there
if [ $? -ne 0 ]; then
    # If key already exists, set it to true
    /usr/libexec/PlistBuddy -c "Set :LSUIElement true" "$PLIST_PATH"
fi

echo "✓ Added LSUIElement=true to Info.plist"

# Optionally add high resolution aware setting
/usr/libexec/PlistBuddy -c "Add :NSHighResolutionCapable bool true" "$PLIST_PATH" 2>/dev/null

echo "✓ Ensured high resolution support is enabled"

# Verify the changes
echo ""
echo "Verification - LSUIElement value:"
/usr/libexec/PlistBuddy -c "Print :LSUIElement" "$PLIST_PATH"

echo ""
echo "Done! The app will now behave as a background utility."
echo "Note: The app will no longer appear in the Dock."
