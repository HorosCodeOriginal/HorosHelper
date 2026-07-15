---
name: horos-design-system
description: HorosCloud Design System für HorosCode-Web-Produkte — Dark-Theme-Tokens, Widget-Patterns, Settings-UI und Komponenten-Konventionen. Verwenden beim Bauen oder Verfeinern der HorosCloudV5-Web-UI, Settings-Widgets, Dashboards oder Epic-Feature-Docs mit konsistenter visueller Sprache.
user-invocable: true
metadata:
  product: HorosCloud
  company: HorosCode
  version: "1.0.0"
---

# Horos Design System

Design-System-Skill für **HorosCloud** (Produkt von **HorosCode**). Nutzen bei neuen Settings-Widgets, Dashboard-Karten, Formularen und jeder Web-UI unter `HorosCloudV5/apps/web`.

**Rule (auto bei Web-Globs):** `@horos-design` · **Agent:** `horos-ui`

## UI Skill Routing (HorosCode)

| Situation | Skill / Agent |
|-----------|---------------|
| HorosCloud Web-UI bauen/refinen | **`@horos-design-system`** (dieser Skill) + `@horos-design` Rule |
| Größere UI-Features delegieren | **horos-ui** Agent |
| Komponenten-Muster wählen (Modal vs Drawer, Table-Patterns) | **`@ui-design-brain`** → [components.md](../ui-design-brain/components.md) |
| UI-Audit nach Implementierung | `@web-design-guidelines` |
| Figma Frame → HorosCloud Web-UI 1:1 | **`@horos-figma-to-code`** (Orchestrator) → `@implementing-figma-designs` |
| Icons/Mockups (Raster) | **Zuko** — nicht dieser Skill |

**Deprecated:** `@using-ui-stack` — Inhalt in diesem Skill + `horos-design.mdc` (8px-Grid, 5 States, a11y). Nicht mehr separat laden.

**Nicht-HorosCloud-UI:** `@frontend-builder` + optional `@ui-design-brain` für generische SaaS-Muster.

## Wann anwenden

- „Neues Settings-Widget“, „Dashboard-Card“, „Einstellungs-Panel“
- HorosCloud-Feature mit sichtbarer UI
- `/doc-epic` HTML/Markdown mit HorosCloud-Dark-Theme
- Figma → Code für HorosCloud (`@horos-figma-to-code` + `@implementing-figma-designs`, Mock-Daten first)

## Visual Identity

### Dark SaaS Dashboard

HorosCloud ist **dark-first**: ruhige Slate-Flächen, klare Hierarchie, **Amber** als einziger starker Brand-Akzent.

| Rolle | Hex | Tailwind | Einsatz |
|-------|-----|----------|---------|
| App background | `#0f172a` | `bg-slate-900` | Root, Agents-Browser-Hintergrund in Docs |
| Surface | `#1e293b` | `bg-slate-800` | Cards, Modals, Sidebar |
| Surface raised | `#334155` | `bg-slate-700` | Inputs, Hover-Flächen |
| Border | — | `border-slate-700` | Card-Ränder, Divider |
| Text primary | — | `text-slate-100` | Body, Titles |
| Text muted | — | `text-slate-400` | Hints, Timestamps |
| Accent | — | `text-amber-400` / `bg-amber-500` | Primary Button, Active Tab, Links |
| Accent hover | — | `hover:bg-amber-600` | Primary Button hover |

### Epic / Doc HTML

Feature-`index.html` (siehe `@doc-epic`):

- Hintergrund `#0f172a`, Sidebar `slate-800`, Amber-Akzente für aktive Nav und Links
- Mermaid-Diagramme auf dunklem Grund; Screenshots unter `docs/screenshots/`

## Spacing & Layout

- **Grid:** 8px — Tailwind `2=8px`, `4=16px`, `6=24px`, `8=32px`
- **Settings-Panel:** `p-6` äußerer Container; Sektionen mit `space-y-6`
- **Widget-Karte:** `rounded-lg border border-slate-700 bg-slate-800 p-4`
- **Form-Zeile:** Label `mb-2 block text-sm font-medium text-slate-300`; Feld `w-full`

## Typography Scale

| Element | Klassen |
|---------|---------|
| Page title | `text-2xl font-semibold text-slate-100` |
| Section title | `text-lg font-medium text-slate-100` |
| Body | `text-sm text-slate-300` oder `text-base` |
| Caption | `text-xs text-slate-400` |
| Code / ID | `font-mono text-sm text-amber-300/90` |

