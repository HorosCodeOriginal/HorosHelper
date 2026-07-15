# HorosHelper — Aufgabenliste

**Firma:** HorosCode · **Produkt:** HorosHelper  
**Beschreibung:** Windows-System-Hilfe-Anwendung — Diagnose, Reparatur und Optimierung für Windows-Nutzer in einer einheitlichen Oberfläche.

---

## Phase 0 — Projekt-Setup

### Architektur & Tech-Stack

- [x] Tech-Stack-Entscheidung treffen und dokumentieren
  - [x] Option A: C# / Avalonia (empfohlen — HorosCode Desktop-Konsistenz, plattformfähig)
  - [ ] Option B: C# / WPF (nur Windows, etabliert, umfangreiche Windows-API-Unterstützung)
  - [ ] Option C: C# / MAUI (modern, aber höhere Komplexität für Desktop-First)
  - [ ] Entscheidung in `docs/architecture.md` festhalten
- [x] Repository-Struktur anlegen
  - [ ] `.gitignore` für C# / .NET
  - [x] `HorosHelp.sln` Solution-Datei
  - [x] Projektordner: `src/`, `tests/`, `assets/`
- [x] Projekt-Dateien initialisieren
  - [x] `src/HorosHelp.App` — Hauptanwendung
  - [x] `src/HorosHelp.Core` — Business-Logik, Services, Windows-API-Wrapper
  - [x] `src/HorosHelp.UI` — Views, ViewModels (MVVM)
  - [x] `tests/HorosHelp.Tests` — Unit- und Integrationstests
- [x] MVVM-Grundgerüst aufbauen
  - [x] Base-ViewModel mit `INotifyPropertyChanged`
  - [x] Navigationsservice-Interface definieren
  - [x] Dependency-Injection-Container einrichten (z. B. Microsoft.Extensions.DependencyInjection)
- [x] Globale App-Konfiguration
  - [x] Einstellungs-Persistenz (z. B. `appsettings.json` oder Registry)
  - [x] Logging-Framework integrieren (z. B. Serilog)
  - [x] Admin-Rechte / UAC-Strategie festlegen (Elevation on demand — `IAdminElevationService`, UAC-Dialog)
- [ ] Design-System-Grundlage
  - [x] Farbpalette, Typografie, Spacing-Tokens in ResourceDictionary
  - [ ] Basiskomponenten: Button, Card, StatusBadge, SectionHeader
- [x] CI/CD-Pipeline
  - [x] GitHub Actions Workflow für Build + Test (`.github/workflows/ci.yml`)
  - [x] Publish win-x64 Release + ZIP-Skript (`scripts/publish.ps1`)

---

## Sidebar-Navigation & Shell

Die linke Sidebar ist die zentrale Navigation der HorosHelper-App. Alle 11 Views haben ein Desktop-Mockup unter `assets/images/dashboard-mockup-XX/`. Shell-Referenz (TitleBar, Sidebar-Stil, StatusBar): `assets/images/dashboard-mockup-01/full.png`.

### Navigations-Spezifikation

| # | Item | Icon-Hinweis | Feature | Phase | Mockup-Pfad |
|---|------|--------------|---------|-------|-------------|
| 1 | Dashboard | Activity / Gauge | Feature 1 — System-Gesundheits-Dashboard | Phase 1 | `assets/images/dashboard-mockup-01/` |
| 2 | Problem-Fixer | Wrench | Feature 2 — Intelligenter Problem-Fixer | Phase 1 | `assets/images/dashboard-mockup-02/` |
| 3 | Wissen | BookOpen | Feature 3 — Windows-Wissens- & Einstellungs-Navigator | Phase 1 | `assets/images/dashboard-mockup-06/` |
| 4 | Speicher | HardDrive | Feature 5 — Speicher- & Festplatten-Manager | Phase 2 | `assets/images/dashboard-mockup-03/` |
| 5 | Startup | Rocket | Feature 4 — Startup & Hintergrund-Optimierer | Phase 2 | `assets/images/dashboard-mockup-08/` |
| 6 | Netzwerk | Wifi | Feature 6 — Netzwerk- & Verbindungs-Assistent | Phase 2 | `assets/images/dashboard-mockup-09/` |
| 7 | Sicherheit | Shield | Feature 7 — Sicherheits- & Privatsphäre-Zentrale | Phase 3 | `assets/images/dashboard-mockup-04/` |
| 8 | Apps | Grid / AppWindow | Feature 8 — App- & Treiber-Pflege | Phase 3 | `assets/images/dashboard-mockup-10/` |
| 9 | Backup | CloudUpload / Archive | Feature 9 — Backup & Wiederherstellungs-Assistent | Phase 3 | `assets/images/dashboard-mockup-11/` |
| 10 | Copilot | Sparkles / MessageSquare | Feature 10 — KI-System-Assistent | Phase 3 | `assets/images/dashboard-mockup-05/` |
| 11 | Einstellungen | Settings / Cog | Shell — App-Einstellungen | Querschnitt | `assets/images/dashboard-mockup-07/` |

