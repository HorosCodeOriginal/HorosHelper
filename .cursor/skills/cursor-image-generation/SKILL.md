---
name: cursor-image-generation
description: >-
  Generiert und iteriert Bilder in Cursor mit dem eingebauten Bildmodell und
  starken Prompts. Verwenden bei Icons, Illustrationen, UI-Mockups, Diagrammen,
  Marketing-Visuals oder jedem Raster-Asset aus Textbeschreibung oder
  Referenzbild.
metadata:
  author: oh-my-cursor
  version: "1.0.0"
---

# Cursor-Bildgenerierung (Nano Banana Pro)

Generiere Bilder im **Cursor-Agent** mit dem **GenerateImage**-Tool. Bildgenerierung läuft über **Google Nano Banana Pro**. Previews landen standardmäßig unter **`assets/`**, sofern du nichts anderes angibst.

Dieser Skill behandelt **Prompting** und **Workflow**, nicht Figma oder Vector-Code (dafür andere Skills).

## Grober Prompt rein, starker Prompt raus

Der Nutzer kann eine **kurze oder vage** Anfrage liefern („Hero für die Login-Seite“, „Cyberpunk-Icon“). **Übergib** diesen String **nicht** roh an **GenerateImage**, wenn die unten beschriebenen Schichten fehlen. Stattdessen:

1. **Fehlende Constraints ableiten oder nachfragen** (Medium, Aspect Ratio, Stil, Brand-Farben, zu rendernder Text).
2. **Anfrage umschreiben** in einen strukturierten Prompt (oder enge zweite Runde) nach den Prinzipien unten.
3. **GenerateImage** nur mit dem umgeschriebenen Prompt aufrufen.

Der Skill ist der Vertrag: Deine Aufgabe ist, die Nutzer-Absicht **vor** der Generierung zu **erweitern und zu schärfen**, dann mit **Deltas** zu **iterieren**.

## Wann diesen Skill nutzen

- Nutzer will ein **Bild**, **Icon**, **Hero-Visual**, **Diagramm-Look**, **Mockup-Still** oder **Iteration** auf ein bestehendes Bild.
- Du brauchst **Text im Bild** (Titel, Labels, Buttons in einem Mockup).
- Nutzer lädt ein **Referenzbild** hoch und will Variation oder Edit-Richtung.

## Kernprinzipien (Nano Banana / Gemini Image-Familie)

Die Bullets unten sind eine **verdichtete Synthese** gängiger Hinweise für Nano Banana Pro / Gemini Image-Modelle — keine wörtlichen Zitate. Für autoritative Formulierungen und Edge Cases die **References** unten.

