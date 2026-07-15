---
name: debugging
description: Systematisches 4-Phasen-Debugging mit Root-Cause-Analyse. Verwenden beim Bugfixen, um zufällige Fixes zu vermeiden.
version: 1.1.1
model: composer-2.5
invoked_by: both
user_invocable: true
tools: [Read, Write, Edit, Bash, Glob, Grep]
best_practices:
  - Investigate root cause before any fix
  - Reproduce the bug reliably first
  - Compare working vs broken examples
  - Make one change at a time
error_handling: strict
streaming: supported
verified: true
lastVerifiedAt: 2026-02-22T00:00:00.000Z
---

**Modus: Kognitiv/Prompt-gesteuert** — Kein eigenständiges Utility-Skript; über Agent-Kontext nutzen.

# Systematisches Debugging

## Überblick

Zufällige Fixes verschwenden Zeit und erzeugen neue Bugs. Schnelle Patches maskieren Grundprobleme.

**Kernprinzip:** IMMER Root Cause finden, bevor du Fixes versuchst. Symptom-Fixes sind Scheitern.

**Den Buchstaben dieses Prozesses zu verletzen heißt, den Geist des Debuggings zu verletzen.**

## Iron Laws

1. **NIEMALS** einen Fix vorschlagen oder implementieren, bevor Phase 1 Root-Cause-Investigation abgeschlossen ist — ein Fix ohne Root Cause ist Raten.
2. **IMMER** den Bug zuverlässig reproduzieren vor dem Debuggen — ohne konsistente Reproduktion debuggst du nicht das echte Problem.
3. **NIEMALS** mehr als eine Änderung gleichzeitig beim Testen einer Hypothese — sonst weißt du nicht, welche Änderung geholfen hat.
4. **IMMER** nach 3 gescheiterten Fix-Versuchen die Architektur hinterfragen — wenn jeder Fix ein neues Problem zeigt, ist es architektonisch, nicht symptomatisch.
5. **NIEMALS** einen failing Test vor dem Fix überspringen — ohne Test kannst du den Fix und Regression nicht verifizieren.

## Wann nutzen

Für JEDES technische Problem:

- Test-Fehlschläge
- Production-Bugs
- Unerwartetes Verhalten
- Performance-Probleme
- Build-Fehlschläge
- Integrationsprobleme

**BESONDERS nutzen, wenn:**

- Zeitdruck (Notfälle machen Raten verlockend)
- „Nur ein schneller Fix“ offensichtlich wirkt
- Du schon mehrere Fixes versucht hast
- Der letzte Fix nicht wirkte
- Du das Problem nicht vollständig verstehst

**Nicht überspringen, wenn:**

- Problem simpel wirkt (einfache Bugs haben auch Root Causes)
- Du es eilig hast (Hektik garantiert Nacharbeit)
- Manager will SOFORT fixen (systematisch ist schneller als Herumprobieren)

## Wann nutzen: debugging vs. smart-debug

| Szenario                             | `debugging` | `smart-debug` |
| ------------------------------------ | --------------- | ----------------- |
| Einfacher, lokal reproduzierbarer Bug | Ja             | Overkill          |
| Root-Cause-Bereich bekannt        | Ja             | Optional          |
| Static Analysis / Code-Review-Bug    | Ja             | Nein                |
| Runtime / Production-Issue           | Hier starten      | Bevorzugt         |
| Intermittent / schwer reproduzierbar     | Eskalieren        | Ja               |
| Braucht Hypothesen-Ranking        | Nein              | Ja (blocking)    |
| Braucht Instrumentation + Log-Analyse | Nein              | Ja               |
| Observability-driven (Traces, APM)   | Nein              | Ja               |

**Faustregel**: Mit `debugging` bei straightforward Bugs starten. Zu `smart-debug` eskalieren bei Hypothesen-Ranking, strukturierter Instrumentation oder intermittent/production-only Bugs.

**Siehe auch**: `.claude/skills/smart-debug/SKILL.md`

## Die vier Phasen

Jede Phase MUSS abgeschlossen sein, bevor du zur nächsten gehst.

### Phase 1: Root-Cause-Investigation

**VOR jedem Fix-Versuch:**

