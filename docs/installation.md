# Installation — HorosHelper

**Firma:** HorosCode · **Produkt:** HorosHelper

## Voraussetzungen

- Windows 10 oder höher (64-Bit)
- [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (framework-dependent Build)

## Aus Entwickler-Build starten

```powershell
dotnet run --project src/HorosHelp.App/HorosHelp.App.csproj
```

## Release-Paket (ZIP)

```powershell
.\scripts\publish.ps1
```

1. ZIP entpacken nach einem Ordner Ihrer Wahl (z. B. `C:\Program Files\HorosHelper\`)
2. `HorosHelp.App.exe` starten
3. Optional: Verknüpfung auf dem Desktop anlegen

## Administratorrechte (UAC)

HorosHelper startet standardmäßig **ohne** Admin-Rechte. Bei Aktionen wie:

- Wiederherstellungspunkt erstellen
- HKLM-Autostart-Einträge ändern
- System-Reparaturen im Problem-Fixer

erscheint ein UAC-Hinweis-Dialog. Nach Bestätigung startet Windows HorosHelper mit erhöhten Rechten neu.

## Logs

Protokolle: `%AppData%\HorosHelper\logs\`

## Deinstallation

Framework-dependent Build: Ordner löschen und ggf. Desktop-Verknüpfung entfernen. Benutzerdaten in `%AppData%\HorosHelper\` können separat gelöscht werden.
