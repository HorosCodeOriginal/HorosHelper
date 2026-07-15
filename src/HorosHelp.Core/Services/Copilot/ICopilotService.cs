using HorosHelp.Core.Models.Copilot;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>HorosHelper Copilot — system context, LLM providers, streaming, and diagnostic wizard.</summary>
public interface ICopilotService : ICopilotScanMemory
{
    CopilotSystemContext BuildContext();

    CopilotResponse GenerateResponse(string userMessage, CopilotSystemContext? context = null);

    IAsyncEnumerable<string> StreamResponseAsync(
        string userMessage,
        CopilotSystemContext? context = null,
        CancellationToken cancellationToken = default);

    CopilotDiagnosticState? GetDiagnosticState();

    void StartDiagnosticMode();

    void CancelDiagnosticMode();

    Task<CopilotResponse> ProcessMessageAsync(
        string userMessage,
        CancellationToken cancellationToken = default);
}