1. **Fehlermeldungen sorgfältig lesen**
   - Fehler oder Warnungen nicht überspringen
   - Enthalten oft die exakte Lösung
   - Stack Traces vollständig lesen
   - Zeilennummern, Dateipfade, Error Codes notieren

2. **Konsistent reproduzieren**
   - Kannst du es zuverlässig auslösen?
   - Was sind die exakten Schritte?
   - Passiert es jedes Mal?
   - Wenn nicht reproduzierbar — mehr Daten sammeln, nicht raten

3. **Kürzliche Änderungen prüfen**
   - Was hat sich geändert, das das verursachen könnte?
   - Git diff, recent commits
   - Neue Dependencies, Config-Änderungen
   - Umgebungsunterschiede

4. **Belege in Multi-Component-Systemen sammeln**

   **WENN das System mehrere Komponenten hat (CI - build - signing, API - service - database):**

   **VOR Fix-Vorschlägen diagnostische Instrumentation hinzufügen:**

   ```
   Für JEDE Komponenten-Grenze:
     - Loggen, welche Daten die Komponente betreten
     - Loggen, welche Daten die Komponente verlassen
     - Environment/Config-Propagation verifizieren
     - State auf jeder Schicht prüfen

   Einmal laufen lassen, um Belege zu sammeln WO es bricht
   DANN Belege analysieren, um fehlerhafte Komponente zu identifizieren
   DANN diese spezifische Komponente untersuchen
   ```

   **Beispiel (Multi-Layer-System):**

   ```bash
   # Layer 1: Workflow
   echo "=== Secrets available in workflow: ==="
   echo "IDENTITY: ${IDENTITY:+SET}${IDENTITY:-UNSET}"

   # Layer 2: Build script
   echo "=== Env vars in build script: ==="
   env | grep IDENTITY || echo "IDENTITY not in environment"

   # Layer 3: Signing script
   echo "=== Keychain state: ==="
   security list-keychains
   security find-identity -v

   # Layer 4: Actual signing
   codesign --sign "$IDENTITY" --verbose=4 "$APP"
   ```

   **Das zeigt:** Welche Schicht fehlschlägt (secrets - workflow OK, workflow - build FAIL)

   **Bei verteilten/Microservice-Systemen — OpenTelemetry-Traces bevorzugen:**

   ```bash
   # Query traces by component (preferred over manual echo/env logging)
   pnpm trace:query --component <service-name> --event <event-name> --since <ISO-8601> --limit 200

   # When trace ID is already known
   pnpm trace:query --trace-id <traceId> --compact --since <ISO-8601> --limit 200
   ```

   **Fragmentierte Traces** (jeder Service hat eigenen Root-Span, Trace-IDs matchen nicht über Grenzen)
   = kaputte Context-Propagation. `traceparent`/`tracestate`-Header-Weiterleitung fixen, bevor du Business-Logik untersuchst.

   > **Instrumentation Gate (vor Hypothesen-Generierung):** Bleibt Runtime-Verhalten nach Static Analysis unklar, gezielte Log-Statements an Key Decision Nodes hinzufügen, bevor du Hypothesen generierst. Session-scoped Log-Dateien (`.claude/context/tmp/debug-{sessionId}.log`) für Runtime-State. Human-in-the-loop: Nutzer bitten, Bug nach Instrumentation zu reproduzieren, bevor du Ergebnisse analysierst. Erst zu Phase 2, wenn Runtime-Belege da sind.

5. **Datenfluss verfolgen**

   **WENN der Fehler tief im Call Stack liegt:**

   Siehe `root-cause-tracing.md` in diesem Verzeichnis für die vollständige Backward-Tracing-Technik.

   **Kurzversion:**
   - Woher kommt der schlechte Wert?
   - Wer hat das mit schlechtem Wert aufgerufen?
   - Weiter nach oben bis zur Quelle
   - An der Quelle fixen, nicht am Symptom

### Phase 2: Pattern-Analyse

**Pattern finden, bevor du fixst:**

1. **Funktionierende Beispiele finden**
   - Ähnlichen funktionierenden Code in derselben Codebase finden
   - Was funktioniert, das dem Kaputten ähnelt?

