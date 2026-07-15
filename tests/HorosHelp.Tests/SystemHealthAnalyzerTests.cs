using HorosHelp.Core.Services.Health;

namespace HorosHelp.Tests;

public class SystemHealthAnalyzerTests
{
    private static readonly SystemHealthThresholds Thresholds = SystemHealthThresholds.Default;

    [Fact]
    public void CalculateHealthScore_Returns100_WhenAllMetricsHealthy()
    {
        var score = SystemHealthAnalyzer.CalculateHealthScore(20, 40, 50, true, Thresholds);

        Assert.Equal(100, score);
    }

    [Fact]
    public void CalculateHealthScore_AppliesPenalties_ForWarningThresholds()
    {
        var score = SystemHealthAnalyzer.CalculateHealthScore(85, 85, 90, true, Thresholds);

        Assert.Equal(65, score);
    }

    [Fact]
    public void CalculateHealthScore_AppliesCriticalPenalties_AndNetworkLoss()
    {
        var score = SystemHealthAnalyzer.CalculateHealthScore(95, 95, 98, false, Thresholds);

        Assert.Equal(5, score);
    }

    [Fact]
    public void BuildWarnings_IncludesDiskWarning_WhenAbove85Percent()
    {
        var warnings = SystemHealthAnalyzer.BuildWarnings(
            30, 40, 86, 800, 931, true, Thresholds);

        Assert.Contains(warnings, w => w.Title == "Wenig freier Speicherplatz");
        Assert.Contains(warnings, w => w.Subtitle.Contains("15%"));
    }

    [Fact]
    public void BuildWarnings_IsEmpty_WhenAllMetricsHealthy()
    {
        var warnings = SystemHealthAnalyzer.BuildWarnings(
            30, 50, 60, 400, 931, true, Thresholds);

        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData(90, "Ihr System ist in gutem Zustand.")]
    [InlineData(70, "Einige Bereiche benötigen Aufmerksamkeit.")]
    [InlineData(40, "Kritische Probleme erkannt — bitte Warnungen prüfen.")]
    public void GetHealthSubText_MapsScoreBands(int score, string expected)
    {
        Assert.Equal(expected, SystemHealthAnalyzer.GetHealthSubText(score));
    }
}
