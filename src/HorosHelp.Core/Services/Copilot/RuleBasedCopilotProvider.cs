using HorosHelp.Core.Models.Copilot;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>Offline rule-based Copilot — always available, no network required.</summary>
public sealed class RuleBasedCopilotProvider : ILlmProvider
{
    public LlmProviderType ProviderType => LlmProviderType.Offline;

    public bool IsAvailable => true;

    public Task<CopilotResponse> CompleteAsync(
        string userMessage,
        CopilotSystemContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CopilotRuleEngine.Generate(userMessage, context));
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string userMessage,
        CopilotSystemContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = CopilotRuleEngine.Generate(userMessage, context);
        foreach (var chunk in ChunkText(response.Message, 12))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return chunk;
            await Task.Delay(18, cancellationToken);
        }
    }

    internal static IEnumerable<string> ChunkText(string text, int chunkSize)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        for (var i = 0; i < text.Length; i += chunkSize)
        {
            var len = Math.Min(chunkSize, text.Length - i);
            yield return text.Substring(i, len);
        }
    }
}