### UI-Verhalten

- [x] Sidebar mit 11 Nav-Items — bei geringer Fensterhöhe **scrollbar** (ScrollViewer)
- [x] Aktiver Nav-Eintrag: Amber-Akzent `#f59e0b` (Hintergrund/Indikator + Text), inaktiv: Slate-Töne
- [x] `Navigationsservice` routet Item → ViewModel gemäß Tabelle oben
- [ ] Sidebar-Breite und Stil 1:1 aus Mockup-01 (`regions/sidebar.png`, 199×747 px)

---

## Phase 1 — MVP (Kernfeatures)

### Feature 1 — System-Gesundheits-Dashboard

**Ziel:** Echtzeit-Übersicht über CPU, RAM, Festplatte, Netzwerk und Windows-Systemstatus.

**Mockup:** `assets/images/dashboard-mockup-01/` · Sidebar-Item: **Dashboard**

#### UI
- [x] Dashboard-View entwerfen und implementieren
- [x] CPU-Auslastungs-Widget (Gauge oder Verlaufsgraph)
- [x] RAM-Nutzungs-Widget (verwendet / verfügbar / gesamt)
- [x] Festplatten-Widget pro Volume (Nutzung, Lese-/Schreibgeschwindigkeit)
- [x] Netzwerk-Widget (Sende- / Empfangsrate, aktive Verbindungen)
- [x] Gesamtgesundheits-Score (Ampel-Indikator: Gut / Achtung / Kritisch)
- [x] Warnungs-Panel für kritische Schwellenwerte

#### Service / Backend
- [x] `SystemHealthService` implementieren
  - [x] CPU-Auslastung via `PerformanceCounter` oder WMI
  - [x] RAM-Statistiken via `GlobalMemoryStatusEx` (P/Invoke)
  - [x] Festplatten-I/O und freier Speicher via `DriveInfo` + WMI
  - [x] Netzwerkstatistiken via `NetworkInterface`
- [x] Polling-Mechanismus mit konfigurierbarem Intervall (Standard: 2 s)
- [x] Schwellenwert-Konfiguration (Nutzer kann Warnwerte anpassen)

#### Windows-API-Integration
- [x] WMI-Queries für Prozess- und Hardware-Daten kapseln
- [x] `PerformanceCounter`-Wrapper mit Fehlerbehandlung
- [ ] Ereignisprotokoll (Windows Event Log) auslesen für Systemfehler

#### Tests
- [ ] Unit-Tests für `SystemHealthService` mit Mock-Daten
- [x] Schwellenwert-Logik testen
- [ ] UI-Binding-Tests (ViewModel → View-Updates)

---

### Feature 2 — Intelligenter Problem-Fixer (One-Click Repair)

**Ziel:** Automatische Erkennung häufiger Windows-Probleme mit Ein-Klick-Reparatur.

**Mockup:** `assets/images/dashboard-mockup-02/` · Sidebar-Item: **Problem-Fixer**

#### UI
- [x] Problem-Scanner-View (Liste erkannter Probleme mit Schweregrad)
- [x] „Jetzt scannen"-Button mit Fortschrittsanzeige
- [x] Problem-Karten mit Beschreibung, Auswirkung und Reparatur-Aktion
- [x] „Alle reparieren"-Button und Einzel-Reparatur
- [x] Reparatur-Protokoll / Ergebnis-Ansicht
- [x] UAC-Hinweis-Dialog wenn Admin-Rechte benötigt (`UacPromptView`, `IUacDialogService`)

