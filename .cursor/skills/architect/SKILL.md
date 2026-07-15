---
name: architect
description: Systemarchitektur entwerfen und technische Strategie auf hoher Ebene festlegen

allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
---

# Architektur-Design

Systemarchitektur entwerfen und strategische technische Entscheidungen treffen.

## Kernprinzip

**Gute Architektur ermöglicht Änderung bei gleichzeitiger Einfachheit.**

## Name

han-core:architect - Systemarchitektur entwerfen und technische Strategie auf hoher Ebene festlegen

## Synopsis

```
/architect [arguments]
```

## Architektur vs. Planung

**Architektur-Design (dieser Skill):**

- Strategisch: „Wie soll das System strukturiert sein?“
- Komponenten-Interaktionen und Grenzen
- Technologie- und Pattern-Wahl
- Langfristige Auswirkungen
- Entscheidungen auf Systemebene

**Technische Planung:**

- Taktisch: „Wie implementiere ich Feature X?“
- Konkrete Implementierungsaufgaben
- Ausführungsdetails
- Kurzfristiger Fokus

**Nutze /architect, wenn:**

- Du neue Systeme oder Subsysteme entwirfst
- Signifikante Systemänderung (neues Subsystem, großes Refactoring)
- Mehrere Komponenten oder Teams betroffen sind
- Große Refactorings mehrere Komponenten betreffen
- Technologieauswahl ansteht
- Systemgrenzen und Interfaces definierst
- Langfristige technische Strategie nötig ist
- Du mehrere Ansätze evaluieren musst
- Entscheidungen breite Auswirkungen haben

**Nutze /plan, wenn:**

- Du innerhalb bestehender Architektur implementierst
- Du ein konkretes Feature in bestehender Architektur umsetzt
- Taktische Ausführungsplanung
- Bekannte Arbeit aufteilen
- Architektur bereits feststeht
- Task-Sequenzierung und Ausführung

## Architektur-Prozess

### 1. Kontext verstehen

**Business-Kontext:**

- Welches Problem lösen wir?
- Wer sind die Nutzer?
- Was sind die Business-Ziele?
- Was sind die Erfolgsmetriken?

**Technischer Kontext:**

- Was existiert heute?
- Welche Constraints gibt es?
- Womit müssen wir integrieren?
- Welche Skalierung müssen wir unterstützen?

**Team-Kontext:**

- Was ist unsere Expertise?
- Was können wir warten?
- Wie ist unsere Velocity?

### 2. Anforderungen sammeln

**Funktionale Anforderungen:**

- Was muss das System tun?
- Was sind die Features?
- Was sind die Nutzerszenarien?

**Nicht-funktionale Anforderungen:**

- **Performance**: Antwortzeit, Durchsatz
- **Skalierbarkeit**: Erwartete Last, Wachstum
- **Verfügbarkeit**: Uptime-Anforderungen
- **Sicherheit**: Compliance, Datenschutz
- **Wartbarkeit**: Teamgröße, Skills
- **Kosten**: Budget-Constraints

**Beispiel:**

```markdown
## Requirements

### Functional
- Users can search products by name/category
- Users can add items to cart
- Users can checkout and pay

### Non-Functional
- Search response time < 200ms (p95)
- Support 10,000 concurrent users
- 99.9% uptime
- PCI DSS compliant for payments
- Team of 5 developers can maintain
```

### 3. Constraints identifizieren

**Technische Constraints:**

- Muss bestehendes Authentifizierungssystem nutzen
- Muss mit Legacy-Inventarsystem integrieren
- Datenbank muss PostgreSQL sein (bestehende Infrastruktur)

**Business-Constraints:**

- Budget für Infrastruktur
- Muss EU-Datenresidenz unterstützen

**Team-Constraints:**

- Team erfahren in Python, weniger in Go
- Kein DevOps-Spezialist im Team
- Remote-Team über Zeitzonen verteilt

### 4. Alternativen betrachten

**Entwirf nie im Vakuum — betrachte Optionen:**

**Beispiel: Datenspeicher-Wahl**

**Option 1: PostgreSQL**

- Pros: Team kennt es, ACID-Garantien, reiche Query-Unterstützung
- Cons: Vertikale Skalierungsgrenzen, Setup-Komplexität

**Option 2: MongoDB**

