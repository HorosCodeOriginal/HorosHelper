using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.ProblemScan;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class ProblemScannerThresholdsTests
{
    [Fact]
    public void DefaultThresholds_MatchMockupExpectations()
    {
        var t = ProblemScannerThresholds.Default;

        Assert.Equal(1.0, t.TempWarningGb);
        Assert.Equal(2.0, t.TempCriticalGb);
        Assert.Equal(8, t.StartupWarningCount);
        Assert.Equal(12, t.StartupCriticalCount);
    }
}

public class ProblemScannerServiceTests
{
    [Fact]
    public async Task ScanAsync_ReturnsProblemCategories_IncludingOptionalRepairs()
    {
        var service = CreateService();
        var result = await service.ScanAsync();

        Assert.True(result.IsComplete);
        Assert.Equal(8, result.Problems.Count);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.Registry);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.TempFiles);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.StartupPrograms);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.DnsFlush);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.WinsockReset);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.WindowsUpdateCache);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.SystemFileCheck);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.SearchIndexReset);
    }

    [Fact]
    public async Task ScanAsync_SystemFileCheckCard_WarnsAboutDuration()
    {
        var service = CreateService();
        var result = await service.ScanAsync();
        var sfc = result.Problems.First(p => p.Kind == ProblemKind.SystemFileCheck);

        Assert.Contains("15", sfc.Subtitle, StringComparison.Ordinal);
        Assert.Contains("45", sfc.Subtitle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RepairAsync_ReturnsLogEntries()
    {
        var service = CreateService();
        var entries = await service.RepairAsync(ProblemKind.TempFiles);

        Assert.NotEmpty(entries);
        Assert.Contains(entries, e => e.Message.Contains("Temp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetRecentRollbacks_ReturnsEntriesFromStore()
    {
        var service = CreateService();
        var rollbacks = service.GetRecentRollbacks();
        Assert.NotNull(rollbacks);
    }

    private static ProblemScannerService CreateService()
    {
        var rollbackStore = new RollbackStore(NullLogger<RollbackStore>.Instance,
            Path.Combine(Path.GetTempPath(), "HorosHelper-ScannerTests", Guid.NewGuid().ToString("N")));

        IRepairAction[] repairs =
        [
            new RegistryRepairAction(NullLogger<RegistryRepairAction>.Instance, rollbackStore),
            new DnsFlushRepair(NullLogger<DnsFlushRepair>.Instance),
            new WinsockResetRepair(NullLogger<WinsockResetRepair>.Instance),
            new WindowsUpdateCacheRepair(NullLogger<WindowsUpdateCacheRepair>.Instance),
            new SfcDismRepair(NullLogger<SfcDismRepair>.Instance),
            new SearchIndexResetRepair(NullLogger<SearchIndexResetRepair>.Instance, rollbackStore),
        ];

        IProblemCheck[] checks =
        [
            new RegistryProblemCheck(NullLogger<RegistryProblemCheck>.Instance),
        ];

        var registryRepair = new RegistryRepairAction(NullLogger<RegistryRepairAction>.Instance, rollbackStore);

        return new ProblemScannerService(
            NullLogger<ProblemScannerService>.Instance,
            new AdminElevationService(),
            repairs,
            checks,
            rollbackStore,
            registryRepair);
    }
}
