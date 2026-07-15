@echo off
REM HorosHelper — Solution bauen (dotnet build HorosHelp.sln)
chcp 65001 >nul
setlocal

cd /d "%~dp0.."
if errorlevel 1 (
    echo Fehler: Repo-Root nicht gefunden.
    pause
    exit /b 1
)

echo HorosHelper — Build...
echo.

dotnet build HorosHelp.sln
set "RC=%ERRORLEVEL%"

if not "%RC%"=="0" (
    echo.
    echo Build fehlgeschlagen.
    pause
    exit /b %RC%
)

echo.
echo Build erfolgreich.
endlocal
