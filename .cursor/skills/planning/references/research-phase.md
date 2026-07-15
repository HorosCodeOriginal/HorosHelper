# Recherche- & Analysephase

**Wann überspringen:** Wenn Researcher-Reports vorliegen, diese Phase überspringen.

## Kernaktivitäten

### Parallele Researcher-Agents
- Mehrere `researcher`-Agents parallel spawnen, um verschiedene Ansätze zu untersuchen
- Warte auf alle Researcher-Reports, bevor du weitermachst
- Jeder Researcher untersucht einen bestimmten Aspekt oder Ansatz

### Sequential Thinking
- Nutze den `sequential-thinking`-Skill für dynamisches und reflektierendes Problemlösen
- Strukturierter Denkprozess für komplexe Analyse
- Ermöglicht mehrstufiges Reasoning mit Revisionsfähigkeit

### Dokumentations-Recherche
- Nutze den `docs-seeker`-Skill zum Lesen und Verstehen von Dokumentation
- Plugins, Packages und Frameworks recherchieren
- Aktuelle technische Dokumentation mit llms.txt-Standard finden

### GitHub-Analyse
- Nutze den `gh`-Befehl zum Lesen und Analysieren von:
  - GitHub Actions Logs
  - Pull Requests
  - Issues und Discussions
- Relevanten technischen Kontext aus GitHub-Ressourcen extrahieren

### Remote-Repository-Analyse
Wenn eine GitHub-Repository-URL gegeben ist, frische Codebase-Zusammenfassung erzeugen:
```bash
# usage: 
repomix --remote <github-repo-url>
# example: 
repomix --remote https://github.com/mrgoonie/human-mcp
```

### Debugger-Delegation
- An `debugger`-Agent für Root-Cause-Analyse delegieren
- Nutzen bei Untersuchung komplexer Issues oder Bugs
- Debugger-Agent spezialisiert auf Diagnose-Tasks

## Best Practices

- Breite vor Tiefe recherchieren
- Findings für Synthesephase dokumentieren
- Mehrere Ansätze zum Vergleich identifizieren
- Edge Cases während der Recherche berücksichtigen
- Sicherheitsimplikationen früh notieren