- Pros: Flexibles Schema, horizontale Skalierung
- Cons: Team unerfahren, Eventual Consistency

**Option 3: DynamoDB**

- Pros: Vollständig managed, Auto-Scaling
- Cons: Vendor Lock-in, Query-Limitierungen, Kosten bei Skalierung

**Entscheidung: PostgreSQL**

- Team-Expertise überwiegt Skalierungsbedenken
- Kann neu evaluiert werden, wenn Skalierung zum Thema wird
- Schnellere initiale Entwicklung

### 5. Systemstruktur entwerfen

**Definiere Komponenten und ihre Verantwortlichkeiten:**

```
┌─────────────────────────────────────────────┐
│             Client Apps                      │
│  (Web, iOS, Android)                         │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│          API Gateway / Load Balancer         │
└────────────────┬────────────────────────────┘
                 │
        ┌────────┴────────┐
        ▼                 ▼
┌───────────────┐  ┌───────────────┐
│   Auth        │  │   Core API     │
│   Service     │  │   Service      │
└───────┬───────┘  └───────┬───────┘
        │                  │
        │         ┌────────┴────────┐
        │         ▼                 ▼
        │  ┌──────────────┐  ┌──────────────┐
        │  │  PostgreSQL  │  │   Redis      │
        │  │  (Primary)   │  │   (Cache)    │
        │  └──────────────┘  └──────────────┘
        │
        ▼
┌───────────────┐
│   User DB     │
└───────────────┘
```

**Komponentenbeschreibungen:**

```markdown
## Components

### API Gateway
**Responsibility:** Route requests, rate limiting, authentication
**Technology:** Nginx
**Dependencies:** Auth Service, Core API Service
**Scale:** 2-3 instances behind load balancer

### Auth Service
**Responsibility:** User authentication, session management, JWT issuing
**Technology:** Python (Flask), PostgreSQL
**API:** REST
**Scale:** Stateless, 2-N instances

### Core API Service
**Responsibility:** Business logic, data access, external integrations
**Technology:** Python (FastAPI), PostgreSQL, Redis
**API:** REST
**Scale:** Stateless, 2-N instances

### PostgreSQL
**Responsibility:** Primary data store
**Scale:** Primary with read replica

### Redis
**Responsibility:** Session storage, caching, rate limiting
**Scale:** Cluster mode (3 nodes)
```

### 6. Interfaces definieren

**API-Verträge:**

```markdown
## API Design

### POST /api/auth/login
**Purpose:** Authenticate user, issue JWT

**Request:**
```json
{
  "email": "user@example.com",
  "password": "secure_password"
}
```

**Response (200):**

```json
{
  "token": "eyJ...",
  "user": {
    "id": "123",
    "email": "user@example.com",
    "name": "John Doe"
  }
}
```

**Errors:**

- 400: Invalid request
- 401: Invalid credentials
- 429: Rate limit exceeded

```

### 7. Für Ausfälle planen

**Was kann schiefgehen?**
- Datenbank nicht verfügbar
- Externe API down
- Netzwerk-Partition
- Hohe Last
- Datenkorruption

**Mitigationsstrategien:**
- Retry mit exponentiellem Backoff
- Circuit Breaker für externe Services
- Graceful Degradation
- Health Checks und Monitoring
- Datenbank-Backups

**Beispiel:**
```markdown
## Failure Scenarios

### Database Unavailable
**Impact:** Cannot read/write data
**Mitigation:**
- Read replica failover (automated)
- Circuit breaker after 3 failures
- Cache serves stale data for 5 minutes
- User sees degraded experience message
**Recovery:** Manual failover to replica, fix primary

### External Payment API Down
**Impact:** Cannot process payments
**Mitigation:**
- Retry 3 times with exponential backoff
- Queue payments for later processing
- User notified of delay
- Alert on-call engineer
**Recovery:** Process queued payments once API recovers
```

### 8. Entscheidungen dokumentieren

**Architecture Decision Record (ADR):**

