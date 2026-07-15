# Defense-in-Depth Validation

## Überblick

Wenn du einen Bug durch ungültige Daten fixst, fühlt sich eine Validierung an einer Stelle ausreichend an. Aber ein einzelner Check kann von anderen Code-Pfaden, Refactoring oder Mocks umgangen werden.

**Kernprinzip:** An JEDER Schicht validieren, durch die Daten fließen. Den Bug strukturell unmöglich machen.

## Warum mehrere Schichten

Einzelne Validierung: „Wir haben den Bug gefixt“
Mehrere Schichten: „Wir haben den Bug unmöglich gemacht“

Verschiedene Schichten fangen verschiedene Fälle:

- Entry Validation fängt die meisten Bugs
- Business Logic fängt Edge Cases
- Environment Guards verhindern kontextspezifische Gefahren
- Debug Logging hilft, wenn andere Schichten versagen

## Die vier Schichten

### Schicht 1: Entry-Point-Validation

**Zweck:** Offensichtlich ungültige Eingaben an der API-Grenze ablehnen

```typescript
function createProject(name: string, workingDirectory: string) {
  if (!workingDirectory || workingDirectory.trim() === '') {
    throw new Error('workingDirectory cannot be empty');
  }
  if (!existsSync(workingDirectory)) {
    throw new Error(`workingDirectory does not exist: ${workingDirectory}`);
  }
  if (!statSync(workingDirectory).isDirectory()) {
    throw new Error(`workingDirectory is not a directory: ${workingDirectory}`);
  }
  // ... proceed
}
```

### Schicht 2: Business-Logic-Validation

**Zweck:** Sicherstellen, dass Daten für diese Operation Sinn ergeben

```typescript
function initializeWorkspace(projectDir: string, sessionId: string) {
  if (!projectDir) {
    throw new Error('projectDir required for workspace initialization');
  }
  // ... proceed
}
```

### Schicht 3: Environment Guards

**Zweck:** Gefährliche Operationen in bestimmten Kontexten verhindern

```typescript
async function gitInit(directory: string) {
  // In tests, refuse git init outside temp directories
  if (process.env.NODE_ENV === 'test') {
    const normalized = normalize(resolve(directory));
    const tmpDir = normalize(resolve(tmpdir()));

    if (!normalized.startsWith(tmpDir)) {
      throw new Error(`Refusing git init outside temp dir during tests: ${directory}`);
    }
  }
  // ... proceed
}
```

### Schicht 4: Debug-Instrumentation

**Zweck:** Kontext für Forensik erfassen

```typescript
async function gitInit(directory: string) {
  const stack = new Error().stack;
  logger.debug('About to git init', {
    directory,
    cwd: process.cwd(),
    stack,
  });
  // ... proceed
}
```

## Pattern anwenden

Wenn du einen Bug findest:

1. **Datenfluss verfolgen** — Wo entsteht der schlechte Wert? Wo wird er genutzt?
2. **Alle Checkpoints mappen** — Jeden Punkt auflisten, den Daten passieren
3. **An jeder Schicht validieren** — Entry, Business, Environment, Debug
4. **Jede Schicht testen** — Versuche Schicht 1 zu umgehen, prüfe ob Schicht 2 greift

## Beispiel aus der Session

Bug: Leeres `projectDir` führte zu `git init` im Source Code

**Datenfluss:**

1. Test-Setup -> leerer String
2. `Project.create(name, '')`
3. `WorkspaceManager.createWorkspace('')`
4. `git init` läuft in `process.cwd()`

**Vier Schichten hinzugefügt:**

- Schicht 1: `Project.create()` validiert nicht leer/existiert/beschreibbar
- Schicht 2: `WorkspaceManager` validiert projectDir nicht leer
- Schicht 3: `WorktreeManager` verweigert git init außerhalb tmpdir in Tests
- Schicht 4: Stack-Trace-Logging vor git init

**Ergebnis:** Alle Tests grün, Bug nicht reproduzierbar

## Wichtige Erkenntnis

Alle vier Schichten waren nötig. Beim Testen fing jede Schicht Bugs, die die anderen verpassten:

- Verschiedene Code-Pfade umgingen Entry Validation
- Mocks umgingen Business-Logic-Checks
- Edge Cases auf anderen Plattformen brauchten Environment Guards
- Debug Logging identifizierte strukturellen Missbrauch

**Nicht bei einer Validierung stoppen.** Checks an jeder Schicht hinzufügen.
