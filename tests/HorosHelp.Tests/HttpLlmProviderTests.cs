using System.Net;
using System.Text;
using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class HttpLlmProviderTests
{
    [Fact]
    public async Task CompleteAsync_ParsesOpenAiResponse()
    {
        var handler = new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"content":"Hallo vom Modell"}}]}""",
                    Encoding.UTF8,
                    "application/json"),
            });

        var provider = CreateProvider(handler, "OpenAiCompatible", "https://api.test");
        var response = await provider.CompleteAsync("Hi", SampleContext());

        Assert.Contains("Hallo vom Modell", response.Message);
    }

    [Fact]
    public async Task StreamAsync_YieldsOpenAiSseChunks()
    {
        var sse = "data: {\"choices\":[{\"delta\":{\"content\":\"Hi\"}}]}\n\ndata: [DONE]\n\n";
        var handler = new StubHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sse, Encoding.UTF8, "text/event-stream"),
            });

        var provider = CreateProvider(handler, "OpenAiCompatible", "https://api.test");
        var chunks = new List<string>();
        await foreach (var chunk in provider.StreamAsync("Hi", SampleContext()))
            chunks.Add(chunk);

        Assert.Contains("Hi", string.Concat(chunks));
    }

    [Fact]
    public async Task RuleBasedProvider_StreamsInChunks()
    {
        var provider = new RuleBasedCopilotProvider();
        var chunks = new List<string>();
        await foreach (var chunk in provider.StreamAsync("Hallo", SampleContext()))
            chunks.Add(chunk);

        Assert.True(chunks.Count > 1);
        Assert.Contains("HorosHelper", string.Concat(chunks), StringComparison.OrdinalIgnoreCase);
    }

    private static HttpLlmProvider CreateProvider(HttpMessageHandler handler, string provider, string baseUrl)
    {
        var settings = new FakeSettingsService(new AppSettings
        {
            Copilot = new CopilotSettings { Provider = provider, BaseUrl = baseUrl, Model = "test-model" },
        });

        return new HttpLlmProvider(
            new HttpClient(handler),
            settings,
            new FakeSecretStore(),
            NullLogger<HttpLlmProvider>.Instance);
    }

    private static CopilotSystemContext SampleContext() => new()
    {
        CpuPercent = 20,
        RamPercent = 40,
        HealthScore = 90,
    };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> factory) => _factory = factory;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_factory(request));
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public FakeSettingsService(AppSettings current) => Current = current;
        public AppSettings Current { get; private set; }
        public AppSettings Load() => Current;
        public void Save(AppSettings settings) => Current = settings;
    }

    private sealed class FakeSecretStore : HorosHelp.Core.Services.Security.ISecureSecretStore
    {
        public bool TryGetSecret(string key, out string? value) { value = null; return false; }
        public void SetSecret(string key, string? value) { }
    }
}
