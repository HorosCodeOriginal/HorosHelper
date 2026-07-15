# Dashboard

**Sidebar:** Dashboard · **Mockup:** `assets/images/dashboard-mockup-01/`

## Zweck

Echtzeit-Übersicht über CPU, RAM, Festplatte, Netzwerk und Gesundheits-Score.

## Kern-Services

- `ISystemHealthService` / `SystemHealthService`
- `IEventLogService` — Systemfehler der letzten 24 h als Warnungen

## UI

- `DashboardViewModel` pollt Snapshot alle 2 s
- KPI-Karten, Sparklines, Warnungs-Panel

## Tests

- `SystemHealthServiceTests`, `SystemHealthServiceEventLogTests`, `ViewModelBindingTests`
