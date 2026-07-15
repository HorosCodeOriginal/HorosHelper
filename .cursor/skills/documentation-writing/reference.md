# Dokumentation schreiben — Vollständige Referenz

Diese Datei enthält die vollständige Spezifikation für Dokumentationsschreiben inklusive Frontmatter, Diataxis-Definitionen und Style-Konventionen.

## YAML-Frontmatter-Spezifikation

Alle umfangreichen Dokumentationsdateien sollten Frontmatter enthalten:

```yaml
---
title: Document Title
description: One-sentence summary for search and discovery
last_updated: 2025-11-25
review_schedule: quarterly | monthly | as-needed
owner: team-name | username
doc_type: tutorial | howto | reference | explanation
---
```

### Pflichtfelder

| Feld         | Zweck                 | Beispiel                          |
| ------------- | ----------------------- | -------------------------------- |
| `title`       | Dokumenttitel          | "Authentication Setup Guide"     |
| `description` | Suchfreundliche Zusammenfassung | "Configure JWT auth for the API" |

### Optionale Felder

| Feld             | Zweck                 | Beispiel                       |
| ----------------- | ----------------------- | ----------------------------- |
| `last_updated`    | Aktualitäts-Tracking       | "2025-11-25"                  |
| `review_schedule` | Wartungszyklus    | "quarterly"                   |
| `owner`           | Verantwortliche Partei       | "platform-team"               |
| `doc_type`        | Diataxis-Klassifikation | "howto"                       |
| `prerequisites`   | Erforderliche Vorlektüre        | "[getting-started.md]"        |
| `related`         | Querverweise        | "[auth-config.md, tokens.md]" |

## Diataxis-Framework — Vollständige Definitionen

### Tutorials (lernorientiert)

**Zweck**: Führe Anfänger durch eine vollständige Lernerfahrung.

**Merkmale**:

- Schritt-für-Schritt-Fortschritt
- Hands-on, umsetzungsorientiert
- Minimale Erklärung (nur genug zum Weitermachen)
- Klare Erfolgskriterien bei jedem Schritt
- Aufbau zu einem vollständigen Ergebnis

**Nutzer-Mindset**: „Ich will lernen“

**Schreibstil**:

- Nutze „wir“, um den Leser einzubeziehen
- Nummeriere jeden Schritt klar
- Füge Checkpoints ein („Du solltest jetzt sehen …“)
- Erkläre nicht warum, zeige wie

**Beispielstruktur**:

````markdown
# Tutorial: Building Your First Agent

## What You'll Build

A simple agent that responds to greetings.

## Prerequisites

- Python 3.10+
- API key configured

## Step 1: Create the Project

Create a new directory...

## Step 2: Write the Agent Code

```python
# ... complete, runnable code
```
````

## Step 3: Test Your Agent

Run: `python agent.py`
You should see: "Agent ready"

## Next Steps

Try [adding tools to your agent](./agent-tools.md).

````

### How-To Guides (aufgabenorientiert)

**Zweck**: Hilf erfahrenen Nutzern, ein konkretes Ziel zu erreichen.

**Merkmale**:
- Adressiert eine reale Aufgabe
- Setzt bestehende Kompetenz voraus
- Fokus auf das Ziel, nicht auf Lernen
- Praktisch und umsetzbar
- Mehrere Wege möglich

**Nutzer-Mindset**: „Ich muss X tun“

**Schreibstil**:
- Direkter, imperativer Ton
- Fokus auf die Aufgabe, nicht Hintergrund
- Häufige Varianten einbeziehen
- Wahrscheinliche Komplikationen ansprechen

**Beispielstruktur**:
```markdown
# How to Deploy to Azure

This guide covers deploying amplihack to Azure Container Apps.

## Prerequisites
- Azure CLI installed
- Container registry configured

## Steps

### 1. Build the Container
```bash
docker build -t amplihack:latest .
````

### 2. Push to Registry

```bash
az acr login --name myregistry
docker push myregistry.azurecr.io/amplihack:latest
```

### 3. Deploy to Container Apps

```bash
az containerapp create \
  --name amplihack \
  --image myregistry.azurecr.io/amplihack:latest
```

## Troubleshooting

### Container fails to start

Check logs: `az containerapp logs show --name amplihack`

## See Also

- [Azure configuration reference](../reference/azure-config.md)

````

### Reference (informationsorientiert)

**Zweck**: Liefere genaue, vollständige technische Informationen.

**Merkmale**:
- Für Nachschlagen organisiert, nicht zum Lesen
- Vollständig und korrekt
- Konsistente Struktur
- Keine Meinungen oder Empfehlungen
- Nüchtern und sachlich

**Nutzer-Mindset**: „Ich brauche die Details“

**Schreibstil**:
- Neutraler, beschreibender Ton
- Durchgängig einheitliche Formatierung
- Tabellen für strukturierte Daten
- Vollständige Parameterlisten

**Beispielstruktur**:
```markdown
# API Reference: /api/v1/analyze

## Endpoint

`POST /api/v1/analyze`

## Request

### Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes | Bearer token |
| `Content-Type` | Yes | `application/json` |

### Body

```json
{
  "file_path": "string (required)",
  "options": {
    "depth": "integer (1-10, default: 3)"
  }
}
````

