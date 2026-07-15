---
name: update-cli-config
description: >-
  Zeigt und ändert Cursor-CLI-Konfiguration in ~/.cursor/cli-config.json.
  Verwenden bei Änderung von CLI-Einstellungen, Permissions, Approval-Mode,
  Vim-Mode, Display-Optionen, Sandbox oder anderen CLI-Präferenzen.
metadata:
  surfaces:
    - cli
---
# Cursor CLI-Konfiguration

Dieser Skill erklärt, wie du Cursor-CLI-Einstellungen in `~/.cursor/cli-config.json` anzeigst und änderst.

## Config-Datei-Speicherort

Die Config-Datei ist `~/.cursor/cli-config.json`.

Projekte können Overrides über `.cursor/cli.json` schichten. Die CLI läuft vom Git-Root zum aktuellen Working Directory und merged jede gefundene `.cursor/cli.json` (tiefere Dateien haben Vorrang). Projekt-Overrides betreffen nur die aktuelle Session; sie werden nicht in die Home-Config zurückgeschrieben.

## Ändern

Lies `~/.cursor/cli-config.json`, wende Änderungen an und schreibe zurück. Standard-JSON. Änderungen wirken nach CLI-Neustart.

## Verfügbare Einstellungen

### `permissions` (required)
Tool-Permission-Regeln. Jeder Eintrag ist ein String-Pattern.
- `allow`: string[] — erlaubte Tool-Aufrufe (z. B. `"Shell(**)"`, `"Mcp(server-name, tool-name)"`)
- `deny`: string[] — verweigerte Tool-Aufrufe

### `editor`
- `vimMode`: boolean — Vim-Keybindings in der CLI-Eingabe
- `defaultBehavior`: `"ide"` | `"agent"` — Standard-Verhaltensmodus

### `display` (optional)
- `showLineNumbers`: boolean (default: false) — Zeilennummern in Code-Ausgabe
- `showThinkingBlocks`: boolean (default: false) — Thinking/Reasoning-Blöcke des Modells
- `showStatusIndicators`: boolean (default: false) — Status-Indikatoren in der UI

### `channel` (optional)
Release-Channel: `"prod"` | `"lab"` | `"static"`

### `maxMode` (optional)
boolean (default: false) — Max Mode für höherwertige Modell-Antworten

### `approvalMode` (optional)
Tool-Approval-Verhalten:
- `"allowlist"` (default) — Approval für Tools außerhalb der Allow-List
- Run Everything — alle Tool-Aufrufe auto-genehmigen (wie `--force` / `--yolo`; Approval-Mode auf den Nicht-Allowlist-Wert setzen)

### `sandbox` (optional)
Sandbox-Ausführungsumgebung:
- `mode`: `"disabled"` | `"enabled"` (default: `"disabled"`)
- `networkAccess`: `"user_config_only"` | `"user_config_with_defaults"` | `"allow_all"` — Netzwerkzugriff aus der Sandbox
- `networkAllowlist`: string[] — erlaubte Domains

### `network` (optional)
- `useHttp1ForAgent`: boolean (default: false) — HTTP/1.1 statt HTTP/2 für Agent-Verbindungen (SSE-Streaming)

### `bedrock` (optional)
AWS-Bedrock-Integration:
- `enabled`: boolean (default: false)
- `mode`: `"access-key"` | `"team-role"` (default: `"access-key"`)
- `region`: string — AWS-Region
- `testModel`: string — Test-Modell
- `teamRoleArn`: string — IAM-Role-ARN für Team-Mode
- `teamExternalId`: string — External ID für STS assume-role

### `attribution` (optional)
Git-Attribution für Agent-Arbeit:
- `attributeCommitsToAgent`: boolean (default: true)
- `attributePRsToAgent`: boolean (default: true)

### `webFetchDomainAllowlist` (optional)
string[] — erlaubte Domains für Web-Fetch (z. B. `"docs.github.com"`, `"*.example.com"`, `"*"`)

## Felder, die du NICHT ändern solltest

Interner/gecachter State — nicht manuell editieren:
- `version` — Config-Schema-Version
- `model` / `selectedModel` / `modelParameters` / `hasChangedDefaultModel` — Model Picker
- `privacyCache` — gecachter Privacy-Mode
- `authInfo` — gecachte Auth-Info
- `showSandboxIntro` — einmaliges UI-Flag
- `conversationClassificationScoredConversations` — interner Cache
