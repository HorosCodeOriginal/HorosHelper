Nutze **avalonia-ui** (HorosCloud Desktop-Spezialist) — **nicht horos-ui** — für Figma/Mockup → Avalonia AXAML.

## High-End 10-Skill Kernset (primär)

```
Mockup → Regionen planen → Design analysieren → Bereich bauen → Preview
  → Screenshot prüfen → Freigabe → nächster Bereich → finale QA
```

| # | Skill | Gate |
|---|-------|------|
| 1 | `@pixel-perfect-ui` | Mockup = Wahrheit, Tokens only |
| 2 | `@incremental-ui` | **Ein Bereich** pro Durchlauf |
| 3 | `@ui-region-planner` | Regionen in `docs/ui-regions.md` — **kein Code vorher** |
| 4 | `@figma-analysis` | Mockup analysieren, Spec schreiben |
| 5 | `@avalonia-preview` | `*Preview.axaml` isoliert renderbar |
| 6 | `@screenshot-reviewer` | Detaillierte Abweichungsanalyse |
| 7 | `@visual-regression` | Score **≥ 98/100** |
| 8 | `@human-approval-gate` | **STOP** — User-Freigabe abwarten |
| 9 | `@avalonia-mvvm` | View/ViewModel/Service, Bindings |
| 10 | `@completion-review-board` | Final-Gate — nie fertig vor Bestehen aller Checks |

**Build-Gate:** `@grinding-until-pass` — `dotnet build` grün vor Fertig-Meldung.

### Tier 2 unterstützend

`@design-token-extractor` · `@component-discovery` · `@preview-first-development` · `@design-memory-updater` · `@architecture-guardian`

### Tier 1 Community ergänzend

`@codebase-onboarding` · `@visual-qa-testing` · `@screenshotting-changelog` · `@parallel-code-review`

Legacy ergänzend: `@avalonia-figma-to-code` → `@avalonia-design-system` → `@implementing-figma-designs-avalonia`

## Schritte

1. **Regionen planen** — `@ui-region-planner` → `docs/ui-regions.md` (Rules `01-workflow`)
2. **Figma MCP** — Frame/URL laden (`.cursor/mcp.json`); ohne Figma Screenshot-Fallback (~70–85 % Parity)
3. **Design analysieren** — `@figma-analysis` + `@design-token-extractor` → Tokens in `design-memory.md`
4. **Ein Bereich** — `@incremental-ui` + `@avalonia-mvvm` + `@pixel-perfect-ui`
5. **Preview-First** — `@preview-first-development` + `@avalonia-preview` (`06-ui-preview`)
6. **Screenshot prüfen** — `@screenshot-reviewer` → `@visual-regression` (≥ 98)
7. **Freigabe** — `@human-approval-gate` (Rules `12-human-approval`) — **nicht weiter ohne OK**
8. **Finale QA** — `@completion-review-board` (Rules `09-completion`)
9. **Mock-Daten first** — UI pixelnah; kein i18n/API im ersten Pass
10. **Verify** — `dotnet build`; Score **≥ 98/100** (`07-review-loop`, `docs/review-checklist.md`)
11. Bei `/wul` vollständig bis Build grün — visuelles Gate bleibt Pflicht

**Raster only** (Icons, Illustration): **Zuko** — nicht in AXAML nachmalen.

**Gib zurück:** geänderte Dateien, Review-Score, Screenshot-Pfad, offene Abweichungen zum Mockup, Human-Approval-Status.
