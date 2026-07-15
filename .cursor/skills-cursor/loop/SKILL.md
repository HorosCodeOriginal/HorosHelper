---
name: loop
description: >-
  Führt einen Prompt oder Skill in dieser Session in wiederkehrendem oder
  variablem Intervall aus (z. B. /loop 5m /foo).
disabled-environments:
  - cloud
---
# Loop

## Parsen

Akzeptiere `/loop [interval] <prompt>`.

- Führendes Intervall: `5m /foo`, `30s check status`, `2h run report`.
- Nachgestelltes Intervall: `check deploy every 5m`, `run tests every 10 minutes`.
- Kein Intervall: dynamischer Modus; der Agent wählt die nächste Verzögerung nach jedem Lauf.
- Leerer Prompt: zeige `Usage: /loop [interval] <prompt>`.

Nutze Intervalle wie `30s`, `5m`, `2h`, `1d`. Wandle Worteinheiten in kurze Einheiten um.

Nutze überwachte Shell-Ausgabe, um den Agent für wiederkehrende lokale Arbeit zu wecken.

## Fester Zeitplan

```bash
while true; do
  sleep <seconds>
  echo 'AGENT_LOOP_TICK_<purpose> {"prompt":"<prompt>"}'
done
```

1. Prüfe bestehende Terminals auf einen bereits laufenden passenden Loop.
2. Starte eine Background-Shell-Schleife mit `notify_on_output`.
3. Nutze ein eindeutiges Sentinel und ein Regex wie `^AGENT_LOOP_TICK_<purpose>`.
4. Smoke-Check einmal für sauberen Start.
5. Führe den Prompt einmal sofort nach dem Arming des Loops aus.
6. Das erste Sentinel sollte erst nach dem initialen Sleep kommen, damit der Start den Prompt nicht doppelt ausführt.
7. Tracke die PID, damit der Agent den Loop bei Anfrage stoppen kann.
8. Kurz bestätigen: Intervall, dass der Prompt bereits einmal lief, wann das erste Tick kommt, und dass der Loop bei jedem Tick feuert, bis gestoppt. Bei späteren Ticks kurzes Update, was sich geändert hat. Beim Stop: Loop gestoppt und warum.

## Dynamischer Zeitplan

Der Nutzer möchte, dass der Agent sich selbst tempo gibt. Entscheide, was die nächste Iteration lohnenswert macht — Zeitablauf oder ein beobachtbares Event.

1. **Führe den Prompt jetzt aus.**
2. **Wenn der nächste Lauf an ein Event gekoppelt ist** (Git-Ref bewegt sich, Log-Zeile matcht, Datei ändert sich, CI-Check fertig), arme einen Background-Watcher, der das Sentinel nur bei Event feuert, mit `notify_on_output` auf `^AGENT_LOOP_WAKE_<purpose>`. Einmal armen; bei späteren Ticks überspringen, wenn er noch läuft.
3. **Am Ende des Turns einen einmaligen zeitbasierten Wake armen**:

```bash
sleep <seconds>
echo 'AGENT_LOOP_WAKE_<purpose> {"prompt":"<prompt>"}'
```

   Mit aktivem Watcher ist das der **Fallback-Heartbeat** — lang halten, damit idle Ticks kein reiner Overhead sind. Ohne Watcher ist das der Takt — Verzögerung wählen, wann sich ein erneuter Check lohnt.

4. **Bei Wake** neueste Payload lesen, ihren `prompt` ausführen, dann nächsten Heartbeat re-armen (Watcher nur re-armen, wenn er beendet ist). Wenn sowohl Output-Wake als auch Completion-Notification kommen: auf Output reagieren, Completion ignorieren.
5. **Zum Stoppen** Watcher-PID killen und nächsten Heartbeat nicht armen.
6. Kurz bestätigen: Self-Pacing, ob Watcher primäres Wake-Signal ist, gewählte Fallback-Verzögerung, und dass der Prompt bereits lief.

## Prompt-Payload

Wake-Benachrichtigungen enthalten einen Output-Dateipfad, keinen eingereichten Prompt. Prompt neben dem Sentinel, vorzugsweise als JSON. Bei Wake neueste passende Zeile lesen und `prompt` ausführen. Der Prompt kann pro Tick variieren.

## Hinweise

- Shell-Befehle titeln als `Loop <schedule>: <prompt>` (z. B. `Loop every 5m: check deploy status`).
- Loop-Syntax an die Shell des Nutzers anpassen (z. B. PowerShell `while ($true) { ... Start-Sleep }` unter Windows). Die Beispiele oben nutzen bash.
- Bevorzuge überwachte Shell-Ausgabe statt OS-Cron, wenn der Agent Wake-Benachrichtigungen braucht; stdout bleibt am überwachten Task hängen.
- Eindeutiges Sentinel pro Loop, damit unrelated Output keine Notifications triggert.
- Keine lauten Befehle innerhalb des Loops.
- Keine doppelten festen Loops oder dynamischen Sleeper erstellen.
- Wenn der Nutzer stoppen möchte: getrackte Loop/Sleeper-PID killen, dann Shell-Task awaiten, damit die Completion-Notification konsumiert wird und den Agent später nicht weckt. Keinen weiteren dynamischen Wake planen.
