---
name: refactoring-patterns
description: 'Benannte Refactoring-Transformationen anwenden, um die Code-Struktur ohne Verhaltensänderung zu verbessern. Nutzen wenn der User „refactor this“, „code smells“, „extract method“, „replace conditional“ oder „technical debt“ sagt. Deckt smell-gesteuertes Refactoring, sichere Transformationssequenzen und Test-Guards ab. Für Code-Qualitätsgrundlagen siehe clean-code. Für Komplexitätsmanagement siehe software-design-philosophy.'
license: MIT
metadata:
  author: wondelai
  version: "1.0.0"
---

# Refactoring Patterns Framework

Ein disziplinierter Ansatz zur Verbesserung der internen Struktur bestehenden Codes ohne Änderung des beobachtbaren Verhaltens. Wende diese benannten Transformationen beim Code-Review, beim Abbau technischer Schulden oder beim Vorbereiten von Code für neue Features an. Jedes Refactoring folgt derselben Schleife: Tests prüfen, eine kleine strukturelle Änderung anwenden, Tests erneut prüfen.

## Kernprinzip

**Refactoring ist kein Rewrite. Es ist eine Sequenz kleiner, verhaltenserhaltender Transformationen, jede durch Tests abgesichert.** Du änderst nie, was der Code tut — du änderst, wie der Code organisiert ist. Die Disziplin kleiner verifizierter Schritte macht Refactoring sicher. Big-Bang-Rewrites scheitern, weil sie strukturelle und Verhaltensänderung kombinieren — dann ist unmöglich zu wissen, was etwas kaputt gemacht hat.

**Das Fundament:** Schlechter Code ist kein Charakterfehler — er ist eine natürliche Folge von Feature-Lieferung unter Zeitdruck. Code Smells sind objektive Signale, dass die Struktur degradiert ist. Benannte Refactorings sind die bewährten mechanischen Rezepte für jeden Smell. Der Smell-Katalog sagt dir, *wo* du schauen sollst; der Refactoring-Katalog sagt dir, *was* du tun sollst.

## Bewertung

**Ziel: 10/10.** Beim Review oder Refactoring strukturelle Qualität 0–10 bewerten nach Einhaltung der Prinzipien unten. 10/10 bedeutet: keine offensichtlichen Smells mehr, jede Funktion macht eine Sache, Namen zeigen Intent, Duplikation ist eliminiert, die Test-Suite deckt die refactorierten Pfade ab. Immer aktuelle Punktzahl und konkrete Refactorings für 10/10 nennen.

## Das Refactoring Patterns Framework

Sechs Schwerpunkte zur systematischen Verbesserung der Code-Struktur:

### 1. Code Smells als Trigger

**Kernkonzept:** Code Smells sind Oberflächenindikatoren tieferer Strukturprobleme. Sie sind keine Bugs — der Code funktioniert — aber sie signalisieren, dass das Design den Code schwerer verständlich, erweiterbar oder wartbar macht. Jeder Smell mappt auf ein oder mehrere benannte Refactorings, die ihn beheben.

**Warum es funktioniert:** Ohne gemeinsames Smell-Vokabular verkommt Code-Review zu subjektivem „Das gefällt mir nicht.“ Benannte Smells geben Teams objektive Kriterien: „Das ist Feature Envy — die Methode nutzt sechs Felder einer anderen Klasse und nur eines der eigenen.“ Der Name zeigt direkt auf den Fix.

**Wichtige Erkenntnisse:**
- Smells clustern in fünf Familien: Bloaters, Object-Orientation Abusers, Change Preventers, Dispensables und Couplers
- Long Method ist der häufigste Smell und das Tor zu den meisten anderen Refactorings
- Duplicate Code ist der größte Treiber von Wartungskosten
- Eine Methode, die einen Kommentar braucht, um zu erklären, *was* sie tut, ist ein Smell — extrahiere und benenne den Block stattdessen
- Shotgun Surgery (eine Änderung erfordert Edits in vielen Klassen) und Divergent Change (eine Klasse ändert sich aus vielen Gründen) sind Gegensätze, die beide falsch platzierte Verantwortlichkeiten signalisieren
- Primitive Obsession — rohe Strings, ints oder Arrays statt kleiner Domain-Objekte — verursacht Fehler und Duplikation in der gesamten Codebase

