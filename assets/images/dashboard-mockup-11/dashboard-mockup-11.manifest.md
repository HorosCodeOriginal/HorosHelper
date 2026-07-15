# Bild-Manifest — HorosHelper — Backup-Wiederherstellung

> **HorosCode** · HorosHelper Desktop-Mockup · 1:1-Asset

## Identität

| Feld | Wert |
|------|------|
| **Canonical** | `full.png` |
| **Asset-Ordner** | `assets/images/dashboard-mockup-11/` |
| **Asset-Slug** | `dashboard-mockup-11` |
| **Absoluter Pfad** | `C:\HorosHelper\assets\images\dashboard-mockup-11\full.png` |
| **Erstellt (UTC)** | `2026-07-14T23:58:00Z` |
| **Skill** | `@cursor-image-generation` |
| **Agent** | Zuko |
| **Version** | v1 |

## Quelle

### Original-Prompt

```text
CRITICAL: NOT a web browser. App name ONLY HorosHelper. Native Windows 11 desktop window, Avalonia style, Backup & Wiederherstellung view.
Native Windows desktop HorosHelper Backup and restore with restore point timeline, backup profile cards, Neues Backup CTA. Sidebar Backup active.
```

### Negative Prompts

```text
Browser chrome, URL bar, tabs, HorosCloud, HorosHelp, purple AI gradients, SaaS web dashboard
```

### Referenzbild

`assets/images/dashboard-mockup-01/full.png` — Shell-Konsistenz (TitleBar, Sidebar-Stil, StatusBar, Farbpalette)

### Model

Cursor GenerateImage · Google Nano Banana Pro · Aspect 16:9 (Output 1536×1024)

## Dimensionen

1536 × 1024 px · 1352.3 KB · PNG · no alpha

## Farben

| # | Hex | RGB | Anteil | Verwendung |
|---|-----|-----|--------|------------|
| 1 | `#0f172a` | rgb(15,23,42) | 36% | Slate-Hintergrund |
| 2 | `#1e293b` | rgb(30,41,59) | 22% | Profil-Karten |
| 3 | `#f59e0b` | rgb(245,158,11) | 14% | Amber-Akzent |
| 4 | `#0b131b` | rgb(11,19,27) | 18% | Studio-Hintergrund |
| 5 | `#22c55e` | rgb(34,197,94) | 6% | Erfolgreich-Status |

## Design-System

slate-900 `#0f172a` · slate-800 `#1e293b` · amber-500 `#f59e0b` · dark-first

## Regions / Bereiche

| Region | Export | Bounds |
|--------|--------|--------|
| Title Bar | regions/title-bar.png | 184,82 1166×46 |
| Sidebar | regions/sidebar.png | 184,128 199×747 |
| Main Content | regions/main-content.png | 384,128 967×747 |
| Status Bar | regions/status-bar.png | 184,875 1166×56 |

Siehe regions/regions.json und regions/README.md

## Einbindung 1:1

```tsx
<img src="/assets/images/dashboard-mockup-11/full.png" alt="HorosHelper Desktop-App: Backup und Wiederherstellung mit Wiederherstellungspunkte-Timeline und Backup-Profilen" width={1536} height={1024} />
```

```xml
<Image Source="avares://HorosHelper/Assets/Images/dashboard-mockup-11/full.png" Stretch="Uniform" />
```

## Verwendungszweck

HorosHelper Backup & Wiederherstellung — natives Windows-Desktop-Marketing-Still (HorosCode)

## Parity-Checkliste

- [x] Native Desktop (kein Browser)
- [x] App-Name HorosHelper
- [x] HorosCloud nicht im Bild
- [x] Slate+Amber Palette
- [x] regions/ mit 4 Slices
- [ ] Avalonia AXAML Umsetzung

## Pipeline-Protokoll

| 2026-07-14T23:58:00Z | GenerateImage + regions + manifest v1 |
