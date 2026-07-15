---
name: create-an-asset
description: >-
  Erstellt maßgeschneiderte Sales-Assets (Landing Pages, Decks, One-Pager,
  Workflow-Demos) aus Deal-Kontext. Prospect, Zielgruppe und Ziel beschreiben —
  poliertes, gebrandetes Asset zum Teilen mit Kunden.
---

# Asset erstellen

Erzeuge individuelle Sales-Assets für Prospect, Zielgruppe und Ziele. Unterstützt interaktive Landing Pages, Präsentations-Decks, Executive One-Pager und Workflow-/Architektur-Demos.

---

## Auslöser

Diesen Skill nutzen, wenn:
- Der Nutzer `/create-an-asset` oder `/create-an-asset [Firmenname]` sagt
- Der Nutzer „Asset erstellen“, „Demo bauen“, „Landing Page machen“, „Workflow mocken“ fragt
- Der Nutzer ein kundenorientiertes Deliverable für ein Sales-Gespräch braucht

---

## Überblick

Dieser Skill erstellt professionelle Sales-Assets, indem er Kontext sammelt zu:
- **(a) Prospect** — Firma, Kontakte, Gespräche, Pain Points
- **(b) Zielgruppe** — wer schaut, was interessiert sie
- **(c) Zweck** — Ziel des Assets, gewünschte nächste Aktion
- **(d) Format** — Landing Page, Deck, One-Pager oder Workflow-Demo

Der Skill recherchiert, strukturiert und baut ein poliertes, gebrandetes Asset zum Teilen mit Kunden.

---

## Phase 0: Kontext-Erkennung & Input-Sammlung

### Schritt 0.1: Seller-Kontext erkennen

Aus der E-Mail-Domain des Nutzers die Firma ableiten.

**Aktionen:**
1. Domain aus Nutzer-E-Mail extrahieren
2. Suchen: `"[domain]" company products services site:linkedin.com OR site:crunchbase.com`
3. Seller-Kontext bestimmen:

| Szenario | Aktion |
|----------|--------|
| **Single-Product-Firma** | Seller-Kontext auto-befüllen |
| **Multi-Product-Firma** | Fragen: „Für welches Produkt oder welche Lösung ist dieses Asset?“ |
| **Consultant/Agentur/generische Domain** | Fragen: „Welche Firma oder welches Produkt vertrittst du?“ |
| **Unbekannt/Startup** | Fragen: „Kurz: Was verkaufst du?“ |

**Seller-Kontext speichern:**
```yaml
seller:
  company: "[Company Name]"
  product: "[Product/Service]"
  value_props:
    - "[Key value prop 1]"
    - "[Key value prop 2]"
    - "[Key value prop 3]"
  differentiators:
    - "[Differentiator 1]"
    - "[Differentiator 2]"
  pricing_model: "[If publicly known]"
```

**In Knowledge Base persistieren** für spätere Sessions. Bei erneutem Aufruf bestätigen: „Ich habe deinen Seller-Kontext vom letzten Mal — verkaufst du noch [Product] bei [Company]?“

---

### Schritt 0.2: Prospect-Kontext sammeln (a)

**Den Nutzer fragen:**

| Feld | Frage | Pflicht |
|-------|--------|----------|
| **Firma** | „Für welche Firma ist dieses Asset?“ | ✓ Ja |
| **Key Contacts** | „Wer sind die wichtigsten Kontakte? (Namen, Rollen)“ | Nein |
| **Deal-Stage** | „In welcher Phase ist der Deal?“ | ✓ Ja |
| **Pain Points** | „Welche Pain Points oder Prioritäten haben sie geteilt?“ | Nein |
| **Vergangene Materialien** | „Lade Gesprächsmaterial hoch (Transkripte, E-Mails, Notizen, Aufnahmen)“ | Nein |

