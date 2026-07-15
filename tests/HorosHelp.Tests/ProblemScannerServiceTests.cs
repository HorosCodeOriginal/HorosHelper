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
        Assert.Equal(5, result.Problems.Count);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.Registry);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.TempFiles);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.StartupPrograms);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.DnsFlush);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.WinsockReset);
    }

    [Fact]
    public async Task RepairAsync_ReturnsLogEntries()
    {
        var service = CreateService();
        var entries = await service.RepairAsync(ProblemKind.TempFiles);

        Assert.NotEmpty(entries);
        Assert.Contains(entries, e => e.Message.Contains("Temp", StringComparison.OrdinalIgnoreCase));
    }

    private static ProblemScannerService CreateService()
    {
        IRepairAction[] repairs =
        [
            new DnsFlushRepair(NullLogger<DnsFlushRepair>.Instance),
            new WinsockResetRepair(NullLogger<WinsockResetRepair>.Instance),
        ];

        return new ProblemScannerService(
            NullLogger<ProblemScannerService>.Instance,
            new AdminElevationService(),
            repairs);
    }
}
