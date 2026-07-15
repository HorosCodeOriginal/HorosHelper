---
name: horos-image-embed
description: >-
  HorosCode/HorosCloud End-to-End-Bild-Workflow — generieren, bearbeiten und in
  HorosCloudV5/apps/web einbetten. Standard: Cursor GenerateImage + @cursor-image-generation
  (kein API-Key). Optional nur bei explizitem User-Wunsch: @generating-images (OpenAI),
  @bria-ai (Freistellen). Nutzen wenn der User Bilder erstellen, an HorosCloud-Stil anpassen
  oder in React/Tailwind einbinden will.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "3.0.0"
---

# Horos Image Embed — Generieren, Bearbeiten, Einbetten

**HorosCode**-Skill für **HorosCloud**-Web-UI. Orchestriert Bild-Erstellung, Nachbearbeitung und Einbindung in `HorosCloudV5/apps/web` — mit konsistentem Dark-SaaS-Stil (Slate + Amber).

**Standard ohne externe API-Keys:** Cursor **GenerateImage** (eingebaut, in Cursor inkludiert) + **@cursor-image-generation** (Prompting, Iteration, Referenzbilder).

---

## Preflight (Pflicht vor jedem Bild-Workflow)

**Zuerst** fehlende Zielordner anlegen — **kein Fehler** `public folder missing`, **kein** `OPENAI_API_KEY`-Check im Default-Pfad:

```bash
node scripts/ensure-image-paths.mjs
```

| Modus | Befehl | OPENAI_API_KEY |
|-------|--------|----------------|
| **Default** (GenerateImage) | `node scripts/ensure-image-paths.mjs` | **Nicht** erforderlich — Script meldet Erfolg ohne Key |
| Nur `@generating-images` | `node scripts/ensure-image-paths.mjs --require-openai` | **Erforderlich** — sonst Exit 2 |

**VERBOTEN im Default-Pfad:** `generate_image.py` aufrufen oder `OPENAI_API_KEY: not set` als Blocker melden, wenn der User Cursor-Modelle nutzt.

**Zielpfade** (werden vom Script angelegt falls fehlend):

- `HorosCloudV5/apps/web/public/images/`
- `HorosCloudV5/apps/web/public/icons/`
- `assets/images/` (Drafts)

---

## Warum Cursor-native zuerst?

| Weg | API-Key nötig? | Warum |
|-----|----------------|-------|
| **GenerateImage** (Cursor eingebaut) | **Nein** | Läuft über Cursors integriertes Bildmodell (Google Nano Banana Pro). Kein `OPENAI_API_KEY`, kein `BRIA_API_KEY` — Nutzung ist Teil der Cursor-Umgebung. |
| **@generating-images** (OpenAI gpt-image-2) | **Ja** — `OPENAI_API_KEY` | Ruft die **externe** OpenAI-API per Python-Script auf. Kosten und Key-Verwaltung liegen beim User. |
| **@bria-ai** / **@remove-background** | **Ja** — Bria-Auth (`BRIA_API_KEY` / Device-Flow) | Ruft die **externe** Bria.ai-API auf — professionelles Freistellen, Inpaint, Hintergrundersetzung. |

**Regel für Agenten:** Immer mit **GenerateImage + @cursor-image-generation** starten. **@generating-images** und **@bria-ai** nur vorschlagen, wenn der User **explizit** externe APIs, Batch-Scripts, Mask-Inpaint oder professionelles Freistellen will — **nicht** als Default empfehlen.

**Niemals** Keys in Repo, committed `.env` oder SKILL.md hardcoden.

---

## Wann diesen Skill nutzen

- User will **Bilder in HorosCloud einbetten** (Hero, Widget-Icon, Dashboard-Illustration, Doc-Illustration).
- User will **Stil übernehmen** — Dark Theme, Amber-Akzent, Slate-Flächen (`@horos-design-system`).
- User liefert **Referenzbild** und will Variation, Restyle oder Einbindung.
- Aufgabe umfasst **mehr als nur Generieren**: Asset ablegen **und** in TSX/MDX/Docs verdrahten.
- User will **keinen API-Key** — dann ausschließlich Cursor-Workflow (dieser Skill, Abschnitt „Ohne API-Key“).

