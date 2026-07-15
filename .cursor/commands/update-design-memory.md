# /update-design-memory — Design-Wissensbasis pflegen

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Neue Tokens aus Mockup (`@design-token-extractor`)
- Nach User-Freigabe neuer Werte
- Drift-Fix aus `@design-system-guardian`

## Agent

**ui-implementation-agent** + Skill `@design-memory-updater`

## Skill-Kette

```
@design-token-extractor → @design-memory-updater → @design-system-guardian
```

## Schritte

1. Neue/geänderte Werte mit Mockup-Beleg dokumentieren
2. `docs/design-memory.md` aktualisieren
3. `docs/design-tokens.md` synchron halten (Referenz)
4. `Styles/DesignTokens.axaml` im echten Projekt spiegeln
5. `docs/design-drift-prevention.md` — Begründung bei Abweichung

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| Token-Einträge | `design-memory.md` |
| Drift-Log | `project-memory.md` Known Issues |

## Human Approval

**Ja** wenn Token vom Mockup abweicht (bewusste Entscheidung).

## Verweise

- `docs/design-drift-prevention.md`
- Rule `02-pixel-perfect.mdc`
