# Regions — {{ASSET_SLUG}}

> **HorosCode** · Human-readable Übersicht aller visuellen Bereiche
> Asset-Ordner: `{{ASSET_FOLDER_PATH}}` · Maschinenlesbar: [`regions.json`](./regions.json)

---

## Übersicht

| # | Slug | Label (DE) | usage | Bounds (px) | Export | zIndex |
|---|------|------------|-------|-------------|--------|--------|
| 1 | `background` | Hintergrund | `bg` | `0,0 — {{FULL_WIDTH}}×{{FULL_HEIGHT}}` | [`background.{{EXT}}`](./background.{{EXT}}) | 0 |
| 2 | `icon-shield` | Schild-Icon | `icon` | `{{ICON_BOUNDS}}` | [`icon-shield.{{EXT}}`](./icon-shield.{{EXT}}) | 10 |
| 3 | `glow-amber` | Amber-Glow | `overlay` | `{{GLOW_BOUNDS}}` | [`glow-amber.{{EXT}}`](./glow-amber.{{EXT}}) | 5 |
| 4 | `text-area` | Textfläche | `text-safe` | `{{TEXT_BOUNDS}}` | — (nur bounds) | 1 |

**Gesamtbild (canonical):** [`../full.{{EXT}}`](../full.{{EXT}}) · **Haupt-Manifest:** [`../{{ASSET_SLUG}}.manifest.md`](../{{ASSET_SLUG}}.manifest.md)

---

## Single-Layer (falls zutreffend)

Wenn dieses Asset **keine** sinnvolle Zerlegung hat:

```text
single-layer: true
Grund: {{SINGLE_LAYER_REASON}}
```

Nur `full.{{EXT}}` verwenden — keine separaten Region-PNGs nötig.

---

## 9-Slice (falls zutreffend)

| sliceRole | Slug | Stretch |
|-----------|------|---------|
| top-left | `{{SLICE_TL}}` | nein |
| top-center | `{{SLICE_TC}}` | horizontal |
| … | … | … |

---

## Workflow für App-Übernahme

1. `full.{{EXT}}` als Referenz / Fallback
2. Layer nach `zIndex` stapeln (`background` → `glow` → `icon`)
3. Position aus `bounds` in `regions.json` als % oder absolute px
4. Mini-Manifest pro Region: `{slug}.manifest.md` (Hex, Zweck, TSX-Snippet)

---

## Verknüpfte Dateien

| Datei | Rolle |
|-------|-------|
| `regions.json` | Maschinenlesbare Definition (Schema v1.0) |
| `*.manifest.md` | Mini-Manifest pro exportierter Region |
| `../{{ASSET_SLUG}}.manifest.md` | Haupt-Manifest inkl. Regions-Tabelle |

---

*© HorosCode · Regions-README für HorosCloud Asset `{{ASSET_SLUG}}`.*
