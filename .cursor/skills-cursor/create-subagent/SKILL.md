---
name: create-subagent
description: >-
  Erstellt Custom Subagents für spezialisierte KI-Aufgaben. Verwenden beim
  Erstellen eines neuen Subagent-Typs, task-spezifischer Agents, Code-Reviewer,
  Debugger oder domänenspezifischer Assistenten mit Custom Prompts.
disable-model-invocation: true
---
# Custom Subagents erstellen

Dieser Skill führt dich durch das Erstellen von Custom Subagents für Cursor. Subagents sind spezialisierte KI-Assistenten in isolierten Kontexten mit eigenen System-Prompts.

## Wann Subagents nutzen

Subagents helfen dir:
- **Kontext zu bewahren**, indem Exploration von der Hauptkonversation isoliert wird
- **Verhalten zu spezialisieren** mit fokussierten System-Prompts für bestimmte Domänen
- **Konfigurationen wiederzuverwenden** über User-Level-Subagents in Projekten

### Aus Kontext ableiten

Bei vorherigem Gesprächskontext Zweck und Verhalten des Subagents aus dem Besprochenen ableiten. Subagent aus spezialisierten Tasks oder Workflows erstellen, die im Gespräch entstanden sind.

## Subagent-Speicherorte

| Speicherort | Scope | Priorität |
|----------|-------|----------|
| `.cursor/agents/` | Aktuelles Projekt | Höher |
| `~/.cursor/agents/` | Alle deine Projekte | Niedriger |

Bei gleichem Namen gewinnt der höher priorisierte Speicherort.

**Projekt-Subagents** (`.cursor/agents/`): ideal für codebase-spezifische Agents. Ins Version Control checken, um mit dem Team zu teilen.

**User-Subagents** (`~/.cursor/agents/`): persönliche Agents in allen Projekten.

## Subagent-Dateiformat

`.md`-Datei mit YAML-Frontmatter und Markdown-Body (System-Prompt):

```markdown
---
name: code-reviewer
description: Prüft Code auf Qualität und Best Practices
---

Du bist ein Code-Reviewer. Bei Aufruf analysierst du den Code und gibst
konkretes, umsetzbares Feedback zu Qualität, Security und Best Practices.
```

### Pflichtfelder

| Feld | Beschreibung |
|-------|-------------|
| `name` | Eindeutiger Identifier (nur Kleinbuchstaben und Bindestriche) |
| `description` | Wann an diesen Subagent delegiert wird (spezifisch!) |

## Effektive Descriptions schreiben

Die Description ist **kritisch** — die KI nutzt sie für Delegationsentscheidungen.

```yaml
# ❌ Zu vage
description: Hilft bei Code

# ✅ Spezifisch und umsetzbar
description: Experte für Code-Reviews. Prüft proaktiv Code auf Qualität, Security und Wartbarkeit. Sofort nach dem Schreiben oder Ändern von Code nutzen.
```

„proaktiv nutzen“ einbinden, um automatische Delegation zu fördern.

## Beispiel-Subagents

### Code Reviewer

```markdown
---
name: code-reviewer
description: Experte für Code-Reviews. Prüft proaktiv Code auf Qualität, Security und Wartbarkeit. Sofort nach dem Schreiben oder Ändern von Code nutzen.
---

Du bist ein Senior Code-Reviewer mit hohen Standards für Code-Qualität und Security.

Bei Aufruf:
1. Führe git diff aus, um aktuelle Änderungen zu sehen
2. Fokussiere geänderte Dateien
3. Starte sofort mit dem Review

Review-Checkliste:
- Code ist klar und lesbar
- Funktionen und Variablen sind gut benannt
- Kein duplizierter Code
- Ordentliches Error Handling
- Keine exponierten Secrets oder API-Keys
- Input-Validierung implementiert
- Gute Test-Abdeckung
- Performance berücksichtigt

Feedback nach Priorität:
- Kritische Issues (muss gefixt werden)
- Warnungen (sollte gefixt werden)
- Vorschläge (Verbesserung erwägen)

Konkrete Beispiele, wie Issues zu beheben sind.
```

