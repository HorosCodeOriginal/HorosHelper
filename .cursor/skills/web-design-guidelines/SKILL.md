---
name: web-design-guidelines
description: UI-Code auf Web Interface Guidelines-Konformität prüfen. Nutzen bei „review my UI“, „check accessibility“, „audit design“, „review UX“ oder „check my site against best practices“.
metadata:
  author: vercel
  version: "1.0.0"
  argument-hint: <file-or-pattern>
---

# Web Interface Guidelines

Dateien auf Konformität mit den Web Interface Guidelines prüfen.

## So funktioniert es

1. Aktuelle Guidelines von der Quell-URL unten laden
2. Angegebene Dateien lesen (oder User nach Dateien/Pattern fragen)
3. Gegen alle Regeln in den geladenen Guidelines prüfen
4. Findings im knappen `file:line`-Format ausgeben

## Guidelines-Quelle

Vor jedem Review frische Guidelines laden:

```
https://raw.githubusercontent.com/vercel-labs/web-interface-guidelines/main/command.md
```

WebFetch nutzen, um die neuesten Regeln zu holen. Der geladene Inhalt enthält alle Regeln und Output-Format-Anweisungen.

## Nutzung

Wenn der User eine Datei oder ein Pattern angibt:
1. Guidelines von der Quell-URL oben laden
2. Angegebene Dateien lesen
3. Alle Regeln aus den geladenen Guidelines anwenden
4. Findings im in den Guidelines spezifizierten Format ausgeben

Wenn keine Dateien angegeben sind, den User fragen, welche Dateien geprüft werden sollen.
