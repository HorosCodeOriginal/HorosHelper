---
name: visual-qa-testing
description: HorosCode Desktop Visual QA — Avalonia Preview, Screenshot, States, a11y vor Fertig-Meldung.
---

# Purpose

Systematische visuelle Qualitätssicherung für **Avalonia Desktop** — nicht nur Browser/Playwright. Jeder Bereich wird wie ein echter User geprüft.

**Firma:** HorosCode · **Stack:** C# / Avalonia

# Workflow

```
Setup Preview → Capture → Compare → States → a11y → Report
```

## 1. Setup Preview

- `*Preview.axaml` starten (`Design.PreviewWith` oder Preview-Projekt)
- Viewport = Mockup-Dimensionen, Dark Theme
- Mock-Daten = Mockup-Inhalt

## 2. Capture

- Screenshot bei `reviews/review-{region}-v{n}.png`
- Scrollbare Regionen: **vollständiger Inhalt**, nicht nur sichtbarer Ausschnitt
- Keine Debug-Overlays

## 3. Compare

- Side-by-side mit `reviews/mockup-{region}.png`
- Score nach 100-Punkte-Modell (`@visual-regression`)

## 4. States (Desktop-spezifisch)

| State | Prüfung |
|-------|---------|
| Default | Basis-Layout vs. Mockup |
| Hover | `:pointerover` Pseudo-Class in Styles |
| Active/Selected | Nav, Tabs, Toggle |
| Focus | Keyboard-Focus-Ring sichtbar |
| Disabled | Opacity, nicht klickbar |

## 5. Accessibility

- Kontrast ≥ WCAG AA (Tokens prüfen)
- `AutomationProperties.Name` auf interaktiven Controls
- Tab-Reihenfolge logisch

# Output

QA-Report pro Bereich:

```markdown
## Visual QA — Header
- Score: 98/100
- States tested: default, hover, active, focus
- a11y: pass (contrast OK, names set)
- Screenshot: reviews/review-header-v2.png
```

# Stop Condition

Kein „done" ohne dokumentierten Score ≥ 98 und State-Checkliste.

# Verweise

- `@visual-regression`, `@avalonia-preview`, `@pixel-perfect-ui`
- Docs: `docs/review-checklist.md`, `docs/visual-regression.md`
- Rule: `08-testing`, `07-review-loop`
