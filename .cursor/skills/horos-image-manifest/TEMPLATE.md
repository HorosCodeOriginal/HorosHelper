# Bild-Manifest — {{DISPLAY_NAME}}

> **HorosCode** · Asset-Manifest für 1:1-Einbindung in **HorosCloud**-Anwendungen.
> Template: `.cursor/skills/horos-image-manifest/TEMPLATE.md` · Skill: `@horos-image-manifest`

---

## Identität

| Feld | Wert |
|------|------|
| **Dateiname (canonical)** | `full.{{EXT}}` |
| **Asset-Ordner** | `{{ASSET_FOLDER_PATH}}` |
| **Asset-Slug** | `{{ASSET_SLUG}}` |
| **Absoluter Pfad (full)** | `{{ABSOLUTE_PATH}}` |
| **Workspace-relativer Pfad** | `{{WORKSPACE_RELATIVE_PATH}}` |
| **Erstellungsdatum (UTC)** | `{{CREATED_AT_ISO}}` |
| **Letzte Änderung (UTC)** | `{{UPDATED_AT_ISO}}` |
| **Skill / Pipeline** | `{{SOURCE_SKILL}}` (z. B. `@cursor-image-generation`, `@horos-web-image-optimize`, `@bria-ai`) |
| **Agent** | `{{AGENT_NAME}}` (z. B. Zuko, horos-ui) |
| **Version / Iteration** | `{{VERSION}}` (z. B. `v1`, `v2-delta-bg`, `iteration-3`) |
| **Manifest-Schema** | `horos-image-manifest/2.0` |
| **Canonical-Basename** | `{{CANONICAL_BASENAME}}` (= Asset-Slug, ohne `-640w`, `-og`) |
| **Regions-JSON** | `regions/regions.json` |

---

## Quelle

### Original-Prompt (vollständig)

```text
{{FULL_PROMPT}}
```

### Negative Prompts

```text
{{NEGATIVE_PROMPTS_OR_NONE}}
```

### Delta-Prompts (Iterationen)

| Iteration | Änderung |
|-----------|----------|
| v1 | `{{DELTA_V1_OR_INITIAL}}` |
| v2 | `{{DELTA_V2_OR_DASH}}` |

### Model / Tool

| Feld | Wert |
|------|------|
| **Generator** | `{{TOOL_NAME}}` (GenerateImage / OpenAI gpt-image-2 / Bria FIBO / RMBG-2.0 / Pillow CLI) |
| **Modell-ID** | `{{MODEL_ID}}` (z. B. Google Nano Banana Pro, gpt-image-2, RMBG-2.0) |
| **API / Laufzeit** | `{{RUNTIME}}` (Cursor eingebaut / OPENAI_API_KEY / BRIA_API_KEY) |
| **Qualitätsstufe** | `{{QUALITY}}` (low / medium / high / auto / WebP q85) |
| **Aspect Ratio (Request)** | `{{REQUESTED_ASPECT_RATIO}}` (1:1 / 16:9 / 9:16 / custom) |

### Referenzbilder (`reference_image_paths`)

| # | Pfad | Rolle |
|---|------|-------|
| 1 | `{{REF_PATH_1_OR_NONE}}` | `{{REF_ROLE_1}}` (Stil / Komposition / Produkt) |
| 2 | `{{REF_PATH_2_OR_NONE}}` | `{{REF_ROLE_2}}` |

### Mockup / Figma-Bezug

| Feld | Wert |
|------|------|
| **Mockup-Quelle** | `{{MOCKUP_SOURCE_OR_NONE}}` |
| **Figma-Frame** | `{{FIGMA_FRAME_OR_NONE}}` |
| **Ziel-Komponente** | `{{TARGET_COMPONENT_PATH_OR_NONE}}` |

---

## Dimensionen

| Feld | Wert |
|------|------|
| **Breite × Höhe** | `{{WIDTH}}` × `{{HEIGHT}}` px |
| **Aspect Ratio (berechnet)** | `{{ASPECT_RATIO}}` (z. B. `1.778` = 16:9) |
| **DPI / Density** | `{{DPI_OR_SCREEN_72}}` (Web: 72 dpi; Print: angeben) |
| **Dateigröße** | `{{FILE_SIZE_KB}}` KB (`{{FILE_SIZE_BYTES}}` Bytes) |
| **Pixel-Dichte** | `{{PIXELS_TOTAL}}` px gesamt |
| **Orientierung** | `{{ORIENTATION}}` (square / landscape / portrait) |

---

## Farben

### Dominante Hex-Werte (#RRGGBB)

Extrahiert via Pillow-Quantize (siehe `@horos-image-manifest` → Metadaten-Workflow):