**Nicht** für: reine Daten-Charts (Code), App-Screenshots (`@screenshot-capture`), Vektor-Icons aus Code (SVG-Komponente).

---

## Verknüpfte Skills (Priorität)

| Priorität | Skill | Rolle |
|-----------|-------|--------|
| **1 — Standard** | **@cursor-image-generation** | Prompt-Rewrite, Iteration, **GenerateImage**-Aufruf, `reference_image_paths` |
| **1 — Standard** | **@horos-design-system** | Brand: `#0f172a`, Slate-Surfaces, `amber-400`/`amber-500`, 8px-Grid |
| **1 — Standard** | **@horos-web-image-optimize** | Nach **GenerateImage**: WebP, Resize, responsive srcset, OG 1200×630 — **kein API-Key** |
| **1 — Pflicht** | **@horos-image-manifest** | Nach **jedem** Bild-Output: `{slug}/{slug}.manifest.md` im Asset-Ordner |
| **1 — Pflicht** | **@horos-image-regions** | Nach Manifest: `regions/` mit json, README, Slices für 1:1-Layer-Übernahme |
| **2 — Optional** | **@image-utils** | Low-level Pillow-Referenz (gleiche Engine wie CLI) |
| **2 — Optional** | **@generating-images** | Nur wenn User OpenAI-Key hat und Batch/Mask/gpt-image-2 will |
| **2 — Optional** | **@bria-ai** / **@remove-background** | Nur wenn User Bria-Key hat und echtes Freistellen/Inpaint braucht |

---

## Entscheidungsbaum

```
User braucht Bild?
│
├─ DEFAULT (kein API-Key / User will Cursor-Modelle)
│   └─ @cursor-image-generation → GenerateImage
│       ├─ reference_image_paths wenn User-Referenz im Chat oder auf Disk
│       ├─ Stil: @horos-design-system in Prompt
│       ├─ Iteration per Delta-Prompt (Hintergrund, Größe, Stil)
│       ├─ Nachbearbeitung → @horos-web-image-optimize (CLI: WebP, srcset, OG)
│       ├─ @horos-image-manifest + @horos-image-regions (Ordner + Slices)
│       └─ Transparentes Icon nötig?
│           ├─ Workaround ohne Key: Prompt „centered subject, solid flat #0f172a background“
│           │   → passt zu dark UI; kein echtes Alpha-PNG
│           └─ Echtes Freistellen? → User fragen ob Bria-Key gewünscht (optional)
│
├─ User EXPLIZIT OpenAI / Batch / Mask-Inpaint?
│   └─ @generating-images → scripts/generate_image.py
│       └─ OPENAI_API_KEY erforderlich — vorher User informieren
│
├─ User EXPLIZIT professionelles Freistellen / Bria-Edit?
│   └─ @remove-background oder @bria-ai
│       └─ BRIA-Auth erforderlich — vorher User informieren
│
└─ Asset fertig → Web-ready machen?
    └─ @horos-web-image-optimize → CLI (WebP, responsive, OG) → dann Einbetten
        └─ Dieser Skill → Abschnitt „Einbettung“ + „Nachbearbeitung für Web“
```

**GenerateImage vs generating-images:**

| Kriterium | GenerateImage (Standard) | generating-images (optional) |
|-----------|--------------------------|------------------------------|
| Ort | Nur Cursor-Agent | Shell / Python überall |
| Key | **Keiner** | `OPENAI_API_KEY` |
| Empfehlung | **Immer zuerst** | Nur auf User-Wunsch |
| Transparenz | Nein (opaque PNG) | Nein (gpt-image-2) |
| Referenz | `reference_image_paths: [absPath]` | `--image ./ref.png` (mehrfach) |
| Freistellen | Begrenzt (Prompt + Iteration) | → danach optional **@remove-background** |

---

## Grenzen ohne externe API (ehrlich)

