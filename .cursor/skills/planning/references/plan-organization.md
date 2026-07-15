# Planerstellung & Organisation

## Verzeichnisstruktur

### Plan-Speicherort
Pläne im Verzeichnis `./plans` mit Zeitstempel und beschreibendem Namen speichern.

**Format:** `plans/YYYYMMDD-HHmm-your-plan-name/`

**Beispiel:** `plans/20251101-1505-authentication-and-profile-implementation/`

### Dateiorganisation

```
plans/
├── 20251101-1505-authentication-and-profile-implementation/
    ├── research/
    │   ├── researcher-XX-report.md
    │   └── ...
│   ├── reports/
│   │   ├── scout-report.md
│   │   ├── researcher-report.md
│   │   └── ...
│   ├── plan.md                                # Übersichts-Einstiegspunkt
│   ├── phase-01-setup-environment.md          # Umgebung einrichten
│   ├── phase-02-implement-database.md         # Datenbankmodelle
│   ├── phase-03-implement-api-endpoints.md    # API-Endpunkte
│   ├── phase-04-implement-ui-components.md    # UI-Komponenten
│   ├── phase-05-implement-authentication.md   # Auth & Autorisierung
│   ├── phase-06-implement-profile.md          # Profilseite
│   ├── phase-07-write-tests.md                # Tests schreiben
│   ├── phase-08-run-tests.md                  # Testausführung
│   ├── phase-09-code-review.md                # Code-Review
│   ├── phase-10-project-management.md         # Projektmanagement
│   ├── phase-11-onboarding.md                 # Onboarding
│   └── phase-12-final-report.md               # Abschlussbericht
└── ...
```

## Dateistruktur

### Übersichtsplan (plan.md)
- Generisch halten und unter 80 Zeilen
- Jede Phase mit Status/Fortschritt auflisten
- Auf detaillierte Phase-Dateien verlinken
- Timeline auf hoher Ebene
- Wichtige Abhängigkeiten

### Phase-Dateien (phase-XX-name.md)
Die Datei `./docs/development-rules.md` vollständig beachten.
Jede Phase-Datei sollte enthalten:

**Kontext-Links**
- Links zu verwandten Reports, Dateien, Dokumentation

**Überblick**
- Datum und Priorität
- Aktueller Status
- Kurze Beschreibung

**Wichtige Erkenntnisse**
- Wichtige Recherche-Findings
- Kritische Überlegungen

**Anforderungen**
- Funktionale Anforderungen
- Nicht-funktionale Anforderungen

**Architektur**
- Systemdesign
- Komponenteninteraktionen
- Datenfluss

**Verwandte Code-Dateien**
- Liste zu ändernder Dateien
- Liste zu erstellender Dateien
- Liste zu löschender Dateien

**Implementierungsschritte**
- Detaillierte, nummerierte Schritte
- Spezifische Anweisungen

**Todo-Liste**
- Checkbox-Liste zum Tracking

**Erfolgskriterien**
- Definition of Done
- Validierungsmethoden

**Risikobewertung**
- Potenzielle Probleme
- Mitigationsstrategien

**Sicherheitsüberlegungen**
- Auth/Authorization
- Datenschutz

**Nächste Schritte**
- Abhängigkeiten
- Follow-up-Tasks
