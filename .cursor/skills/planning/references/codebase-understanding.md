# Codebase-Verständnis-Phase

**Wann überspringen:** Wenn Scout-Reports vorliegen, diese Phase überspringen.

## Kernaktivitäten

### Parallele Scout-Agents
- Nutze `/scout:ext` (bevorzugt) oder `/scout` (Fallback), um die Codebase nach Dateien zu durchsuchen, die für die Aufgabe nötig sind
- Jeder Scout findet Dateien für bestimmte Aufgabenaspekte
- Warte auf alle Scout-Reports vor der Analyse
- Effizient zum Finden relevanter Code-Stellen in großen Codebases

### Essenzielle Dokumentations-Review
Lies IMMER zuerst diese Dateien:

1. **`./docs/development-rules.md`** (WICHTIG)
   - Dateinamen-Konventionen
   - Dateigrößen-Management
   - Entwicklungsregeln und Best Practices
   - Code-Qualitätsstandards
   - Sicherheitsrichtlinien

2. **`./docs/codebase-summary.md`**
   - Projektstruktur und aktueller Status
   - Architektur-Überblick auf hoher Ebene
   - Komponentenbeziehungen

3. **`./docs/code-standards.md`**
   - Coding-Konventionen und Standards
   - Sprachspezifische Patterns
   - Namenskonventionen

4. **`./docs/design-guidelines.md`** (falls vorhanden)
   - Design-System-Richtlinien
   - Branding- und UI/UX-Konventionen
   - Komponentenbibliothek-Nutzung

### Umgebungsanalyse
- Entwicklungsumgebung-Setup prüfen
- Dotenv-Dateien und Konfiguration analysieren
- Erforderliche Abhängigkeiten identifizieren
- Build- und Deployment-Prozesse verstehen

### Pattern-Erkennung
- Bestehende Patterns in der Codebase studieren
- Konventionen und Architekturentscheidungen identifizieren
- Konsistenz in Implementierungsansätzen notieren
- Fehlerbehandlungs-Patterns verstehen

### Integrationsplanung
- Identifizieren, wie neue Features in bestehende Architektur integrieren
- Abhängigkeiten zwischen Komponenten abbilden
- Datenfluss und State-Management verstehen
- Rückwärtskompatibilität berücksichtigen

## Best Practices

- Mit Dokumentation starten, bevor du in Code eintauchst
- Scouts für gezielte Dateisuche nutzen
- Gefundene Patterns für Konsistenz dokumentieren
- Inkonsistenzen oder technische Schulden notieren
- Auswirkungen auf bestehende Features bedenken
