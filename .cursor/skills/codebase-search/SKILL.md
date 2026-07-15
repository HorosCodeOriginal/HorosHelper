---
name: codebase-search
description: Durchsucht und navigiert große Codebases effizient. Verwenden beim Finden von Code-Patterns, Verfolgen von Funktionsaufrufen, Verstehen der Struktur oder Lokalisieren von Bugs. Semantic Search, Grep, AST-Analyse.
allowed-tools: [Read, Grep, Glob, Bash]
tags: [codebase-search, grep, code-navigation, pattern-matching]
platforms: [Claude, ChatGPT, Gemini]
---

# Codebase-Suche

## Wann diesen Skill nutzen
- Bestimmte Funktionen oder Klassen finden
- Funktionsaufrufe und Abhängigkeiten verfolgen
- Code-Struktur und Architektur verstehen
- Nutzungsbeispiele finden
- Code-Patterns identifizieren
- Bugs oder Issues lokalisieren
- Code-Archäologie (Legacy-Code)
- Impact-Analyse vor Änderungen

## Anweisungen

### Schritt 1: Verstehen, was du suchst

**Feature-Implementierung**:
- Wo ist Feature X implementiert?
- Wie funktioniert Feature Y?
- Welche Dateien sind an Feature Z beteiligt?

**Bug-Lokalisierung**:
- Woher kommt dieser Fehler?
- Welcher Code behandelt diesen Fall?
- Wo werden diese Daten geändert?

**API-Nutzung**:
- Wie wird diese API genutzt?
- Wo wird diese Funktion aufgerufen?
- Welche Beispiele gibt es?

**Konfiguration**:
- Wo sind Einstellungen definiert?
- Wie ist das konfiguriert?
- Welche Config-Optionen gibt es?

### Schritt 2: Suchstrategie wählen

**Semantic Search** (für konzeptuelle Fragen):
```
Nutzen wenn: Du konzeptuell weißt, was du suchst
Examples:
- "How do we handle user authentication?"
- "Where is email validation implemented?"
- "How do we connect to the database?"

Vorteile:
- Findet relevanten Code nach Bedeutung
- Funktioniert in unbekannten Codebases
- Gut für explorative Suchen
```

**Grep** (für exakten Text/Patterns):
```
Nutzen wenn: Du exakten Text oder Patterns kennst
Examples:
- Function names: "def authenticate"
- Class names: "class UserManager"
- Error messages: "Invalid credentials"
- Specific strings: "API_KEY"

Vorteile:
- Schnell und präzise
- Funktioniert mit Regex-Patterns
- Gut für bekannte Begriffe
```

**Glob** (für Datei-Discovery):
```
Nutzen wenn: Du Dateien per Pattern finden musst
Examples:
- "**/*.test.js" (all test files)
- "**/config*.yaml" (config files)
- "src/**/*Controller.py" (controllers)

Vorteile:
- Schnell Dateien nach Typ finden
- Dateistruktur entdecken
- Verwandte Dateien lokalisieren
```

### Schritt 3: Such-Workflow

**1. Breit starten, dann eingrenzen**:
```
Step 1: Semantic search "How does authentication work?"
Result: Points to auth/ directory

Step 2: Grep in auth/ for specific function
Pattern: "def verify_token"
Result: Found in auth/jwt.py

Step 3: Read the file
File: auth/jwt.py
Result: Understand implementation
```

**2. Verzeichnis-Targeting nutzen**:
```
# Ohne Target starten (überall suchen)
Query: "Where is user login implemented?"
Target: []

# Mit spezifischem Verzeichnis verfeinern
Query: "Where is login validated?"
Target: ["backend/auth/"]
```

**3. Suchen kombinieren**:
```
# Find where feature is implemented
Semantic: "user registration flow"

# Find all files involved
Grep: "def register_user"

# Find test files
Glob: "**/*register*test*.py"

# Understand the implementation
Read: registration.py, test_registration.py
```

### Schritt 4: Häufige Such-Patterns

**Funktionsdefinition finden**:
```bash
# Python
grep -n "def function_name" --type py

# JavaScript
grep -n "function functionName" --type js
grep -n "const functionName = " --type js

# TypeScript
grep -n "function functionName" --type ts
grep -n "export const functionName" --type ts

# Go
grep -n "func functionName" --type go

# Java
grep -n "public.*functionName" --type java
```

