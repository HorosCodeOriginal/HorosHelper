# Code Smell Catalog

Umfassender Katalog von Code Smells nach Familie. Jeder Smell enthält Beschreibung, Erkennungsheuristiken und die benannten Refactorings, die ihn beheben.

## Was ist ein Code Smell?

Ein Code Smell ist ein Oberflächenindikator, der meist auf ein tieferes Strukturproblem hinweist. Smells sind keine Bugs — der Code funktioniert korrekt — aber sie machen den Code schwerer verständlich, erweiterbar und wartbar. Der Begriff stammt von Kent Beck und wurde von Martin Fowler popularisiert.

**Kernprinzipien:**
- Smells sind Heuristiken, keine Regeln — Urteilsvermögen nutzen
- Ein Smell ohne echtes Problem im Kontext kann bleiben
- Smells clustern: einen zu fixen zeigt oft andere in der Nähe
- Das Mapping „Smell → Refactoring“ ist viele-zu-viele; ein Smell kann mehrere Refactorings brauchen

---

## Familie 1: Bloaters

Smells, bei denen Code zu groß wird, um effektiv damit zu arbeiten.

### Long Method

**Beschreibung:** Eine Methode, die zu viel auf einmal tun will. Je länger eine Methode, desto schwerer zu verstehen, testen und wiederverwenden.

**Erkennungsheuristiken:**
- Mehr als 10–15 Zeilen ausführbaren Codes
- Mehrere Einrückungsebenen
- Kommentare, die „Abschnitte“ innerhalb der Methode trennen
- Mehrere Verantwortlichkeiten auf einen Blick sichtbar
- Schwierigkeit, die Methode zu benennen, weil sie mehrere Dinge tut

**Typische Fixes:**
- Extract Method — jeden logischen Abschnitt in benannte Methode ziehen
- Replace Temp with Query — Temporaries entfernen, die Extraktion blockieren
- Replace Method with Method Object — wenn Extraktion durch verknüpfte lokale Variablen blockiert ist
- Decompose Conditional — wenn die Länge von komplexer Verzweigung kommt

**Beispiel-Smell:**
```
function processOrder(order) {
  // validate order
  if (!order.items || order.items.length === 0) { ... }
  if (!order.customer) { ... }
  // calculate totals
  let subtotal = 0;
  for (const item of order.items) { subtotal += item.price * item.qty; }
  let tax = subtotal * 0.08;
  let shipping = subtotal > 100 ? 0 : 9.99;
  // apply discounts
  if (order.customer.isPremium) { ... }
  // save to database
  db.save({ ...order, subtotal, tax, shipping });
  // send confirmation
  emailService.send(order.customer.email, ...);
}
```

Jeder Kommentarblock sollte eigene Methode sein: `validateOrder()`, `calculateTotals()`, `applyDiscounts()`, `saveOrder()`, `sendConfirmation()`.

### Large Class

**Beschreibung:** Eine Klasse mit zu vielen Feldern, zu vielen Methoden oder zu vielen Verantwortlichkeiten. Oft „God Class“ oder „Blob“ genannt.

**Erkennungsheuristiken:**
- Mehr als 200–300 Zeilen
- Mehr als 10–15 Felder
- Felder, die in Untergruppen clustern (z. B. Adress-, Billing-Felder)
- Methoden, die nur eine Teilmenge der Felder nutzen
- Vager Klassenname (`Manager`, `Handler`, `Processor`, `Utils`)

**Typische Fixes:**
- Extract Class — entlang der Änderungsachse splitten
- Extract Subclass — wenn Verhalten nach Typ variiert
- Replace Data Value with Object — wenn Feldcluster ein Konzept repräsentieren

### Long Parameter List

**Beschreibung:** Eine Methode mit mehr als drei oder vier Parametern — Aufrufe verwirrend und fehleranfällig.

**Erkennungsheuristiken:**
- Mehr als 3–4 Parameter
- Boolean-Parameter, die Verhalten umschalten
- Parameter, die immer zusammen reisen
- Caller übergeben `null` für ungenutzte Parameter

**Typische Fixes:**
- Introduce Parameter Object — verwandte Params in `DateRange`, `Address`, `Options` gruppieren
- Preserve Whole Object — Objekt statt extrahierter Felder übergeben
- Replace Parameter with Method — Methode holt Daten selbst

### Data Clumps

**Beschreibung:** Gruppen von Variablen, die an mehreren Stellen zusammen auftauchen — Methodenparameter, Felddeklarationen oder lokale Variablen.

**Erkennungsheuristiken:**
- Dieselben drei oder mehr Felder in mehreren Klassen
- Dieselbe Parametergruppe in mehreren Methodensignaturen
- Ein Mitglied der Gruppe löschen ergibt ohne die anderen keinen Sinn

**Typische Fixes:**
- Extract Class — neue Klasse für den Clump (`Address`, `DateRange`, `Coordinates`)
- Introduce Parameter Object — Parametergruppen durch neue Klasse ersetzen
- Preserve Whole Object — Objekt statt zerlegter Felder übergeben