**Deal-Stage-Optionen:**
- Intro / Erstes Meeting
- Discovery
- Evaluation / Technisches Review
- POC / Pilot
- Verhandlung
- Abschluss

---

### Schritt 0.3: Zielgruppen-Kontext sammeln (b)

**Den Nutzer fragen:**

| Feld | Frage | Pflicht |
|-------|--------|----------|
| **Zielgruppen-Typ** | „Wer schaut sich das an?“ | ✓ Ja |
| **Konkrete Rollen** | „Bestimmte Titel zum Tailoring? (z. B. CTO, VP Engineering, CFO)“ | Nein |
| **Hauptanliegen** | „Was interessiert sie am meisten?“ | ✓ Ja |
| **Einwände** | „Bedenken oder Einwände, die du adressieren willst?“ | Nein |

**Zielgruppen-Typ-Optionen:**
- Executive (C-Suite, VPs)
- Technisch (Architects, Engineers, Developers)
- Operations (Ops, IT, Procurement)
- Gemischt / Cross-functional

**Hauptanliegen-Optionen:**
- ROI / Business Impact
- Technische Tiefe / Architektur
- Strategische Ausrichtung
- Risikominimierung / Security
- Implementierung / Timeline

---

### Schritt 0.4: Zweck-Kontext sammeln (c)

**Den Nutzer fragen:**

| Feld | Frage | Pflicht |
|-------|--------|----------|
| **Ziel** | „Was ist das Ziel dieses Assets?“ | ✓ Ja |
| **Gewünschte Aktion** | „Was soll der Betrachter danach tun?“ | ✓ Ja |

**Ziel-Optionen:**
- Intro / Erster Eindruck
- Discovery-Follow-up
- Technischer Deep-Dive
- Executive Alignment / Business Case
- POC-Vorschlag
- Deal-Abschluss

---

### Schritt 0.5: Format wählen (d)

**Den Nutzer fragen:** „Welches Format passt am besten?“

| Format | Beschreibung | Am besten für |
|--------|-------------|----------|
| **Interaktive Landing Page** | Multi-Tab mit Demos, Metriken, Rechnern | Exec Alignment, Intros, Value Prop |
| **Deck-Style** | Lineare Slides, präsentationsbereit | Formelle Meetings, große Audiences |
| **One-Pager** | Ein-Scroll Executive Summary | Leave-behinds, kurze Zusammenfassungen |
| **Workflow / Architektur-Demo** | Interaktives Diagramm mit animiertem Flow | Technische Deep-Dives, POC-Demos, Integrationen |

---

### Schritt 0.6: Format-spezifische Inputs

#### Bei „Workflow / Architektur-Demo“:

**Zuerst aus der Beschreibung des Nutzers parsen.** Suche nach:
- Genannten Systemen und Komponenten
- Beschriebenen Datenflüssen
- Menschlichen Interaktionspunkten
- Beispiel-Szenarien

**Dann Lücken fragen:**

| Wenn fehlend… | Fragen… |
|---------------|--------|
| Komponenten unklar | „Welche Systeme oder Komponenten sind beteiligt? (Datenbanken, APIs, KI, Middleware usw.)“ |
| Flow unklar | „Beschreib mir den Schritt-für-Schritt-Flow“ |
| Human Touchpoints unklar | „Wo interagiert ein Mensch im Workflow?“ |
| Szenario vage | „Was ist ein konkretes Demo-Beispiel-Szenario?“ |
| Integration-Details | „Bestimmte Tools oder Plattformen hervorheben?“ |

---

## Phase 1: Recherche (adaptiv)

### Kontext-Reichtum einschätzen

| Level | Indikatoren | Recherche-Tiefe |
|-------|------------|----------------|
| **Reich** | Transkripte hochgeladen, detaillierte Pain Points, klare Anforderungen | Leicht — nur Lücken füllen |
| **Moderat** | Etwas Kontext, keine Transkripte | Mittel — Firma + Branche |
| **Spärlich** | Nur Firmenname | Tief — voller Recherche-Durchlauf |

