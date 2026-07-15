---
name: visual-regression
description: HorosCode Screenshot-vs-Mockup Vergleich — Diff-Report, Score ≥98, Korrektur-Loop.
---

# Purpose

Nachweisbare visuelle Parity zwischen Mockup (SOLL) und Avalonia Preview-Screenshot (IST).

**Firma:** HorosCode · **Rule:** `07-review-loop`

# Inputs

| Input | Pfad / Quelle |
|-------|---------------|
| Mockup-Referenz | `reviews/mockup-{region}.png` oder Figma Export |
| IST-Screenshot | `reviews/review-{region}-v{n}.png` |
| Preview | `*Preview.axaml` bei Mockup-Dimensionen |
| Scoring-Modell | `docs/parameters.md`, `docs/review-checklist.md` |

# Compare

1. Side-by-side: SOLL links, IST rechts (gleiche Pixel-Dimensionen)
2. Kategorien bewerten: Spacing (25), Typography (20), Colors (20), Layout (15), Components (10), a11y (10)
3. Abweichungen mit **konkreten Werten** notieren (nicht „sieht anders aus")
4. Fix-Priorität: Layout-breaking → Spacing → Colors → Typography → Details

## Diff-Beispiel

```markdown
## Review Diffs — Header (Score: 94/100)
1. [Spacing] padding-left: Mockup 24px, IST 16px → Spacing6
2. [Typography] nav: Mockup 13px Medium, IST 14px Regular
3. [Colors] active: IST #FBBF24, Mockup #F59E0B → ColorAccentPrimary
```

# Output (Mismatch Report)

- Gesamt-Score `/100`
- Nummerierte Diff-Liste mit Kategorie-Tags
- Fix-Vorschläge mit Token-Namen
- Nach Fix: neuer Screenshot `v{n+1}`, Re-Score

# Rule

> **Score < 98 = nicht fertig.** Max. 3 Korrektur-Runden, dann User einbeziehen. Nie „close enough" bei 95 akzeptieren.

Auto-Fail unabhängig vom Score:
- Design verbessert vs. Mockup
- Kein isoliertes Preview getestet
- Erfolg ohne Screenshot-Beweis gemeldet

# Verweise

- Docs: `docs/visual-regression.md`, `docs/review-checklist.md`
- Fidelity: `@pixel-perfect-ui`
- Preview: `@avalonia-preview`
- Desktop Capture: manuell (Win+Shift+S) oder isoliertes Preview-Projekt
