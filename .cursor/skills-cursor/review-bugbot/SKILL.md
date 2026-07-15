---
name: review-bugbot
description: Prüft Code-Änderungen mit dem Bugbot-Subagenten.
---
# Review Bugbot

Verwende diesen Skill, wenn der Nutzer `/review-bugbot` ausführen möchte.

Starte genau einen `bugbot`-Subagenten mit:

- `readonly: true`
- `run_in_background: false`, es sei denn, explizit Background gewünscht
- `description: "Bugbot"`
- `subagent_type: "bugbot"`

Der Review-Subagent berechnet den lokalen Diff aus dem Repository-Pfad — berechne den Diff nicht selbst vor dem Start. Der Repository-Pfad sollte der aktive Workspace oder Repository-Root für den zu reviewenden Code sein.

Standardmäßig leitet der Review-Subagent den tatsächlichen Base-Branch des Repos ab (z. B. `main`) bei `branch changes`. In den meisten Fällen kein `Base Branch` angeben. Nur angeben, wenn du weißt, dass der aktuelle Branch oder PR gegen einen anderen Branch als den Default-Base verglichen werden soll — z. B. wenn du den aktuellen Branch von einem anderen Branch abgezweigt hast.

Sonderfall: Wenn der Nutzer explizit einen bestimmten PR oder Branch reviewen möchte, stelle sicher, dass das Ziel vor dem Subagent-Start ausgecheckt ist:
- Beispiele: `github.com/... /review`, `review {link}` oder `review {branch-name}`.
- Löse den PR-Link, die PR-Nummer oder den Branch-Namen zum PR-Head-Branch oder benannten Branch auf.
- Prüfe, ob der Ziel-Branch bereits der lokal ausgecheckte Branch ist. Wenn ja, weiter.
- Wenn ein anderer Branch ausgecheckt ist, versuche zum Ziel-Branch zu wechseln.
- Wenn Git den Wechsel verweigert (überschriebene lokale Dateien, Konflikte, anderer Checkout-Blocker), erkläre den Blocker und frage, ob der Nutzer lokale Änderungen stashen möchte, bevor du es erneut versuchst.
- Stashe nur nach Bestätigung. Bei erfolgreichem Stash erneut zum Ziel-Branch wechseln.
- Starte den Review-Subagenten erst, wenn der Ziel-Branch lokal ausgecheckt ist.

Verwende exakt diese Prompt-Form:

```text
Full Repository Path: <absolute repository path>
Diff: <one of: "branch changes", "uncommitted changes", "natural language">
Base Branch: <only include this line when reviewing branch changes against a known specific base branch>
Change Description: <required only when Diff is "natural language"; list each changed file and what changed in it>
Custom Instructions: <only include this line when the user gave specific review instructions>
```

Standard: `branch changes` — Branch-Änderungen gegen den Merge-Base mit dem Default/Base-Branch, einschließlich committed, staged und unstaged. Wenn der Nutzer nur uncommitted, lokalen Working Tree, dirty oder noch nicht committede Änderungen reviewen möchte: `uncommitted changes`.

Wenn der Review-Subagent vor Findings fehlschlägt, prüfe den Fehlertext.

- Bei falscher Subagent-Aufruf (fehlender `Full Repository Path`, fehlender `Diff`, falsche Prompt-Form, falscher Subagent-Typ): Korrigiere den Aufruf und versuche einmal sofort erneut.
- Wenn der Subagent meldet, dass er den Diff nicht berechnen konnte (leerer Diff, fehlende Diff-Metadaten): einmal mit `Diff: natural language` erneut versuchen, `Base Branch` weglassen und `Change Description` angeben. Der Subagent liest die Dateien direkt. Nur als letzter Ausweg, nachdem der reguläre diff-basierte Review fehlgeschlagen ist.

  Schreibe die `Change Description` als einen Block pro geänderter Datei: Header `<path> (added|modified|deleted|renamed)` gefolgt von Bullet Points der Änderungen. Zeilennummern oder -bereiche inline, wenn hilfreich und bekannt (z. B. `(L40-58)` oder `around L120`); optional, nicht auf jedem Bullet Pflicht. Beispiel:

  ```text
  src/auth/login.ts (modified):
  - rewrote validateSession (L40-58) to check token expiry before the DB lookup
  - removed the fallback that accepted empty tokens

  src/auth/legacy.ts (deleted)

  src/auth/mfa.ts (added):
  - new verifyMfaCode() (L1-30) that calls the TOTP service and rate-limits attempts
  ```
- Bei jedem anderen Subagent-Fehler: einmal mit derselben Prompt-Form erneut versuchen.
- Wenn derselbe Fehler nach dem Retry bleibt: stoppen. Kurz mitteilen, dass der Review-Subagent nicht abschließen konnte, inkl. kurzem Fehler/Blocker. Nicht weiter retryen.

Nach Abschluss des Subagenten das Ergebnis zusammenfassen:

- Wenn kein Diff gefunden oder leer: dem Nutzer in einem Satz mitteilen, dass es keinen Diff zum Reviewen gab.
- Wenn keine Issues: einzeiliger Status wie „Bugbot fand 2 Findings“ / „Bugbot fand keine Bugs“.
- Bei Issues: kompakte Markdown-Tabelle, eine Zeile pro Finding, nach Severity sortiert (höchste zuerst), exakt diese Spalten: Severity, Location (file:line), Finding. Datei und Zeile in der Location-Spalte als `file:line`.

Findings nicht beheben oder Review nicht erneut ausführen, es sei denn, der Nutzer fragt explizit nach dem nächsten Schritt.
