# /compare-screenshot — Screenshot vs. Mockup

**Firma:** HorosCode · **Stack:** Avalonia Desktop

## Trigger

- Detaillierter Pixel-Vergleich
- Nach Korrektur-Runde (Re-Review)
- Baseline-Update

## Agent

**ui-review-agent**

## Skill-Kette

```
@screenshot-reviewer → @visual-regression
```

## Schritte

1. Pfade aus `docs/visual-regression-baselines.md` laden
2. Expected (Mockup) vs. Current (Screenshot) vs. Diff (optional)
3. Regionale Hotspots markieren (Header, Sidebar, …)
4. Score + priorisierte Fix-Liste
5. Baseline-Tabelle aktualisieren

## Erwartete Outputs

| Output | Ort |
|--------|-----|
| Vergleichsbericht | Chat |
| PNGs | `previews/<region>/expected|current|diff.png` |
| Score | `region-registry.md` Spalte Score |

## Human Approval

**Nein** für Vergleich. **Ja** wenn Ergebnis ≥98 und Region abgeschlossen werden soll.

## Verweise

- `docs/visual-regression.md`
- `/review-ui`
