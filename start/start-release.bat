@echo off
REM HorosHelper — Release-Build starten (artifacts\publish\win-x64\HorosHelp.App.exe)
chcp 65001 >nul
setlocal

cd /d "%~dp0.."
if errorlevel 1 (
    echo Fehler: Repo-Root nicht gefunden.
    pause
    exit /b 1
)

set "EXE=artifacts\publish\win-x64\HorosHelp.App.exe"

if exist "%EXE%" (
    echo HorosHelper Release starten: %EXE%
    echo.
    start "" "%EXE%"
    exit /b 0
)

echo Release-Build nicht gefunden: %EXE%
echo.
echo Optionen:
echo   [1] Jetzt bauen und veroeffentlichen (publish.bat)
echo   [2] Abbrechen
echo.
choice /c 12 /n /m "Auswahl"

if errorlevel 2 exit /b 1
if errorlevel 1 (
    call "%~dp0publish.bat"
    if errorlevel 1 (
        pause
        exit /b 1
    )
)

if not exist "%EXE%" (
    echo.
    echo Publish abgeschlossen, aber EXE fehlt noch: %EXE%
    pause
    exit /b 1
)

echo.
echo Starte Release...
start "" "%EXE%"

endlocal
