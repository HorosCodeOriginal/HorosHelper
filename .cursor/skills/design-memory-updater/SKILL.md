---
name: design-memory-updater
description: HorosCode design-memory.md pflegen — Spacing, Typo, Farben, Radius, Patterns bei neuen Entdeckungen.
---

# Purpose

`docs/design-memory.md` als **lebende Design-Wissensbasis** aktuell halten. Jede neu im Mockup entdeckte Pattern oder Token-Variante wird dokumentiert — nicht nur in AXAML hardcoded.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Works with:** `@design-token-extractor`

# Update

Wann `design-memory.md` ergänzen:

| Trigger | Was eintragen |
|---------|---------------|
| Neuer Mockup-Bereich analysiert | Region-spezifische Token-Verwendung |
| Neues Spacing-Muster | z. B. „Card-Grid: 16px gap, 24px padding" |
| Neue Typografie-Kombination | z. B. „Section-Header: 13px SemiBold, uppercase" |
| Neue Farbkombination | z. B. „Active-Nav: ColorAccent auf ColorSurfaceElevated" |
| Neuer Radius/Shadow | z. B. „Modal: RadiusLg + ElevationModal" |
| Wiederkehrendes Layout-Pattern | z. B. „Sidebar-Item: 40px height, 12px left indent" |

## Update-Workflow

1. Während `@figma-analysis` oder `@design-token-extractor` — neue Werte identifizieren
2. Prüfen: existiert Token schon in `docs/design-tokens.md`?
3. Wenn ja: Verwendungskontext in `design-memory.md` ergänzen
4. Wenn nein: erst Token in `design-tokens.md`, dann Memory-Eintrag
5. ResourceDictionary-Sync notieren (`@avalonia-design-system`)

## Memory-Eintrag-Format

```markdown
### Sidebar Navigation Item
- **Height:** 40px (Spacing10)
- **Padding:** 12px 16px (Spacing3 Spacing4)
- **Active:** Background=ColorAccentMuted, Foreground=ColorAccentPrimary
- **Icon:** 20×20, ColorIconSecondary, 8px gap to label
- **First seen:** Sidebar region, mockup-v3.png
```

## Sync mit ResourceDictionary

Nach Memory-Update prüfen, ob `App.axaml` / Theme-Dateien den Token-Werten entsprechen. Abweichung zwischen Memory und ResourceDictionary = sofort beheben.

# Output

- Aktualisierte `docs/design-memory.md` (semantischer Kontext)
- Querverweis zu `docs/design-tokens.md` (maschinenlesbare Werte)
- Keine stillen Hardcodes in AXAML ohne Memory-Eintrag

# Rule

> **Bei neuen Patterns sofort updaten.** Was nicht in design-memory steht, darf nicht als Projekt-Standard gelten. Never invent — nur Mockup-belegte Werte.

# Verweise

- Docs: `docs/design-memory.md`, `docs/design-tokens.md`
- Works with: `@design-token-extractor`, `@figma-analysis`, `@pixel-perfect-ui`
- Legacy: `@avalonia-design-system`
- Rule: `02-pixel-perfect`, `10-anti-patterns`
