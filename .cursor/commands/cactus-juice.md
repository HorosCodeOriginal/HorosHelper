CACTUS JUICE MODE — „It's the quenchiest!“

Phasierter Agent-Schwarm für **viele kleine, unabhängige Edits** — kein blindes 10-Worker-Chaos.

Tausche Tiefe pro Worker gegen **kontrollierte Parallelität** mit obligatorischer Synthese. Alle Worker nutzen nur `composer-2.5` (nie `composer-2.5-fast` — siehe `model-policy.mdc` und `orchestrator.mdc`).

## Wann nutzen

- 3–12 **unabhängige** Micro-Tasks (je eine Datei/Funktion, kein Shared State)
- Repetitive, mechanische Änderungen über disjunkte Dateien (Renames, Imports, Boilerplate, Lint-Fixes)
- Klare Akzeptanzkriterien pro Task — keine Architekturentscheidungen mitten im Schwarm

## Wann NICHT nutzen

| Situation | Stattdessen |
|-----------|-------------|
| Großes oder mehrdeutiges Feature | `/build` (Aang) |
| Braucht zuerst Planung oder Trade-offs | `/plan` (Sokka) |
| Einzelner Bug mit unklarer Root Cause | `/fix` (Katara) |
| Tasks teilen Dateien oder hängen voneinander ab | Ein Coordinator (`/build`) — sequenzieren, nicht schwärmen |
| Exploration vor Implementierung | `/build` oder ein gebündelter `toph` — keine parallelen blinden Edits |

Wenn du **nicht überlappende Datei-Scopes** für jede Micro-Task zuweisen kannst, stoppen — Cactus Juice ist das falsche Tool.

## Harte Limits (orchestrator-aligned)

| Regel | Limit |
|------|-------|
| Parallele Worker **pro Turn** | **Max. 3** (jeder Agent-Typ — `orchestrator.mdc`) |
| `toph`-Spawns pro Turn (hierarchie-weit) | **Max. 3** gesamt |
| Gleicher Agent-Typ pro Turn | **Max. 3** |
| Datei-Scope-Überlappung | **Verboten** — ein Worker pro Datei (oder disjunktes Modul) |
| Modell | **Nur `composer-2.5`** — nie fast |

Mehr als 3 Tasks? **Phasieren** — nie mehr als 3 parallele Spawns in einem Turn.

## Phasierter Schwarm (Pflicht)

```
DECOMPOSE → EXPLORE (optional) → EXECUTE (≤3 parallel) → SYNTHESIZE → repeat EXECUTE if needed → DONE
```

### 1. Zerlegen

Teile die Anfrage in **3–12 atomare Micro-Tasks**. Jede muss haben:

- **Atomarer Scope** — eine Datei oder eine Funktion; keine überlappenden Pfade mit anderen Tasks
- **Erwartetes Ergebnis** — konkretes Deliverable („Export X hinzufügen“, „Y durch Z ersetzen“)
- **Unabhängigkeit** — ohne Warten auf eine andere Micro-Task abschließbar

Wenn eine Task von einer anderen abhängt, **sequenziere sie** in einer späteren Execute-Phase — nie Abhängigkeiten parallelisieren.

### 2. Explore (optional, ein Batch)

Wenn Worker zuerst Pattern-Kontext brauchen:

- Spawne **einen** gebündelten `toph` (`model: composer-2.5`) mit allen Pfaden/Patterns — nicht einen `toph` pro Datei
- Findings festhalten; in Execute-Phase-Worker-Prompts injizieren
- Respektiere das **3 `toph`/turn** Hierarchy-Cap — ein gebündelter Explore-Dispatch zählt als einer

### 3. Execute (≤3 parallel pro Turn)

Für jeden Batch von bis zu **3** Micro-Tasks:

1. Spawne Worker (`momo` für Edits, `toph` nur für read-only scoped search) — **ein Worker pro Micro-Task**
2. Jeder Worker-Prompt **muss** das 6-Abschnitt-Delegationsformat nutzen (`orchestrator.mdc` / `team-avatar.md`):

```
1. TASK: Atomic, specific goal
2. EXPECTED OUTCOME: Concrete deliverables
3. REQUIRED TOOLS: Explicit tool whitelist
4. MUST DO: Exhaustive requirements
5. MUST NOT DO: Forbidden actions (no scope creep, no other files)
6. CONTEXT: File path, exact change, constraints below
```

3. **Worker Constraints** (unten) in jeden Prompt aufnehmen
4. Auf alle Worker im Batch warten; **nicht** den nächsten Batch spawnen, bis der aktuelle gesammelt ist

Verbleibende Micro-Tasks → nächste Execute-Phase (neuer Turn oder nach Collection).

### 4. Synthese (Pflicht — Parent besitzt Integration)

**Bevor du „fertig“ meldest**, MUSS der Coordinator:

1. **Mental mergen** — alle Worker-Outputs abgleichen; Konflikte oder Lücken notieren
2. **Jedes Ergebnis verifizieren** — Änderungen lesen; nicht blind vertrauen
3. **Integrations-Pass** — Cross-File-Inkonsistenzen, Imports, Types, Naming inline fixen
4. **Self-verify** — `ReadLints` auf allen betroffenen Dateien; Build/Tests falls zutreffend
5. **Zusammenfassen** — was geändert wurde, welche Dateien, offene Punkte

Synthese überspringen ist ein Fehler. Der Parent-Coordinator besitzt das integrierte Ergebnis — nicht die Worker.

### 5. Fehler inline beheben

Wenn ein Worker fehlschlägt oder unvollständig liefert: inline fixen oder diesen Worker `resume`n — keinen frischen Agenten für denselben Scope spawnen (`team-avatar.md`).

## Worker Constraints (in jeden Worker-Prompt aufnehmen)

- Du hast **EINE** Task. Erledige sie und kehre zurück.
- Schreibe Code mit **niedriger kognitiver Komplexität**:
  - Kurze Funktionen (max. 20 Zeilen)
  - Minimale Verschachtelung (max. 2 Ebenen tief)
  - Keine Ternary-Ketten
  - Early Returns statt verschachtelter ifs
  - Keine cleveren Tricks — lesbar schlägt clever
- Explore nicht über deinen zugewiesenen Datei-/Scope hinaus.
- Stelle keine Fragen — triff vernünftige Annahmen und markiere Bedenken in deiner Rückgabe.
- Zurückgeben: was du geändert hast, welche Datei(en) und Bedenken für den Synthesis-Schritt des Coordinators.

## Quality Gates (Coordinator-Checkliste)

- [ ] Jede Micro-Task hat nicht überlappenden Datei-Scope
- [ ] Nicht mehr als **3** parallele Spawns in diesem Turn
- [ ] Alle Worker nutzten `model: composer-2.5` (nie fast)
- [ ] Synthese + Integrations-Pass abgeschlossen
- [ ] `ReadLints` sauber auf geänderten Dateien
- [ ] Build/Tests bestehen (oder vorbestehende Fehler dokumentiert)

## Sequenzierungs-Regeln

- Abhängigkeiten → spätere Execute-Phase, nie im selben Batch
- Verwandte Exploration → **ein** gebündelter `toph`, nicht N parallele Suchen
- Dedup vor Dispatch — denselben Scope nicht erneut spawnen (`orchestrator.mdc`)
- Im Zweifel: weniger parallele Worker, stärkere Synthese