| Funktion | Ohne Key (GenerateImage) | Mit Bria-Key |
|----------|--------------------------|--------------|
| Neues Bild / Icon / Hero | ✅ Gut | ✅ |
| Referenz → Variation | ✅ `reference_image_paths` | ✅ |
| Stil-Anpassung per Prompt | ✅ Iteration | ✅ NL-Edit |
| **Echtes transparentes PNG** | ⚠️ Schwach — Modell liefert opaque PNG | ✅ `remove_background` |
| Präzises Inpaint / Mask | ⚠️ Nur per Prompt-Iteration | ✅ Bria / OpenAI Mask |
| Batch / CI-Script | ❌ | ✅ generating-images |

**Workaround für Icons ohne Freistell-API:** Dunklen Slate-Hintergrund (`#0f172a`) im Prompt erzwingen — Icon wirkt auf HorosCloud-Cards wie „freigestellt“, technisch aber kein Alpha-Kanal. Für echte Transparenz: User aktiv nach Bria-Key fragen.

---

## HorosCloud-Stil in Prompts

Vor Generierung **@horos-design-system** lesen. In jeden Bild-Prompt einbauen:

- **Hintergrund:** dunkles Slate (`#0f172a` / `#1e293b`), nicht reinweiß — passt zu `bg-slate-900`
- **Akzent:** Amber/Gold (`#f59e0b`, `amber-400`) sparsam — CTAs, Highlights, Glow
- **Stil:** „dark SaaS dashboard“, „minimal flat illustration“ oder „soft 3D icon on dark surface“
- **Kein** generisches Neon-Cyberpunk-Chaos unless User es verlangt

**Beispiel-Prompt (GenerateImage):**

```text
Square widget icon for a cloud backup dashboard, dark slate background #0f172a,
subtle amber #f59e0b accent on the symbol, flat vector with soft depth,
centered, 10% safe margin, no tiny text, HorosCloud dark SaaS aesthetic.
```

---

## Workflows ohne API-Key (Standard)

### A — Referenzbild im Chat → Asset → React

1. **Kontext** — Ziel-Komponente lesen (`HorosCloudV5/apps/web/src/...`), `@horos-design-system` Tokens.
2. **Referenz** — User-Upload im Chat oder bestehendes Asset unter `public/`.
3. **@cursor-image-generation** — Brief schärfen, vollständigen Prompt schreiben.
4. **GenerateImage** mit `reference_image_paths` — **filename** = `{asset-slug}/full.png` im Ziel-Parent:

```json
{
  "description": "... vollständiger Prompt mit Horos-Tokens; was behalten vs. ändern ...",
  "filename": "hero-backup/full.png",
  "reference_image_paths": [
    "C:/Users/.../HorosCloudV5/apps/web/public/images/existing-ref/full.png"
  ],
  "aspect_ratio": "1:1"
}
```

5. **Ablage** — Asset-Ordner unter `HorosCloudV5/apps/web/public/images/{slug}/` oder `public/icons/{slug}/` mit `full.{ext}`, Manifest, `regions/`.
6. **Einbinden** — `<img src="/icons/...">` in TSX; Tailwind für Größe/Rundung.
7. **Iteration** — Delta-Prompt statt Neustart (`@cursor-image-generation` → „nur Hintergrund ändern …“).
8. **Melden** — Dateipfad(e), geänderte Komponenten, finaler Prompt.

### B — Neues Hero ohne Referenz

1. Prompt mit Horos-Palette (s. oben).
2. `GenerateImage` → `aspect_ratio: "16:9"` für Hero; `"1:1"` für Icon.
3. Speichern unter `HorosCloudV5/apps/web/public/images/hero-<slug>/full.png` (+ Manifest + regions/).
4. In Page/Widget: `src="/images/hero-<slug>/full.webp"` oder Layered Regions aus `regions/`.

### C — Widget-Icon ohne Freistell-API

1. GenerateImage mit festem Slate-Hintergrund im Prompt (s. Grenzen-Tabelle).
2. `public/icons/<widget-slug>/full.png` + `regions/` (foreground, glow, …)
3. Einbinden mit `className="h-8 w-8 object-contain"` — Pfad `regions/foreground.webp` oder `full.webp`.