### Immer recherchieren:

1. **Prospect-Basics**
   - Suche: `"[Company]" annual report investor presentation 2025 2026`
   - Suche: `"[Company]" CEO strategy priorities 2025 2026`
   - Extrahieren: Umsatz, Mitarbeiter, Key Metrics, strategische Prioritäten

2. **Führung**
   - Suche: `"[Company]" CEO CTO CIO 2025`
   - Extrahieren: Namen, Titel, aktuelle Zitate zu Strategie/Technologie

3. **Markenfarben**
   - Suche: `"[Company]" brand guidelines`
   - Oder von Firmen-Website extrahieren
   - Speichern: Primärfarbe, Sekundärfarbe, Akzent

### Bei moderatem/spärlichem Kontext zusätzlich recherchieren:

4. **Branchenkontext**
   - Suche: `"[Industry]" trends challenges 2025 2026`
   - Extrahieren: Typische Pain Points, Marktdynamik

5. **Technologie-Landschaft**
   - Suche: `"[Company]" technology stack tools platforms`
   - Extrahieren: Aktuelle Lösungen, potenzielle Integrationspunkte

6. **Wettbewerbskontext**
   - Suche: `"[Company]" vs [seller's competitors]`
   - Extrahieren: Aktuelle Lösungen, Wechsel-Signale

### Bei hochgeladenen Transkripten/Materialien:

7. **Gesprächsanalyse**
   - Extrahieren: Genannte Pain Points, Entscheidungskriterien, Einwände, Timeline
   - Identifizieren: Key Quotes (exakte Sprache nutzen)
   - Notieren: Spezifische Terminologie, Akronyme, interne Projektnamen

---

## Phase 2: Struktur-Entscheidung

### Interaktive Landing Page

| Zweck | Empfohlene Sektionen |
|---------|---------------------|
| **Intro** | Company Fit → Solution Overview → Key Use Cases → Why Us → Next Steps |
| **Discovery-Follow-up** | Their Priorities → How We Help → Relevant Examples → ROI Framework → Next Steps |
| **Technischer Deep-Dive** | Architecture → Security & Compliance → Integration → Performance → Support |
| **Exec Alignment** | Strategic Fit → Business Impact → ROI Calculator → Risk Mitigation → Partnership |
| **POC-Vorschlag** | Scope → Success Criteria → Timeline → Team → Investment → Next Steps |
| **Deal-Abschluss** | Value Summary → Pricing → Implementation Plan → Terms → Sign-off |

**Zielgruppen-Anpassungen:**
- **Executive**: Mit Business Impact, ROI, strategischer Ausrichtung starten
- **Technisch**: Mit Architektur, Security, Integrationstiefe starten
- **Operations**: Mit Workflow-Impact, Change Management, Support starten
- **Gemischt**: Strategisch + taktisch balancieren; Tabs für unterschiedliche Tiefen

---

### Deck-Style

Gleiche Sektionen wie Landing Page, als lineare Slides:

```
1. Titelfolie (Prospect + Seller Logos, Partnership-Framing)
2. Agenda
3-N. Eine Sektion pro Slide (oder 2–3 Slides bei dichten Sektionen)
N+1. Summary / Key Takeaways
N+2. Next Steps / CTA
N+3. Appendix (optional — detaillierte Specs, Pricing usw.)
```

**Slide-Prinzipien:**
- Eine Kernbotschaft pro Slide
- Visuell > textlastig
- Metrics und Sprache des Prospects nutzen
- Speaker Notes einbinden

---

### One-Pager

Auf Ein-Scroll-Format verdichten:

```
┌─────────────────────────────────────┐
│ HERO: "[Prospect Goal] with [Product]" │
├─────────────────────────────────────┤
│ KEY POINT 1     │ KEY POINT 2     │ KEY POINT 3     │
│ [Icon + 2-3     │ [Icon + 2-3     │ [Icon + 2-3     │
│  sentences]     │  sentences]     │  sentences]     │
├─────────────────────────────────────┤
│ PROOF POINT: [Metric, quote, or case study] │
├─────────────────────────────────────┤
│ CTA: [Clear next action] │ [Contact info] │
└─────────────────────────────────────┘
```

---

### Workflow / Architektur-Demo

**Struktur nach Komplexität:**

| Komplexität | Komponenten | Struktur |
|------------|------------|-----------|
| **Einfach** | 3–5 | Single-View-Diagramm mit Schritt-Annotationen |
| **Mittel** | 5–10 | Zoombarer Canvas mit Schritt-für-Schritt-Walkthrough |
| **Komplex** | 10+ | Multi-Layer-View (Overview → Detail) mit Guided Tour |

**Standard-Elemente:**

1. **Title Bar**: `[Scenario Name] — Powered by [Seller Product]`
2. **Component Nodes**: Visuelle Boxen/Icons pro System
3. **Flow Arrows**: Animierte Verbindungen für Datenbewegung
4. **Step Panel**: Sidebar erklärt aktuellen Schritt in Plain Language
5. **Controls**: Play / Pause / Step Forward / Step Back / Reset
6. **Annotations**: Callouts für Key Decision Points und Value-Adds
7. **Data Preview**: Sample Payloads oder Transformationen pro Schritt

---

## Phase 3: Content-Generierung

### Allgemeine Prinzipien

Alle Inhalte sollten:
- **Spezifische Pain Points** aus Nutzer-Input oder Transkripten referenzieren
- **Sprache des Prospects** nutzen — ihre Terminologie, ihre Prioritäten
- **Seller-Produkt** → **Prospect-Bedürfnisse** explizit mappen
- **Proof Points** wo verfügbar (Case Studies, Metrics, Quotes)
- **Maßgeschneidert wirken**, nicht templated

---

### Sektions-Templates

#### Hero / Intro
```
Headline: "[Prospect's Goal] with [Seller's Product]"
Subhead: An ihre genannte Priorität oder Top-Branchen-Herausforderung knüpfen
Metrics: 3–4 Key Facts zum Prospect (zeigt, dass du recherchiert hast)
```

#### Their Priorities (bei Discovery-Follow-up)
```
Spezifische Pain Points aus dem Gespräch referenzieren:
- Wo möglich ihre exakten Worte nutzen
- Zeigen, dass du zugehört und verstanden hast
- Jedes mit deiner Hilfe verbinden
```

#### Solution Mapping
```
Pro Pain Point:
├── Die Herausforderung (in ihren Worten)
├── Wie [Product] sie adressiert
├── Proof Point oder Beispiel
└── Outcome / Benefit
```

#### Use Cases / Demos
```
3–5 relevante Use Cases:
├── Visuelles Mockup oder interaktive Demo
├── Business Impact (quantifiziert wenn möglich)
├── „How it works“ — 3–4 Schritte Zusammenfassung
└── Relevant für ihre Branche/Rolle
```

#### ROI / Business Case
```
Interaktiver Rechner mit:
├── Inputs relevant für ihr Business (aus Recherche)
│   ├── Anzahl User/Developers
│   ├── Aktuelle Kosten oder Zeitaufwand
│   └── Erwartete Verbesserung %
├── Outputs:
│   ├── Jährlicher Wert / Einsparungen
│   ├── Kosten der Lösung
│   ├── Net ROI
│   └── Payback Period
└── Annahmen klar (editierbar)
```

#### Why Us / Differentiators
```
├── Differentiatoren vs. Alternativen, die sie erwägen
├── Trust, Security, Compliance-Positionierung
├── Support- und Partnership-Modell
└── Kunden-Proof Points (Logos, Quotes, Case Studies)
```

