# /create-adr — Architecture Decision Record

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Framework-, Navigations- oder Theme-Entscheidung
- Breaking MVVM-Änderung
- Deprecated-Komponente mit Ersatz

## Agent

**architecture-agent** (Modell: `composer-2.5`)

## Skill-Kette

```
@architecture-guardian → (Dokumentation)
```

## Schritte

1. Kontext, Entscheidung, Alternativen, Konsequenzen
2. Datei `ADR/adr-NNN-<slug>.md` anlegen (fortlaufende Nummer)
3. `docs/project-memory.md` — Kurzverweis
4. Bestehende ADRs 001–003 als Vorlage

## Vorlage

```markdown
# ADR-NNN: Titel

**Status:** proposed | accepted | deprecated
**Datum:** YYYY-MM-DD
**Kontext:** …
**Entscheidung:** …
**Alternativen:** …
**Konsequenzen:** …
```

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| ADR-Datei | `ADR/` |
| Memory-Eintrag | `project-memory.md` |

## Human Approval

**Ja** — ADR `accepted` nur nach User-Freigabe.

## Verweise

- `ADR/adr-001-framework.md` … `adr-003-theme.md`
- `/check-architecture`