### Primitive Obsession

**Beschreibung:** Primitive Typen (Strings, ints, Arrays) für Domain-Konzepte statt kleiner Objekte.

**Erkennungsheuristiken:**
- Konstanten oder Magic Numbers für Typen (`int ADMIN = 1`)
- Strings für strukturierte Daten (Telefon, PLZ, Währungen)
- Arrays oder Tuples statt benannter Strukturen
- Validierungslogik für denselben Typ an mehreren Stellen verstreut

**Typische Fixes:**
- Replace Data Value with Object — `String email` wird `EmailAddress`
- Replace Type Code with Subclasses — wenn Type Code Verhalten antreibt
- Replace Type Code with Strategy — wenn Subclassing unpraktisch
- Replace Magic Number with Symbolic Constant — Namen statt Zahlen

---

## Familie 2: Object-Orientation Abusers

Smells, bei denen OO-Features falsch oder gar nicht genutzt werden.

### Switch Statements

**Beschreibung:** Derselbe switch/case oder if/else auf Type Code an mehreren Stellen. Neuer Typ erfordert Update jedes Switch.

**Erkennungsheuristiken:**
- `switch` auf Type- oder Statusfeld an mehr als einer Stelle
- `if/else if`-Kette mit `instanceof` oder Type-Strings
- Neuer Typ erfordert Edits in mehreren Dateien

**Typische Fixes:**
- Replace Conditional with Polymorphism — jeder Typ implementiert eigenes Verhalten
- Replace Type Code with Subclasses + Replace Conditional with Polymorphism
- Replace Type Code with Strategy, wenn Typ zur Laufzeit wechseln kann

**Beispiel:**
```
// SMELL: same switch in calculatePay(), generateReport(), getPermissions()
switch (employee.type) {
  case 'engineer': return basePay;
  case 'manager': return basePay + bonus;
  case 'salesperson': return basePay + commission;
}

// FIX: polymorphism
class Engineer extends Employee {
  calculatePay() { return this.basePay; }
}
class Manager extends Employee {
  calculatePay() { return this.basePay + this.bonus; }
}
```

### Refused Bequest

**Beschreibung:** Eine Subclass erbt Methoden oder Daten, die sie nicht will. Sie überschreibt Parent-Methoden mit Nichts oder wirft „not supported“.

**Erkennungsheuristiken:**
- Subclass überschreibt Methode mit Nichts oder throw
- Subclass nutzt nur kleinen Bruchteil geerbter Methoden
- Die „is-a“-Beziehung fühlt sich erzwungen an

**Typische Fixes:**
- Push Down Method / Push Down Field — ungewollte Member zum Sibling verschieben, der sie nutzt
- Replace Inheritance with Delegation — Child hält Referenz auf Parent statt zu extenden

### Alternative Classes with Different Interfaces

**Beschreibung:** Zwei Klassen tun im Wesentlichen dasselbe, haben aber unterschiedliche Methodennamen und Signaturen — Austauschbarkeit unmöglich.

**Erkennungsheuristiken:**
- Zwei Klassen mit ähnlichem Zweck, unterschiedlichen Methodennamen
- Caller wählen zwischen ihnen, können sie aber nicht polymorph behandeln
- Logik-Duplikation, weil keine gemeinsame Schnittstelle existiert

**Typische Fixes:**
- Rename Method — Namen über beide Klassen angleichen
- Extract Superclass or Extract Interface — gemeinsamen Vertrag definieren
- Move Method — angleichen, was jede Klasse bietet

---

## Familie 3: Change Preventers

Smells, die Änderungen teuer machen, weil verwandte Logik verstreut ist.

### Divergent Change

**Beschreibung:** Eine Klasse ändert sich aus mehreren unabhängigen Gründen. Gegenteil des Single Responsibility Principle.

**Erkennungsheuristiken:**
- Dieselbe Klasse für verschiedene Änderungsarten editieren (neue DB, neues Report-Format, neue Business-Regel)
- Methoden clustern in Gruppen ohne Interaktion
- Verschiedene Teammitglieder editieren dieselbe Datei für verschiedene Features

**Typische Fixes:**
- Extract Class — Klasse entlang ihrer Änderungsachsen splitten
- Jede resultierende Klasse sollte sich aus genau einem Grund ändern

### Shotgun Surgery

**Beschreibung:** Eine einzelne logische Änderung erfordert Edits in vielen Klassen. Gegenteil von Divergent Change.

**Erkennungsheuristiken:**
- Kleine funktionale Änderung berührt 5+ Dateien
- Neues Feld muss in mehrere Klassen
- Formatänderung erfordert Edits an verstreuten Stellen

**Typische Fixes:**
- Move Method / Move Field — verwandte Logik in eine Klasse konsolidieren
- Inline Class — wenn verstreute Teile zu klein, in die Klasse mergen, die die Verantwortung haben sollte

---

## Familie 4: Dispensables

Smells, bei denen etwas existiert, aber nicht sollte.

### Lazy Class

**Beschreibung:** Eine Klasse, die zu wenig tut, um ihr Dasein zu rechtfertigen. Jede Klasse kostet Komplexität; trägt sie nicht, mergen.

