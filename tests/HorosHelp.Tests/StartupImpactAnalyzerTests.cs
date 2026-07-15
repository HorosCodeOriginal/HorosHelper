using HorosHelp.Core.Models.Startup;
using HorosHelp.Core.Services.Startup;

namespace HorosHelp.Tests;

public class StartupImpactAnalyzerTests
{
    [Theory]
    [InlineData("Spotify", @"C:\Program Files\Spotify\spotify.exe", StartupImpact.Hoch)]
    [InlineData("NVIDIA Display", @"C:\Windows\System32\nvidia.exe", StartupImpact.Niedrig)]
    [InlineData("CustomTool", @"C:\Apps\custom.exe", StartupImpact.Mittel)]
    public void ClassifyImpact_UsesHeuristics(string name, string command, StartupImpact expected)
    {
        var impact = StartupImpactAnalyzer.ClassifyImpact(name, command);
        Assert.Equal(expected, impact);
    }

    [Theory]
    [InlineData("Spotify", "spotify.exe", StartupImpact.Hoch, true)]
    [InlineData("NVIDIA", "nvidia.exe", StartupImpact.Niedrig, false)]
    [InlineData("Windows Security", "securityhealthsystray.exe", StartupImpact.Niedrig, false)]
    public void IsSafeToDisable_RespectsImpactAndKnownNames(
        string name,
        string command,
        StartupImpact impact,
        bool expected)
    {
        var safe = StartupImpactAnalyzer.IsSafeToDisable(name, command, impact);
        Assert.Equal(expected, safe);
    }

    [Fact]
    public void CountSafeToDisable_CountsOnlyEnabledSafeEntries()
    {
        var entries = new[]
        {
            new StartupEntryInfo
            {
                Id = "1", Name = "Spotify", Publisher = "x", Command = "x",
                IsEnabled = true, IsSafeToDisable = true,
            },
            new StartupEntryInfo
            {
                Id = "2", Name = "NVIDIA", Publisher = "x", Command = "x",
                IsEnabled = true, IsSafeToDisable = false,
            },
            new StartupEntryInfo
            {
                Id = "3", Name = "Discord", Publisher = "x", Command = "x",
                IsEnabled = false, IsSafeToDisable = true,
            },
        };

        Assert.Equal(1, StartupImpactAnalyzer.CountSafeToDisable(entries));
    }
}