| Rang | Hex | RGB | Anteil (ca.) | Verwendung im Bild |
|------|-----|-----|--------------|-------------------|
| 1 | `#{{DOMINANT_1}}` | `{{RGB_1}}` | `{{PCT_1}}%` | `{{USAGE_1}}` (z. B. Hintergrund Slate) |
| 2 | `#{{DOMINANT_2}}` | `{{RGB_2}}` | `{{PCT_2}}%` | `{{USAGE_2}}` |
| 3 | `#{{DOMINANT_3}}` | `{{RGB_3}}` | `{{PCT_3}}%` | `{{USAGE_3}}` |
| 4 | `#{{DOMINANT_4}}` | `{{RGB_4}}` | `{{PCT_4}}%` | `{{USAGE_4}}` |
| 5 | `#{{DOMINANT_5}}` | `{{RGB_5}}` | `{{PCT_5}}%` | `{{USAGE_5}}` |

### Palette-Liste (kompakt)

```text
#{{DOMINANT_1}}, #{{DOMINANT_2}}, #{{DOMINANT_3}}, #{{DOMINANT_4}}, #{{DOMINANT_5}}
```

### Transparenz

| Feld | Wert |
|------|------|
| **Alpha-Kanal** | `{{HAS_ALPHA}}` (ja / nein) |
| **PIL-Modus** | `{{PIL_MODE}}` (RGBA / RGB / P / L) |
| **Transparente Pixel** | `{{TRANSPARENT_PCT}}%` (0% wenn opaque) |
| **Freistell-Methode** | `{{CUTOUT_METHOD}}` (keine / Bria RMBG-2.0 / Prompt-Slate-Workaround) |

---

## Format

| Feld | Wert |
|------|------|
| **Dateiformat** | `{{FORMAT}}` (PNG / WebP / JPEG) |
| **MIME-Type** | `image/{{MIME}}` |
| **Optimierungsstufe** | `{{OPTIMIZATION}}` (z. B. WebP quality 85, PNG unkomprimiert) |
| **Farbraum** | `{{COLORSPACE}}` (sRGB) |
| **Animation** | `{{ANIMATED}}` (nein) |

### srcset-Varianten (falls vorhanden)

| Datei | Breite | Format | Größe (KB) | URL-Pfad |
|-------|--------|--------|------------|----------|
| `{{BASENAME}}-640w.webp` | 640 | WebP | `{{SIZE_640}}` | `/images/{{BASENAME}}-640w.webp` |
| `{{BASENAME}}-960w.webp` | 960 | WebP | `{{SIZE_960}}` | `/images/{{BASENAME}}-960w.webp` |
| `{{BASENAME}}-1280w.webp` | 1280 | WebP | `{{SIZE_1280}}` | `/images/{{BASENAME}}-1280w.webp` |
| `{{BASENAME}}-1920w.webp` | 1920 | WebP | `{{SIZE_1920}}` | `/images/{{BASENAME}}-1920w.webp` |
| `og-{{BASENAME}}.webp` | 1200×630 | WebP | `{{SIZE_OG}}` | `/images/og-{{BASENAME}}.webp` |

---

## Design-System-Bezug (HorosCloud)

| Token | Hex / Klasse | Im Bild verwendet? |
|-------|--------------|-------------------|
| App background | `#0f172a` / `bg-slate-900` | `{{USES_SLATE_900}}` |
| Surface | `#1e293b` / `bg-slate-800` | `{{USES_SLATE_800}}` |
| Surface raised | `#334155` / `bg-slate-700` | `{{USES_SLATE_700}}` |
| Border | `border-slate-700` | `{{USES_BORDER}}` |
| Text primary | `text-slate-100` | `{{USES_TEXT_PRIMARY}}` |
| Text muted | `text-slate-400` | `{{USES_TEXT_MUTED}}` |
| Accent | `amber-400` / `#fbbf24` | `{{USES_AMBER}}` |
| Accent strong | `amber-500` / `#f59e0b` | `{{USES_AMBER_500}}` |

| Feld | Wert |
|------|------|
| **Dark-Mode-Kontext** | `{{DARK_MODE_CONTEXT}}` (dark-first / light-card / neutral) |
| **Intended Background** | `{{INTENDED_BG}}` (z. B. `bg-slate-800` Card, `bg-slate-900` Page) |
| **Kontrast zum Hintergrund** | `{{CONTRAST_NOTES}}` (z. B. Icon auf slate-800: ausreichend) |

---

## Regions / Bereiche

Maschinenlesbar: [`regions/regions.json`](./regions/regions.json) · Human-readable: [`regions/README.md`](./regions/README.md)