#### Next Steps / CTA
```
├── Klare Aktion passend zum Zweck (c)
├── Konkreter nächster Schritt (nicht vages „let's chat“)
├── Kontaktinformationen
├── Vorgeschlagene Timeline
└── Was nach der Aktion passiert
```

---

### Workflow-Demo-Inhalt

#### Komponenten-Definitionen

Pro System definieren:

```yaml
component:
  id: "snowflake"
  label: "Snowflake Data Warehouse"
  type: "database"  # database | api | ai | middleware | human | document | output
  icon: "database"
  description: "Financial performance data"
  brand_color: "#29B5E8"
```

**Komponenten-Typen:**
- `human` — Person, die initiiert oder empfängt
- `document` — PDFs, Verträge, Dateien
- `ai` — KI/ML-Modelle, Agents
- `database` — Data Stores, Warehouses
- `api` — APIs, Services
- `middleware` — Integrationsplattformen, MCP-Server
- `output` — Dashboards, Reports, Notifications

#### Flow-Schritte

Pro Schritt definieren:

```yaml
step:
  number: 1
  from: "human"
  to: "claude"
  action: "Initiates performance review"
  description: "Sarah, a Brand Analyst at [Prospect], kicks off the quarterly review..."
  data_example: "Review request: Nike brand, Q4 2025"
  duration: "~1 second"
  value_note: "No manual data gathering required"
```

#### Szenario-Narrativ

Klaren, spezifischen Walkthrough schreiben:

```
Step 1: Human Trigger
"Sarah, a Brand Performance Analyst at Centric Brands, needs to review
Q4 performance for the Nike license agreement. She opens the review
dashboard and clicks 'Start Review'..."

Step 2: Contract Analysis
"Claude retrieves the Nike contract PDF and extracts the performance
obligations: minimum $50M revenue, 12% margin requirement, quarterly
reporting deadline..."

Step 3: Data Query
"Claude formulates a query and sends it to Workato DataGenie:
'Get Q4 2025 revenue and gross margin for Nike brand from Snowflake'..."

Step 4: Results & Synthesis
"Snowflake returns the data. Claude compares actuals vs. obligations:
Revenue $52.3M ✓ (exceeded by $2.3M)
Margin 11.2% ⚠️ (0.8% below threshold)..."

Step 5: Insight Delivery
"Claude synthesizes findings into an executive summary with
recommendations: 'Review promotional spend allocation to improve
margin performance...'"
```

---

## Phase 4: Visuelles Design

### Farbsystem

```css
:root {
    /* === Prospect Brand (Primary) === */
    --brand-primary: #[extracted from research];
    --brand-secondary: #[extracted];
    --brand-primary-rgb: [r, g, b]; /* For rgba() usage */

    /* === Dark Theme Base === */
    --bg-primary: #0a0d14;
    --bg-elevated: #0f131c;
    --bg-surface: #161b28;
    --bg-hover: #1e2536;

    /* === Text === */
    --text-primary: #ffffff;
    --text-secondary: rgba(255, 255, 255, 0.7);
    --text-muted: rgba(255, 255, 255, 0.5);

    /* === Accent === */
    --accent: var(--brand-primary);
    --accent-hover: var(--brand-secondary);
    --accent-glow: rgba(var(--brand-primary-rgb), 0.3);

    /* === Status === */
    --success: #10b981;
    --warning: #f59e0b;
    --error: #ef4444;
}
```

### Typography

```css
/* Primary: Clean, professional sans-serif */
font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;

/* Headings */
h1: 2.5rem, font-weight: 700
h2: 1.75rem, font-weight: 600
h3: 1.25rem, font-weight: 600

/* Body */
body: 1rem, font-weight: 400, line-height: 1.6

/* Captions/Labels */
small: 0.875rem, font-weight: 500
```

### Visuelle Elemente

