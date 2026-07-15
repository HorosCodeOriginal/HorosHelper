---
name: horos-image-manifest
description: >-
  HorosCode Pflicht-Manifest für jedes generierte, optimierte oder eingebettete
  Raster-Asset — sehr detaillierte Markdown-Datei mit Hex-Farben, Dimensionen,
  Prompts, Design-Tokens und 1:1-Einbindungs-Snippets. Verwenden bei GenerateImage,
  Bria, OpenAI, optimize, embed, remove-background — kein Erfolg ohne Manifest.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "2.0.0"
---

# Horos Image Manifest — 1:1-Asset-Dokumentation (Pflicht)

**HorosCode**-Skill für **HorosCloud**. Jedes generierte, optimierte, freigestellte oder eingebettete Raster-Asset **MUSS** in einem **eigenen Asset-Ordner** mit begleitendem Markdown-Manifest und **`regions/`**-Zerlegung liegen — damit Bilder und einzelne visuelle Bereiche **lupenrein 1:1** in die Anwendung übernommen werden können.

**Rule (auto bei Bild-Workflows):** `@image-asset` (Alias: `@image-manifest`) · **Template:** `TEMPLATE.md` · **Regions:** `@horos-image-regions`

---

## Wann diesen Skill nutzen (PFLICHT)

**Immer** — unmittelbar nach Erstellung oder Änderung eines Bild-Outputs:

| Auslöser | Skill / Tool | Manifest nötig |
|----------|--------------|----------------|
| Cursor **GenerateImage** | `@cursor-image-generation` | ✅ sofort nach Speichern |
| OpenAI gpt-image-2 | `@generating-images` | ✅ nach Script-Output |
| Bria Generierung / Edit | `@bria-ai` | ✅ nach Download |
| Hintergrund entfernen | `@remove-background` | ✅ nach Cutout-PNG |
| Web-Optimierung (WebP, srcset, OG) | `@horos-web-image-optimize` | ✅ aktualisieren oder neu |
| End-to-End-Pipeline | `@horos-image-embed` | ✅ nach jedem Pipeline-Schritt |
| Manueller Import + Optimierung | `@image-utils` CLI | ✅ wenn Asset für HorosCloud bestimmt |

**Kein Erfolg melden**, bevor Asset-Ordner, Manifest, `regions/regions.json` + `regions/README.md` existieren und mit `Read` verifiziert wurden (s. `@horos-image-regions`).

---

## Speicher-Schema (verbindlich)

### Primärregel: Asset-Ordner mit Manifest + Regions

Jedes logische Asset lebt in **einem eigenen Ordner** `{parent}/{asset-slug}/`:

```text
{parent}/{asset-slug}/
  full.{ext}                    # Gesamtbild (canonical Master)
  {asset-slug}.manifest.md      # Dieses Manifest
  regions/
    regions.json
    README.md
    {region-slug}.{ext}
    {region-slug}.manifest.md
```

| Asset-Ordner | Manifest-Pfad | Canonical Master |
|--------------|---------------|------------------|
| `public/images/hero-backup/` | `public/images/hero-backup/hero-backup.manifest.md` | `public/images/hero-backup/full.webp` |
| `public/icons/widget-notizen/` | `public/icons/widget-notizen/widget-notizen.manifest.md` | `public/icons/widget-notizen/full.png` |
| `assets/images/hero-draft/` | `assets/images/hero-draft/hero-draft.manifest.md` | `assets/images/hero-draft/full.png` |

Details, Schema, Crop-Workflow: **`@horos-image-regions`** · `.cursor/skills/horos-image-regions/SKILL.md`

### Asset-Slug / Canonical-Basename

- Ordner-Name = `{asset-slug}` in Kebab-Case (`hero-backup`, `widget-notizen`)
- **Ohne** Breiten-Suffixe: `-640w`, `-960w`, `-1280w`, `-1920w`
- **Ohne** OG-Suffix: `-og` — OG in Sektion **Format → srcset-Varianten**
- **Eine** Manifest-Datei pro Asset-Ordner; srcset-Varianten im Manifest gelistet

