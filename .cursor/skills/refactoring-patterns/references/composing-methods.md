# Composing Methods

Detaillierte Referenz für Refactorings, die lange Methoden in gut benannte, kohäsive Teile zerlegen. Das sind die am häufigsten genutzten Refactorings und das Fundament aller Code-Verbesserung.

---

## Extract Method

Das wichtigste Refactoring. Einen Code-Fragment in eine Methode verwandeln, deren Name den Zweck des Fragments erklärt.

### Motivation

Du hast ein Code-Fragment, das zusammengehört. Je länger eine Methode, desto schwerer zu verstehen. Wenn du einen Kommentar siehst, der erklärt, was der nächste Block tut, ist das ein Signal zum Extrahieren. Der Kommentar wird der Methodenname.

### Mechanik

1. Neue Methode erstellen und nach der *Absicht* des Codes benennen (was es tut, nicht wie)
2. Extrahierten Code in die neue Methode kopieren
3. Extrahierten Code auf Referenzen zu lokalen Variablen scannen — diese werden Parameter oder Rückgabewerte
4. Lokale Variablen als lokal zur neuen Methode deklarieren, wenn sie nur im extrahierten Code genutzt werden
5. Verbleibende lokale Variablen als Parameter übergeben
6. Wenn mehr als ein Wert zurückgegeben werden muss, Objekt zurückgeben oder weiter extrahieren
7. Originalcode durch Aufruf der neuen Methode ersetzen
8. Tests laufen lassen

### Beispiel

**Before:**
```javascript
function printOwing(invoice) {
  let outstanding = 0;

  // print banner
  console.log("***********************");
  console.log("**** Customer Owes ****");
  console.log("***********************");

  // calculate outstanding
  for (const order of invoice.orders) {
    outstanding += order.amount;
  }

  // print details
  console.log(`name: ${invoice.customer}`);
  console.log(`amount: ${outstanding}`);
  console.log(`due: ${invoice.dueDate.toLocaleDateString()}`);
}
```

**After:**
```javascript
function printOwing(invoice) {
  printBanner();
  const outstanding = calculateOutstanding(invoice);
  printDetails(invoice, outstanding);
}

function printBanner() {
  console.log("***********************");
  console.log("**** Customer Owes ****");
  console.log("***********************");
}

function calculateOutstanding(invoice) {
  let result = 0;
  for (const order of invoice.orders) {
    result += order.amount;
  }
  return result;
}

function printDetails(invoice, outstanding) {
  console.log(`name: ${invoice.customer}`);
  console.log(`amount: ${outstanding}`);
  console.log(`due: ${invoice.dueDate.toLocaleDateString()}`);
}
```

### Naming-Richtlinien

| Bad Name | Good Name | Why |
|----------|-----------|-----|
| `doStuff()` | `calculateMonthlyTotal()` | Benennt die Absicht, nicht die Vagheit |
| `process()` | `validateAndSaveOrder()` | Spezifisch, was es tut |
| `handleData()` | `parseCSVRow()` | Benennt das Domain-Konzept |
| `helper()` | `formatCurrencyForDisplay()` | Beschreibt die Transformation |
| `step2()` | `applyDiscountRules()` | Benennt das Business-Konzept |

**Faustregel:** Wenn du keinen guten Namen findest, sind die Extraktionsgrenzen vielleicht falsch. Versuche ein anderes Fragment zu extrahieren.

---

## Inline Method

Das Inverse von Extract Method. Einen Methodenaufruf durch den Methodenbody ersetzen, wenn der Body so klar ist wie der Name, oder wenn du schlecht faktorisierten Code neu gruppieren musst.

### Motivation

Manchmal ist ein Methodenbody so offensichtlich wie der Methodenname. Indirection ohne Wert ist Rauschen. Auch nützlich als Zwischenschritt: schlecht zerlegte Methode inlinen, dann entlang besserer Grenzen neu extrahieren.

### Mechanik

1. Prüfen, dass die Methode nicht polymorph ist (keine Subclass-Overrides)
2. Alle Caller finden
3. Jeden Aufruf durch den Methodenbody ersetzen
4. Methode löschen
5. Tests laufen lassen

### Beispiel

**Before:**
```python
def get_rating(self):
    return 2 if self.more_than_five_late_deliveries() else 1

def more_than_five_late_deliveries(self):
    return self.late_deliveries > 5
```