2. **Mit Referenzen vergleichen**
   - Implementierst du ein Pattern, Referenz-Implementation KOMPLETT lesen
   - Nicht überfliegen — jede Zeile
   - Pattern vollständig verstehen, bevor du es anwendest

3. **Unterschiede identifizieren**
   - Was unterscheidet funktionierend von kaputt?
   - Jeden Unterschied listen, so klein er auch sei
   - Nicht annehmen „das kann nicht relevant sein“

4. **Dependencies verstehen**
   - Welche anderen Komponenten braucht das?
   - Welche Settings, Config, Environment?
   - Welche Annahmen macht es?

### Phase 3: Hypothese und Testen

**Wissenschaftliche Methode:**

1. **Einzelne Hypothese formulieren**
   - Klar sagen: „Ich denke X ist die Root Cause, weil Y“
   - Aufschreiben
   - Spezifisch, nicht vage

2. **Minimal testen**
   - KLEINSTE mögliche Änderung zum Testen der Hypothese
   - Eine Variable zur Zeit
   - Nicht mehrere Dinge gleichzeitig fixen

3. **Vor Fortsetzung verifizieren**
   - Hat es funktioniert? Ja → Phase 4
   - Nicht funktioniert? NEUE Hypothese
   - KEINE weiteren Fixes obendrauf

4. **Wenn du es nicht weißt**
   - Sage „Ich verstehe X nicht“
   - Nicht so tun als wüsstest du es
   - Um Hilfe bitten
   - Mehr recherchieren

### Phase 4: Implementation

**Root Cause fixen, nicht das Symptom:**

1. **Failing Test Case erstellen**
   - Einfachste mögliche Reproduktion
   - Automatisierter Test wenn möglich
   - One-off Test-Skript ohne Framework
   - MUSS vor dem Fix existieren
   - `tdd`-Skill für ordentliche failing Tests nutzen

2. **Einzelnen Fix implementieren**
   - Identifizierte Root Cause adressieren
   - EINE Änderung zur Zeit
   - Keine „während ich hier bin“-Verbesserungen
   - Kein gebündeltes Refactoring

3. **Fix verifizieren**
   - Test grün jetzt?
   - Keine anderen Tests kaputt?
   - Issue wirklich gelöst?

4. **Aufräumen**
   - Alle Instrumentation dieser Debug-Session entfernen (Logs, temporäre Diagnostics)
   - Cleanup verifizieren: grep nach Session-Debug-ID oder Instrumentation-Markern
   - Beispiel: `rg "debug-{sessionId}" --type-add 'src:*.{js,ts,cjs,mjs}' -tsrc .`

5. **Wenn der Fix nicht wirkt**
   - STOP
   - Zählen: Wie viele Fixes hast du versucht?
   - Wenn < 3: Zurück zu Phase 1, mit neuen Infos neu analysieren
   - **Wenn >= 3: STOP und Architektur hinterfragen (Schritt 6 unten)**
   - KEIN Fix #4 ohne Architektur-Diskussion

6. **Wenn 3+ Fixes fehlschlugen: Architektur hinterfragen**

   **Pattern für architektonisches Problem:**
   - Jeder Fix zeigt neuen Shared State/Coupling/Problem woanders
   - Fixes brauchen „massives Refactoring“
   - Jeder Fix erzeugt neue Symptome woanders

   **STOP und Grundlagen hinterfragen:**
   - Ist dieses Pattern grundsätzlich sound?
   - „Hängen wir aus Trägheit dran“?
   - Architektur refactoren vs. Symptome weiter fixen?

   **Mit deinem menschlichen Partner besprechen, bevor du weitere Fixes versuchst**

   Das ist KEINE gescheiterte Hypothese — das ist falsche Architektur.

## Red Flags — STOP und Prozess folgen

Wenn du dich denkst:

- „Schneller Fix jetzt, später untersuchen“
- „Probiere einfach X zu ändern und schau ob es wirkt“
- „Mehrere Änderungen, Tests laufen lassen“
- „Test überspringen, ich verifiziere manuell“
- „Es ist wahrscheinlich X, ich fixe das“
- „Ich verstehe es nicht ganz, aber das könnte wirken“
- „Pattern sagt X, aber ich passe es anders an“
- „Hauptprobleme: [listet Fixes ohne Investigation]“
- Lösungen vorschlagen vor Datenfluss-Tracing
- **„Noch ein Fix-Versuch“ (wenn schon 2+ versucht)**
- **Jeder Fix zeigt neues Problem woanders**

**Alles davon heißt: STOP. Zurück zu Phase 1.**

**Wenn 3+ Fixes fehlschlugen:** Architektur hinterfragen (siehe Phase 4.6)

## Signale deines menschlichen Partners, dass du es falsch machst

**Achte auf diese Umleitungen:**

- „Passiert das nicht?“ — Du hast angenommen ohne zu verifizieren
- „Zeigt uns das …?“ — Du hättest Belege sammeln sollen
- „Hör auf zu raten“ — Du schlägst Fixes ohne Verständnis vor
- „Ultrathink this“ — Grundlagen hinterfragen, nicht nur Symptome
- „Wir hängen fest?“ (frustriert) — Dein Ansatz funktioniert nicht

**Wenn du das siehst:** STOP. Zurück zu Phase 1.

## Häufige Rationalisierungen

| Ausrede                                       | Realität                                                                 |
| -------------------------------------------- | ----------------------------------------------------------------------- |
| „Issue ist simpel, brauche keinen Prozess“        | Einfache Issues haben auch Root Causes. Prozess ist bei einfachen Bugs schnell.    |
| „Notfall, keine Zeit für Prozess“             | Systematisches Debugging ist SCHNELLER als Raten.          |
| „Probiere das erst, dann untersuche“      | Erster Fix setzt das Pattern. Von Anfang an richtig.                 |
| „Test schreibe ich nach bestätigtem Fix“ | Ungetestete Fixes halten nicht. Test zuerst beweist es.                       |
| „Mehrere Fixes sparen Zeit“          | Du isolierst nicht, was wirkte. Erzeugt neue Bugs.                             |
| „Referenz zu lang, ich passe Pattern an“ | Teilverständnis garantiert Bugs. Komplett lesen.              |
| „Ich sehe das Problem, ich fixe es“           | Symptome sehen ≠ Root Cause verstehen.                |
| „Noch ein Fix“ (nach 2+ Fehlschlägen)   | 3+ Fehlschläge = architektonisches Problem. Pattern hinterfragen. |

## Kurzreferenz

| Phase                 | Kernaktivitäten                                         | Erfolgskriterium            |
| --------------------- | ------------------------------------------------------ | --------------------------- |
| **1. Root Cause**     | Fehler lesen, reproduzieren, Änderungen prüfen, Belege sammeln | WAS und WARUM verstehen     |
| **2. Pattern**        | Funktionierende Beispiele finden, vergleichen                         | Unterschiede identifizieren        |
| **3. Hypothese**     | Theorie bilden, minimal testen                            | Bestätigt oder neue Hypothese |
| **4. Implementation** | Test erstellen, fixen, verifizieren                               | Bug gelöst, Tests grün    |

## Wenn der Prozess „keine Root Cause“ zeigt

Zeigt systematische Investigation, dass das Issue wirklich environmental, timing-abhängig oder extern ist:

1. Du hast den Prozess abgeschlossen
2. Dokumentiere, was du untersucht hast
3. Passendes Handling (Retry, Timeout, Fehlermeldung)
4. Monitoring/Logging für spätere Investigation

**Aber:** 95 % der „keine Root Cause“-Fälle sind unvollständige Investigation.

## Unterstützende Techniken

Diese Techniken sind Teil des systematischen Debuggings in diesem Verzeichnis:

- **`root-cause-tracing.md`** — Bugs rückwärts durch den Call Stack zur ursprünglichen Trigger-Quelle
- **`defense-in-depth.md`** — Validierung auf mehreren Schichten nach gefundener Root Cause
- **`condition-based-waiting.md`** — Arbiträre Timeouts durch Condition-Polling ersetzen
- **find-polluter** — Für Test-Pollution-Bisection (flaky Tests durch Shared State): `.claude/tools/analysis/find-polluter/find-polluter.sh` (oder `find-polluter.ps1` auf Windows) vom Projekt-Root, um das verschmutzende Test zu isolieren.

