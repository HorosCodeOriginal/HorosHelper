# Organizing Data

Detaillierte Referenz für Refactorings, die verbessern, wie Daten repräsentiert werden. Rohe Primitives, Magic Numbers, exponierte Felder und mutable Collections erzeugen subtile Bugs und streuen Domain-Wissen. Diese Refactorings ersetzen primitive Repräsentationen durch Objekte, die Verhalten kapseln und Invarianten durchsetzen.

---

## Replace Data Value with Object

Ein primitives Datenelement in eine Klasse wrappen, wenn es zugehöriges Verhalten oder Validierung hat. Das ist die Heilung für Primitive Obsession.

### Motivation

Ein Datenwert startet als einfacher String oder Zahl. Dann kommt Validierung. Dann Formatierung. Dann Vergleichslogik. Dann dieselbe Validierung an drei Stellen. Dann verdient der Wert ein Objekt zu sein.

### Mechanik

1. Klasse für den Wert mit Konstruktor, der das Primitive nimmt
2. Validierung im Konstruktor hinzufügen
3. Verhaltensmethoden hinzufügen (Formatierung, Vergleich, etc.)
4. Feldtyp von Primitive auf neue Klasse ändern
5. Allen Code, der das Feld setzt, anpassen, um Instanz der neuen Klasse zu erstellen
6. Allen Code, der das Feld liest, anpassen, um Methoden des Objekts zu nutzen
7. Tests laufen lassen

### Beispiel

**Before:**
```javascript
class Order {
  constructor(customer) {
    this.customer = customer; // just a string name
  }
}

// Scattered validation in multiple places:
if (order.customer === '') throw new Error('no customer');
if (otherOrder.customer === '') throw new Error('no customer');
```

**After:**
```javascript
class Customer {
  constructor(name) {
    if (!name || name.trim() === '') {
      throw new Error('Customer name is required');
    }
    this._name = name.trim();
  }

  get name() { return this._name; }

  equals(other) {
    return other instanceof Customer && this._name === other._name;
  }
}

class Order {
  constructor(customer) {
    this.customer = new Customer(customer);
  }
}
```

### Häufige Primitive-zu-Objekt-Upgrades

| Primitive | Object | Behavior It Gains |
|-----------|--------|-------------------|
| `String email` | `EmailAddress` | Format validation, domain extraction |
| `number cents` | `Money` | Currency, rounding rules, arithmetic |
| `String phone` | `PhoneNumber` | Formatting, country code parsing |
| `number lat, number lng` | `Coordinates` | Distance calculation, validation |
| `String startDate, String endDate` | `DateRange` | Contains, overlaps, duration |
| `number celsius` | `Temperature` | Unit conversion, comparison |
| `String hex` | `Color` | Parsing, lightness, contrast |
| `number status` | `OrderStatus` | Valid transitions, display name |

---

## Change Value to Reference

Ein Value Object in ein Reference Object umwandeln, wenn Identitätssemantik nötig ist — wenn Änderungen an einer Instanz überall sichtbar sein sollen, wo sie genutzt wird.

### Motivation

Wenn du mehrere Kopien desselben Kunden hast, ändert die Telefonnummer an einer nicht die anderen. Wenn Business-Regeln eine einzige geteilte Instanz verlangen, Value zu Reference mit Registry oder Repository konvertieren.

### Mechanik

1. Factory-Methode für das Objekt bestimmen oder erstellen
2. Registry (Map, Repository oder Lookup-Service) für Instanzen einrichten
3. Factory so ändern, dass sie die Registry vor neuer Instanzen prüft
4. Client-Code auf Factory statt Konstruktor umstellen
5. Tests laufen lassen

### Beispiel

```javascript
// Registry pattern:
class CustomerRepository {
  constructor() {
    this._customers = new Map();
  }

  get(id) {
    if (!this._customers.has(id)) {
      this._customers.set(id, new Customer(id));
    }
    return this._customers.get(id);
  }
}

// All orders for customer #123 now share the same Customer object
const repo = new CustomerRepository();
const order1 = new Order(repo.get(123));
const order2 = new Order(repo.get(123));
// order1.customer === order2.customer  // true (same reference)
```

### Value vs. Reference: Entscheidungshilfe

| Question | Value | Reference |
|----------|-------|-----------|
| Brauchst du Identität (überall dasselbe Objekt)? | No | Yes |
| Ist das Objekt immutable? | Typically | May be mutable |
| Vergleichst du nach Inhalt? | Yes (`equals()`) | No (identity `===`) |
| Examples | Money, DateRange, Color | Customer, Account, Product |