```markdown
# ADR-001: Use PostgreSQL for Primary Database

**Status:** Accepted
**Date:** 2024-01-15
**Deciders:** Tech Lead, Backend Team

## Context

We need to choose a primary database for user data, products, and orders.

Requirements:
- Strong consistency (ACID)
- Complex queries (joins, aggregations)
- < 200ms query time for 90% of queries
- Support 100k users initially

## Decision

Use PostgreSQL as primary database.

## Alternatives Considered

### MongoDB
- **Pros:** Flexible schema, horizontal scaling
- **Cons:** Team unfamiliar, eventual consistency issues
- **Why not:** Team expertise more valuable than flexibility

### DynamoDB
- **Pros:** Managed service, auto-scaling
- **Cons:** Vendor lock-in, limited query capability, cost
- **Why not:** Query limitations would hurt development velocity

### MySQL
- **Pros:** Similar to PostgreSQL, team knows it
- **Cons:** Less feature-rich than PostgreSQL
- **Why not:** PostgreSQL offers JSON support, better full-text search

## Consequences

**Positive:**
- Team can be productive immediately
- Strong consistency guarantees
- Rich query capabilities
- JSON support for flexible data

**Negative:**
- Vertical scaling limits (mitigated: can add read replicas)
- More complex than managed services (mitigated: use RDS)
- Higher operational overhead

**Trade-offs:**
- Chose familiarity over horizontal scaling
- Chose rich queries over eventual consistency
- Can re-evaluate if scale requirements change

## Validation

- Team confirmed expertise in PostgreSQL
- Load testing shows meets performance requirements
- Cost analysis shows acceptable for first year
```

## Architektur-Dokumentstruktur

```markdown
# Architecture Design: [System/Feature Name]

## Context

### Problem Statement
[What business problem are we solving?]

### Goals
[What are we trying to achieve?]

### Non-Goals
[What are we explicitly NOT trying to achieve?]

### Requirements

**Functional:**
- [Requirement 1]
- [Requirement 2]

**Non-Functional:**
- Performance: [e.g., < 200ms response time]
- Scalability: [e.g., handle 10k concurrent users]
- Security: [e.g., PCI compliance]
- Maintainability: [e.g., easy to modify]

### Constraints
[Technical, time, resource, or business constraints]

## Current Architecture
[What exists today? What needs to change?]

## Proposed Architecture

### High-Level Design

[Diagram or description of system components]

```

┌─────────────┐     ┌─────────────┐     ┌──────────────┐
│   Client    │────▶│   API       │────▶│  Database    │
└─────────────┘     └─────────────┘     └──────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │  Cache      │
                    └─────────────┘

```

### Components

#### Component A
**Responsibility:** [What it does]
**Interface:** [How others interact with it]
**Dependencies:** [What it depends on]
**Technology:** [Implementation stack]

#### Component B
[Similar structure...]

### Data Flow
[How data moves through the system]

### API Design
[Key endpoints, schemas, contracts]

### Data Model
[Database schema, key entities]

### Security Model
[Authentication, authorization, data protection]

## Alternative Approaches Considered

### Alternative 1: [Approach name]
**Pros:**
- [Advantage 1]
- [Advantage 2]

**Cons:**
- [Disadvantage 1]
- [Disadvantage 2]

**Why not chosen:** [Reasoning]

### Alternative 2: [Another approach]
[Similar structure...]

## Decision Rationale

### Why This Architecture?
[Explain the key decisions and trade-offs]

### Trade-offs Accepted
[What we gave up for what benefits]

### Assumptions
[What we're assuming to be true]

## Implementation Strategy

### Phase 1: [Foundation]
[What to build first]

### Phase 2: [Core Features]
[Next phase]

### Phase 3: [Enhancement]
[Final phase]

### Migration Strategy
[If replacing existing system, how to transition?]

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| [Risk 1] | High | Medium | [How to mitigate] |
| [Risk 2] | Medium | Low | [How to mitigate] |

## Testing Strategy
[How will we validate this architecture?]

## Monitoring & Observability
[How will we know if it's working?]

## Success Metrics
[How will we measure success?]

## Open Questions
[What still needs to be resolved?]

## References
[Links to research, related docs, RFCs]
```

## Architektur-Prinzipien

### 1. Einfachheit

**Starte einfach, füge Komplexität nur bei Bedarf hinzu.**

```
BAD: Microservices from day 1 with 20 services
GOOD: Start with monolith, split when needed
```

**YAGNI anwenden:** You Aren't Gonna Need It

- Nicht für hypothetische Zukunft bauen
- Hinzufügen, wenn wirklich nötig
- Einfacher ist leichter wartbar

### 2. Separation of Concerns

