---
name: horos-figma-to-code
description: >-
  HorosCloud Figma Frame → React/TSX 1:1 — Figma MCP, Design-System-Mapping,
  Mock-Daten, Parity-Verify. Verwenden wenn ein Figma-Mockup als HorosCloud
  Web-UI nachgebaut werden soll (nicht Raster/Icons — dafür Zuko).
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "1.0.0"
---

# Horos Figma → Code (HorosCloud 1:1)

End-to-End-Skill für **HorosCode** / **HorosCloud**: Figma-Frame als produktionsnahe React-UI unter `HorosCloudV5/apps/web/src` — pixelnah, design-system-konform, mit Mock-Daten und Screenshot-Parity.

**Agent:** `horos-ui` · **Rule:** `@mockup-to-code` · **Design-Tokens:** `@horos-design-system`

## Wann nutzen

| Situation | Dieser Skill | Stattdessen |
|-----------|--------------|-------------|
| Figma-Frame / Figma-URL → HorosCloud Web-UI (React/TSX) | **Ja** | — |
| Settings-Widget, Dashboard, Formular 1:1 aus Mockup | **Ja** | — |
| „Mockup nachcoden“, „Figma to code“, „1:1 UI“ | **Ja** → **horos-ui** | nicht Zuko |
| Nur Icon, Hero-Raster, Marketing-Still | Nein | **Zuko** + `@horos-image-embed` |
| Mobile / Expo / `apps/mobile` | Nein | **Suki** |
| Generische SaaS-UI ohne HorosCloud | Nein | `@frontend-builder` + `@ui-design-brain` |

## Voraussetzungen

| Voraussetzung | Pfad / Aktion |
|---------------|---------------|
| **Figma MCP** | `.cursor/mcp.json` → `figma` Server (`https://mcp.figma.com/mcp`); in Cursor authentifizieren |
| **Design System** | `@horos-design-system` + Rule `@horos-design` |
| **Agent** | **horos-ui** für Implementierung (Leaf Worker) |
| **Dev-Server** | `HorosCloudV5/apps/web` — typisch `http://localhost:5173` |
| **Figma-Input** | Frame-Link, Node-ID oder Figma MCP Selection |

Ohne Figma-Zugang: **Screenshot-to-Code nur als Notlösung** (siehe unten) — Qualität ~70–85 %.

## Skill-Kette (Reihenfolge)

1. **`@horos-figma-to-code`** (dieser Skill) — Workflow & Parity
2. **`@horos-design-system`** — Slate + Amber Tokens, Spacing, States
3. **`@implementing-figma-designs`** — HorosCloud-Komponenten-Inventar, Mock-Daten-Patterns
4. **`@ui-design-brain`** — Komponententyp wählen (Modal vs. Drawer, Table-Layout)
5. **`@wide-ultra-logical`** — bei `/wul`: vollständige Lieferung + Selbst-Verifikation
6. **`@screenshot-capture`** + **`screenshot-verify.mdc`** — visuelle Parity nach Implementierung

Raster-Assets im Frame (Icons, Hero): parallel **Zuko** + `@horos-image-embed` — nicht Layout in PNG nachbauen.

## Schritt-für-Schritt Workflow

### 1. Frame laden (Figma MCP)

1. Figma MCP in Cursor prüfen (`GetMcpTools` / MCP-Panel).
2. Frame analysieren: Layout-Hierarchie, Auto-Layout, Spacing, Typography, Farben, Komponenten-Instanzen.
3. Notieren: Breakpoints, Zustände (default/hover/disabled), leere/loading/error Varianten.

**Figma MCP typische Schritte:**

- Frame-URL oder Node-ID vom User
- MCP: Design-Kontext, Variablen, Export-Hinweise abrufen
- Abstände in px → auf 8px-Grid mappen (`gap-2` = 8px, `gap-4` = 16px, …)

### 2. Bestehende HorosCloud-Komponenten mappen

Vor neuen Primitives:

```text
Glob: HorosCloudV5/apps/web/src/**/*.tsx
Grep: ähnliche Widget-Namen, Tailwind-Muster (slate-800, amber-500)
```

Aktuelles Inventar (Stand Template — bei jedem Task per Glob aktualisieren):

| Pfad | Zweck |
|------|-------|
| `components/demo/DashboardStatCard.tsx` | Dashboard-Karte: loading, disabled, trend, refresh |
| `components/demo/index.ts` | Barrel-Export Demo-Komponenten |
| `pages/DemoPage.tsx` | Demo-Layout, Hero `<picture>`, Design-Mode-Ziel |
| `App.tsx` | React-Router-Routen |
| `index.css` | CSS-Variablen `--hc-*`, Tailwind-Basis |

Neue UI: Feature-Ordner spiegeln — z. B. `components/settings/`, `pages/SettingsPage.tsx`.

### 3. Mock-Daten first (UI vor API)

**Erster Pass — nur UI:**

- Text und Zahlen **exakt wie im Figma-Frame** hardcoden
- Mock-Arrays/Objekte für Listen, Tabellen, Cards
- **Kein** i18n, **keine** API-Calls, **kein** React Query in Pass 1
- `onClick` → `console.log` oder lokaler UI-State

Details: `@implementing-figma-designs` → „Zuerst UI, Daten später“.

### 4. Implementieren (horos-ui / Executor)

