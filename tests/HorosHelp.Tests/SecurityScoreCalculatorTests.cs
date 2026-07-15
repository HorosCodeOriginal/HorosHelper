using HorosHelp.Core.Services.Apps;
using HorosHelp.Core.Services.Security;

namespace HorosHelp.Tests;

public class SecurityScoreCalculatorTests
{
    [Fact]
    public void Calculate_Returns100_WhenAllComponentsHealthy()
    {
        var score = SecurityScoreCalculator.Calculate(new SecurityScoreInput());

        Assert.Equal(100, score);
    }

    [Fact]
    public void Calculate_Returns95_WhenOnlyRecentScanMissing()
    {
        var score = SecurityScoreCalculator.Calculate(new SecurityScoreInput
        {
            RecentScan = false,
        });

        Assert.Equal(95, score);
    }

    [Fact]
    public void Calculate_Returns92_WhenUpdatesPending()
    {
        var score = SecurityScoreCalculator.Calculate(new SecurityScoreInput
        {
            SecurityUpdatesCurrent = false,
        });

        Assert.Equal(92, score);
    }

    [Fact]
    public void Calculate_AppliesFirewallAndDefenderPenalties()
    {
        var score = SecurityScoreCalculator.Calculate(new SecurityScoreInput
        {
            FirewallEnabled = false,
            DefenderActive = false,
        });

        Assert.Equal(45, score);
    }

    [Fact]
    public void Calculate_AppliesRealTimeProtectionPenalty_WhenDefenderActive()
    {
        var score = SecurityScoreCalculator.Calculate(new SecurityScoreInput
        {
            DefenderActive = true,
            RealTimeProtectionEnabled = false,
        });

        Assert.Equal(85, score);
    }

    [Theory]
    [InlineData(92, "Ausgezeichnet")]
    [InlineData(80, "Gut")]
    [InlineData(65, "Ausreichend")]
    [InlineData(40, "Verbesserung nötig")]
    public void GetScoreStatusLabel_MapsBands(int score, string expected)
    {
        Assert.Equal(expected, SecurityScoreCalculator.GetScoreStatusLabel(score));
    }

    [Fact]
    public void HasRecentScan_ReturnsTrue_ForScanWithinSevenDays()
    {
        var recent = DateTimeOffset.UtcNow.AddDays(-2);
        Assert.True(SecurityScoreCalculator.HasRecentScan(recent));
    }

    [Fact]
    public void HasRecentScan_ReturnsFalse_ForOldScan()
    {
        var old = DateTimeOffset.UtcNow.AddDays(-10);
        Assert.False(SecurityScoreCalculator.HasRecentScan(old));
    }
}
