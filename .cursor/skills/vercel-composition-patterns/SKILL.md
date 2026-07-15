---
name: vercel-composition-patterns
description:
  React-Composition-Patterns, die skalieren. Nutzen beim Refactoring von Komponenten mit
  Boolean-Prop-Proliferation, beim Bau flexibler Komponentenbibliotheken oder beim
  Design wiederverwendbarer APIs. Triggert bei Compound Components,
  Render Props, Context Providers oder Komponentenarchitektur. Enthält React-19-API-Änderungen.
license: MIT
metadata:
  author: vercel
  version: '1.0.0'
---

# React Composition Patterns

Composition-Patterns für flexible, wartbare React-Komponenten. Vermeide Boolean-Prop-Proliferation durch Compound Components, State-Lifting und Komposition interner Teile. Diese Patterns machen Codebases für Menschen und KI-Agents beim Skalieren leichter.

## Wann anwenden

Diese Richtlinien nutzen, wenn:

- Du Komponenten mit vielen Boolean-Props refactorierst
- Du wiederverwendbare Komponentenbibliotheken baust
- Du flexible Komponenten-APIs designst
- Du Komponentenarchitektur reviewst
- Du mit Compound Components oder Context Providers arbeitest

## Regelkategorien nach Priorität

| Priority | Category                | Impact | Prefix          |
| -------- | ----------------------- | ------ | --------------- |
| 1        | Component Architecture  | HIGH   | `architecture-` |
| 2        | State Management        | MEDIUM | `state-`        |
| 3        | Implementation Patterns | MEDIUM | `patterns-`     |
| 4        | React 19 APIs           | MEDIUM | `react19-`      |

## Schnellreferenz

### 1. Component Architecture (HIGH)

- `architecture-avoid-boolean-props` - Keine Boolean-Props für Verhaltensanpassung; Composition nutzen
- `architecture-compound-components` - Komplexe Komponenten mit geteiltem Context strukturieren

### 2. State Management (MEDIUM)

- `state-decouple-implementation` - Nur der Provider weiß, wie State verwaltet wird
- `state-context-interface` - Generisches Interface mit state, actions, meta für Dependency Injection
- `state-lift-state` - State in Provider-Komponenten für Sibling-Zugriff verschieben

### 3. Implementation Patterns (MEDIUM)

- `patterns-explicit-variants` - Explizite Varianten-Komponenten statt Boolean-Modi
- `patterns-children-over-render-props` - children für Composition statt renderX-Props

### 4. React 19 APIs (MEDIUM)

> **⚠️ Nur React 19+.** Diesen Abschnitt überspringen bei React 18 oder früher.

- `react19-no-forwardref` - Kein `forwardRef`; `use()` statt `useContext()` nutzen

## Nutzung

Einzelne Regeldateien für detaillierte Erklärungen und Code-Beispiele lesen:

```
rules/architecture-avoid-boolean-props.md
rules/state-context-interface.md
```

Jede Regeldatei enthält:

- Kurze Erklärung, warum es wichtig ist
- Falsches Code-Beispiel mit Erklärung
- Richtiges Code-Beispiel mit Erklärung
- Zusätzlicher Kontext und Referenzen

## Vollständiges kompiliertes Dokument

Für den kompletten Leitfaden mit allen ausgebreiteten Regeln: `AGENTS.md`
