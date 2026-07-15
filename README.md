# HorosHelper

**Firma:** [HorosCode](https://horoscode.de) · **Produkt:** HorosHelper

HorosHelper ist eine Windows-Desktop-Anwendung von HorosCode zur Systemdiagnose, Reparatur und Optimierung — gebaut mit **C#**, **.NET 9** und **Avalonia UI**.

## Features

- **Dashboard** — Echtzeit-Systemgesundheit (CPU, RAM, Festplatte, Netzwerk)
- **Problem-Fixer** — Scan und Reparatur häufiger Windows-Probleme
- **Wissen** — Windows-Einstellungen und Hilfe-Navigator
- **Speicher** — Festplatten- und Speicher-Analyse
- **Startup** — Autostart- und Hintergrundprozess-Optimierung
- **Netzwerk** — Verbindungs- und Latenz-Assistent
- **Sicherheit** — Firewall, Defender und Sicherheits-Score
- **Apps** — Installierte Programme und Treiber-Pflege
- **Backup** — Wiederherstellungspunkte und Ordner-Backups
- **Copilot** — Lokaler KI-System-Assistent (regelbasiert)
- **UAC on demand** — Administratorrechte nur bei Bedarf (Wiederherstellungspunkte, HKLM-Autostart, Reparaturen)

## Requirements

| Anforderung | Version |
|-------------|---------|
| Betriebssystem | Windows 10 oder höher |
| .NET Runtime | .NET 9 SDK (Entwicklung) / .NET 9 Desktop Runtime (Ausführung) |
| Architektur | x64 |

## Build & Run

```powershell
# Repository klonen
git clone <repo-url>
cd HorosHelper

# Abhängigkeiten wiederherstellen und bauen
dotnet restore HorosHelp.sln
dotnet build HorosHelp.sln -c Release

# Tests
dotnet test HorosHelp.sln -c Release

# App starten
dotnet run --project src/HorosHelp.App/HorosHelp.App.csproj
```

## Release-Paket erstellen

```powershell
.\scripts\publish.ps1
```

Erzeugt ein framework-dependent Release unter `artifacts/publish/win-x64/` und ein ZIP unter `artifacts/HorosHelper-win-x64.zip`.

Details: [docs/installation.md](docs/installation.md)

## Screenshots

<!-- TODO: Screenshots einfügen nach UI-Freigabe -->
| Bereich | Pfad |
|---------|------|
| Dashboard | `assets/images/dashboard-mockup-01/full.png` |
| Backup | `assets/images/dashboard-mockup-11/full.png` |

## Projektstruktur

```
HorosHelper/
├── src/
│   ├── HorosHelp.App/     # Avalonia Desktop-Host
│   ├── HorosHelp.Core/    # Services, Models, Windows-API
│   └── HorosHelp.UI/      # Views, ViewModels (MVVM)
├── tests/HorosHelp.Tests/
├── scripts/               # Build- und Publish-Skripte
└── docs/                  # Architektur und Installation
```

## CI/CD

GitHub Actions baut und testet bei jedem Push/PR auf `main` (`.github/workflows/ci.yml`).

## License

Copyright © HorosCode. Alle Rechte vorbehalten.