**After:**
```python
def get_rating(self):
    return 2 if self.late_deliveries > 5 else 1
```

### Wann NICHT inlinen

- Wenn der Methodenname Domain-Bedeutung kommuniziert, die der Code nicht tut
- Wenn die Methode an mehreren Stellen genutzt wird (DRY)
- Wenn die Methode in Subklassen überschrieben wird

---

## Extract Variable

Eine lokale Variable für einen komplexen Ausdruck einführen, um ihn selbstdokumentierend zu machen.

### Motivation

Ausdrücke können schwer lesbar werden. Eine gut benannte Variable für einen Sub-Ausdruck wirkt als Inline-Dokumentation und erleichtert Debugging.

### Mechanik

1. Komplexen Ausdruck oder Sub-Ausdruck identifizieren
2. Variable nach der Absicht des Ausdrucks deklarieren
3. Ausdruck durch die Variable ersetzen
4. Tests laufen lassen

### Beispiel

**Before:**
```javascript
return order.quantity * order.itemPrice -
  Math.max(0, order.quantity - 500) * order.itemPrice * 0.05 +
  Math.min(order.quantity * order.itemPrice * 0.1, 100);
```

**After:**
```javascript
const basePrice = order.quantity * order.itemPrice;
const quantityDiscount = Math.max(0, order.quantity - 500) * order.itemPrice * 0.05;
const shippingCap = Math.min(basePrice * 0.1, 100);
return basePrice - quantityDiscount + shippingCap;
```

---

## Inline Variable

Das Inverse von Extract Variable. Eine Variable entfernen, wenn der Ausdruck genauso klar ist.

### Wann nutzen

- Der Variablenname fügt keine Info über den Ausdruck hinaus hinzu
- Die Variable wird einmal zugewiesen und einmal genutzt
- Die Variable blockiert ein anderes Refactoring (z. B. inlinen, um dann Extract Method zu machen)

### Beispiel

**Before:**
```python
base_price = order.base_price()
return base_price > 1000
```

**After:**
```python
return order.base_price() > 1000
```

---

## Replace Temp with Query

Eine temporäre Variable in einen Methodenaufruf verwandeln, damit die Berechnung wiederverwendbar ist und die Originalmethode kürzer wird.

### Motivation

Temporaries sind nur innerhalb einer Methode sichtbar. Wenn dieselbe Berechnung woanders gebraucht wird, wird sie dupliziert. Eine Query-Methode ist für die ganze Klasse sichtbar (oder kann in eine andere Klasse extrahiert werden).

### Mechanik

1. Prüfen, dass die Variable einmal zugewiesen wird und der Ausdruck keine Side Effects hat
2. Rechte Seite der Zuweisung in eine neue Methode extrahieren
3. Alle Referenzen auf die Temp durch Aufrufe der neuen Methode ersetzen
4. Temp-Deklaration und -Zuweisung entfernen
5. Tests laufen lassen

### Beispiel

**Before:**
```javascript
class Order {
  getPrice() {
    const basePrice = this.quantity * this.itemPrice;
    if (basePrice > 1000) {
      return basePrice * 0.95;
    } else {
      return basePrice * 0.98;
    }
  }
}
```

**After:**
```javascript
class Order {
  getPrice() {
    if (this.basePrice() > 1000) {
      return this.basePrice() * 0.95;
    } else {
      return this.basePrice() * 0.98;
    }
  }

  basePrice() {
    return this.quantity * this.itemPrice;
  }
}
```

### Performance-Hinweis

Die Methode mehrfach aufzurufen statt in einer Temp zu cachen wirkt verschwenderisch. In der Praxis ist der Performance-Impact für den meisten Code vernachlässigbar. Vor Optimierung profilen. Refactorierter Code ist leichter später zu optimieren, weil der Hot Path isoliert ist.

---

## Split Temporary Variable

Wenn eine temporäre Variable mehr als einmal zugewiesen wird (und es kein Schleifenzähler oder Sammelvariable ist), erfüllt sie zwei verschiedene Jobs. Gib jedem Job seine eigene Variable.

### Motivation

Eine Temp, die zweimal für verschiedene Zwecke zugewiesen wird, täuscht vor, die Zuweisungen hängen zusammen. Jede Rolle verdient eine eigene Variable mit beschreibendem Namen.

### Mechanik

