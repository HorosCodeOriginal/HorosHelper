---
name: ui-design-brain
description: Produktionsreife UI mit echten Komponenten-Patterns und Best Practices aus 60+ dokumentierten Interface-Komponenten. Verwenden, wenn du Web-Interfaces, Seiten, Dashboards, Formulare, Navigation oder beliebige UI bauen sollst — modernes, minimales SaaS-Qualitäts-Output auf Basis von Design-System-Konventionen statt generischer KI-Patterns.
license: Complete terms in LICENSE.txt
metadata:
  source: https://github.com/carmahhawwari/ui-design-brain
---

# UI Design Brain

Komponenten-Pattern-Katalog (60+ Patterns von [component.gallery](https://component.gallery)). **Nicht** der primäre HorosCode-Einstiegspunkt — Routing siehe unten.

## Wann nutzen (HorosCode)

| Schritt | Skill |
|------|-------|
| 1. Tokens, Theme, HorosCloud-Widgets | **`@horos-design-system`** (immer zuerst für HorosCloud) |
| 2. Welcher Komponententyp? (Modal vs. Drawer, Table-Layout) | **Dieser Skill** → [components.md](components.md) |
| 3. HorosCloud-UI implementieren | **horos-ui**-Agent oder Executor mit `@horos-design`-Rule |

**Nicht nutzen:** `@using-ui-stack` — veraltet; in `@horos-design-system` zusammengeführt.

## Wann nutzen (nicht-HorosCloud)

Anwenden beim Bau generischer Web-UI, wenn du Komponentenauswahl brauchst:

- SaaS-Dashboards, Formulare, Navigation, Overlays
- React/HTML/Tailwind-Komponenten ohne projektspezifisches Design-System

Lies [components.md](components.md) als vollständige Referenz, bevor du UI-Code generierst.

## Design-Philosophie

Jede generierte Oberfläche soll sich **modern, minimal und produktionsreif** anfühlen — nicht wie ein Template.

### Kernprinzipien

1. **Zurückhaltung statt Dekoration.** Weniger Elemente, hoch verfeinert. Weißraum ist ein Feature.
2. **Typografie trägt Hierarchie.** Display-Font mit klarer Body-Font paaren. Gewichtskontrast zwischen Überschriften und Labels maximieren.
3. **Ein starker Farbmoment.** Zuerst neutrale Palette (warme Off-Whites, Near-Blacks, gedämpfte Mid-Tones). Ein selbstbewusster Akzent. Wenn es auf einem Poster oder Buchcover wirken könnte, ist es wahrscheinlich zeitlos.
4. **Spacing ist Struktur.** 8-px-Grid nutzen. Engere Abstände gruppieren Verwandtes; großzügige Abstände lassen Hero-Content atmen.
5. **Barrierefreiheit ist nicht verhandelbar.** WCAG-AA-Kontrastminimum. Fokus-Indikatoren. Semantisches HTML. Tastaturnavigation.
6. **Keine generische KI-Ästhetik.** Vermeiden: Lila-auf-Weiß-Verläufe, Inter/Roboto-Defaults, gleichmäßige Card-Grids, austauschbare Layouts. Jede Oberfläche soll für ihren Kontext designed wirken.

### Qualitätsmaßstab

Das Ergebnis soll dem entsprechen, was du von einem Senior Product Designer bei einem Top-SaaS-Unternehmen erwartest:

- Klarer visueller Rhythmus mit bewusster Asymmetrie
- Offensichtliche interaktive Affordanzen (Hover, Focus, Active)
- Graceful Edge Cases (Empty States, Loading, Error)
- Responsive ohne Breakpoint-Artefakte

## Workflow

### Schritt 1 — Komponenten identifizieren

Lies die User-Anfrage und bestimme benötigte UI-Komponenten. Referenziere [components.md](components.md) für jede Komponente per Name oder Alias.

Häufige Zuordnungen:

- „navigation“ → Header, Navigation, Breadcrumbs, Tabs
- „form“ → Form, Text input, Select, Checkbox, Radio button, Button
- „data display“ → Table, Card, List, Badge, Avatar
- „feedback“ → Alert, Toast, Modal, Spinner, Progress bar, Empty state
- „input“ → Text input, Textarea, Select, Combobox, Datepicker, File upload, Slider
- „overlay“ → Modal, Drawer, Popover, Tooltip, Dropdown menu

### Schritt 2 — Best Practices anwenden

Für jede Komponente in der Oberfläche deren Best Practices aus der Referenz folgen. Breit geltende Regeln:

**Layout**

- Einspaltige Formulare — schneller zu scannen
- Konsistente vertikale Lanes in wiederholten Zeilen (Listen, Tabellen)
- Feste Breiten-Slots für Icons und Aktionen, auch wenn leer
- Cards: Media → Titel → Meta → Aktion

**Interaktion**

- Buttons: Verb-first-Labels („Änderungen speichern“, nicht „Absenden“), ein Primary pro Abschnitt
- Modals: immer X, Abbrechen und Escape; Fokus fangen; Fokus beim Schließen zurückgeben
- Toasts: Auto-Dismiss 4–6 s, manuelles Schließen erlauben, neueste oben stapeln
- Toggles: nur bei sofortiger Wirkung — Checkboxen in Formularen mit Speichern

**Typografie & Spacing**

- Strikte Heading-Hierarchie (h1 → h2 → h3), ein h1 pro Seite
- Mindestens 44 px Touch-Targets auf Mobile
- Labels über Inputs (vertikale Formulare) oder daneben (horizontal)
- Placeholder nur als Formathinweis, nie als Label-Ersatz

**States**

- Empty States: Illustration + hilfreiche Headline + Primary-CTA
- Loading: Skeleton Screens > Spinner (nach 300 ms Verzögerung zeigen)
- Validierung: inline on blur, nicht bei jedem Tastendruck
- Disabled: visuell klar, aber noch lesbar

### Schritt 3 — Design-Richtung wählen

Style-Preset wählen, das zur User-Intent passt, oder bei Unklarheit nachfragen:

**Modern SaaS** (Standard)

- Neutrale Palette, ein starker Akzent
- 8-px-Grid, großzügiger Weißraum
- Clean, professionell, luftig

**HorosCloud / Enterprise Dashboard** (HorosCode-Produkte)

- Dark-first: `#0f172a` Hintergrund, Slate-Oberflächen, Amber-Akzent
- Informationsdichte Settings-Panels mit klarer Abschnittshierarchie
- Siehe `@horos-design-system` für Tokens und Widget-Patterns

**Apple-level Minimal**

- Fast monochrom, warme Grautöne
- Große Typo-Hierarchie, enges Tracking bei Display-Text
- Viel Weißraum, Micro-Interactions (150–250 ms ease-out)

**Data Dashboard**

- Datendicht, für Scanbarkeit optimiert
- Konsistente vertikale Ausrichtung über Zeilen
- Klare Metrik-Hierarchie: KPI → Trend → Detail

### Schritt 4 — Code generieren

Produktionsreifen Code nach diesen Regeln schreiben:

```
Stack:       React + Tailwind CSS (sofern User nichts anderes angibt)
Spacing:     Tailwind-Spacing-Skala (p-2, gap-4, etc.) auf 8px-Grid
Colors:      CSS-Variablen oder Tailwind-Config für Palette-Konsistenz
Typography:  Tailwind text utilities; ausdrucksstarke Font-Pairings via Google Fonts
States:      Hover, Focus, Active, Disabled für alle interaktiven Elemente
Responsive:  Mobile-first; testen bei 375, 768, 1440 px
Accessibility: Semantisches HTML, ARIA wo nötig, Fokus-Management
```

## Komponenten-Kurzreferenz

Die 15 häufigsten Komponenten. Für alle 60+ mit Best Practices, Aliasen und Layout-Beispielen siehe [components.md](components.md).

| Komponente | Wann nutzen | Kernregel |
|-----------|------------|----------|
| **Button** | Aktionen auslösen | Verb-first-Labels; ein Primary pro Abschnitt |
| **Card** | Entität darstellen | Media → Titel → Meta → Aktion; Schatten ODER Border, nicht beides |
| **Modal** | Fokussierte Aufmerksamkeit | Fokus fangen; X + Abbrechen + Escape zum Schließen |
| **Navigation** | Seiten-/Abschnittslinks | Max. 5–7 Items; klarer Active-State |
| **Table** | Strukturierte Daten | Sticky Header; Zahlen rechtsbündig; sortierbare Spalten |
| **Tabs** | Panels wechseln | 2–7 Tabs; Active-Indikator; Accordion auf Mobile |
| **Form** | Eingaben sammeln | Eine Spalte; Labels oben; Inline-Validierung on blur |
| **Toast** | Kurze Bestätigung | Auto-Dismiss 4–6 s; Undo bei destruktiven Ops |
| **Alert** | Wichtiger Status | Semantische Farben + Icon; max. 2 Sätze |
| **Drawer** | Sekundäres Panel | Rechts für Detail, links für Nav; 320–480 px Desktop |
| **Search input** | Inhalt finden | Cmd/Ctrl+K-Shortcut; Debounce 200–300 ms |
| **Empty state** | Keine Daten | Illustration + Headline + CTA; positive Formulierung |
| **Skeleton** | Loading-Platzhalter | Layout-Form treffen; Shimmer-Animation |
| **Badge** | Status/Metadaten-Label | 1–2 Wörter; Pill für Status; begrenzte Farbpalette |
| **Dropdown menu** | Aktions-/Nav-Optionen | 7±2 Items; destruktive Aktionen zuletzt in Rot |

## Anti-Patterns vermeiden

Niemals generieren — sie signalisieren generische, minderwertige UI:

- **Regenbogen-Badges** — jeder Status eine andere knallige Farbe ohne Semantik
- **Modal in Modal** — Seite oder Drawer für komplexe Flows
- **Disabled Submit ohne Erklärung** — immer zeigen, was fehlt
- **Spinner für vorhersagbare Layouts** — Skeleton Screens stattdessen
- **„Hier klicken“-Links** — Linktext muss Ziel beschreiben
- **Hamburger-Menü auf Desktop** — sichtbare Navigation, wenn Platz da ist
- **Auto-advancing Carousels** — Navigation dem User überlassen
- **Nur-Placeholder-Felder** — immer sichtbare Labels
- **Gleichgewichtige Buttons** — Primary/Secondary/Tertiary-Hierarchie
- **Winziger Text (< 12 px)** — Body mindestens 14 px, besser 16 px