**Verwandte Skills:**

- **tdd** — Failing Test Case (Phase 4, Schritt 1)
- **verification-before-completion** — Fix verifizieren vor Erfolgsmeldung

## Real-World Impact

Aus Debugging-Sessions:

- Systematischer Ansatz: 15–30 Minuten zum Fix
- Random-Fixes-Ansatz: 2–3 Stunden Herumprobieren
- First-time-Fix-Rate: 95 % vs. 40 %
- Neue Bugs: Nahe null vs. häufig

## KI-unterstütztes Debugging & moderne Observability (2025+)

### OpenTelemetry: Der neue Stack Trace

Bei verteilten Systemen ersetzen OpenTelemetry-Traces manuelles `echo`/`env`-Belege-Sammeln. Ein Trace zeigt die komplette Request-Journey über Service-Grenzen via Span-IDs und Trace-IDs (W3C Trace Context: `traceparent`/`tracestate`).

**Beleg-Hierarchie bei verteilten Failures (bevorzugt in dieser Reihenfolge):**

```
1. Distributed Traces (OpenTelemetry Spans, korrelierte Trace-IDs)
2. Strukturierte Logs mit Correlation-IDs
3. Metriken mit Timestamps
4. Manuelle Instrumentation (Phase 1 Schritt 4 Bash-Beispiele)
```

**Häufiges Symptom — fragmentierte Traces:**
Jeder Service zeigt eigenen Root-Span, Trace-IDs matchen nicht. Context-Propagation kaputt — Header-Weiterleitung fixen, bevor Business-Logik.

### KI-unterstützte Root-Cause-Analyse

LLM-basierte Debugging-Agents (2025-Pattern) ergänzen Phase 1 durch Lesen von Production-Traces und Korrelation mit Codebase-Kontext für minimale Reproduktionsfälle.

**KI-Unterstützung nutzen für:**

- Hochkomplexe verteilte Failures mit Multi-Service-Blast-Radius
- On-Call-Incidents mit schneller Root-Cause-Identifikation
- Production-Traces in deterministische Test-Reproducers umwandeln

**Phase 1 NICHT überspringen** bei KI-Unterstützung. KI-Vorschläge sind Hypothesen — Phase 3 (Hypothesen-Test) vor jedem KI-suggested Fix. KI ersetzt keine systematische Investigation; sie beschleunigt Belege-Sammlung.

## Anti-Patterns

| Anti-Pattern                                 | Warum es scheitert                                                                      | Korrekter Ansatz                                                     |
| -------------------------------------------- | --------------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| „Schneller Fix jetzt, später untersuchen“       | Quick Fix wird permanent; Root Cause kommt als anderes Symptom zurück | Immer Phase 1 vor Production-Code              |
| Mehrere Änderungen gleichzeitig              | Unklar, welche Änderung half oder brach; Regressionen       | Eine Änderung pro Hypothesen-Test; verifizieren vor der nächsten        |
| KI-suggested Fixes ohne Test | KI-Vorschläge sind Hypothesen, keine Fakten; blind anwenden überspringt Phase 3     | KI-Vorschläge als Hypothesen testen, nicht als Antworten implementieren |
| 4. Fix nach 3 Fehlschlägen        | N+1 Fix-Versuche auf kaputtem Ansatz verschlimmern das Problem                        | Nach 3 Fehlschlägen Architektur-Review                |
| Failing Test vor Fix überspringen     | Fix nicht verifizierbar, Regressionen unsichtbar                    | Failing Test zuerst; beweist Root Cause und verifiziert Fix |

## Memory-Protokoll (PFLICHT)

**Vor Start:**
`.claude/context/memory/learnings.md` lesen

**Nach Abschluss:**

- Neues Pattern → `.claude/context/memory/learnings.md`
- Issue gefunden → `.claude/context/memory/issues.md`
- Entscheidung → `.claude/context/memory/decisions.md`

> UNTERBRECHUNG ANNEHMEN: Wenn es nicht in Memory steht, ist es nicht passiert.