### Migration: Legacy Flat Files

```text
# Alt (Legacy — weiterhin in Manifest-Docs referenzierbar)
public/images/hero-backup.webp
public/images/hero-backup.manifest.md

# Neu (Pflicht ab v2)
public/images/hero-backup/full.webp
public/images/hero-backup/hero-backup.manifest.md
public/images/hero-backup/regions/...
```

Bei nächster Bearbeitung: Datei nach `{slug}/full.{ext}` verschieben, Manifest in Ordner, `regions/` anlegen, TSX-Pfade aktualisieren.

### Ausnahme: Zentrale Manifest-Ablage (nur Drafts)

Wenn ausschließlich unter `assets/` gearbeitet wird **und** der User zentrale Ablage verlangt:

```text
assets/images/{asset-slug}/           # Ordner-Pflicht bleibt
assets/manifests/{asset-slug}.manifest.md   # optional statt co-located
```

**Regel:** Asset-Ordner mit `full` + `regions/` ist **immer** Pflicht. Nur Manifest-Pfad darf zentral sein — Default ist Manifest **im Asset-Ordner**.

### Dateiname

- Suffix: `.manifest.md` (nicht `.md` allein)
- Name: `{asset-slug}.manifest.md` im Asset-Ordner
- Keine Leerzeichen; Kebab-Case

---

## Workflow (verbindliche Reihenfolge)

```text
1. Bild erzeugen / optimieren / freistellen → in {parent}/{asset-slug}/full.{ext}
2. Asset-Ordner + regions/ anlegen (@horos-image-regions)
3. Metadaten sammeln (CLI --info, Dateigröße, Dominant Colors)
4. Manifest aus TEMPLATE.md befüllen → {asset-slug}.manifest.md
5. regions.json, README, Region-Slices + Mini-Manifests
6. Manifest + regions mit Read verifizieren (keine {{PLACEHOLDER}})
7. Erst dann Erfolg melden
```

Bei **Iteration** (v2, Delta-Prompt): bestehendes Manifest **aktualisieren** — `Version / Iteration` hochzählen, Delta-Tabelle ergänzen, `Letzte Änderung` setzen.

Bei **Optimierung** (PNG → WebP, srcset): Manifest **aktualisieren** — Sektionen Format, Dimensionen, Verwandte Dateien, Einbindung 1:1 anpassen.

### Integration mit `@horos-image-embed`

```text
@horos-image-embed
  → @cursor-image-generation → GenerateImage → {slug}/full.{ext}
  → @horos-image-manifest (Haupt-Manifest)         ← PFLICHT
  → @horos-image-regions (identify → crop → json)  ← PFLICHT
  → @horos-web-image-optimize (full + regions)
  → @horos-image-manifest + @horos-image-regions (aktualisieren)
  → horos-ui / TSX-Einbindung (full oder Layer)
  → Parity + Regions-Sektion                       ← PFLICHT vor „fertig“
```

---

## Metadaten sammeln (Agenten-Pflicht)

### Basis-Metadaten (Pillow CLI)

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input "PFAD/ZUM/BILD.png" \
  --info
```

Liefert JSON: `width`, `height`, `mode`, `format`, `has_alpha`, `aspect_ratio`.

### Dateigröße

```bash
# PowerShell
(Get-Item "PFAD").Length

