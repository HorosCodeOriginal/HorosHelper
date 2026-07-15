---
name: design-system-guardian
description: HorosCode Avalonia — prüft Farben, Radius, Spacing, Typography und Component Usage gegen design-memory und governance. Verhindert Design-Drift vor Commit/Review.
---

# Design System Guardian — HorosCode

**Firma:** HorosCode · **Produkt:** HorosCloud

Pflicht-Skill vor Implementierung neuer UI und bei Token-Änderungen. Ergänzt `@design-token-extractor` — fokussiert auf **Validierung**, nicht Extraktion.

---

## Wann anwenden

- Vor erstem AXAML einer Region
- Nach `@design-token-extractor` — Werte gegen `docs/design-memory.md` prüfen
- Bei Command `/update-design-memory` — Drift-Check
- Wenn Agent neue Farbe/Spacing vorschlägt
- Recovery: Agent hat Inline-Styles erfunden (`workflows/recovery-workflow.md`)

---

## Prüfablauf

```
1. READ docs/design-memory.md + docs/design-tokens.md
2. READ docs/component-governance.md (Allowed/Deprecated)
3. GREP geänderte .axaml auf Inline-Farben, Magic Numbers
4. MAP jeden Wert → Token oder Mockup-Beleg
5. FAIL wenn unbelegt → docs/design-drift-prevention.md Prozess
```

---

## Harte Regeln

| Bereich | Erlaubt | Verboten |
|---------|---------|----------|
| Farben | `DynamicResource Horos*` | `#RRGGBB` inline |
| Spacing | `HorosSpacing*` | `Margin="13"` ohne Token |
| Radius | `HorosRadius*` | Ad-hoc `CornerRadius` |
| Typography | `Classes="horos-*"` | FontSize hardcoded |
| Komponenten | `component-governance.md` Allowed | Deprecated ohne ADR |

---

## Component Usage

1. `docs/component-catalog.md` — existiert Komponente?
2. `docs/component-guidelines.md` — richtige Wahl (Button vs. Hyperlink)?
3. Status in `component-governance.md`: **Experimental** nur mit Flag

Neue Komponente → Catalog + Governance **vor** Merge.

---

## Bei Drift

1. Stoppen — kein weiteres AXAML
2. Wert in `design-memory.md` dokumentieren **oder** Mockup-Wert korrigieren
3. `@design-memory-updater` für persistente Tokens
4. `docs/metrics.md` — Open Issues +1 bis gelöst

---

## Output

```
design-system-guardian: pass | fail
Violations: [{file, line, type, value, expected}]
Action: use_token | document_in_memory | fix_mockup_ref
```

---

## Verweise

- `docs/design-drift-prevention.md`
- `docs/component-governance.md`
- Rules: `02-pixel-perfect.mdc`, `10-anti-patterns.mdc`
- Persona: `docs/personas.md` → Design System Guardian
