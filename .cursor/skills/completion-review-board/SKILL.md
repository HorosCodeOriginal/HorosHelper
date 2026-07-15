---
name: completion-review-board
description: HorosCode Final-Gate — Build, Types, a11y, Preview, Screenshot, Architektur — nie fertig vor Bestehen aller Checks.
---

# Purpose

**Abschluss-Prüfung** vor „Done"-Meldung — Region, Bereich oder gesamte Shell. Aggregiert alle Gates in einer finalen Checkliste.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Rule:** `09-completion`

# Checks

## Region Complete (ein Bereich)

| # | Check | Skill / Rule | Pflicht |
|---|-------|--------------|---------|
| 1 | Mockup-Analyse dokumentiert | `@figma-analysis`, `03-mockup-analysis` | ✓ |
| 2 | Region in `ui-regions.md` geplant | `@ui-region-planner` | ✓ |
| 3 | Tokens aus design-memory | `@design-token-extractor` | ✓ |
| 4 | Komponenten: Search→Reuse→Extend→Create | `@component-discovery` | ✓ |
| 5 | MVVM sauber | `@avalonia-mvvm` | ✓ |
| 6 | Architektur validiert | `@architecture-guardian` | ✓ |
| 7 | Preview läuft isoliert | `@preview-first-development` | ✓ |
| 8 | `dotnet build` grün | `@grinding-until-pass`, `08-testing` | ✓ |
| 9 | `ReadLints` clean | `08-testing` | ✓ |
| 10 | Screenshot-Review ≥ 98 | `@screenshot-reviewer`, `@visual-regression` | ✓ |
| 11 | a11y Basis (Kontrast, Focus) | `@visual-qa-testing` | ✓ |
| 12 | Human Approval erhalten | `@human-approval-gate`, `12-human-approval` | ✓ |

## Shell / App Complete (alle Regionen)

Zusätzlich zu oben:

- [ ] Jede Region einzeln completed (alle 12 Checks)
- [ ] Shell-Assembly mit gleichem 98-Gate reviewed
- [ ] Full-App-Screenshot vs. Gesamt-Mockup
- [ ] User-Freigabe für finale Assembly

## Completion Report

```markdown
## Completion Review — Header

| Check | Status |
|-------|--------|
| Analysis | ✓ docs/ui-regions.md |
| MVVM | ✓ HeaderViewModel, no code-behind logic |
| Architecture | ✓ @architecture-guardian pass |
| Preview | ✓ HeaderPreview.axaml |
| Build | ✓ dotnet build OK |
| Screenshot | ✓ 98/100 — reviews/review-header-v2.png |
| a11y | ✓ contrast OK, AutomationProperties.Name set |
| Human Approval | ✓ approved 2026-07-14 |

**Verdict: COMPLETE** — proceed to Sidebar
```

# Output

- Ausgefüllte Completion-Checkliste
- Verdict: **COMPLETE** oder **BLOCKED** (mit fehlenden Checks)
- Bei BLOCKED: konkrete nächste Schritte

# Rule

> **Niemals als fertig markieren, bevor alle Checks bestanden.** „Mostly done", fehlender Score oder fehlende Human Approval = BLOCKED. Kein nächster Bereich bei BLOCKED-Status.

# Verweise

- Rule: `09-completion.mdc`, `12-human-approval`, `08-testing`
- Works with: gesamte High-End-Kette (siehe `docs/skills.md`)
- Docs: `docs/review-checklist.md`, `docs/skills.md`
- Agent: `avalonia-ui` · Command: `/mockup-avalonia`
