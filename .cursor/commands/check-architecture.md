# /check-architecture — MVVM & Struktur prüfen

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Neue ViewModel/Service-Dateien
- Hook nach ViewModel-Änderung
- Vor Integration in Shell
- Recovery bei Schicht-Verstoß

## Agent

**architecture-agent**

## Skill-Kette

```
@architecture-guardian → @avalonia-mvvm
```

## Schritte

1. Geänderte Dateien gegen `docs/architecture.md` prüfen
2. Ordnerstruktur vs. `examples/folder-structure/`
3. DI-Registrierung, Navigation (`ADR/adr-002-navigation.md`)
4. Verstöße P0/P1 listen
5. Entscheidungen in `docs/project-memory.md` wenn neu

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| Architektur-Report | Chat |
| ADR-Vorschlag | nur bei struktureller Änderung |
| Kein UI-Redesign | — |

## Human Approval

**Ja** wenn ADR oder Breaking Structure Change vorgeschlagen wird.

## Verweise

- `docs/coding-standards.md`
- `/create-adr`