**Klassendefinition finden**:
```bash
# Python
grep -n "class ClassName" --type py

# JavaScript/TypeScript
grep -n "class ClassName" --type js,ts

# Java
grep -n "public class ClassName" --type java

# C++
grep -n "class ClassName" --type cpp
```

**Klassen-/Funktionsnutzung finden**:
```bash
# Python
grep -n "ClassName(" --type py
grep -n "function_name(" --type py

# JavaScript
grep -n "new ClassName" --type js
grep -n "functionName(" --type js
```

**Imports/Requires finden**:
```bash
# Python
grep -n "from.*import.*ModuleName" --type py
grep -n "import.*ModuleName" --type py

# JavaScript
grep -n "import.*from.*module-name" --type js
grep -n "require.*module-name" --type js

# Go
grep -n "import.*package-name" --type go
```

**Konfiguration finden**:
```bash
# Config files
glob "**/*config*.{json,yaml,yml,toml,ini}"

# Environment variables
grep -n "process\\.env\\." --type js
grep -n "os\\.environ" --type py

# Constants
grep -n "^[A-Z_]+\\s*=" --type py
grep -n "const [A-Z_]+" --type js
```

**TODO/FIXME finden**:
```bash
grep -n "TODO|FIXME|HACK|XXX" -i
```

**Error Handling finden**:
```bash
# Python
grep -n "try:|except|raise" --type py

# JavaScript
grep -n "try|catch|throw" --type js

# Go
grep -n "if err != nil" --type go
```

### Schritt 5: Fortgeschrittene Techniken

**Datenfluss verfolgen**:
```
1. Find where data is created
   Semantic: "Where is user object created?"

2. Search for variable usage
   Grep: "user\\." with context lines

3. Follow transformations
   Read: Files that modify user

4. Find where it's consumed
   Grep: "user\\." in relevant files
```

**Alle Call-Sites einer Funktion finden**:
```
1. Find function definition
   Grep: "def process_payment"
   Result: payments/processor.py:45

2. Find all imports of that module
   Grep: "from payments.processor import"
   Result: Multiple files

3. Find all calls to the function
   Grep: "process_payment\\("
   Result: All callsites

4. Read each callsite for context
   Read: Each file with context
```

**Feature End-to-End verstehen**:
```
1. Find API endpoint
   Semantic: "Where is user registration endpoint?"
   Result: routes/auth.py

2. Trace to controller
   Read: routes/auth.py
   Find: Calls to AuthController.register

3. Trace to service
   Read: controllers/auth.py
   Find: Calls to UserService.create_user

4. Trace to database
   Read: services/user.py
   Find: Database operations

5. Find tests
   Glob: "**/*auth*test*.py"
   Read: Test files for examples
```

**Verwandte Dateien finden**:
```
1. Start with known file
   Example: models/user.py

2. Find imports of this file
   Grep: "from models.user import"

3. Find files this imports
   Read: models/user.py
   Note: Import statements

4. Build dependency graph
   Map: All related files
```

**Impact-Analyse**:
```
Vor Änderung von Funktion X:

1. Find all callsites
   Grep: "function_name\\("

2. Find all tests
   Grep: "test.*function_name" -i

3. Check related functionality
   Semantic: "What depends on X?"

4. Review each usage
   Read: Each file using function

5. Plan changes
   Document: Impact and required updates
```

### Schritt 6: Such-Optimierung

**Passenden Kontext nutzen**:
```bash
# See surrounding context
grep -n "pattern" -C 5  # 5 lines before and after
grep -n "pattern" -B 3  # 3 lines before
grep -n "pattern" -A 3  # 3 lines after
```

**Groß-/Kleinschreibung**:
```bash
# Case insensitive
grep -n "pattern" -i

# Case sensitive (default)
grep -n "Pattern"
```

**Dateityp-Filter**:
```bash
# Specific type
grep -n "pattern" --type py

# Multiple types
grep -n "pattern" --type py,js,ts

# Exclude types
grep -n "pattern" --glob "!*.test.js"
```

**Regex-Patterns**:
```bash
# Any character: .
grep -n "function.*Name"

# Start of line: ^
grep -n "^class"

# End of line: $
grep -n "TODO$"

# Optional: ?
grep -n "function_name_?()"

# One or more: +
grep -n "[A-Z_]+"

# Zero or more: *
grep -n "import.*"

# Alternatives: |
grep -n "TODO|FIXME"

# Groups: ()
grep -n "(get|set)_user"

# Escape special chars: \
grep -n "function\(\)"
```

