# Simplifying Conditional Logic

Detaillierte Referenz für Refactorings, die komplexe bedingte Strukturen zähmen. Conditionals sind der schwerste Code zum Lesen und der wahrscheinlichste für Bugs. Diese Refactorings zerlegen, konsolidieren und ersetzen Conditionals durch klarere Alternativen.

---

## Decompose Conditional

Bedingung, Then-Branch und Else-Branch eines komplexen Conditionals in gut benannte Methoden extrahieren.

### Motivation

Ein langes `if` mit zusammengesetzter Bedingung und mehrzeiligen Zweigen zwingt den Leser, jeden Pfad mental zu simulieren. Indem du jeden Teil benennst, wird das Conditional zu lesbarer Prosa.

### Mechanik

1. Bedingung in Methode extrahieren, deren Name die Bedeutung beschreibt (nicht die Mechanik)
2. Then-Body in Methode extrahieren, die beschreibt, was passiert
3. Else-Body in Methode extrahieren, die beschreibt, was passiert
4. Tests laufen lassen

### Beispiel

**Before:**
```javascript
function calculateCharge(date, quantity, plan) {
  let charge;
  if (date.getMonth() >= 6 && date.getMonth() <= 8) {
    charge = quantity * plan.summerRate;
  } else {
    charge = quantity * plan.regularRate + plan.regularServiceCharge;
  }
  return charge;
}
```

**After:**
```javascript
function calculateCharge(date, quantity, plan) {
  if (isSummer(date)) {
    return summerCharge(quantity, plan);
  } else {
    return regularCharge(quantity, plan);
  }
}

function isSummer(date) {
  return date.getMonth() >= 6 && date.getMonth() <= 8;
}

function summerCharge(quantity, plan) {
  return quantity * plan.summerRate;
}

function regularCharge(quantity, plan) {
  return quantity * plan.regularRate + plan.regularServiceCharge;
}
```

### Die Bedingung benennen

| Condition Expression | Good Name |
|---------------------|-----------|
| `date.getMonth() >= 6 && date.getMonth() <= 8` | `isSummer(date)` |
| `user.age >= 18 && user.hasConsent` | `isEligible(user)` |
| `cart.total > 100 && !cart.hasPromo` | `qualifiesForDiscount(cart)` |
| `retries < MAX && !response.ok` | `shouldRetry(retries, response)` |
| `file.size > 0 && file.ext === '.csv'` | `isValidUpload(file)` |

Der Bedingungsname soll eine Ja/Nein-Frage in Domain-Vokabular beantworten.

---

## Consolidate Conditional Expression

Eine Reihe bedingter Checks, die alle zum gleichen Ergebnis führen, in ein Conditional mit beschreibendem Namen zusammenfassen.

### Motivation

Wenn mehrere Bedingungen denselben Wert zurückgeben, macht ein benannter Check die Logik klarer: „All das bedeutet dasselbe — diese Situation ist X.“

### Mechanik

1. Prüfen, dass alle Conditionals keine Side Effects haben
2. Mit logischen Operatoren kombinieren (`&&`, `||`)
3. Kombinierte Bedingung in benannte Methode extrahieren
4. Tests laufen lassen

### Beispiel

**Before:**
```python
def disability_amount(employee):
    if employee.seniority < 2:
        return 0
    if employee.months_disabled > 12:
        return 0
    if employee.is_part_time:
        return 0
    # compute disability amount...
    return base_amount * 1.5
```

**After:**
```python
def disability_amount(employee):
    if is_not_eligible_for_disability(employee):
        return 0
    return base_amount * 1.5

def is_not_eligible_for_disability(employee):
    return (employee.seniority < 2
            or employee.months_disabled > 12
            or employee.is_part_time)
```

### Wann konsolidieren vs. getrennt lassen

| Situation | Aktion |
|-----------|--------|
| Alle Bedingungen meinen dasselbe Business-Konzept | In einen benannten Check konsolidieren |
| Bedingungen sind unabhängig mit verschiedenen Gründen | Getrennt lassen (jede verdient eigenen Namen) |
| Bedingungen sollen der Reihe nach aus Performance-Gründen evaluiert werden | Getrennt für Short-Circuit-Klarheit lassen |

---

## Replace Nested Conditional with Guard Clauses

Sonderfälle und Edge Conditions oben in der Methode behandeln und früh returnen — Hauptpfad flach und unindented lassen.

### Motivation

Tief verschachtelte `if/else`-Strukturen verdecken den Normalpfad. Guard Clauses machen klar: „Das sind die Edge Cases. Jetzt die Hauptlogik.“ Der Hauptpfad läuft auf niedrigster Einrückungsebene.

