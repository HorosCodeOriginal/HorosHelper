---
name: generating-images
description: >-
  OPTIONAL — OpenAI Image API (gpt-image-2) nur wenn der Nutzer explizit OpenAI
  statt Cursor GenerateImage will. Erfordert OPENAI_API_KEY. Standard-Bildpfad:
  @cursor-image-generation + GenerateImage (kein Key). Nicht automatisch laden.
user-invocable: true
---

# Bilder generieren (OpenAI gpt-image-2)

> **Nicht der HorosCode-Default.** Standard ist **Cursor GenerateImage** via `@cursor-image-generation` — **kein** `OPENAI_API_KEY`. Diesen Skill **nur** nutzen, wenn der Nutzer **explizit** OpenAI / `generate_image.py` verlangt.

Nutze diesen Skill, wenn der Nutzer ein Bild generieren oder bearbeiten möchte. Er wrappt OpenAIs `gpt-image-2` via Python-Skript, unterstützt Text-only-Prompts und ein oder mehr Referenzbilder, schreibt PNG/JPEG/WebP auf Disk.

## Harte Regeln (nicht verletzen)

1. **Immer `gpt-image-2`.** Nie Fallback auf `gpt-image-1`, `dall-e-3` oder andere Modelle. Das Skript hat deshalb kein `--model`-Flag.
2. **Bei Fehlern sofort abbrechen.** Nicht retryen, Modelle tauschen, Credentials patchen oder Qualität still degradieren. Exit ≠ 0: Fehler wörtlich an Nutzer und stoppen.
3. **Fehlendes `OPENAI_API_KEY` nicht „fixen“** durch `.env`, 1Password usw., außer der Nutzer sagt es explizit. Fehlt die Variable: fragen, wie sie bereitgestellt werden soll, dann stoppen.
4. **Preflight nur für diesen Skill:** `node scripts/ensure-image-paths.mjs --require-openai` — prüft Key **nur** hier, nicht im Cursor-Default-Pfad.

## Wann nutzen

- Nutzer will ein generiertes Bild: Icon, Logo, Illustration, Mockup, OG-Bild,
  Blog-Hero, Marketing-Asset, Concept Art, diagrammartiges Bild usw.
- Nutzer liefert ein oder mehrere Bilder und will bearbeiten, restylen, kombinieren oder
  als Referenz nutzen.
- Nutzer will einen Bildteil entfernen/ersetzen (mit `--mask`).

**Nicht** diesen Skill nutzen für:
- Charts/Plots/Data Viz (stattdessen per Code generieren).
- Bestehende Fotos beschaffen (Stock-Photo-Skill falls vorhanden).
- Screenshots der App des Nutzers (Screenshot-Skill falls vorhanden).

## Voraussetzungen

### 1. OpenAI API-Key

