---
name: planning
description: "Technische Implementierungsplanung und Architekturdesign. Fähigkeiten: Feature-Planung, Systemarchitektur, technische Bewertung, Implementierungs-Roadmaps, Anforderungsaufteilung, Trade-off-Analyse, Codebase-Analyse, Lösungsdesign. Aktionen: planen, architektieren, designen, bewerten, technische Lösungen aufschlüsseln. Schlüsselwörter: Implementierungsplan, technisches Design, Architektur, Systemdesign, Roadmap, Anforderungsanalyse, Trade-offs, technische Bewertung, Feature-Planung, Lösungsdesign, Skalierbarkeit, Sicherheit, Wartbarkeit, Sprint-Planung, Task-Aufschlüsselung. Nutzen wenn: neue Features geplant werden, Systemarchitektur entworfen wird, technische Ansätze bewertet werden, Implementierungs-Roadmaps erstellt werden, komplexe Anforderungen aufgeschlüsselt werden, technische Trade-offs bewertet werden."
license: MIT
---

# Planning

Erstelle detaillierte technische Implementierungspläne durch Recherche, Codebase-Analyse, Lösungsdesign und umfassende Dokumentation.

## Wann nutzen

Nutze diesen Skill wenn:
- Du neue Feature-Implementierungen planst
- Du Systemdesigns architektierst
- Du technische Ansätze bewertest
- Du Implementierungs-Roadmaps erstellst
- Du komplexe Anforderungen aufschlüsselst
- Du technische Trade-offs bewertest

## Kernaufgaben & Regeln

Immer unter Beachtung der Prinzipien **YAGNI**, **KISS** und **DRY**.
**Sei ehrlich, direkt, auf den Punkt und knapp.**

### 1. Recherche & Analyse
Laden: `references/research-phase.md`
**Überspringen wenn:** Researcher-Reports vorliegen

### 2. Codebase-Verständnis
Laden: `references/codebase-understanding.md`
**Überspringen wenn:** Scout-Reports vorliegen

### 3. Lösungsdesign
Laden: `references/solution-design.md`

### 4. Planerstellung & Organisation
Laden: `references/plan-organization.md`

### 5. Task-Aufschlüsselung & Output-Standards
Laden: `references/output-standards.md`

## Workflow

1. **Erstanalyse** → Codebase-Docs lesen, Kontext verstehen
2. **Recherchephase** → Researcher spawnen, Ansätze untersuchen
3. **Synthese** → Reports analysieren, optimale Lösung identifizieren
4. **Designphase** → Architektur, Implementierungsdesign erstellen
5. **Plan-Dokumentation** → Umfassenden Plan schreiben
6. **Review & Verfeinerung** → Vollständigkeit, Klarheit, Umsetzbarkeit sicherstellen

## Output-Anforderungen

- KEIN Code implementieren — nur Pläne erstellen
- Mit Plan-Dateipfad und Zusammenfassung antworten
- Selbstständige Pläne mit nötigem Kontext sicherstellen
- Code-Snippets/Pseudocode zur Klärung einbinden
- Mehrere Optionen mit Trade-offs wenn sinnvoll
- Die Datei `./docs/development-rules.md` vollständig beachten.

**Plan-Verzeichnisstruktur**
```
plans/
└── YYYYMMDD-HHmm-plan-name/
    ├── research/
    │   ├── researcher-XX-report.md
    │   └── ...
    ├── reports/
    │   ├── XX-report.md
    │   └── ...
    ├── scout/
    │   ├── scout-XX-report.md
    │   └── ...
    ├── plan.md
    ├── phase-XX-phase-name-here.md
    └── ...
```

## Qualitätsstandards

- Gründlich und spezifisch sein
- Langfristige Wartbarkeit berücksichtigen
- Bei Unsicherheit gründlich recherchieren
- Sicherheits- und Performance-Bedenken adressieren
- Pläne detailliert genug für Junior-Entwickler machen
- Gegen bestehende Codebase-Patterns validieren

**Merke:** Planqualität bestimmt Implementierungserfolg. Sei umfassend und berücksichtige alle Lösungsaspekte.
