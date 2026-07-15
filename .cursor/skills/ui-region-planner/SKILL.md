---
name: ui-region-planner
description: HorosCode Mockup-Zerlegung in UI-Regionen mit Implementierungsreihenfolge — kein Code vor Regionen.
---

# Purpose

Große Mockups in **implementierbare Regionen** zerlegen, bevor ein einziges AXAML geschrieben wird. Ergänzt `@incremental-ui` mit strukturierter Planung und dokumentierter Reihenfolge.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Stack:** C# / Avalonia / MVVM

# Regions

Standard-Regionstypen (nach Bedarf kombinieren oder unterteilen):

| Typ | Beispiel | Typische Abhängigkeit |
|-----|----------|----------------------|
| **Header** | App-Bar, Logo, User-Menu | Keine |
| **Navigation** | Top-Nav, Breadcrumbs | Nach Header |
| **Sidebar** | Linke/rechte Leiste | Nach Header |
| **Content** | Hauptbereich, Dashboard-Grid | Nach Shell-Struktur |
| **Cards** | KPI-Karten, List-Items | Innerhalb Content |
| **Forms** | Einstellungen, Dialog-Inhalte | Nach Content-Layout |
| **Dialogs** | Modal, Flyout, Drawer | Nach Basis-Komponenten |
| **Footer** | Status-Bar, Copyright | Nach Content |

## Zerlegungs-Workflow

1. Mockup/Figma-Frame öffnen — Gesamtdimensionen notieren (z. B. `1440×900`)
2. Visuelle Grenzen markieren (Header-Höhe, Sidebar-Breite in px)
3. Jede Region in `docs/ui-regions.md` eintragen mit Status `pending`
4. **Implementierungsreihenfolge** festlegen (top-down, Shell zuletzt)
5. Abhängigkeiten dokumentieren (Sidebar braucht Header-Höhe als Offset)

## Typische Reihenfolge

```
Header → Navigation → Sidebar → Toolbar → Content Panel → Cards/Forms → Dialogs → Footer → Shell Assembly
```

Shell Assembly ist **immer letzter** Schritt — erst wenn alle Regionen ≥ 98 und freigegeben.

# Output

Aktualisierte `docs/ui-regions.md` mit:

```markdown
| Region | Status | Mockup-Ref | Dimensionen | Abhängigkeit | Reihenfolge |
|--------|--------|------------|-------------|--------------|-------------|
| Header | pending | mockup-header.png | 1440×56 | — | 1 |
| Sidebar | pending | mockup-sidebar.png | 240×844 | Header | 2 |
```

Plus kurze Implementierungsreihenfolge als nummerierte Liste.

# Rule

> **Niemals Code schreiben, bevor alle Regionen definiert und in `docs/ui-regions.md` dokumentiert sind.** Ein Bereich pro Task — Planung ist kein Implementierungs-Pass.

# Verweise

- Docs: `docs/ui-regions.md`
- Ergänzt: `@incremental-ui`, `@figma-analysis`
- Rule: `01-workflow`, `03-mockup-analysis`
- Agent: `avalonia-ui` · Command: `/mockup-avalonia`