## Component Patterns

### Primary Button

```tsx
<button
  type="button"
  className="rounded-md bg-amber-500 px-4 py-2 text-sm font-medium text-slate-900 hover:bg-amber-600 focus:outline-none focus:ring-2 focus:ring-amber-500/50 disabled:opacity-50"
>
  Speichern
</button>
```

### Secondary Button

```tsx
<button
  type="button"
  className="rounded-md border border-slate-600 bg-slate-800 px-4 py-2 text-sm text-slate-200 hover:bg-slate-700 focus:ring-2 focus:ring-slate-500/50"
>
  Abbrechen
</button>
```

### Settings Section

```tsx
<section className="space-y-4 rounded-lg border border-slate-700 bg-slate-800/50 p-6">
  <h2 className="text-lg font-medium text-slate-100">Abschnittstitel</h2>
  {/* fields */}
</section>
```

### Widget Card (Dashboard)

- Header: Icon + Title + optional Badge
- Body: Hauptinhalt oder Preview
- Footer: sekundäre Aktionen rechts

### States-Checkliste

Für jedes interaktive Element dokumentieren/implementieren:

1. Default 2. Hover 3. Focus 4. Active 5. Disabled  
6. Loading (Skeleton bevorzugt) 7. Empty 8. Error

## Widget & Doc Slugs

UI-Pfad → Feature-Doc-Slug (kebab-case):

| UI-Pfad | Slug-Beispiel |
|---------|---------------|
| Einstellungen → Widgets → Notizen → Canvas | `settings-widgets-notizen-bereich-canvas-design` |
| Einstellungen → Widgets → YouTube | `settings-widgets-youtube-playlist` |

Docs: `HorosCloudV5/docs/features/<slug>/`

## File-Discovery-Workflow

Vor neuer UI:

1. `Glob` / `Grep` in `HorosCloudV5/apps/web/src` nach ähnlichen Widgets oder Settings-Routen
2. Geschwister-Komponenten lesen für Import-Pfade und State (Zustand, React Query usw.)
3. Bestehende Primitives erweitern — Button/Card nicht duplizieren, wenn Shared Components existieren
4. Liegen Tokens in CSS/Tailwind-Config, **lies und nutze sie** statt diese Tabelle zu duplizieren

## Integration mit Cursor-Features

| Feature | HorosCloud-Nutzung |
|---------|------------------|
| **Design Mode** | Dev-Server → Element wählen → gezielte Fixes in gemappten Komponenten |
| **Figma MCP / Plugin** | Frame-Link → `@horos-figma-to-code` + **horos-ui** Agent |
| **horos-ui agent** | Delegation für größere UI-Features |
| **Zuko** | Nur Raster/Icons/Marketing-Mockups |
| **web-design-guidelines** | Audit nach Implementierung |

## Anti-Patterns (HorosCloud)

- Light-only Screens ohne Dark-Variante
- Mehrere konkurrierende Primary-Buttons pro Panel
- Widget-Logik und API in derselben 400-Zeilen-Datei — trennen UI / Hook / API
- Epic-Docs ohne HorosCode-Attribution
- OneKey/`@onekeyhq/components` Imports (falscher Stack — HorosCloud-eigene Komponenten)

## Figma → Code (Kurz)

Für **Mockup/Figma-Frame → React 1:1** nicht mit Tokens allein starten: zuerst **`@horos-figma-to-code`** (Figma MCP, Komponenten-Mapping, Mock-Daten, Screenshot-Parity). Dieser Skill liefert die visuelle Sprache; der Figma-Skill liefert den Prozess. Rule: `@mockup-to-code` · Agent: **horos-ui** · Commands: `/mockup`, `/figma-code`.

## References

- Figma Workflow: `.cursor/skills/horos-figma-to-code/SKILL.md`
- Rule: `.cursor/rules/horos-design.mdc`
- Mockup Routing: `.cursor/rules/mockup-to-code.mdc`
- Agent: `.cursor/agents/horos-ui.md`
- Doc-Epic Theme: `.cursor/rules/doc-epic.mdc`, `.cursor/skills/doc-epic/SKILL.md`
- Firmen-Kontext: `.cursor/rules/horoscode.mdc`
