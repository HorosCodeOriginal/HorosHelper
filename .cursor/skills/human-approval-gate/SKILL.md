---
name: human-approval-gate
description: HorosCode Human-Approval-Gate — nach Region Preview+Screenshot+Summary, auf User-Freigabe warten.
---

# Purpose

Erzwingt den **menschlichen Freigabe-Stopp** nach jedem UI-Bereich. Technischer Score ≥ 98 ersetzt keine explizite User-Zustimmung.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Rule:** `12-human-approval`

# Gate

Nach Abschluss eines Bereichs (Preview + Screenshot-Review + Build):

```
Review ≥98 → [STOP] → User Approval → Nächster Bereich
```

## Was präsentieren

```markdown
## Region Ready for Approval: Header

**Preview:** Views/Previews/HeaderPreview.axaml
**Screenshot:** reviews/review-header-v2.png
**Score:** 98/100 (via @screenshot-reviewer)

### Summary
- 1440×56 Header mit Logo, Navigation, User-Menu
- Tokens: ColorSurfacePrimary, Spacing6, FontSizeNav
- Build: dotnet build OK
- MVVM: HeaderViewModel + IHeaderService (mock)

**Awaiting your approval to proceed to Sidebar.**
```

Pflicht-Inhalte:
- Preview-Dateipfad
- Screenshot-Pfad (oder eingebettetes Bild)
- Review-Score mit Kurz-Diff (falls < 100)
- Was gebaut wurde (2–4 Bullet Points)
- Explizite Freigabe-Anfrage

## Gültige User-Antworten

| User sagt | Aktion |
|-----------|--------|
| „OK" / „Approved" / „Weiter" | Status `approved` in `docs/ui-regions.md`, nächster Bereich |
| „Fix X" / Feedback | Fixes → Re-Preview → Re-Review → erneut anfragen |
| „Stop" / „Pause" | Workflow halten, State dokumentieren |
| Stille | **Nicht fortfahren** — auf explizite Freigabe warten |

## Status in ui-regions.md

```markdown
| Header | approved | 98/100 | 2026-07-14 |
| Sidebar | in_progress | — | — |
```

# Output

- Freigabe-Anfrage mit Preview + Screenshot + Summary
- Aktualisierter Region-Status nach User-OK
- Kein Start des nächsten Bereichs ohne dokumentierte Freigabe

# Rule

> **Nach jeder Region: STOP.** Score ≥ 98 ≠ automatische Freigabe. Niemals Sidebar starten, während Header auf Approval wartet. Kein Shell-Merge ohne per-Region-Approvals.

# Verweise

- Rule: `12-human-approval.mdc`, `01-workflow`
- Works with: `@screenshot-reviewer`, `@visual-regression`, `@preview-first-development`
- Docs: `docs/ui-regions.md`, `docs/review-checklist.md`
- Agent: `avalonia-ui` · Command: `/mockup-avalonia`
