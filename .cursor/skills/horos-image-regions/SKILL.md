---
name: horos-image-regions
description: >-
  HorosCode Pflicht-Regionen-Zerlegung für jedes Raster-Asset — eigener Ordner
  pro Bild mit full.{ext}, Manifest, regions.json und exportierten Slices für
  1:1-Übernahme einzelner UI-Zonen in HorosCloud. Verwenden nach GenerateImage,
  optimize, embed, remove-background — kein Erfolg ohne Ordner + regions/.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "1.0.0"
---

# Horos Image Regions — Visuelle Bereiche für 1:1-App-Übernahme (Pflicht)

**HorosCode**-Skill für **HorosCloud**. Jedes generierte, optimierte oder eingebettete Raster-Asset **MUSS** in einem **eigenen Asset-Ordner** liegen — mit kanonischem Gesamtbild, Manifest und strukturierter **`regions/`**-Zerlegung, damit einzelne visuelle Bereiche (Icons, Badges, Hintergründe, Textflächen) **lupenrein 1:1** in die Anwendung übernommen werden können.

**Rule (auto bei Bild-Workflows):** `@image-asset` (ehem. `@image-manifest`) · **Schema:** `regions.schema.json` · **Templates:** `TEMPLATE-regions.json`, `TEMPLATE-region-manifest.md`, `TEMPLATE-regions-README.md`

---

## Wann diesen Skill nutzen (PFLICHT)

**Immer** — unmittelbar nach Erstellung oder Änderung eines Bild-Outputs, **zusammen** mit `@horos-image-manifest`:

| Auslöser | Skill / Tool | Regions nötig |
|----------|--------------|---------------|
| Cursor **GenerateImage** | `@cursor-image-generation` | ✅ nach Speichern in `{slug}/` |
| OpenAI gpt-image-2 | `@generating-images` | ✅ nach Script-Output |
| Bria Generierung / Edit | `@bria-ai` | ✅ nach Download |
| Hintergrund entfernen | `@remove-background` | ✅ — oft neue Layer (fg/bg/glow) |
| Web-Optimierung | `@horos-web-image-optimize` | ✅ `full.{ext}` + Region-Slices aktualisieren |
| End-to-End-Pipeline | `@horos-image-embed` | ✅ nach jedem Pipeline-Schritt |
| Manueller Import | `@image-utils` CLI | ✅ wenn Asset für HorosCloud bestimmt |

**Kein Erfolg melden**, bevor Asset-Ordner, Manifest, `regions/regions.json`, `regions/README.md` existieren und mindestens `full` + **eine Region** (oder dokumentiertes Single-Layer) mit `Read` verifiziert wurden.

---

## Ordner-Schema (Default, verbindlich)

Jedes logische Asset lebt in **einem eigenen Ordner** `{parent}/{asset-slug}/`:

```text
{parent}/{asset-slug}/
  full.{ext}                    # Gesamtbild (canonical Master)
  {asset-slug}.manifest.md      # Haupt-Manifest (s. @horos-image-manifest)
  regions/
    regions.json                # Maschinenlesbare Region-Definition
    README.md                   # Human-readable Übersicht aller Bereiche
    {region-slug}.{ext}         # z. B. background.png, icon-center.png
    {region-slug}.manifest.md   # Mini-Manifest pro Region (Hex, Bounds, Zweck)
```

### Beispiel (Hero Backup)

```text
HorosCloudV5/apps/web/public/images/hero-backup/
  full.webp
  hero-backup.manifest.md
  regions/
    regions.json
    README.md
    background.webp
    background.manifest.md
    icon-shield.webp
    icon-shield.manifest.md
    glow-amber.webp
    glow-amber.manifest.md
    text-area.webp
    text-area.manifest.md
```

**URLs (Vite `public/`):**

| Datei | URL |
|-------|-----|
| Gesamtbild | `/images/hero-backup/full.webp` |
| Region Shield | `/images/hero-backup/regions/icon-shield.webp` |

