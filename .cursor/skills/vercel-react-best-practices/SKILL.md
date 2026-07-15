---
name: vercel-react-best-practices
description: React- und Next.js-Performance-Optimierungsrichtlinien von Vercel Engineering. Diesen Skill nutzen beim Schreiben, Reviewen oder Refactoring von React/Next.js-Code für optimale Performance-Patterns. Triggert bei React-Komponenten, Next.js-Pages, Data Fetching, Bundle-Optimierung oder Performance-Verbesserungen.
license: MIT
metadata:
  author: vercel
  version: "1.0.0"
---

# Vercel React Best Practices

Umfassender Performance-Optimierungsleitfaden für React- und Next.js-Anwendungen, gepflegt von Vercel. Enthält 69 Regeln in 8 Kategorien, nach Impact priorisiert, um automatisiertes Refactoring und Code-Generierung zu leiten.

## Wann anwenden

Diese Richtlinien nutzen, wenn:
- Du neue React-Komponenten oder Next.js-Pages schreibst
- Du Data Fetching implementierst (client- oder serverseitig)
- Du Code auf Performance-Probleme reviewst
- Du bestehenden React/Next.js-Code refactorierst
- Du Bundle-Größe oder Ladezeiten optimierst

## Regelkategorien nach Priorität

| Priority | Category | Impact | Prefix |
|----------|----------|--------|--------|
| 1 | Eliminating Waterfalls | CRITICAL | `async-` |
| 2 | Bundle Size Optimization | CRITICAL | `bundle-` |
| 3 | Server-Side Performance | HIGH | `server-` |
| 4 | Client-Side Data Fetching | MEDIUM-HIGH | `client-` |
| 5 | Re-render Optimization | MEDIUM | `rerender-` |
| 6 | Rendering Performance | MEDIUM | `rendering-` |
| 7 | JavaScript Performance | LOW-MEDIUM | `js-` |
| 8 | Advanced Patterns | LOW | `advanced-` |

## Schnellreferenz

### 1. Eliminating Waterfalls (CRITICAL)

- `async-cheap-condition-before-await` - Günstige sync-Bedingungen vor await von Flags oder Remote-Werten prüfen
- `async-defer-await` - await in Zweige verschieben, wo es wirklich genutzt wird
- `async-parallel` - Promise.all() für unabhängige Operationen
- `async-dependencies` - better-all für partielle Abhängigkeiten
- `async-api-routes` - Promises früh starten, await spät in API-Routes
- `async-suspense-boundaries` - Suspense zum Streamen von Content

### 2. Bundle Size Optimization (CRITICAL)

- `bundle-barrel-imports` - Direkt importieren, Barrel-Dateien vermeiden
- `bundle-dynamic-imports` - next/dynamic für schwere Komponenten
- `bundle-defer-third-party` - Analytics/Logging nach Hydration laden
- `bundle-conditional` - Module nur laden, wenn Feature aktiviert ist
- `bundle-preload` - Preload bei Hover/Focus für gefühlte Geschwindigkeit

### 3. Server-Side Performance (HIGH)

- `server-auth-actions` - Server Actions wie API-Routes authentifizieren
- `server-cache-react` - React.cache() für per-Request-Deduplizierung
- `server-cache-lru` - LRU-Cache für Cross-Request-Caching
- `server-dedup-props` - Doppelte Serialisierung in RSC-Props vermeiden
- `server-hoist-static-io` - Statisches I/O (Fonts, Logos) auf Modulebene hoisten
- `server-no-shared-module-state` - Kein modulweites mutable Request-State in RSC/SSR
- `server-serialization` - An Client Components übergebene Daten minimieren
- `server-parallel-fetching` - Komponenten umstrukturieren für parallele Fetches
- `server-parallel-nested-fetching` - Verschachtelte Fetches pro Item in Promise.all ketten
- `server-after-nonblocking` - after() für nicht-blockierende Operationen

### 4. Client-Side Data Fetching (MEDIUM-HIGH)

- `client-swr-dedup` - SWR für automatische Request-Deduplizierung
- `client-event-listeners` - Globale Event-Listener deduplizieren
- `client-passive-event-listeners` - Passive Listener für Scroll
- `client-localstorage-schema` - localStorage-Daten versionieren und minimieren

### 5. Re-render Optimization (MEDIUM)

