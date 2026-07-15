# /analyze-mockup — Mockup analysieren

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Neues Mockup/Figma-Frame vor Implementierung
- Start eines Regions-Tasks
- Recovery: Agent hat ohne Analyse codiert

## Agent

**ui-implementation-agent** (Vorbereitung) oder **avalonia-ui**

## Skill-Kette

```
@figma-analysis → @design-token-extractor → @ui-region-planner
  → @design-system-guardian (Validierung)
```

## Schritte

1. Mockup/Screenshot lesen (Figma MCP oder PNG)
2. Regionen identifizieren → `docs/region-registry.md` aktualisieren
3. Tokens extrahieren → `docs/design-memory.md` (via `/update-design-memory`)
4. Komponenten-Mapping → `docs/component-catalog.md` Notizen
5. Analyse-Spec in `docs/task-registry.md` verlinken

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| Regions-Tabelle | `docs/region-registry.md` |
| Token-Liste | `docs/design-memory.md` |
| Komponenten-Hypothesen | Task-Notiz |
| Kein AXAML | — |

## Human Approval

**Nicht erforderlich** für Analyse allein. Erforderlich wenn neue Tokens vom Mockup abweichen und dokumentiert werden müssen.

## Verweise

- `workflows/mockup-to-region.md`
- Rule `03-mockup-analysis.mdc`
- `docs/prompt-library.md` → „Analyze Mockup"
