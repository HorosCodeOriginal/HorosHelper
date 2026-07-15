---
name: incremental-ui
description: HorosCode Avalonia Bereich-für-Bereich Workflow — ein Region pro Task, Gates zwischen Schritten.
---

# Purpose

Verhindert Big-Bang-Implementierung. Jeder Task bearbeitet **genau einen UI-Bereich** mit festen Gates zwischen Analyse, Code, Preview und Review.

**Firma:** HorosCode · **Agent:** `avalonia-ui` · **Command:** `/mockup-avalonia`

# Workflow

```
Analyze → Split → Implement → Preview → Review → Fix → Next
```

| Phase | Aktion | Output |
|-------|--------|--------|
| **Analyze** | Mockup lesen, Tokens extrahieren | Design Spec (`@figma-analysis`) |
| **Split** | Bereiche in `docs/ui-regions.md` | Region-Liste mit Status |
| **Implement** | Ein Bereich: AXAML + ViewModel | `HeaderView.axaml`, `HeaderViewModel.cs` |
| **Preview** | Isoliertes Preview-Fenster | `HeaderPreview.axaml` (`@avalonia-preview`) |
| **Review** | Screenshot vs. Mockup | Score ≥ 98 (`@visual-regression`) |
| **Fix** | Diff-Liste abarbeiten | Re-Screenshot (max. 3 Runden) |
| **Next** | Human Approval → nächster Bereich | Status `approved` in `ui-regions.md` |

## Typische Reihenfolge

```
Header → Sidebar → Toolbar → Content Panel → Footer → Shell Assembly
```

Shell Assembly ist **letzter** Schritt — erst wenn alle Regionen ≥ 98 und freigegeben.

# Forbidden

- Header + Sidebar + Content in einem Pass
- Nächster Bereich ohne Human Approval (`12-human-approval`)
- Shell-Merge vor vollständigem Region-Review
- Code vor abgeschlossener Analyse (`@figma-analysis`)
- API/i18n/Backend im ersten UI-Pass (Mock-Daten first)

# Stop Condition

Task endet nach **einem** Bereich mit dokumentiertem Score und Freigabe-Anfrage — nicht mit „fast fertiger App".

# Verweise

- Rule: `01-workflow`, `03-mockup-analysis`, `12-human-approval`
- Docs: `docs/ui-regions.md`, `docs/skills.md`
- Fidelity: `@pixel-perfect-ui`
