using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Settings;
using HorosHelp.UI.ViewModels.Features;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class CopilotViewModelSmokeTests
{
    [Fact]
    public void CopilotViewModel_Constructs_WithWelcomeMessage()
    {
        using var vm = new CopilotViewModel(
            new FakeCopilotService(),
            new FakeHealthService(),
            new FakeNavigationService(),
            NullLogger<CopilotViewModel>.Instance);

        Assert.Equal("HorosHelper Copilot", vm.Title);
        Assert.NotEmpty(vm.Messages);
        Assert.False(vm.IsTyping);
    }

    [Fact]
    public void EinstellungenViewModel_Constructs_WithCopilotDefaults()
    {
        var vm = new EinstellungenViewModel(
            new FakeSettingsService(),
            new FakeSecretStore(),
            new FakeLogViewer(),
            NullLogger<EinstellungenViewModel>.Instance);

        Assert.Equal("Offline", vm.CopilotProvider);
        Assert.True(vm.ShowCopilotHttpFields == false);
    }

    private sealed class FakeCopilotService : ICopilotService
    {
        public CopilotSystemContext BuildContext() => new() { HealthScore = 80 };

        public CopilotResponse GenerateResponse(string userMessage, CopilotSystemContext? context = null) =>
            new() { Message = "Willkommen!" };

        public async IAsyncEnumerable<string> StreamResponseAsync(
            string userMessage,
            CopilotSystemContext? context = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return "Test";
            await Task.CompletedTask;
        }

        public CopilotDiagnosticState? GetDiagnosticState() => null;
        public void StartDiagnosticMode() { }
        public void CancelDiagnosticMode() { }

        public Task<CopilotResponse> ProcessMessageAsync(string userMessage, CancellationToken cancellationToken = default) =>
            Task.FromResult(new CopilotResponse { Message = "OK" });

        public void RememberScanResult(HorosHelp.Core.Models.ProblemScan.ScanResult result) { }
    }

    private sealed class FakeHealthService : ISystemHealthService
    {
        public SystemHealthSnapshot GetSnapshot() => new() { HealthScore = 80, CpuPercent = 10, RamPercent = 50 };
        public void Dispose() { }
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public string? CurrentRoute { get; private set; }
        public object? CurrentViewModel { get; private set; }
        public event EventHandler? Navigated;
        public void NavigateTo(string route)
        {
            CurrentRoute = route;
            Navigated?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public AppSettings Current { get; } = new();
        public AppSettings Load() => Current;
        public void Save(AppSettings settings) { }
    }

    private sealed class FakeSecretStore : HorosHelp.Core.Services.Security.ISecureSecretStore
    {
        public bool TryGetSecret(string key, out string? value) { value = null; return false; }
        public void SetSecret(string key, string? value) { }
    }

    private sealed class FakeLogViewer : HorosHelp.Core.Services.Logging.ILogViewerService
    {
        public string LogsDirectory => "C:\\temp";
        public IReadOnlyList<HorosHelp.Core.Services.Logging.LogFileInfo> GetRecentLogFiles(int maxCount = 20) => [];
        public string ReadTail(string fileName, int lineCount = 200) => "";
    }
}
