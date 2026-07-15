---
name: create-skill
description: >-
  Erstellt Cursor Agent Skills. Verwenden beim Authoring eines neuen Skills oder
  bei Fragen zur SKILL.md-Struktur.
---
# Skills in Cursor erstellen

Dieser Skill führt dich durch effektive Agent Skills für Cursor. Skills sind Markdown-Dateien, die dem Agenten spezifische Tasks beibringen: PR-Reviews nach Team-Standards, Commit-Messages im Wunschformat, Datenbankschemas abfragen oder spezialisierte Workflows.

## Bevor du beginnst: Anforderungen sammeln

Vor dem Erstellen eines Skills vom Nutzer klären:

1. **Zweck und Scope**: Welcher Task oder Workflow?
2. **Ziel-Speicherort**: Persönlicher Skill (~/.cursor/skills/) oder Projekt-Skill (.cursor/skills/)?
3. **Trigger-Szenarien**: Wann soll der Agent den Skill automatisch anwenden?
4. **Domänenwissen**: Welche Spezialinfos braucht der Agent?
5. **Output-Format**: Templates, Formate, Stile?
6. **Bestehende Patterns**: Beispiele oder Konventionen?

### Wörtlicher Text vom Nutzer

Enthält der Nutzer exakte Formulierungen für den Skill, **wörtlich** in `SKILL.md` übernehmen (gleiche Wörter, gleiche Reihenfolge). Nicht paraphrasieren, weichzeichnen oder ungefragt Kommentare/Überschriften drumherum.

### Aus Kontext ableiten

Bei Gesprächskontext Skill aus Besprochenem ableiten — Workflows, Patterns, Domänenwissen aus der Konversation.

### Zusätzliche Informationen sammeln

Bei Klärungsbedarf das AskQuestion-Tool nutzen, wenn verfügbar:

```
Beispiel AskQuestion:
- „Wo soll dieser Skill gespeichert werden?“ mit Optionen wie [„Persönlich (~/.cursor/skills/)“, „Projekt (.cursor/skills/)“]
- „Soll dieser Skill ausführbare Skripte enthalten?“ mit Optionen wie [„Ja“, „Nein“]
```

Ist AskQuestion nicht verfügbar, diese Fragen im Gespräch stellen.

---

## Skill-Dateistruktur

### Verzeichnis-Layout

Skills liegen als Verzeichnisse mit einer `SKILL.md`-Datei:

```
skill-name/
├── SKILL.md              # Pflicht — Hauptanleitung
├── reference.md          # Optional — ausführliche Doku
├── examples.md           # Optional — Nutzungsbeispiele
└── scripts/              # Optional — Utility-Skripte
    ├── validate.py
    └── helper.sh
```

### Speicherorte

| Typ | Pfad | Scope |
|------|------|-------|
| Persönlich | ~/.cursor/skills/skill-name/ | In allen deinen Projekten |
| Projekt | .cursor/skills/skill-name/ | Mit allen Repo-Nutzern geteilt |

**WICHTIG:** Skills niemals in `~/.cursor/skills-cursor/` anlegen. Dieses Verzeichnis ist für Cursors interne Built-in-Skills reserviert und wird vom System verwaltet.

### SKILL.md-Struktur

Jeder Skill braucht `SKILL.md` mit YAML-Frontmatter und Markdown-Body:

```markdown
---
name: your-skill-name
description: Brief description of what this skill does and when to use it
disable-model-invocation: true
---

# Your Skill Name

## Instructions
Clear, step-by-step guidance for the agent.

## Examples
Concrete examples of using this skill.
```

Default `disable-model-invocation: true`, damit der Skill nur bei expliziter Nennung lädt. Weglassen nur, wenn der Agent automatisch aus Kontext invoken soll.

### Pflicht-Metadaten-Felder

| Feld | Anforderungen | Zweck |
|-------|--------------|---------|
| `name` | Max. 64 Zeichen, nur Kleinbuchstaben/Zahlen/Bindestriche | Eindeutiger Skill-Identifier |
| `description` | Max. 1024 Zeichen, nicht leer | Hilft dem Agenten zu entscheiden, wann der Skill gilt |

