---
name: design-patterns-implementation
description: >-
  Wendet passende Design Patterns (Singleton, Factory, Observer, Strategy usw.)
  an, um Architekturprobleme zu lösen. Verwenden beim Refactoring der
  Code-Architektur, bei erweiterbaren Systemen oder SOLID-Prinzipien.
---

# Design Patterns implementieren

## Überblick

Bewährte Design Patterns anwenden für wartbare, erweiterbare und testbare Code-Architekturen.

## Wann nutzen

- Häufige Architekturprobleme lösen
- Code wartbarer und testbarer machen
- Erweiterbare Plugin-Systeme implementieren
- Komponenten entkoppeln
- SOLID-Prinzipien folgen
- Code Reviews mit Architektur-Themen

## Häufige Design Patterns

### 1. **Singleton Pattern**

Stelle sicher, dass eine Klasse nur eine Instanz mit globalem Zugriff hat.

```typescript
class DatabaseConnection {
  private static instance: DatabaseConnection;
  private connection: any;

  private constructor() {
    this.connection = this.createConnection();
  }

  public static getInstance(): DatabaseConnection {
    if (!DatabaseConnection.instance) {
      DatabaseConnection.instance = new DatabaseConnection();
    }
    return DatabaseConnection.instance;
  }

  private createConnection() {
    return { /* connection logic */ };
  }
}

// Usage
const db1 = DatabaseConnection.getInstance();
const db2 = DatabaseConnection.getInstance();
// db1 === db2 (same instance)
```

### 2. **Factory Pattern**

Erzeuge Objekte, ohne die exakten Klassen anzugeben.

```python
from abc import ABC, abstractmethod

class PaymentProcessor(ABC):
    @abstractmethod
    def process_payment(self, amount: float) -> bool:
        pass

class StripeProcessor(PaymentProcessor):
    def process_payment(self, amount: float) -> bool:
        # Stripe-specific logic
        return True

class PayPalProcessor(PaymentProcessor):
    def process_payment(self, amount: float) -> bool:
        # PayPal-specific logic
        return True

class PaymentProcessorFactory:
    @staticmethod
    def create_processor(processor_type: str) -> PaymentProcessor:
        if processor_type == 'stripe':
            return StripeProcessor()
        elif processor_type == 'paypal':
            return PayPalProcessor()
        else:
            raise ValueError(f'Unknown processor: {processor_type}')

# Usage
processor = PaymentProcessorFactory.create_processor('stripe')
processor.process_payment(100.00)
```

### 3. **Observer Pattern**

Definiere eine Eins-zu-Viele-Abhängigkeit für Event-Benachrichtigungen.

```javascript
class Subject {
  constructor() {
    this.observers = [];
  }

  attach(observer) {
    this.observers.push(observer);
  }

  detach(observer) {
    this.observers = this.observers.filter(obs => obs !== observer);
  }

  notify(data) {
    this.observers.forEach(observer => observer.update(data));
  }
}

class Observer {
  update(data) {
    console.log('Received update:', data);
  }
}

// Usage
const subject = new Subject();
const observer1 = new Observer();
const observer2 = new Observer();

subject.attach(observer1);
subject.attach(observer2);
subject.notify({ event: 'data_changed' });
```

### 4. **Strategy Pattern**

Definiere eine Familie von Algorithmen und mache sie austauschbar.

```java
interface CompressionStrategy {
    byte[] compress(byte[] data);
}

class ZipCompression implements CompressionStrategy {
    public byte[] compress(byte[] data) {
        // ZIP compression logic
        return data;
    }
}

class GzipCompression implements CompressionStrategy {
    public byte[] compress(byte[] data) {
        // GZIP compression logic
        return data;
    }
}

class FileCompressor {
    private CompressionStrategy strategy;

    public FileCompressor(CompressionStrategy strategy) {
        this.strategy = strategy;
    }

    public void setStrategy(CompressionStrategy strategy) {
        this.strategy = strategy;
    }

    public byte[] compressFile(byte[] data) {
        return strategy.compress(data);
    }
}

// Usage
FileCompressor compressor = new FileCompressor(new ZipCompression());
compressor.compressFile(fileData);

// Change strategy at runtime
compressor.setStrategy(new GzipCompression());
compressor.compressFile(fileData);
```

### 5. **Decorator Pattern**

Füge Objekten dynamisch Verantwortlichkeiten hinzu.

