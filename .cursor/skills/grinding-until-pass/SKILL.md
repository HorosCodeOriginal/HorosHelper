---
name: grinding-until-pass
description: HorosCode Build/Test-Loop bis grün — dotnet build, Lint, keine kaputten Zwischenstände.
---

# Purpose

Technisches Qualitäts-Gate: **Build und Lint müssen grün sein**, bevor visuelles Review oder Fertig-Meldung.

**Firma:** HorosCode · **Rules:** `08-testing`, `09-completion`

# Workflow

```
Edit → Build → Lint → Fix → Repeat until pass
```

## Loop

```bash
dotnet build
# Bei Fehler: Fehler lesen → minimal fixen → erneut build

# Optional pro geänderte Datei:
# ReadLints auf *.axaml, *.cs
```

## Checklist pro Iteration

- [ ] `dotnet build` Exit Code 0
- [ ] `ReadLints` auf alle geänderten Dateien — 0 Errors
- [ ] Keine `as any`, `@ts-ignore`, leeren catch-Blöcke
- [ ] Preview kompiliert (`*Preview.axaml` im Projekt)
- [ ] Keine temporären Debug-Ausgaben (`Console.WriteLine` in ViewModels)

## Bei Fehlschlag

1. **Root Cause** lesen — nicht raten
2. **Minimaler Fix** — kein Shotgun-Debugging
3. Max. **3 Iterationen** pro Fehlerklasse
4. Nach 3 Fehlschlägen: User einbeziehen mit Fehler-Log

# Stop Condition

```
dotnet build → 0 errors, 0 warnings (Projekt-Standard)
ReadLints → 0 errors auf geänderten Dateien
```

Erst dann: visuelles Gate (`@visual-regression`).

# Kombination mit Wide Ultra-Logical

Bei `/wul`: Grinding-Loop ist **Pflicht** vor jeder Erfolgsmeldung — kein „fast fertig" mit rotem Build.

# Verweise

- Rule: `08-testing`, `09-completion`
- Visuell danach: `@visual-regression`, `@visual-qa-testing`
- Command: `/mockup-avalonia` Schritt 7
