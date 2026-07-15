using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.Network;
using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Network;
using HorosHelp.Core.Services.ProblemScan;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class CopilotDiagnosticWizardTests
{
    [Fact]
    public void ProcessMessage_StartsWizard_OnDiagnoseKeyword()
    {
        var wizard = new CopilotDiagnosticWizard();
        var step = wizard.ProcessMessage("Starte eine Diagnose");

        Assert.True(step.Handled);
        Assert.Contains("Beschreiben Sie", step.Message);
        Assert.True(wizard.IsActive);
    }

    [Fact]
    public void ProcessMessage_RunsTools_AfterConfirmation()
    {
        var wizard = new CopilotDiagnosticWizard();
        wizard.Start();
        wizard.ProcessMessage("PC ist langsam");
        wizard.ProcessMessage("Leistung");
        var step = wizard.ProcessMessage("Ja");

        Assert.True(step.Handled);
        Assert.Contains(CopilotToolId.RunProblemScan, step.ToolsToRun);
    }

    [Fact]
    public void ClassifyCategory_DetectsNetwork()
    {
        var category = CopilotDiagnosticWizard.ClassifyCategory("netzwerk und wlan");
        Assert.Equal(CopilotDiagnosticCategory.Network, category);
    }

    [Fact]
    public async Task CopilotToolExecutor_RunNetworkPing_UsesNetworkService()
    {
        var executor = new CopilotToolExecutor(
            new FakeScanner(),
            new FakeNetworkService(),
            new FakeScanMemory(),
            NullLogger<CopilotToolExecutor>.Instance);

        var result = await executor.ExecuteAsync(CopilotToolId.RunNetworkPing);

        Assert.True(result.Success);
        Assert.Contains("Ping", result.Summary);
    }

    private sealed class FakeScanner : IProblemScannerService
    {
        public Task<ScanResult> ScanAsync(IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ScanResult { Problems = [] });

        public Task<IReadOnlyList<ScanLogEntry>> RepairAsync(ProblemKind? kind = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ScanLogEntry>>([]);

        public IReadOnlyList<RollbackEntry> GetRecentRollbacks(int maxCount = 5) => [];

        public Task<IReadOnlyList<ScanLogEntry>> RollbackAsync(string rollbackId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ScanLogEntry>>([]);
    }

    private sealed class FakeNetworkService : INetworkService
    {
        public NetworkSnapshot GetSnapshot() => new();

        public Task<LatencyMeasurement> MeasureLatencyAsync(string host = "google.de", CancellationToken cancellationToken = default) =>
            Task.FromResult(new LatencyMeasurement());

        public Task<NetworkDiagnosticResult> PingAsync(string host = "google.de", CancellationToken cancellationToken = default) =>
            Task.FromResult(new NetworkDiagnosticResult { Success = true, Title = "Ping OK", Output = "8.8.8.8 erreichbar" });

        public Task<NetworkDiagnosticResult> TracertAsync(string host = "google.de", CancellationToken cancellationToken = default) =>
            Task.FromResult(new NetworkDiagnosticResult());

        public Task<NetworkDiagnosticResult> DnsLookupAsync(string host = "google.de", CancellationToken cancellationToken = default) =>
            Task.FromResult(new NetworkDiagnosticResult());

        public Task<NetworkDiagnosticResult> RenewIpAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new NetworkDiagnosticResult());
    }

    private sealed class FakeScanMemory : ICopilotScanMemory
    {
        public void RememberScanResult(ScanResult result) { }
    }
}
