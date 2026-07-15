---
name: update-cursor-settings
description: >-
  Ändert Cursor/VSCode-Nutzereinstellungen in settings.json. Verwenden bei
  Editor-Einstellungen, Präferenzen, Themes, Schriftgröße, Tab-Größe,
  Format on Save, Auto Save, Keybindings oder anderen settings.json-Werten.
metadata:
  surfaces:
    - ide
---
# Cursor-Einstellungen aktualisieren

Dieser Skill führt dich durch Änderungen an Cursor/VSCode-Nutzereinstellungen. Verwende ihn, wenn du Editor-Einstellungen, Präferenzen, Themes, Keybindings oder `settings.json`-Werte ändern möchtest.

## Speicherort der Einstellungsdatei

| OS | Pfad |
|----|------|
| macOS | ~/Library/Application Support/Cursor/User/settings.json |
| Linux | ~/.config/Cursor/User/settings.json |
| Windows | %APPDATA%\Cursor\User\settings.json |

## Vor dem Ändern

1. **Bestehende Einstellungsdatei lesen**, um die aktuelle Konfiguration zu verstehen
2. **Bestehende Einstellungen bewahren** — nur das vom Nutzer Gewünschte ändern/hinzufügen
3. **JSON-Syntax validieren** vor dem Schreiben, um den Editor nicht zu brechen

## Einstellungen ändern

### Schritt 1: Aktuelle Einstellungen lesen

```typescript
// Read the settings file first
const settingsPath = "~/Library/Application Support/Cursor/User/settings.json";
// Use the Read tool to get current contents
```

### Schritt 2: Zu ändernde Einstellung identifizieren

Häufige Kategorien:
- **Editor**: `editor.fontSize`, `editor.tabSize`, `editor.wordWrap`, `editor.formatOnSave`
- **Workbench**: `workbench.colorTheme`, `workbench.iconTheme`, `workbench.sideBar.location`
- **Files**: `files.autoSave`, `files.exclude`, `files.associations`
- **Terminal**: `terminal.integrated.fontSize`, `terminal.integrated.shell.*`
- **Cursor-spezifisch**: Einstellungen mit Präfix `cursor.` oder `aipopup.`

### Schritt 3: Einstellung aktualisieren

Bei settings.json:
1. Bestehendes JSON parsen (Kommentare beachten — VSCode settings unterstützen JSON mit Kommentaren)
2. Gewünschte Einstellung hinzufügen oder aktualisieren
3. Alle anderen Einstellungen bewahren
4. Mit korrekter Formatierung zurückschreiben (2-Space-Einrückung)

### Beispiel: Schriftgröße ändern

Wenn der Nutzer sagt „Schrift größer machen“:

```json
{
  "editor.fontSize": 16
}
```

### Beispiel: Format on Save aktivieren

Wenn der Nutzer sagt „Code beim Speichern formatieren“:

```json
{
  "editor.formatOnSave": true
}
```

### Beispiel: Theme ändern

Wenn der Nutzer sagt „dunkles Theme“ oder „Theme ändern“:

```json
{
  "workbench.colorTheme": "Default Dark Modern"
}
```

## Wichtige Hinweise

1. **JSON mit Kommentaren**: VSCode/Cursor settings.json unterstützt Kommentare (`//` und `/* */`). Beim Lesen Kommentare beachten. Beim Schreiben Kommentare wenn möglich bewahren.

2. **Neustart kann nötig sein**: Manche Einstellungen wirken sofort, andere brauchen Window-Reload oder Cursor-Neustart. Den Nutzer informieren, wenn ein Neustart nötig ist.

3. **Backup**: Bei größeren Änderungen erwähnen, dass der Nutzer per Ctrl/Cmd+Z in der Einstellungsdatei oder per Git-Revert rückgängig machen kann, falls getrackt.

4. **Workspace vs. User Settings**:
   - User Settings (dieser Skill): global für alle Projekte
   - Workspace Settings (`.vscode/settings.json`): nur aktuelles Projekt

5. **Commit Attribution**: Wenn der Nutzer nach Commit-Attribution fragt, klären, ob **CLI-Agent** oder **IDE-Agent** gemeint ist. Für CLI: `~/.cursor/cli-config.json`. Für IDE: UI unter **Cursor Settings > Agent > Attribution** (nicht settings.json).

## Häufige Nutzerwünsche → Einstellungen

| Nutzerwunsch | Einstellung |
|--------------|---------|
| „größere/kleinere Schrift“ | `editor.fontSize` |
| „Tab-Größe ändern“ | `editor.tabSize` |
| „Format on Save“ | `editor.formatOnSave` |
| „Word Wrap“ | `editor.wordWrap` |
| „Theme ändern“ | `workbench.colorTheme` |
| „Minimap ausblenden“ | `editor.minimap.enabled` |
| „Auto Save“ | `files.autoSave` |
| „Zeilennummern“ | `editor.lineNumbers` |
| „Bracket Matching“ | `editor.bracketPairColorization.enabled` |
| „Cursor-Stil“ | `editor.cursorStyle` |
| „Smooth Scrolling“ | `editor.smoothScrolling` |

## Workflow

1. ~/Library/Application Support/Cursor/User/settings.json lesen
2. JSON-Inhalt parsen
3. Gewünschte Einstellung(en) hinzufügen/ändern
4. Aktualisiertes JSON zurückschreiben
5. Nutzer informieren, dass die Einstellung geändert wurde und ob Reload nötig ist
