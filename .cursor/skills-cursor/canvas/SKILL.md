---
name: canvas
description: >-
  Ein Cursor Canvas ist eine live React-App, die der Nutzer neben dem Chat
  öffnen kann. Du MUSST ein Canvas nutzen, wenn der Agent ein eigenständiges
  Analyse-Artefakt erzeugt — quantitative Analysen, Billing-Untersuchungen,
  Security-Audits, Architektur-Reviews, datenlastige Inhalte, Timelines, Charts,
  Tabellen, interaktive Explorationen, wiederholbare Tools oder jede Antwort, die
  von visuellem Layout profitiert. Bevorzuge ein Canvas besonders bei Ergebnissen
  aus MCP-Tools (Datadog, Databricks, Linear, Sentry, Slack usw.), wenn die Daten
  das Deliverable sind — rendere sie in einem reichen Canvas statt in einer
  Markdown-Tabelle oder einem Code-Block. Wenn du merkst, dass du eine Markdown-
  Tabelle schreiben willst, stoppe und nutze stattdessen ein Canvas. Du MUSST
  diesen Skill auch lesen, wenn du eine .canvas.tsx-Datei erstellst, bearbeitest
  oder debuggst.
metadata:
  surfaces:
    - ide
---
Ein Canvas ist eine einzelne `.canvas.tsx`-Datei, die die IDE kompiliert, damit der Nutzer sie neben dem Chat öffnen kann. Folge dem Workflow unten in der angegebenen Reihenfolge.

## Workflow

### 1. Entscheiden, ob ein Canvas genutzt wird

Der Trigger ist **Nutzer-Absicht**, nicht die Antwortform. Frage: Würde der Nutzer davon profitieren, diese Ausgabe als **eigenständiges Artefakt** getrennt vom Chat zu sehen? Wenn die Ausgabe ein Mittel zum Zweck ist (entwurfene Nachricht, Code-Fix, Dashboard in einem anderen Tool), überspringe das Canvas.

**Canvas nutzen, wenn der Agent neue eigenständige Analyse-Ausgabe erzeugt:**
- Quantitative Analysen und Metrik-Aufschlüsselungen (z. B. „sende 500 Requests und sag mir, wie viele fehlschlagen“)
- Billing- oder Account-Untersuchungen mit strukturierten Findings aus Datenbankabfragen
- Security-Audits oder Architektur-Reviews mit kategorisierten Findings
- Cross-System-Datenanalysen und Overlap-Reports
- Strukturierte Daten aus MCP-Tools (Databricks, Datadog usw.), wenn die Daten DAS Deliverable sind
- Finanzanalysen, Margin-Dekompositionen, Usage-Trend-Reports
- Tabellen mit mehr als einer Handvoll Zeilen, die der Nutzer sehen wollte

**KEIN Canvas, wenn:**
- Der Nutzer Arbeit in einem **bestimmten Tool** will — „erstelle ein Datadog-Dashboard“ bedeutet ein Datadog-Dashboard, kein Canvas
- Der Nutzer ein **konkretes Deliverable** will — „Support-Antwort entwerfen“, „diesen Code fixen“, „diesen PR“
- Der Nutzer **in einem bestehenden Artefakt** arbeitet — HTML-Dashboard verbessern, bestehende Datei bearbeiten
- Der Nutzer **gezielt debuggt** oder aktiv entwickelt, auch wenn unterwegs strukturierte Findings entstehen
- Kurze Faktenantworten, einmalige Datei-Edits oder schnelle Klärungsfragen
- MCP-Tools als **Zwischenschritt** für ein anderes Deliverable abgefragt werden (z. B. Stripe abfragen, um eine Support-Antwort zu entwerfen)

### 2. Canvas schreiben

**Speicherort.** Canvases liegen unter `/Users/<user>/.cursor/projects/<workspace>/canvases/<name>.canvas.tsx`. Die IDE erkennt nur Canvases, die direkt in genau diesem Verzeichnis geschrieben wurden — Unterordner, andere Extensions und andere Orte werden nicht erkannt. Für ein neues Canvas immer das Write-File-Tool nutzen, um die `.canvas.tsx` an genau diesem Pfad zu erstellen; nicht aufhören, nachdem du dem Nutzer nur den Pfad genannt oder Code im Chat gezeigt hast. Behandle das verwaltete `canvases/`-Verzeichnis als von Cursor bereitgestellt: schreibe die Canvas-Datei direkt dorthin und **verbringe keine Turns** mit `mkdir` oder Existenz-Checks vor dem Schreiben. Auflisten für andere Zwecke (z. B. bestehende Canvases prüfen) ist ok. Wenn du das Workspace-Verzeichnis nicht aus absoluten Pfaden in deiner Umgebung (Terminals, Transcripts, kürzlich angesehene Dateien) bestimmen kannst, liste `~/.cursor/projects/` statt zu raten. Beschreibender kebab-case-Dateiname mit `.canvas.tsx`; Akronyme groß, Rest klein.

**Datei-Regeln:**
- Genau eine `.canvas.tsx`-Datei pro Canvas. Keine Helper-, Style- oder Support-Module.
- Import **nur** aus `cursor/canvas`. Keine relativen Imports, keine npm-Packages, keine Node-Built-ins.
- Top-Level-Komponente als Default-Export.
- Alle Daten inline einbetten. **Kein `fetch()`, keine Netzwerk-Calls.**

**Niemals leere States rendern.** Ein Canvas existiert, um echten Inhalt zu zeigen. Wenn ein Abschnitt, Chart, Tabelle oder eine Komponente keine Daten hat, **weglassen** — nicht mit Platzhaltertext („Add header here“, „TODO“, „Example“), „No data“-Meldung, leerem Array, Null-Zeilen oder leerem Chart-Rahmen rendern. Wenn das gesamte Canvas leer wäre, weil dir die Daten fehlen: kein Canvas — dem Nutzer sagen, was fehlt, und danach fragen.