**Cards:**
- Hintergrund: `var(--bg-surface)`
- Border: 1px solid rgba(255,255,255,0.1)
- Border-radius: 12px
- Box-shadow: dezent, geschichtet
- Hover: leichte Elevation, Border-Glow

**Buttons:**
- Primary: `var(--accent)` Hintergrund, weiße Schrift
- Secondary: transparent, Accent-Border
- Hover: Helligkeit erhöhen, dezente Skalierung

**Animationen:**
- Transitions: 200–300ms ease
- Tab-Wechsel: Fade + Slide
- Hover-States: smooth, nicht ruckartig
- Loading: dezenter Pulse oder Skeleton

### Workflow-Demo-spezifisch

**Component Nodes:**
```css
.node {
    background: var(--bg-surface);
    border: 2px solid var(--brand-primary);
    border-radius: 12px;
    padding: 16px;
    min-width: 140px;
}

.node.active {
    box-shadow: 0 0 20px var(--accent-glow);
    border-color: var(--accent);
}

.node.human {
    border-color: #f59e0b; /* Warm color for humans */
}

.node.ai {
    background: linear-gradient(135deg, var(--bg-surface), var(--bg-elevated));
    border-color: var(--accent);
}
```

**Flow Arrows:**
```css
.arrow {
    stroke: var(--text-muted);
    stroke-width: 2;
    fill: none;
    marker-end: url(#arrowhead);
}

.arrow.active {
    stroke: var(--accent);
    stroke-dasharray: 8 4;
    animation: flowDash 1s linear infinite;
}
```

**Canvas:**
```css
.canvas {
    background:
        radial-gradient(circle at center, var(--bg-elevated) 0%, var(--bg-primary) 100%),
        url("data:image/svg+xml,..."); /* Subtle grid pattern */
    overflow: auto;
}
```

---

## Phase 5: Klärungsfragen (PFLICHT)

**Vor dem Bau jedes Assets immer Klärungsfragen stellen.** Das sichert Alignment und verhindert verschwendete Arbeit.

### Schritt 5.1: Verständnis zusammenfassen

Zuerst dem Nutzer zeigen, was du verstanden hast:

```
"So plane ich das Asset:

**Asset**: [Format] für [Prospect Company]
**Zielgruppe**: [Audience type] — konkret [roles if known]
**Ziel**: [Purpose] → hin zu [desired action]
**Key Themes**: [2-3 Hauptpunkte]

[Bei Workflow-Demos zusätzlich:]
**Komponenten**: [Liste der Systeme]
**Flow**: [Step 1] → [Step 2] → [Step 3] → ...
```

### Schritt 5.2: Standardfragen (ALLE Formate)

| Frage | Warum |
|----------|-----|
| „Passt das zu deiner Vision?“ | Verständnis bestätigen |
| „Was ist DIE eine Sache, die dieses Asset treffen muss?“ | Fokus auf Priorität |
| „Ton? (Bold & confident / Consultative / Technical & precise)“ | Stil-Alignment |
| „Fokussiert und knapp oder umfassend?“ | Scope-Kalibrierung |

### Schritt 5.3: Format-spezifische Fragen

#### Interaktive Landing Page:
- „Welche Sektionen zählen für diese Zielgruppe am meisten?“
- „Bestimmte Demos oder Use Cases hervorheben?“
- „ROI-Rechner einbauen?“
- „Wettbewerbs-Positionierung adressieren?“

#### Deck-Style:
- „Wie lang ist die Präsentation? (hilft bei Slide-Anzahl)“
- „Live präsentieren oder Leave-behind?“
- „Bestimmter Flow oder Narrative Arc?“

#### One-Pager:
- „Was ist die eine wichtigste Botschaft?“
- „Bestimmter Proof Point oder Stat?“
- „Gedruckt oder digital?“

