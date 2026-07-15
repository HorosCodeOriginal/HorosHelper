# /review-ui — UI visuell prüfen

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Nach `*Preview.axaml` Screenshot
- Vor Human Approval
- Command nach `/create-preview`

## Agent

**ui-review-agent**

## Skill-Kette

```
@screenshot-reviewer → @visual-regression → @visual-qa-testing
```

## Schritte

1. Mockup (SOLL) und `previews/<region>/current.png` (IST) laden
2. Abweichungen P0–P2 listen
3. Score nach `docs/review-checklist.md` (Ziel **≥98**)
4. `docs/visual-regression-baselines.md` aktualisieren
5. `docs/metrics.md` — Review Count +1
6. Bei ≥98: **Human Approval anfragen** (`@human-approval-gate`)

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| Score + Diff-Liste | Chat + `task-registry.md` |
| Screenshots | `previews/<region>/` |
| Status `done` oder zurück `in_progress` | `region-registry.md` |

## Human Approval

**Ja** — nach technischem Score ≥98. Review ersetzt **nicht** User-Freigabe (`12-human-approval`).

## Verweise

- `workflows/visual-regression-loop.md`
- `/compare-screenshot`
