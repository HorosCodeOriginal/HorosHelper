---
name: pixel-perfect-ui
description: HorosCode Avalonia 1:1 Fidelity — Mockup ist Wahrheit, ≥98/100, keine Design-Verbesserungen.
---

# Purpose

Erzwingt **pixelnahe Parity** zwischen Mockup/Figma (SOLL) und Avalonia-UI (IST) im HorosCode Desktop-Workflow. Gilt für jeden Bereich (Header, Sidebar, Panel …) bevor Human Approval.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Stack:** C# / Avalonia / MVVM

# Rules

## Never

- Mockup „verbessern" (Farben, Spacing, Typo anpassen ohne Mockup-Beleg)
- Hardcoded Colors/Fonts außerhalb `docs/design-memory.md` / `docs/design-tokens.md`
- Erfolg melden ohne Preview-Screenshot und dokumentierten Score
- Web-Patterns (Tailwind, flex-gap-Hacks) in AXAML übernehmen
- Ganze App ohne Bereichs-Gates implementieren

## Always

- Mockup/Figma-Frame als **einzige visuelle Wahrheit** behandeln
- Tokens aus `docs/design-memory.md` — Search → Reuse → Extend → Create
- Abweichungen in px messen (±1 px Toleranz bei Spacing)
- `*Preview.axaml` isoliert rendern vor Shell-Merge
- Score ≥ **98/100** dokumentieren (`docs/review-checklist.md`)
- Human Approval abwarten (`12-human-approval`) vor nächstem Bereich

# Success Criteria (≥98%)

| Kategorie | Punkte | Prüfung |
|-----------|--------|---------|
| Spacing | 25 | Margin, Padding, Gap — Mockup vs. IST |
| Typography | 20 | FontFamily, Size, Weight, LineHeight |
| Colors | 20 | Nur Design-Tokens |
| Layout | 15 | Alignment, Sizing, Grid-Struktur |
| Components | 10 | Control-Wahl, States (hover/active) |
| Accessibility | 10 | Kontrast, Focus, AutomationProperties |

**Minimum:** 98/100 — darunter Korrektur-Loop (`@visual-regression`).

# Beispiel (Avalonia)

```xml
<!-- FALSCH: hardcoded -->
<Border Background="#1E293B" Padding="16">

<!-- RICHTIG: Token aus ResourceDictionary -->
<Border Background="{StaticResource ColorSurfacePrimary}"
        Padding="{StaticResource Spacing6}">
```

# Verweise

- Rule: `02-pixel-perfect`, `10-anti-patterns`
- Review: `@visual-regression`, `docs/review-checklist.md`
- Legacy ergänzend: `@avalonia-design-system`