#### Service / Backend
- [x] `ProblemScannerService` mit Plugin-Architektur für erweiterbare Checks
- [ ] Repair-Actions implementieren:
  - [x] Temporäre Dateien bereinigen (`%TEMP%`, Windows Temp)
  - [ ] Windows-Update-Cache leeren
  - [x] DNS-Cache leeren (`ipconfig /flushdns`)
  - [ ] Beschädigte Systemdateien prüfen (SFC / DISM via Process)
  - [ ] Defekte Registry-Einträge erkennen (bekannte Muster)
  - [ ] Windows-Suchdienst zurücksetzen
  - [x] Netzwerkstack zurücksetzen (`netsh winsock reset`)
- [ ] Rückgängig-Mechanismus für reversible Aktionen
- [x] Scan-Ergebnisse in Log-Datei speichern

#### Windows-API-Integration
- [x] `Process.Start` für System-Kommandos mit Elevation
- [x] Registry-Zugriff via `Microsoft.Win32.Registry`
- [ ] Windows-Dienste starten/stoppen via `ServiceController`

#### Tests
- [x] Unit-Tests für jeden Check-Plugin
- [x] Reparatur-Aktionen mit Mock-Filesystem testen
- [ ] Rollback-Logik testen

---

### Feature 3 — Windows-Wissens- & Einstellungs-Navigator

**Ziel:** Durchsuchbare Wissensdatenbank für Windows-Einstellungen mit direkter Verknüpfung zu System-Dialogen.

**Mockup:** `assets/images/dashboard-mockup-06/` · Sidebar-Item: **Wissen**

#### UI
- [x] Suchfeld mit Echtzeit-Filterung (Volltextsuche)
- [x] Kategorisierte Artikelliste (Netzwerk, Sicherheit, Leistung, …)
- [x] Artikel-Detailansicht mit Schritt-für-Schritt-Anleitungen
- [x] „Direkt öffnen"-Buttons für Windows-Einstellungen (Deep-Links)
- [x] Favoriten / Lesezeichen für häufig genutzte Artikel
- [x] Offline-Wissensbasis (keine Internetverbindung nötig)

#### Service / Backend
- [x] `KnowledgeBaseService` mit lokalem JSON/XML-Datenspeicher
- [x] Volltextsuche-Engine (z. B. einfacher Tokenizer oder Lucene.NET)
- [x] `WindowsSettingsLauncher` — Deep-Links zu ms-settings: URI-Schema
- [x] Artikel-Format definieren (Markdown oder strukturiertes JSON)
- [x] Initiale Wissensbasis befüllen (min. 50 Windows-Themen auf Deutsch)

#### Windows-API-Integration
- [x] `ms-settings:` URI-Links für alle relevanten Einstellungsseiten
- [ ] `Shell32` für das Öffnen von Control-Panel-Elementen

#### Tests
- [x] Suchfunktions-Tests mit verschiedenen Queries
- [ ] Deep-Link-Verifikation (Windows-Dialoge öffnen sich korrekt)
- [x] Artikel-Parsing-Tests

---

## Phase 2 — Erweiterung

### Feature 4 — Startup & Hintergrund-Optimierer

**Ziel:** Autostart-Einträge verwalten und Hintergrundprozesse kontrollieren.

**Mockup:** `assets/images/dashboard-mockup-08/` · Sidebar-Item: **Startup**

#### UI
- [x] Autostart-Liste mit Status (aktiviert / deaktiviert), Publisher, Startzeit-Impact
- [x] Toggles zum Aktivieren / Deaktivieren einzelner Einträge
- [x] Prozess-Liste mit CPU/RAM-Verbrauch
- [ ] Prozesse beenden (mit Bestätigungsdialog)
- [ ] Empfehlungs-Badges (sicher zu deaktivieren / unbekannt / System)

#### Service / Backend
- [x] `StartupManagerService`
  - [x] Autostart-Einträge aus Registry und Startup-Ordner lesen
  - [x] Einträge aktivieren / deaktivieren / löschen
- [x] `ProcessManagerService`
  - [x] Laufende Prozesse mit Ressourcenverbrauch auflisten
  - [ ] Prozesse beenden (mit Bestätigungsdialog)
