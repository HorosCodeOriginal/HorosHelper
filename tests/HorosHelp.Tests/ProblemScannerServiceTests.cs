using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.ProblemScan;

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
    public async Task ScanAsync_ReturnsThreeProblemCategories()
    {
        var service = CreateService();
        var result = await service.ScanAsync();

        Assert.True(result.IsComplete);
        Assert.Equal(3, result.Problems.Count);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.Registry);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.TempFiles);
        Assert.Contains(result.Problems, p => p.Kind == ProblemKind.StartupPrograms);
    }

    [Fact]
    public async Task RepairAsync_ReturnsLogEntries()
    {
        var service = CreateService();
        var entries = await service.RepairAsync(ProblemKind.TempFiles);

        Assert.NotEmpty(entries);
        Assert.Contains(entries, e => e.Message.Contains("Temp", StringComparison.OrdinalIgnoreCase));
    }

    private static ProblemScannerService CreateService() =>
        new(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProblemScannerService>.Instance);
}