---

## Replace Array with Object

Ein Array, das als Record genutzt wird (jede Position hat andere Bedeutung), durch ein Objekt mit benannten Feldern ersetzen.

### Motivation

`row[0]` ist der Name, `row[1]` das Alter, `row[2]` die Abteilung. Das ist fragil, unlesbar und typsicher unsicher. Benannte Felder machen die Struktur selbstdokumentierend.

### Mechanik

1. Klasse mit Feld pro Array-Position erstellen
2. Getter und Setter für jedes Feld hinzufügen
3. Array-Erstellung durch Objektkonstruktion ersetzen
4. Positionszugriff durch benannten Zugriff ersetzen
5. Tests laufen lassen

### Beispiel

**Before:**
```python
performance = ["Liverpool", 15, 2]
name = performance[0]
wins = performance[1]
losses = performance[2]
```

**After:**
```python
class Performance:
    def __init__(self, name, wins, losses):
        self.name = name
        self.wins = wins
        self.losses = losses

performance = Performance("Liverpool", 15, 2)
name = performance.name
wins = performance.wins
losses = performance.losses
```

---

## Replace Magic Number with Symbolic Constant

Ein Literal mit besonderer Bedeutung durch eine benannte Konstante ersetzen.

### Motivation

`9.81` bedeutet im Code nichts. `GRAVITATIONAL_ACCELERATION = 9.81` kommuniziert Absicht, verhindert Tippfehler (der Konstantenname wird vom Compiler geprüft) und zentralisiert den Wert für einfache Änderung.

### Mechanik

1. Konstante deklarieren und auf die Magic Number setzen
2. Alle Vorkommen der Magic Number finden
3. Jedes Vorkommen durch die Konstante ersetzen (prüfen, dass jedes dasselbe Konzept meint — `100` kann „Prozent“ an einer Stelle und „max items“ an anderer bedeuten)
4. Tests laufen lassen

### Häufige Magic-Number-Kategorien

| Category | Before | After |
|----------|--------|-------|
| Physics | `9.81` | `GRAVITATIONAL_ACCELERATION` |
| Business rules | `0.08` | `SALES_TAX_RATE` |
| Limits | `255` | `MAX_RGB_VALUE` |
| HTTP | `404` | `HTTP_NOT_FOUND` |
| Time | `86400` | `SECONDS_PER_DAY` |
| Retry | `3` | `MAX_RETRY_ATTEMPTS` |
| Thresholds | `100` | `FREE_SHIPPING_THRESHOLD` |

### Wann NICHT ersetzen

- `0` und `1` in Arithmetik sind meist als Literale ok
- Schleifenzähler (`for i in range(10)`) sind aus Kontext klar
- Array-Index `[0]` für „erstes Element“ ist idiomatisch

---

## Encapsulate Field

Direkten Zugriff auf ein öffentliches Feld durch Getter- und Setter-Methoden ersetzen.

### Motivation

Ein öffentliches Feld gibt keine Kontrolle über Reads und Writes. Du kannst später keine Validierung, Logging oder berechnete Werte hinzufügen, ohne jeden Caller zu ändern. Kapselung schafft eine Naht für zukünftige Änderung.

### Mechanik

1. Getter und Setter für das Feld erstellen
2. Alle Feldreferenzen finden und Reads durch Getter, Writes durch Setter ersetzen
3. Feld private machen
4. Tests laufen lassen

### Beispiel

**Before:**
```python
class Person:
    def __init__(self, name):
        self.name = name  # public field

# Client:
person.name = "   Bob   "  # no validation, no trimming
```

**After:**
```python
class Person:
    def __init__(self, name):
        self._name = None
        self.name = name  # uses the setter

    @property
    def name(self):
        return self._name

    @name.setter
    def name(self, value):
        if not value or not value.strip():
            raise ValueError("Name cannot be empty")
        self._name = value.strip()
```

---

## Encapsulate Collection

Keine rohe mutable Collection aus einem Getter zurückgeben. Stattdessen unmodifiable View oder Kopie zurückgeben und explizite add/remove-Methoden bereitstellen.

### Motivation

Wenn ein Getter eine mutable Liste zurückgibt, können Caller Items hinzufügen, entfernen oder leeren, ohne dass das besitzende Objekt es weiß. Das bricht Kapselung — das Objekt kann keine Invarianten durchsetzen, Events feuern oder Änderungen validieren.

### Mechanik

