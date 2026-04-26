# ForgePLM Setup Script
# Restores NuGet packages and builds the main ForgePLM solutions.

$ErrorActionPreference = "Stop"

Write-Host "ForgePLM setup starting..." -ForegroundColor Cyan

# Move to repo root based on this script's location
$RepoRoot = Resolve-Path "$PSScriptRoot\.."
Set-Location $RepoRoot

Write-Host "Repo root: $RepoRoot" -ForegroundColor DarkGray

# Download NuGet CLI if missing
$NuGetExe = Join-Path $RepoRoot "nuget.exe"

if (!(Test-Path $NuGetExe)) {
    Write-Host "nuget.exe not found. Downloading..." -ForegroundColor Yellow
    Invoke-WebRequest `
        -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" `
        -OutFile $NuGetExe
}

# Restore legacy packages.config dependencies for SolidWorks Add-In
Write-Host "Restoring SolidWorks Add-In NuGet packages..." -ForegroundColor Cyan
& $NuGetExe restore ".\ForgePLM.SolidWorks.Addin.slnx"

# Build Administrator solution
Write-Host "Building ForgePLM Administrator solution..." -ForegroundColor Cyan
dotnet build ".\ForgePLM.Administrator.slnx"

# Build SolidWorks Add-In solution
Write-Host "Building ForgePLM SolidWorks Add-In solution..." -ForegroundColor Cyan
dotnet build ".\ForgePLM.SolidWorks.Addin.slnx"

Write-Host ""
Write-Host "ForgePLM setup complete." -ForegroundColor Green