**Code-Anwendungen:**

| Kontext | Pattern | Beispiel |
|---------|---------|---------|
| **Method > 10 lines** | Extract Method | Schleifenkörper in `calculateLineTotal()` ziehen |
| **Class > 200 lines** | Extract Class | Shipping-Logik in `ShippingCalculator` verschieben |
| **Switch on type code** | Replace Conditional with Polymorphism | Subklassen für jeden Order-Typ erstellen |
| **Multiple methods use same params** | Introduce Parameter Object | `startDate, endDate` in `DateRange` gruppieren |
| **Method uses another object's data** | Move Method | `calculateDiscount()` in die `Customer`-Klasse verschieben |
| **Copy-pasted logic** | Extract Method + Pull Up Method | Über gemeinsame Methode oder Basisklasse teilen |

Siehe: [references/smell-catalog.md](references/smell-catalog.md)

### 2. Composing Methods

**Kernkonzept:** Die meisten Refactorings starten hier. Lange Methoden werden in kleinere, gut benannte Teile zerlegt. Jedes extrahierte Stück soll eine Sache tun und der Name soll sagen, welche. Ziel: Methoden wie Prosa lesbar — eine Sequenz hochleveliger Schritte, jeder delegiert an einen klar benannten Helper.

**Warum es funktioniert:** Kurze Methoden mit intention-revealing Namen eliminieren Kommentarbedarf, machen Bugs offensichtlich (jede Methode ist klein genug auf einen Blick zu prüfen) und ermöglichen Wiederverwendung. Die kognitive Kosten eines Methodenaufrufs sind nahe null, wenn der Name alles sagt.

**Wichtige Erkenntnisse:**
- Extract Method ist das wichtigste Refactoring — zuerst meistern
- Wenn du einen Kommentar schreiben willst, extrahiere den Codeblock und nutze den Kommentar als Methodennamen
- Inline Method, wenn ein Methodenbody so klar ist wie der Name — Indirection ohne Wert ist Rauschen
- Replace Temp with Query, wenn eine temporäre Variable einen berechneten Wert hält, der an mehreren Stellen genutzt wird
- Split Temporary Variable, wenn eine Variable für zwei verschiedene Zwecke wiederverwendet wird
- Replace Method with Method Object, wenn eine Methode zu verwickelt zum Extrahieren ist (viele lokale Variablen referenzieren sich gegenseitig)

**Code-Anwendungen:**

| Kontext | Pattern | Beispiel |
|---------|---------|---------|
| **Block with a comment** | Extract Method | `// check eligibility` wird `isEligible()` |
| **Temp used once** | Inline Variable | `const price = order.getPrice()` entfernen, wenn nur einmal genutzt |
| **Temp used in multiple places** | Replace Temp with Query | `let discount = getDiscount()` durch Methodenaufrufe ersetzen |
| **Temp assigned twice for different reasons** | Split Temporary Variable | `perimeterWidth` und `perimeterHeight` einführen |
| **Trivial delegating method** | Inline Method | `moreThanFiveDeliveries()` inlinen, wenn `return deliveries > 5` und nur einmal genutzt |
| **Complex method with many locals** | Replace Method with Method Object | Methode in eigene Klasse verschieben, Locals werden Felder |

Siehe: [references/composing-methods.md](references/composing-methods.md)

### 3. Moving Features Between Objects

**Kernkonzept:** Die Schlüsselentscheidung im OO-Design ist, wo Verantwortlichkeiten liegen. Wenn eine Methode oder ein Feld in der falschen Klasse ist — erkennbar an Feature Envy, übermäßiger Kopplung oder unausgewogener Klassengrößen — verschiebe sie dorthin, wo sie hingehört.

**Warum es funktioniert:** Gut platzierte Verantwortlichkeiten reduzieren Kopplung und erhöhen Kohäsion. Wenn eine Methode in der Klasse lebt, deren Daten sie nutzt, betreffen Datenänderungen nur eine Klasse. Falsch platzierte Methoden erzeugen unsichtbare Abhängigkeiten, die Shotgun Surgery verursachen.