| # | Slug | Label (DE) | usage | Bounds (x,y,w×h) | Export | zIndex |
|---|------|------------|-------|------------------|--------|--------|
| 1 | `{{REGION_SLUG_1}}` | `{{REGION_LABEL_1}}` | `{{REGION_USAGE_1}}` | `{{REGION_BOUNDS_1}}` | [`regions/{{REGION_SLUG_1}}.{{EXT}}`](./regions/{{REGION_SLUG_1}}.{{EXT}}) | `{{REGION_Z_1}}` |
| 2 | `{{REGION_SLUG_2}}` | `{{REGION_LABEL_2}}` | `{{REGION_USAGE_2}}` | `{{REGION_BOUNDS_2}}` | [`regions/{{REGION_SLUG_2}}.{{EXT}}`](./regions/{{REGION_SLUG_2}}.{{EXT}}) | `{{REGION_Z_2}}` |
| 3 | `{{REGION_SLUG_3}}` | `{{REGION_LABEL_3}}` | `{{REGION_USAGE_3}}` | `{{REGION_BOUNDS_3}}` | `{{REGION_EXPORT_3}}` | `{{REGION_Z_3}}` |

| Feld | Wert |
|------|------|
| **Anzahl Regionen** | `{{REGION_COUNT}}` |
| **single-layer** | `{{SINGLE_LAYER}}` (ja/nein — s. regions/README wenn ja) |
| **nineSlice** | `{{NINE_SLICE}}` (ja/nein) |

---

## Einbindung 1:1

### Verwendungszweck

`{{USAGE_PURPOSE}}` — Widget-Icon / Hero / OG / Doc-Illustration / Avatar / Marketing-Still / Pattern / Dekorativ

### Layered Regions (optional)

```tsx
<div className="relative {{CONTAINER_CLASSES}}">
  <img src="/{{ASSET_URL_PREFIX}}/regions/background.{{EXT}}" alt="" aria-hidden className="absolute inset-0 h-full w-full object-cover" />
  <img src="/{{ASSET_URL_PREFIX}}/regions/{{REGION_SLUG_2}}.{{EXT}}" alt="" aria-hidden className="{{REGION_2_CLASSES}}" />
</div>
```

### `<img>` (Vite `public/` — Gesamtbild)

```html
<img
  src="{{PUBLIC_URL}}"
  alt="{{ALT_TEXT_DE}}"
  width="{{WIDTH}}"
  height="{{HEIGHT}}"
  class="{{TAILWIND_CLASSES}}"
  loading="{{LOADING}}"
  decoding="async"
/>
```

### Next.js `Image` (falls Next-App)

```tsx
import Image from "next/image";

<Image
  src="{{PUBLIC_URL}}"
  alt="{{ALT_TEXT_DE}}"
  width={{{WIDTH}}}
  height={{{HEIGHT}}}
  className="{{TAILWIND_CLASSES}}"
  priority={{{PRIORITY_BOOL}}}
/>
```

### Responsive `<picture>` (mit srcset)

```tsx
<picture>
  <source
    type="image/webp"
    srcSet="{{SRCSET_STRING}}"
    sizes="{{SIZES_STRING}}"
  />
  <img
    src="{{FALLBACK_URL}}"
    alt="{{ALT_TEXT_DE}}"
    className="{{TAILWIND_CLASSES}}"
    width={{{WIDTH}}}
    height={{{HEIGHT}}}
    loading="{{LOADING}}"
    decoding="async"
  />
</picture>
```

### CSS `background-image` (Hero / Dekorativ)

```css
.hero-{{SLUG}} {
  background-image: url("{{PUBLIC_URL}}");
  background-size: {{BG_SIZE}};
  background-position: {{BG_POSITION}};
  background-repeat: no-repeat;
}
```

```tsx
<div
  className="relative {{CONTAINER_CLASSES}}"
  style={{ backgroundImage: "url('{{PUBLIC_URL}}')" }}
  role="{{ROLE_OR_NONE}}"
  aria-label="{{ARIA_LABEL_OR_NONE}}"
/>
```

### A11y

| Feld | Wert |
|------|------|
| **alt (Deutsch)** | `{{ALT_TEXT_DE}}` |
| **aria-label** | `{{ARIA_LABEL_OR_EMPTY}}` |
| **Dekorativ** | `{{IS_DECORATIVE}}` (ja → `alt=""` + `aria-hidden`) |
| **Figcaption** | `{{FIGCAPTION_OR_NONE}}` |

---

## Layout

