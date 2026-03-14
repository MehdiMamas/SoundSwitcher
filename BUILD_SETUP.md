# Build Setup Guide - SoundSwitcher

## Quick Build (from repo root)

All commands below work in **Git Bash**. Run from the repository root.

```bash
MSBUILD="/c/Program Files (x86)/Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/MSBuild.exe"

# 1. restore nuget packages
./nuget.exe restore ALsSoundSwitcher_Frontend/ALsSoundSwitcher.sln -Source https://api.nuget.org/v3/index.json

# 2. build frontend (WinForms, .NET Framework 4.8.1)
"$MSBUILD" ALsSoundSwitcher_Frontend/ALsSoundSwitcher.sln -p:Configuration=Release -p:Platform="Any CPU"

# 3. build backend (C++, x64)
"$MSBUILD" SetPlaybackDevice/SetPlaybackDevice.sln -p:Configuration=Release -p:Platform=x64
```

Or as a single copy-paste block:

```bash
./nuget.exe restore ALsSoundSwitcher_Frontend/ALsSoundSwitcher.sln -Source https://api.nuget.org/v3/index.json && "/c/Program Files (x86)/Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/MSBuild.exe" ALsSoundSwitcher_Frontend/ALsSoundSwitcher.sln -p:Configuration=Release -p:Platform="Any CPU" && "/c/Program Files (x86)/Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/MSBuild.exe" SetPlaybackDevice/SetPlaybackDevice.sln -p:Configuration=Release -p:Platform=x64
```

## Build Output

- **Frontend:** `ALsSoundSwitcher_Frontend/ALsSoundSwitcher/bin/Release/ALsSoundSwitcher.exe`
- **Backend:** `SetPlaybackDevice/x64/Release/SetPlaybackDevice.exe`

Open the release folder:

```bash
explorer "ALsSoundSwitcher_Frontend/ALsSoundSwitcher/bin/Release"
```

---

## Prerequisites (first-time setup)

### 1. .NET Framework 4.8.1 Developer Pack

```powershell
winget install Microsoft.DotNet.Framework.DeveloperPack_4
```

### 2. Visual Studio 2022 Build Tools

```powershell
winget install Microsoft.VisualStudio.2022.BuildTools --override "--add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --add Microsoft.VisualStudio.Workload.VCTools --quiet --wait"
```

This provides:
- MSBuild for .NET Framework WinForms projects
- VC++ toolchain (v143) for the C++ backend

### 3. NuGet CLI

```powershell
Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "nuget.exe"
```

Required because the project uses `packages.config` format (not PackageReference).

---

## Repository Changes (already applied)

### Retarget to .NET Framework 4.8.1

`ALsSoundSwitcher_Frontend/ALsSoundSwitcher/ALsSoundSwitcher.csproj`: changed `TargetFrameworkVersion` from `v4.5.2` to `v4.8.1`.

### Fix DotNetZip reference

Same `.csproj`: changed `Ionic.Zip` reference to `DotNetZip.1.11.0/lib/net20/DotNetZip.dll` to match the restored NuGet package.

### Fix C++ CRT warnings

`SetPlaybackDevice/SetPlaybackDevice/SetPlaybackDevice.vcxproj`: added `_CRT_SECURE_NO_WARNINGS` to preprocessor definitions.

---

## Key Points

- **Use MSBuild, not `dotnet build`** -- this is a .NET Framework project, not .NET Core/5+.
- **Target Framework:** .NET Framework 4.8.1 (retargeted from 4.5.2).
- **NuGet:** use `nuget.exe` CLI for `packages.config`-based restore.
- **C++ Toolchain:** requires VC++ v143 toolset from VS 2022 Build Tools.

## Troubleshooting

- **"Target framework not found"**: install .NET Framework 4.8.1 Developer Pack.
- **"MSBuild not found"**: install VS 2022 Build Tools with ManagedDesktopBuildTools + VCTools workloads.
- **"NuGet packages missing"**: run `nuget.exe restore` before building.
- **C++ build errors**: ensure VCTools workload is installed.