### Asset-Slug

- Kebab-Case, beschreibend: `hero-backup`, `widget-notizen`, `badge-premium`
- **Ohne** Breiten-Suffixe (`-640w`), **ohne** OG-Suffix (`-og`) im Ordnernamen
- srcset/OG-Varianten werden im Haupt-Manifest gelistet; Region-Slices beziehen sich auf `full.{ext}`

### Parent-Verzeichnisse (HorosCloudV5)

| Zweck | Parent |
|-------|--------|
| Hero / Illustration | `HorosCloudV5/apps/web/public/images/` |
| Icons | `HorosCloudV5/apps/web/public/icons/` |
| Drafts | `assets/images/` |
| Doc-Illustration | `HorosCloudV5/docs/screenshots/<feature-slug>/` |

### Migration: Legacy Flat Files

Bestehende flache Dateien **ohne** Ordner:

```text
# Alt (Legacy)
public/images/hero-backup.webp
public/images/hero-backup.manifest.md

# Neu (Pflicht)
public/images/hero-backup/full.webp
public/images/hero-backup/hero-backup.manifest.md
public/images/hero-backup/regions/...
```

**Regel:** Bei nächster Bearbeitung migrieren — Datei nach `{slug}/full.{ext}` verschieben, Manifest in Ordner, `regions/` anlegen. Alte Pfade in TSX aktualisieren oder Redirect im Manifest dokumentieren.

---

## Was ist ein „Bereich“ (Region)?

Eine **Region** ist ein semantisch oder visuell zusammenhängender Ausschnitt des Gesamtbildes, der **einzeln** in die App übernommen werden kann.

### Semantische UI-Zonen

| Typ | Beispiel | `usage` in regions.json |
|-----|----------|---------------------------|
| Icon / Symbol | Shield, Cloud, Widget-Glyph | `icon` |
| Badge / Label | „Pro“, „Neu“-Ribbon | `overlay` |
| Hintergrund | Slate-Fläche, Gradient, Pattern | `bg` |
| Textfläche | Leerer Bereich für UI-Text | `text-safe` |
| Dekor | Partikel, Linien, Ornament | `decorative` |
| Schatten / Glow | Amber-Glow, Drop-Shadow | `overlay` |
| CTA-Bereich | Button-Zone im Mockup-Still | `text-safe` oder `overlay` |

### Nach Bildtyp

| Bildtyp | Typische Regionen |
|---------|-------------------|
| **Flache Illustration** | Dominante visuelle Cluster (Figur, Boden, Himmel, Akzent) |
| **Icon** | `foreground`, `background`, `glow`, optional `badge` |
| **UI-Mockup-Still** | Device-Frame, Screen-Content, Schatten, Dekor |
| **Hero** | `background`, Hauptmotiv, Text-Safe-Zone, Glow, Badge |
| **Stretchbare UI-BG** | Optional **9-Slice-Grid** (s. unten) |

### 9-Slice (optional)

Für **stretchbare** Panel-Hintergründe: neun Regionen mit festen Ecken/Kanten und stretchbarem Center:

```text
top-left    top-center    top-right
mid-left    center        mid-right
bottom-left bottom-center bottom-right
```

In `regions.json`: `nineSlice: true` auf Asset-Ebene; jede Region `usage: "bg"` mit `sliceRole`: `top-left` … `bottom-right`.

### Wann **nicht** slicen

| Situation | Vorgehen |
|-----------|----------|
| Reines 1-Layer-Icon ohne sinnvolle Trennung | Ordner **trotzdem**; `regions/README.md` → „single-layer“; `regions.json` mit einer Region `full` oder nur Metadaten ohne separate PNGs |
| Foto ohne UI-Semantik | Mindestens `background` + `subject` wenn visuell trennbar; sonst single-layer dokumentieren |
| User verlangt explizit „nur eine Datei“ | Ordner + `full` + Manifest + `regions.json` mit `regions: []` und README-Begründung |

