---
name: architecture-guardian
description: HorosCode Architektur-Validierung — Ordnerstruktur, MVVM, Services, Navigation, State — Verstöße ablehnen.
---

# Purpose

Architektur-Konformität im HorosCode Avalonia-Projekt **vor und nach** Implementierung prüfen. Verstöße blockieren Merge und Fertig-Meldung.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Docs:** `docs/architecture.md`, `ADR/`

# Checks

## Ordnerstruktur

```
Views/           → AXAML + minimal Code-Behind
ViewModels/      → ObservableObject, Commands
Services/        → Interfaces + Implementations
Repositories/    → Daten-Zugriff
Models/          → DTOs, Entities
Resources/       → ResourceDictionary, Themes
Views/Previews/  → isolierte Preview-Fenster
```

Verboten: Business-Logik in `Views/`, AXAML in `Services/`, gemischte Schichten.

## MVVM

| Check | Pass | Fail |
|-------|------|------|
| View Code-Behind | nur `InitializeComponent()` | Event-Handler mit Logik |
| State | ViewModel Properties | View setzt Properties direkt |
| Actions | `RelayCommand` | `Click="Handler"` |
| Data | Service/Repository injiziert | ViewModel ruft HttpClient direkt |

## Services & DI

- Interfaces in `Services/Abstractions/` oder `Services/I*.cs`
- Registrierung in `App.axaml.cs` oder `Program.cs` (DI-Container)
- ViewModels erhalten Services per Constructor Injection
- Kein `new ConcreteService()` in ViewModels

## Navigation & State

- Shell + Region Navigation per ADR-002 (`ADR/adr-002-navigation.md`)
- Navigation-State im ViewModel oder dediziertem `INavigationService`
- Kein statischer Global-State für UI-Flows
- Dialoge über `IDialogService` oder Avalonia-Dialog-Pattern

## Anti-Patterns (sofort reject)

- God-ViewModel (> 300 Zeilen ohne Aufteilung)
- Code-Behind mit `if/else` Business-Logik
- Hardcoded Colors/Fonts ohne Token
- Web-Patterns (Tailwind-Klassen, flex-gap-Hacks) in AXAML
- Zwei Komponenten für dasselbe UI-Element (→ `@component-discovery`)

# Output

- Architektur-Checkliste (Pass/Fail pro Kategorie)
- Liste der Verstöße mit Datei + Zeile
- Empfehlung: Fix vor Preview/Review

# Rule

> **Architektur-Verstöße ablehnen.** Kein „später refactoren" — MVVM und Schichttrennung sind Gates, keine Nice-to-haves. Bei Unklarheit: `docs/architecture.md` und `ADR/` konsultieren.

# Verweise

- Docs: `docs/architecture.md`, `docs/coding-standards.md`, `docs/avalonia-patterns.md`
- ADR: `ADR/adr-001-framework.md`, `ADR/adr-002-navigation.md`, `ADR/adr-003-theme.md`
- Works with: `@avalonia-mvvm`, `@component-discovery`, `@completion-review-board`
- Rule: `05-csharp`, `10-anti-patterns`
