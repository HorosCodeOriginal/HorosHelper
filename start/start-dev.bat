@echo off
REM HorosHelper — Entwicklungsmodus starten (dotnet run, Debug-Build)
chcp 65001 >nul
setlocal

cd /d "%~dp0.."
if errorlevel 1 (
    echo Fehler: Repo-Root nicht gefunden.
    pause
    exit /b 1
)

echo HorosHelper — Entwicklung starten...
echo Projekt: src\HorosHelp.App\HorosHelp.App.csproj
echo.

dotnet run --project src\HorosHelp.App\HorosHelp.App.csproj
if errorlevel 1 (
    echo.
    echo Start fehlgeschlagen.
    pause
    exit /b 1
)

endlocal
