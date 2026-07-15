# Copilot (KI-System-Assistent)

**Sidebar:** Copilot · **Mockup:** `assets/images/dashboard-mockup-05/`

## Zweck

Deutschsprachiger Assistent für Diagnosen, Erklärungen und Navigation — **standardmäßig offline**.

## Provider-Architektur

| Provider | Klasse | Netzwerk |
|----------|--------|----------|
| Offline (Default) | `RuleBasedCopilotProvider` | Nein |
| OpenAI-kompatibel | `HttpLlmProvider` | Ja, konfigurierbare BaseUrl |
| Ollama | `HttpLlmProvider` | Ja, Default `http://localhost:11434` |

- `ILlmProviderFactory` wählt Provider aus `AppSettings.Copilot`
- API-Key: `DpapiSecretStore` (`copilot-api-key.dpapi`)

## Features

- **Streaming:** `ICopilotService.StreamResponseAsync` → `CopilotViewModel`
- **Diagnose-Modus:** `CopilotDiagnosticWizard` + `CopilotToolExecutor` (Problem-Scan, Netzwerk-Ping)
- Systemkontext aus Health, Startup, Storage, Security

## Einstellungen

Einstellungen → Kategorie **Copilot**: Provider, URL, Modell, API-Key

## Tests

- `CopilotRuleEngineTests`, `LlmProviderFactoryTests`, `HttpLlmProviderTests`, `CopilotDiagnosticWizardTests`
