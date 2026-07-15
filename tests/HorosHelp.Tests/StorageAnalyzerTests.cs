using HorosHelp.Core.Services.Storage;

namespace HorosHelp.Tests;

public class StorageAnalyzerTests
{
    [Fact]
    public void BuildCategories_CalculatesPercentOfUsed()
    {
        var sizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["Apps & Programme"] = 100,
            ["Betriebssystem"] = 50,
        };

        var categories = StorageAnalyzer.BuildCategories(200, sizes);

        Assert.Equal(2, categories.Count);
        Assert.Equal(50.0, categories[0].PercentOfUsed);
        Assert.Equal(25.0, categories[1].PercentOfUsed);
    }

    [Fact]
    public void BuildCleanupCandidates_CalculatesSharePercent()
    {
        var candidates = new Dictionary<string, (string Name, long SizeBytes)>(StringComparer.OrdinalIgnoreCase)
        {
            ["temp"] = ("Temporäre Dateien", 60),
            ["recycle"] = ("Papierkorb", 40),
        };

        var result = StorageAnalyzer.BuildCleanupCandidates(candidates);

        Assert.Equal(60, result[0].SharePercent);
        Assert.Equal(40, result[1].SharePercent);
    }

    [Theory]
    [InlineData(4_200_000_000, 211_300_000_000, 2)]
    [InlineData(0, 100, 0)]
    public void CalculateCleanupChartPercent_Clamped(long reclaimable, long used, double expected)
    {
        var percent = StorageAnalyzer.CalculateCleanupChartPercent(reclaimable, used);
        Assert.Equal(expected, percent);
    }

    [Fact]
    public void FormatSizeDe_UsesGermanDecimalSeparatorForGb()
    {
        var text = StorageAnalyzer.FormatSizeDe(1_500_000_000);
        Assert.Contains(',', text);
        Assert.EndsWith("GB", text);
    }
}