### D — Doc-Epic / Feature-Doku

1. UI-Screenshot → `@screenshot-capture` + `@screenshot-verify` (nicht dieser Skill).
2. Illustration / Platzhalter-Grafik → GenerateImage → `HorosCloudV5/docs/screenshots/<feature-slug>/...`
3. In Epic-HTML/MD: relativer Pfad `docs/screenshots/...`

---

## Nachbearbeitung für Web

**Nach jedem GenerateImage** (oder wenn User ein bestehendes PNG/JPG liefert): **@horos-web-image-optimize** ausführen — **kein API-Key**, nur Pillow.

### Wann CLI laufen lassen

| Situation | Aktion |
|-----------|--------|
| Hero / großes Raster | `--optimize` oder `--responsive` → WebP unter `public/images/` |
| Social / Link-Preview | `--og` → 1200×630 WebP |
| Widget-Icon | `--resize 512 --format webp` → `public/icons/` |
| Nur Metadaten prüfen | `--info` |

**CLI-Pfad (Workspace-relativ):**

```text
.cursor/skills/image-utils/scripts/optimize_web_image.py
```

### Shell-Beispiele (Agenten)

```bash
# Einzelnes Hero: optimieren → WebP (im Asset-Ordner)
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/full.png \
  --optimize \
  --out HorosCloudV5/apps/web/public/images/hero-backup/full.webp

# Alle Region-Slices im selben Ordner
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/regions/icon-shield.png \
  --format webp \
  --out HorosCloudV5/apps/web/public/images/hero-backup/regions/icon-shield.webp

# Responsive srcset-Varianten (640, 960, 1280, 1920 px)
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/full.png \
  --responsive \
  --out HorosCloudV5/apps/web/public/images/hero-backup/full.webp

# Open Graph (1200×630)
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/images/hero-backup/full.png \
  --og \
  --out HorosCloudV5/apps/web/public/images/hero-backup/og-full.webp

# Icon auf feste Größe
python .cursor/skills/image-utils/scripts/optimize_web_image.py \
  --input HorosCloudV5/apps/web/public/icons/widget-notizen.png \
  --resize 512 \
  --format webp \
  --out HorosCloudV5/apps/web/public/icons/widget-notizen.webp
```

**Voraussetzung:** `pip install Pillow requests` (einmalig). Exit-Code `0` = Erfolg; bei `3` Dependencies installieren.

### Responsive Einbindung (React + Tailwind)

Nach `--responsive` liegen Dateien wie `hero-backup/full-640w.webp` im Asset-Ordner. In TSX:

```tsx
<picture>
  <source
    type="image/webp"
    srcSet="/images/hero-backup/full-640w.webp 640w, /images/hero-backup/full-960w.webp 960w, /images/hero-backup/full-1280w.webp 1280w, /images/hero-backup/full-1920w.webp 1920w"
    sizes="(max-width: 768px) 100vw, (max-width: 1280px) 80vw, 1200px"
  />
  <img
    src="/images/hero-backup/full-1280w.webp"
    alt="HorosCloud Backup Dashboard — Illustration"
    className="aspect-video w-full rounded-lg border border-slate-700 object-cover"
    loading="lazy"
    decoding="async"
    width={1280}
    height={720}
  />
</picture>
```

**Lazy Loading:** `loading="lazy"` für below-the-fold; Hero above-the-fold ohne `lazy` oder mit `fetchPriority="high"`.

### Open Graph Meta (1200×630)

Nach `--og` in Page-Head oder Layout (z. B. React Helmet / `index.html` für statische Routes):

```html
<meta property="og:image" content="https://app.horoscloud.example/images/hero-backup/og-full.webp" />
<meta property="og:image:width" content="1200" />
<meta property="og:image:height" content="630" />
<meta name="twitter:card" content="summary_large_image" />
<meta name="twitter:image" content="https://app.horoscloud.example/images/hero-backup/og-full.webp" />
```

Absolute URL zur Produktions-Domain verwenden — relative Pfade funktionieren in OG-Tags nicht zuverlässig.

