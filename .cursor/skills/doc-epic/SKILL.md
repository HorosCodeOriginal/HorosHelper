---
name: doc-epic
version: 1.0.0
description: >-
  Epic Feature Documentation — mehrteilige Markdown-Suite + HTML-Index pro Funktion.
  Aktivieren bei /doc-epic, /feature-docs, /doku-episch, /doc:function.
source_urls:
  - https://diataxis.fr/
auto_activates:
  - "/doc-epic"
  - "/feature-docs"
  - "/doku-episch"
  - "/doc:function"
  - "epic documentation"
  - "feature docs suite"
token_budget: 2400
---

# Epic Feature Documentation (`/doc-epic`)

## Zweck

Dokumentiert **eine zusammenhängende Funktion** als Suite aus mehreren Markdown-Dateien plus einer standalone `index.html` zur Epic-Visualisierung. Für Iroh und jeden Agent, der mit `/doc-epic` beauftragt wird.

## Wann aktiv

- Nutzer sagt `/doc-epic`, `/feature-docs`, `/doku-episch`, `/doc:function`
- Nutzer verweist auf `@doc-epic` oder die Regel `.cursor/rules/doc-epic.mdc`
- Orchestrator weist explizit Epic-Dokumentation zu

## Workflow (Pflichtreihenfolge)

### 1. Scope & Slug festlegen

- UI-Pfad in Worten notieren (z. B. Einstellungen → Widgets → Notizen → Canvas-Stil).
- Slug: Segmente mit `-` verbinden → `settings-widgets-notizen-bereich-canvas-design`.
- Zielordner: `HorosCloudV5/docs/features/<slug>/`.

### 2. Code lesen (vor dem Schreiben)

Suche und lies die relevanten Dateien:

| Was | Typische Pfade |
|-----|----------------|
| Settings-UI | `apps/web/src/features/settings/*SettingsPage.tsx` |
| Feature-Komponenten | `apps/web/src/features/<bereich>/` |
| Store / State | `apps/web/src/stores/` |
| Utils / Typen | `**/*Utils.ts`, `**/types.ts` |
| Styles | `**/*.css` neben dem Feature |
| Tests | `**/*.test.ts` für Verhalten & Edge Cases |
| Bestehende Widget-Docs | `docs/widgets/*.md` als Stilreferenz |

**Nicht** mit Platzhalter-Text starten. Wenn Code fehlt, im Doc benennen — nicht erfinden.

### 3. Markdown-Suite anlegen

Jede Datei hat **einen** Diataxis-Typ:

| Datei | Diataxis | Inhalt |
|-------|----------|--------|
| `README.md` | Hub | Kurzbeschreibung, Inhaltsverzeichnis, Quick Links |
| `01-uebersicht.md` | Explanation | Was, warum, Abgrenzung zu verwandten Features |
| `02-benutzer-anleitung.md` | How-to | Schritt-für-Schritt für Endnutzer |
| `03-einstellungen.md` | Reference | Routen, Labels, Defaults, Persistenz-Keys |
| `04-architektur.md` | Explanation | Komponentenbaum, Stores, Events, CSS-Klassen |
| `05-api-datenmodell.md` | Reference | Interfaces, Export/Import, Sync-Felder |
| `06-faq-troubleshooting.md` | How-to | Symptom → Ursache → Lösung |
| `07-changelog-feature.md` | optional | Nur wenn Feature-Historie relevant |

**Sprache:** Deutsch. **Code:** echte Pfade in Backticks.

### 4. `index.html` erstellen

Standalone, kein Build:

- Dunkles HorosCloud-Theme (`#0f172a` Hintergrund, Amber-Akzente).
- Fixe Sidebar mit Ankern zu Sektionen + Links zu `.md`-Dateien.
- Inline-CSS; optional kleines JS nur für Sidebar-Toggle/Mermaid.
- **Mermaid** per CDN (`https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js`) für Architektur-/Datenfluss-Diagramme.
- **Screenshot-Platzhalter** mit Pfad-Hinweis, z. B. `docs/screenshots/features/<slug>/01-canvas-classic.png`.

### 5. Index verlinken

In `HorosCloudV5/docs/README.md` unter **Epic Features** eintragen:

```markdown
| [settings-widgets-notizen-bereich-canvas-design](features/settings-widgets-notizen-bereich-canvas-design/README.md) | Canvas-Stil & Bereichs-Design (Notizen) · [HTML](features/.../index.html) |
```

Erwähne den Parameter `/doc-epic` im README-Abschnitt.

### 6. Qualität prüfen

- [ ] Alle MD-Dateien gelesen und kohärent verlinkt
- [ ] Kein erfundenes API-Verhalten
- [ ] `index.html` im Browser öffnbar (relative Links)
- [ ] Von `docs/README.md` erreichbar
- [ ] Bestehende Docs nicht unnötig dupliziert — stattdessen verlinken (z. B. `docs/widgets/QUICK-NOTES-WIDGET.md`)

## Slug-Beispiele

```
settings/widgets/notizen/bereich-canvas-design
  → settings-widgets-notizen-bereich-canvas-design

settings/widgets/youtube/playlist-editor
  → settings-widgets-youtube-playlist-editor

dashboard/widgets/quick-notes/collapse
  → dashboard-widgets-quick-notes-collapse
```

## Anti-Patterns

| Vermeiden | Stattdessen |
|-----------|-------------|
| Eine 2000-Zeilen-Datei | Suite mit klarer Rolle pro Datei |
| „TODO: später ausfüllen“ | Blocker benennen oder weglassen |
| Englische Nutzer-UI-Labels | Deutsche Labels wie in der App |
| Screenshots erfinden | Platzhalter mit Zielpfad unter `docs/screenshots/` |

## Referenzen

- Regel: `.cursor/rules/doc-epic.mdc`
- Parameter: `.cursor/rules/agent-context-modes.mdc` → Epic Feature Documentation
- Stil-Vorbild Widget-Doc: `HorosCloudV5/docs/widgets/QUICK-NOTES-WIDGET.md`
- Documentation-Writing-Skill: `.cursor/skills/documentation-writing/SKILL.md`
