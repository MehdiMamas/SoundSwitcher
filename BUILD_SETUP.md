# Build Setup Guide - SoundSwitcher

This document provides step-by-step instructions to build SoundSwitcher on a fresh Windows machine.

## Prerequisites Installation

### 1. Install .NET Framework 4.8.1 Developer Pack

```powershell
winget install Microsoft.DotNet.Framework.DeveloperPack_4
```

This installs the 4.8.1 targeting pack. The project was retargeted from 4.5.2 to 4.8.1 to match available tooling.

### 2. Install Visual Studio 2022 Build Tools

```powershell
winget install Microsoft.VisualStudio.2022.BuildTools --override "--add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --add Microsoft.VisualStudio.Workload.VCTools --quiet --wait"
```

This provides:
- MSBuild for .NET Framework WinForms projects
- VC++ toolchain (cl.exe under `BuildTools\VC\Tools\MSVC\...`) for the C++ backend

### 3. Download NuGet CLI

```powershell
Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "nuget.exe"
```

Required because the project uses packages.config format (not PackageReference).

### 4. (Optional) Install .NET SDK 8

```powershell
winget install Microsoft.DotNet.SDK.8
```

This was installed but not used for the final build. The project uses MSBuild from VS Build Tools, not `dotnet build`.

---

## Repository Changes Required

These changes were already applied to make the project build.

### 1. Retarget WinForms Project to .NET Framework 4.8.1

**File:** `ALsSoundSwitcher_Frontend/ALsSoundSwitcher/ALsSoundSwitcher.csproj`

```xml
<!-- changed from -->
<TargetFrameworkVersion>v4.5.2</TargetFrameworkVersion>

<!-- to -->
<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
```

### 2. Fix DotNetZip Reference

**File:** `ALsSoundSwitcher_Frontend/ALsSoundSwitcher/ALsSoundSwitcher.csproj`

The old reference pointed to `Ionic.Zip/Ionic.Zip.dll` but the restored NuGet package provides `DotNetZip.dll`:

```xml
<!-- changed from -->
<Reference Include="Ionic.Zip">
  <HintPath>..\packages\Ionic.Zip\Ionic.Zip.dll</HintPath>
</Reference>

<!-- to -->
<Reference Include="DotNetZip">
  <HintPath>..\packages\DotNetZip.1.11.0\lib\net20\DotNetZip.dll</HintPath>
</Reference>
```

### 3. Fix C++ Unsafe CRT Warnings

**File:** `SetPlaybackDevice/SetPlaybackDevice/SetPlaybackDevice.vcxproj`

Added `_CRT_SECURE_NO_WARNINGS` to preprocessor definitions:

```xml
<PreprocessorDefinitions>WIN32;_DEBUG;_CONSOLE;_CRT_SECURE_NO_WARNINGS;%(PreprocessorDefinitions)</PreprocessorDefinitions>
```

---

## Build Steps

### 1. Restore NuGet Packages

From the repository root:

```powershell
.\nuget.exe restore ALsSoundSwitcher_Frontend\ALsSoundSwitcher.sln -Source https://api.nuget.org/v3/index.json
```

### 2. Build WinForms Frontend

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
  ALsSoundSwitcher_Frontend\ALsSoundSwitcher.sln `
  /p:Configuration=Release `
  /p:Platform="Any CPU"
```

### 3. Build C++ Backend (SetPlaybackDevice)

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
  SetPlaybackDevice\SetPlaybackDevice.sln `
  /p:Configuration=Release `
  /p:Platform=x64
```

---

## Build Output Locations

- **Frontend:** `ALsSoundSwitcher_Frontend\ALsSoundSwitcher\bin\Release\ALsSoundSwitcher.exe`
- **Backend:** `SetPlaybackDevice\SetPlaybackDevice\x64\Release\SetPlaybackDevice.exe`

---

## Key Points

1. **Use MSBuild, not `dotnet build`**: This is a .NET Framework project, not .NET Core/5+. MSBuild from Visual Studio Build Tools is required.

2. **Target Framework**: The project targets .NET Framework 4.8.1 (retargeted from 4.5.2).

3. **NuGet Restore**: Use `nuget.exe` for packages.config-based projects.

4. **C++ Toolchain**: Requires VC++ tools from Visual Studio Build Tools (v143 toolset).

---

## Troubleshooting

- **"Target framework not found"**: Ensure .NET Framework 4.8.1 Developer Pack is installed.
- **"MSBuild not found"**: Verify Visual Studio 2022 Build Tools with Managed Desktop Build Tools workload is installed.
- **"NuGet packages missing"**: Run `nuget.exe restore` before building.
- **C++ build errors**: Ensure VC++ tools (VCTools workload) are installed via Build Tools installer.