**Jede Komponente hat eine klare Verantwortlichkeit.**

```
GOOD:
- Auth Service: Authentication only
- User Service: User profile management
- Order Service: Order processing

BAD:
- God Service: Does everything
```

**SOLID-Prinzipien anwenden:**

- Single Responsibility
- Open/Closed
- Liskov Substitution
- Interface Segregation
- Dependency Inversion

### 3. Loose Coupling

**Komponenten hängen von Interfaces ab, nicht von Implementierungen.**

```typescript
// BAD: Tight coupling
class OrderService {
  constructor(private db: PostgresDatabase) {}
}

// GOOD: Loose coupling
class OrderService {
  constructor(private db: Database) {}  // Interface
}
```

**Vorteile:**

- Einfacher zu testen (Interface mocken)
- Einfacher Implementierungen zu tauschen
- Komponenten können unabhängig evolvieren

### 4. High Cohesion

**Zusammengehörige Funktionalität bleibt zusammen.**

```
GOOD:
user/
  - create_user.ts
  - update_user.ts
  - delete_user.ts
  - user_repository.ts

BAD:
create/
  - create_user.ts
  - create_order.ts
update/
  - update_user.ts
  - update_order.ts
```

### 5. Explicit Over Implicit

**Mache Abhängigkeiten und Verträge klar.**

```typescript
// BAD: Implicit dependency
function processOrder(orderId: string) {
  const db = global.database  // Where does this come from?
  // ...
}

// GOOD: Explicit dependency
function processOrder(
  orderId: string,
  db: Database,
  logger: Logger
) {
  // Dependencies are clear
}
```

### 6. Fail Fast

**Erkenne und melde Fehler früh.**

```typescript
// BAD: Silent failure
function divide(a: number, b: number) {
  if (b === 0) return 0  // Wrong!
  return a / b
}

// GOOD: Fail fast
function divide(a: number, b: number) {
  if (b === 0) {
    throw new Error('Division by zero')
  }
  return a / b
}
```

### 7. Für Testbarkeit designen

**Mache es einfach zu testen.**

```typescript
// BAD: Hard to test
class OrderService {
  processOrder(orderId: string) {
    const db = new PostgresDatabase()  // Can't mock
    const api = new PaymentAPI()       // Can't mock
    // ...
  }
}

// GOOD: Easy to test
class OrderService {
  constructor(
    private db: Database,      // Can inject mock
    private api: PaymentAPI    // Can inject mock
  ) {}

  processOrder(orderId: string) {
    // ...
  }
}
```

## Häufige Architektur-Patterns

### Layered Architecture

```
┌─────────────────────┐
│  Presentation       │ (UI, API controllers)
├─────────────────────┤
│  Business Logic     │ (Domain, services)
├─────────────────────┤
│  Data Access        │ (Repositories, ORMs)
├─────────────────────┤
│  Database           │ (Storage)
└─────────────────────┘
```

**Wann nutzen:** Einfache bis moderate Komplexität

### Hexagonal Architecture (Ports & Adapters)

```
        ┌───────────────────────┐
        │   External Systems    │
        │  (UI, DB, APIs)       │
        └──────────┬────────────┘
                   │
        ┌──────────▼────────────┐
        │      Adapters         │ (Implementation)
        │  (REST, PostgreSQL)   │
        └──────────┬────────────┘
                   │
        ┌──────────▼────────────┐
        │       Ports           │ (Interfaces)
        │  (IUserRepo, IAuth)   │
        └──────────┬────────────┘
                   │
        ┌──────────▼────────────┐
        │    Core Domain        │ (Business logic)
        │    (Pure logic)       │
        └───────────────────────┘
```

**Wann nutzen:** Business-Logik isolieren, mehrere Frontends

### Microservices

```
┌─────────┐  ┌─────────┐  ┌─────────┐
│  User   │  │  Order  │  │ Payment │
│ Service │  │ Service │  │ Service │
└────┬────┘  └────┬────┘  └────┬────┘
     │            │            │
     └────────────┴────────────┘
                  │
          ┌───────▼────────┐
          │  Message Bus   │
          │  (Event-driven)│
          └────────────────┘
```

**Wann nutzen:** Großes Team, unabhängiges Deploy, klare Grenzen

**Vermeiden, wenn:** Kleines Team, unklare Grenzen, frühe Phase

### Event-Driven Architecture

