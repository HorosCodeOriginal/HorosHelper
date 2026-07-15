# Refactoring Workflow

Detaillierte Referenz für wann und wie du sicher refactorierst. Die Disziplin des Refactorings ist so wichtig wie die einzelnen Transformationen zu kennen. Diese Referenz deckt den Refactoring-Zyklus, Timing, Sicherheitstechniken und Strategien für groß angelegtes Refactoring in Produktionssystemen ab.

---

## Der Refactoring-Zyklus

Jedes Refactoring folgt derselben Vier-Schritte-Schleife:

```
1. Run tests → GREEN
2. Apply one small structural change
3. Run tests → GREEN
4. Commit
```

**Dann wiederholen.**

### Warum kleine Schritte wichtig sind

| Ansatz | Risiko | Wiederherstellungszeit |
|----------|------|---------------|
| Ein Refactoring nach dem anderen | Minimal — bei Testfehler ist die Ursache offensichtlich | Sekunden (eine Änderung revertieren) |
| Mehrere Refactorings zwischen Tests | Mittel — Debuggen, welches gebrochen hat | Minuten |
| Big-Bang-Rewrite | Maximum — strukturelle und Verhaltensänderung gemischt | Stunden bis Tage (oder nie) |

**Regel:** Wenn nach einem Refactoring-Schritt ein Test fehlschlägt, **sofort revertieren**. Nicht debuggen. Der Schritt war zu groß oder falsch. Revertieren, nachdenken, kleineren Schritt versuchen.

### Die zwei Hüte

Martin Fowler beschreibt zwei getrennte Arbeitsmodi. Du trägst nur einen „Hut“ zur Zeit:

| Hut | Was du tust | Was du nicht tust |
|-----|-------------|-------------------|
| **Refactoring** | Struktur ändern, Verhalten identisch halten | Features hinzufügen, Bugs fixen, Tests ändern |
| **Adding Function** | Neues Verhalten hinzufügen, neue Tests schreiben | Bestehende Code-Struktur ändern |

**Hüte wechseln:** Du kannst häufig wechseln, aber nie beide gleichzeitig. Typische Sequenz:

1. Refactoring-Hut: umstrukturieren, damit das neue Feature leicht hinzuzufügen ist. Commit.
2. Adding-Function-Hut: Feature und Tests hinzufügen. Commit.
3. Refactoring-Hut: aufräumen, was das neue Feature hinterlassen hat. Commit.

---

## Wann refactorieren

### Preparatory Refactoring (Refactorieren, um die Änderung leicht zu machen)

**Trigger:** Du willst ein Feature hinzufügen, und der Code ist nicht strukturiert, um es leicht aufzunehmen.

**Beispiel:** Du brauchst eine neue Zahlungsmethode. Die Payment-Logik steckt in einer langen if/else-Kette. Bevor du den neuen Zweig hinzufügst, refactoriere zu Replace Conditional with Polymorphism. Jetzt heißt neue Zahlungsmethode: eine neue Klasse erstellen.

**Kent Becks Zitat:** „Make the change easy (warning: this may be hard), then make the easy change.“

**Der Nutzen:** Das Feature ist schneller hinzuzufügen, weniger fehleranfällig, und das Refactoring verbessert den Code für alle zukünftigen Änderungen, nicht nur diese eine.

### Comprehension Refactoring (Refactorieren zum Verstehen)

**Trigger:** Du liest Code und kämpfst beim Verstehen. Variablen umbenennen, Methoden extrahieren, umstrukturieren, damit der Code seine Absicht ausdrückt.

**Beispiel:** Du triffst auf eine Funktion `calc` mit Variablen `a`, `b` und `temp`. Während du herausfindest, was jede tut, benennst du um: `calculateMonthlyPayment`, `principal`, `interestRate`, `monthlyAmount`. Das Verständnis, das du gewinnst, steckt im Code selbst.

**Ward Cunninghams Einsicht:** „By refactoring, I move the understanding from my head into the code itself.“

### Litter-Pickup Refactoring (Boy Scout Rule)

**Trigger:** Du fasst eine Datei aus irgendeinem Grund an und siehst eine kleine Verbesserung. Mach sie.

