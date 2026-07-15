---
name: horos-web-image-optimize
description: >-
  HorosCode Web-Bild-Nachbearbeitung — PNG/JPG nach Generierung oder Import in
  web-taugliche WebP-Assets umwandeln (Resize, optimize_for_web, responsive srcset,
  OG 1200×630). Kein API-Key. Nutzen wenn Raster-Assets für HorosCloudV5/apps/web
  optimiert werden sollen — nach GenerateImage oder bei bestehenden Dateien.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "2.0.0"
---

# Horos Web Image Optimize — Nachbearbeitung für Web

**HorosCode**-Skill für **HorosCloud**-Web-Assets. Deterministische Pixel-Operationen (Pillow) — **kein API-Key**, keine KI-Generierung.

**Schwester-Skills:**

| Skill | Rolle |
|-------|-------|
| **@horos-image-embed** | End-to-End: Generieren → **dieser Skill** → Einbetten |
| **@cursor-image-generation** | Davor: GenerateImage-Prompting |
| **@horos-design-system** | Danach: TSX mit Dark-SaaS-Tokens |
| **@image-utils** | Low-Level-API (`ImageUtils`) — gleiche Logik wie CLI |

---

## Wann diesen Skill nutzen

- **Nach GenerateImage:** PNG liegt in `public/` oder `assets/` → WebP + Größen anpassen.
- User hat **bestehendes PNG/JPG** und will web-ready Assets (ohne neu zu generieren).
- **Responsive srcset** für Hero / Feature-Bilder.
- **Open Graph** / Twitter-Card: 1200×630 WebP.
- **Icons** auf feste Pixelgröße (z. B. 512×512 WebP).

**Nicht** für: Bildgenerierung (→ **@cursor-image-generation**), Freistellen (→ optional **@remove-background**), App-Screenshots (→ **@screenshot-capture**).

---

## CLI (Agenten-Pflicht)

**Script:**

```text
.cursor/skills/image-utils/scripts/optimize_web_image.py
```

Wrappt `references/code-examples/image_utils.py` — **keine duplizierte Logik**.

### Voraussetzungen

```bash
pip install Pillow requests
```

### Hilfe & Exit-Codes

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py --help
```

| Code | Bedeutung |
|------|-----------|
| `0` | Erfolg |
| `1` | Verarbeitungsfehler |
| `2` | Ungültige Args / Datei fehlt |
| `3` | Pillow nicht installiert |

### Befehle (Copy-Paste für Shell)

**Metadaten (vor/nach Optimierung):**

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup.png \
  --info
```

**Einzelnes WebP (optimize_for_web):**

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/full.png \
  --optimize \
  --out HorosCloudV5/apps/web/public/images/hero-backup/full.webp
```

**Resize + WebP:**

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input path/to/source.png \
  --resize 1280 \
  --format webp \
  --quality 85 \
  --out HorosCloudV5/apps/web/public/images/hero-backup.webp
```

**Responsive Varianten** (Standard-Breiten: 640, 960, 1280, 1920):

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/full.png \
  --responsive \
  --out HorosCloudV5/apps/web/public/images/hero-backup/full.webp
```

Erzeugt z. B. `hero-backup-640w.webp`, `hero-backup-960w.webp`, …

**Eigene Breiten:**

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input hero.png \
  --responsive \
  --responsive-widths 480,768,1024 \
  --out HorosCloudV5/apps/web/public/images/hero.webp
```

**Open Graph (1200×630, zentrierter Crop):**

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/full.png \
  --og \
  --out HorosCloudV5/apps/web/public/images/hero-backup/og-full.webp
```

**JSON-Ausgabe (für Agent-Parsing):**

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input hero.png --optimize --json
```

---

## Zielpfade (HorosCloudV5)

| Asset-Typ | Pfad | URL |
|-----------|------|-----|
| Hero / Illustration | `HorosCloudV5/apps/web/public/images/{slug}/full.webp` | `/images/{slug}/full.webp` |
| Region-Slices | `…/public/images/{slug}/regions/*.webp` | `/images/{slug}/regions/…` |
| Widget-Icons | `HorosCloudV5/apps/web/public/icons/{slug}/full.webp` | `/icons/{slug}/full.webp` |
| OG / Social | `…/public/images/{slug}/og-full.webp` oder Parent-OG | `/images/…` |
| Doc-Illustration | `HorosCloudV5/docs/screenshots/<slug>/` | relativ in Docs |

**Regel:** CLI auf `full.{ext}` und jede exportierte Region in `regions/` anwenden. Nach Optimierung Manifest + `regions.json` aktualisieren.

---

## Einbindung nach Optimierung

### Responsive `<picture>` (Tailwind)

