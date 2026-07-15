Nutze **Zuko** (den visuellen Spezialisten) mit dem **cursor-image-generation**-Skill für diese Aufgabe.

0. **Preflight:** `node scripts/ensure-image-paths.mjs` — legt `public/images`, `public/icons`, `assets/images` an falls fehlend. **Kein** `OPENAI_API_KEY` nötig (Cursor GenerateImage). `generate_image.py` nur bei explizitem OpenAI-Wunsch + `--require-openai`.
1. **Lies** den **cursor-image-generation**-Skill (`skills/cursor-image-generation/SKILL.md`) und folge seinem Workflow: **erweitere ein grobes User-Briefing zu einem vollen Prompt**, bevor du **GenerateImage** aufrufst (layered prompts, iteration).
2. **Kläre** nur Blockierendes (Asset-Typ, Aspect Ratio, Brand Colors, Referenzbild), wenn die User-Nachricht zu vage für sicheres Rewriting ist.
3. **Rewrite** die Anfrage per Skill in einen starken Prompt, dann **generate** mit dem **GenerateImage**-Tool; speichere als **Asset-Ordner** `{parent}/{asset-slug}/full.{ext}` (Default: `assets/images/{slug}/full.png`).
4. **Manifest + Regions (Pflicht):** Nach GenerateImage **`@horos-image-manifest`** + **`@horos-image-regions`** — Ordner mit `{slug}.manifest.md`, `regions/regions.json`, README, Slices; `Read` auf full + ≥1 Region. Rule: `@image-asset`.
5. **Iteriere** mit gezielten Edits per Skill — Manifest/Regions-Version aktualisieren.
6. **Gib zurück**: Asset-Ordner-Pfad, `full.{ext}`, **regions/**, Manifest-Pfad, finalen Prompt, optionale nächste Iterationen.

Wenn die Aufgabe **Figma-Frame oder Mockup → React/TSX Code** ist, nutze **horos-ui** mit **`@horos-figma-to-code`** (Commands: `/mockup`, `/figma-code`) — **nicht** Zuko für Layout.