- [x] Eintrags-Datenbank mit bekannten Safe-to-Disable-Einträgen (Heuristik)

#### Windows-API-Integration
- [x] Registry-Schlüssel: `HKCU\...\Run`, `HKLM\...\Run`
- [x] Startup-Ordner: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
- [x] `System.Diagnostics.Process` für Prozess-Management

#### Tests
- [x] Lese- / Schreib-Tests für Registry-Einträge (mit Mock)
- [x] Prozess-Listing-Tests

---

### Feature 5 — Speicher- & Festplatten-Manager

**Ziel:** Festplattennutzung analysieren, Bereinigung durchführen, Gesundheit überwachen.

**Mockup:** `assets/images/dashboard-mockup-03/` · Sidebar-Item: **Speicher**

#### UI
- [x] Speicher-Karte pro Laufwerk mit Nutzungsanzeige
- [ ] Ordner-Analyse mit Baumstruktur (größte Verzeichnisse zuerst)
- [x] Bereinigungsoptionen-Checkliste (Papierkorb, Downloads, Temp, …)
- [x] Bereinigung-Vorschau (Größe vor Ausführung)
- [ ] S.M.A.R.T.-Status-Anzeige pro Laufwerk

#### Service / Backend
- [ ] `DiskAnalyzerService` — Verzeichnisbaum mit Größenberechnung
- [x] `DiskCleanupService`
  - [ ] Papierkorb leeren
  - [x] Temp-Ordner bereinigen
  - [ ] Windows Update Cache (`SoftwareDistribution\Download`)
  - [ ] Browser-Cache-Erkennung (Chrome, Edge, Firefox)
- [ ] `SmartDiskService` — S.M.A.R.T.-Daten via WMI

#### Windows-API-Integration
- [ ] WMI `Win32_DiskDrive` für S.M.A.R.T.
- [ ] `Shell32.SHEmptyRecycleBin` für Papierkorb
- [x] `Directory.GetFiles` + Größenberechnung rekursiv

#### Tests
- [x] Größenberechnung-Tests mit Mock-Filesystem
- [x] Bereinigungslogik-Tests (nur Safe-Paths)

---

### Feature 6 — Netzwerk- & Verbindungs-Assistent

**Ziel:** Netzwerkstatus anzeigen, Verbindungsprobleme diagnostizieren und beheben.

**Mockup:** `assets/images/dashboard-mockup-09/` · Sidebar-Item: **Netzwerk**

#### UI
- [x] Netzwerkadapter-Übersicht (IP, Gateway, DNS, Status)
- [x] Verbindungstest-Panel (Ping, Tracert, DNS-Lookup)
- [x] WLAN-Netzwerkliste mit Signalstärke
- [ ] Diagnose-Assistent (Schritt-für-Schritt Fehlersuche)
- [x] Reparatur-Aktionen (Adapter zurücksetzen, IP erneuern, DNS flushen)

#### Service / Backend
- [x] `NetworkDiagnosticService`
  - [x] Ping, Traceroute, DNS-Lookup durchführen
  - [x] Adapter-Informationen via `NetworkInterface`
- [x] `NetworkRepairService`
  - [x] `ipconfig /release` + `/renew`
  - [ ] `netsh winsock reset`
  - [ ] Adapter deaktivieren / aktivieren
- [x] `WifiScannerService` via `NativeWifi` oder WlanAPI (P/Invoke)

#### Windows-API-Integration
- [x] `System.Net.NetworkInformation` für Netzwerk-Statistiken
- [x] WlanAPI (P/Invoke) für WLAN-Scan
- [x] `netsh` Kommandos via `Process.Start`

#### Tests
- [x] Diagnose-Service-Tests mit Mock-Netzwerkdaten
- [ ] Reparatur-Aktions-Tests

---

## Phase 3 — Advanced Features

### Feature 7 — Sicherheits- & Privatsphäre-Zentrale

**Ziel:** Windows-Sicherheitseinstellungen prüfen und Privatsphäre-Optionen verwalten.

**Mockup:** `assets/images/dashboard-mockup-04/` · Sidebar-Item: **Sicherheit**

#### UI
- [x] Sicherheits-Score mit Aufschlüsselung nach Kategorien
- [x] Firewall-Status und Schnell-Toggle
- [x] Windows Defender-Status und letzte Scan-Ergebnisse
- [x] Privatsphäre-Einstellungen-Liste mit Erklärung und Toggle
- [ ] Update-Status (ausstehende Sicherheitsupdates)

