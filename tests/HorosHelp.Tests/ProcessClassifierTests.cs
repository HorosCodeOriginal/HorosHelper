using HorosHelp.Core.Models.Processes;
using HorosHelp.Core.Services.Processes;

namespace HorosHelp.Tests;

public class ProcessClassifierTests
{
    [Theory]
    [InlineData("svchost.exe", ProcessSafetyLevel.System)]
    [InlineData("explorer.exe", ProcessSafetyLevel.System)]
    [InlineData("Spotify.exe", ProcessSafetyLevel.Safe)]
    [InlineData("CustomVendorApp.exe", ProcessSafetyLevel.Unknown)]
    public void Classify_ReturnsExpectedLevel(string processName, ProcessSafetyLevel expected)
    {
        Assert.Equal(expected, ProcessClassifier.Classify(processName));
    }

    [Theory]
    [InlineData(ProcessSafetyLevel.System, false)]
    [InlineData(ProcessSafetyLevel.Safe, true)]
    [InlineData(ProcessSafetyLevel.Unknown, true)]
    public void CanTerminate_RespectsSafetyLevel(ProcessSafetyLevel level, bool expected)
    {
        Assert.Equal(expected, ProcessClassifier.CanTerminate(level));
    }

    [Theory]
    [InlineData(ProcessSafetyLevel.System, "System")]
    [InlineData(ProcessSafetyLevel.Safe, "Sicher")]
    [InlineData(ProcessSafetyLevel.Unknown, "Unbekannt")]
    public void GetSafetyLabel_ReturnsGermanLabels(ProcessSafetyLevel level, string expected)
    {
        Assert.Equal(expected, ProcessClassifier.GetSafetyLabel(level));
    }
}
