using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>Selects the active LLM provider from app settings; offline is always the safe default.</summary>
public interface ILlmProviderFactory
{
    ILlmProvider GetActiveProvider();

    ILlmProvider GetOfflineProvider();
}

public sealed class LlmProviderFactory : ILlmProviderFactory
{
    private readonly ISettingsService _settingsService;
    private readonly RuleBasedCopilotProvider _offlineProvider;
    private readonly HttpLlmProvider _httpProvider;
    private readonly ILogger<LlmProviderFactory> _logger;

    public LlmProviderFactory(
        ISettingsService settingsService,
        RuleBasedCopilotProvider offlineProvider,
        HttpLlmProvider httpProvider,
        ILogger<LlmProviderFactory> logger)
    {
        _settingsService = settingsService;
        _offlineProvider = offlineProvider;
        _httpProvider = httpProvider;
        _logger = logger;
    }

    public ILlmProvider GetOfflineProvider() => _offlineProvider;

    public ILlmProvider GetActiveProvider()
    {
        var providerName = _settingsService.Current.Copilot.Provider;
        if (providerName is "OpenAiCompatible" or "Ollama")
        {
            if (_httpProvider.IsAvailable)
                return _httpProvider;

            _logger.LogWarning(
                "Copilot provider {Provider} not configured; falling back to offline rules.",
                providerName);
        }

        return _offlineProvider;
    }
}