**Beispiele:**
- Irreführenden Variablennamen umbenennen
- Methode aus langer Funktion extrahieren
- Toten Code entfernen
- Fehlende Guard Clause hinzufügen

**Die Regel:** Code sauberer hinterlassen als du ihn gefunden hast. Jede kleine Verbesserung summiert sich. Eine Codebase, die jeder Entwickler beim Anfassen aufräumt, bleibt gesund.

### Rule of Three

**Trigger:** Das dritte Mal, dass du duplizierten Code oder ein wiederholtes Pattern siehst.

**Der Verlauf:**
1. Erstes Mal: schreiben
2. Zweites Mal: über die Duplikation stöhnen, aber tolerieren
3. Drittes Mal: refactorieren — gemeinsames Pattern extrahieren

**Warum drei, nicht zwei:** Vorzeitige Abstraktion ist so gefährlich wie Duplikation. Zwei Vorkommen können Zufall sein. Drei bestätigen das Pattern.

### Long-Term Refactoring

**Trigger:** Ein großes Strukturproblem, das nicht in einer Session fixbar ist.

**Beispiele:**
- Library oder Framework ersetzen
- Monolith in Module splitten
- Verbreitete Datenrepräsentation ändern

**Ansatz:** Das Team einigt sich auf Zielarchitektur. Jeder macht kleine Schritte dorthin während regulärer Arbeit. Niemand stoppt Feature-Entwicklung für einen „Refactoring-Sprint“.

---

## Wann NICHT refactorieren

Nicht jeder Code verdient Refactoring. Spare Mühe für Code, der es rechtfertigt.

### Code, den du in Ruhe lassen solltest

| Situation | Why |
|-----------|-----|
| Der Code funktioniert und niemand muss ihn ändern | Hinter sauberer Schnittstelle kostet internes Chaos nichts |
| Rewrite von Grund auf ist einfacher | Wenn der Code klein ist und Rewrite straightforward, nicht polieren, was du ersetzt |
| Keine Tests und Hinzufügen ist unpraktisch | Refactoring ohne Tests zu riskant; zuerst Characterization Tests erwägen |
| Der Code wird bald gelöscht | Schönen Code mit bekanntem End-of-Life nicht verschönern |
| Du explorierst oder prototypst | Wegwerf-Code profitiert von Geschwindigkeit, nicht Struktur |

### Die „Messy Middle“-Falle

Manche Teams schwingen zwischen Extremen:
- **Nie refactorieren:** Technische Schulden sammeln sich, bis Entwicklung stockt
- **Immer refactorieren:** Code gold-platen, der es nicht braucht; Features langsam shippen

Die richtige Balance: **Code refactorieren, den du gleich ändern wirst, oder der die Velocity aktiv schädigt.** Nicht refactorieren nur weil er nicht schön ist.

---

## Testing und Refactoring

### Das Sicherheitsnetz

Tests sind für Refactoring nicht optional. Ohne sie kannst du nicht verifizieren, dass Verhalten erhalten bleibt.

| Test Type | Role in Refactoring |
|-----------|-------------------|
| Unit tests | Schnelles Feedback zu einzelnem Methodenverhalten |
| Integration tests | Verhalten über zusammenarbeitende Objekte verifizieren |
| Characterization tests | Bestehendes Verhalten von Legacy-Code erfassen (Startpunkt) |
| Regression tests | Gesamtsystem funktioniert nach Änderungen noch |

### Characterization Tests

Wenn du Code ohne Tests triffst, den du refactorieren musst:

1. Code mit bekannten Inputs laufen lassen
2. Tatsächliche Outputs beobachten (auch wenn du sie für „falsch“ hältst)
3. Tests schreiben, die das aktuelle Verhalten asserten
4. Jetzt hast du ein Sicherheitsnetz — refactoriere frei

**Beispiel:**
```python
def test_weird_edge_case():
    # This behavior may be "wrong" but it's what exists.
    # Capture it so refactoring doesn't accidentally change it.
    result = calculate_shipping(weight=0, distance=100)
    assert result == 5.99  # Captures existing behavior
```