**Wichtige Erkenntnisse:**
- Move Method, wenn eine Methode mehr Features einer anderen Klasse nutzt als der eigenen
- Move Field, wenn ein Feld mehr von einer anderen Klasse genutzt wird als von der Klasse, in der es lebt
- Extract Class, wenn eine Klasse zwei Dinge tut — entlang der Änderungsachse splitten
- Inline Class, wenn eine Klasse zu wenig tut, um ihr Dasein zu rechtfertigen
- Hide Delegate, um das Law of Demeter durchzusetzen — ein Client soll keine Objektkette navigieren
- Remove Middle Man, wenn eine Klasse nur Aufrufe weiterleitet
- Die Spannung zwischen Hide Delegate und Remove Middle Man wird fallweise gelöst: Delegate verstecken, wenn die Kette instabil ist; Middle Man entfernen, wenn Weiterleitung die ganze Klasse wird

**Code-Anwendungen:**

| Kontext | Pattern | Beispiel |
|---------|---------|---------|
| **Method envies another class** | Move Method | `calculateShipping()` von `Order` nach `ShippingPolicy` verschieben |
| **Field used by another class constantly** | Move Field | `discountRate` von `Order` nach `Customer` verschieben |
| **God class with 500+ lines** | Extract Class | `Address`-Felder und -Methoden in eigene Klasse ziehen |
| **Tiny class with one field** | Inline Class | `PhoneNumber` zurück in `Contact` mergen, wenn kein Verhalten |
| **Client calls a.getB().getC()** | Hide Delegate | `a.getCThroughB()` hinzufügen, damit Client C nicht kennt |
| **Class only forwards calls** | Remove Middle Man | Client ruft Delegate direkt auf |

Siehe: [references/moving-features.md](references/moving-features.md)

### 4. Organizing Data

**Kernkonzept:** Rohe Daten — Magic Numbers, exponierte Felder, als Integer dargestellte Type Codes, parallele Arrays — erzeugen subtile Bugs und streuen Domain-Wissen. Ersetze primitive Repräsentationen durch Objekte, die Verhalten kapseln und Invarianten durchsetzen.

**Warum es funktioniert:** Ein `int` für einen Währungsbetrag kennt keine Rundungsregeln, Währungscodes oder Formatierung. Ein `Money`-Objekt kapselt das alles. Wenn Domain-Konzepte als First-Class-Objekte modelliert sind, leben Business-Regeln an einem Ort, Validierung passiert automatisch, und das Typsystem fängt Fehler zur Compile-Zeit ab.

**Wichtige Erkenntnisse:**
- Replace Magic Number with Symbolic Constant als einfachstes Daten-Refactoring — benennt die Absicht
- Replace Data Value with Object (Primitive Obsession Heilung) — Strings und Zahlen in Domain-Objekte wrappen (`EmailAddress`, `Money`, `Temperature`)
- Encapsulate Field — nie ein rohes Feld exponieren; Getter/Setter erlauben später Validierung, Logging oder Berechnung
- Encapsulate Collection — unmodifiable View zurückgeben; Caller dürfen interne Liste nie mutieren
- Replace Type Code with Subclasses, wenn der Type Code Verhalten beeinflusst; Strategy, wenn Subclassing unpraktisch ist
- Change Value to Reference, wenn Identitätssemantik nötig ist (ein geteiltes `Customer`-Objekt, keine Kopien)

**Code-Anwendungen:**

| Kontext | Pattern | Beispiel |
|---------|---------|---------|
| **`if (status == 2)`** | Replace Magic Number with Symbolic Constant | `if (status == ORDER_SHIPPED)` |
| **`String email` passed everywhere** | Replace Data Value with Object | `EmailAddress`-Klasse mit Validierung erstellen |
| **Public field** | Encapsulate Field | `order.total` durch `order.getTotal()` ersetzen |
| **Getter returns mutable list** | Encapsulate Collection | `Collections.unmodifiableList(items)` zurückgeben |
| **`int typeCode` with switch** | Replace Type Code with Subclasses | `Employee` -> `Engineer`, `Manager`, `Salesperson` |
| **Duplicated customer records** | Change Value to Reference | Eine `Customer`-Instanz über Registry teilen |