```
┌─────────┐       ┌─────────────┐       ┌─────────┐
│Producer │──────▶│ Event Bus   │──────▶│Consumer │
└─────────┘       └─────────────┘       └─────────┘
```

**Wann nutzen:** Async-Verarbeitung, entkoppelte Systeme, Audit Trails

## Architektur-Patterns zum Betrachten

**System-Patterns:**

- Layered architecture
- Microservices vs. Monolith
- Event-driven architecture
- CQRS (Command Query Responsibility Segregation)
- Hexagonal architecture

**Data-Patterns:**

- Database per service
- Shared database
- Event sourcing
- CQRS
- Cache-aside

**Integration-Patterns:**

- API Gateway
- Service mesh
- Message queue
- Pub/sub
- GraphQL federation

## Anti-Patterns

### Premature Optimization

**Optimiere nicht für Skalierung, die du nicht hast.**

```
BAD: Build microservices for 100 users
GOOD: Start with monolith, split when needed
```

### Resume-Driven Architecture

**Wähle Technologie nicht zum Lebenslauf-Aufbau.**

```
BAD: "I want to learn Kubernetes, let's use it"
GOOD: "Kubernetes fits our scale needs"
```

### Distributed Monolith

**Microservices, die eng gekoppelt sind.**

```
BAD: Service A can't deploy without Service B
GOOD: Services are independently deployable
```

### Big Ball of Mud

**Keine Struktur, alles hängt von allem ab.**

```
BAD: Any code can call any other code
GOOD: Clear layers and boundaries
```

### Analysis Paralysis

**Zu viel analysieren, nie shippen.**

```
BAD: Spend months on perfect architecture
GOOD: Design enough to start, iterate
```

## Architektur-Review-Checkliste

- [ ] Business-Ziele klar verstanden
- [ ] Funktionale Anforderungen dokumentiert
- [ ] Nicht-funktionale Anforderungen definiert
- [ ] Constraints identifiziert
- [ ] Mehrere Alternativen betrachtet
- [ ] Trade-offs explizit genannt
- [ ] Komponenten-Verantwortlichkeiten klar
- [ ] Interfaces gut definiert
- [ ] Datenfluss dokumentiert
- [ ] Ausfallszenarien geplant
- [ ] Sicherheitsmodell definiert
- [ ] Skalierbarkeit adressiert
- [ ] Testbarkeit eingebaut
- [ ] Entscheidungen dokumentiert (ADRs)
- [ ] Implementierungsphasen skizziert
- [ ] Team kann implementieren und warten
- [ ] Erfolgsmetriken definiert

## Beispiele

Wenn der User sagt:

- „Entwirf die Architektur für unser Multi-Tenant-System“
- „Wie sollten wir unsere Microservices strukturieren?“
- „Plane den technischen Ansatz für Echtzeit-Benachrichtigungen“
- „Entwirf das Datenmodell für unseren Marktplatz“
- „Erstelle Architektur für Migration von Monolith zu Services“

## Integration mit anderen Skills

- **solid-principles** anwenden — Komponenten-Design leiten
- **simplicity-principles** anwenden — KISS, YAGNI
- **orthogonality-principle** anwenden — unabhängige Komponenten
- **structural-design-principles** anwenden — Kompositions-Patterns
- **technical-planning** nutzen — für Implementierung nach dem Design

## Hinweise

- TaskCreate nutzen, um Architektur-Design-Schritte zu tracken
- Alle relevanten Design-Principle-Skills anwenden
- Diagramme erstellen (ASCII-Art oder Zeichen-Tools referenzieren)
- Architektur evolviert — Entscheidungen und Änderungen dokumentieren
- /plan für detaillierte Implementierung nach Architektur-Freigabe erwägen
- Entscheidungen als ADRs archivieren für spätere Referenz

## Merke

1. **Einfachheit zuerst** — Starte einfach, füge Komplexität bei Bedarf hinzu
2. **Entscheidungen dokumentieren** — Dein zukünftiges Ich wird es dir danken
3. **Alternativen betrachten** — Nie nur die erste Idee
4. **Trade-offs nennen** — Jede Entscheidung hat Konsequenzen
5. **Für Änderung designen** — Systeme evolvieren

**Die beste Architektur ist die, die einfach genug zum Shippen und flexibel genug zum Evolvieren ist.**
