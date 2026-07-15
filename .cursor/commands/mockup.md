Nutze **horos-ui** (HorosCloud Web-UI-Spezialist) — **nicht Zuko** — für Figma/Mockup → React/TSX.

1. **Lies** `@horos-figma-to-code` (`.cursor/skills/horos-figma-to-code/SKILL.md`) und folge der Skill-Kette:
   - `@horos-design-system` → `@implementing-figma-designs` → optional `@ui-design-brain`
2. **Figma MCP** — Frame/URL laden (`.cursor/mcp.json`); ohne Figma nur Screenshot-Fallback (~70–85 % Parity).
3. **Mock-Daten first** — UI pixelnah; kein i18n/API im ersten Pass.
4. **Mappe** auf bestehende Komponenten in `HorosCloudV5/apps/web/src` (Glob/Grep vor neuen Primitives).
5. **Verify** — ReadLints + Build; bei `/wul` vollständig bis grün.
6. **Screenshot-Parity** — `@screenshot-capture` + Read-Verify (`screenshot-verify.mdc`).

**Raster only** (Icons, Hero, Illustration): `/image` → **Zuko** + `@horos-image-embed`.

**Gib zurück:** geänderte Dateien, Parity-Status, offene Abweichungen zum Figma-Frame.