```tsx
<picture>
  <source
    type="image/webp"
    srcSet="/images/hero-backup/full-640w.webp 640w, /images/hero-backup/full-960w.webp 960w, /images/hero-backup/full-1280w.webp 1280w, /images/hero-backup/full-1920w.webp 1920w"
    sizes="(max-width: 768px) 100vw, (max-width: 1280px) 80vw, 1200px"
  />
  <img
    src="/images/hero-backup/full-1280w.webp"
    alt="HorosCloud Backup — Dashboard-Illustration"
    className="aspect-video w-full rounded-lg border border-slate-700 object-cover"
    loading="lazy"
    decoding="async"
    width={1280}
    height={720}
  />
</picture>
```

### Einfaches Icon (kein srcset nötig)

```tsx
<img
  src="/icons/widget-notizen/full.webp"
  alt=""
  aria-hidden
  className="h-10 w-10 object-contain"
  width={512}
  height={512}
/>
```

### Open Graph Meta

```html
<meta property="og:image" content="https://<production-domain>/images/hero-backup/og-full.webp" />
<meta property="og:image:width" content="1200" />
<meta property="og:image:height" content="630" />
<meta property="og:image:type" content="image/webp" />
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:image" content="https://<production-domain>/images/hero-backup/og-full.webp" />
```

---

## Entscheidungsbaum

```
Bestehendes Raster (PNG/JPG)?
│
├─ Nur für Web ausliefern → --optimize → .webp in public/
├─ Mehrere Viewports → --responsive → *-{width}w.webp + <picture>
├─ Link-Preview / Social → --og → og-*.webp + meta tags
├─ Icon feste Größe → --resize N --format webp → public/icons/
└─ Frisch von GenerateImage?
    └─ Zuerst ablegen (PNG ok) → dann einer der Schritte oben
```

---

## Defaults (sinnvoll für HorosCloud)

| Parameter | Default | Anmerkung |
|-----------|---------|-----------|
| Format | WebP | Beste Größe/Qualität für Web |
| Quality | 85 | Gut für UI-Illustrationen |
| max-dimension (`--optimize`) | 1920 | Größer wird skaliert |
| responsive widths | 640, 960, 1280, 1920 | Tailwind-Breakpoints-nah |
| OG | 1200×630 | Standard OG/Twitter large |

---

## Checkliste vor „fertig“

- [ ] **Asset-Ordner** mit `full.{ext}` + `regions/` vorhanden
- [ ] **Manifest** + **regions.json** aktualisiert und mit `Read` verifiziert
- [ ] CLI mit Exit-Code `0` gelaufen
- [ ] Ausgabedatei(en) existieren unter `public/images/` oder `public/icons/`
- [ ] TSX/HTML referenziert **.webp** (nicht vergessenes PNG)
- [ ] `alt` / `aria-hidden` gesetzt (**@horos-image-embed**)
- [ ] `width`/`height` gesetzt wo Layout-Shift vermieden werden soll
- [ ] OG: absolute Produktions-URL in Meta-Tags
- [ ] Bei Hero above-the-fold: `loading="lazy"` **weglassen** oder `fetchPriority="high"`

---

## Agent-Zuordnung

| Agent | Schritt |
|-------|---------|
| **Zuko** | Nach GenerateImage: CLI ausführen, Pfade melden |
| **horos-ui** | `<picture>`, lazy, OG-Meta in Komponenten/Layout |

**Pipeline:**

```text
@horos-image-embed → @cursor-image-generation → GenerateImage → {slug}/full
  → @horos-image-manifest + @horos-image-regions (Pflicht)
  → @horos-web-image-optimize (full + regions) → Manifest/regions aktualisieren → horos-ui
```

---

## Manifest + Regions (Pflicht)

Nach **jeder** CLI-Optimierung (`--optimize`, `--responsive`, `--og`, `--resize`):

1. Asset-Ordner `{parent}/{slug}/` — `full.{ext}` + `regions/*` optimieren
2. Manifest `{slug}.manifest.md` aktualisieren (Format, srcset, Regions-Tabelle)
3. `regions.json` — `exportPath`-Einträge auf `.webp` anpassen
4. `Read` auf full + mindestens eine Region — **kein Erfolg ohne aktuelles Manifest + regions/**

Details: `@horos-image-manifest`, `@horos-image-regions` · Rule: `@image-asset`

---

## Referenzen

- CLI: `.cursor/skills/image-utils/scripts/optimize_web_image.py`
- ImageUtils: `.cursor/skills/image-utils/references/code-examples/image_utils.py`
- Embed-Workflow: `.cursor/skills/horos-image-embed/SKILL.md`
- Manifest: `.cursor/skills/horos-image-manifest/SKILL.md`
- Regions: `.cursor/skills/horos-image-regions/SKILL.md`
- Design: `.cursor/skills/horos-design-system/SKILL.md`
- Firma/Produkt: `.cursor/rules/horoscode.mdc`