---

## Effektive Descriptions schreiben

Die Description ist **kritisch** für Skill-Discovery. Der Agent nutzt sie, um zu entscheiden, wann dein Skill angewendet wird.

### Best Practices für Descriptions

1. **In der dritten Person schreiben** (Description wird ins System-Prompt injiziert):
   - ✅ Gut: „Verarbeitet Excel-Dateien und erzeugt Reports“
   - ❌ Vermeiden: „Ich kann dir helfen, Excel zu verarbeiten“
   - ❌ Vermeiden: „Du kannst das nutzen, um Excel zu verarbeiten“

2. **Spezifisch sein und Trigger-Begriffe einbauen**:
   - ✅ Gut: „Extrahiert Text und Tabellen aus PDFs, füllt Formulare, merged Dokumente. Nutzen bei PDF-Arbeit oder wenn der Nutzer PDFs, Formulare oder Dokument-Extraktion erwähnt.“
   - ❌ Vage: „Hilft bei Dokumenten“

3. **Sowohl WAS als auch WANN**:
   - WAS: Was der Skill tut (konkrete Fähigkeiten)
   - WANN: Wann der Agent ihn nutzen soll (Trigger-Szenarien)

### Description-Beispiele

```yaml
# PDF Processing
description: Extract text and tables from PDF files, fill forms, merge documents. Use when working with PDF files or when the user mentions PDFs, forms, or document extraction.

# Excel Analysis
description: Analyze Excel spreadsheets, create pivot tables, generate charts. Use when analyzing Excel files, spreadsheets, tabular data, or .xlsx files.

# Git Commit Helper
description: Generate descriptive commit messages by analyzing git diffs. Use when the user asks for help writing commit messages or reviewing staged changes.

# Code Review
description: Review code for quality, security, and best practices following team standards. Use when reviewing pull requests, code changes, or when the user asks for a code review.
```

---

## Kern-Authoring-Prinzipien

### 1. Kürze zählt

Das Kontextfenster teilst du mit Conversation History, anderen Skills und Requests. Jedes Token konkurriert um Platz.

**Default-Annahme**: Der Agent ist schon sehr schlau. Nur Kontext hinzufügen, den er nicht hat.

Jede Information hinterfragen:
- „Braucht der Agent diese Erklärung wirklich?“
- „Kann ich annehmen, dass der Agent das weiß?“
- „Rechtfertigt dieser Absatz seine Token-Kosten?“

**Gut (knappe)**:
```markdown
## Extract PDF text

Use pdfplumber for text extraction:

\`\`\`python
import pdfplumber

with pdfplumber.open("file.pdf") as pdf:
    text = pdf.pages[0].extract_text()
\`\`\`
```

**Schlecht (verbose)**:
```markdown
## Extract PDF text

PDF (Portable Document Format) files are a common file format that contains
text, images, and other content. To extract text from a PDF, you'll need to
use a library. There are many libraries available for PDF processing, but we
recommend pdfplumber because it's easy to use and handles most cases well...
```

### 2. SKILL.md unter 500 Zeilen halten

Für optimale Performance sollte die Haupt-SKILL.md knapp sein. Progressive Disclosure für Details.

### 3. Progressive Disclosure

Wesentliches in SKILL.md; ausführliche Referenz in separate Dateien, die der Agent nur bei Bedarf liest.

```markdown
# PDF Processing

## Quick start
[Essential instructions here]

## Additional resources
- For complete API details, see [reference.md](reference.md)
- For usage examples, see [examples.md](examples.md)
```

**Referenzen nur eine Ebene tief** — direkt von SKILL.md zu Referenzdateien verlinken. Tief verschachtelte Referenzen können zu partiellen Reads führen.

### 4. Passende Freiheitsgrade setzen

Spezifität an Fragilität der Aufgabe anpassen:

| Freiheitsgrad | Wann | Beispiel |
|---------------|-------------|---------|
| **Hoch** (Text-Anweisungen) | Mehrere valide Ansätze, kontextabhängig | Code-Review-Guidelines |
| **Mittel** (Pseudocode/Templates) | Bevorzugtes Pattern mit akzeptabler Variation | Report-Generierung |
| **Niedrig** (spezifische Skripte) | Fragile Ops, Konsistenz kritisch | Datenbank-Migrationen |

---

## Häufige Patterns

### Template-Pattern

Output-Format-Templates bereitstellen:

```markdown
## Report structure

Nutze dieses Template:

\`\`\`markdown
# [Analysis Title]

## Executive summary
[One-paragraph overview of key findings]

## Key findings
- Finding 1 with supporting data
- Finding 2 with supporting data

## Recommendations
1. Specific actionable recommendation
2. Specific actionable recommendation
\`\`\`
```

### Examples-Pattern

Bei Skills, deren Output-Qualität von Beispielen abhängt:

```markdown
## Commit message format

**Example 1:**
Input: Added user authentication with JWT tokens
Output:
\`\`\`
feat(auth): implement JWT-based authentication

Add login endpoint and token validation middleware
\`\`\`

**Example 2:**
Input: Fixed bug where dates displayed incorrectly
Output:
\`\`\`
fix(reports): correct date formatting in timezone conversion

Use UTC timestamps consistently across report generation
\`\`\`
```

### Workflow-Pattern

Komplexe Ops in klare Schritte mit Checklisten zerlegen:

```markdown
## Form filling workflow

Copy this checklist and track progress:

\`\`\`
Task Progress:
- [ ] Step 1: Analyze the form
- [ ] Step 2: Create field mapping
- [ ] Step 3: Validate mapping
- [ ] Step 4: Fill the form
- [ ] Step 5: Verify output
\`\`\`

**Step 1: Analyze the form**
Run: \`python scripts/analyze_form.py input.pdf\`
...
```

### Conditional-Workflow-Pattern

Durch Entscheidungspunkte führen:

```markdown
## Document modification workflow

1. Determine the modification type:

   **Creating new content?** → Follow "Creation workflow" below
   **Editing existing content?** → Follow "Editing workflow" below

2. Creation workflow:
   - Use docx-js library
   - Build document from scratch
   ...
```

### Feedback-Loop-Pattern

Bei qualitätskritischen Tasks Validierungs-Loops implementieren:

```markdown
## Document editing process

1. Make your edits
2. **Validate immediately**: \`python scripts/validate.py output/\`
3. If validation fails:
   - Review the error message
   - Fix the issues
   - Run validation again
4. **Only proceed when validation passes**
```

---

## Utility-Skripte

Vorgefertigte Skripte haben Vorteile gegenüber generiertem Code:
- Zuverlässiger als generierter Code
- Spart Tokens (kein Code im Kontext)
- Spart Zeit (keine Code-Generierung)
- Konsistenz über Nutzungen hinweg

```markdown
## Utility scripts

**analyze_form.py**: Extract all form fields from PDF
\`\`\`bash
python scripts/analyze_form.py input.pdf > fields.json
\`\`\`

**validate.py**: Check for errors
\`\`\`bash
python scripts/validate.py fields.json
# Returns: "OK" or lists conflicts
\`\`\`
```

Klarstellen, ob der Agent das Skript **ausführen** (häufig) oder als Referenz **lesen** soll.

---

## Anti-Patterns vermeiden

### 1. Windows-Pfade
- ✅ Use: `scripts/helper.py`
- ❌ Avoid: `scripts\helper.py`

### 2. Zu viele Optionen
```markdown
# Bad - confusing
"You can use pypdf, or pdfplumber, or PyMuPDF, or..."

# Good - provide a default with escape hatch
"Use pdfplumber for text extraction.
For scanned PDFs requiring OCR, use pdf2image with pytesseract instead."
```

### 3. Zeitabhängige Informationen
```markdown
# Bad - will become outdated
"If you're doing this before August 2025, use the old API."

# Good - use an "old patterns" section
## Current method
Use the v2 API endpoint.

## Old patterns (deprecated)
<details>
<summary>Legacy v1 API</summary>
...
</details>
```