**Jeden Plot beschriften.** Charts und Tabellen müssen sich selbst erklären — ein Leser, der nur das Canvas sieht, soll genau wissen, was er sieht. Für jeden Plot:
- Titel mit der **spezifischen Metrik** (nicht „Metrics“ — „API error rate by service“).
- **Achsenbeschriftungen mit Einheiten** auf beiden Achsen (z. B. „Date“, „Latency (ms)“).
- **Legende**, wenn mehr als eine Serie gezeigt wird, mit exakten Seriennamen aus den Quelldaten.
- **Quelle und Zeitraum** in einer kleinen Caption (z. B. „Source: Datadog · last 7 days“). Wenn ein Wert eine Transformation ist (mean, p95, normalized, smoothed), in der Beschriftung sagen.

**Komponenten-Discovery:** bevorzuge Built-in-`cursor/canvas`-Komponenten statt handgerolltem Markup. Die volle öffentliche Surface (Components, Hooks, Prop-Types, Tokens) ist in `~/.cursor/skills-cursor/canvas/sdk/index.d.ts` und den Geschwister-`.d.ts`-Dateien deklariert — lies sie für exakte Exports, Prop-Shapes oder Hook-Signaturen statt zu raten. Ein nicht existierender Export ist der häufigste Runtime-Fehler.

Wende die Canvas-Generierungs-Policy unten beim Schreiben an und schließe den Pre-Delivery-Self-Check (Abschnitt 6) ab, bevor du das Canvas zurückgibst.

## Design-Hinweise

Sei kreativ. Das SDK gibt expressive Bausteine — nutze sie in der Kombination, die dem Inhalt am besten dient. Aber kein Slop: keine Gradients, keine Emojis, keine Box-Shadows, keine Regenbogen-Farben. Cursor-Canvases sind flach, minimal und zweckgerichtet.

### Visuelle Hierarchie

Nicht alles verdient gleiche Behandlung. Primärer Inhalt bekommt mehr Platz, größere Überschriften und Akzentfarbe. Unterstützender Inhalt bleibt kompakt. Squint-Test: Augen zusammenkneifen — erkennst du, was zählt?

**Farbe.** Alle Farben aus `useHostTheme()`-Tokens — lies das JSDoc in den SDK-Deklarationen für Return-Shape und Nutzungsmuster. Kein hardcodiertes Hex. Akzentfarbe gezielt, nicht überall.

### Slop-Patterns — verboten

Diese Patterns erzeugen minderwertige Ausgabe. Bei 2+ vorhanden: neu designen.

- **Gradients** — kein `linear-gradient`, `radial-gradient`, `background-clip: text`.
- **Emojis** — kein Emoji als Icon, Status-Indikator, Bullet oder Abschnittsmarker.
- **Box shadows** — kein `box-shadow`. Nur flache Surfaces.
- **Wall of identical cards** — jeder Abschnitt im gleichen Card-Style ohne Variation. Offene Abschnitte mit Cards mischen.
- **Rainbow coloring** — jede Elemente andere Farbe. Die meisten Elemente neutral; Farbe sparsam mit Zweck.
- **Giant text** — Schriftgrößen über H1 (24px) oder fetter Text in CardHeader.
- **Decorative borders** — farbige Borders an jedem Element. Borders sind strukturell (subtile Stroke-Tokens), nicht dekorativ.

### Pre-Delivery-Self-Check

Vor Rückgabe des Canvas-Codes prüfen:
1. Hat das Layout visuelle Hierarchie? Eins sollte hervorstechen.
2. Gibt es Variation in der Komposition? Nicht nur eine Spalte gleichförmiger Blöcke.
3. Slop-Check: verbotene Patterns oben scannen.

## Canvas vorstellen

Wann immer du dem Nutzer ein Canvas erwähnst — erstellt, aktualisiert oder zum Öffnen — **immer** einen Markdown-Link zu dieser `.canvas.tsx`-Datei mit vollem absoluten Pfad einbinden (z. B. `[billing-review](/Users/<user>/.cursor/projects/<workspace>/canvases/billing-review.canvas.tsx)`). Kurzes beschreibendes Label als Link-Text; Canvas nicht nur per Name oder Pfad ohne Link nennen.

Bei neuem Canvas kurze Notiz in der Chat-Antwort, dass der Nutzer es neben dem Chat öffnen kann, mit diesem Link:

- **Erstes Canvas** — wenn keine anderen `.canvas.tsx` im `canvases/`-Verzeichnis des Workspaces existieren, ein Satz, was ein Canvas ist.
- **Unaufgefordertes Canvas** — wenn der Nutzer kein Canvas wollte, ein Satz, warum du es statt Plain Text gewählt hast.

Beides kann gleichzeitig gelten; ein oder zwei Sätze reichen. Intro bei weiteren Canvases überspringen, außer du erwähnst dieses Canvas erneut (trotzdem verlinken).

## Troubleshooting

Wenn ein Canvas leer oder fehlend wirkt, liegt es meist daran, dass es nicht exakt unter `/Users/<user>/.cursor/projects/<workspace>/canvases/` geschrieben wurde — dort erneut speichern. Nicht debuggen, indem du das verwaltete Verzeichnis manuell erstellst; auf Korrektur des Dateipfads fokussieren. Nutzer können den Canvas-Dateipfad in der Antwort anklicken wie jeden anderen Pfad in Cursor. Jede Canvas-Bearbeitung liefert eine `Canvas TypeScript check`-Zeile im Tool-Result mit den aktuellen Type-Errors der Datei (oder „no errors“) — das ist das autoritative Diagnose-Signal.
