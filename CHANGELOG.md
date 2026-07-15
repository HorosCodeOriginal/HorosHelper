# Changelog

Alle wesentlichen Änderungen an **HorosHelper** (HorosCode).

## [Unreleased] — 2026-07-15

### Added
- **Copilot Advanced:** `ILlmProvider` mit `RuleBasedCopilotProvider` (Offline-Default) und `HttpLlmProvider` (OpenAI-kompatibel / Ollama)
- API-Schlüssel-Speicherung via `DpapiSecretStore` — nie im Klartext in `settings.json`
- Antwort-Streaming (`IAsyncEnumerable`) in `CopilotViewModel`
- Diagnose-Modus mit Rückfragen und Tool-Calling (`CopilotDiagnosticWizard`, `CopilotToolExecutor`)
- Copilot-Einstellungen in Einstellungen-View (Provider, BaseUrl, Modell, API-Key)
- Lokalisierung: `Strings.resx` / `Strings.de-DE.resx` / `Strings.en.resx` für Shell-Nav + Copilot-Strings
- `de-DE`-Kultur beim App-Start; Englisch-Fallback für Nav-Labels
- Feature-Docs unter `docs/features/`
- Unit-Tests: Provider-Auswahl, HTTP-Streaming (Mock-Handler), Diagnose-Wizard, ViewModel-Smoke

### Changed
- Sidebar-Breite auf **199 px** (Mockup-01)
- Basiskomponenten-Styles: `horos-card`, `horos-primary`, `status-badge`, `section-header`
- `docs/architecture.md` um Copilot- und Lokalisierungs-Abschnitte erweitert

### Process (nicht automatisierbar)
- UI-Smoke-Tests, Integrationstests für Win32-Wrapper und Code-Review vor Merge bleiben manuelle Gates

## 2026-07-15 — Feature 9 Backup

- Inkrementelles Backup, AES-256, Task Scheduler, `SRSetRestorePoint` P/Invoke
- `InputSecurityValidator`, 184 Unit-Tests grün

## 2026-07-15 — Phase 2/3

- Netzwerk-Diagnose-Wizard, S.M.A.R.T., Sicherheit/Privacy, strukturiertes Logging

## 2026-07-15 — MVP

- Avalonia Shell, 11 Nav-Views, MVVM + DI, CI/CD