Siehe: [references/organizing-data.md](references/organizing-data.md)

### 5. Simplifying Conditional Logic

**Kernkonzept:** Komplexe Conditionals — tief verschachtelte if/else-Bäume, lange Switch-Statements, überall verstreute Null-Checks — sind der schwerste Code zum Lesen und der wahrscheinlichste für Bugs. Benannte Refactorings zerlegen, konsolidieren und ersetzen Conditionals durch klarere Strukturen.

**Warum es funktioniert:** Ein Conditional mit sechs Zweigen und verschachtelten Sub-Conditionals zwingt den Leser, jeden Pfad mental zu simulieren. Die Bedingung in gut benannte Methoden zu zerlegen macht jeden Zweig selbstdokumentierend. Conditionals durch Polymorphismus zu ersetzen eliminiert ganze Kategorien von „diesen Fall vergessen“-Bugs.

**Wichtige Erkenntnisse:**
- Decompose Conditional: Bedingung, Then-Branch und Else-Branch in benannte Methoden extrahieren
- Consolidate Conditional Expression: mehrere Bedingungen mit gleichem Ergebnis in einen benannten Check mergen
- Replace Nested Conditional with Guard Clauses: Edge Cases früh behandeln und returnen, Hauptpfad unindented lassen
- Replace Conditional with Polymorphism: Goldstandard für typbasierte Conditionals — jeder Typ kennt sein eigenes Verhalten
- Introduce Special Case (Null Object): `if (x == null)`-Checks eliminieren durch Objekt, das „nichts“ mit sicherem Default-Verhalten repräsentiert
- Introduce Assertion: Annahmen explizit machen, damit sie in der Entwicklung schnell fehlschlagen

**Code-Anwendungen:**

| Kontext | Pattern | Beispiel |
|---------|---------|---------|
| **Long `if` with complex condition** | Decompose Conditional | `isSummer(date)` und `summerCharge()` extrahieren |
| **Multiple `if`s return same value** | Consolidate Conditional | In `isDisabled()` mit early return kombinieren |
| **Deeply nested `if/else`** | Replace with Guard Clauses | Edge Cases zuerst prüfen, early return, Hauptpfad flach |
| **Switch on object type** | Replace Conditional with Polymorphism | Jeder Typ implementiert eigenes `calculatePay()` |
| **`if (customer == null)` everywhere** | Introduce Special Case | `NullCustomer` mit Default-Verhalten erstellen |
| **Hidden assumption in code** | Introduce Assertion | `assert quantity > 0` am Methodeneingang |

Siehe: [references/simplifying-conditionals.md](references/simplifying-conditionals.md)

### 6. Safe Refactoring Workflow

**Kernkonzept:** Refactoring ist nur sicher, wenn es in Tests eingewickelt ist. Der Workflow ist mechanisch: Tests laufen (grün), eine kleine Transformation anwenden, Tests laufen (grün), committen. Wenn Tests rot werden, letzte Änderung revertieren — kein Debuggen eines kaputten Refactorings.

**Warum es funktioniert:** Kleine Schritte machen trivial, was schiefging (es war das Letzte, was du getan hast). Ein fehlgeschlagenen Schritt zu revertieren kostet Sekunden. Ein fehlgeschlagenes Big-Bang-Rewrite zu debuggen kostet Tage. Häufige Commits erzeugen Save Points.