Sie folgen dem Geist der öffentlichen Google-Guides ([Prompt-Tipps](https://blog.google/products/gemini/prompting-tips-nano-banana-pro), [Google Cloud Guide](https://cloud.google.com/blog/products/ai-machine-learning/ultimate-prompting-guide-for-nano-banana), [DeepMind Prompt Guide](https://deepmind.google/models/gemini-image/prompt-guide/)):

1. **Briefe einen menschlichen Künstler** — Klare, grammatikalische Sätze. Kein Keyword-Soup (`"cyber, 4k, hdr, epic"`), außer du willst bewusst Tag-Ästhetik.
2. **Beschreibung schichten** — Subjekt → Aktion/Pose → Umgebung → **Kamera** (Wide Shot, Isometric, Macro) → **Licht** (weiches Fensterlicht, Neon-Rim, bewölkt) → **Materialien** (gebürstetes Aluminium, mattes Papier, Glas) → **Stil** (Editorial-Foto, Flat Illustration, Low-Poly-3D-Render).
3. **Text in Bildern** — Exakte Wortwahl in **doppelten Anführungszeichen** und Typografie-Gefühl (z. B. `"bold geometric sans"`, `"narrow serif for headlines"`). Bei wichtigem Text Lesbarkeit und hohen Kontrast verlangen.
4. **Aspect Ratio und Framing** — **Orientierung** (quadratisch, 16:9 Landscape, 9:16 Story) und **Safe Margins** bei Crop (z. B. App-Icon: zentriertes Motiv, Padding).
5. **Editieren, nicht immer neu würfeln** — Wenn das Bild grob passt, **spezifische Änderungen** verlangen (`"change the background to warm beige"`, `"make the logo 20% larger"`, `"remove the extra person on the left"`) statt komplett neuem Prompt.
6. **Referenzbilder** — Bei Referenz beschreiben, **was bleiben soll** (Palette, Mood, Komposition) und **was sich ändern soll**, damit das Modell nicht driftet.

## Anti-Patterns

- Vage Superlative ohne visuellen Anker: „schöner / premium / moderner“ — immer **konkrete** Hinweise (Materialien, Palette, Epoche, Referenz).
- Widersprüchliche Constraints in einem Shot: „minimalistisch“ + „dichte Infografik“ + „einzelnes Hero-Objekt“ — in **Schritte** oder Iterationen teilen.
- **Use Case** ignorieren: Icon vs. Hero vs. Print — **Zielgröße** oder **Betrachtungsabstand** nennen, wenn relevant.

## Workflow

0. **Preflight** — `node scripts/ensure-image-paths.mjs` (legt `public/` + `assets/` an). **Kein** `OPENAI_API_KEY` — nur **GenerateImage**, nicht `generate_image.py`.
1. **Erfassen** — Nutzer-Brief auch bei einer Zeile annehmen; Lücken notieren.
2. **Klären** — Output-Medium (Icon, Social, Slide, Mockup), grobe Maße oder Aspect Ratio, Brand-Farben, Must-have vs. Nice-to-have (nur fragen, wenn blockierend).
3. **Umschreiben** — **Vollen** Prompt nach Schicht-Reihenfolge oben produzieren.
4. **Generieren** — **GenerateImage** mit umgeschriebener Description. Speichern unter **`assets/images/{asset-slug}/full.png`** (oder User-Pfad als `{parent}/{slug}/full.{ext}`) mit beschreibendem Asset-Slug.
5. **Iterieren** — Wenn nah dran: **Delta**-Prompt; wenn falsch: die **Schicht** anpassen, die scheiterte (Kamera, Licht, Stil), bevor alles weggeworfen wird.
6. **Manifest + Regions** — `@horos-image-manifest` + `@horos-image-regions`: Ordner `{slug}/` mit Manifest, `regions.json`, Slices; mit `Read` verifizieren.
7. **Melden** — Asset-Ordner-Pfad, `full.{ext}`, **Manifest-Pfad**, **regions/**, finalen Prompt zurückgeben.

## Prompting-Patterns (kopieren und anpassen)

Eckige Klammern `[like-this]` sind Platzhalter. Doppelte Anführungszeichen `"..."` markieren exakten Text im Bild.

**App-Icon (quadratisch, klein lesbar)**

```text
Square app icon, centered symbol of [subject], flat vector style with subtle depth, limited palette [colors], 10% safe margin from edges, no tiny text, crisp edges, high contrast on [background tone].
```

**UI-Mockup-Still (Marketing)**

```text
Photorealistic product screenshot of a [mobile/web] app, [screen name] view, centered device, soft studio lighting, neutral background, clean sans UI. Render the following text exactly: headline "[headline text]", button label "[CTA text]". Modern SaaS aesthetic.
```

**Illustration (kein Foto)**

```text
Editorial illustration of [subject], [mood], limited palette [colors], visible brush texture or clean vector shapes (pick one), generous whitespace, no photorealistic faces unless requested.
```

**Diagramm / Konzept**

```text
Isometric diagram of [system], simple shapes, light grid, high contrast lines, no clutter, presentation slide style. Label the zones exactly: "[zone A label]", "[zone B label]".
```

## Beispiele: schwach vs. stärker

| Schwach | Stärker |
|------|----------|
| `"A nice logo for my app"` | `Minimal wordmark for a productivity app, lowercase sans-serif feel, single accent color #2563EB on white, generous letter-spacing, no icon, horizontal logo lockup.` |
| `"Cyberpunk city"` | `Wide 16:9 cinematic shot of a rainy cyberpunk street at night, neon reflections on wet asphalt, single vanishing point, shallow depth of field, no readable text, teal and magenta accents.` |
| `"Fix the image"` | `Keep the same subject and composition; change only the background to soft gradient from #0f172a to #1e293b; leave lighting on the subject unchanged.` |

---

## Manifest + Regions (Pflicht)

Nach **jedem** erfolgreichen **GenerateImage**-Aufruf **sofort** `@horos-image-manifest` **und** `@horos-image-regions`:

1. Asset-Ordner `{parent}/{asset-slug}/` anlegen
2. `full.{ext}` als canonical Master
3. Metadaten: `optimize_web_image.py --info` + Dominant-Color-Script (≥5 `#RRGGBB`)
4. Manifest: `{asset-slug}.manifest.md` im Ordner
5. Regionen identifizieren (Read auf Bild) → crop → `regions/`
6. `regions.json`, README, Mini-Manifests pro Region
7. `Read` auf full + mindestens 1 Region — **kein Erfolg ohne Ordner + regions/**

Rule: `@image-asset` · Templates: `@horos-image-manifest`, `@horos-image-regions`

---

## References

- [Google — Prompt tips (Nano Banana Pro)](https://blog.google/products/gemini/prompting-tips-nano-banana-pro)
- [Google Cloud — Ultimate prompting guide (Nano Banana)](https://cloud.google.com/blog/products/ai-machine-learning/ultimate-prompting-guide-for-nano-banana)
- [DeepMind — Gemini image prompt guide](https://deepmind.google/models/gemini-image/prompt-guide/)
