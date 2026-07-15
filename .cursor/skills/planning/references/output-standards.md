# Output-Standards & Qualität

## Task-Aufschlüsselung

- Komplexe Anforderungen in handhabbare, umsetzbare Tasks zerlegen
- Jeder Task eigenständig ausführbar mit klaren Abhängigkeiten
- Priorisieren nach Abhängigkeiten, Risiko, Business-Value
- Mehrdeutigkeit in Anweisungen eliminieren
- Konkrete Dateipfade für alle Änderungen angeben
- Klare Akzeptanzkriterien pro Task liefern

### Dateiverwaltung
Betroffene Dateien auflisten mit:
- Vollständigen Pfaden (nicht relativ)
- Aktionstyp (modify/create/delete)
- Kurzer Änderungsbeschreibung
- Abhängigkeiten zu anderen Änderungen
- Die Datei `./docs/development-rules.md` vollständig beachten.

## Workflow

1. **Erstanalyse** → Docs lesen, Kontext verstehen
2. **Recherchephase** → Researcher parallel spawnen, Ansätze untersuchen
3. **Synthese** → Reports analysieren, optimale Lösung identifizieren
4. **Designphase** → Architektur, Implementierungsdesign erstellen
5. **Plan-Dokumentation** → Umfassenden Plan in Markdown schreiben
6. **Review & Verfeinerung** → Vollständigkeit, Klarheit, Umsetzbarkeit sicherstellen

## Output-Anforderungen

### Was Planer tun
- NUR Pläne erstellen (keine Implementierung)
- Plan-Dateipfad und Zusammenfassung liefern
- Selbstständige Pläne mit nötigem Kontext
- Code-Snippets/Pseudocode zur Klärung
- Mehrere Optionen mit Trade-offs wenn sinnvoll
- Die Datei `./docs/development-rules.md` vollständig beachten.

### Schreibstil
**WICHTIG:** Grammatik zugunsten von Kürze opfern
- Klarheit vor Eloquenz
- Bullets und Listen nutzen
- Kurze Sätze
- Überflüssige Wörter entfernen
- Umsetzbare Infos priorisieren

### Offene Fragen
**WICHTIG:** Offene Fragen am Ende auflisten
- Fragen, die Klärung brauchen
- Technische Entscheidungen, die Input erfordern
- Unbekanntes mit Implementierungsauswirkung
- Trade-offs, die Business-Entscheidungen brauchen

## Qualitätsstandards

### Gründlichkeit
- Gründlich und spezifisch in Recherche/Planung
- Edge Cases, Fehlermodi berücksichtigen
- Gesamte User Journey durchdenken
- Alle Annahmen dokumentieren

### Wartbarkeit
- Langfristige Wartbarkeit berücksichtigen
- Für zukünftige Änderungen designen
- Entscheidungsbegründung dokumentieren
- Over-Engineering vermeiden
- Die Datei `./docs/development-rules.md` vollständig beachten.

### Recherchetiefe
- Bei Unsicherheit mehr recherchieren
- Mehrere Optionen mit klaren Trade-offs
- Gegen Best Practices validieren
- Branchenstandards berücksichtigen

### Sicherheit & Performance
- Alle Sicherheitsbedenken adressieren
- Performance-Auswirkungen identifizieren
- Für Skalierbarkeit planen
- Ressourcenbeschränkungen berücksichtigen

### Umsetzbarkeit
- Detailliert genug für Junior-Entwickler
- Gegen bestehende Patterns validieren
- Codebase-Standards-Konsistenz sicherstellen
- Klare Beispiele liefern

**Merke:** Planqualität bestimmt Implementierungserfolg. Sei umfassend und berücksichtige alle Lösungsaspekte.
