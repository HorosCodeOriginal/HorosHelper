using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Security;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>OpenAI-compatible chat completions via HttpClient (OpenAI, Azure, Ollama).</summary>
public sealed class HttpLlmProvider : ILlmProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly ISecureSecretStore _secretStore;
    private readonly ILogger<HttpLlmProvider> _logger;
    private readonly LlmProviderType _providerType;

    public HttpLlmProvider(
        HttpClient httpClient,
        ISettingsService settingsService,
        ISecureSecretStore secretStore,
        ILogger<HttpLlmProvider> logger)
        : this(httpClient, settingsService, secretStore, logger, LlmProviderType.OpenAiCompatible)
    {
    }

    public HttpLlmProvider(
        HttpClient httpClient,
        ISettingsService settingsService,
        ISecureSecretStore secretStore,
        ILogger<HttpLlmProvider> logger,
        LlmProviderType providerType)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _secretStore = secretStore;
        _logger = logger;
        _providerType = providerType;
    }

    public LlmProviderType ProviderType =>
        _settingsService.Current.Copilot.Provider switch
        {
            "Ollama" => LlmProviderType.Ollama,
            "OpenAiCompatible" => LlmProviderType.OpenAiCompatible,
            _ => _providerType,
        };

    public bool IsAvailable
    {
        get
        {
            var settings = _settingsService.Current.Copilot;
            return !string.IsNullOrWhiteSpace(ResolveBaseUrl(settings));
        }
    }

    public async Task<CopilotResponse> CompleteAsync(
        string userMessage,
        CopilotSystemContext context,
        CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.Current.Copilot;
        var request = BuildRequest(userMessage, context, settings, stream: false);
        var endpoint = BuildChatEndpoint(settings);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        ApplyAuth(httpRequest, settings);
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (string.Equals(settings.Provider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            var ollama = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken);
            var ollamaContent = ollama?.Message?.Content ?? "";
            return new CopilotResponse
            {
                Message = string.IsNullOrWhiteSpace(ollamaContent)
                    ? "Keine Antwort vom Modell erhalten."
                    : ollamaContent.Trim(),
                Actions = CopilotRuleEngine.BuildDefaultActions(context),
            };
        }

        var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Leere LLM-Antwort.");

        var content = payload.Choices?.FirstOrDefault()?.Message?.Content ?? "";
        return new CopilotResponse
        {
            Message = string.IsNullOrWhiteSpace(content)
                ? "Keine Antwort vom Modell erhalten."
                : content.Trim(),
            Actions = CopilotRuleEngine.BuildDefaultActions(context),
        };
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string userMessage,
        CopilotSystemContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.Current.Copilot;
        var request = BuildRequest(userMessage, context, settings, stream: true);
        var endpoint = BuildChatEndpoint(settings);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        ApplyAuth(httpRequest, settings);
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var isOllama = string.Equals(settings.Provider, "Ollama", StringComparison.OrdinalIgnoreCase);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!isOllama && line.StartsWith("data:", StringComparison.Ordinal))
            {
                var data = line["data:".Length..].Trim();
                if (data == "[DONE]")
                    break;

                string? delta;
                try
                {
                    var chunk = JsonSerializer.Deserialize<ChatCompletionResponse>(data, JsonOptions);
                    delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                }
                catch (JsonException ex)
                {
                    _logger.LogDebug(ex, "Skipping unparseable SSE chunk.");
                    continue;
                }

                if (!string.IsNullOrEmpty(delta))
                    yield return delta;
                continue;
            }

            if (isOllama || line.StartsWith('{'))
            {
                string? delta;
                try
                {
                    var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, JsonOptions);
                    delta = chunk?.Message?.Content;
                    if (chunk?.Done == true)
                        break;
                }
                catch (JsonException ex)
                {
                    _logger.LogDebug(ex, "Skipping unparseable Ollama chunk.");
                    continue;
                }

                if (!string.IsNullOrEmpty(delta))
                    yield return delta;
            }
        }
    }

    private void ApplyAuth(HttpRequestMessage request, CopilotSettings settings)
    {
        if (string.Equals(settings.Provider, "Ollama", StringComparison.OrdinalIgnoreCase))
            return;

        if (_secretStore.TryGetSecret(DpapiSecretStore.CopilotApiKeySecretName, out var apiKey)
            && !string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    private static ChatCompletionRequest BuildRequest(
        string userMessage,
        CopilotSystemContext context,
        CopilotSettings settings,
        bool stream)
    {
        var systemPrompt = new StringBuilder();
        systemPrompt.AppendLine("Du bist HorosHelper Copilot — ein deutscher Windows-System-Assistent.");
        systemPrompt.AppendLine($"CPU: {context.CpuPercent:F0}%, RAM: {context.RamPercent:F0}%, Gesundheit: {context.HealthScore}.");
        systemPrompt.AppendLine("Antworte kurz und hilfreich auf Deutsch.");

        return new ChatCompletionRequest
        {
            Model = ResolveModel(settings),
            Stream = stream,
            Messages =
            [
                new ChatMessageDto { Role = "system", Content = systemPrompt.ToString() },
                new ChatMessageDto { Role = "user", Content = userMessage },
            ],
        };
    }

    private static string ResolveBaseUrl(CopilotSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
            return settings.BaseUrl.TrimEnd('/');

        return settings.Provider switch
        {
            "Ollama" => "http://localhost:11434",
            "OpenAiCompatible" => "",
            _ => "",
        };
    }

    private static string ResolveModel(CopilotSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Model))
            return settings.Model;

        return settings.Provider switch
        {
            "Ollama" => "llama3.2",
            _ => "gpt-4o-mini",
        };
    }

    private string BuildChatEndpoint(CopilotSettings settings)
    {
        var baseUrl = ResolveBaseUrl(settings);
        return _providerType == LlmProviderType.Ollama
            || _settingsService.Current.Copilot.Provider == "Ollama"
            ? $"{baseUrl}/api/chat"
            : $"{baseUrl}/v1/chat/completions";
    }

    private sealed class ChatCompletionRequest
    {
        public string Model { get; set; } = "";
        public bool Stream { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = [];
    }

    private sealed class ChatMessageDto
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }

    private sealed class ChatCompletionResponse
    {
        public List<ChatChoiceDto>? Choices { get; set; }
    }

    private sealed class ChatChoiceDto
    {
        public ChatMessageDto? Message { get; set; }
        public ChatDeltaDto? Delta { get; set; }
    }

    private sealed class ChatDeltaDto
    {
        public string? Content { get; set; }
    }

    private sealed class OllamaChatResponse
    {
        public ChatMessageDto? Message { get; set; }
        public bool Done { get; set; }
    }
}
