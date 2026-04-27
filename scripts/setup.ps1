# ForgePLM Setup Script
# Restores NuGet packages and builds the main ForgePLM solutions.

$ErrorActionPreference = "Stop"
$example = "src/ForgePLM.Runtime.Host/appsettings.example.json"
$target = "src/ForgePLM.Runtime.Host/appsettings.json"

if (!(Test-Path $target)) {
    Write-Host "Creating local appsettings.json from template..." -ForegroundColor Yellow
    Copy-Item $example $target
}

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
Write-Host "Publishing ForgePLM Runtime Host..." -ForegroundColor Cyan

$root = Resolve-Path "$PSScriptRoot\.."
$runtimePublishPath = Join-Path $root "run\ForgePLM.Runtime.Host"

New-Item -ItemType Directory -Force -Path $runtimePublishPath | Out-Null

dotnet publish "src\ForgePLM.Runtime.Host\ForgePLM.Runtime.Host.csproj" `
    -c Release `
    -o $runtimePublishPath

if (Test-Path (Join-Path $runtimePublishPath "ForgePLM.Runtime.Host.exe")) {
    Write-Host "✔ Runtime Host published to: $runtimePublishPath" -ForegroundColor Green
} else {
    Write-Host "✖ Runtime Host publish did not produce expected EXE" -ForegroundColor Red
}


Write-Host ""
Write-Host "Publishing ForgePLM Administrator..." -ForegroundColor Cyan

# Get repo root (where script is being run)
$root = Get-Location

# Define output path
$publishPath = Join-Path $root "run\ForgePLM.Administrator"

# Ensure directory exists
New-Item -ItemType Directory -Force -Path $publishPath | Out-Null

# Publish the Administrator project
dotnet publish "src/ForgePLM.Administrator" `
    -c Release `
    -o $publishPath

Write-Host "Administrator published to: $publishPath" -ForegroundColor Green

Write-Host ""
Write-Host "ForgePLM setup complete." -ForegroundColor Green