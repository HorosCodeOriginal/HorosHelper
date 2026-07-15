@echo off
REM HorosHelper — Tests ausfuehren (dotnet test HorosHelp.sln)
chcp 65001 >nul
setlocal

cd /d "%~dp0.."
if errorlevel 1 (
    echo Fehler: Repo-Root nicht gefunden.
    pause
    exit /b 1
)

echo HorosHelper — Tests...
echo.

dotnet test HorosHelp.sln
set "RC=%ERRORLEVEL%"

if not "%RC%"=="0" (
    echo.
    echo Tests fehlgeschlagen.
    pause
    exit /b %RC%
)

echo.
echo Alle Tests bestanden.
endlocal
