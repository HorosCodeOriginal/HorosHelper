# /create-preview — Isoliertes Preview anlegen

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Neue View ohne Preview
- Hook: nach `*.axaml` View (nicht Preview) erstellt
- Vor Screenshot-Workflow

## Agent

**ui-implementation-agent**

## Skill-Kette

```
@preview-first-development → @avalonia-preview → @grinding-until-pass
```

## Schritte

1. `templates/view-template.md` als Struktur
2. `<Region>Preview.axaml` + minimales ViewModel mit Mock-Daten
3. Eintrag in `docs/preview-registry.md`
4. `dotnet build` — Preview muss kompilieren
5. Optional: Screenshot nach `previews/<region>/current.png`

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| `*Preview.axaml` | `Views/Previews/` oder Projektkonvention |
| Registry-Eintrag | `docs/preview-registry.md` |
| Build | grün |

## Human Approval

**Nein** — technischer Schritt. Approval nach vollständigem Region-Review.

## Verweise

- Rule `06-ui-preview.mdc`
- `previews/README.md`