### Test-Driven Refactoring Steps

1. **Vor dem Start:** Alle Tests laufen lassen. Schlägt einer fehl, zuerst fixen.
2. **Nach jedem Refactoring-Schritt:** Tests laufen lassen. Alle müssen grün sein.
3. **Wenn ein Test fehlschlägt:** Sofort revertieren. Nicht debuggen.
4. **Nach Abschluss einer logischen Refactoring-Gruppe:** Committen.
5. **Wenn du während Refactoring einen Bug findest:** Refactoring stoppen. Bug fixen (Adding-Function-Hut). Dann Refactoring fortsetzen.

---

## Refactoring und Performance

### Die häufige Angst

„Machen all diese kleinen Methoden und Indirection den Code nicht langsamer?“

### Die Realität

1. Die meisten Performance-Bedenken zu refactoriertem Code sind unbegründet. Moderne Compiler und Runtimes inlinen kleine Methoden.
2. Performance-Engpässe sind fast nie dort, wo du denkst. Zuerst profilen.
3. Gut strukturierter Code ist **leichter** zu optimieren, weil der Hot Path isoliert ist.

### Die Drei-Schritte-Performance-Strategie

1. **Zuerst klaren Code schreiben.** Nicht während Refactoring optimieren.
2. **Laufendes System profilen.** Tatsächlichen Engpass finden (meist 10 % des Codes verursachen 90 % des Problems).
3. **Nur den gemessenen Hot Path optimieren.** Gut refactorierter Code macht das einfach, weil der Hot Path in einer kleinen, isolierten Methode liegt.

### Wenn Refactoring Performance wirklich schadet

| Refactoring | Potential Cost | Mitigation |
|-------------|---------------|------------|
| Replace Temp with Query | Methode mehrfach statt einmal gecacht aufgerufen | Cachen, wenn Profiling Impact zeigt |
| Extract Method | Zusätzlicher Methodenaufruf-Overhead | Meist vom Compiler/JIT inlined |
| Replace Conditional with Polymorphism | Virtual Dispatch statt Branch | In den meisten Fällen vernachlässigbar; bei Zweifel profilen |
| Introduce Parameter Object | Objektallokation pro Aufruf | Oft wegoptimiert; ggf. Pooling |

**Kern-Einsicht:** Optimierung und Refactoring sind getrennte Anliegen. Zuerst für Klarheit refactorieren, dann den gemessenen Engpass optimieren.

---

## Branch by Abstraction

Technik für groß angelegte Änderungen an weit genutzter Komponente ohne lang lebenden Feature-Branch.

### Wann nutzen

- Framework, Library oder große interne Komponente ersetzen
- Ersatz dauert Wochen oder Monate
- Du musst während der Transition Features shippen
- Feature-Branches würden stale werden und Merge-Konflikte verursachen

### Wie es funktioniert

```
Step 1: Identify the component to replace (OldComponent)
Step 2: Create an abstraction layer (interface) that wraps OldComponent
Step 3: Change all callers to use the abstraction (deploy incrementally)
Step 4: Create NewComponent that implements the same abstraction
Step 5: Switch the abstraction to point to NewComponent (one change, deploy)
Step 6: Remove OldComponent and the abstraction layer (clean up)
```

### Beispiel

**Step 1-2:** Abstraktion einführen
```python
# Before: callers use OldPaymentGateway directly
class OldPaymentGateway:
    def charge(self, amount, card): ...

# After: introduce abstraction
class PaymentGateway(ABC):
    @abstractmethod
    def charge(self, amount, card): ...

class OldPaymentGateway(PaymentGateway):
    def charge(self, amount, card): ...  # existing implementation
```

**Step 3:** Caller auf `PaymentGateway` (Abstraktion) migrieren. Deploy.

**Step 4:** `NewPaymentGateway(PaymentGateway)` bauen. Gründlich testen.

**Step 5:** Wiring umschalten:
```python
# In configuration:
# gateway = OldPaymentGateway()  # old
gateway = NewPaymentGateway()    # new
```

