---
name: review
description: Prüft Code-Änderungen mit dem Bugbot- oder Security-Review-Subagenten.
disable-model-invocation: true
---
# Review

Frage den Nutzer mit dem AskQuestion-Tool, welches Review ausgeführt werden soll. Wenn AskQuestion nicht verfügbar ist, frage den Nutzer direkt. Stelle genau eine Single-Select-Frage mit zwei Optionen:

- `bugbot`: Bugbot (`/review-bugbot`)
- `security`: Security Review (`/review-security`)

Nach der Auswahl führe das passende Review einmal aus:

- Bugbot: Folge den Anweisungen von `/review-bugbot`.
- Security Review: Folge den Anweisungen von `/review-security`.
