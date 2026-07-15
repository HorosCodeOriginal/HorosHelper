using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.ProblemScan;

namespace HorosHelp.Tests;

public class RegistryPatternAnalyzerTests
{
    [Fact]
    public void AnalyzeRunValue_DetectsMissingExecutable()
    {
        var issues = RegistryPatternAnalyzer.AnalyzeRunValue(
            @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run",
            "BrokenApp",
            @"C:\Missing\app.exe");

        Assert.Single(issues);
        Assert.Equal(RegistryIssueKind.MissingExecutable, issues[0].Kind);
    }

    [Fact]
    public void AnalyzeRunValue_IgnoresExistingExecutable()
    {
        var exe = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];
        var issues = RegistryPatternAnalyzer.AnalyzeRunValue(
            @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run",
            "ValidApp",
            $"\"{exe}\"");

        Assert.Empty(issues);
    }

    [Fact]
    public void AnalyzeRunValue_DetectsSuspiciousRunOnceLocation()
    {
        var issues = RegistryPatternAnalyzer.AnalyzeRunValue(
            @"HKCU\Software\Microsoft\Windows\CurrentVersion\RunOnce",
            "Updater",
            "notepad.exe");

        Assert.Contains(issues, i => i.Kind == RegistryIssueKind.SuspiciousRunLocation);
    }

    [Fact]
    public void AnalyzeAppPath_DetectsMissingDirectory()
    {
        var issues = RegistryPatternAnalyzer.AnalyzeAppPath(
            @"HKLM\Software\Microsoft\Windows\CurrentVersion\App Paths\fake.exe",
            "Path",
            @"C:\DoesNotExist\folder");

        Assert.Single(issues);
        Assert.Equal(RegistryIssueKind.InvalidAppPath, issues[0].Kind);
    }

    [Theory]
    [InlineData(@"""C:\Program Files\App\app.exe"" /silent", @"C:\Program Files\App\app.exe")]
    [InlineData(@"C:\Windows\System32\notepad.exe", @"C:\Windows\System32\notepad.exe")]
    public void ExtractExecutablePath_ParsesQuotedAndPlainCommands(string command, string expected)
    {
        Assert.Equal(expected, RegistryPatternAnalyzer.ExtractExecutablePath(command));
    }
}

public class RegistryProblemCheckTests
{
    [Fact]
    public void BuildResult_MarksRepairableIssuesAsWarning()
    {
        var issues = new List<RegistryIssue>
        {
            new(
                RegistryIssueKind.MissingExecutable,
                @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run",
                "Broken",
                @"C:\missing.exe",
                "fehlende Datei"),
        };

        var result = RegistryProblemCheck.BuildResult(issues, usedMock: false);

        Assert.Equal(ProblemSeverity.Warning, result.Card.Severity);
        Assert.True(result.Card.IsRepairable);
        Assert.Single(result.Items);
    }
}