**Typische Fixes:** Inline Class, Collapse Hierarchy

### Dead Code

**Beschreibung:** Code, der nie ausgeführt wird — unerreichbare Zweige, ungenutzte Variablen, unnötige Parameter, Methoden ohne Caller.

**Typische Fixes:** Löschen. Versionskontrolle erinnert sich.

### Speculative Generality

**Beschreibung:** Abstraktionen, Parameter, Hooks oder Klassen „falls wir sie mal brauchen“. YAGNI — You Aren't Gonna Need It.

**Erkennungsheuristiken:**
- Abstract Classes mit nur einer Subclass
- Parameter, die immer mit demselben Wert übergeben werden
- Methoden, die nur von Tests aufgerufen werden
- Framework-Infrastruktur ohne aktuellen Nutzen

**Typische Fixes:**
- Collapse Hierarchy — unnötige Abstract Class entfernen
- Remove Parameter — ungenutzte Params löschen
- Inline Class / Inline Method — unnötige Indirection zusammenfallen lassen

### Duplicate Code

**Beschreibung:** Dieselbe oder fast identische Codestruktur an mehr als einer Stelle. Der häufigste und teuerste Smell.

**Erkennungsheuristiken:**
- Copy-paste-Blöcke mit kleinen Variationen
- Methoden in verschiedenen Klassen, die dasselbe tun
- Bedingte Zweige mit identischen Bodies

**Typische Fixes:**
- Extract Method — gemeinsamen Code teilen
- Pull Up Method — gemeinsame Methode in Basisklasse verschieben
- Extract Superclass / Extract Class — wenn Duplikation Klassen überspannt
- Form Template Method — wenn Methodenstruktur identisch, Details variieren

---

## Familie 5: Couplers

Smells, bei denen Klassen zu eng aneinander gebunden sind.

### Feature Envy

**Beschreibung:** Eine Methode nutzt mehr Features (Felder und Methoden) einer anderen Klasse als der eigenen. Sie „beneidet“ die Daten der anderen Klasse.

**Erkennungsheuristiken:**
- Methode ruft 3+ Getter auf einem fremden Objekt auf
- Methode könnte in die andere Klasse verschoben werden und bräuchte weniger Parameter

**Typische Fixes:**
- Move Method — Methode zur beneideten Klasse verschieben
- Extract Method + Move Method — neidischen Teil extrahieren, dann verschieben

### Inappropriate Intimacy

**Beschreibung:** Zwei Klassen sind übermäßig verflochten — greifen auf private Details der anderen zu, bilden bidirektionale Abhängigkeit.

**Typische Fixes:**
- Move Method / Move Field, um Cross-Boundary-Traffic zu reduzieren
- Extract Class — gemeinsames Anliegen an neutralen Ort
- Replace Inheritance with Delegation, wenn Subclass zu viele Parent-Internals nutzt

### Message Chains

**Beschreibung:** Client fragt Objekt A nach B, B nach C, C nach D: `a.getB().getC().getD()`. Client ist an die gesamte Navigationsstruktur gekoppelt.

**Typische Fixes:**
- Hide Delegate — A liefert die Antwort direkt
- Extract Method + Move Method — Kette in das Objekt schieben, das die Antwort wissen sollte

### Middle Man

**Beschreibung:** Eine Klasse, deren Methoden nur an eine andere delegieren. Fügt Indirection ohne Wert hinzu.

**Erkennungsheuristiken:**
- Mehr als die Hälfte der Methoden sind Einzeiler-Delegationen
- Die Klasse hat keine eigene Logik

**Typische Fixes:**
- Remove Middle Man — Client ruft Delegate direkt auf
- Inline Method — triviale Forwarding-Methoden in Caller mergen

---

## Smell-zu-Refactoring Schnellreferenz

| Smell | Primary Refactoring | Secondary Refactoring |
|-------|--------------------|-----------------------|
| Long Method | Extract Method | Replace Temp with Query |
| Large Class | Extract Class | Extract Subclass |
| Long Parameter List | Introduce Parameter Object | Preserve Whole Object |
| Data Clumps | Extract Class | Introduce Parameter Object |
| Primitive Obsession | Replace Data Value with Object | Replace Type Code with Subclasses |
| Switch Statements | Replace Conditional with Polymorphism | Replace Type Code with Strategy |
| Refused Bequest | Replace Inheritance with Delegation | Push Down Method |
| Divergent Change | Extract Class | -- |
| Shotgun Surgery | Move Method / Move Field | Inline Class |
| Lazy Class | Inline Class | Collapse Hierarchy |
| Dead Code | Delete it | -- |
| Speculative Generality | Collapse Hierarchy | Inline Class |
| Duplicate Code | Extract Method | Pull Up Method |
| Feature Envy | Move Method | Extract Method + Move Method |
| Inappropriate Intimacy | Move Method / Move Field | Extract Class |
| Message Chains | Hide Delegate | Extract Method |
| Middle Man | Remove Middle Man | Inline Method |
