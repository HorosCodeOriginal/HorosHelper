---
name: parallel-code-review
description: HorosCode 5-Perspektiven Code-Review — Fidelity, MVVM, Tokens, a11y, Anti-Patterns parallel.
---

# Purpose

Strukturiertes Multi-Perspektiven-Review vor Fertig-Meldung — fängt Abweichungen auf, die ein einzelner Check übersieht.

**Firma:** HorosCode · **Rule:** `07-review-loop`, `10-anti-patterns`

# Five Perspectives

Parallel prüfen (mental oder als Checkliste):

## 1. Fidelity Reviewer

- Mockup vs. Screenshot: Spacing, Typo, Colors
- Score ≥ 98? (`@pixel-perfect-ui`)
- Design verbessert? → **Reject**

## 2. MVVM Reviewer

- View = nur AXAML + Bindings, keine Business-Logik
- ViewModel: `INotifyPropertyChanged`, Commands korrekt
- Kein Code-Behind außer `InitializeComponent`
- `docs/avalonia-patterns.md`, `docs/coding-standards.md`

## 3. Token Reviewer

- Alle Colors/Fonts/Spacing aus ResourceDictionary
- Keine hardcoded `#RRGGBB` in AXAML
- Neue Tokens in `design-memory.md` dokumentiert

## 4. Accessibility Reviewer

- Kontrast, Focus-States, AutomationProperties
- Keyboard-Navigation testbar
- Rule `08-testing`

## 5. Anti-Pattern Reviewer

- Kein Web/Tailwind-Mix in Avalonia
- Kein Big-Bang (nur ein Bereich?)
- Preview existiert? Human Approval pending?
- Rule `10-anti-patterns`

# Output

```markdown
## Parallel Review — Header
| Perspective | Status | Notes |
|-------------|--------|-------|
| Fidelity | ✅ 98/100 | — |
| MVVM | ✅ | Clean bindings |
| Tokens | ⚠️ | 1 hardcoded color → fix |
| a11y | ✅ | Names set |
| Anti-Patterns | ✅ | Single region |
**Verdict:** Fix Token issue → re-review
```

# Rule

Alle 5 Perspektiven müssen **pass** sein — ein ⚠️ blockiert Fertig-Meldung.

# Verweise

- `@visual-regression`, `@pixel-perfect-ui`, `@avalonia-preview`
- Docs: `docs/review-checklist.md`, `docs/coding-standards.md`
