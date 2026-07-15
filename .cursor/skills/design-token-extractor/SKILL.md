---
name: design-token-extractor
description: HorosCode Design-Tokens aus Mockup extrahieren — Farben, Typo, Spacing → design-tokens.md + design-memory.md.
---

# Purpose

Systematische Extraktion aller visuellen Werte aus Mockup/Figma in eine **wiederverwendbare Token-Spezifikation**. Verhindert Hardcoding und erfundene Designwerte.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Stack:** Avalonia ResourceDictionary

# Extract

Aus Mockup/Figma messen und kategorisieren:

| Kategorie | Extrahieren | Beispiel-Token |
|-----------|-------------|----------------|
| **Colors** | Hex/RGBA, Opacity, States | `ColorSurfacePrimary`, `ColorAccentPrimary` |
| **Typography** | FontFamily, Size, Weight, LineHeight | `FontSizeBody`, `FontWeightSemiBold` |
| **Spacing** | Margin, Padding, Gap (px) | `Spacing4` (16px), `Spacing6` (24px) |
| **Elevation** | Shadow blur/offset/spread | `ElevationCard`, `ElevationModal` |
| **Radius** | Border-Radius pro Ecke | `RadiusSm` (4px), `RadiusLg` (12px) |
| **Breakpoints** | Layout-Umschaltpunkte | `BreakpointMd` (1024px) |

## Extraktions-Workflow

1. Mockup öffnen — Referenz-Frame-Dimensionen notieren
2. Primärfarben, Oberflächen, Akzente identifizieren (max. 12 Kernfarben zuerst)
3. Typografie-Skala messen (H1–Body–Caption, min. 3 Stufen)
4. Spacing-Raster ableiten (4px/8px-Basis üblich)
5. Neue Tokens in `docs/design-tokens.md` (Referenz) eintragen
6. Kontext + Verwendungsmuster in `docs/design-memory.md` ergänzen
7. ResourceDictionary-Mapping vorbereiten (`@avalonia-design-system`)

## Avalonia-Mapping

```xml
<!-- App.axaml / Theme.axaml -->
<Color x:Key="ColorSurfacePrimary">#0F172A</Color>
<Thickness x:Key="Spacing6">24</Thickness>
<CornerRadius x:Key="RadiusLg">12</CornerRadius>
```

## Qualitäts-Check vor Implement

- [ ] Jede Farbe im Mockup hat einen Token-Namen
- [ ] Spacing-Werte auf 4px/8px-Raster gerundet (oder Mockup-exakt dokumentiert)
- [ ] Kein Token ohne Eintrag in beiden Docs (tokens + memory)
- [ ] `@design-memory-updater` bei neuen Verwendungsmustern aufrufen

# Output

- `docs/design-tokens.md` — maschinenlesbare Token-Referenz (Name, Wert, Kategorie)
- `docs/design-memory.md` — semantischer Kontext (wann welcher Token, Patterns)
- Kurze Design Spec für aktuellen Bereich (optional in Region-Notiz)

# Rule

> **Tokens wiederverwenden, niemals erfinden.** Fehlt ein Wert im Mockup → User fragen. Kein `#1E293B` hardcoded in AXAML ohne Token-Eintrag.

# Verweise

- Docs: `docs/design-tokens.md`, `docs/design-memory.md`
- Ergänzt: `@figma-analysis`, `@design-memory-updater`
- Legacy: `@avalonia-design-system`
- Rule: `02-pixel-perfect`, `10-anti-patterns`
