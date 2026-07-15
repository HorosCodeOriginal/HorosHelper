using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class LlmProviderFactoryTests
{
    [Fact]
    public void GetActiveProvider_ReturnsOffline_ByDefault()
    {
        var settings = new FakeSettingsService(new AppSettings());
        var factory = CreateFactory(settings);

        var provider = factory.GetActiveProvider();

        Assert.Equal(LlmProviderType.Offline, provider.ProviderType);
    }

    [Fact]
    public void GetActiveProvider_ReturnsHttp_WhenOpenAiConfigured()
    {
        var settings = new FakeSettingsService(new AppSettings
        {
            Copilot = new CopilotSettings
            {
                Provider = "OpenAiCompatible",
                BaseUrl = "https://api.example.com",
            },
        });
        var factory = CreateFactory(settings);

        var provider = factory.GetActiveProvider();

        Assert.Equal(LlmProviderType.OpenAiCompatible, provider.ProviderType);
    }

    [Fact]
    public void GetActiveProvider_FallsBackToOffline_WhenHttpNotConfigured()
    {
        var settings = new FakeSettingsService(new AppSettings
        {
            Copilot = new CopilotSettings { Provider = "OpenAiCompatible", BaseUrl = "" },
        });
        var factory = CreateFactory(settings);

        var provider = factory.GetActiveProvider();

        Assert.Equal(LlmProviderType.Offline, provider.ProviderType);
    }

    private static LlmProviderFactory CreateFactory(FakeSettingsService settings)
    {
        var offline = new RuleBasedCopilotProvider();
        var http = new HttpLlmProvider(
            new HttpClient(),
            settings,
            new FakeSecretStore(),
            NullLogger<HttpLlmProvider>.Instance);
        return new LlmProviderFactory(settings, offline, http, NullLogger<LlmProviderFactory>.Instance);
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
        public bool TryGetSecret(string key, out string? value)
        {
            value = null;
            return false;
        }

        public void SetSecret(string key, string? value) { }
    }
}