### Mechanik

1. Jeden Edge Case oder Sonderfall identifizieren
2. Nach oben als `if (condition) return earlyValue;` verschieben
3. Entsprechendes `else` entfernen und Einrückung reduzieren
4. Tests laufen lassen

### Beispiel

**Before:**
```javascript
function payAmount(employee) {
  let result;
  if (employee.isSeparated) {
    result = { amount: 0, reasonCode: 'SEP' };
  } else {
    if (employee.isRetired) {
      result = { amount: 0, reasonCode: 'RET' };
    } else {
      // main calculation
      result = {
        amount: employee.salary * employee.rate,
        reasonCode: 'REG'
      };
    }
  }
  return result;
}
```

**After:**
```javascript
function payAmount(employee) {
  if (employee.isSeparated) return { amount: 0, reasonCode: 'SEP' };
  if (employee.isRetired) return { amount: 0, reasonCode: 'RET' };

  return {
    amount: employee.salary * employee.rate,
    reasonCode: 'REG'
  };
}
```

### Guard-Clause-Patterns

| Pattern | Example |
|---------|---------|
| Null check | `if (input == null) return defaultValue;` |
| Empty check | `if (items.length === 0) return [];` |
| Permission check | `if (!user.canEdit) throw new ForbiddenError();` |
| Boundary check | `if (index < 0 \|\| index >= size) throw new RangeError();` |
| Status check | `if (order.isCancelled) return zeroPay();` |

### „One return“ vs. Guard Clauses

Manche Coding-Standards verlangen ein Return pro Methode. Das führt zu tief verschachtelten Conditionals und temporären Ergebnisvariablen. Guard Clauses mit early returns erzeugen klareren, flacheren Code. Fowler empfiehlt Guard Clauses explizit gegen Single-Return bei Methoden mit Sonderfällen.

---

## Replace Conditional with Polymorphism

Ein Conditional, das Typ, Status oder Kategorie prüft und zu unterschiedlichem Verhalten verzweigt, durch polymorphe Klassen ersetzen, in denen jeder Typ seine eigene Implementierung liefert.

### Motivation

Das ist der Goldstandard zum Eliminieren typbasierter Conditionals. Statt einer Funktion, die jeden Typ kennt, kennt jeder Typ sich selbst. Neuer Typ heißt neue Klasse — nicht bestehende Conditionals an mehreren Stellen editieren (Open/Closed Principle).

### Mechanik

1. Wenn das Conditional auf Type Code basiert, zuerst Replace Type Code with Subclasses anwenden
2. Basismethode (ggf. abstrakt) in der Superklasse erstellen
3. Jeden Zweig des Conditionals in die entsprechende Subclass als Override kopieren
4. Conditional aus der Superklasse entfernen (oder als Default-Fall belassen)
5. Tests laufen lassen

### Beispiel

**Before:**
```python
class Bird:
    def __init__(self, bird_type, voltage=0, coconut_count=0):
        self.type = bird_type
        self.voltage = voltage
        self.coconut_count = coconut_count

    def speed(self):
        if self.type == 'european':
            return 35 - (self.voltage / 10)
        elif self.type == 'african':
            return 40 - (2 * self.coconut_count)
        elif self.type == 'norwegian_blue':
            return 0 if self.voltage > 100 else 10 + (self.voltage / 10)
        else:
            raise ValueError(f"Unknown bird type: {self.type}")
```

**After:**
```python
class Bird:
    def speed(self):
        raise NotImplementedError

class EuropeanSwallow(Bird):
    def speed(self):
        return 35 - (self.voltage / 10)

class AfricanSwallow(Bird):
    def speed(self):
        return 40 - (2 * self.coconut_count)

class NorwegianBlueParrot(Bird):
    def speed(self):
        return 0 if self.voltage > 100 else 10 + (self.voltage / 10)
```

### Wann Polymorphismus vs. Conditional behalten

| Situation | Recommendation |
|-----------|---------------|
| Conditional in mehreren Methoden | Polymorphismus — Typen kennen eigenes Verhalten |
| Nur eine Methode hat das Conditional | Vielleicht Overkill — Decompose Conditional reicht |
| Neue Typen werden häufig hinzugefügt | Polymorphismus — Open/Closed Principle |
| Typmenge ist fix und klein (z. B. 2–3) | Conditional kann einfacher sein |
| Verhalten variiert nach Code, der zur Laufzeit wechselt | Strategy-Pattern statt Vererbung |

---

## Introduce Special Case (Null Object)

Statt in jedem Caller auf einen Sonderfall (meist null) zu prüfen, eine Klasse erstellen, die das Sonderfall-Verhalten kapselt.