### 4. Inkonsistente Terminologie
Einen Begriff wählen und durchhalten:
- ✅ Always "API endpoint" (not mixing "URL", "route", "path")
- ✅ Always "field" (not mixing "box", "element", "control")

### 5. Vage Skill-Namen
- ✅ Good: `processing-pdfs`, `analyzing-spreadsheets`
- ❌ Avoid: `helper`, `utils`, `tools`

---

## Skill-Erstellungs-Workflow

Beim Helfen beim Skill-Erstellen diesen Prozess folgen:

### Phase 1: Discovery

Informationen sammeln zu:
1. Zweck und primärer Use Case des Skills
2. Speicherort (persönlich vs. Projekt)
3. Trigger-Szenarien
4. Spezifische Anforderungen oder Constraints
5. Bestehende Beispiele oder Patterns

Bei AskQuestion-Zugang für strukturiertes Sammeln nutzen. Sonst im Gespräch fragen.

### Phase 2: Design

1. Skill-Namen entwerfen (lowercase, Bindestriche, max. 64 Zeichen)
2. Spezifische Description in dritter Person schreiben
3. Benötigte Hauptsektionen skizzieren
4. Prüfen, ob Support-Dateien oder Skripte nötig sind

### Phase 3: Implementation

1. Verzeichnisstruktur anlegen
2. SKILL.md mit Frontmatter schreiben
3. Support-Referenzdateien anlegen
4. Utility-Skripte bei Bedarf anlegen

### Phase 4: Verification

1. Prüfen, dass SKILL.md unter 500 Zeilen ist
2. Description spezifisch mit Trigger-Begriffen prüfen
3. Konsistente Terminologie durchhalten
4. Alle Dateireferenzen nur eine Ebene tief
5. Testen, dass der Skill discoverable und anwendbar ist

---

## Vollständiges Beispiel

Ein vollständiges Beispiel eines gut strukturierten Skills:

**Verzeichnisstruktur:**
```
code-review/
├── SKILL.md
├── STANDARDS.md
└── examples.md
```

**SKILL.md:**
```markdown
---
name: code-review
description: Review code for quality, security, and maintainability following team standards. Use when reviewing pull requests, examining code changes, or when the user asks for a code review.
---

# Code Review

## Quick Start

When reviewing code:

1. Check for correctness and potential bugs
2. Verify security best practices
3. Assess code readability and maintainability
4. Ensure tests are adequate

## Review Checklist

- [ ] Logic is correct and handles edge cases
- [ ] No security vulnerabilities (SQL injection, XSS, etc.)
- [ ] Code follows project style conventions
- [ ] Functions are appropriately sized and focused
- [ ] Error handling is comprehensive
- [ ] Tests cover the changes

## Providing Feedback

Format feedback as:
- 🔴 **Critical**: Must fix before merge
- 🟡 **Suggestion**: Consider improving
- 🟢 **Nice to have**: Optional enhancement

## Additional Resources

- For detailed coding standards, see [STANDARDS.md](STANDARDS.md)
- For example reviews, see [examples.md](examples.md)
```

---

## Zusammenfassungs-Checkliste

Vor Finalisierung eines Skills prüfen:

### Kern-Qualität
- [ ] Description spezifisch mit Key Terms
- [ ] Description enthält WAS und WANN
- [ ] In dritter Person geschrieben
- [ ] SKILL.md-Body unter 500 Zeilen
- [ ] Konsistente Terminologie
- [ ] Beispiele konkret, nicht abstrakt

### Structure
- [ ] Dateireferenzen eine Ebene tief
- [ ] Progressive Disclosure passend genutzt
- [ ] Workflows mit klaren Schritten
- [ ] Keine zeitabhängigen Infos

### Bei Skripten
- [ ] Skripte lösen Probleme statt abzuschieben
- [ ] Benötigte Packages dokumentiert
- [ ] Error Handling explizit und hilfreich
- [ ] Keine Windows-Pfade
