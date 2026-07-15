#Requires -Version 5.1
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\HorosHelp.App\HorosHelp.App.csproj"
$publishDir = Join-Path $root "artifacts\publish\win-x64"
$zipPath = Join-Path $root "artifacts\HorosHelper-win-x64.zip"

Write-Host "HorosHelper publish (framework-dependent, win-x64)" -ForegroundColor Cyan
Write-Host "Project: $project"

if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

$artifactsRoot = Join-Path $root "artifacts"
if (-not (Test-Path $artifactsRoot)) {
    New-Item -ItemType Directory -Path $artifactsRoot | Out-Null
}

dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o $publishDir

if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath

Write-Host ""
Write-Host "Publish complete:" -ForegroundColor Green
Write-Host "  Folder: $publishDir"
Write-Host "  ZIP:    $zipPath"
