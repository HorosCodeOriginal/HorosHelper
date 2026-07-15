---
name: split-to-prs
description: >-
  Teilt die aktuelle Arbeit in kleine, reviewbare PRs. Verwenden, wenn der
  Nutzer einen Chat, Änderungssatz, Branch oder PR aufteilen möchte.
---
# In PRs aufteilen

Verwandle einen Haufen Arbeit in ein paar kleine PRs.

## Harte Regeln

- Erstelle keine Branches, committe nicht, pushe nicht und öffne keine PRs, bis der Nutzer den Split-Plan genehmigt hat.
- Verwirf niemals Nutzerarbeit. Keine destruktiven Git-Befehle (`reset --hard`, `clean -fdx`, Branch-Löschung, Force-Push, History-Rewrite) ohne explizite Genehmigung.
- Speichere immer einen wiederherstellbaren Snapshot, bevor du Arbeit verschiebst. Das startet oft mit dirty Work auf `main` — nimm nicht an, dass bereits ein sicherer Branch existiert.
- Stage nur benannte Dateien oder Hunks. Kein `git add .` / `git add -A`.

## 1. Zustand prüfen

Vergleiche die aktuelle Arbeit mit dem Default-Branch des Repos, einschließlich committed und uncommitted Änderungen. Fasse die echten Slices zusammen, die du siehst, und nutze die Chat-Historie, um die Absicht wiederherzustellen.

Bevor du Slices vorschlägst, finde Ownership-Signale für die betroffenen Pfade (`CODEOWNERS`, verschachtelte Ownership-Dateien, `tools/ownership/PRODUCTOWNERS` oder Repo-Äquivalente) und nutze sie, um natürliche Reviewer-Grenzen zu identifizieren.

## 2. Split vorschlagen

Nutze dein Urteil beim Detailgrad. Meist reichen PR-Titel. Füge eine einzeilige Scope-Notiz nur hinzu, wenn ein Titel unklar ist. Zeige ein Mermaid-Diagramm, wenn es mehrere Slices gibt.

Optimiere für reviewer-ausgerichtete PRs mit minimalem unrelated Diff: teile unabhängige Owner oder Concerns, halte eng gekoppelte Änderungen zusammen, und wenn Stacking nötig ist, ordne Foundations vor Consumern.

Standard: unabhängige PRs vom Default-Branch. Stack PRs nur, wenn die Abhängigkeit real ist.

Bitte um Genehmigung, bevor du startest.

## 3. Split ausführen

- Bei uncommitted Work: speichere einen wiederherstellbaren Snapshot ohne den Working Tree zu ändern:

  ```bash
  SHA=$(git stash create "pre-split")
  if [ -n "$SHA" ]; then
    git update-ref "refs/backup/pre-split-$(date +%s)" "$SHA"
  fi
  ```

- Für jeden genehmigten Slice: erstelle einen Branch von der richtigen Base, stage und committe nur die geplanten Dateien oder Hunks, dann push und öffne den PR.

## 4. Zurückmelden

Kurz halten: PR-Titel und URLs, plus was auf dem Start-Branch oder Working Tree übrig bleibt. Lösche den Backup-Ref oder Original-Branch nicht, es sei denn, der Nutzer fragt danach.
