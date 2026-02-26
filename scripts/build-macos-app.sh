#!/usr/bin/env bash
set -euo pipefail

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

rm -rf "$app_dir"
mkdir -p "$macos_dir" "$resources_dir"

cp -R "$publish_dir"/. "$macos_dir"/

project_file="$repo_root/src/FolderCreator.Ui/FolderCreator.Ui.csproj"
project_name="$(basename "${project_file%.csproj}")"
assembly_name="$(sed -n 's:.*<AssemblyName>\(.*\)</AssemblyName>.*:\1:p' "$project_file" | head -n 1)"
exe_name="${assembly_name:-$project_name}"
main_executable="$macos_dir/$exe_name"

if [ ! -f "$main_executable" ]; then
  echo "Error: Expected main executable not found: $main_executable" >&2
  exit 1
fi

chmod +x "$main_executable"

printf 'App bundle created: %s\n' "$app_dir"
