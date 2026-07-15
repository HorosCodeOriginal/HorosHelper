---
name: babysit
description: >-
  Hält einen PR merge-bereit, indem Kommentare triagiert, klare Konflikte
  gelöst und CI in einer Schleife repariert wird.
---
# PR babysitten
Deine Aufgabe ist es, diesen PR in einen merge-bereiten Zustand zu bringen.

Prüfe PR-Status, Kommentare und die neueste CI und behebe alle Probleme, bis der PR bereit zum Mergen ist.

1. Merge-Konflikte: Löse Merge-Konflikte intelligent und bewahre dabei die Absicht und Korrektheit der Änderungen auf deinem Branch und dem Base-Branch. Wenn sich die Absichten widersprechen, brich den Merge ab und bitte um Klärung.
2. Kommentare: Prüfe aktive, ungelöste Kommentare (einschließlich Bugbot) und bearbeite Change Requests / Bug-Reports, wo sie berechtigt sind. Beim Abrufen von GitHub-Kommentaren filtere zuerst gelöste Threads heraus. Lies nur den Kommentartext und die minimale Location/URL, die zum Handeln nötig ist; lies nicht die gesamte JSON-Ausgabe oder andere unnötige Payload-Daten. Validiere von Bugbot gemeldete Probleme sorgfältig und handle nur bei gültigen Befunden; erkläre, wenn du nicht zustimmst oder unsicher bist.
3. CI: Behebe CI-Probleme, die durch Änderungen im Scope dieses PRs verursacht wurden. Ändere niemals CI-Checks/Workflows nur, damit Fehler durchlaufen, und mache keine unrelated Code-Änderungen; wenn das nötig wäre, melde dich stattdessen zurück. Bei merge-blockierenden Fehlern, die unrelated zum PR wirken, prüfe, ob der Branch hinter dem Base-Branch liegt, und merge die neuesten Änderungen — ein anderer PR könnte sie bereits behoben haben. Pushe scoped Fixes und beobachte CI erneut, bis mergeable + grün + Kommentare triagiert.
