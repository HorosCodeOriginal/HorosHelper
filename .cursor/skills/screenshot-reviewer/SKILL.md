---
name: screenshot-reviewer
description: HorosCode detaillierte Screenshot-Analyse — Spacing, Typo, Alignment, Farben, Radius, Icons vs. Mockup.
---

# Purpose

**Tiefenanalyse** des Avalonia Preview-Screenshots gegen das Mockup. Ergänzt `@visual-regression` mit granularer Prüfliste und ausführlichem Abweichungsbericht — nicht nur Gesamt-Score.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Rule:** `07-review-loop`

# Review

Systematisch jeden visuellen Aspekt prüfen:

| Kategorie | Prüfpunkte | Toleranz |
|-----------|------------|----------|
| **Spacing** | Margin, Padding, Gap zwischen Elementen | ±1 px |
| **Typography** | FontFamily, Size, Weight, LineHeight, LetterSpacing | Exakt |
| **Alignment** | Horizontal/Vertical Center, Baseline, Grid-Spalten | ±1 px |
| **Sizing** | Width, Height, Min/Max, Aspect Ratio | Exakt |
| **Colors** | Background, Foreground, Border, States (hover/active) | Token-Match |
| **Radius** | CornerRadius pro Ecke, einheitlich vs. Mockup | Exakt |
| **Icons** | Größe, Stroke, Farbe, Abstand zum Label | ±1 px |
| **Elevation** | Shadow blur, offset, opacity | Visuell ±2 px |

## Review-Workflow

1. Mockup-Referenz und IST-Screenshot **gleiche Pixel-Dimensionen** nebeneinander
2. Von außen nach innen: Container → Layout → Elemente → Details
3. Jede Abweichung mit **konkreten Werten** notieren (nicht „sieht anders aus")
4. Kategorie-Tag vergeben: `[Spacing]`, `[Typography]`, `[Colors]`, …
5. Fix-Vorschlag mit Token-Namen (`Spacing6` statt `16px`)
6. Gesamt-Score nach `docs/review-checklist.md` berechnen

## Detaillierter Diff-Beispiel

```markdown
## Screenshot Review — Sidebar (Score: 96/100)

### Spacing
1. [Spacing] nav-item padding: Mockup 12px 16px, IST 8px 12px → Spacing3 + Spacing4
2. [Spacing] gap zwischen Icons: Mockup 8px, IST 12px → Spacing2

### Typography
3. [Typography] section-label: Mockup 11px SemiBold uppercase, IST 12px Regular

### Icons
4. [Icons] dashboard-icon: Mockup 20×20 #94A3B8, IST 24×24 #64748B

### Colors
5. [Colors] active-item bg: Mockup ColorAccentMuted, IST hardcoded #334155
```

# Output

- **Detaillierter Abweichungsbericht** (nummeriert, kategorisiert)
- Gesamt-Score `/100` nach Scoring-Modell
- Priorisierte Fix-Liste (Layout-breaking zuerst)
- Screenshot-Pfade: SOLL + IST + Version

Works with `@visual-regression` — dort Korrektur-Loop und Re-Score; hier die Detail-Analyse.

# Rule

> **Kein Oberflächen-Review.** Jede Abweichung braucht Messwerte. Score < 98 = nicht freigabefähig. Nie „fast identisch" bei fehlenden Icon-Größen akzeptieren.

# Verweise

- Docs: `docs/review-checklist.md`, `docs/visual-regression.md`
- Ergänzt: `@visual-regression`, `@pixel-perfect-ui`
- Preview: `@avalonia-preview`, `@preview-first-development`
- Rule: `07-review-loop`, `02-pixel-perfect`
