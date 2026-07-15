---
name: mgrep-code-search
description: Semantische Code-Suche mit mgrep für effiziente Codebase-Exploration. Verwenden beim Suchen oder Erkunden von Codebases mit mehr als 30 nicht-gitignorierten Dateien und/oder verschachtelten Verzeichnisstrukturen. Natürlichsprachige semantische Suche ergänzt klassisches grep/ripgrep zum Finden von Features, Verstehen von Intent und Erkunden unbekannten Codes.
---

# mgrep Code Search

## Überblick

mgrep ist ein semantisches Suchtool für natürlichsprachige Abfragen über Code, Text, PDFs und Bilder. Besonders effektiv bei größeren oder komplexen Codebases, wo klassisches Pattern-Matching an Grenzen stößt.

## Wann diesen Skill nutzen

Nutze mgrep, wenn:
- die Codebase mehr als 30 nicht-gitignorierte Dateien enthält
- verschachtelte Verzeichnisstrukturen vorliegen
- du nach Konzepten, Features oder Intent suchst statt nach exakten Strings
- du eine unbekannte Codebase erkundest
- du verstehen willst, „wo“ oder „wie“ etwas implementiert ist

Nutze klassisches grep/ripgrep, wenn:
- du nach exakten Patterns oder Symbolen suchst
- Regex-basiertes Refactoring ansteht
- du spezifische Funktions- oder Variablennamen verfolgst

## Schnellstart

### Indexierung

Vor der Suche den Watcher starten, um das Repository zu indexieren:

```bash
bunx @mixedbread/mgrep watch
```

Der `watch`-Befehl indexiert das Repository und hält die Synchronisation mit Dateiänderungen. Er respektiert `.gitignore`- und `.mgrepignore`-Patterns.

### Suchen

```bash
bunx @mixedbread/mgrep "deine natürlichsprachige Abfrage" [path]
```

## Suchbefehle

### Einfache Suche

```bash
bunx @mixedbread/mgrep "where is authentication configured?"
bunx @mixedbread/mgrep "how do we handle errors in API calls?" src/
bunx @mixedbread/mgrep "database connection setup" src/lib
```

### Suchoptionen

| Option | Beschreibung |
|--------|-------------|
| `-m <count>` | Maximale Ergebnisse (Standard: 10) |
| `-c, --content` | Vollständigen Ergebnisinhalt anzeigen |
| `-a, --answer` | KI-gestützte Synthese der Ergebnisse erzeugen |
| `-s, --sync` | Index vor der Suche aktualisieren |
| `--no-rerank` | Relevanz-Optimierung deaktivieren |

### Beispiele mit Optionen

```bash
# Mehr Ergebnisse
bunx @mixedbread/mgrep -m 25 "user authentication flow"

# Vollständigen Inhalt der Treffer anzeigen
bunx @mixedbread/mgrep -c "error handling patterns"

# KI-synthetisierte Antwort erhalten
bunx @mixedbread/mgrep -a "how does the caching layer work?"

# Index vor der Suche synchronisieren
bunx @mixedbread/mgrep -s "payment processing" src/services
```

## Workflow

1. **Watcher starten** (einmal pro Session oder bei größeren Dateiänderungen):
   ```bash
   bunx @mixedbread/mgrep watch
   ```

2. **Semantisch suchen**:
   ```bash
   bunx @mixedbread/mgrep "wonach du suchst" [optional/path]
   ```

3. **Bei Bedarf verfeinern** mit Pfad-Einschränkungen oder Optionen:
   ```bash
   bunx @mixedbread/mgrep -m 20 -c "verfeinerte Abfrage" src/specific/directory
   ```

## Umgebungsvariablen

Standardwerte per Umgebungsvariablen konfigurieren:

| Variable | Zweck |
|----------|-------|
| `MGREP_MAX_COUNT` | Standard-Ergebnislimit |
| `MGREP_CONTENT` | Inhaltsanzeige aktivieren (1/true) |
| `MGREP_ANSWER` | KI-Synthese aktivieren (1/true) |
| `MGREP_SYNC` | Sync vor Suche (1/true) |

## Wichtige Hinweise

- Immer `bunx @mixedbread/mgrep` für Befehle nutzen (nicht npm/npx oder direkte Installation)
- Vor der Suche `bunx @mixedbread/mgrep watch` ausführen, damit der Index aktuell ist
- mgrep respektiert `.gitignore`-Patterns automatisch
- `.mgrepignore` für zusätzliche Ausschlüsse anlegen