**Kein ML-Segmentierung-Versprechen:** Regionen werden per **visueller Analyse** (`Read` auf Bild), User-/Mockup-Spec oder **Pillow-Crop** (`@image-utils`) identifiziert — nicht per automatischer KI-Segmentierung, sofern nicht explizit dokumentiert.

---

## regions.json Schema

Maschinenlesbare Definition — validiert gegen `regions.schema.json`.

### Asset-Ebene

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `schemaVersion` | string | `"1.0"` |
| `assetSlug` | string | Ordner-Name |
| `parentFullPath` | string | Workspace-relativer Pfad zu `full.{ext}` |
| `fullWidth` / `fullHeight` | number | px des Gesamtbildes |
| `nineSlice` | boolean | Optional — 9-Slice-Grid aktiv |
| `regions` | array | Liste der Region-Objekte |

### Pro Region

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `id` | string | Stabile ID (`region-icon-shield`) |
| `slug` | string | Dateiname ohne Extension (`icon-shield`) |
| `label` | object | `{ "de": "...", "en": "..." }` |
| `bounds` | object | `{ "x", "y", "width", "height" }` in px relativ zu `full` |
| `dominantHex` | string[] | ≥1 `#RRGGBB` aus Region-Crop |
| `transparent` | boolean | Alpha in Region |
| `zIndex` / `layer` | number | Stapelreihenfolge (0 = hinten) |
| `usage` | enum | `icon` \| `bg` \| `overlay` \| `text-safe` \| `decorative` |
| `exportPath` | string | Relativ zum Asset-Ordner (`regions/icon-shield.webp`) |
| `parentFullPath` | string | Wie Asset-Ebene |
| `sliceRole` | string | Optional bei 9-Slice |
| `notes` | string | Optional — DE/EN Kurznotiz |

**Template:** `TEMPLATE-regions.json` · **Schema:** `regions.schema.json`

---

## Workflow (verbindliche Reihenfolge)

```text
1. Bild generieren → in {parent}/{asset-slug}/full.{ext} ablegen
2. @horos-image-manifest — {asset-slug}.manifest.md im selben Ordner
3. Bereiche identifizieren:
   a) Read auf full.{ext} — visuelle Cluster / UI-Zonen
   b) User-Briefing / Mockup-Spec / Figma-Annotationen
   c) Optional @remove-background — isolierte Layer als eigene Region
4. Pro Region:
   a) Bounds festlegen (x, y, width, height)
   b) Pillow crop (@image-utils) → regions/{region-slug}.{ext}
   c) Dominant-Hex aus Crop extrahieren
   d) Mini-Manifest aus TEMPLATE-region-manifest.md
5. regions/regions.json schreiben (alle Regionen)
6. regions/README.md — Tabelle + Zweck pro Bereich
7. Haupt-Manifest: Sektion „Regions / Bereiche“ aktualisieren
8. Read-Verify: full.{ext} + mindestens 1 Region (oder single-layer README)
9. Erst dann Erfolg melden
```

### Pillow-Crop (Agenten)

```bash
python -c "
from PIL import Image
import sys
path, out, x, y, w, h = sys.argv[1:7]
img = Image.open(path)
crop = img.crop((int(x), int(y), int(x)+int(w), int(y)+int(h)))
crop.save(out)
print(f'Saved {out} {w}x{h}')
" \"{parent}/{slug}/full.png\" \"{parent}/{slug}/regions/icon-shield.png\" 120 80 256 256
```

Oder `ImageUtils.crop()` aus `@image-utils` — `left, top, right, bottom` = `x, y, x+width, y+height`.

### Integration mit `@horos-image-embed`