1. `addItem()`- und `removeItem()`-Methoden auf der besitzenden Klasse hinzufügen
2. Getter so ändern, dass er unmodifiable View (oder Kopie) zurückgibt
3. Alle Caller finden, die die Collection über den Getter mutieren, und auf add/remove-Methoden umstellen
4. Tests laufen lassen

### Beispiel

**Before:**
```javascript
class Course {}

class Person {
  get courses() { return this._courses; }
  set courses(list) { this._courses = list; }
}

// Client can mutate freely:
person.courses.push(newCourse);        // bypasses Person
person.courses.splice(0, 1);           // bypasses Person
person.courses = [];                   // replaces internal state
```

**After:**
```javascript
class Person {
  get courses() {
    return [...this._courses]; // return a copy
  }

  addCourse(course) {
    this._courses.push(course);
  }

  removeCourse(course) {
    const index = this._courses.indexOf(course);
    if (index === -1) throw new RangeError('Course not found');
    this._courses.splice(index, 1);
  }

  get numberOfCourses() {
    return this._courses.length;
  }
}
```

### Sprachspezifische Patterns

| Language | Unmodifiable Return |
|----------|-------------------|
| Java | `Collections.unmodifiableList(list)` |
| JavaScript | `[...this._items]` or `Object.freeze([...this._items])` |
| Python | `tuple(self._items)` or `list(self._items)` (return a copy) |
| C# | `items.AsReadOnly()` |
| Go | Return a slice copy: `append([]T{}, items...)` |

---

## Replace Type Code with Class

Einen Type Code (Integer- oder String-Konstante), der Verhalten nicht beeinflusst, durch eine richtige Klasse ersetzen. Nutzen, wenn der Type Code nur zur Kategorisierung dient, aber keine bedingte Logik antreibt.

### Wann welches nutzen

| Situation | Refactoring |
|-----------|-------------|
| Type Code ist nur informativ (kein Verhaltenswechsel) | Replace Type Code with Class |
| Type Code treibt Verhalten über Conditionals | Replace Type Code with Subclasses |
| Type Code kann zur Laufzeit wechseln | Replace Type Code with Strategy/State |
| Type Code hat wenige Werte und Sprache unterstützt es | Enum nutzen |

### Replace Type Code with Subclasses

Genutzt, wenn der Type Code Verhalten über Conditionals bestimmt.

**Before:**
```javascript
class Employee {
  constructor(type) {
    this._type = type; // 'engineer', 'manager', 'salesperson'
  }

  calculatePay() {
    switch (this._type) {
      case 'engineer': return this.basePay;
      case 'manager': return this.basePay + this.bonus;
      case 'salesperson': return this.basePay + this.commission;
    }
  }

  canApproveExpenses() {
    return this._type === 'manager';
  }
}
```

**After:**
```javascript
class Employee {
  calculatePay() { throw new Error('abstract'); }
  canApproveExpenses() { return false; }
}

class Engineer extends Employee {
  calculatePay() { return this.basePay; }
}

class Manager extends Employee {
  calculatePay() { return this.basePay + this.bonus; }
  canApproveExpenses() { return true; }
}

class Salesperson extends Employee {
  calculatePay() { return this.basePay + this.commission; }
}
```

### Replace Type Code with Strategy/State

Genutzt, wenn der Type Code zur Laufzeit wechseln kann (Mitarbeiter von Engineer zu Manager befördert), sodass Subclassing des Employees selbst nicht möglich ist.

**After (Strategy):**
```javascript
class Employee {
  constructor(type) {
    this._type = type; // EmployeeType strategy object
  }

  calculatePay() {
    return this._type.calculatePay(this);
  }

  promoteToManager() {
    this._type = new ManagerType();
  }
}

class EngineerType {
  calculatePay(employee) { return employee.basePay; }
}

class ManagerType {
  calculatePay(employee) { return employee.basePay + employee.bonus; }
}
```

---

## Entscheidungshilfe: Welches Daten-Refactoring nutzen?

| Situation | Refactoring |
|-----------|-------------|
| Primitive Value hat zugehöriges Verhalten | Replace Data Value with Object |
| Eine geteilte Instanz im System nötig | Change Value to Reference |
| Array-Positionen haben verschiedene Bedeutungen | Replace Array with Object |
| Literal hat Domain-Bedeutung | Replace Magic Number with Symbolic Constant |
| Public Field braucht zukünftige Flexibilität | Encapsulate Field |
| Getter gibt mutable Collection zurück | Encapsulate Collection |
| Type Code ist informativ | Replace Type Code with Class / Enum |
| Type Code treibt Verhalten | Replace Type Code with Subclasses |
| Type Code wechselt zur Laufzeit | Replace Type Code with Strategy |
