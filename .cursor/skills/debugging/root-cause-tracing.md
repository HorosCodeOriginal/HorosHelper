# Root Cause Tracing

## Überblick

Bugs manifestieren sich oft tief im Call Stack (git init im falschen Verzeichnis, Datei am falschen Ort, DB mit falschem Pfad geöffnet). Der Instinkt ist, dort zu fixen, wo der Fehler erscheint — das behandelt nur ein Symptom.

**Kernprinzip:** Rückwärts durch die Call Chain bis zum ursprünglichen Trigger verfolgen, dann an der Quelle fixen.

## Wann nutzen

**Nutzen, wenn:**

- Fehler passiert tief in der Ausführung (nicht am Entry Point)
- Stack Trace zeigt lange Call Chain
- Unklar, wo ungültige Daten entstanden
- Du finden musst, welcher Test/Code das Problem auslöst

## Der Tracing-Prozess

### 1. Symptom beobachten

```
Error: git init failed in /Users/jesse/project/packages/core
```

### 2. Unmittelbare Ursache finden

**Welcher Code verursacht das direkt?**

```typescript
await execFileAsync('git', ['init'], { cwd: projectDir });
```

### 3. Fragen: Wer hat das aufgerufen?

```typescript
WorktreeManager.createSessionWorktree(projectDir, sessionId)
  -> called by Session.initializeWorkspace()
  -> called by Session.create()
  -> called by test at Project.create()
```

### 4. Weiter nach oben

**Welcher Wert wurde übergeben?**

- `projectDir = ''` (leerer String!)
- Leerer String als `cwd` wird zu `process.cwd()`
- Das ist das Source-Code-Verzeichnis!

### 5. Originalen Trigger finden

**Woher kam der leere String?**

```typescript
const context = setupCoreTest(); // Returns { tempDir: '' }
Project.create('name', context.tempDir); // Accessed before beforeEach!
```

## Stack Traces hinzufügen

Wenn du manuell nicht verfolgen kannst, Instrumentation hinzufügen:

```typescript
// Before the problematic operation
async function gitInit(directory: string) {
  const stack = new Error().stack;
  console.error('DEBUG git init:', {
    directory,
    cwd: process.cwd(),
    nodeEnv: process.env.NODE_ENV,
    stack,
  });

  await execFileAsync('git', ['init'], { cwd: directory });
}
```

**Kritisch:** In Tests `console.error()` nutzen (nicht Logger — wird evtl. unterdrückt)

**Ausführen und erfassen:**

```bash
npm test 2>&1 | grep 'DEBUG git init'
```

**Stack Traces analysieren:**

- Nach Test-Dateinamen suchen
- Zeilennummer finden, die den Aufruf auslöst
- Pattern identifizieren (gleicher Test? gleicher Parameter?)

## Welcher Test die Verschmutzung verursacht

Wenn etwas während Tests erscheint, aber du nicht weißt welcher Test:

Bisektion zum Polluter:

1. Erste Hälfte der Tests laufen lassen
2. Prüfen, ob Verschmutzung auftritt
3. Wenn ja, diese Hälfte bisektieren
4. Wenn nein, andere Hälfte bisektieren
5. Wiederholen bis ein einzelner Test identifiziert ist

## Reales Beispiel: Leeres projectDir

**Symptom:** `.git` in `packages/core/` (Source Code) erstellt

**Trace-Kette:**

1. `git init` läuft in `process.cwd()` <- leerer cwd-Parameter
2. WorktreeManager mit leerem projectDir aufgerufen
3. Session.create() übergab leeren String
4. Test griff auf `context.tempDir` vor beforeEach zu
5. setupCoreTest() liefert anfangs `{ tempDir: '' }`

**Root Cause:** Top-Level-Variablen-Init mit leerem Wert

**Fix:** tempDir als Getter, der wirft, wenn vor beforeEach zugegriffen wird

**Auch Defense-in-Depth:**

- Schicht 1: Project.create() validiert Verzeichnis
- Schicht 2: WorkspaceManager validiert nicht leer
- Schicht 3: NODE_ENV-Guard verweigert git init außerhalb tmpdir
- Schicht 4: Stack-Trace-Logging vor git init

## Kernprinzip

**NIEMALS nur dort fixen, wo der Fehler erscheint.** Zurückverfolgen zum ursprünglichen Trigger.

## Stack-Trace-Tipps

**In Tests:** `console.error()` statt Logger — Logger kann unterdrückt sein
**Vor der Operation:** Loggen vor der gefährlichen Operation, nicht erst nach Fehlschlag
**Kontext einbinden:** Verzeichnis, cwd, Umgebungsvariablen, Timestamps
**Stack erfassen:** `new Error().stack` zeigt die komplette Call Chain

## Real-World-Impact

Aus einer Debug-Session:

- Root Cause über 5 Ebenen Trace gefunden
- An der Quelle gefixt (Getter-Validation)
- 4 Defense-Schichten hinzugefügt
- Alle Tests grün, null Verschmutzung