### Motivation

`if (customer == null)`-Checks in der Codebase erzeugen Rauschen und sind leicht zu vergessen. Ein `NullCustomer`- oder `UnknownCustomer`-Objekt antwortet auf dieselben Methoden mit sicherem Default-Verhalten.

### Mechanik

1. Subclass oder separate Klasse für den Sonderfall erstellen
2. Methode in Superklasse oder Factory hinzufügen, die den Sonderfall erzeugt (z. B. `Customer.unknown()`)
3. Jede Methode im Sonderfall mit dem Default-Verhalten implementieren, das Caller nach ihren Null-Checks nutzen
4. Caller auf Sonderfall-Objekt statt null umstellen
5. Null-Checks aus Callern entfernen
6. Tests laufen lassen

### Beispiel

**Before:**
```javascript
// Scattered throughout the codebase:
const customerName = (customer !== null) ? customer.name : 'Occupant';
const billingPlan = (customer !== null) ? customer.billingPlan : BillingPlan.basic();
const paymentHistory = (customer !== null) ? customer.paymentHistory : new NullPaymentHistory();
```

**After:**
```javascript
class UnknownCustomer {
  get name() { return 'Occupant'; }
  get billingPlan() { return BillingPlan.basic(); }
  get paymentHistory() { return new NullPaymentHistory(); }
  get isUnknown() { return true; }
}

class Customer {
  get isUnknown() { return false; }
  // ... normal implementation
}

// Callers (no more null checks):
const customerName = customer.name;
const billingPlan = customer.billingPlan;
```

### Häufige Sonderfälle

| Domain | Special Case Object | Default Behavior |
|--------|-------------------|------------------|
| Customer | `UnknownCustomer` | Returns "Occupant", basic plan |
| Currency | `NullMoney` | Zero amount, no currency |
| Logger | `NullLogger` | Silently discards all messages |
| Permission | `DeniedPermission` | Returns false for all checks |
| Config | `DefaultConfig` | Returns sensible defaults |
| User | `AnonymousUser` | Read-only, no privileges |

---

## Introduce Assertion

Eine Annahme explizit machen, indem du eine Assertion einfügst, die bei Verletzung schnell fehlschlägt.

### Motivation

Assertions dokumentieren, was der Code für wahr hält. Sie sind ausführbare Dokumentation, die Bugs in der Entwicklung fängt. Anders als Kommentare werden Assertions zur Laufzeit geprüft.

### Mechanik

1. Annahme im Code identifizieren (Bedingung, die immer wahr sein sollte)
2. Assertion an der Stelle einfügen, wo die Annahme gilt
3. Sicherstellen, dass die Assertion keine Side Effects hat
4. Tests laufen lassen (sollten noch grün sein — schlägt Assertion fehl, hast du einen Bug gefunden)

### Beispiel

**Before:**
```python
def apply_discount(product, discount_rate):
    # discount should be between 0 and 1
    price = product.base_price * (1 - discount_rate)
    return price
```

**After:**
```python
def apply_discount(product, discount_rate):
    assert 0 <= discount_rate <= 1, f"Discount rate must be 0-1, got {discount_rate}"
    price = product.base_price * (1 - discount_rate)
    return price
```

### Assertion-Richtlinien

| Guideline | Rationale |
|-----------|-----------|
| Nie Assertions für Input-Validierung | Assertions können in Produktion deaktiviert sein; Exceptions für untrusted Input |
| Assertions für Programmierfehler | Bedingungen, die bei korrektem Code nie auftreten sollten |
| Assertion-Messages beschreibend halten | Tatsächlichen Wert und erwartete Constraint angeben |
| Keine Side Effects in Assertions | `assert items.remove(x)` bricht, wenn Assertions deaktiviert sind |

---

## Entscheidungshilfe: Welches Conditional-Refactoring nutzen?

| Situation | Refactoring |
|-----------|-------------|
| Langer, komplexer Bedingungsausdruck | Decompose Conditional |
| Mehrere Bedingungen führen zum gleichen Ergebnis | Consolidate Conditional Expression |
| Verschachteltes if/else mit Sonderfällen | Replace Nested Conditional with Guard Clauses |
| Switch/if auf Type Code an mehreren Stellen | Replace Conditional with Polymorphism |
| Null-Checks überall verstreut | Introduce Special Case (Null Object) |
| Versteckte Annahme in der Logik | Introduce Assertion |
| Bedingung einmal, kleine Typmenge | Conditional behalten, aber dekomponieren |
| Bedingung variiert zur Laufzeit | Strategy-Pattern nutzen |
