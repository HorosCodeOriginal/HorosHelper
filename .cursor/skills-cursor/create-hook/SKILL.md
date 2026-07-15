---
name: create-hook
description: >-
  Erstellt Cursor Hooks. Verwenden beim Erstellen eines Hooks, Schreiben von
  hooks.json, Hinzufügen von Hook-Skripten oder Automatisierung rund um
  Agent-Events.
---
# Cursor Hooks erstellen

Erstelle Hooks, wenn Cursor vor oder nach Agent-Events eigene Logik ausführen soll. Hooks sind Skripte oder prompt-basierte Checks, die JSON über stdin/stdout austauschen und Verhalten beobachten, blockieren, ändern oder nachbearbeiten können.

Wenn der Nutzer einen Hook möchte, höre nicht bei der Formatbeschreibung auf. Sammle fehlende Anforderungen und erstelle oder aktualisiere die Hook-Dateien direkt.

## Anforderungen sammeln

Bevor du etwas schreibst, kläre:

1. **Scope**: Projekt-Hook oder User-Hook?
2. **Trigger**: Welches Event soll den Hook ausführen?
3. **Verhalten**: Audit, deny/allow, Input umschreiben, Kontext injizieren oder Workflow fortsetzen?
4. **Implementierung**: Command-Hook (Skript) oder Prompt-Hook?
5. **Filtering**: Matcher nötig, damit er nur für bestimmte Tools, Commands oder Subagent-Typen läuft?
6. **Safety**: Bei Fehlern fail open oder fail closed?

Leite aus dem Gespräch ab, wenn möglich. Frage nur nach fehlenden Teilen.

## Richtigen Speicherort wählen

- **Projekt-Hooks**: `.cursor/hooks.json` und `.cursor/hooks/*`
- **User-Hooks**: `~/.cursor/hooks.json` und `~/.cursor/hooks/*`

Pfadverhalten:

- **Projekt-Hooks** laufen vom Projekt-Root — Pfade wie `.cursor/hooks/my-hook.sh`
- **User-Hooks** laufen von `~/.cursor/` — Pfade wie `./hooks/my-hook.sh` oder `hooks/my-hook.sh`

Bevorzuge **Projekt-Hooks**, wenn das Verhalten im Repo geteilt und versioniert werden soll.

## Hook-Event wählen

Das engste Event, das zum Ziel passt.

### Häufige Agent-Events

- `sessionStart`, `sessionEnd`: Session einrichten oder auditieren
- `preToolUse`, `postToolUse`, `postToolUseFailure`: über alle Tools
- `subagentStart`, `subagentStop`: Task/Subagent-Workflows steuern oder fortsetzen
- `beforeShellExecution`, `afterShellExecution`: Terminal-Befehle gate'n oder auditieren
- `beforeMCPExecution`, `afterMCPExecution`: MCP-Tool-Aufrufe gate'n oder auditieren
- `beforeReadFile`, `afterFileEdit`: Dateizugriff steuern oder Edits nachbearbeiten
- `beforeSubmitPrompt`: Prompts vor dem Senden validieren
- `preCompact`: Context-Compaction beobachten
- `stop`: Agent-Abschluss behandeln
- `afterAgentResponse`, `afterAgentThought`: Agent-Output oder Reasoning tracken

### Tab-Events

- `beforeTabFileRead`: Dateizugriff für Inline-Completions
- `afterTabFileEdit`: von Tab gemachte Edits nachbearbeiten

### Schnell-Auswahl

- **Shell-Befehle blocken oder genehmigen** -> `beforeShellExecution`
- **Shell-Output auditieren** -> `afterShellExecution`
- **Dateien nach Edits formatieren** -> `afterFileEdit`
- **Bestimmten Tool-Aufruf blocken oder umschreiben** -> `preToolUse`
- **Kontext nach erfolgreichem Tool hinzufügen** -> `postToolUse`
- **Steuern, ob Subagents laufen** -> `subagentStart`
- **Subagent-Loops verketten** -> `subagentStop`
- **Prompts auf Secrets oder Policy prüfen** -> `beforeSubmitPrompt`
- **MCP-Aufrufe schützen** -> `beforeMCPExecution`

## hooks.json-Format

`hooks.json` mit Schema-Version 1:

```json
{
  "version": 1,
  "hooks": {
    "afterFileEdit": [
      {
        "command": ".cursor/hooks/format.sh"
      }
    ]
  }
}
```

Jede Hook-Definition kann enthalten:

- `command`: Shell-Befehl oder Skript-Pfad
- `type`: `"command"` oder `"prompt"` (default `"command"`)
- `timeout`: Timeout in Sekunden
- `matcher`: Filter, wann der Hook läuft
- `failClosed`: Aktion blockieren, wenn Hook crasht, timeout oder ungültiges JSON
- `loop_limit`: vor allem für `stop`- und `subagentStop`-Follow-up-Loops

## Matcher

Matcher vermeiden, dass der Hook bei jedem Event läuft.

- `preToolUse` / `postToolUse` / `postToolUseFailure`: Tool-Typ wie `Shell`, `Read`, `Write`, `Task` oder MCP-Tools in `MCP: ...`-Form
- `subagentStart` / `subagentStop`: Subagent-Typ wie `generalPurpose`, `explore` oder `shell`
- `beforeShellExecution` / `afterShellExecution`: vollständiger Shell-Command-String
- `beforeReadFile`: Tool-Typ wie `Read` oder `TabRead`
- `afterFileEdit`: Tool-Typ wie `Write` oder `TabWrite`
- `beforeSubmitPrompt`: Wert `UserPromptSubmit`

