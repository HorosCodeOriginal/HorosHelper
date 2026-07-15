using HorosHelp.Core.Models.Copilot;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>Pluggable LLM backend for HorosHelper Copilot (offline rules or HTTP endpoints).</summary>
public interface ILlmProvider
{
    LlmProviderType ProviderType { get; }

    bool IsAvailable { get; }

    Task<CopilotResponse> CompleteAsync(
        string userMessage,
        CopilotSystemContext context,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> StreamAsync(
        string userMessage,
        CopilotSystemContext context,
        CancellationToken cancellationToken = default);
}