**Wichtige Erkenntnisse:**
- Der Refactoring-Zyklus: test -> refactor -> test -> commit (wiederholen)
- Rule of Three: Duplikation einmal tolerieren, zweimal notieren, beim dritten Mal refactorieren
- Preparatory refactoring: Code umstrukturieren, um das Feature leicht zu machen, *bevor* du das Feature hinzufügst
- Comprehension refactoring: refactorieren, um Code beim Lesen zu verstehen — klarer hinterlassen als gefunden
- Litter-pickup refactoring: kleine Verbesserungen, wann immer du eine Datei anfasst (Boy Scout Rule)
- Wann NICHT refactorieren: wenn Rewrite von Grund auf einfacher ist, wenn keine Tests und Hinzufügen zuerst nicht machbar, oder wenn der Code bald gelöscht wird
- Refactoring und Performance: zuerst für Klarheit refactorieren, dann profilen und den gemessenen Engpass optimieren — refactorierter Code ist leichter zu tunen, weil der Hot Path isoliert ist
- Branch by Abstraction und Parallel Change ermöglichen große Refactorings in Produktionssystemen ohne Feature-Branches

**Code-Anwendungen:**

| Kontext | Pattern | Beispiel |
|---------|---------|---------|
| **About to add a feature** | Preparatory Refactoring | Methode extrahieren, damit Einfügepunkt für neues Feature sauber ist |
| **Reading unfamiliar code** | Comprehension Refactoring | Variablen umbenennen und Methoden extrahieren, um Intent zu verstehen |
| **Saw a small issue while working** | Litter-Pickup Refactoring | Smell fixen, bevor du weitermachst (Boy Scout Rule) |
| **Third copy of same logic** | Rule of Three | Gemeinsame Logik in gemeinsame Methode extrahieren |
| **Large API change in production** | Branch by Abstraction | Abstraktionsschicht einführen, Caller migrieren, alten Pfad entfernen |
| **Renaming a widely-used method** | Parallel Change | Neuen Namen hinzufügen, alten deprecaten, Caller migrieren, alten entfernen |

Siehe: [references/refactoring-workflow.md](references/refactoring-workflow.md)

## Häufige Fehler

| Fehler | Warum es scheitert | Fix |
|---------|-------------|-----|
| Refactoring ohne Tests | Kein Sicherheitsnetz — du siehst nicht, ob sich Verhalten geändert hat | Zuerst Characterization Tests schreiben, dann refactorieren |
| Big-bang rewrite statt inkrementeller Schritte | Kombiniert strukturelle und Verhaltensänderung; unmöglich zu debuggen | Kleinster möglicher Schritt, nach jedem Tests |
| Refactoring und Features gleichzeitig | Zwei Hüte — du kannst keine Änderung isoliert verifizieren | Hüte trennen: zuerst refactorieren (commit), dann Feature (commit) |
| Umbenennen ohne alle Caller zu aktualisieren | Bricht Build oder erzeugt Dead Code | IDE-Rename-Refactoring nutzen; alle Referenzen suchen |
| Zu viele winzige Methoden extrahieren | Indirection ohne Klarheit bei schlechten Namen | Jede extrahierte Methode braucht einen Namen, der das Lesen des Bodys überflüssig macht |
| Smell-Katalog ignorieren | Fixes neu erfinden statt bewährte Rezepte | Benannte Smells lernen; jeder mappt auf konkrete Refactorings |
| Code refactorieren, der gelöscht wird | Verschwendete Mühe — Politur an verurteiltem Code | Zuerst fragen: ist die Lebensdauer lang genug für die Investition? |
| Vorzeitig während Refactoring optimieren | Vermischt Klarheit mit Performance; optimierter Code oft schwerer lesbar | Zuerst für Klarheit refactorieren, dann profilen, dann nur gemessenen Hot Path optimieren |

## Schnell-Diagnose

| Frage | Wenn Nein | Aktion |
|----------|-------|--------|
| Laufen Tests vor dem Start? | Du hast kein Sicherheitsnetz | Zuerst Tests schreiben/fixen — nicht ohne grüne Tests refactorieren |
| Kannst du den Smell benennen, den du fixst? | Du refactorierst nach Instinkt, nicht nach Katalog | Smell aus Katalog identifizieren, dann vorgeschriebenes Refactoring anwenden |
| Ist jede Methode unter ~10 Zeilen? | Long Methods wahrscheinlich vorhanden | Extract Method anwenden, lange Methoden in benannte Schritte zerlegen |
| Hat jede Klasse einen einzigen Änderungsgrund? | Divergent Change oder Large Class Smell | Extract Class anwenden, Verantwortlichkeiten trennen |
| Gibt es duplizierte Codeblöcke? | Duplicate Code ist der teuerste Smell | Gemeinsame Logik in gemeinsame Methode oder Basisklasse extrahieren |
| Nutzen Conditionals Polymorphismus wo passend? | Switch Statements oder komplexe `if/else`-Bäume bleiben | Replace Conditional with Polymorphism anwenden |
| Committest du nach jedem Refactoring-Schritt? | Du riskierst Datenverlust und gemischte Änderungen | Nach jeder grün-zu-grün-Transformation committen |
| Ist der Code nach deiner Änderung leichter lesbar? | Refactoring hat vielleicht Komplexität hinzugefügt | Revertieren und anderen Ansatz versuchen |

