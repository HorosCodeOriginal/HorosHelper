---
name: figma-analysis
description: HorosCode Mockup/Figma-Analyse vor Code — Tokens, Spec, Regionen; kein AXAML vor Abschluss.
---

# Purpose

Strukturierte Extraktion aus Figma-Frame oder Screenshot **bevor** die erste AXAML-Zeile geschrieben wird.

**Firma:** HorosCode · **Rule:** `03-mockup-analysis`

# Extract

| Dimension | Was messen | Wo ablegen |
|-----------|------------|------------|
| **Colors** | Hex/RGB, Opacity, Gradients | `docs/design-tokens.md`, `design-memory.md` |
| **Spacing** | Margin, Padding, Gap (px) | Token-Namen (`Spacing4`, `Spacing6`) |
| **Typography** | Family, Size, Weight, LineHeight | `FontSize*`, `FontWeight*` |
| **Radius** | BorderRadius pro Ecke | `RadiusSm`, `RadiusMd` |
| **Shadows** | Offset, Blur, Spread, Color | `ShadowCard`, `ShadowElevated` |
| **Hierarchy** | Z-Order, Gruppen, Auto-Layout | `docs/ui-regions.md` |

## Figma MCP

1. Frame-Link oder Node-ID aus User-Input
2. Figma MCP laden (`.cursor/mcp.json`)
3. Layer-Hierarchie → Regionen-Zerlegung
4. Auto-Layout-Werte → Spacing-Tokens

Ohne Figma: Screenshot + Grid-Overlay (~70–85 % Startparität).

# Output (Design Spec)

Kurzes Spec-Dokument pro Bereich (in `docs/ui-regions.md` oder Task-Kommentar):

```markdown
## Region: Header (1440×56)
- Background: ColorSurfacePrimary (#0F172A)
- Padding: 24px horizontal, 12px vertical
- Nav: FontSize13, Medium, gap 32px
- Components: Logo, NavLinks, UserAvatar, SearchIcon
- States: active nav = ColorAccentPrimary
```

Region-Status: `pending` → `in_progress` → `review` → `approved`

# Rule

> **Kein Code vor Analyse.** Erst Spec + Token-Check + Komponenten-Search (`docs/component-catalog.md`), dann Implement.

# Verweise

- Docs: `docs/design-tokens.md`, `docs/ui-regions.md`, `docs/design-memory.md`
- Workflow: `@incremental-ui`
- Legacy ergänzend: `@avalonia-figma-to-code` (Orchestrator), `@avalonia-design-system` (Token-Mapping)
