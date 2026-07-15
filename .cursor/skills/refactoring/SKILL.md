---
name: Refactoring
description: Verwenden, wenn du „refactor code“, „rename function“, „extract method“, „reduce complexity“, „migrate code“, „code migration“, „modernize code“, „split class“, „move code“ fragst oder Code-Umstrukturierung und Refactoring erwähnst.
version: 1.0.0
---

# Refactoring

Umfassender Refactoring-Skill für sichere Code-Umstrukturierung, Migrationen und Modernisierung.

## Kernfähigkeiten

### Sicheres Umbenennen

Identifikatoren in der gesamten Codebase umbenennen:

**Rename-Workflow:**

1. **Alle Referenzen finden**: Alle Verwendungen suchen
2. **Scope identifizieren**: Modul-lokal, paketweit oder öffentliche API
3. **Abhängigkeiten prüfen**: Externer Code, der brechen könnte
4. **Umbenennung durchführen**: Alle Vorkommen aktualisieren
5. **Verifizieren**: Tests ausführen, Imports prüfen

**Suchmuster:**

```bash
# Alle Referenzen zu einer Funktion finden
grep -rn "function_name" --include="*.py"

# Klassenverwendungen finden
grep -rn "ClassName" --include="*.py"

# Imports finden
grep -rn "from .* import.*function_name" --include="*.py"
```

**Überlegungen beim Umbenennen:**

- Docstrings aktualisieren, die den alten Namen erwähnen
- Kommentare aktualisieren, die den Namen referenzieren
- Konfigurationsdateien aktualisieren
- Tests aktualisieren
- Deprecation-Zeitraum für öffentliche APIs erwägen

### Methoden-Extraktion

Code in separate Funktionen extrahieren:

**Wann extrahieren:**

- Codeblock ist zu lang (> 20 Zeilen)
- Code ist woanders dupliziert
- Code hat einen klaren einzelnen Zweck
- Code kann unabhängig getestet werden

**Extraktionsprozess:**

1. Zu extrahierenden Codeblock identifizieren
2. Eingaben (Parameter) bestimmen
3. Ausgaben (Rückgabewerte) bestimmen
4. Neue Funktion mit klarem Namen erstellen
5. Originalcode durch Funktionsaufruf ersetzen
6. Tests für neue Funktion hinzufügen

**Vorher:**

```python
def process_order(order):
    # Validate order (candidate for extraction)
    if not order.items:
        raise ValueError("Empty order")
    if order.total < 0:
        raise ValueError("Invalid total")
    for item in order.items:
        if item.quantity <= 0:
            raise ValueError("Invalid quantity")

    # Process payment
    payment_result = gateway.charge(order.total)
    return payment_result
```

**Nachher:**

```python
def validate_order(order: Order) -> None:
    """Validate order has valid items and total."""
    if not order.items:
        raise ValueError("Empty order")
    if order.total < 0:
        raise ValueError("Invalid total")
    for item in order.items:
        if item.quantity <= 0:
            raise ValueError("Invalid quantity")

def process_order(order: Order) -> PaymentResult:
    validate_order(order)
    return gateway.charge(order.total)
```

### Klassen-Aufteilung

Große Klassen in fokussierte Komponenten aufteilen:

**Wann aufteilen:**

- Klasse hat mehrere Verantwortlichkeiten
- Klasse hat > 500 Zeilen
- Methodengruppen arbeiten mit unterschiedlichen Daten
- Tests erfordern übermäßiges Mocking

**Aufteilungsstrategien:**

**Klasse extrahieren:**

```python
# Before: God class
class OrderManager:
    def create_order(self): ...
    def validate_order(self): ...
    def calculate_tax(self): ...
    def calculate_shipping(self): ...
    def send_confirmation_email(self): ...
    def send_shipping_notification(self): ...

# After: Focused classes
class OrderService:
    def create_order(self): ...
    def validate_order(self): ...

class PricingService:
    def calculate_tax(self): ...
    def calculate_shipping(self): ...

class NotificationService:
    def send_confirmation_email(self): ...
    def send_shipping_notification(self): ...
```

### Komplexitätsreduktion

Zyklomatische Komplexität reduzieren:

**Conditionals durch Polymorphismus ersetzen:**

```python
# Before: Complex switch
def calculate_price(product_type, base_price):
    if product_type == "physical":
        return base_price + shipping_cost
    elif product_type == "digital":
        return base_price
    elif product_type == "subscription":
        return base_price * 12 * 0.9
    else:
        raise ValueError("Unknown type")

# After: Polymorphism
class Product(ABC):
    @abstractmethod
    def calculate_price(self, base_price): ...

class PhysicalProduct(Product):
    def calculate_price(self, base_price):
        return base_price + self.shipping_cost

class DigitalProduct(Product):
    def calculate_price(self, base_price):
        return base_price

class Subscription(Product):
    def calculate_price(self, base_price):
        return base_price * 12 * 0.9
```