## Referenzdateien

- [smell-catalog.md](references/smell-catalog.md): Umfassender Katalog von Code Smells nach Familie — Bloaters, Object-Orientation Abusers, Change Preventers, Dispensables und Couplers — mit Erkennungsheuristiken und Fix-Mappings
- [composing-methods.md](references/composing-methods.md): Extract Method, Inline Method, Extract Variable, Inline Variable, Replace Temp with Query, Split Temporary Variable, Remove Assignments to Parameters, Replace Method with Method Object — Motivation, Mechanik und Beispiele
- [moving-features.md](references/moving-features.md): Move Method, Move Field, Extract Class, Inline Class, Hide Delegate, Remove Middle Man — wann und wie Verantwortlichkeiten zwischen Objekten verteilt werden
- [organizing-data.md](references/organizing-data.md): Replace Data Value with Object, Change Value to Reference, Replace Array with Object, Replace Magic Number, Encapsulate Field, Encapsulate Collection, Replace Type Code with Class/Subclasses/Strategy
- [simplifying-conditionals.md](references/simplifying-conditionals.md): Decompose Conditional, Consolidate Conditional, Replace Nested Conditional with Guard Clauses, Replace Conditional with Polymorphism, Introduce Special Case, Introduce Assertion — mit Vorher/Nachher-Beispielen
- [refactoring-workflow.md](references/refactoring-workflow.md): Der Refactoring-Zyklus, wann refactorieren, wann NICHT refactorieren, Refactoring und Performance, Branch by Abstraction, Parallel Change

## Weiterführende Literatur

Dieser Skill basiert auf dem definitiven Leitfaden zur Verbesserung des Designs bestehenden Codes:

- [*"Refactoring: Improving the Design of Existing Code (2nd Edition)"*](https://www.amazon.com/Refactoring-Improving-Existing-Addison-Wesley-Signature/dp/0134757599?tag=wondelai00-20) von Martin Fowler
- [*"Working Effectively with Legacy Code"*](https://www.amazon.com/Working-Effectively-Legacy-Michael-Feathers/dp/0131177052?tag=wondelai00-20) von Michael Feathers (Begleiter für Code ohne Tests)
- [*"Clean Code: A Handbook of Agile Software Craftsmanship"*](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882?tag=wondelai00-20) von Robert C. Martin (ergänzende Naming- und Style-Prinzipien)

## Über den Autor

**Martin Fowler** ist Chief Scientist bei Thoughtworks und eine der einflussreichsten Stimmen im Software Engineering. Er ist Autor von *Refactoring: Improving the Design of Existing Code* (1999, 2. Auflage 2018), das benannte, katalogbasierte Refactoring-Transformationen in den Mainstream brachte. Fowler ist auch Autor von *Patterns of Enterprise Application Architecture*, *UML Distilled* und zahlreicher einflussreicher Artikel zu Softwaredesign, agiler Methodik und Continuous Delivery. Er war Unterzeichner des Agile Manifesto und setzt sich seit Jahrzehnten für evolutionäres Design ein — kontinuierliche Verbesserung der Code-Struktur durch diszipliniertes, inkrementelles Refactoring statt upfront Big Design. Sein Refactoring-Katalog, ursprünglich in Java geschrieben, wurde auf praktisch jede Programmiersprache adaptiert und ist in die automatischen Refactoring-Tools jeder großen IDE eingebaut.
