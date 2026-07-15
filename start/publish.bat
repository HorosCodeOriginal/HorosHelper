@echo off
REM HorosHelper — Release veroeffentlichen (scripts\publish.ps1)
chcp 65001 >nul
setlocal

cd /d "%~dp0.."
if errorlevel 1 (
    echo Fehler: Repo-Root nicht gefunden.
    pause
    exit /b 1
)

set "PUBLISH_PS1=scripts\publish.ps1"

if exist "%PUBLISH_PS1%" (
    echo HorosHelper — Publish via %PUBLISH_PS1%
    echo.
    powershell -NoProfile -ExecutionPolicy Bypass -File "%PUBLISH_PS1%"
    set "RC=%ERRORLEVEL%"
) else (
    echo Skript nicht gefunden: %PUBLISH_PS1%
    echo Fallback: dotnet publish...
    echo.
    dotnet publish src\HorosHelp.App\HorosHelp.App.csproj -c Release -r win-x64 --self-contained false -o artifacts\publish\win-x64
    set "RC=%ERRORLEVEL%"
)

if not "%RC%"=="0" (
    echo.
    echo Publish fehlgeschlagen.
    pause
    exit /b %RC%
)

echo.
echo Publish erfolgreich. Ausgabe: artifacts\publish\win-x64\
endlocal
