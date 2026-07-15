Nutze **Zuko** (visueller Spezialist) bei Bedarf für visuellen Kontext — primär aber den **horos-web-image-optimize**-Skill (HorosCode) und das **image-utils**-CLI-Script.

0. **Preflight:** `node scripts/ensure-image-paths.mjs` — stellt `public/images`, `public/icons` sicher. Kein `OPENAI_API_KEY` nötig.
1. **Lies** den Skill **horos-web-image-optimize** (`skills/horos-web-image-optimize/SKILL.md`) und kläre nur Blockierendes (Eingabepfad im Asset-Ordner `{slug}/full.{ext}`, Ziel: Hero/Icon/OG/responsive, Ausgabe unter `HorosCloudV5/apps/web/public/{images|icons}/{slug}/`).
2. **Eingabe** — `full.{ext}` im Asset-Ordner bestätigen; optional `--info` für Metadaten; **regions/**-Slices ebenfalls optimieren.
3. **CLI ausführen** — `.cursor/skills/image-utils/scripts/optimize_web_image.py` mit passenden Flags:
   - `--optimize` — einzelnes WebP für `full` und Region-Slices
   - `--resize <px>` — feste Breite/Höhe (z. B. Icons 512)
   - `--responsive` — Varianten 640/960/1280/1920 (oder `--responsive-widths`)
   - `--og` — Open Graph 1200×630
   - `--out <pfad>` — Ziel im Asset-Ordner (`{slug}/full.webp`, `regions/*.webp`)
   - `--json` — strukturierte Ausgabe für Agent-Parsing
4. **Manifest + Regions (Pflicht):** Nach Optimierung **`@horos-image-manifest`** + **`@horos-image-regions`** aktualisieren (WebP-Pfade in json, Regions-Tabelle); `Read` verifizieren. Rule: `@image-asset`.
5. **Ausgabe melden** — alle `.webp`-Pfade (full + regions), Manifest-Pfad, Dateigrößen, Exit-Code (`0` = Erfolg).
6. **Einbindung** — optional `<picture>`-srcset, Layered Regions, `width`/`height`, `alt`/`aria-hidden`; bei `--og` OG/Twitter-Meta. Für TSX: **@horos-design-system**; für vollen Flow: **@horos-image-embed**.

Wenn der User **neu generieren und optimieren** will (nicht nur bestehendes Raster), verweise auf `/image` bzw. **@horos-image-embed** als End-to-End-Pipeline.
