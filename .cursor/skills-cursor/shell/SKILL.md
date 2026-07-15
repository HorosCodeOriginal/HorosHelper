---
name: shell
description: >-
  Führt den Rest einer /shell-Anfrage als wörtlichen Shell-Befehl aus. Nur
  verwenden, wenn der Nutzer explizit /shell aufruft und den folgenden Text
  direkt im Terminal ausführen möchte.
disable-model-invocation: true
---
# Shell-Befehle ausführen

Verwende diesen Skill nur, wenn der Nutzer explizit `/shell` aufruft.

## Verhalten

1. Behandle den gesamten Nutzertext nach dem `/shell`-Aufruf als den wörtlichen Shell-Befehl zum Ausführen.
2. Führe diesen Befehl sofort mit dem Terminal-Tool aus.
3. Schreibe den Befehl nicht um, erkläre ihn nicht und „verbessere“ ihn nicht vor dem Ausführen.
4. Inspiziere das Repository nicht zuerst, es sei denn, der Befehl selbst erfordert Repository-Kontext.
5. Wenn der Nutzer `/shell` ohne folgenden Text aufruft, frage, welcher Befehl ausgeführt werden soll.

## Antwort

- Führe den Befehl zuerst aus.
- Melde danach kurz den Exit-Status und relevantes stdout oder stderr.