## Best Practices

1. **Mit Semantic Search starten**: Bei unbekanntem Code oder konzeptuellen Fragen
2. **Grep für Präzision**: Wenn du exakte Begriffe kennst
3. **Mehrere Suchen kombinieren**: Verständnis schrittweise aufbauen
4. **Umgebungskontext lesen**: Nicht nur Matching-Zeilen
5. **Dateihistorie prüfen**: `git blame` für Kontext
6. **Findings dokumentieren**: Wichtige Entdeckungen notieren
7. **Annahmen verifizieren**: Echten Code lesen, nicht raten
8. **Verzeichnis-Targeting**: Scope wenn möglich eingrenzen
9. **Daten folgen**: Datenfluss durchs System verfolgen
10. **Tests prüfen**: Tests zeigen oft Nutzungsbeispiele

## Häufige Such-Szenarien

### Szenario 1: Bug verstehen
```
1. Find error message
   Grep: "exact error message"

2. Find where it's thrown
   Read: File with error

3. Find what triggers it
   Semantic: "What causes X error?"

4. Find related code
   Grep: Related function names

5. Check tests
   Glob: "**/*test*.py"
   Look: For related test cases
```

### Szenario 2: Neue Codebase lernen
```
1. Find entry point
   Semantic: "Where does the application start?"
   Common files: main.py, index.js, app.py

2. Find main routes/endpoints
   Grep: "route|endpoint|@app\\."

3. Find data models
   Semantic: "Where are data models defined?"
   Common: models/, entities/

4. Find configuration
   Glob: "**/*config*"

5. Read README and docs
   Read: README.md, docs/
```

### Szenario 3: Refactoring-Vorbereitung
```
1. Find all usages
   Grep: "function_to_change"

2. Find tests
   Grep: "test.*function_to_change"

3. Find dependencies
   Semantic: "What does X depend on?"

4. Check imports
   Grep: "from.*import.*X"

5. Document scope
   List: All affected files
```

### Szenario 4: Feature hinzufügen
```
1. Find similar features
   Semantic: "How is similar feature implemented?"

2. Find where to add code
   Semantic: "Where should new feature go?"

3. Check patterns
   Read: Similar implementations

4. Find tests to emulate
   Glob: Test files for similar features

5. Check documentation
   Grep: "TODO.*new feature" -i
```

## Tool-Integration

**Git-Integration**:
```bash
# Wer hat diese Zeile geändert?
git blame filename

# Historie einer Datei
git log -p filename

# Wann Funktion hinzugefügt wurde
git log -S "function_name" --source --all

# Commits die X erwähnen
git log --grep="feature name"
```

**IDE-Integration**:
- „Go to Definition“ für schnelle Navigation
- „Find References“ für Nutzung
- „Find in Files“ für breite Suche
- Symbol-Suche für Klassen/Funktionen

**Dokumentation**:
- Inline-Kommentare prüfen
- Docstrings suchen
- README-Dateien lesen
- Architektur-Docs prüfen

## Troubleshooting

**Keine Ergebnisse**:
- Rechtschreibung und Case prüfen
- Semantic Search statt Grep
- Scope erweitern (Verzeichnis-Target entfernen)
- Andere Suchbegriffe
- Prüfen ob Dateien in .gitignore

**Zu viele Ergebnisse**:
- Verzeichnis-Targeting hinzufügen
- Spezifischere Patterns
- Nach Dateityp filtern
- Exakte Phrasen (Anführungszeichen)

**Falsche Ergebnisse**:
- Query spezifischer
- Grep statt Semantic für exakte Begriffe
- Kontext zu Semantic-Queries
- Dateitypen prüfen

## Referenzen

- [Ripgrep User Guide](https://github.com/BurntSushi/ripgrep/blob/master/GUIDE.md)
- [Regular Expressions Tutorial](https://regexone.com/)
- [Git Blame Guide](https://git-scm.com/docs/git-blame)

## Beispiele

### Beispiel 1: Basis-Nutzung
<!-- Add example content here -->

### Beispiel 2: Erweiterte Nutzung
<!-- Add advanced example content here -->