- Stack: **React + TypeScript + Tailwind** (Vite unter `HorosCloudV5/apps/web`)
- Tokens aus `@horos-design-system`: `bg-slate-900`, `bg-slate-800`, `text-slate-100`, `amber-500` CTAs
- Semantisches HTML: `main`, `section`, `button`, `label` + `htmlFor`
- Alle **5 Pflicht-States** für interaktive Elemente (siehe Parity-Checkliste)
- Keine OneKey/`@onekeyhq/components` Imports

Komponenten-Pattern wählen: `@ui-design-brain` → `components.md`.

### 5. `/wul` Verify (Build + Lint)

Mit **Wide Ultra-Logical** aktiv:

1. `ReadLints` auf allen geänderten `.tsx`/`.css`
2. Build/Lint im Web-Paket (z. B. `npm run build` / `npm run lint` in `HorosCloudV5/apps/web`)
3. Fehler fixen — kein „TODO später“

### 6. Screenshot-Parity (Capture + Verify)

1. Dev-Server starten; Route der neuen UI öffnen
2. **`@screenshot-capture`**: Full-Content bei scrollbarem UI (nicht nur Viewport)
3. Jede PNG mit **`Read`** prüfen (`screenshot-verify.mdc`)
4. Side-by-side mit Figma: Spacing, Typo, Farben, Alignment
5. Abweichungen fixen → erneut capturen bis Parity stimmt

## Parity-Checkliste

Vor „fertig“ alle Punkte abhaken:

### Layout & Spacing

- [ ] 8px-Grid: Abstände als Tailwind (`p-4`, `gap-6`, …) — keine willkürlichen px-Werte
- [ ] Auto-Layout-Richtung (row/column) und Alignment gematcht
- [ ] Max-Width / Grid-Spalten wie im Frame
- [ ] Responsive Breakpoints sinnvoll (sm/md/lg)

### Tokens (Slate + Amber)

- [ ] Hintergrund: `slate-900` / `#0f172a`
- [ ] Surfaces: `slate-800`, `slate-700`
- [ ] Text: `slate-100`, `slate-400` muted
- [ ] Akzent: `amber-400`–`amber-500` (ein dominanter CTA-Farbton)
- [ ] CSS-Variablen aus `index.css` bevorzugt (`--hc-bg-base`, …)

### 5 States (interaktiv)

- [ ] Default
- [ ] Hover
- [ ] Focus (`ring-2 ring-amber-500/50`)
- [ ] Active
- [ ] Disabled (`opacity-50`, `cursor-not-allowed`)

Optional je Kontext: Loading (Skeleton), Empty, Error.

### Barrierefreiheit

- [ ] WCAG-AA-Kontrast
- [ ] Touch-Ziele ≥ 44×44px
- [ ] `aria-*` wo nötig; sichtbare Labels
- [ ] Tastatur: Tab-Reihenfolge, Escape bei Modals

### Inhalt

- [ ] Copy stimmt mit Figma (Pass 1: hardcoded OK)
- [ ] Icons/Raster: `@horos-image-embed` unter `public/icons/` oder `public/images/`

## Screenshot-to-Code — nur Notlösung

Wenn **kein** Figma MCP / kein Frame-Link:

| Methode | Erwartete Parity | Empfehlung |
|---------|------------------|------------|
| Figma MCP + Design System | ~95–100 % | **Standard** |
| Figma Export + manuelles Messen | ~85–95 % | OK mit Disziplin |
| Screenshot → „baue das nach“ | ~70–85 % | **Notlösung** — immer Screenshot-Verify + manuelle Parity-Liste |

Bei Screenshot-Fallback: trotzdem `@horos-design-system` Tokens erzwingen; nicht generische AI-Layouts.

## Pfade (HorosCloud Web)

| Bereich | Pfad |
|---------|------|
| App-Quellcode | `HorosCloudV5/apps/web/src/` |
| Komponenten | `HorosCloudV5/apps/web/src/components/` |
| Seiten | `HorosCloudV5/apps/web/src/pages/` |
| Globale Styles | `HorosCloudV5/apps/web/src/index.css` |
| Statische Assets | `HorosCloudV5/apps/web/public/images/`, `public/icons/` |
| Feature-Docs | `HorosCloudV5/docs/features/<slug>/` |
| Cursor Meta | `.cursor/skills/horos-figma-to-code/`, `.cursor/rules/mockup-to-code.mdc` |

## Cursor-Commands

| Command | Zweck |
|---------|-------|
| `/mockup` | Figma/Mockup → Code via **horos-ui** + diese Skill-Kette |
| `/figma-code` | Alias — gleicher Workflow |
| `/image` | Nur Raster — **Zuko**, nicht Layout-Code |

## Verweise

| Thema | Datei |
|-------|-------|
| Design Tokens & Widgets | `.cursor/skills/horos-design-system/SKILL.md` |
| Figma-Implementierung (HorosCloud-Fork) | `.cursor/skills/implementing-figma-designs/SKILL.md` |
| Komponenten-Patterns | `.cursor/skills/ui-design-brain/SKILL.md` |
| horos-ui Agent | `.cursor/agents/horos-ui.md` |
| Mockup-Routing Rule | `.cursor/rules/mockup-to-code.mdc` |
| Design Rule (Globs) | `.cursor/rules/horos-design.mdc` |
| Wide Ultra-Logical | `.cursor/skills/wide-ultra-logical/SKILL.md` |
| Screenshot Capture | `.cursor/skills/screenshot-capture/SKILL.md` |
| Raster-Pipeline | `.cursor/skills/horos-image-embed/SKILL.md` |
| Figma MCP Config | `.cursor/mcp.json` |
| Firmen-Kontext | `.cursor/rules/horoscode.mdc` |