#### Workflow / Architektur-Demo:
- „Komponenten bestätigen: [list]. Fehlt etwas?“
- „So habe ich den Flow verstanden: [steps]. Stimmt das?“
- „Realistische Sample-Daten oder abstrakt?“
- „Integrations-Details hervorheben oder zurücknehmen?“
- „Klick durch Schritte oder Auto-Play?“

### Schritt 5.4: Bestätigen und starten

Nach Nutzer-Antwort:

```
"Alles klar. Ich habe, was ich brauche. Baue jetzt dein [format]..."
```

Oder bei Unklarheit:

```
"Noch eine kurze Frage: [specific follow-up]"
```

**Max. 2 Frage-Runden.** Bleibt es vage, vernünftige Wahl treffen und notieren: „Ich habe X gewählt — leicht anpassbar, wenn du Y bevorzugst.“

---

## Phase 6: Bauen & Ausliefern

### Asset bauen

Alle Specs oben befolgen:
1. Struktur aus Phase 2
2. Content aus Phase 3
3. Visuelles Design aus Phase 4
4. Alle interaktiven Elemente funktionsfähig
5. Responsiveness testen (falls zutreffend)

### Output-Format

**Alle Formate**: Self-contained HTML
- CSS inline oder in `<style>`
- JS inline oder in `<script>`
- Keine externen Dependencies (außer Google Fonts)
- Einzeldatei zum einfachen Teilen

**Dateinamen**: `[ProspectName]-[format]-[date].html`
- Beispiel: `CentricBrands-workflow-demo-2026-01-28.html`

### Delivery-Nachricht

```markdown
## ✓ Asset Created: [Prospect Name]

[View your asset](computer:///path/to/file.html)

---

**Summary**
- **Format**: [Interactive Page / Deck / One-Pager / Workflow Demo]
- **Audience**: [Type and roles]
- **Purpose**: [Goal] → [Desired action]
- **Sections/Steps**: [Count and list]

---

**Deployment-Optionen**

Zum Teilen mit deinem Kunden:
- **Static Hosting**: Upload zu Netlify, Vercel, GitHub Pages, AWS S3 oder jedem Static Host
- **Passwortschutz**: Die meisten Hosts bieten das (z. B. Netlify Site Protection)
- **Direkt teilen**: HTML-Datei direkt senden — vollständig self-contained
- **Embed**: Datei kann per iframe in andere Pages eingebunden werden

---

**Anpassung**

Sag Bescheid, wenn du möchtest:
- Farben oder Styling anpassen
- Sektionen hinzufügen, entfernen oder umsortieren
- Messaging oder Copy verfeinern
- Flow oder Architektur ändern (bei Workflow-Demos)
- Mehr interaktive Elemente
- Export als PDF oder statische Bilder
```

---

## Phase 7: Iterations-Support

Nach Auslieferung bereit für Iteration:

| Nutzer-Wunsch | Aktion |
|--------------|--------|
| „Farben ändern“ | Mit neuer Palette regenerieren, Content behalten |
| „Sektion zu X hinzufügen“ | Neue Sektion einfügen, Flow beibehalten |
| „Kürzer machen“ | Verdichten, Key Points priorisieren |
| „Flow stimmt nicht“ | Architektur nach Korrektur neu bauen |
| „Unsere Marke nutzen“ | Von Prospect- zu Seller-Brand wechseln |
| „Mehr Detail bei Schritt 3“ | Diese Sektion gezielt erweitern |
| „Als PDF?“ | Druck-optimierte Version liefern |

**Merken**: Default Prospect-Markenfarben; Seller kann nach erstem Build auf eigene Marke oder neutrale Palette wechseln.

---

## Qualitäts-Checkliste

Vor Auslieferung prüfen:

### Content
- [ ] Prospect-Firmenname durchgängig korrekt
- [ ] Führungsnamen aktuell (nicht veraltet)
- [ ] Pain Points spiegeln Input/Transkripte
- [ ] Seller-Produkt korrekt dargestellt
- [ ] Kein Platzhaltertext übrig
- [ ] Proof Points korrekt und belegt

