---
name: statusline
description: >-
  Konfiguriert eine benutzerdefinierte Statuszeile in der CLI. Verwenden bei
  Status Line, statusline, statusLine, CLI-Statusleiste, Prompt-Footer-Anpassung
  oder Session-Kontext über dem Prompt.
---
# CLI-Statuszeile

Die CLI unterstützt eine konfigurierbare Statuszeile über dem Prompt. Ein Befehl wird bei jedem Conversation-Update gespawnt, erhält JSON auf stdin mit Session-Beschreibung, stdout wird als Statuszeile angezeigt. Spec aligned mit [Claude Code status line](https://code.claude.com/docs/en/statusline).

## Konfiguration

`statusLine`-Eintrag in `~/.cursor/cli-config.json`:

```json
{
  "statusLine": {
    "type": "command",
    "command": "~/.cursor/statusline.sh",
    "padding": 2
  }
}
```

Das Feld `command` unterstützt vollständige Pfade, `~`-Expansion und Shell-Style-Argument-Splitting. Du kannst auf eine Skriptdatei zeigen oder einen Inline-Befehl wie `jq -r '...'` nutzen.

| Feld | Pflicht | Default | Beschreibung |
|-------|----------|---------|-------------|
| `type` | ja | — | Muss `"command"` sein |
| `command` | ja | — | Pfad zu einem Executable oder Inline-Befehl. `~` wird expandiert. |
| `padding` | nein | `0` | Horizontaler Einzug (in Zeichen) für den Statuszeilen-Container. |
| `updateIntervalMs` | nein | `300` | Mindestintervall zwischen Aufrufen. Begrenzt auf >= 300ms. |
| `timeoutMs` | nein | `2000` | Maximale Laufzeit des Befehls, bevor er beendet wird. |

## Stdin-Payload

Der Befehl erhält ein JSON-Objekt auf stdin. Das TypeScript-Interface ist `StatusLinePayload` in `packages/agent-cli/src/hooks/use-status-line.ts`.

### Vollständiges JSON-Schema

```json
{
  "session_id": "abc123",
  "session_name": "my session",
  "transcript_path": "/path/to/transcript.jsonl",
  "render_width_chars": 120,
  "cwd": "/Users/me/project",
  "autorun": false,
  "model": {
    "id": "claude-4-opus",
    "display_name": "Claude 4 Opus",
    "param_summary": "(Thinking)",
    "max_mode": true
  },
  "workspace": {
    "current_dir": "/Users/me/project",
    "project_dir": "/Users/me/project/.cursor/transcripts",
    "added_dirs": []
  },
  "version": "1.2.3",
  "output_style": {
    "name": "default"
  },
  "context_window": {
    "total_input_tokens": 15234,
    "total_output_tokens": null,
    "context_window_size": 200000,
    "used_percentage": 34.5,
    "remaining_percentage": 65.5,
    "current_usage": null
  },
  "vim": {
    "mode": "NORMAL"
  },
  "worktree": {
    "name": "my-feature",
    "path": "/Users/me/.cursor/worktrees/repo/my-feature"
  }
}
```

### Verfügbare Felder

| Feld | Beschreibung |
|-------|-------------|
| `session_id` | Eindeutige Session-ID |
| `session_name` | Benutzerdefinierter Session-Name. Fehlt, wenn kein Name gesetzt wurde |
| `transcript_path` | Pfad zur Conversation-Transcript-Datei |
| `render_width_chars` | Nutzbare Terminal-Spalten minus eingebautes Padding |
| `cwd`, `workspace.current_dir` | Aktuelles Arbeitsverzeichnis (beide enthalten denselben Wert) |
| `autorun` | `true`, wenn Auto-Run für die aktuelle Session aktiv ist |
| `workspace.project_dir` | Verzeichnis, in dem Transcripts gespeichert werden |
| `workspace.added_dirs` | Zusätzliche Verzeichnisse (vorerst leeres Array) |
| `model.id`, `model.display_name` | Aktuelle Modell-ID und Anzeigename |
| `model.param_summary` | Formatierter Parameter-Summary (z. B. „(Thinking)“, „High“). Fehlt, wenn leer |
| `model.max_mode` | `true`, wenn Max Mode aktiv ist. Sonst fehlt |
| `version` | CLI-Versionsstring |
| `output_style.name` | `"default"` oder `"compact"` |
| `context_window.total_input_tokens` | Geschätzte Input-Tokens (abgeleitet aus used_percentage) |
| `context_window.total_output_tokens` | Kumulierte Output-Tokens (null, wenn nicht getrackt) |
| `context_window.context_window_size` | Maximale Context-Window-Größe in Tokens |
| `context_window.used_percentage` | Prozent des genutzten Context Windows |
| `context_window.remaining_percentage` | Prozent des verbleibenden Context Windows |
| `context_window.current_usage` | Token-Zahlen vom letzten API-Call (null vor dem ersten Call) |
| `vim.mode` | `"NORMAL"` oder `"INSERT"`, wenn Vim-Mode aktiv ist |
| `worktree.name` | Worktree-Name, wenn innerhalb eines Worktrees |
| `worktree.path` | Absoluter Pfad zum Worktree-Verzeichnis |

### Felder, die fehlen können

- `session_name` — nur vorhanden, wenn ein benutzerdefinierter Name gesetzt wurde
- `model.param_summary` — nur vorhanden, wenn das Modell Nicht-Default-Parameter hat
- `model.max_mode` — nur vorhanden, wenn Max Mode aktiv ist
- `vim` — nur vorhanden, wenn Vim-Mode aktiv ist
- `worktree` — nur vorhanden, wenn in einem Worktree

### Felder, die null sein können

- `context_window.current_usage` — null vor dem ersten API-Call
- `context_window.used_percentage`, `context_window.remaining_percentage` — können zu Session-Beginn null sein

## Stdout / Rendering

- **Mehrzeilig** unterstützt: jede stdout-Zeile rendert als eigene Zeile im Statusbereich.
- **ANSI-Farbcodes** unterstützt (chalk, tput, `\033[32m` usw.).
- Beendet der Befehl mit non-zero und leerem stdout, wird die Statuszeile nicht aktualisiert (vorheriger Text bleibt).
- Bei Timeout oder neuem Update während der Skript-Laufzeit wird der laufende Prozess beendet.
- Die Statuszeile läuft lokal und verbraucht keine API-Tokens.

## Beispiele

### Basic: Modell + Context-Nutzung

```bash
#!/usr/bin/env bash
payload=$(cat)
model=$(echo "$payload" | jq -r '.model.display_name')
pct=$(echo "$payload" | jq -r '.context_window.used_percentage // 0' | cut -d. -f1)
printf "\033[90m%s  ctx %s%%\033[0m" "$model" "$pct"
```

### Context-Fortschrittsbalken

```bash
#!/usr/bin/env bash
input=$(cat)
MODEL=$(echo "$input" | jq -r '.model.display_name')
PCT=$(echo "$input" | jq -r '.context_window.used_percentage // 0' | cut -d. -f1)

BAR_WIDTH=10
FILLED=$((PCT * BAR_WIDTH / 100))
EMPTY=$((BAR_WIDTH - FILLED))
BAR=""
[ "$FILLED" -gt 0 ] && printf -v FILL "%${FILLED}s" && BAR="${FILL// /▓}"
[ "$EMPTY" -gt 0 ] && printf -v PAD "%${EMPTY}s" && BAR="${BAR}${PAD// /░}"

echo "[$MODEL] $BAR $PCT%"
```

### Mehrzeilig mit Git-Info

```bash
#!/usr/bin/env bash
input=$(cat)
MODEL=$(echo "$input" | jq -r '.model.display_name')
DIR=$(echo "$input" | jq -r '.workspace.current_dir')
PCT=$(echo "$input" | jq -r '.context_window.used_percentage // 0' | cut -d. -f1)

BRANCH=""
git rev-parse --git-dir > /dev/null 2>&1 && BRANCH=" | 🌿 $(git branch --show-current 2>/dev/null)"

echo -e "\033[36m[$MODEL]\033[0m 📁 ${DIR##*/}$BRANCH"
echo -e "ctx $PCT%"
```

### Inline-jq-Befehl (ohne Skriptdatei)

```json
{
  "statusLine": {
    "type": "command",
    "command": "jq -r '\"[\\(.model.display_name)] \\(.context_window.used_percentage // 0)% context\"'"
  }
}
```

## Testen

Skript mit Mock-Input testen:

```bash
echo '{"model":{"display_name":"Opus"},"context_window":{"used_percentage":25}}' | ./statusline.sh
```

Der Befehl wird mit `child_process.spawn` gespawnt (kein Shell auf Unix, `shell: true` auf Windows für .cmd/.bat-Kompatibilität). Updates werden im konfigurierten Intervall debounced. Triggert ein neues Update während ein Skript läuft, wird der laufende Prozess per `AbortController` beendet und der neue Aufruf startet sofort.