Du brauchst `OPENAI_API_KEY` in der Umgebung. Key unter
[platform.openai.com/api-keys](https://platform.openai.com/api-keys).

Der Skill liefert `.env.example` neben dieser `SKILL.md`. Kopieren und
Key eintragen:

```bash
cp .env.example .env
# then edit .env and put your real key in
```

Dann vor dem Skript exportieren:

```bash
set -a && source .env && set +a
```

Oder direkt in der Shell:

```bash
export OPENAI_API_KEY="sk-..."
```

Ist `OPENAI_API_KEY` nicht gesetzt, beendet das Skript sofort mit Code 2.
**Nicht** von woanders lesen ohne explizite Erlaubnis des Nutzers.

### 2. Org-Verifizierung

Deine OpenAI-Org muss für `gpt-image-2` verifiziert sein unter
[platform.openai.com/settings/organization/general](https://platform.openai.com/settings/organization/general).
Bei 403 mit „organization must be verified“ melden und
stoppen — Modelle nicht wechseln.

### 3. Python-Abhängigkeit

```bash
pip install --upgrade openai
```

## Skript-Speicherort

Das Python-Skript liegt neben dieser `SKILL.md` unter `scripts/generate_image.py`.
Ist der Skill unter `~/.cursor/skills/generating-images/` installiert, liegt das
Skript unter `~/.cursor/skills/generating-images/scripts/generate_image.py`.

Es gibt absolute Pfade der geschriebenen Bilder auf stdout aus. Fehler gehen
auf stderr mit non-zero Exit-Code; das Skript bricht beim ersten Fehler ab.

## Aufruf

Immer über das Shell-Tool. Sinnvollen Output-Pfad im Workspace wählen
(z. B. `./public/generated/<slug>.png` für Web-Projekte oder
`./<slug>.png` sonst).

### 1. Text-to-image

```bash
python3 ~/.cursor/skills/generating-images/scripts/generate_image.py \
  --prompt "Minimal flat-vector app icon for a note-taking app, indigo gradient, rounded square, soft shadow" \
  --size 1024x1024 \
  --quality high \
  --out ./icon.png
```

### 2. Image-to-image (one reference)

```bash
python3 ~/.cursor/skills/generating-images/scripts/generate_image.py \
  --prompt "Restyle this photo as a watercolor painting with warm tones" \
  --image ./photo.jpg \
  --out ./photo-watercolor.png
```

### 3. Multiple reference images

```bash
python3 ~/.cursor/skills/generating-images/scripts/generate_image.py \
  --prompt "Photorealistic flat-lay product shot combining all of these items on a white background" \
  --image ./a.png --image ./b.png --image ./c.png \
  --out ./flatlay.png
```

### 4. Masked edit (inpainting)

Die Maske muss gleiche Größe und Format wie das erste Input-Bild haben, mit Alpha-Kanal für die editierbare Region.

```bash
python3 ~/.cursor/skills/generating-images/scripts/generate_image.py \
  --prompt "Replace the sky with a vivid sunset" \
  --image ./scene.png --mask ./sky-mask.png \
  --out ./scene-sunset.png
```

### 5. Batch / parallel mode (many distinct images at once)

Wenn du **mehrere verschiedene Bilder** auf einmal generieren musst (e.g. a set
of blog heroes, several icon variations with different prompts, OG images for
many pages), use `--batch` instead of running the script N times. It runs all
jobs in parallel from a single Python process — much faster than serial calls
and avoids repeated SDK startup cost.

JSON-Datei mit allen Jobs schreiben, dann Skript einmal aufrufen:

```bash
cat > /tmp/img-jobs.json <<'EOF'
[
  {
    "prompt": "Minimal flat-vector app icon for a note-taking app, indigo gradient, rounded square",
    "out": "./public/icons/notes.png",
    "size": "1024x1024",
    "quality": "high"
  },
  {
    "prompt": "Photoreal blog hero: a cozy library with warm afternoon light, 5:3 ratio",
    "out": "./public/static/blog/library.png",
    "size": "1600x960",
    "quality": "medium"
  },
  {
    "prompt": "Restyle this product photo as a watercolor painting with warm tones",
    "image": ["./public/products/mug.jpg"],
    "out": "./public/products/mug-watercolor.png"
  }
]
EOF

python3 ~/.cursor/skills/generating-images/scripts/generate_image.py \
  --batch /tmp/img-jobs.json --concurrency 5
```

Jedes Job-Objekt akzeptiert dieselben Felder wie CLI-Flags: `prompt` (required),
`out`, `size`, `quality`, `format`, `n`, `image` (string or array of strings),
`mask`. Defaults wie beim Single-Shot-CLI.

Verhalten:

- Alle Jobs laufen parallel bis `--concurrency` (Default 4). Sinnvoller Bereich 3–8; OpenAI rate-limited pro Org — nicht übertreiben.
- Absoluter Pfad jedes erfolgreich geschriebenen Bildes sofort auf stdout, eine Zeile pro Job.
- Schlägt ein Job fehl, Fehler auf stderr (`ERROR: job <i> failed: ...`)
  und Skript exit 1 **nach** den restlichen Jobs. Andere Jobs werden nicht abgebrochen — partieller Output ist ok, nur fehlgeschlagene retryen.
- `--batch` schließt sich gegenseitig mit `--prompt` / `--image` / `--mask` aus.

**Wann `--batch` statt paralleler Shell-Calls:** sobald du ≥2 verschiedene Bilder im selben Turn generierst. Nicht mehrere parallele Shell-Aufrufe — ein Batch-Call.

**Nicht mit `--n` verwechseln.** `--n` erzeugt mehrere Variationen des *gleichen* Prompts in einem API-Call (günstiger, gleiche Idee). `--batch` führt *verschiedene* Prompts parallel aus. Kombinierbar: Batch-Job mit `"n": 4` für 4 Variationen eines Prompts.

## Flags-Referenz

| Flag | Default | Hinweise |
|------|---------|-------|
| `--prompt` | required* | Pflicht außer bei `--batch`. Immer angeben, auch beim Editieren. |
| `--image` | none | Mehrfach für mehrere Referenzen. Triggert `images.edit`. |
| `--mask` | none | Optionale Inpainting-Maske (PNG mit Alpha). |
| `--out` | `./image.png` | Output-Pfad; Index-Suffix bei `--n > 1`. |
| `--size` | `auto` | `1024x1024`, `1536x1024`, `1024x1536`, `2048x2048`, `3840x2160` usw. Kanten Vielfache von 16, max. 3840px, Ratio ≤ 3:1. |
| `--quality` | `auto` | `low` (schnelle Entwürfe), `medium`, `high` (finale Assets). |
| `--format` | `png` | `png`, `jpeg`, `webp`. |
| `--n` | `1` | Variationen des GLEICHEN Prompts in einem Call. |
| `--batch` | none | Pfad zu JSON-Array von Job-Objekten; parallel ausführen. |
| `--concurrency` | `4` | Max. parallele Worker im `--batch`-Modus. |

Es gibt absichtlich **kein `--model`-Flag**. Modell ist hardcoded `gpt-image-2`.

## Größen-Empfehlung

- App icons / square thumbnails → `1024x1024`
- Landing-page heroes / OG images → `1536x1024`
- Blog hero (5:3) → `1600x960` (both edges multiples of 16, ratio = 5:3)
- Mobile / portrait illustrations → `1024x1536`
- Marketing posters / 4K assets → `3840x2160`

## Qualitäts-Empfehlung

- `low` für schnelle Exploration / Entwürfe (günstigst, schnellst).
- `medium` ist guter Default.
- `high` nur für finale, ship-ready Assets — deutlich teurer, bis ~2 Minuten.

Sagt der Nutzer nur „generate an image“ ohne Finalitäts-Signal, Default `--quality medium`.

## Prompt-Schreib-Tipps

Für beste Ergebnisse im Prompt:
- Subjekt (was im Bild ist)
- Stil (flat vector, watercolor, photoreal, isometric, line drawing, 3D render…)
- Komposition / Kamera (close-up, top-down, wide shot)
- Farbpalette / Stimmung
- Hintergrund (weiß, Verlauf, Szene — Hinweis: `gpt-image-2` unterstützt keine transparenten Hintergründe)
- Text der erscheinen muss, in Anführungszeichen (`gpt-image-2` rendert Text gut)

Bei vagem Prompt mit sinnvollen Defaults erweitern statt nachfragen, außer die Anfrage ist wirklich mehrdeutig.

## Nach der Generierung

1. Output-Pfad dem Nutzer zurückmelden.
2. Bild **nicht** in Markdown einbetten — Cursor zeigt generierte Dateien automatisch im Workspace.
3. **`@horos-image-manifest` (Pflicht):** `{basename}.manifest.md` neben Output schreiben — voller Prompt, OpenAI-Metadaten (`--size`, `--quality`), Dimensionen, Hex-Farben, Einbindungs-Snippet; mit `Read` verifizieren.
4. Für Website/App ggf. Optimizer (`@horos-web-image-optimize`) wenn Dateigröße zählt — danach Manifest aktualisieren.

## Kontext VOR Generierung sammeln

Hat der Nutzer nicht exakt gesagt, was er will (Subjekt, Stil, Palette, Größe, Ziel), zuerst kurz Kontext sammeln. Ziel: Bild wirkt am Zielort, nicht wie random Asset. Diesen Schritt überspringen = häufigster Grund für off-brand Ergebnisse.

In etwa dieser Reihenfolge prüfen:

1. **Geschwister-Bilder am Zielort.** If the image will live in
   `public/static/blog/`, `public/static/marketing/`, `assets/`, etc., open
   one or two existing images in that folder with the Read tool. Match their:
   - Illustration style (3D cartoon, flat vector, photoreal, line art, isometric…)
   - Color palette and lighting
   - Subject conventions (e.g. "always features the product mascot", "always a
     metaphor, never literal screenshots", etc.)
   - Aspect ratio and resolution

2. **Die Oberfläche, die es anzeigt.** Read the relevant file:
   - Blog post → read the MDX/Markdown (title, tags, opening paragraphs, key metaphors).
   - Landing page section → read the component, headline, and surrounding copy.
   - README → read the top of the README.
   - Component → read the component to understand what it represents.

   Pull the image's *meaning* from the actual content, not just the filename.

3. **Brand / Design Tokens.** If the project has a clearly defined palette,
   logo, or mascot, mirror them. Quick places to check:
   - `tailwind.config.*` for brand colors
   - `globals.css` / theme files for CSS variables
   - `public/` for logos / mascot assets
   - Any existing OG images or marketing assets

4. **Aspect Ratio / Größe.** Pick `--size` based on the surface:
   blog hero, OG image, square avatar, mobile portrait, etc. Match what's
   already there.

Dann Prompt mit dem Gelernten schreiben: subject pulled from the
content, style + palette pulled from sibling assets and brand tokens,
composition matched to the surface.

Hat der Nutzer *explizite* Vorgaben (Stil, Farben, exaktes Subjekt), respektieren und Kontext-Sammlung überspringen. Bei partiellen Vorgaben Kontext für offene Teile sammeln.

Keine Klärungsfragen für Dinge, die du aus der Codebase ableiten kannst — zuerst inferieren, nur bei echter Mehrdeutigkeit fragen.

## Platzieren UND einbinden — nicht nur Datei ablegen

Will der Nutzer ein Bild für eine konkrete Oberfläche (Blog, Landing Page, OG-Card, README, Komponente usw.), bist du für den ganzen Job verantwortlich, nicht nur die PNG. Immer in dieser Reihenfolge:

1. **Korrekten Speicherort wählen** for that surface. Look at what already
   exists and match it. Examples:
   - Blog hero → wherever existing blog images live (e.g.
     `apps/<app>/public/static/blog/<slug>.png`).
   - Landing page asset → wherever other landing assets live (e.g.
     `apps/<app>/public/static/marketing/...`).
   - README / docs image → `docs/images/`, `assets/`, or next to the doc.
   - Component-specific asset → next to the component or in its
     `public/`/`assets/` folder.

   Use the file's slug, component name, or section name for the filename. Don't
   invent a new convention if one already exists.

2. **Bild einbinden** so it actually shows where the user wanted it. This is
   not optional. Examples:
   - Blog post MDX → update the `image:` (or equivalent) frontmatter field to
     point at the new path. Replace any placeholder Unsplash/stock URL.
   - Landing page section → import or reference the new asset in the relevant
     component/JSX.
   - OG image → update the `<meta property="og:image">` / metadata config.
   - README → add the appropriate Markdown image tag.

3. **Bestehende Konventionen matchen** for paths (relative vs `/static/...` vs
   `@/assets/...`), file format (png/webp/jpg), and any wrapper components
   (`next/image`, custom `<Image>`, etc.).

4. **Nicht zuerst fragen.** If the user asked for an image for a known surface, do
   the placement + wiring automatically and tell them what you changed at the
   end. Only ask when the destination is genuinely ambiguous.

## Fehler — melden, nicht verstecken

Tritt eines der folgenden ein, **sofort stoppen** und Fehler melden. Nicht retryen, Modell nicht wechseln, Prompt nicht ändern.

- `OPENAI_API_KEY is not set` → Nutzer fragen, wie er sie bereitstellt.
- `openai package not installed` → Nutzer `pip install --upgrade openai` sagen.
- 403 "organization must be verified" → Nutzer Verifizierung unter
  [platform.openai.com/settings/organization/general](https://platform.openai.com/settings/organization/general).
  Modelle nicht wechseln.
- 400 size error → melden; Nutzer gültige Größe wählen lassen.
- 400 transparent background → melden; `gpt-image-2` unterstützt keine Transparenz.
- Jeder andere API-Fehler → wörtlich melden und stoppen.