| Feld | Wert |
|------|------|
| **Empfohlene Container-Größe** | `{{CONTAINER_SIZE}}` (z. B. `w-full max-w-4xl`, `h-10 w-10`) |
| **object-fit** | `{{OBJECT_FIT}}` (`contain` / `cover` / `fill`) |
| **object-position** | `{{OBJECT_POSITION}}` (`center` / `top` / `50% 30%`) |
| **Aspect Ratio (CSS)** | `{{CSS_ASPECT}}` (`aspect-video` / `aspect-square` / `auto`) |
| **Padding / Margin** | `{{SPACING}}` (aus Mockup, z. B. `p-4`, `safe-margin 10%`) |
| **Border / Radius** | `{{BORDER_RADIUS}}` (z. B. `rounded-lg border border-slate-700`) |
| **Z-Index / Layer** | `{{Z_INDEX}}` (z. B. dekorativ `opacity-10` hinter Content) |
| **Above-the-fold** | `{{ABOVE_FOLD}}` (ja → kein `loading="lazy"`, ggf. `fetchPriority="high"`) |

---

## Parity-Checkliste (Mockup ↔ Implementierung)

Vor „fertig“ jede Zeile prüfen — Abweichungen in **Notizen** dokumentieren:

- [ ] **Maße:** Breite×Höhe im UI = Manifest (`{{WIDTH}}×{{HEIGHT}}` oder skaliert mit gleichem AR)
- [ ] **Farben:** Dominante Hex-Werte sichtbar korrekt (Toleranz ±2 RGB bei Kompression)
- [ ] **Hintergrund:** Intended Background (`{{INTENDED_BG}}`) — kein heller Rand / kein Checkerboard
- [ ] **Beschnitt:** `object-fit` / `object-position` — kein ungewolltes Cropping
- [ ] **Transparenz:** Alpha wie spezifiziert (`{{HAS_ALPHA}}`)
- [ ] **Schärfe:** Kein sichtbares Upscaling-Artefakt bei Zielgröße
- [ ] **Text im Bild:** Lesbar, exakte Wortwahl wie Prompt (falls Text-Asset)
- [ ] **Abstand:** Safe-Margin / Padding wie Mockup
- [ ] **Dark-Mode:** Auf `bg-slate-900` und `bg-slate-800` geprüft
- [ ] **Responsive:** srcset-Stufen laden korrekte Variante
- [ ] **OG/Social:** 1200×630 Crop zentriert, kein wichtiger Inhalt abgeschnitten
- [ ] **A11y:** `alt` / `aria-hidden` korrekt gesetzt
- [ ] **Performance:** WebP ausgeliefert, `width`/`height` gesetzt (CLS)

### Notizen zu Abweichungen

```text
{{PARITY_NOTES_OR_NONE}}
```

---

## Verwandte Dateien

| Datei | Pfad | Rolle |
|-------|------|-------|
| Canonical full | `{{WORKSPACE_RELATIVE_PATH}}` | Master `full.{{EXT}}` |
| Regions-JSON | `regions/regions.json` | Maschinenlesbare Bereiche |
| Regions-README | `regions/README.md` | Human-readable Übersicht |
| Region-Slices | `regions/*.{{EXT}}` | Exportierte Bereiche |
| Region-Manifeste | `regions/*.manifest.md` | Mini-Manifest pro Region |
| Original (PNG) | `{{ORIGINAL_PATH_OR_NONE}}` | Master vor Optimierung |
| WebP optimiert | `{{WEBP_PATH_OR_NONE}}` | Produktions-Asset |
| srcset-Varianten | `{{SRCSET_DIR_OR_NONE}}` | Responsive Auslieferung |
| OG-Variante | `{{OG_PATH_OR_NONE}}` | Link-Preview |
| Freistell-Output | `{{CUTOUT_PATH_OR_NONE}}` | Transparentes PNG |
| Eingebettete TSX | `{{TSX_PATH_OR_NONE}}` | Komponente mit Referenz |
| Doc-Epic | `{{DOC_PATH_OR_NONE}}` | Feature-Doku |
| Vorherige Iteration | `{{PREV_ITERATION_PATH_OR_NONE}}` | Vergleich v(n-1) |

---

## Pipeline-Protokoll

| Schritt | Zeitstempel | Status |
|---------|-------------|--------|
| Generierung | `{{TS_GENERATE}}` | `{{STATUS_GENERATE}}` |
| Optimierung | `{{TS_OPTIMIZE}}` | `{{STATUS_OPTIMIZE}}` |
| Manifest geschrieben | `{{TS_MANIFEST}}` | `{{STATUS_MANIFEST}}` |
| Regions erstellt | `{{TS_REGIONS}}` | `{{STATUS_REGIONS}}` |
| Manifest + Regions verifiziert (Read) | `{{TS_VERIFY}}` | `{{STATUS_VERIFY}}` |
| In App eingebunden | `{{TS_EMBED}}` | `{{STATUS_EMBED}}` |

---

*© HorosCode · Manifest generiert für HorosCloud 1:1-Asset-Parity.*