- `rerender-defer-reads` - Nicht State subscriben, das nur in Callbacks genutzt wird
- `rerender-memo` - Teure Arbeit in memoized Komponenten extrahieren
- `rerender-memo-with-default-value` - Default-Non-Primitive-Props hoisten
- `rerender-dependencies` - Primitive Dependencies in Effects
- `rerender-derived-state` - Abgeleitete Booleans subscriben, nicht Rohwerte
- `rerender-derived-state-no-effect` - State beim Render ableiten, nicht in Effects
- `rerender-functional-setstate` - Functional setState für stabile Callbacks
- `rerender-lazy-state-init` - Funktion an useState für teure Werte
- `rerender-simple-expression-in-memo` - Kein memo für einfache Primitives
- `rerender-split-combined-hooks` - Hooks mit unabhängigen Dependencies splitten
- `rerender-move-effect-to-event` - Interaktionslogik in Event-Handler
- `rerender-transitions` - startTransition für nicht-dringende Updates
- `rerender-use-deferred-value` - Teure Renders deferieren für responsive Eingabe
- `rerender-use-ref-transient-values` - Refs für häufige transient Werte
- `rerender-no-inline-components` - Keine Komponenten innerhalb von Komponenten definieren

### 6. Rendering Performance (MEDIUM)

- `rendering-animate-svg-wrapper` - div-Wrapper animieren, nicht SVG-Element
- `rendering-content-visibility` - content-visibility für lange Listen
- `rendering-hoist-jsx` - Statisches JSX außerhalb von Komponenten extrahieren
- `rendering-svg-precision` - SVG-Koordinatenpräzision reduzieren
- `rendering-hydration-no-flicker` - Inline-Script für client-only Daten
- `rendering-hydration-suppress-warning` - Erwartete Mismatches unterdrücken
- `rendering-activity` - Activity-Komponente für show/hide
- `rendering-conditional-render` - Ternary statt && für Conditionals
- `rendering-usetransition-loading` - useTransition für Loading-State bevorzugen
- `rendering-resource-hints` - React-DOM-Resource-Hints für Preloading
- `rendering-script-defer-async` - defer oder async auf script-Tags

### 7. JavaScript Performance (LOW-MEDIUM)

- `js-batch-dom-css` - CSS-Änderungen via Klassen oder cssText gruppieren
- `js-index-maps` - Map für wiederholte Lookups bauen
- `js-cache-property-access` - Objekteigenschaften in Schleifen cachen
- `js-cache-function-results` - Funktionsergebnisse in modulweiter Map cachen
- `js-cache-storage` - localStorage/sessionStorage-Reads cachen
- `js-combine-iterations` - Mehrere filter/map in eine Schleife kombinieren
- `js-length-check-first` - Array-Länge vor teurem Vergleich prüfen
- `js-early-exit` - Früh aus Funktionen returnen
- `js-hoist-regexp` - RegExp-Erstellung außerhalb von Schleifen hoisten
- `js-min-max-loop` - Schleife für min/max statt sort
- `js-set-map-lookups` - Set/Map für O(1)-Lookups
- `js-tosorted-immutable` - toSorted() für Immutability
- `js-flatmap-filter` - flatMap zum Mappen und Filtern in einem Durchlauf
- `js-request-idle-callback` - Nicht-kritische Arbeit auf Browser-Idle-Zeit deferieren

### 8. Advanced Patterns (LOW)

- `advanced-effect-event-deps` - `useEffectEvent`-Ergebnisse nicht in Effect-Deps
- `advanced-event-handler-refs` - Event-Handler in Refs speichern
- `advanced-init-once` - App einmal pro App-Load initialisieren
- `advanced-use-latest` - useLatest für stabile Callback-Refs

## Nutzung

Einzelne Regeldateien für detaillierte Erklärungen und Code-Beispiele lesen:

```
rules/async-parallel.md
rules/bundle-barrel-imports.md
```

Jede Regeldatei enthält:
- Kurze Erklärung, warum es wichtig ist
- Falsches Code-Beispiel mit Erklärung
- Richtiges Code-Beispiel mit Erklärung
- Zusätzlicher Kontext und Referenzen

## Vollständiges kompiliertes Dokument

Für den kompletten Leitfaden mit allen ausgebreiteten Regeln: `AGENTS.md`
