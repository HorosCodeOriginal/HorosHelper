---
name: migrate-to-skills
description: >-
  Konvertiert Cursor Rules „Applied intelligently“ (.cursor/rules/*.mdc) und
  Slash-Commands (.cursor/commands/*.md) ins Agent-Skills-Format
  (.cursor/skills/). Verwenden bei Migration von Rules oder Commands zu Skills,
  Konvertierung von .mdc-Rules zu SKILL.md oder Konsolidierung von Commands im
  skills-Verzeichnis.
disable-model-invocation: true
---
# Rules und Slash-Commands zu Skills migrieren

Konvertiere Cursor Rules („Applied intelligently“) und Slash-Commands ins Agent-Skills-Format.

**KRITISCH: Body-Inhalt exakt bewahren. Nicht ändern, neu formatieren oder „verbessern“ — wörtlich kopieren.**

## Speicherorte

| Ebene | Quelle | Ziel |
|-------|--------|------|
| Projekt | `{workspaceFolder}/**/.cursor/rules/*.mdc`, `{workspaceFolder}/.cursor/commands/*.md` |
| User | `~/.cursor/commands/*.md` |

Hinweise:
- Cursor Rules im Projekt können in verschachtelten Verzeichnissen liegen. Gründlich suchen und Glob-Patterns nutzen.
- Alles in ~/.cursor/worktrees ignorieren
- Alles in ~/.cursor/skills-cursor ignorieren. Reserviert für Cursors interne Built-in-Skills und vom System verwaltet.

## Zu migrierende Dateien finden

**Rules**: Migrieren, wenn die Rule eine `description` hat, aber KEINE `globs` und KEIN `alwaysApply: true`.

**Commands**: Alle migrieren — plain Markdown ohne Frontmatter.

## Konvertierungsformat

### Rules: .mdc → SKILL.md

```markdown
# Before: .cursor/rules/my-rule.mdc
---
description: What this rule does
globs:
alwaysApply: false
---
# Title
Body content...
```

```markdown
# After: .cursor/skills/my-rule/SKILL.md
---
name: my-rule
description: What this rule does
---
# Title
Body content...
```

Änderungen: `name` hinzufügen, `globs`/`alwaysApply` entfernen, Body exakt behalten.

### Commands: .md → SKILL.md

```markdown
# Before: .cursor/commands/commit.md
# Commit current work
Instructions here...
```

```markdown
# After: .cursor/skills/commit/SKILL.md
---
name: commit
description: Commit current work with standardized message format
disable-model-invocation: true
---
# Commit current work
Instructions here...
```

Änderungen: Frontmatter mit `name` (vom Dateinamen), `description` (aus Inhalt ableiten) und `disable-model-invocation: true`, Body exakt behalten.

**Hinweis:** `disable-model-invocation: true` verhindert automatische Skill-Aufrufe durch das Modell. Slash-Commands sind für explizite Nutzer-Auslösung über das `/`-Menü gedacht, nicht für automatische Modell-Vorschläge.

## Hinweise

- `name` nur Kleinbuchstaben mit Bindestrichen
- `description` ist kritisch für Skill-Discovery
- Originale optional nach erfolgreicher Migration löschen

### Rule migrieren (.mdc → SKILL.md)

1. Rule-Datei lesen
2. `description` aus dem Frontmatter extrahieren
3. Body-Inhalt extrahieren (alles nach dem schließenden `---` des Frontmatters)
4. Skill-Verzeichnis erstellen: `.cursor/skills/{skill-name}/` (Skill-Name = Dateiname ohne .mdc)
5. `SKILL.md` mit neuem Frontmatter (`name` und `description`) + EXAKTEM Original-Body schreiben (Whitespace, Formatierung, Code-Blöcke wörtlich)
6. Original-Rule-Datei löschen

### Command migrieren (.md → SKILL.md)

1. Command-Datei lesen
2. Description aus der ersten Überschrift (ohne `#`-Präfix)
3. Skill-Verzeichnis erstellen: `.cursor/skills/{skill-name}/` (Skill-Name = Dateiname ohne .md)
4. `SKILL.md` mit neuem Frontmatter (`name`, `description`, `disable-model-invocation: true`) + Leerzeile + EXAKTEM Original-Inhalt schreiben
5. Original-Command-Datei löschen

**KRITISCH: Body zeichengenau kopieren. Nicht neu formatieren, Tippfehler korrigieren oder „verbessern“.**

## Workflow

Wenn das Task-Tool verfügbar ist:
Lies NICHT selbst alle Dateien. Das delegierst du an Subagenten. Deine Aufgabe: Subagenten pro Dateikategorie dispatchen und auf Ergebnisse warten.

1. [ ] Skills-Verzeichnisse anlegen, falls nicht vorhanden (`.cursor/skills/` für Projekt, `~/.cursor/skills/` für User)
2. Drei schnelle General-Purpose-Subagenten (NICHT explore) parallel dispatchen für Projekt-Rules (Pattern: `{workspaceFolder}/**/.cursor/rules/*.mdc`), User-Commands (Pattern: `~/.cursor/commands/*.md`) und Projekt-Commands (Pattern: `{workspaceFolder}/**/.cursor/commands/*.md`):
  I. [ ] Dateien zum Migrieren im Pattern finden
  II. [ ] Bei Rules prüfen, ob „applied intelligently“ (hat `description`, keine `globs`, kein `alwaysApply: true`). Commands immer migrieren. Dateien NICHT per Terminal lesen. Read-Tool nutzen.
  III. [ ] Liste der zu migrierenden Dateien. Wenn leer, fertig.
  IV. [ ] Pro Datei: lesen, dann neue Skill-Datei schreiben mit EXAKTEM Body. NICHT per Terminal schreiben. Edit-Tool nutzen.
  V. [ ] Original löschen. NICHT per Terminal löschen. Delete-Tool nutzen.
  VI. [ ] Liste aller migrierten Skill-Dateien mit Original-Pfaden zurückgeben.
3. [ ] Auf alle Subagenten warten und dem Nutzer zusammenfassen. WICHTIG: Mitteilen, dass er zur Rücknahme der Migration fragen kann.
4. [ ] Wenn der Nutzer die Migration rückgängig machen möchte: Gegenteil der obigen Schritte zur Wiederherstellung der Originale.


Wenn das Task-Tool nicht verfügbar ist:
1. [ ] Skills-Verzeichnisse anlegen, falls nicht vorhanden
2. [ ] Zu migrierende Dateien in Projekt (`.cursor/`) und User (`~/.cursor/`) finden
3. [ ] Bei Rules prüfen, ob „applied intelligently“. Commands immer migrieren. NICHT per Terminal lesen.
4. [ ] Liste erstellen. Wenn leer, fertig.
5. [ ] Pro Datei: lesen, Skill-Datei mit EXAKTEM Body schreiben. NICHT per Terminal.
6. [ ] Original löschen. NICHT per Terminal.
7. [ ] Dem Nutzer zusammenfassen. WICHTIG: Rücknahme-Hinweis.
8. [ ] Bei Rücknahme-Wunsch: Originale wiederherstellen.