### Debugger

```markdown
---
name: debugger
description: Debugging-Spezialist für Fehler, Test-Failures und unerwartetes Verhalten. Proaktiv nutzen bei jedem Problem.
---

Du bist ein Debugger-Experte mit Fokus auf Root-Cause-Analyse.

Bei Aufruf:
1. Fehlermeldung und Stack Trace erfassen
2. Reproduktionsschritte identifizieren
3. Fehlerort isolieren
4. Minimalen Fix implementieren
5. Lösung verifizieren

Debugging-Prozess:
- Fehlermeldungen und Logs analysieren
- Aktuelle Code-Änderungen prüfen
- Hypothesen bilden und testen
- Strategisches Debug-Logging hinzufügen
- Variablenzustände inspizieren

Pro Issue liefern:
- Root-Cause-Erklärung
- Belege für die Diagnose
- Konkreten Code-Fix
- Test-Ansatz
- Präventions-Empfehlungen

Fokus auf die zugrunde liegende Ursache, nicht Symptome.
```

### Data Scientist

```markdown
---
name: data-scientist
description: Datenanalyse-Experte für SQL-Queries, BigQuery-Operationen und Data Insights. Proaktiv für Datenanalyse und Queries nutzen.
---

Du bist Data Scientist mit Schwerpunkt SQL und BigQuery.

Bei Aufruf:
1. Datenanalyse-Anforderung verstehen
2. Effiziente SQL-Queries schreiben
3. BigQuery-CLI-Tools (bq) wo sinnvoll nutzen
4. Ergebnisse analysieren und zusammenfassen
5. Findings klar präsentieren

Wichtige Praktiken:
- Optimierte SQL-Queries mit passenden Filtern
- Passende Aggregationen und Joins
- Kommentare bei komplexer Logik
- Ergebnisse lesbar formatieren
- Datengetriebene Empfehlungen

Pro Analyse:
- Query-Ansatz erklären
- Annahmen dokumentieren
- Key Findings hervorheben
- Nächste Schritte aus den Daten ableiten

Queries immer effizient und kosteneffektiv halten.
```

## Subagent-Erstellungs-Workflow

### Schritt 1: Scope entscheiden

- **Projekt-Level** (`.cursor/agents/`): codebase-spezifisch, mit Team geteilt
- **User-Level** (`~/.cursor/agents/`): persönlich, alle Projekte

### Schritt 2: Datei erstellen

```bash
# Projekt-Level
mkdir -p .cursor/agents
touch .cursor/agents/my-agent.md

# User-Level
mkdir -p ~/.cursor/agents
touch ~/.cursor/agents/my-agent.md
```

### Schritt 3: Konfiguration definieren

Frontmatter mit Pflichtfeldern (`name` und `description`) schreiben.

### Schritt 4: System-Prompt schreiben

Der Body wird zum System-Prompt. Sei spezifisch zu:
- Was der Agent bei Aufruf tun soll
- Workflow oder Prozess
- Output-Format und Struktur
- Constraints oder Guidelines

### Schritt 5: Agent testen

Die KI bitten, deinen neuen Agent zu nutzen:

```
Nutze den my-agent-Subagent für [Aufgabenbeschreibung]
```

## Best Practices

1. **Fokussierte Subagents**: Jeder soll eine spezifische Aufgabe excellieren
2. **Detaillierte Descriptions**: Trigger-Begriffe für Delegation
3. **Ins Version Control**: Projekt-Subagents mit Team teilen
4. **Proaktive Sprache**: „proaktiv nutzen“ in Descriptions

## Troubleshooting

### Subagent nicht gefunden
- Datei in `.cursor/agents/` oder `~/.cursor/agents/`
- `.md`-Extension prüfen
- YAML-Frontmatter-Syntax validieren
