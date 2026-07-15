---
name: avalonia-figma-to-code
description: >-
  HorosCloud Figma Frame / Screenshot → Avalonia AXAML 1:1 — Figma MCP, Design-System-Mapping,
  Mock-Daten, Preview-Viewer, Screenshot-Review 98/100. Verwenden wenn ein Mockup als
  HorosCloud Desktop-UI (C#/Avalonia) nachgebaut werden soll (nicht Web horos-ui, nicht Raster Zuko).
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "1.0.0"
---

# Avalonia Figma → Code (HorosCloud Desktop 1:1)

End-to-End-Skill für **HorosCode** / **HorosCloud**: Figma-Frame oder Screenshot als produktionsnahe Avalonia-UI — pixelnah, token-konform, incremental mit Preview und 98/100 Review.

**Agent:** `avalonia-ui` · **Rules:** `00-agent`–`12-human-approval` · **Tokens:** `@avalonia-design-system`

## Wann nutzen

| Situation | Dieser Skill | Stattdessen |
|-----------|--------------|-------------|
| Figma-Frame → HorosCloud Desktop (AXAML) | **Ja** | — |
| Screenshot → Avalonia 1:1 | **Ja** (Notlösung) | — |
| „Mockup nachcoden" Desktop | **Ja** → **avalonia-ui** | nicht horos-ui |
| HorosCloud Web React/TSX | Nein | **horos-ui** + `@horos-figma-to-code` |
| Nur Icon / Hero-Raster | Nein | **Zuko** |
| Mobile / `apps/mobile` | Nein | **Suki** |

## Voraussetzungen

| Voraussetzung | Pfad / Aktion |
|---------------|---------------|
| **Figma MCP** | `.cursor/mcp.json` → Figma; authentifizieren |
| **Design Memory** | `docs/design-memory.md` |
| **Agent** | **avalonia-ui** |
| **Desktop-Projekt** | `*.csproj` mit Avalonia — oder neu anlegen |
| **Input** | Frame-Link, Node-ID, Figma Selection oder Screenshot |

## Skill-Kette (Reihenfolge)

1. **`@avalonia-figma-to-code`** (dieser Skill) — Workflow & Parity
2. **`@avalonia-design-system`** — Slate + Amber Tokens → ResourceDictionary
3. **`@implementing-figma-designs-avalonia`** — Inventar, Mock-Daten, Preview
4. **Review** — `docs/review-checklist.md` (Score ≥ 98)
5. Optional **`@wide-ultra-logical`** bei `/wul`

## Schritt-für-Schritt Workflow

### 1. Frame / Screenshot laden

**Figma MCP (bevorzugt, ~95–100 % Start-Parity):**

1. MCP prüfen (`GetMcpTools`)
2. Hierarchie, Auto-Layout, Spacing, Typography, Farben extrahieren
3. Zustände notieren: default, hover, disabled, focus

**Screenshot-Fallback (~70–85 %):**

1. User-Screenshot + Viewport-Größe
2. Bereiche manuell markieren
3. Mehr Korrektur-Runden einplanen

### 2. Bereiche zerlegen (incremental)

Rule `01-workflow`, `03-mockup-analysis`:

- Header, Sidebar, Content, … — **ein Bereich pro Task**
- Pro Bereich: `*Preview.axaml` planen

### 3. Tokens extrahieren

Figma-Werte → `docs/design-memory.md` → `Styles/DesignTokens.axaml`

Keine inline Hex-Werte in Views (Rules `02-pixel-perfect`, `10-anti-patterns`).

### 4. Bestehende Komponenten mappen

```text
Glob: **/*.axaml
Grep: HorosButton, HorosCard, BrushSurface, Spacing
```

Wiederverwenden vor neuen Primitives.

### 5. Implementieren (aktueller Bereich nur)

- AXAML Layout — Mockup-strukturgetreu (`02-pixel-perfect`)
- ViewModel mit Mock-Daten
- `Views/Previews/{Bereich}Preview.axaml`
- Styles in ResourceDictionary — nicht global erfinden (`10-anti-patterns`)

### 6. Preview & Screenshot

- Isolierter Preview starten
- PNG: `review-{bereich}-v1.png`
- Viewport = Mockup-Größe

### 7. Review & Korrektur

`docs/review-checklist.md` — Bewertung 100 Punkte, Gate **≥ 98**

Diff-Liste → fixen → Re-Screenshot → bis Gate erfüllt.

### 8. Human Approval & Nächster Bereich

Rule `12-human-approval` — User-Freigabe abwarten.

Erst nach Approval: nächsten Bereich oder Shell-Zusammenführung.

## Bewertungsmodell (Kurz)

| Kategorie | Punkte |
|-----------|--------|
| Spacing | 25 |
| Typography | 20 |
| Colors | 20 |
| Layout | 15 |
| Components | 10 |
| Accessibility | 10 |

Details: `docs/parameters.md`

## Modell

- **avalonia-ui** mit `composer-2.5`
- Kein Modell-Wechsel mid-Bereich

## Parity-Erwartung

| Quelle | Typisch |
|--------|---------|
| Figma MCP | ~95–100 % → Ziel 98 nach Review |
| Screenshot | ~70–85 % → Korrektur Pflicht |

## Verboten

- Design verbessern / modernisieren
- Ganze App in einem Pass
- Erfolg ohne Screenshot + Score
- Web Tailwind-Patterns in AXAML

## Verweise

- Agent: `.cursor/agents/avalonia-ui.md`
- Parameter: `docs/parameters.md`
- Design Memory: `docs/design-memory.md`
- Master: `AGENTS.md`
