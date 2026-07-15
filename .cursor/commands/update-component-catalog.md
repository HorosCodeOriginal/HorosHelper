# /update-component-catalog — Komponenten-Inventar pflegen

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Neue UserControl erstellt (nach Search→Create)
- Komponente deprecated
- Governance-Status ändern

## Agent

**ui-implementation-agent** oder **architecture-agent**

## Skill-Kette

```
@component-discovery → @design-system-guardian
```

## Schritte

1. `@component-discovery` — Duplikat-Check
2. Eintrag in `docs/component-catalog.md` (Name, Pfad, Props, Status)
3. `docs/component-governance.md` — Allowed/Deprecated/Experimental
4. `docs/component-guidelines.md` — Wann verwenden
5. Preview-Referenz falls vorhanden

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| Catalog-Zeile | `component-catalog.md` |
| Governance-Status | `component-governance.md` |

## Human Approval

**Ja** für Status **Deprecated** oder Löschung. **Nein** für neues Allowed mit Mockup-Bezug.

## Verweise

- Rule `04-component-discovery.mdc`
- `templates/view-template.md`