```typescript
interface Coffee {
  cost(): number;
  description(): string;
}

class SimpleCoffee implements Coffee {
  cost(): number {
    return 5;
  }

  description(): string {
    return 'Simple coffee';
  }
}

class MilkDecorator implements Coffee {
  constructor(private coffee: Coffee) {}

  cost(): number {
    return this.coffee.cost() + 2;
  }

  description(): string {
    return this.coffee.description() + ', milk';
  }
}

class SugarDecorator implements Coffee {
  constructor(private coffee: Coffee) {}

  cost(): number {
    return this.coffee.cost() + 1;
  }

  description(): string {
    return this.coffee.description() + ', sugar';
  }
}

// Usage
let coffee: Coffee = new SimpleCoffee();
console.log(coffee.cost()); // 5

coffee = new MilkDecorator(coffee);
console.log(coffee.cost()); // 7

coffee = new SugarDecorator(coffee);
console.log(coffee.cost()); // 8
console.log(coffee.description()); // "Simple coffee, milk, sugar"
```

### 6. **Repository Pattern**

Kapsle Datenzugriffslogik ab.

```python
from abc import ABC, abstractmethod
from typing import List, Optional

class UserRepository(ABC):
    @abstractmethod
    def find_by_id(self, user_id: int) -> Optional[User]:
        pass

    @abstractmethod
    def find_all(self) -> List[User]:
        pass

    @abstractmethod
    def save(self, user: User) -> User:
        pass

    @abstractmethod
    def delete(self, user_id: int) -> bool:
        pass

class DatabaseUserRepository(UserRepository):
    def __init__(self, db_connection):
        self.db = db_connection

    def find_by_id(self, user_id: int) -> Optional[User]:
        result = self.db.query('SELECT * FROM users WHERE id = ?', user_id)
        return User.from_dict(result) if result else None

    def find_all(self) -> List[User]:
        results = self.db.query('SELECT * FROM users')
        return [User.from_dict(r) for r in results]

    def save(self, user: User) -> User:
        self.db.execute('INSERT INTO users (...) VALUES (...)', user.to_dict())
        return user

    def delete(self, user_id: int) -> bool:
        return self.db.execute('DELETE FROM users WHERE id = ?', user_id)
```

### 7. **Dependency Injection**

Kehre die Kontrolle um, indem du Abhängigkeiten injizierst.

```typescript
// Bad: Hard-coded dependencies
class OrderService {
  private db = new MySQLDatabase(); // Tightly coupled
  private email = new GmailService(); // Tightly coupled

  createOrder(order: Order) {
    this.db.save(order);
    this.email.send(order.customer_email, 'Order created');
  }
}

// Good: Dependency injection
interface Database {
  save(entity: any): void;
}

interface EmailService {
  send(to: string, subject: string): void;
}

class OrderService {
  constructor(
    private db: Database,
    private email: EmailService
  ) {}

  createOrder(order: Order) {
    this.db.save(order);
    this.email.send(order.customer_email, 'Order created');
  }
}

// Usage - easy to test with mocks
const service = new OrderService(
  new MySQLDatabase(),
  new GmailService()
);

// Test with mocks
const testService = new OrderService(
  new MockDatabase(),
  new MockEmailService()
);
```

## Best Practices

### ✅ DO
- Wähle Patterns, die echte Probleme lösen
- Halte Patterns einfach und verständlich
- Dokumentiere, warum Patterns gewählt wurden
- Denke an Testbarkeit
- Folge SOLID-Prinzipien
- Nutze Dependency Injection
- Bevorzuge Komposition vor Vererbung

### ❌ DON'T
- Wende Patterns an, ohne sie zu verstehen
- Over-engineere einfache Lösungen
- Erzwinge Patterns, wo sie nicht passen
- Erzeuge unnötige Abstraktionsschichten
- Ignoriere die Vertrautheit des Teams mit Patterns

## Wann welches Pattern nutzen

| Pattern | Anwendungsfall |
|---------|----------|
| **Singleton** | Datenbankverbindungen, Konfigurationsmanager |
| **Factory** | Objekte basierend auf Laufzeitbedingungen erzeugen |
| **Observer** | Event-Systeme, Pub/Sub, reaktive Programmierung |
| **Strategy** | Algorithmen, die zur Laufzeit ausgetauscht werden können |
| **Decorator** | Features dynamisch hinzufügen ohne Vererbung |
| **Repository** | Datenzugriff von Business-Logik abstrahieren |
| **Adapter** | Inkompatible Interfaces zusammenarbeiten lassen |
| **Facade** | Komplexe Subsysteme vereinfachen |
| **Command** | Undo/Redo, Task-Queuing, Macro-Recording |

## Ressourcen

- „Design Patterns“ von Gang of Four
- „Head First Design Patterns“ von Freeman & Freeman
- refactoring.guru/design-patterns