1. Erste Zuweisung umbenennen, um ihren Zweck widerzuspiegeln
2. Als `const`/`final` deklarieren, wenn möglich
3. Alle Nutzungen, die auf den Wert der ersten Zuweisung verweisen, auf den neuen Namen umstellen
4. Für jede weitere Zuweisung mit anderem Namen wiederholen
5. Tests laufen lassen

### Beispiel

**Before:**
```javascript
let temp = 2 * (height + width);  // perimeter
console.log(temp);
temp = height * width;            // area
console.log(temp);
```

**After:**
```javascript
const perimeter = 2 * (height + width);
console.log(perimeter);
const area = height * width;
console.log(area);
```

---

## Remove Assignments to Parameters

Niemals einem Parameter innerhalb eines Methodenbodys zuweisen. Das verwirrt Leser, ob die Änderung für den Caller sichtbar ist (in pass-by-value nicht; bei Objektmutationen in pass-by-reference schon).

### Mechanik

1. Neue lokale Variable für den Parameter erstellen
2. Alle Zuweisungen an den Parameter durch Zuweisungen an die neue Variable ersetzen
3. Tests laufen lassen

### Beispiel

**Before:**
```python
def discount(input_val, quantity):
    if quantity > 50:
        input_val -= 2
    if quantity > 100:
        input_val -= 1
    return input_val
```

**After:**
```python
def discount(input_val, quantity):
    result = input_val
    if quantity > 50:
        result -= 2
    if quantity > 100:
        result -= 1
    return result
```

---

## Replace Method with Method Object

Wenn eine Methode zu verwickelt mit lokalen Variablen ist, um daraus zu extrahieren, die gesamte Methode in eine eigene Klasse verschieben, in der lokale Variablen Felder werden. Dann kannst du frei Sub-Methoden extrahieren.

### Motivation

Manchmal hat eine lange Methode so viele verknüpfte lokale Variablen, dass Extract Method unmöglich ist (zu viele Parameter nötig). Indem du die Methode in eine eigene Klasse verwandelst, werden alle Locals zu Feldern, für extrahierte Methoden ohne Parameter zugänglich.

### Mechanik

1. Neue Klasse nach dem Zweck der Methode benennen
2. Feld für das Originalobjekt und für jede lokale Variable und jeden Parameter hinzufügen
3. Konstruktor erstellen, der das Originalobjekt und alle Parameter nimmt
4. Methodenbody in eine `compute()`- (oder ähnliche) Methode kopieren
5. Originalmethode ersetzen durch: neues Objekt erstellen, `compute()` aufrufen
6. Jetzt frei Methoden in der neuen Klasse extrahieren — Locals sind Felder, kein Parameter-Passing nötig
7. Tests laufen lassen

### Beispiel

**Before:**
```python
class Account:
    def gamma(self, input_val, quantity, year_to_date):
        # 50 lines of tangled computation using all three params
        # plus self.fields -- too intertwined to extract
        ...
```

**After:**
```python
class GammaCalculation:
    def __init__(self, account, input_val, quantity, year_to_date):
        self.account = account
        self.input_val = input_val
        self.quantity = quantity
        self.year_to_date = year_to_date

    def compute(self):
        # Now extract freely -- all variables are fields
        self._apply_quantity_adjustment()
        self._apply_yearly_factor()
        return self.input_val

    def _apply_quantity_adjustment(self):
        # can access self.quantity, self.input_val freely
        ...

    def _apply_yearly_factor(self):
        ...

class Account:
    def gamma(self, input_val, quantity, year_to_date):
        return GammaCalculation(self, input_val, quantity, year_to_date).compute()
```

---

## Entscheidungshilfe: Welches Composing-Refactoring nutzen?

| Situation | Refactoring |
|-----------|-------------|
| Codeblock kann nach Absicht benannt werden | Extract Method |
| Methodenbody ist trivial und Name fügt nichts hinzu | Inline Method |
| Komplexer Ausdruck braucht Erklärung | Extract Variable |
| Variable fügt über den Ausdruck hinaus keine Bedeutung hinzu | Inline Variable |
| Dieselbe Berechnung in mehreren Methoden nötig | Replace Temp with Query |
| Eine Variable erfüllt zwei Zwecke | Split Temporary Variable |
| Parameter wird innerhalb der Methode neu zugewiesen | Remove Assignments to Parameters |
| Lange Methode mit zu vielen verknüpften Locals | Replace Method with Method Object |