```text
@horos-image-embed
  → @cursor-image-generation → GenerateImage → {slug}/full.png
  → @horos-image-manifest (Haupt-Manifest)
  → @horos-image-regions (identify → crop → regions.json + Slices)
  → @horos-web-image-optimize (full + Regionen → WebP)
  → @horos-image-manifest + @horos-image-regions (aktualisieren)
  → horos-ui / TSX — einzelne Region-Pfade oder composite
```

---

## Einbindung 1:1 (App)

### Gesamtbild

```tsx
<img
  src="/images/hero-backup/full.webp"
  alt="HorosCloud Backup — Hero"
  width={1280}
  height={720}
  className="aspect-video w-full object-cover"
/>
```

### Einzelne Region (Layered UI)

```tsx
<div className="relative aspect-video w-full">
  <img
    src="/images/hero-backup/regions/background.webp"
    alt=""
    aria-hidden
    className="absolute inset-0 h-full w-full object-cover"
  />
  <img
    src="/images/hero-backup/regions/icon-shield.webp"
    alt=""
    aria-hidden
    className="absolute left-[9.4%] top-[11.1%] h-[35.6%] w-[20%] object-contain"
  />
  <img
    src="/images/hero-backup/regions/glow-amber.webp"
    alt=""
    aria-hidden
    className="pointer-events-none absolute inset-0 mix-blend-screen opacity-80"
  />
</div>
```

Positionen aus `bounds` + `fullWidth`/`fullHeight` als Prozent oder absolute px ableiten.

---

## Verifikation vor Erfolg (BLOCKER)

1. Ordner `{parent}/{asset-slug}/` existiert
2. `full.{ext}` existiert und mit `Read` plausibel
3. `{asset-slug}.manifest.md` vollständig (s. `@horos-image-manifest`)
4. `regions/regions.json` validiert gegen Schema (Felder vollständig)
5. `regions/README.md` listet alle Bereiche
6. Jede Region in `regions.json` mit `exportPath` — Datei existiert **oder** single-layer in README begründet
7. Pro exportierter Region: `{region-slug}.manifest.md` mit Bounds + Hex
8. `Read` auf `full` + **mindestens eine** Region-Datei (außer dokumentiertes single-layer)
9. Haupt-Manifest enthält Sektion **Regions / Bereiche** mit Tabelle + Link zu `regions.json`

**Fehlt Ordner, regions.json oder README → Aufgabe NICHT abgeschlossen** (s. Rule `@image-asset`).

---

## Agent-Zuordnung

| Agent | Verantwortung |
|-------|---------------|
| **Zuko** | Ordner anlegen, Regionen identifizieren, croppen, regions.json |
| **horos-ui** | TSX mit Region-Pfaden; Layer-Position aus bounds |
| **Jeder Executor** | Der das Bild schreibt — Regions sind Teil derselben Aufgabe |

---

## Verknüpfte Skills

| Skill | Beziehung |
|-------|-----------|
| `@horos-image-manifest` | Haupt-Manifest im Asset-Ordner; Regions-Sektion |
| `@horos-image-embed` | Orchestriert Pipeline inkl. Regions |
| `@cursor-image-generation` | Output-Ziel = `{slug}/full.{ext}` |
| `@horos-web-image-optimize` | Optimiert `full` + `regions/*` |
| `@image-utils` | Pillow crop, `--info`, Dominant Colors |
| `@remove-background` | Optional — fg/bg als separate Regionen |
| **Zuko** | Standard-Agent für Region-Workflow |

---

## Referenzen

- Schema: `.cursor/skills/horos-image-regions/regions.schema.json`
- Templates: `TEMPLATE-regions.json`, `TEMPLATE-region-manifest.md`, `TEMPLATE-regions-README.md`
- Manifest: `.cursor/skills/horos-image-manifest/SKILL.md`
- Rule: `.cursor/rules/image-asset.mdc`
- Crop-API: `.cursor/skills/image-utils/SKILL.md`
- Firma/Produkt: `.cursor/rules/horoscode.mdc`
