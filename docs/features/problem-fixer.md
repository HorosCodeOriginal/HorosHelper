# Problem-Fixer

**Sidebar:** Problem-Fixer · **Mockup:** `assets/images/dashboard-mockup-02/`

## Zweck

Automatische Erkennung häufiger Windows-Probleme mit Ein-Klick-Reparatur und Rollback.

## Kern-Services

- `IProblemScannerService` — Plugin-Architektur (`IProblemCheck`, `IRepairAction`)
- `IRollbackStore` — Snapshots unter `%LocalAppData%\HorosHelper\rollback\`

## Reparaturen

DNS-Flush, Winsock, Update-Cache, SFC/DISM, Registry, Suchdienst, Temp-Bereinigung

## UI

- Scan-Fortschritt, Problem-Karten, UAC-Shield auf Admin-Aktionen

## Tests

- `ProblemScannerServiceTests`, `RepairCommandBuilderTests`, Registry/Rollback-Tests
