# Moving Features Between Objects

Detaillierte Referenz für Refactorings, die Verantwortlichkeiten zwischen Klassen neu verteilen. Die Grundfrage im OO-Design ist: Wo soll dieses Verhalten leben? Diese Refactorings liefern die mechanischen Schritte, Dinge an den richtigen Ort zu verschieben.

---

## Move Method

Eine Methode in die Klasse verschieben, die sie am meisten nutzt. Eine Methode, die mehr Features einer anderen Klasse als der eigenen nutzt, hat Feature Envy und gehört woanders hin.

### Motivation

Der häufigste Grund für Move Method ist Feature Envy — wenn eine Methode die meiste Zeit mit einem anderen Objekt spricht. Das Verschieben reduziert Kopplung: Die Methode lebt jetzt dort, wo ihre Daten leben, sodass Datenänderungen nicht nach außen rippeln.

### Mechanik

1. Alle Features (Felder und Methoden) prüfen, die die Methode nutzt. Bestimmen, welche Klasse die meisten von der Methode genutzten Features hat.
2. Verwandte Methoden in der Quellklasse prüfen. Wenn andere Methoden dieselbe Zielklasse nutzen, sie gemeinsam verschieben erwägen.
3. Superklassen und Subklassen auf Overrides oder verwandte Deklarationen prüfen.
4. Methode in der Zielklasse deklarieren. Body kopieren und Referenzen anpassen — `this` bezieht sich jetzt auf das Ziel; das Quellobjekt muss ggf. als Parameter übergeben werden.
5. Quellmethode in Delegationsmethode verwandeln (Ziel aufrufen).
6. Tests laufen lassen.
7. Delegationsmethode entfernen erwägen, wenn keine anderen Caller sie brauchen.
8. Tests laufen lassen.

### Beispiel

**Before:**
```javascript
class Account {
  overdraftCharge() {
    if (this.type.isPremium()) {
      let result = 10;
      if (this.daysOverdrawn > 7) {
        result += (this.daysOverdrawn - 7) * 0.85;
      }
      return result;
    } else {
      return this.daysOverdrawn * 1.75;
    }
  }
}
```

Die Methode hängt stark von `this.type` (einem `AccountType`-Objekt) ab. Dorthin verschieben.

**After:**
```javascript
class AccountType {
  overdraftCharge(daysOverdrawn) {
    if (this.isPremium()) {
      let result = 10;
      if (daysOverdrawn > 7) {
        result += (daysOverdrawn - 7) * 0.85;
      }
      return result;
    } else {
      return daysOverdrawn * 1.75;
    }
  }
}

class Account {
  overdraftCharge() {
    return this.type.overdraftCharge(this.daysOverdrawn);
  }
}
```

### Entscheidungskriterien

Methode verschieben wenn:
- Sie mehr Felder/Methoden einer anderen Klasse nutzt als der eigenen
- Die Zielklasse sich wahrscheinlich in Weisen ändert, die diese Methode betreffen
- Verwandte Methoden bereits in der Zielklasse leben

Nicht verschieben wenn:
- Die Methode Features mehrerer Klassen gleichermaßen nutzt (am stabilsten Ort belassen)
- Polymorphismus auf der Quellklasse nötig ist

---

## Move Field

Ein Feld in die Klasse verschieben, die es am meisten nutzt. Ähnlich wie Move Method, aber für Daten.

### Motivation

Ein Feld, das mehr von einer anderen Klasse genutzt wird, signalisiert, dass das Datenmodell nicht zum Verhaltensmodell passt. Das Verschieben hält Daten und Verhalten zusammen.

### Mechanik

1. Wenn das Feld public ist, zuerst kapseln (Encapsulate Field)
2. Feld in der Zielklasse mit Getter und Setter erstellen
3. Bestimmen, wie vom Quell- zum Zielobjekt referenziert wird (meist bestehende Assoziation)
4. Quell-Getter so anpassen, dass er an das Ziel delegiert
5. Tests laufen lassen
6. Feld aus der Quellklasse entfernen
7. Tests laufen lassen

### Beispiel

**Before:**
```python
class Customer:
    def __init__(self):
        self.discount_rate = 0.0

class Order:
    def discounted_total(self):
        return self.base_total() - (self.base_total() * self.customer.discount_rate)
```

`discount_rate` wird nur von `Order` über `Customer` gelesen. Wenn die meiste Logik mit `discount_rate` im Kunden-Pricing-Kontext lebt, in `Customer` belassen. Wenn aber `Order` der primäre Consumer ist und `discount_rate` wirklich Order-Pricing-Policy betrifft, Verschiebung erwägen.

---

## Extract Class