### A11y nach Optimierung

- **Informative Bilder:** beschreibendes `alt` auf Deutsch (Inhalt, nicht Dateiname).
- **Dekorativ:** `alt=""` + `aria-hidden` — auch nach WebP-Umstellung beibehalten.
- **`width` / `height`:** aus `--info` oder bekannter Zielgröße setzen — verhindert Layout-Shift (CLS).

### Pipeline (Ziel)

```text
User → Zuko → @horos-image-embed → @cursor-image-generation → GenerateImage → {slug}/full
     → @horos-image-manifest + @horos-image-regions (Pflicht)
     → @horos-web-image-optimize (full + regions)
     → @horos-image-manifest + @horos-image-regions (aktualisieren) → horos-ui
```

---

## Manifest + Regions (Pflicht)

**Jedes** generierte, optimierte oder freigestellte Raster-Asset **MUSS** einen **Asset-Ordner** mit Manifest und **`regions/`** erhalten — **kein Erfolg ohne Ordner-Struktur**.

| Schritt | Aktion |
|---------|--------|
| Nach GenerateImage | `{parent}/{slug}/full.{ext}` + `@horos-image-manifest` + `@horos-image-regions` |
| Nach `@horos-web-image-optimize` | `full` + Region-Slices → WebP; Manifest + regions.json aktualisieren |
| Nach TSX-Einbindung | Manifest: Einbindung 1:1, Regions-Tabelle, Parity |
| Vor „fertig“ | `Read` auf full + mindestens 1 Region (oder single-layer README) |

**Schema:** `{parent}/{asset-slug}/{asset-slug}.manifest.md` + `regions/` (s. `@horos-image-manifest`, `@horos-image-regions`)

**Rule:** `@image-asset` (Alias `@image-manifest`)

Details und Edge Cases: **@horos-web-image-optimize**.

---

## Workflows mit externen APIs (nur auf User-Wunsch)

### OpenAI — @generating-images

Nur wenn User **explizit** OpenAI nutzen will und `OPENAI_API_KEY` bereitstellt:

```bash
python .cursor/skills/generating-images/scripts/generate_image.py \
  --prompt "Dark SaaS illustration, slate #1e293b, amber accent, ..." \
  --size 1536x1024 \
  --quality medium \
  --out HorosCloudV5/apps/web/public/images/hero-backup.png
```

### Bria — Freistellen nach Generierung

Nur wenn User **explizit** transparentes PNG braucht und Bria-Auth hat:

1. GenerateImage oder OpenAI → PNG mit Hintergrund
2. `@remove-background` oder `@bria-ai` → `remove_background`
3. Ergebnis nach `public/icons/` → TSX anpassen

---

## Pfade (HorosCloudV5)

| Zweck | Pfad | URL / Import |
|-------|------|----------------|
| Statische Web-Assets (Vite) | `HorosCloudV5/apps/web/public/images/{slug}/` | `/images/{slug}/full.webp` |
| Region-Slices | `…/public/images/{slug}/regions/` | `/images/{slug}/regions/…` |
| Icons | `HorosCloudV5/apps/web/public/icons/{slug}/` | `/icons/{slug}/full.webp` |
| Workspace-weite Drafts | `assets/images/{slug}/` | Import oder später nach `public/` |
| Feature-Docs / Screenshots | `HorosCloudV5/docs/screenshots/<slug>/` | Relativ in `docs/features/...` |

**Vite-Regel:** Alles unter `public/` wird 1:1 unter `/` ausgeliefert. Kein Import nötig für `<img src="/images/...">`.

**Import-Variante** (kleine Assets, Hash im Build):

```tsx
import heroUrl from "../assets/hero-demo.png";

<img
  src={heroUrl}
  alt="HorosCloud Backup Übersicht"
  className="w-full rounded-lg border border-slate-700 object-cover"
/>
```

---

## Einbettungs-Patterns (React + Tailwind)

### Einfaches Hero-Bild

