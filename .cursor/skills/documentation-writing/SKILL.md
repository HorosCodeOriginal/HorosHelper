---
name: documentation-writing
version: 1.0.0
description: >-
  Klare, auffindbare Software-Dokumentation nach den Eight Rules und Diataxis.
  Verwenden für README, API-Docs, Tutorials, How-to-Guides oder Projekt-Doku.
  Erzwingt docs/-Ablage, Linking und ausführbare Beispiele.
source_urls:
  - https://diataxis.fr/
  - https://www.writethedocs.org/guide/writing/docs-principles/
  - https://github.blog/developer-skills/documentation-done-right-a-developers-guide/
auto_activates:
  - "write documentation"
  - "create docs"
  - "document this feature"
  - "create a README"
  - "write a tutorial"
  - "api docs"
  - "how-to guide"
token_budget: 1800
---

# Dokumentation schreiben

## Zweck

Erstellt hochwertige, auffindbare Dokumentation nach Eight Rules und Diataxis. Stellt sicher, dass Docs korrekt abgelegt, verlinkt und mit echten, ausführbaren Beispielen versehen sind.

## Wann ich aktiviert werde

Automatisch bei Erwähnung von:

- „write documentation“ oder „create docs“
- „document this feature/module/API“
- „create a README“ oder „write a tutorial“
- „explain how this works“
- Jeder Anfrage zur Erstellung von Markdown-Dokumentation

## Kernregeln (PFLICHT)

### Die Eight Rules

1. **Location**: Alle Docs im `docs/`-Verzeichnis
2. **Linking**: Jedes Doc von mindestens einem anderen Doc verlinkt
3. **Simplicity**: Klare Sprache, unnötige Wörter entfernen
4. **Real Examples**: Ausführbarer Code, keine „foo/bar“-Platzhalter
5. **Diataxis**: Ein Doc-Typ pro Datei (tutorial/howto/reference/explanation)
6. **Scanability**: Aussagekräftige Überschriften, Inhaltsverzeichnis für lange Docs
7. **Local Links**: Relative Pfade, Kontext bei Links
8. **Currency**: Veraltete Docs löschen, Update-Metadaten einfügen

### Was NICHT in Docs gehört

**Niemals in `docs/`:**

- Statusberichte oder Fortschrittsupdates
- Testergebnisse oder Benchmarks
- Meeting-Notizen oder Entscheidungen
- Pläne mit Daten
- Momentaufnahmen zu einem Zeitpunkt

**Wo zeitgebundene Infos hingehören:**

- Testergebnisse → CI-Logs, GitHub Actions
- Status-Updates → GitHub Issues
- Fortschritt → Pull-Request-Beschreibungen
- Entscheidungen → Commit-Messages

## Quick Start

### Neues Dokument erstellen

```markdown
# [Feature Name]

Brief one-sentence description of what this is.

## Quick Start

Minimal steps to get started (3-5 steps max).

## Contents

- [Configuration](#configuration)
- [Usage](#usage)
- [Troubleshooting](#troubleshooting)

## Configuration

Step-by-step setup with real examples.

## Usage

Common use cases with runnable code.

## Troubleshooting

Common problems and solutions.
```

### Dokumenttypen (Diataxis)

| Type        | Purpose       | Location          | User Question           |
| ----------- | ------------- | ----------------- | ----------------------- |
| Tutorial    | Lernen        | `docs/tutorials/` | „Bring mir bei, wie“    |
| How-To      | Umsetzen      | `docs/howto/`     | „Hilf mir, X zu tun“    |
| Reference   | Information   | `docs/reference/` | „Welche Optionen gibt es?“ |
| Explanation | Verstehen     | `docs/concepts/`  | „Warum ist es so?“      |

## Workflow

### Schritt 1: Dokumenttyp bestimmen

Frag: Was will der Leser erreichen?

- Etwas Neues lernen → Tutorial
- Ein konkretes Problem lösen → How-To
- Details nachschlagen → Reference
- Konzepte verstehen → Explanation

### Schritt 2: Ablageort wählen

```
docs/
├── tutorials/     # Learning-oriented
├── howto/         # Task-oriented
├── reference/     # Information-oriented
├── concepts/      # Understanding-oriented
└── index.md       # Links to all docs
```

### Schritt 3: Mit Beispielen schreiben

Jedes Konzept braucht ein ausführbares Beispiel:

```python
# Example: Analyze file complexity
from amplihack import analyze

result = analyze("src/main.py")
print(f"Complexity: {result.score}")
# Output: Complexity: 12.5
```

### Schritt 4: Vom Index verlinken

Eintrag in `docs/index.md` hinzufügen:

```markdown
- [New Feature Guide](./howto/new-feature.md) - How to configure X
```

### Schritt 5: Validieren

Checkliste vor Abschluss:

- [ ] Datei im `docs/`-Verzeichnis
- [ ] Vom Index oder Parent-Doc verlinkt
- [ ] Keine zeitgebundenen Informationen
- [ ] Alle Beispiele getestet
- [ ] Folgt einem Diataxis-Typ

## Navigationshilfe

### Wann du Support-Dateien lesen solltest

**reference.md** — Lesen, wenn du brauchst:

- Vollständige Frontmatter-Spezifikation
- Detaillierte Diataxis-Typdefinitionen
- Markdown-Style-Konventionen
- Dokumentations-Review-Checkliste

**examples.md** — Lesen, wenn du brauchst:

- Vollständige Dokumentvorlagen für jeden Typ
- Praxisbeispiele für Dokumentation
- Vorher/Nachher-Verbesserungsbeispiele
- Komplexe Dokumentationsmuster

## Anti-Patterns vermeiden

| Anti-Pattern       | Warum schlecht | Besserer Ansatz                |
| ------------------ | ------------- | ------------------------------ |
| „Click here“-Links | Kein Kontext  | „Siehe [auth config](./auth.md)“ |
| foo/bar-Beispiele  | Unrealistisch | Echten Projektcode nutzen      |
| Textwand           | Schwer scannbar | Überschriften und Bullets nutzen |
| Waisen-Docs        | Werden nie gefunden | Vom Index verlinken       |
| Status in Docs     | Veraltet schnell | Issues/PRs nutzen          |

## Retcon-Dokumentation (Ausnahme)

Wenn du Dokumentation VOR der Implementierung schreibst (document-driven development):

````markdown
# [PLANNED - Implementation Pending]

This document describes the intended behavior of Feature X.

## Planned Interface

```python
# [PLANNED] - This API will be implemented
def future_function(input: str) -> Result:
    """Process input and return result."""
    pass
```
````

Sobald implementiert, entferne die `[PLANNED]`-Marker und aktualisiere mit echten Beispielen.

---

## README-Struktur (HorosCode-Projekte)

Jedes Projekt-README sollte enthalten:

1. **Projektname** + Ein-Satz-Beschreibung (was und warum)
2. **Quick Start** — installieren und starten in ≤3 Schritten
3. **Installation** — Anforderungen und Setup
4. **Usage** — häufige Muster mit ausführbaren Beispielen
5. **Configuration** — Env-Vars und Optionen
6. **Development** — lokal linten, testen, bauen
7. **Contributing** + **License** (falls zutreffend)

Template: [references/readme-template.md](./references/readme-template.md)

## API-Dokumentation

Pro Endpoint oder öffentliche Funktion dokumentieren:

- **Description** — was es macht
- **Parameters** — Name, Typ, required/optional, Beschreibung
- **Return value** — Typ und Struktur
- **Errors** — Bedingungen und Codes
- **Examples** — ausführbare Nutzung

Template: [references/api-template.md](./references/api-template.md)

## Code-Kommentare

Kommentiere **warum**, nicht **was**:

```typescript
// Bad: Sets the count to zero
count = 0;

// Good: Reset count for new measurement cycle
count = 0;
```

## Konversationeller Stil (nutzerorientierte Docs)

Beim Schreiben von How-tos und Guides:

1. **Zielgruppe zuerst** — Komplexität an Leser anpassen; Aufgaben nicht „einfach“ nennen
2. **Mit Aktion starten** — was tun, dann warum
3. **Überschriften mit Aussage** — „SAML vor Nutzeranlage setzen“ statt „SAML-Konfiguration“
4. **Laut vorlesen** — klingt es zu formal, vereinfachen
5. **Beschreibende Links** — nie „hier klicken“
6. **Backticks** für Code/Variablen; **fett** für UI-Labels
7. **Füllwörter streichen** — ein klarer Zweck pro Absatz

| Schreib | Nicht |
|-------|-----|
| Schau in [die Docs](link) | Klick [hier](link) |
| **Speichern**-Button | `Save`-Button |
| kannst nicht, mach nicht | cannot, do not |

## Architektur-Dokumentation

Abdecken: Systemüberblick, Komponentenbeziehungen, Datenfluss, Designentscheidungen, Trade-offs. Mermaid für Flows; Diagramme einfach halten.

## Dokumentations-Checkliste

**README:** Beschreibung, Quick Start, Installation, Usage, Config, Contributing

**Code-Docs:** öffentliche APIs dokumentiert, Parameter/Returns, Beispiele für komplexe Funktionen

**API-Docs:** alle Endpoints, Schemas, Auth, Errors, Rate Limits

## Veraltete Skills (hier zusammengeführt)

- `docs-write` → Konversationeller-Stil-Abschnitt oben
- `documentation-engineer` → README/API-Templates in `references/`
- `crafting-effective-readme` → README-Struktur-Abschnitt oben

Für HorosCloud **Feature-Epics** (mehrere Dateien + HTML) nutze stattdessen **`@doc-epic`**.

---

**Vollständige Referenz**: [reference.md](./reference.md) · **Templates**: [examples.md](./examples.md)