### Parameters

| Parameter       | Type    | Required | Default | Description     |
| --------------- | ------- | -------- | ------- | --------------- |
| `file_path`     | string  | Yes      | -       | Path to analyze |
| `options.depth` | integer | No       | 3       | Analysis depth  |

## Response

### Success (200)

```json
{
  "complexity": 12.5,
  "issues": []
}
```

### Errors

| Code | Meaning              |
| ---- | -------------------- |
| 400  | Invalid request body |
| 404  | File not found       |
| 500  | Internal error       |

````

### Explanation (verständnisorientiert)

**Zweck**: Hilf Lesern, Konzepte und Kontext zu verstehen.

**Merkmale**:
- Liefert Hintergrund und Kontext
- Erklärt „warum“, nicht „wie“
- Verbindet Konzepte miteinander
- Kann Geschichte und Begründung enthalten
- Unterstützt tieferes Verständnis

**Nutzer-Mindset**: „Ich will verstehen“

**Schreibstil**:
- Reflektierender, analytischer Ton
- Verbindungen explizit machen
- Analogien und Vergleiche nutzen
- Trade-offs und Alternativen diskutieren

**Beispielstruktur**:
```markdown
# Understanding the Brick Philosophy

## Why Modularity Matters

Traditional software development often leads to tightly coupled code...

## The LEGO Analogy

Like LEGO bricks, our modules have standardized connection points...

## Comparison with Other Approaches

| Approach | Pros | Cons |
|----------|------|------|
| Monolith | Simple start | Hard to maintain |
| Microservices | Independent | Complex infrastructure |
| Brick Philosophy | Balanced | Requires discipline |

## Historical Context

This philosophy emerged from observing AI-assisted development patterns...

## Trade-offs

Choosing the brick philosophy means accepting:
- Stricter module boundaries
- More upfront design effort
- But: easier regeneration and maintenance

## Related Concepts
- [Zero-BS Implementation](./zero-bs.md)
- [Regeneratable Code](./regeneratable.md)
````

## Markdown-Style-Konventionen

### Überschriften

```markdown
# Title (H1) - One per document

## Major Section (H2) - Primary divisions

### Subsection (H3) - Secondary divisions

#### Detail (H4) - Rarely needed
```

### Code-Blöcke

Immer die Sprache angeben:

````markdown
```python
def example():
    pass
```
````

````

Erwartete Ausgabe einbeziehen:

```markdown
```python
print("Hello")
# Output: Hello
````

````

### Links

Interne Links nutzen relative Pfade:
```markdown
See [authentication config](./auth-config.md)
````

Externe Links mit Kontext:

```markdown
Based on [Anthropic's Agent SDK](https://docs.anthropic.com/agent-sdk)
```

### Admonitions

```markdown
> **Note**: Important information

> **Warning**: Potential issues

> **Tip**: Helpful suggestions
```

## Dokumentations-Review-Checkliste

### Vor dem Schreiben

- [ ] Dokumenttyp identifiziert (Diataxis)
- [ ] Korrekter Ablageort in `docs/` gewählt
- [ ] Bestehende verwandte Docs geprüft
- [ ] Verlinkendes Parent-Dokument identifiziert

### Während des Schreibens

- [ ] Klare, einfache Sprache
- [ ] Jeder Abschnitt hat klaren Zweck
- [ ] Beispiele sind real und ausführbar
- [ ] Überschriften sind aussagekräftig
- [ ] Keine zeitgebundenen Informationen

### Vor dem Einreichen

- [ ] Datei liegt im `docs/`-Verzeichnis
- [ ] Vom `docs/index.md` oder Parent verlinkt
- [ ] Frontmatter enthalten (für umfangreiche Docs)
- [ ] Alle Code-Beispiele getestet
- [ ] Rechtschreibung und Grammatik geprüft
- [ ] Relative Links funktionieren

### Nach dem Einreichen

- [ ] Links in gerenderter Ansicht prüfen
- [ ] Inhaltsverzeichnis rendert korrekt
- [ ] Suche findet das Dokument

## Token-Budget-Überlegungen

Beim Schreiben von Dokumentation:

- Einzelne Docs unter 300 Zeilen für beste Lesbarkeit halten
- Große Docs nach Thema in mehrere Dateien aufteilen
- Links nutzen, um verwandte Inhalte zu referenzieren
- Informationen nicht über Docs duplizieren
- Progressive Disclosure: Überblick → Details

## Häufige Fehler vermeiden

| Fehler          | Problem          | Lösung               |
| ---------------- | ---------------- | ---------------------- |
| Doc-Typen mischen | Verwirrt Leser | Ein Typ pro Datei      |
| Generische Beispiele | Nicht hilfreich | Echten Projektcode nutzen |
| Fehlender Kontext | Waisen-Links     | Beschreibenden Text hinzufügen |
| Veralteter Inhalt | Irreführend       | Review-Zyklus        |
| Tiefe Verschachtelung | Schwer navigierbar | Struktur flacher machen |
| Zu viele Details | Überwältigend     | Progressive Disclosure |
