---
name: create-rule
description: >-
  Erstellt Cursor Rules für persistente KI-Anleitung. Verwenden beim Erstellen
  einer Rule, Hinzufügen von Coding-Standards, Projekt-Konventionen,
  dateispezifischen Patterns, RULE.md-Dateien oder Fragen zu .cursor/rules/
  oder AGENTS.md.
---
# Cursor Rules erstellen

Erstelle Projekt-Rules in `.cursor/rules/`, um dem KI-Agenten persistenten Kontext zu geben.

## Anforderungen sammeln

Vor dem Erstellen einer Rule klären:

1. **Zweck**: Was soll diese Rule durchsetzen oder vermitteln?
2. **Scope**: Immer anwenden oder nur für bestimmte Dateien?
3. **Datei-Patterns**: Bei dateispezifischen Rules welche Glob-Patterns?

### Aus Kontext ableiten

Bei vorherigem Gesprächskontext Rules aus dem Besprochenen ableiten. Mehrere Rules möglich bei unterschiedlichen Themen. Keine redundanten Fragen, wenn der Kontext Antworten liefert.

### Pflichtfragen

Wenn der Nutzer den Scope nicht genannt hat, fragen:
- „Soll diese Rule immer gelten oder nur bei bestimmten Dateien?“

Bei genannten Dateien ohne konkrete Patterns:
- „Welche Datei-Patterns soll die Rule abdecken?“ (z. B. `**/*.ts`, `backend/**/*.py`)

Klarheit bei Datei-Patterns ist sehr wichtig.

Nutze das AskQuestion-Tool, wenn verfügbar.

---

## Rule-Dateiformat

Rules sind `.mdc`-Dateien in `.cursor/rules/` mit YAML-Frontmatter:

```
.cursor/rules/
  typescript-standards.mdc
  react-patterns.mdc
  api-conventions.mdc
```

### Dateistruktur

```markdown
---
description: Brief description of what this rule does
globs: **/*.ts  # File pattern for file-specific rules
alwaysApply: false  # Set to true if rule should always apply
---

# Rule Title

Your rule content here...
```

### Frontmatter-Felder

| Feld | Typ | Beschreibung |
|-------|------|-------------|
| `description` | string | Was die Rule tut (im Rule Picker) |
| `globs` | string | Datei-Pattern — Rule gilt bei passenden offenen Dateien |
| `alwaysApply` | boolean | Wenn true, gilt für jede Session |

---

## Rule-Konfigurationen

### Immer anwenden

Für universelle Standards in jeder Konversation:

```yaml
---
description: Core coding standards for the project
alwaysApply: true
---
```

### Auf bestimmte Dateien anwenden

Für Rules bei bestimmten Dateitypen:

```yaml
---
description: TypeScript conventions for this project
globs: **/*.ts
alwaysApply: false
---
```

---

## Best Practices

### Rules knapp halten

- **Unter 50 Zeilen**: knapp und auf den Punkt
- **Ein Thema pro Rule**: große Rules in fokussierte Teile splitten
- **Umsetzbar**: wie klare interne Docs schreiben
- **Konkrete Beispiele**: idealerweise, wie man Probleme behebt

---

## Beispiel-Rules

### TypeScript-Standards

```markdown
---
description: TypeScript coding standards
globs: **/*.ts
alwaysApply: false
---

# Error Handling

\`\`\`typescript
// ❌ BAD
try {
  await fetchData();
} catch (e) {}

// ✅ GOOD
try {
  await fetchData();
} catch (e) {
  logger.error('Failed to fetch', { error: e });
  throw new DataFetchError('Unable to retrieve data', { cause: e });
}
\`\`\`
```

### React-Patterns

```markdown
---
description: React component patterns
globs: **/*.tsx
alwaysApply: false
---

# React Patterns

- Use functional components
- Extract custom hooks for reusable logic
- Colocate styles with components
```

---

## Checkliste

- [ ] Datei ist `.mdc` in `.cursor/rules/`
- [ ] Frontmatter korrekt konfiguriert
- [ ] Inhalt unter 500 Zeilen
- [ ] Enthält konkrete Beispiele