Wichtige Matcher-Warnung:

- Matcher nutzen JavaScript-Regex, nicht POSIX/grep
- Keine POSIX-Klassen wie `[[:space:]]`; JavaScript-Äquivalente wie `\s`
- Bei kniffligen Matchern: Hook zuerst ohne Matcher oder mit sehr einfachem Matcher zum Laufen bringen, dann verschärfen

Wenn der Hook nur für eine riskante Befehlsfamilie gilt: script-seitiges Filtern für die erste Version, Matcher danach nur wenn einfach und klar korrekt.

## Command Hooks

Command Hooks sind der Default. Sie erhalten JSON auf stdin und können JSON auf stdout zurückgeben.

Vor Nutzung prüfen, dass jede Abhängigkeit in der Hook-Umgebung läuft:

- gültiges Shebang und ausführbares Skript
- Helper-Binaries installiert und auf `$PATH`
- bei Abhängigkeit von `jq`, `python3`, `node` oder repo-lokalen CLIs explizit verifizieren

Nicht annehmen, dass eine Binary existiert, nur weil sie auf deiner Maschine üblich ist.

### Minimales Projekt-Beispiel

```json
{
  "version": 1,
  "hooks": {
    "beforeShellExecution": [
      {
        "command": ".cursor/hooks/approve-network.sh",
        "matcher": "curl|wget|nc ",
        "failClosed": true
      }
    ]
  }
}
```

```bash
#!/bin/bash
input=$(cat)
command=$(echo "$input" | jq -r '.command // empty')

if [[ "$command" =~ curl|wget|nc ]]; then
  echo '{
    "permission": "ask",
    "user_message": "This command may make a network request. Please review it before continuing.",
    "agent_message": "A hook flagged this shell command as a possible network call."
  }'
  exit 0
fi

echo '{ "permission": "allow" }'
exit 0
```

Wichtiges Verhalten:

- Exit code `0`: Erfolg
- Exit code `2`: Aktion blockieren, wie deny
- Andere non-zero Exit codes: default fail open, außer `failClosed: true`

Hook-Skripte nach dem Erstellen ausführbar machen.

## Prompt Hooks

Prompt Hooks, wenn die Policy leichter zu beschreiben als zu skripten ist.

```json
{
  "version": 1,
  "hooks": {
    "beforeShellExecution": [
      {
        "type": "prompt",
        "prompt": "Does this command look safe to execute? Only allow read-only operations. Here is the hook input: $ARGUMENTS",
        "timeout": 10
      }
    ]
  }
}
```

Prompt Hooks für leichte Policy-Entscheidungen. Command Hooks, wenn die Logik deterministisch sein muss oder der Nutzer exaktes, auditierbares Verhalten braucht.

## Event-Output-Spickzettel

Nur die vom Event unterstützten Output-Felder zurückgeben.

- `preToolUse`: `permission`, `user_message`, `agent_message`, `updated_input`
- `postToolUse`: `additional_context`; bei MCP-Tools auch `updated_mcp_tool_output`
- `subagentStart`: `permission`, `user_message`
- `subagentStop`: `followup_message`
- `beforeShellExecution` / `beforeMCPExecution`: `permission`, `user_message`, `agent_message`

Tool-Aufruf umschreiben: `preToolUse`. Nur Shell-Befehle gate'n: `beforeShellExecution`.

## Implementierungs-Workflow

1. Richtigen Speicherort und Event wählen
2. Richtige `hooks.json` erstellen oder aktualisieren
3. Ohne Matcher oder mit einfachstem sicheren Matcher starten
4. Skript im passenden hooks-Verzeichnis erstellen
5. stdin-JSON lesen und gefordertes Verhalten implementieren
6. Skript ausführbar machen
7. Helper-Binaries installiert und auf `$PATH` verifizieren
8. Relevante Aktion triggern zum Testen
9. Verhalten in Cursors **Hooks**-Settings-Tab oder **Hooks**-Output-Channel prüfen

Bei bestehendem Hooks-Setup: unrelated Hooks bewahren, nur Minimum ändern.

## Validierung und Troubleshooting

- Cursor beobachtet `hooks.json` und lädt bei Save neu
- Wenn Hooks nicht laden: Cursor neu starten
- Relative Pfade doppelt prüfen:
  - Projekt-Hooks -> relativ zum Projekt-Root
  - User-Hooks -> relativ zu `~/.cursor/`
- Wenn der Hook gar nicht lädt: Matcher/Config-Parsing zuerst verdächtigen; Matcher entfernen und Basis-Hook bestätigen
- Externe Commands im Skript: jeweils mit `command -v` o. Ä. prüfen
- Block bei Fehler: `failClosed: true`
- Absichtliches Blocken per Command Hook: Exit code `2` ist gültig

## Abschluss-Checkliste

- [ ] Richtiger Hook-Speicherort und Pfad-Stil
- [ ] Engstes korrektes Event gewählt
- [ ] Matcher wo passend
- [ ] Nur vom Event unterstützte Felder zurückgegeben
- [ ] Skript ausführbar
- [ ] Hook durch echtes Event getestet
- [ ] Hooks-Tab oder Hooks-Output-Channel bei Debugging geprüft