# Bash
stat -c%s "PFAD"   # Bytes
```

In Manifest: KB (1 Dezimalstelle) und Bytes.

### Dominante Hex-Farben (#RRGGBB)

Nach jedem Asset **mindestens 5 dominante Farben** extrahieren. Einmaliger Python-Aufruf (Pillow):

```bash
python -c "
from PIL import Image
from collections import Counter
import sys
path = sys.argv[1]
img = Image.open(path).convert('RGBA')
# Auf 64x64 quantisieren für Performance
small = img.resize((64, 64))
rgb = small.convert('RGB')
q = rgb.quantize(colors=8, method=2)
palette = q.getpalette()[:24]
counts = Counter(q.getdata())
total = sum(counts.values())
for i, (idx, cnt) in enumerate(counts.most_common(5)):
    r,g,b = palette[idx*3], palette[idx*3+1], palette[idx*3+2]
    pct = round(100 * cnt / total, 1)
    print(f'#{r:02x}{g:02x}{b:02x} rgb({r},{g},{b}) {pct}%')
" "PFAD/ZUM/BILD.png"
```

Ergebnis in Tabelle **Farben → Dominante Hex-Werte** eintragen; **Verwendung** manuell zuordnen (Hintergrund, Akzent, Subjekt).

Zusätzlich: Bild mit `Read` öffnen — visuelle Plausibilität der Hex-Werte prüfen.

### Transparenz

- `has_alpha` aus `--info`
- Bei Cutout: `Transparente Pixel` schätzen oder „100% außerhalb Subjekt“ notieren
- Freistell-Methode: `Bria RMBG-2.0` / `Prompt-Slate-Workaround` / `keine`

---

## Pflicht-Sektionen im Manifest

Jede Sektion aus `TEMPLATE.md` **muss** befüllt sein. Keine `{{PLACEHOLDER}}`, kein `TBD`, kein `TODO`.

| # | Sektion | Inhalt |
|---|---------|--------|
| 1 | **Identität** | Asset-Slug, Ordner-Pfad, `full.{ext}`, Datum, Skill, Agent, Version |
| 2 | **Quelle** | Vollständiger Prompt, Negative, Model/Tool, Referenzbilder |
| 3 | **Dimensionen** | px, AR, DPI, Dateigröße KB (von `full`) |
| 4 | **Farben** | ≥5 dominante `#RRGGBB`, Palette, Transparenz |
| 5 | **Format** | PNG/WebP/JPG, Optimierung, srcset-Tabelle |
| 6 | **Design-System** | HorosCloud Tokens (slate-*, amber-*), Dark-Mode, intended background |
| 7 | **Regions / Bereiche** | Tabelle aller Regionen + Link zu `regions/regions.json` |
| 8 | **Einbindung 1:1** | `<img>` / Next `Image` / `<picture>` / Layered Regions, alt, aria |
| 9 | **Layout** | Container, object-fit, object-position, spacing |
| 10 | **Verwendungszweck** | Widget-Icon / Hero / OG / Doc / Avatar / … |
| 11 | **Parity-Checkliste** | Alle Checkboxen bewertet; Abweichungen in Notizen |
| 12 | **Verwandte Dateien** | WebP, srcset, Region-Slices, TSX, Docs, Voriteration |
| 13 | **Pipeline-Protokoll** | Zeitstempel pro Schritt |

---

## Template (Copy-Paste)

Vollständiges Template mit Platzhaltern: **`.cursor/skills/horos-image-manifest/TEMPLATE.md`**

Kurz-Minimalstruktur zum schnellen Start (danach alle Felder ausfüllen):

```markdown
# Bild-Manifest — {Anzeigename}

> HorosCode · HorosCloud 1:1-Asset

## Identität
| Dateiname | … |
| Pfad | … |
| Skill | @cursor-image-generation |
| Version | v1 |

## Quelle
### Original-Prompt (vollständig)
\`\`\`text
…
\`\`\`

## Dimensionen
| Breite × Höhe | 1280 × 720 px |
| Dateigröße | 142.3 KB |

## Farben
| #1 | #0f172a | 34% | Hintergrund |
…

## Einbindung 1:1
\`\`\`tsx
<img src="/images/…" alt="…" width={1280} height={720} className="…" />
\`\`\`

## Parity-Checkliste
- [x] Maße …
…
```