**Step 6:** `OldPaymentGateway` löschen. Abstraktion optional inlinen, wenn nur noch eine Implementierung bleibt.

---

## Parallel Change (Expand-Migrate-Contract)

Technik für sichere Breaking-API-Änderungen durch paralleles Betreiben alter und neuer Version.

### Wann nutzen

- Weit genutzte Methode umbenennen oder Signatur ändern
- Datenformat ändern, während Consumer noch altes Format lesen
- Von einer API zur anderen migrieren, wenn nicht alle Consumer gleichzeitig aktualisiert werden können

### Die drei Phasen

**1. Expand:** Neue Version neben der alten hinzufügen.
```python
class User:
    def get_full_name(self):     # new name
        return f"{self.first} {self.last}"

    def getFullName(self):       # old name, still works
        return self.get_full_name()  # delegates to new
```

**2. Migrate:** Alle Caller auf die neue Version umstellen. Kann inkrementell über mehrere Deployments passieren.

**3. Contract:** Alte Version entfernen, sobald alle Caller migriert sind.
```python
class User:
    def get_full_name(self):     # only the new version remains
        return f"{self.first} {self.last}"
```

### Parallel Change für Daten

```
1. Expand: Write to both old and new columns/formats
2. Migrate: Update all readers to use the new format
3. Contract: Stop writing the old format, remove old column
```

---

## Large-Scale Refactoring Strategies

### The Strangler Fig Pattern

Legacy-System schrittweise ersetzen, indem neue Funktionalität drumherum gebaut und immer mehr Traffic auf das neue System geleitet wird, bis das alte abgeschaltet werden kann.

| Phase | Action |
|-------|--------|
| 1. Intercept | Routing-Layer vor dem Legacy-System platzieren |
| 2. Build new | Neue Komponenten hinter dem Router implementieren |
| 3. Redirect | Requests auf neue Komponenten leiten, sobald sie bereit sind |
| 4. Retire | Alte Komponenten abschalten, wenn kein Traffic mehr ankommt |

### Mikado Method

Für komplexe Refactorings mit vielen Abhängigkeiten:

1. Das gewünschte Refactoring versuchen
2. Wenn es bricht, notieren, was zuerst geändert werden muss (Voraussetzungen)
3. Änderung revertieren
4. Voraussetzungen rekursiv fixen (jede kann eigene haben)
5. Abhängigkeitsgraph bauen (der „Mikado Graph“)
6. Graph von den Blättern (Tasks ohne Abhängigkeit) zur Wurzel lösen

### Feature Toggles während Refactoring

Feature Flags nutzen, um refactorierte Komponente schrittweise auszurollen:

```python
if feature_flag('new_pricing_engine'):
    return new_pricing_engine.calculate(order)
else:
    return old_pricing_engine.calculate(order)
```

Das erlaubt:
- Inkrementelles Rollout (10 % Traffic, dann 50 %, dann 100 %)
- Sofortiges Rollback per Flag
- A/B-Vergleich alt vs. neu

---

## Refactoring Checklist

Diese Checkliste vor, während und nach Refactoring-Sessions nutzen:

### Before Starting

- [ ] Alle bestehenden Tests grün
- [ ] Du hast den konkreten Smell oder Verbesserungsziel identifiziert
- [ ] Du kannst die Refactoring(s) benennen, die du anwendest
- [ ] Der Bereich hat Test-Coverage (ggf. Characterization Tests hinzufügen)

### During Refactoring

- [ ] Jeder Schritt ist die kleinstmögliche Transformation
- [ ] Tests nach jedem Schritt
- [ ] Bei Test-Fail sofort revertieren (nicht debuggen)
- [ ] Du trägst nur den Refactoring-Hut (keine neuen Features)
- [ ] Commit nach jeder logischen Gruppe von Schritten

### After Completing

- [ ] Alle Tests noch grün
- [ ] Der Code ist leichter lesbar als vorher
- [ ] Variablen- und Methodennamen zeigen Intent
- [ ] Keine unnötigen Kommentare mehr (Code erklärt sich selbst)
- [ ] Keine neuen Smells eingeführt
- [ ] Änderungen mit klarer Refactoring-Message committed
