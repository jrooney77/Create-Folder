#!/usr/bin/env bash
set -euo pipefail

VERSION="1.0.0"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"

app_script="$script_dir/build-macos-app.sh"
app_dir="$repo_root/dist/Create Folder.app"
dmg_path="$repo_root/dist/Create-Folder-macOS-${VERSION}.dmg"

if [ ! -f "$app_script" ]; then
  echo "Error: Missing app packaging script: $app_script" >&2
  exit 1
fi

bash "$app_script"

if [ ! -d "$app_dir" ]; then
  echo "Error: Expected app bundle not found after build: $app_dir" >&2
  exit 1
fi

staging_root="$(mktemp -d "${TMPDIR:-/tmp}/create-folder-dmg.XXXXXX")"
trap 'rm -rf "$staging_root"' EXIT

cp -R "$app_dir" "$staging_root/"

rm -f "$dmg_path"
hdiutil create \
  -volname "Create Folder" \
  -srcfolder "$staging_root" \
  -ov \
  -format UDZO \
  "$dmg_path" >/dev/null

printf 'DMG created: %s\n' "$dmg_path"