Für Produktion **immer** das vollständige `TEMPLATE.md` verwenden.

---

## Beispiel: Widget-Icon nach GenerateImage

**Ordner:** `HorosCloudV5/apps/web/public/icons/widget-notizen/`  
**Master:** `…/widget-notizen/full.png`  
**Manifest:** `…/widget-notizen/widget-notizen.manifest.md`  
**Regions:** `…/widget-notizen/regions/` (z. B. `foreground.png`, `glow-amber.png`)

Auszug (gekürzt — echtes Manifest ist vollständig):

```markdown
## Farben
| 1 | `#0f172a` | rgb(15,23,42) | 41.2% | Slate-Hintergrund |
| 2 | `#f59e0b` | rgb(245,158,11) | 18.7% | Amber-Symbol |

## Einbindung 1:1
\`\`\`tsx
<img
  src="/icons/widget-notizen/full.png"
  alt=""
  aria-hidden
  width={512}
  height={512}
  className="h-10 w-10 shrink-0 object-contain"
/>
\`\`\`

## Verwendungszweck
Widget-Icon — Notizen-Card in Settings-Dashboard
```

---

## Verifikation vor Erfolg (BLOCKER)

1. Asset-Ordner `{parent}/{asset-slug}/` existiert mit `full.{ext}`
2. Manifest `{asset-slug}.manifest.md` im Asset-Ordner
3. `regions/regions.json` + `regions/README.md` vorhanden
4. `Read` auf Manifest — keine unersetzten `{{…}}`-Platzhalter
5. Alle 13 Pflicht-Sektionen mit echten Werten (inkl. **Regions / Bereiche**)
6. Hex-Tabelle hat ≥5 Einträge
7. Mindestens ein Einbindungs-Snippet (`<img>`, `<picture>` oder Layered Regions)
8. `Read` auf `full` + mindestens eine Region (oder single-layer in README)
9. Parity-Checkliste: jede Zeile `[x]` oder `[ ]` mit Begründung

**Fehlt Ordner, Manifest oder regions/ → Aufgabe NICHT abgeschlossen** (siehe Rule `@image-asset`).

---

## Agent-Zuordnung

| Agent | Verantwortung |
|-------|---------------|
| **Zuko** | Asset-Ordner, Manifest, Regions nach GenerateImage + Optimierung |
| **horos-ui** | Manifest um TSX-Pfade und Parity nach Einbindung ergänzen |
| **Jeder Executor** | Der das Bild schreibt — Manifest ist Teil derselben Aufgabe |

---

## Verknüpfte Skills

| Skill | Beziehung |
|-------|-----------|
| `@horos-image-embed` | Orchestriert Pipeline inkl. Manifest + Regions |
| `@horos-image-regions` | Ordner-Schema, regions.json, Slices, Crop |
| `@cursor-image-generation` | Liefert Prompt für Quelle-Sektion |
| `@horos-web-image-optimize` | Liefert srcset/OG für Format-Sektion |
| `@horos-design-system` | Tokens für Design-System-Sektion |
| `@generating-images` | OpenAI-Metadaten in Quelle |
| `@bria-ai` / `@remove-background` | Cutout-Metadaten, Alpha |
| `@image-utils` | CLI `--info`, Optimierung |

---

## Referenzen

- Template: `.cursor/skills/horos-image-manifest/TEMPLATE.md`
- Rule: `.cursor/rules/image-asset.mdc` (Alias: `image-manifest.mdc`)
- Regions: `.cursor/skills/horos-image-regions/SKILL.md`
- Design-Tokens: `.cursor/skills/horos-design-system/SKILL.md`
- CLI: `.cursor/skills/image-utils/scripts/optimize_web_image.py`
- Firma/Produkt: `.cursor/rules/horoscode.mdc`