**Guard Clauses extrahieren:**

```python
# Before: Nested conditionals
def process(data):
    if data:
        if data.is_valid:
            if data.is_ready:
                return do_processing(data)
    return None

# After: Guard clauses
def process(data):
    if not data:
        return None
    if not data.is_valid:
        return None
    if not data.is_ready:
        return None
    return do_processing(data)
```

### Code-Migration

Code zwischen Patterns oder Versionen migrieren:

**Migrationsarten:**

- Python 2 zu 3
- Sync zu Async
- ORM-Migrationen
- API-Versions-Upgrades
- Framework-Migrationen

**Migrations-Workflow:**

1. **Scope bewerten**: Was muss sich ändern
2. **Kompatibilitätsschicht erstellen**: Bei schrittweiser Migration
3. **In Phasen migrieren**: Mit risikoarmen Bereichen beginnen
4. **Tests beibehalten**: Verhalten sicherstellen
5. **Alten Code entfernen**: Nach Migration aufräumen

**Beispiel: Sync zu Async**

```python
# Before: Synchronous
def fetch_user(user_id: int) -> User:
    response = requests.get(f"/users/{user_id}")
    return User.from_dict(response.json())

# After: Asynchronous
async def fetch_user(user_id: int) -> User:
    async with aiohttp.ClientSession() as session:
        async with session.get(f"/users/{user_id}") as response:
            data = await response.json()
            return User.from_dict(data)
```

## Refactoring-Workflow

### Sicherer Refactoring-Prozess

1. **Testabdeckung sicherstellen**: Tests hinzufügen, falls fehlend
2. **Tests ausführen**: Grüne Baseline verifizieren
3. **Kleine Änderung**: Ein Refactoring nach dem anderen
4. **Tests erneut ausführen**: Weiterhin grün verifizieren
5. **Committen**: Fortschritt speichern
6. **Wiederholen**: Mit nächster Änderung fortfahren

### Checkliste vor dem Refactoring

```
[ ] Tests für betroffenen Code vorhanden
[ ] Alle Tests bestanden
[ ] Änderung ist gut verstanden
[ ] Impact-Scope identifiziert
[ ] Rollback-Plan existiert
```

### Verifikation nach dem Refactoring

```
[ ] Alle Tests bestehen weiterhin
[ ] Keine neuen Lint-Fehler
[ ] Type-Checking bestanden
[ ] Funktionalität unverändert
[ ] Performance akzeptabel
```

## Häufige Refactorings

### Toten Code entfernen

```python
# Finden und entfernen:
# - Ungenutzte Imports
# - Ungenutzte Variablen
# - Unerreichbarer Code
# - Auskommentierter Code
# - Veraltete Funktionen

# Tools:
# - vulture (Python)
# - autoflake (Python)
# - eslint (JavaScript)
```

### Ausdrücke vereinfachen

```python
# Before
if condition == True:
    return True
else:
    return False

# After
return condition
```

### Erklärende Variablen einführen

```python
# Before
if user.age >= 18 and user.country in ['US', 'CA'] and user.verified:
    allow_purchase()

# After
is_adult = user.age >= 18
is_supported_country = user.country in ['US', 'CA']
is_verified = user.verified
can_purchase = is_adult and is_supported_country and is_verified

if can_purchase:
    allow_purchase()
```

## Sicherheitsrichtlinien

### Breaking Changes

Beim Refactoring öffentlicher APIs:

1. Zuerst Deprecation-Warnungen hinzufügen
2. Rückwärtskompatibilität beibehalten
3. Migrationspfad dokumentieren
4. Nach Deprecation-Zeitraum entfernen

### Datenbank-Migrationen

Beim Refactoring von Datenmodellen:

1. Migrationsskripte erstellen
2. Migrationen auf Datenkopie testen
3. Rollback planen
4. Zero-Downtime-Migrationen erwägen

### Performance-Auswirkungen

Nach dem Refactoring:

1. Performance-Benchmarks ausführen
2. Mit Baseline vergleichen
3. Bei Regression profilen
4. Bei Bedarf optimieren

## Integration

Mit anderen Skills koordinieren:

- **code-quality skill**: Verbesserung messen
- **test-coverage skill**: Testabdeckung sicherstellen
- **architecture-review skill**: Strukturänderungen validieren
- **git-workflows skill**: Passende Commit-Messages für Refactorings