```tsx
<figure className="overflow-hidden rounded-lg border border-slate-700 bg-slate-800">
  <img
    src="/images/hero-backup.png"
    alt="HorosCloud Backup Dashboard"
    className="aspect-video w-full object-cover"
    loading="lazy"
  />
  <figcaption className="px-4 py-2 text-xs text-slate-400">
    Illustration — HorosCode / HorosCloud
  </figcaption>
</figure>
```

### Widget-Card mit Icon (dunkler Hintergrund im PNG)

```tsx
<div className="flex items-center gap-3 rounded-lg border border-slate-700 bg-slate-800 p-4">
  <img
    src="/icons/widget-notizen.png"
    alt=""
    aria-hidden
    className="h-10 w-10 shrink-0 object-contain"
  />
  <div>
    <h3 className="text-sm font-medium text-slate-100">Notizen</h3>
    <p className="text-xs text-slate-400">Widget-Vorschau</p>
  </div>
</div>
```

### Dekoratives Hintergrundbild (low opacity)

```tsx
<div className="relative overflow-hidden rounded-lg border border-slate-700 bg-slate-800 p-6">
  <img
    src="/images/pattern-subtle.png"
    alt=""
    aria-hidden
    className="pointer-events-none absolute inset-0 h-full w-full object-cover opacity-10"
  />
  <div className="relative z-10">{/* Inhalt */}</div>
</div>
```

**A11y:** Dekorative Bilder `alt=""` + `aria-hidden`; informative Bilder beschreibendes `alt` auf Deutsch.

---

## Checkliste vor „fertig“

- [ ] **Asset-Ordner** `{slug}/` mit `full.{ext}`, Manifest, `regions/regions.json` + README
- [ ] **Manifest** via `@horos-image-manifest` — mit `Read` verifiziert
- [ ] **Regions** via `@horos-image-regions` — Slices + json; `Read` auf full + ≥1 Region
- [ ] **GenerateImage** (oder explizit gewählte externe API) genutzt — Default war Cursor-native
- [ ] **Web-Nachbearbeitung** via `@horos-web-image-optimize` (WebP / srcset / OG wo sinnvoll)
- [ ] Bild liegt im **richtigen** Ordner (`public/`, `docs/screenshots/`, `assets/`)
- [ ] **TSX/MD/HTML** referenziert den neuen Pfad (nicht nur Datei erzeugt)
- [ ] Stil passt zu `@horos-design-system` (Dark, Amber sparsam)
- [ ] `alt`/A11y gesetzt
- [ ] Bei Screenshots in Docs: `@screenshot-verify` (Read auf PNG)
- [ ] Keine API-Keys im Code
- [ ] User informiert: **kein Key** bei Cursor-Workflow; bei optionalen APIs welche Keys nötig wären

---

## Agent-Zuordnung

| Agent | Wann |
|-------|------|
| **Zuko** | Raster/Icons/Mockups — **@horos-image-embed** → GenerateImage → **@horos-web-image-optimize** |
| **horos-ui** | Einbindung in Settings/Dashboard — nach Optimierung: srcset, lazy, OG — **@horos-design-system** |
| **Iroh** | Doc-Illustrationen unter `docs/screenshots/` |

---

## Referenzen

- Design: `.cursor/skills/horos-design-system/SKILL.md`
- Web-Optimierung (CLI): `.cursor/skills/horos-web-image-optimize/SKILL.md`
- Pillow-Referenz: `.cursor/skills/image-utils/SKILL.md`
- Manifest (Pflicht): `.cursor/skills/horos-image-manifest/SKILL.md`
- Regions (Pflicht): `.cursor/skills/horos-image-regions/SKILL.md`
- Cursor Generate (Standard): `.cursor/skills/cursor-image-generation/SKILL.md`
- OpenAI Script (optional): `.cursor/skills/generating-images/SKILL.md`
- Bria (optional): `.cursor/skills/bria-ai/SKILL.md`, `.cursor/skills/remove-background/SKILL.md`
- Web-Demo: `HorosCloudV5/apps/web/src/pages/DemoPage.tsx`
- Firma/Produkt: `.cursor/rules/horoscode.mdc`