Eine Klasse, die zwei Dinge tut, in zwei Klassen splitten, die jeweils eine Sache tun.

### Motivation

Eine Klasse mit zu vielen Verantwortlichkeiten wächst zu groß und wird schwer verständlich. Wenn du eine kohärente Teilmenge von Feldern und Methoden identifizieren kannst, die eher zueinander als zum Rest der Klasse gehören, verdient diese Teilmenge eine eigene Klasse.

### Mechanik

1. Teilmenge der Verantwortlichkeiten zum Auslagern identifizieren
2. Neue Klasse nach der ausgelagerten Verantwortlichkeit benennen
3. Link von der alten zur neuen Klasse hinzufügen
4. Move Field für jedes Feld in der Teilmenge nutzen
5. Move Method für jede Methode in der Teilmenge nutzen
6. Schnittstellen beider Klassen prüfen. Unnötige Methoden entfernen, umbenennen wo passend.
7. Entscheiden, ob die neue Klasse exponiert oder hinter der Originalklasse versteckt wird
8. Tests laufen lassen

### Beispiel

**Before:**
```javascript
class Person {
  constructor() {
    this.name = '';
    this.officeAreaCode = '';
    this.officeNumber = '';
  }

  get telephoneNumber() {
    return `(${this.officeAreaCode}) ${this.officeNumber}`;
  }
}
```

**After:**
```javascript
class TelephoneNumber {
  constructor() {
    this.areaCode = '';
    this.number = '';
  }

  toString() {
    return `(${this.areaCode}) ${this.number}`;
  }
}

class Person {
  constructor() {
    this.name = '';
    this.telephoneNumber = new TelephoneNumber();
  }

  get telephone() {
    return this.telephoneNumber.toString();
  }
}
```

### Signale für Extraktion

| Signal | Was extrahieren |
|--------|----------------|
| Feldnamen-Präfix-Gruppen (z. B. `shippingStreet`, `shippingCity`) | `ShippingAddress`-Klasse |
| Methoden, die nur eine Teilmenge von Feldern nutzen | Teilmenge + ihre Methoden = neue Klasse |
| Teilmengen ändern sich mit unterschiedlicher Rate | Die schneller ändernde Teilmenge verdient eigene Klasse |
| Teilmengen haben verschiedene Collaborators | Jede Collaborator-Beziehung = potenzielle Klassengrenze |

---

## Inline Class

Das Inverse von Extract Class. Eine Klasse, die ihr Gewicht nicht mehr trägt, in eine andere Klasse mergen.

### Motivation

Eine Klasse, die zu wenig tut — vielleicht nach früheren Refactorings, die Verantwortlichkeiten woandershin verschoben haben — fügt Komplexität ohne Wert hinzu. Zurück in die Klasse falten, die sie nutzt.

### Mechanik

1. Für jede öffentliche Methode und jedes Feld der Quellklasse entsprechendes Mitglied in der Zielklasse erstellen
2. Alle Referenzen auf die Quellklasse auf die Zielklasse umstellen
3. Tests laufen lassen
4. Quellklasse löschen
5. Tests laufen lassen

### Wann nutzen

- Die Klasse hat nur ein oder zwei triviale Methoden
- Die Klasse entstand durch Extract Class, aber spätere Refactorings haben sie geleert
- Die Klasse fügt Indirection ohne Logik, Validierung oder eigenes Verhalten hinzu

---

## Hide Delegate

Kapseln, dass ein Objekt an ein anderes delegiert. Methode auf dem Server erstellen, die den Delegate vor dem Client versteckt — Law of Demeter durchsetzen.

### Motivation

Wenn ein Client `person.getDepartment().getManager()` aufruft, kennt er die `Department`-Klasse — er ist an die Navigationsstruktur gekoppelt. Ändert `Department` seine Schnittstelle, bricht der Client. Mit `person.getManager()` (intern `department.getManager()`), kennt der Client nur `Person`.

### Mechanik

1. Für jede Methode, die der Client auf dem Delegate aufruft, einfache Delegationsmethode auf dem Server erstellen
2. Client auf Server-Methode umstellen
3. Wenn kein Client mehr den Delegate-Accessor braucht, ihn entfernen
4. Tests laufen lassen

### Beispiel

**Before:**
```python
# Client code:
manager = person.department.manager
```

**After:**
```python
class Person:
    @property
    def manager(self):
        return self.department.manager

# Client code:
manager = person.manager
```

### Der Trade-off

Jeden Delegate zu verstecken führt zum Middle Man Smell — eine Klasse, die nur weiterleitet. Die richtige Balance:

| Situation | Aktion |
|-----------|--------|
| Delegate-Schnittstelle ist instabil | Verstecken (Caller vor Änderung schützen) |
| Client nutzt viele Delegate-Methoden | Hide Delegate für jede erwägen |
| Server wird reine Weiterleitung | Remove Middle Man |
| Kette ist tief (a.b.c.d) | Definitiv verstecken |

---

## Remove Middle Man

Das Inverse von Hide Delegate. Wenn eine Klasse hauptsächlich aus Methoden besteht, die an eine andere delegieren, Client den Delegate direkt aufrufen lassen.

### Motivation

Während ein System evolviert, sammeln sich Delegationsmethoden, bis die „Server“-Klasse keinen Wert mehr hat — nur Durchreichen. Dann Indirection entfernen.

### Mechanik

1. Getter für den Delegate auf dem Server erstellen (falls nicht vorhanden)
2. Für jede Delegationsmethode ohne Mehrwert Client auf direkten Delegate-Aufruf umleiten
3. Delegationsmethode vom Server entfernen
4. Tests laufen lassen

### Beispiel

**Before:**
```javascript
class Person {
  get manager() { return this.department.manager; }
  get budget() { return this.department.budget; }
  get headcount() { return this.department.headcount; }
  get location() { return this.department.location; }
  // ... 10 more forwarding methods
}
```

**After:**
```javascript
class Person {
  get department() { return this._department; }
}

// Client:
const manager = person.department.manager;
```

---

## Introduce Foreign Method

Wenn eine Server-Klasse eine zusätzliche Methode braucht, du sie aber nicht ändern kannst (Third-Party-Library, eingefrorenes Modul), die Methode in der Client-Klasse erstellen und das Server-Objekt als erstes Argument übergeben.

### Motivation

Eine Utility-Methode, die „auf“ der Server-Klasse sein sollte, aber nicht hinzugefügt werden kann. Die Foreign Method ist ein Workaround — als solches markieren, damit sie bei Öffnung der Server-Klasse verschoben werden kann.

### Beispiel

```python
# Server class (third-party, can't modify):
# date = Date(year, month, day)

# Foreign method in client:
def next_day(date):
    """Foreign method -- should be on Date class."""
    return Date(date.year, date.month, date.day + 1)
```

---

## Introduce Local Extension

Wenn du mehrere Foreign Methods auf einer Server-Klasse brauchst, die du nicht ändern kannst, neue Klasse erstellen — Subclass oder Wrapper — die die fehlenden Methoden hinzufügt.

### Subclass vs. Wrapper

| Approach | When to Use |
|----------|-------------|
| Subclass | Wenn du die Server-Klasse subclassen kannst; einfachster Ansatz |
| Wrapper (Decorator) | Wenn Subclassing nicht geht (final class); alle Originalmethoden weiterleiten |

### Beispiel (Wrapper)

```javascript
class EnhancedDate {
  constructor(date) {
    this._original = date;
  }

  // Forward original methods
  getYear() { return this._original.getYear(); }
  getMonth() { return this._original.getMonth(); }

  // New methods
  nextDay() {
    return new EnhancedDate(
      new Date(this._original.getTime() + 86400000)
    );
  }

  isWeekend() {
    const day = this._original.getDay();
    return day === 0 || day === 6;
  }
}
```

---

## Entscheidungshilfe: Wo gehört dieses Verhalten hin?

Mit diesen Fragen entscheiden, ob und wohin Code verschoben wird:

| Question | If Yes | Action |
|----------|--------|--------|
| Nutzt diese Methode mehr Features einer anderen Klasse? | Feature Envy | Move Method in diese Klasse |
| Wird dieses Feld mehr von einer anderen Klasse genutzt? | Falsch platzierte Daten | Move Field in diese Klasse |
| Hat diese Klasse zwei Feldgruppen ohne Interaktion? | Mehrere Verantwortlichkeiten | Extract Class |
| Ist diese Klasse nur ein dünner Wrapper ohne Logik? | Unnötige Indirection | Inline Class |
| Navigiert der Client durch eine Objektkette? | Enge Kopplung | Hide Delegate |
| Leitet diese Klasse nur Aufrufe weiter? | Middle Man Smell | Remove Middle Man |
| Methode zu Klasse hinzufügen, die du nicht ändern kannst? | Fehlendes Feature | Introduce Foreign Method oder Local Extension |

### Die Verantwortlichkeits-Platzierungs-Heuristik

Bei Unsicherheit, wo eine Methode hin soll, fragen: **„Wenn sich die Daten ändern, die diese Methode nutzt — welche Klasse müsste aktualisiert werden?“** Die Methode gehört in diese Klasse. So bleiben Daten und Verhalten zusammen und der Ripple-Effekt von Änderungen wird minimiert.
