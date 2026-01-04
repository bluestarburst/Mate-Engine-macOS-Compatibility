#!/bin/bash
# Build script for macOS native plugin
# This compiles MacOSWindowHelper.mm into a .bundle that Unity can use

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SOURCE_FILE="${SCRIPT_DIR}/MacOSWindowHelper.mm"
BUNDLE_NAME="MacOSWindowHelper.bundle"
OUTPUT_DIR="${SCRIPT_DIR}"
TEMP_DIR="${SCRIPT_DIR}/build_temp"

echo "Building macOS Window Helper Plugin..."
echo "Source: ${SOURCE_FILE}"
echo "Output: ${OUTPUT_DIR}/${BUNDLE_NAME}"

# Clean previous build
rm -rf "${TEMP_DIR}"
rm -rf "${OUTPUT_DIR}/${BUNDLE_NAME}"
mkdir -p "${TEMP_DIR}"

# Compile the plugin
# We need to compile for both x86_64 and arm64 (universal binary)
echo "Compiling for x86_64 and arm64..."

# Get the SDK path
SDK_PATH=$(xcrun --show-sdk-path)
echo "Using SDK: ${SDK_PATH}"

clang++ -arch x86_64 -arch arm64 \
    -dynamiclib \
    -o "${TEMP_DIR}/MacOSWindowHelper" \
    -isysroot "${SDK_PATH}" \
    -framework Cocoa \
    -framework AppKit \
    -framework Foundation \
    -std=c++11 \
    -mmacosx-version-min=10.13 \
    -fPIC \
    -Wno-deprecated-declarations \
    "${SOURCE_FILE}"

echo "Creating bundle structure..."

# Create bundle structure
mkdir -p "${OUTPUT_DIR}/${BUNDLE_NAME}/Contents/MacOS"

# Copy the compiled library
cp "${TEMP_DIR}/MacOSWindowHelper" "${OUTPUT_DIR}/${BUNDLE_NAME}/Contents/MacOS/"

# Create Info.plist
cat > "${OUTPUT_DIR}/${BUNDLE_NAME}/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>MacOSWindowHelper</string>
    <key>CFBundleIdentifier</key>
    <string>com.mateengine.macoswindowhelper</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>MacOSWindowHelper</string>
    <key>CFBundlePackageType</key>
    <string>BNDL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>CFBundleSupportedPlatforms</key>
    <array>
        <string>MacOSX</string>
    </array>
    <key>LSMinimumSystemVersion</key>
    <string>10.13</string>
</dict>
</plist>
EOF

# Clean up temp files
rm -rf "${TEMP_DIR}"

echo "Build complete!"
echo "Bundle created at: ${OUTPUT_DIR}/${BUNDLE_NAME}"
echo ""
echo "To use in Unity:"
echo "1. The bundle is already in the correct location"
echo "2. Unity should automatically recognize it"
echo "3. If needed, restart Unity to reload the plugin"
echo ""
echo "Testing the plugin..."
otool -L "${OUTPUT_DIR}/${BUNDLE_NAME}/Contents/MacOS/MacOSWindowHelper"