### Visuell
- [ ] Markenfarben korrekt
- [ ] Text lesbar (Kontrast)
- [ ] Animationen smooth, nicht ablenkend
- [ ] Mobile responsive (bei interaktiver Page)
- [ ] Dark Theme poliert

### Funktional
- [ ] Alle Tabs/Sektionen laden
- [ ] Interaktive Elemente funktionieren (Rechner, Demos)
- [ ] Workflow-Schritte animieren (falls zutreffend)
- [ ] Navigation intuitiv
- [ ] CTA klar und klickbar

### Professionell
- [ ] Ton passt zur Zielgruppe
- [ ] Detailtiefe passend zum Zweck
- [ ] Keine Tipp- oder Grammatikfehler
- [ ] Wirkt maßgeschneidert, nicht templated

---

## Beispiele

### Beispiel 1: Executive Landing Page

**Input:**
- Prospect: Acme Corp (Manufacturing)
- Zielgruppe: C-Suite
- Zweck: Exec Alignment nach Discovery
- Format: Interaktive Landing Page

**Output-Struktur:**
```
[Tabs]
Strategic Fit | Business Impact | ROI Calculator | Security & Trust | Next Steps

[Strategic Fit tab]
- Acmes genannte Prioritäten (aus Discovery Call)
- Wie [Product] passt
- Relevante Manufacturing-Kunden
```

### Beispiel 2: Technische Workflow-Demo

**Input:**
- Prospect: Centric Brands
- Zielgruppe: IT Architects
- Zweck: POC-Vorschlag
- Format: Workflow-Demo
- Komponenten: Claude, Workato DataGenie, Snowflake, PDF-Verträge

**Output-Struktur:**
```
[Interactive canvas with 5 nodes]
Human → Claude → PDF Contracts → Workato → Snowflake
         ↓
    [Results back to Human]

[Step-by-step walkthrough with sample data]
[Controls: Play | Pause | Step | Reset]
```

### Beispiel 3: Sales One-Pager

**Input:**
- Prospect: TechStart Inc
- Zielgruppe: VP Engineering
- Zweck: Leave-behind nach erstem Meeting
- Format: One-Pager

**Output-Struktur:**
```
Hero: "Accelerate TechStart's Product Velocity"
Point 1: [Dev productivity]
Point 2: [Code quality]
Point 3: [Time to market]
Proof: "Similar companies saw 40% faster releases"
CTA: "Schedule technical deep-dive"
```

---

## Anhang: Komponenten-Icons

Für Workflow-Demos diese Icon-Mappings:

| Typ | Icon | Beispiel |
|------|------|---------|
| human | 👤 oder Person-SVG | User, Analyst, Admin |
| document | 📄 oder File-SVG | PDF, Contract, Report |
| ai | 🤖 oder Brain-SVG | Claude, AI Agent |
| database | 🗄️ oder Cylinder-SVG | Snowflake, Postgres |
| api | 🔌 oder Plug-SVG | REST API, GraphQL |
| middleware | ⚡ oder Hub-SVG | Workato, MCP Server |
| output | 📊 oder Screen-SVG | Dashboard, Report |

---

## Anhang: Markenfarben-Fallbacks

Wenn Markenfarben nicht extrahiert werden können:

| Branche | Primary | Secondary |
|----------|---------|-----------|
| Technology | #2563eb | #7c3aed |
| Finance | #0f172a | #3b82f6 |
| Healthcare | #0891b2 | #06b6d4 |
| Manufacturing | #ea580c | #f97316 |
| Retail | #db2777 | #ec4899 |
| Energy | #16a34a | #22c55e |
| Default | #3b82f6 | #8b5cf6 |

---

*Skill für generalisierte Sales-Asset-Generierung. Funktioniert für jeden Seller, jedes Produkt, jeden Prospect.*