#### Service / Backend
- [x] `SecurityService` / `ISecurityService`
  - [x] Windows Defender Status via WMI `SecurityCenter2` + PowerShell-Fallback
  - [x] Firewall-Status via `netsh` + Registry
  - [ ] UAC-Level prüfen
  - [ ] Ausstehende Windows-Updates (WUA API oder PowerShell)
- [ ] `PrivacyService`
  - [ ] Telemetrie-Einstellungen lesen und schreiben (Registry)
  - [ ] App-Berechtigungen (Kamera, Mikrofon, Standort) verwalten
  - [ ] Cortana / Suchdaten-Einstellungen

#### Windows-API-Integration
- [x] WMI `SecurityCenter2` Namespace
- [x] Firewall via `netsh` / Registry
- [ ] Windows Update Agent (WUA) API

#### Tests
- [x] Status-Check-Tests (`SecurityScoreCalculator`)
- [ ] Privacy-Toggle-Tests (Registry Read/Write Mock)

---

### Feature 8 — App- & Treiber-Pflege

**Ziel:** Installierte Anwendungen und Treiber verwalten, Updates finden und bereinigen.

**Mockup:** `assets/images/dashboard-mockup-10/` · Sidebar-Item: **Apps**

#### UI
- [x] Installierte Apps-Liste (Name, Version, Größe, Datum)
- [x] Filter nach Kategorie, Alter, Größe
- [x] Deinstallations-Workflow mit Bestätigung
- [x] Treiber-Liste mit Status (aktuell / veraltet / fehlt)
- [x] Update-Hinweise für veraltete Treiber

#### Service / Backend
- [x] `AppMaintenanceService` / `IAppMaintenanceService`
  - [x] Installierte Apps aus Registry auflesen
  - [x] Deinstallation via `UninstallString` starten
- [x] Treiber via `Win32_PnPSignedDriver` (WMI)
  - [x] Geräte-Manager öffnen für Updates
- [ ] Bereinigung zurückgelassener Registry-Einträge nach Deinstallation

#### Windows-API-Integration
- [x] Registry `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`
- [x] WMI `Win32_PnPSignedDriver`

#### Tests
- [x] App-Listen-Tests mit Mock-Registry-Daten (`InstalledAppParser`)
- [x] Deinstallations-Workflow-Tests

---

### Feature 9 — Backup & Wiederherstellungs-Assistent

**Ziel:** Einfache Backups erstellen, Wiederherstellungspunkte verwalten und Daten sichern.

**Mockup:** `assets/images/dashboard-mockup-11/` · Sidebar-Item: **Backup**

#### UI
- [x] Wiederherstellungspunkte-Timeline live (`ItemsControl` → `RestorePoints`)
- [x] „Wiederherstellungspunkt erstellen"-Button
- [x] Backup-Profil-Konfigurator (Ordner auswählen, Ziel, Zeitplan)
- [x] Backup-Verlauf mit Status und Größe
- [x] Schnell-Backup für Dokumente / Desktop / Bilder

#### Service / Backend
- [x] `BackupService` / `IBackupService`
  - [x] Wiederherstellungspunkte auflisten via PowerShell `Get-ComputerRestorePoint`
  - [x] Neuen Punkt erstellen via `Checkpoint-Computer` (Admin/UAC-Hinweis)
- [x] Backup-Profile (JSON in `%AppData%\HorosHelper\backup-profiles.json`)
  - [x] Datei-/Ordner-Backup (native Copy)
  - [ ] Backup-Zeitplanung via Windows Task Scheduler
  - [ ] Inkrementelles Backup (nur geänderte Dateien)
- [x] Backup-Manifest (JSON) für Wiederherstellungs-Tracking

#### Windows-API-Integration
- [x] PowerShell `Get-ComputerRestorePoint` / `Checkpoint-Computer`
- [ ] `SRSetRestorePoint` via P/Invoke oder PowerShell-Aufruf
- [ ] Windows Task Scheduler COM-Interface

#### Tests
- [ ] Wiederherstellungspunkt-Service-Tests mit WMI-Mock
- [ ] Backup-Logik-Tests mit Mock-Filesystem
- [ ] Zeitplanung-Tests

