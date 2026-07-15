---
name: implementing-figma-designs
description: >-
  Setzt Figma-Designs 1:1 für HorosCloud Web-UI um — React, TypeScript, Tailwind,
  Mock-Daten first. HorosCode-Fork (kein OneKey). Orchestrator-Skill: @horos-figma-to-code.
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

# Figma-Designs umsetzen (HorosCloud)

Dieser Skill hilft dir, Figma-Designs **1:1** als **HorosCloud** Web-UI unter `HorosCloudV5/apps/web/src` umzusetzen — React + TypeScript + Tailwind, design-system-konform.

**Firma:** HorosCode · **Produkt:** HorosCloud

**Orchestrierung:** Für den vollständigen Workflow (Figma MCP, Parity, Screenshots) zuerst **`@horos-figma-to-code`** lesen. Dieser Skill ist die **Implementierungs-Referenz** (Komponenten, Tokens, Mock-Daten).

## Kernprinzipien

### Zuerst UI, Daten später

Bei Figma-Implementierungen hat **pixelgenaue UI** Vorrang vor Datenintegration:

1. **Mock-Daten nutzen** — Werte hardcoden, die exakt zum Design passen
2. **i18n überspringen** — Plain Strings direkt aus Figma, keine Übersetzungskeys
3. **API-Calls überspringen** — In Pass 1 keine echten Daten laden
4. **Design treffen** — Visuelle Genauigkeit, Abstände, Farben, Layout, States

### Was du NICHT tun solltest

- Nicht sorgen, woher Daten kommen (Pass 1)
- Keine i18n-Keys hinzufügen
- Keine API-Integration oder Fetch-Hooks in Pass 1
- Kein komplexes globales State-Management für Mock-UI
- **Keine** OneKey / `@onekeyhq/components` Imports

### Was du tun solltest

- Text exakt wie in Figma hardcoden
- Mock-Arrays/Objekte für Listen und Cards
- Auf Komponentenstruktur und Tailwind-Styling fokussieren
- Abstände, Farben und Typografie aus `@horos-design-system` / Figma übernehmen
- Bestehende HorosCloud-Komponenten wiederverwenden

## HorosCloud Projektstruktur

```text
HorosCloudV5/apps/web/src/
├── components/          # Wiederverwendbare UI
│   └── demo/            # Demo-/Referenz-Komponenten
├── pages/               # Routen-Views
├── App.tsx              # Router
├── main.tsx             # Entry
├── index.css            # Tailwind + --hc-* CSS-Variablen
└── vite-env.d.ts
```

Statische Assets: `HorosCloudV5/apps/web/public/images/`, `public/icons/`

## Komponenten-Inventar (aktueller Stand)

Vor jeder Implementierung per `Glob` / `Grep` aktualisieren — nicht blind vertrauen.

| Pfad | Export / Zweck |
|------|----------------|
| `components/demo/DashboardStatCard.tsx` | `DashboardStatCard` — KPI-Karte mit loading, disabled, trend, refresh |
| `components/demo/index.ts` | Barrel: `DashboardStatCard`, `DashboardStatCardProps`, `TrendDirection` |
| `pages/DemoPage.tsx` | Demo-Seite: Hero `<picture>`, Stat-Card-Grid, Design-Mode-Hinweise |
| `App.tsx` | Routes: `/` → `/demo` |
| `index.css` | `--hc-bg-base`, `--hc-bg-elevated`, `--hc-accent`, Tailwind-Layers |

**Pattern für neue Features:**

```text
components/<feature>/<ComponentName>.tsx
components/<feature>/index.ts          # optional barrel
pages/<FeaturePage>.tsx
```

Beispiel Settings-Widget: `components/settings/NotesCanvasWidget.tsx` + Route in `App.tsx`.

### Komponente nachschlagen

1. **Glob** nach ähnlichen Widgets:

   ```text
   Glob: HorosCloudV5/apps/web/src/components/**/*.tsx
   Grep: slate-800|amber-500|rounded-lg border
   ```

2. **Quellcode lesen** für Props, States, Tailwind-Muster:

   ```text
   Read: HorosCloudV5/apps/web/src/components/demo/DashboardStatCard.tsx
   ```

3. **Geschwister-Seite** für Layout-Kontext:

   ```text
   Read: HorosCloudV5/apps/web/src/pages/DemoPage.tsx
   ```

Referenz-Komponente `DashboardStatCard` zeigt HorosCloud-Konventionen: `article` + `aria-*`, Skeleton-Loading, 5 interaktive States, Amber-Fokus-Ring.

## Imports & Stack

HorosCloud Web nutzt **kein** externes UI-Paket wie OneKey — lokale Komponenten + Tailwind:

```tsx
import { useState, useCallback } from "react";
import { DashboardStatCard } from "../components/demo/DashboardStatCard";
// oder relativer Pfad aus feature folder
```

**Verboten:**

```tsx
import { Button, Stack } from "@onekeyhq/components"; // falscher Stack
```

## Spacing-Tokens (8px-Grid)

Aus `@horos-design-system` / `@horos-design`:

| Tailwind | px | Typisch |
|----------|-----|---------|
| `gap-1` / `p-1` | 4 | Icon-in-Button |
| `gap-2` / `p-2` | 8 | Kompakte Zeilen |
| `gap-4` / `p-4` | 16 | Card-Padding, Form-Felder |
| `gap-6` / `p-6` | 24 | Section-Padding |
| `gap-8` | 32 | Zwischen Sektionen |

Figma px → nächstes 8px-Vielfach mappen (z. B. 20px → `p-5` oder `gap-5` wenn 20px beabsichtigt).

## Farb- & Typografie-Tokens

### Dark Theme (Slate + Amber)

| Rolle | Tailwind | Hex (Referenz) |
|-------|----------|----------------|
| App background | `bg-slate-900` | `#0f172a` |
| Surface / Card | `bg-slate-800` | `#1e293b` |
| Surface raised | `bg-slate-700` | `#334155` |
| Border | `border-slate-700` | — |
| Text primary | `text-slate-100` | — |
| Text muted | `text-slate-400` | — |
| Accent / CTA | `bg-amber-500`, `text-amber-400` | — |
| Focus ring | `ring-amber-500/50` | — |

CSS-Variablen in `index.css`: `--hc-bg-base`, `--hc-bg-elevated`, `--hc-bg-muted`, `--hc-accent`.

### Typografie

| Element | Klassen |
|---------|---------|
| Page title | `text-2xl font-semibold text-slate-100` |
| Section title | `text-lg font-medium text-slate-100` |
| Body | `text-sm text-slate-300` |
| Caption | `text-xs text-slate-400` |
| Code / ID | `font-mono text-sm text-amber-300/90` |

## Häufige Patterns

### Layout

```tsx
<main className="min-h-screen bg-slate-900 px-6 py-10">
  <div className="mx-auto max-w-4xl space-y-8">
    <section className="grid gap-4 sm:grid-cols-2">{/* cards */}</section>
  </div>
</main>
```

### Widget Card

```tsx
<article className="rounded-lg border border-slate-700 bg-slate-800 p-4">
  {/* header → body → footer */}
</article>
```

### Primary Button

```tsx
<button
  type="button"
  className="rounded-md bg-amber-500 px-4 py-2 text-sm font-medium text-slate-900 hover:bg-amber-600 focus:outline-none focus:ring-2 focus:ring-amber-500/50 disabled:opacity-50"
>
  Speichern
</button>
```

### Mock-Daten

```tsx
const mockBackups = [
  { id: "1", name: "MacBook Pro", files: "1.284", trend: "+12,4 %" },
  { id: "2", name: "iPhone 15", files: "892", trend: "Stabil" },
];
```

### Bild aus public/

```tsx
<img
  src="/images/hero-example.webp"
  alt="Beschreibung"
  className="aspect-video w-full object-cover"
  width={1280}
  height={853}
/>
```

Raster generieren/einbinden: **Zuko** + `@horos-image-embed` — nicht in diesem Skill.

## Interaktions-States (Pflicht)

Jedes interaktive Element: Default, Hover, Focus, Active, Disabled.

Loading: Skeleton (`animate-pulse bg-slate-700`) — siehe `DashboardStatCard`.

## Workflow

1. **`@horos-figma-to-code`** — Figma MCP, Routing, Parity-Plan
2. Figma-Frame analysieren (MCP oder Export)
3. Benötigte UI-Typen identifizieren → `@ui-design-brain` bei Bedarf
4. **Glob/Grep** in `HorosCloudV5/apps/web/src` — bestehende Komponenten mappen
5. Mock-Daten passend zum Design erstellen
6. UI mit Tailwind + `@horos-design-system` Tokens implementieren
7. Lint/Build; Screenshot-Parity (`@screenshot-capture`)

## Verweise

| Thema | Pfad |
|-------|------|
| End-to-End Figma Workflow | `.cursor/skills/horos-figma-to-code/SKILL.md` |
| Design System | `.cursor/skills/horos-design-system/SKILL.md` |
| Design Rule | `.cursor/rules/horos-design.mdc` |
| Mockup Routing | `.cursor/rules/mockup-to-code.mdc` |
| horos-ui Agent | `.cursor/agents/horos-ui.md` |
| Komponenten-Patterns | `.cursor/skills/ui-design-brain/components.md` |
