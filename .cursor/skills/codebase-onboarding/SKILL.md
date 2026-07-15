---
name: codebase-onboarding
description: HorosCode Avalonia Architektur-Analyse vor erstem Code — MVVM, Rules, Tokens, Komponenten.
---

# Purpose

Vor Implementierung die **bestehende Codebase verstehen** — verhindert Duplikate, falsche Patterns und Token-Erfindung.

**Firma:** HorosCode · **Agent:** `avalonia-ui`

# Workflow

```
Architecture → Rules → Tokens → Components → Regions → Plan
```

## 1. Architecture

Lies `docs/architecture.md`:
- MVVM-Schichten (View / ViewModel / Service / Repository)
- Rules 00–12 Überblick
- ADR (`ADR/adr-001` … `adr-003`)

## 2. Rules

| Rule | Warum vor Code |
|------|----------------|
| `01-workflow` | Incremental, ein Bereich |
| `03-mockup-analysis` | Analyse-Gate |
| `04-component-discovery` | Search vor Create |
| `05-csharp` | C#/Avalonia Konventionen |
| `06-ui-preview` | Preview-Pflicht |
| `12-human-approval` | Stop-Gate |

## 3. Tokens

- `docs/design-memory.md` — einzige Token-Quelle
- `docs/design-tokens.md` — Referenz, Never invent
- `@avalonia-design-system` — ResourceDictionary-Mapping

## 4. Components

`docs/component-catalog.md` durchsuchen:

```
Search → Reuse → Extend → Create
```

Bestehende Controls/Styles wiederverwenden bevor neue AXAML-Dateien entstehen.

## 5. Regions

`docs/ui-regions.md` — welcher Bereich ist `in_progress`? Welche sind `approved`?

## 6. Plan

Mini-Plan vor erstem Code:

```markdown
- Region: Header (1440×56)
- Reuse: HcButton, HcNavLink aus component-catalog
- New: HeaderView.axaml + HeaderPreview.axaml
- Tokens: ColorSurfacePrimary, Spacing6
```

# Output

Kurzer Onboarding-Report (5–10 Zeilen) mit Architektur-Fit, wiederverwendbaren Komponenten und offenen Fragen.

# Rule

> **Kein Code ohne Onboarding** bei neuem Projekt oder unbekanntem Modul.

# Verweise

- Docs: `docs/architecture.md`, `docs/avalonia-patterns.md`, `docs/coding-standards.md`
- Skills: `@figma-analysis`, `@incremental-ui`
- Legacy: `@avalonia-figma-to-code`
