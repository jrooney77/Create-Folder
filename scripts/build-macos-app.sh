#!/usr/bin/env bash
set -euo pipefail

VERSION="1.0.0"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"

shopt -s nullglob
publish_candidates=( "$repo_root"/src/FolderCreator.Ui/bin/Release/*/osx-x64/publish )
shopt -u nullglob

if [ "${#publish_candidates[@]}" -eq 0 ]; then
  echo "Error: No publish folder found under src/FolderCreator.Ui/bin/Release/*/osx-x64/publish" >&2
  echo "Run: dotnet publish -c Release -r osx-x64 --self-contained true" >&2
  exit 1
fi

publish_dir=""
for candidate in "${publish_candidates[@]}"; do
  if [ -z "$publish_dir" ] || [ "$candidate" -nt "$publish_dir" ]; then
    publish_dir="$candidate"
  fi
done

app_dir="$repo_root/dist/Create Folder.app"
contents_dir="$app_dir/Contents"
macos_dir="$contents_dir/MacOS"
resources_dir="$contents_dir/Resources"
info_plist="$contents_dir/Info.plist"
assets_dir="$repo_root/assets"
source_icns="$assets_dir/AppIcon.icns"
source_png="$assets_dir/AppIcon.png"
bundle_icns="$resources_dir/AppIcon.icns"

bundle_icon_key="AppIcon"

project_file="$repo_root/src/FolderCreator.Ui/FolderCreator.Ui.csproj"
project_name="$(basename "${project_file%.csproj}")"
assembly_name="$(sed -n 's:.*<AssemblyName>\(.*\)</AssemblyName>.*:\1:p' "$project_file" | head -n 1)"
preferred_exe_name="${assembly_name:-$project_name}"

detected_exe_name=""

if [ -f "$publish_dir/$preferred_exe_name" ] && [ -x "$publish_dir/$preferred_exe_name" ]; then
  detected_exe_name="$preferred_exe_name"
else
  shopt -s nullglob
  for candidate in "$publish_dir"/*; do
    [ -f "$candidate" ] || continue
    [ -x "$candidate" ] || continue

    candidate_name="$(basename "$candidate")"

    if [ "$candidate_name" = "$preferred_exe_name" ]; then
      detected_exe_name="$candidate_name"
      break
    fi

    # Prefer a top-level executable without a conventional extension.
    if [[ "$candidate_name" != *.* ]]; then
      detected_exe_name="$candidate_name"
      break
    fi
  done
  shopt -u nullglob
fi

if [ -z "$detected_exe_name" ]; then
  echo "Error: Could not detect main executable in publish output: $publish_dir" >&2
  exit 1
fi

rm -rf "$app_dir"
mkdir -p "$macos_dir" "$resources_dir"

cp -R "$publish_dir"/. "$macos_dir"/
main_executable="$macos_dir/$detected_exe_name"

if [ ! -f "$main_executable" ]; then
  echo "Error: Expected main executable not found: $main_executable" >&2
  exit 1
fi

chmod +x "$main_executable"

if [ -f "$source_icns" ]; then
  cp "$source_icns" "$bundle_icns"
  printf 'Using existing icon: %s\n' "$source_icns"
elif [ -f "$source_png" ]; then
  if ! command -v sips >/dev/null 2>&1 || ! command -v iconutil >/dev/null 2>&1; then
    echo "Warning: sips and/or iconutil not available; cannot generate $source_icns from $source_png. Continuing without app icon file." >&2
  else
  icon_tmp_root="$(mktemp -d "${TMPDIR:-/tmp}/appicon.XXXXXX")"
  iconset_dir="$icon_tmp_root/AppIcon.iconset"
  mkdir -p "$iconset_dir"
  trap 'rm -rf "$icon_tmp_root"' EXIT

  generate_icon_png() {
    local size="$1"
    local output_name="$2"
    sips -z "$size" "$size" "$source_png" --out "$iconset_dir/$output_name" >/dev/null
  }

  generate_icon_png 16 "icon_16x16.png"
  generate_icon_png 32 "icon_16x16@2x.png"
  generate_icon_png 32 "icon_32x32.png"
  generate_icon_png 64 "icon_32x32@2x.png"
  generate_icon_png 128 "icon_128x128.png"
  generate_icon_png 256 "icon_128x128@2x.png"
  generate_icon_png 256 "icon_256x256.png"
  generate_icon_png 512 "icon_256x256@2x.png"
  generate_icon_png 512 "icon_512x512.png"
  generate_icon_png 1024 "icon_512x512@2x.png"

  if ! iconutil -c icns "$iconset_dir" -o "$source_icns"; then
    echo "Warning: Failed to generate $source_icns from $source_png; continuing without app icon." >&2
  else
    cp "$source_icns" "$bundle_icns"
    printf 'Generated icon: %s\n' "$source_icns"
  fi
  fi
else
  echo "Warning: No app icon found at \"$source_icns\" or \"$source_png\"; building app without custom icon." >&2
fi

cat > "$info_plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>Create Folder</string>
  <key>CFBundleDisplayName</key>
  <string>Create Folder</string>
  <key>CFBundleIdentifier</key>
  <string>com.jrooney77.createfolder</string>
  <key>CFBundleShortVersionString</key>
  <string>${VERSION}</string>
  <key>CFBundleVersion</key>
  <string>${VERSION}</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleExecutable</key>
  <string>${detected_exe_name}</string>
  <key>CFBundleIconFile</key>
  <string>${bundle_icon_key}</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
EOF

touch "$app_dir"

printf 'Detected executable: %s\n' "$detected_exe_name"
printf 'App bundle created: %s\n' "$app_dir"
