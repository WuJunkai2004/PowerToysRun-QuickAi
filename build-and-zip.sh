#!/usr/bin/env bash
set -euo pipefail

# Resolve repo root (directory of this script)
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Paths & names
PROJECT_PATH="$ROOT_DIR/QuickAi/Community.PowerToys.Run.Plugin.QuickAi/Community.PowerToys.Run.Plugin.QuickAi.csproj"
PLUGIN_NAME="QuickAi"
PLUGIN_DIR="$ROOT_DIR/QuickAi/Community.PowerToys.Run.Plugin.QuickAi"
PLUGIN_JSON="$PLUGIN_DIR/plugin.json"

# Get version from plugin.json
VERSION="$(sed -n 's/.*"Version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$PLUGIN_JSON")"

echo "📋 Plugin: $PLUGIN_NAME"
echo "📋 Version: $VERSION"

# Clean up previous builds
rm -rf "$ROOT_DIR/QuickAi/Publish"
rm -rf "$PLUGIN_DIR/obj" "$PLUGIN_DIR/bin"
rm -f  "$ROOT_DIR/${PLUGIN_NAME}-"*.zip

# Dependencies to exclude (provided by PowerToys)
DEPENDENCIES_TO_EXCLUDE=(
  "PowerToys.Common.UI.*"
  "PowerToys.ManagedCommon.*"
  "PowerToys.Settings.UI.Lib.*"
  "Wox.Infrastructure.*"
  "Wox.Plugin.*"
)

# Build for x64
echo "🛠️  Building for x64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-x64 --self-contained false

# Build for ARM64
echo "🛠️  Building for ARM64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-arm64 --self-contained false

# Publish folders
PUBLISH_X64="$PLUGIN_DIR/bin/Release/net9.0-windows10.0.22621.0/win-x64/publish"
PUBLISH_ARM64="$PLUGIN_DIR/bin/Release/net9.0-windows10.0.22621.0/win-arm64/publish"

# Destinations for packaging (intermediate)
DEST_X64="$ROOT_DIR/QuickAi/Publish/x64"
DEST_ARM64="$ROOT_DIR/QuickAi/Publish/arm64"

# Root-level ZIP targets
ZIP_X64="$ROOT_DIR/${PLUGIN_NAME}-${VERSION}-x64.zip"
ZIP_ARM64="$ROOT_DIR/${PLUGIN_NAME}-${VERSION}-arm64.zip"

# Package x64
echo "📦 Packaging x64..."
rm -rf "$DEST_X64"
mkdir -p "$DEST_X64"
cp -r "$PUBLISH_X64"/. "$DEST_X64/"

# Remove unnecessary dependencies
for dep in "${DEPENDENCIES_TO_EXCLUDE[@]}"; do
  find "$DEST_X64" -type f -name "$dep" -delete 2>/dev/null || true
done

# Create ZIP in repo root
( cd "$DEST_X64" && zip -r "$ZIP_X64" . )

# Package ARM64
echo "📦 Packaging ARM64..."
rm -rf "$DEST_ARM64"
mkdir -p "$DEST_ARM64"
cp -r "$PUBLISH_ARM64"/. "$DEST_ARM64/"

for dep in "${DEPENDENCIES_TO_EXCLUDE[@]}"; do
  find "$DEST_ARM64" -type f -name "$dep" -delete 2>/dev/null || true
done

# Create ZIP in repo root
( cd "$DEST_ARM64" && zip -r "$ZIP_ARM64" . )

echo "✅ Done! Created at repo root:"
echo " - $(basename "$ZIP_X64")"
echo " - $(basename "$ZIP_ARM64")"

# Generate checksums
echo "🔐 Generating checksums..."
echo "x64  SHA256: $(sha256sum "$ZIP_X64"   | cut -d' ' -f1)"
echo "ARM64 SHA256: $(sha256sum "$ZIP_ARM64" | cut -d' ' -f1)"
