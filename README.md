# Folder Creator C#

A lightweight .NET project for quickly creating a main folder and a set of subfolders.

This repository currently contains two app entry points:
- `Folder Creator C#.csproj` (root): console app
- `src/FolderCreator.Ui/FolderCreator.Ui.csproj`: Avalonia desktop UI app

## Features

- Create a primary folder and multiple subfolders in one action
- Basic validation for empty/invalid names
- Real-time folder name sanitization (trims whitespace and removes invalid filename characters as you type)
- Clear success/error messaging
- Live preview of the final root path and each folder path to be created
- Tree-style preview rendering in the Preview card (monospace) using connectors (`├──`, `└──`) and ignoring empty subfolder rows
- Animated status banner that fades/slides in when status updates are present, with accent-responsive border styling
- Cross-platform UI built with Avalonia (desktop project)
- Direction 2 visual foundation for the UI:
  - Frosted/glass window treatment with a glass surface container
  - Glass-card layout for all major sections (Base Directory, Primary Folder Name, Subfolders, Preview, Create/Open actions)
  - Left accent stripe treatment on cards using shared `Accent` brush
  - Icon-chip section headers for faster visual scanning (📂 📁 ➕ 👁 🚀)
  - Typography hierarchy resources (`TextBlock.title`, `TextBlock.section`, `TextBlock.label`)
  - Refined button classes (`primary`, `ghost`, `danger`) with hover lift + smooth transitions
  - Shared visual resources for `GlassSurface`, `CardBg`, `CardBgDark`, and `TextMuted`
- Persists the last selected base directory between app launches (UI app)
- Optional `Open Folder` action after successful creation (macOS `open`, Windows `explorer`, Linux `xdg-open`)
- Keyboard shortcuts in UI:
  - `Enter` runs create
  - `Cmd+O` (macOS) opens folder picker

## Tech Stack

- C# / .NET 10.0
- Avalonia UI 11.3.12 (desktop app)
- CommunityToolkit.Mvvm 8.4.0 (UI view models/commands)

## Prerequisites

- .NET SDK 10.0+

Check your SDK version:

```bash
dotnet --version
```

## Quick Start

### 1) Clone and restore

```bash
git clone <your-repo-url>
cd "Folder Creator C#"
dotnet restore
```

### 2) Run the console app (root project)

```bash
dotnet run --project "Folder Creator C#.csproj"
```

The console flow will prompt for:
- Base directory (or current directory)
- Main folder name
- Number of subfolders
- Individual subfolder names

### 3) Run the desktop UI app (Avalonia)

```bash
dotnet run --project src/FolderCreator.Ui/FolderCreator.Ui.csproj
```

If your environment blocks Avalonia telemetry file writes, run with telemetry disabled:

```bash
AVALONIA_TELEMETRY_OPTOUT=1 dotnet run --project src/FolderCreator.Ui/FolderCreator.Ui.csproj
```

## Build

Build console app:

```bash
dotnet build "Folder Creator C#.csproj"
```

Build UI app:

```bash
dotnet build src/FolderCreator.Ui/FolderCreator.Ui.csproj
```

## Project Structure

```text
.
├── Folder Creator C#.csproj                # Console app project
├── Program.cs                              # Console app entry point
├── src/
│   └── FolderCreator.Ui/
│       ├── FolderCreator.Ui.csproj         # Avalonia desktop project
│       ├── App.axaml                       # App-level styles/theme wiring
│       ├── Views/MainWindow.axaml          # Main UI
│       ├── ViewModels/                     # MVVM view models
│       ├── Services/FolderCreatorService.cs# Folder creation logic
│       └── Converters/                     # UI value converters
└── LICENSE
```

## Notes

- The solution file currently references the root console project.
- The UI project can still be built/run directly with `--project` commands above.
- UI settings are saved to app data as `FolderCreator/settings.json`:
  - macOS/Linux: `Environment.SpecialFolder.ApplicationData`
  - Stored field: `LastBaseDirectory` (restored on startup if the directory still exists)
- In the Avalonia app, folder browsing is wired through MVVM (`BrowseBaseDirectoryCommand`) while folder picker UI stays in `MainWindow.axaml.cs` via a `PickFolderAsync` delegate on the view model.
- `OpenLastCreatedCommand` is enabled only when `LastCreatedPath` is set and still exists on disk.
- `PreviewTreeText` is refreshed alongside `PreviewItems`; list preview behavior remains intact for compatibility while the UI emphasizes tree view.
- macOS note: if Avalonia native startup fails with `RenderTimer` error code `-6661`, the app now exits gracefully with a clear message instead of aborting; this points to a host/runtime graphics-session issue rather than user data.

## License

MIT. See `LICENSE`.