---

### Feature 10 — KI-System-Assistent (HorosHelper Copilot)

**Ziel:** KI-gestützter Assistent für Diagnosen, Erklärungen und Problemlösungen in natürlicher Sprache.

**Mockup:** `assets/images/dashboard-mockup-05/` · Sidebar-Item: **Copilot**

#### UI
- [x] Chat-Interface im HorosHelper-Design
- [x] Eingabefeld mit Sende-Button und Enter-Support
- [x] Nachrichtenblase-Design (Nutzer vs. Assistent)
- [x] Aktionskarten im Chat (z. B. „Jetzt reparieren"-Button direkt im Antwort-Kontext)
- [ ] Diagnose-Modus: Assistent stellt Rückfragen und führt automatisch Scans durch
- [x] Verlaufs-Anzeige (letzte Sitzungen)
- [x] Datenschutz-Hinweis (was wird gesendet / lokal verarbeitet)

#### Service / Backend
- [x] `CopilotService` / `ICopilotService` — regelbasiert, lokal (MVP)
  - [x] Systemkontext injizieren (aktuelle Diagnose-Daten als Kontext)
  - [x] Aktionskarten für Navigation (Startup, Speicher, Dashboard, …)
  - [ ] Antwort-Streaming
- [x] `CopilotRuleEngine` — Kontext aus Health/Startup/Storage/Security
- [x] Lokaler Fallback (einfache Regelbasierte Antworten ohne Internetverbindung)
- [x] Konversations-Verlauf-Speicherung (in-memory)
- [ ] API-Provider auswählen: OpenAI / Azure OpenAI / lokales Modell (Ollama)

#### Windows-API-Integration
- [x] Nutzt `SystemHealthService` + `StartupService` + `StorageService` + `SecurityService` als Kontext-Quellen
- [ ] Netzwerkzugang via `HttpClient`

#### Tests
- [x] `CopilotRuleEngine`-Tests
- [x] Context-Provider-Tests (via `BuildContext`)
- [ ] Tool-Calling-Logik testen

---

## Querschnitts-Themen

### Shell — Einstellungen

**Mockup:** `assets/images/dashboard-mockup-07/` · Sidebar-Item: **Einstellungen** (immer als letzter Nav-Eintrag)

- [x] Einstellungen-View gemäß Mockup (Kategorie-Nav, Toggles, Theme, Sprache, Scan-Intervall)
- [ ] Von Sidebar-Item 11 erreichbar; unabhängig von Feature-Phasen

### Logging & Fehlerbehandlung

- [ ] Strukturiertes Logging mit Serilog (File + Console Sink)
- [ ] Globaler Exception-Handler (unbehandelte Fehler abfangen, Log + Nutzer-Dialog)
- [ ] Crash-Report-Funktion (optional: anonymisiertes Senden an Telemetrie-Endpunkt)
- [ ] Log-Viewer-Seite in der App (letzte Fehler einsehbar)

### Admin-Rechte & UAC

- [x] UAC-Elevation-Strategie festlegen:
  - [ ] Option A: App startet immer als Admin (einfacher, weniger sicher)
  - [x] Option B: Elevation on demand per Aktion (empfohlen) — `IAdminElevationService` + `RequestElevation(runas)`
- [ ] UAC-Shield-Icon auf Buttons die Admin-Rechte benötigen
- [x] Elevated-Helper-Prozess oder COM-Elevation für Admin-Aktionen — App-Neustart via `runas`
- [x] Shell StatusBar Admin-Badge live (`ShellViewModel` → `IsRunningAsAdmin`)

### Sicherheit

- [ ] Keine hardcodierten API-Keys (Key-Store verwenden)
- [ ] Alle Registry-Schreiboperationen mit Transaktionssicherheit
- [ ] Backup-Daten lokal verschlüsseln (AES-256)
- [ ] Input-Validierung für alle Nutzereingaben (keine Shell-Injection)
- [ ] Least-Privilege-Prinzip: Admin-Rechte nur wo nötig

### Lokalisierung (Deutsch)

- [ ] Alle UI-Texte in Ressourcen-Datei auslagern (`de-DE`)
- [ ] Datum- und Zahlenformatierung auf Deutsch einstellen
- [ ] Wissensdatenbank auf Deutsch verfassen
- [ ] Englische Basis-Übersetzung parallel pflegen (Fallback)

### Dokumentation

- [ ] `docs/architecture.md` — Architektur-Entscheidungen und Projektstruktur
- [ ] `docs/features/` — Feature-Dokumentation pro Bereich
- [x] `README.md` — Projekt-Einstieg, Setup, Build-Anleitung
- [x] `docs/installation.md` — Installation und UAC-Hinweise
- [ ] `CHANGELOG.md` — Versionshistorie
- [ ] Inline-XML-Dokumentation für alle öffentlichen APIs

### Performance & UX

- [ ] Startzeit < 2 Sekunden (kein blockierendes IO beim Start)
- [ ] Alle langen Operationen asynchron (`async/await`) + Fortschrittsanzeige
- [ ] Responsive UI (keine Blockierung des UI-Threads)
- [ ] Dunkles Theme als Standard (HorosCode Design-System)

### Tests & Qualität

- [ ] Unit-Test-Abdeckung ≥ 70 % für Core-Services
- [ ] Integrationstests für Windows-API-Wrapper
- [ ] UI-Smoke-Tests (Fenster öffnet sich, Navigation funktioniert)
- [ ] Code-Review vor jedem Merge in `main`

---

## Erledigt

**2026-07-15 (Phase 4 — Infra & UAC)**

- Backup-Timeline live: `BackupView.axaml` `ItemsControl` an `RestorePoints` (Datum, Beschreibung, „Aktuell“-Badge)
- CI/CD: `.github/workflows/ci.yml` — restore, build, test, optional publish win-x64
- Installer/Docs: `README.md`, `docs/installation.md`, `scripts/publish.ps1` (framework-dependent ZIP)
- UAC: `IAdminElevationService` / `AdminElevationService` (`WindowsPrincipal`), `UacPromptView` Modal, Integration in Backup/Startup/Problem-Fixer, Shell Admin-Badge live
- Unit-Tests: `AdminElevationServiceTests` (64 Tests gesamt, alle grün)

**2026-07-15 (Phase 3)**

- Feature 7: `ISecurityService` / `SecurityService` — Firewall (netsh/Registry), Defender (WMI/PowerShell), Security-Score, Live-Schutz; `SicherheitViewModel` live
- Feature 8: `IAppMaintenanceService` / `AppMaintenanceService` — Registry-Apps, WMI-Treiber, Deinstall + Geräte-Manager; `AppsViewModel` mit Suche/Filter
- Feature 9: `IBackupService` / `BackupService` — Restore Points (PowerShell), JSON-Profile in AppData, Ordner-Backup; `BackupViewModel` live
- Feature 10: `ICopilotService` / `CopilotService` — regelbasierter lokaler Assistent, Aktionskarten + Navigation; `CopilotViewModel` mit Chat-Verlauf
- Unit-Tests: `SecurityScoreCalculatorTests`, `CopilotRuleEngineTests`, `InstalledAppParserTests` (60 Tests gesamt, alle grün)

**2026-07-15**

- Phase 0: Solution-Struktur (`HorosHelp.sln`, `src/`, `tests/`, `assets/`), Tech-Stack **C# / Avalonia**
- MVVM-Grundgerüst (`ViewModelBase`), `INavigationService`, DI (`Microsoft.Extensions.DependencyInjection`)
- Design-Tokens & Styles (`DesignTokens.axaml`, `Components.axaml`)
- Shell: TitleBar, Sidebar (11 Nav-Items), StatusBar, Content-Host (`ShellView`)
- Alle 11 Mockup-Views implementiert (Dashboard, Problem-Fixer, Wissen, Speicher, Startup, Netzwerk, Sicherheit, Apps, Backup, Copilot, Einstellungen) — Mock-Daten
- `NavigationService` + 11 Routen (`NavigationRoutes`)
- Custom Controls: `ProgressRing`, `Sparkline`
- Sidebar-Verhalten: `ScrollViewer`, Amber-Aktiv (`#F59E0B`)
- `tests/HorosHelp.Tests` Grundgerüst inkl. Navigation-Tests

---

*Erstellt: 15. Juli 2026 · HorosCode · HorosHelper*
