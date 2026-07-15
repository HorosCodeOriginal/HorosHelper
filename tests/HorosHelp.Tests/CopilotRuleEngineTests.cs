using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Services.Copilot;

namespace HorosHelp.Tests;

public class CopilotRuleEngineTests
{
    private static CopilotSystemContext SampleContext() => new()
    {
        CpuPercent = 34,
        RamPercent = 62,
        RamUsedGb = 9.9,
        RamTotalGb = 15.9,
        HealthScore = 85,
        StartupEntryCount = 14,
        SafeToDisableStartupCount = 3,
        ReclaimableStorageGb = 2.4,
        OpenProblemCount = 0,
        SecurityScore = 92,
        ActiveProcessCount = 128,
    };

    [Fact]
    public void Generate_ReturnsSlowPcResponse_ForPerformanceQuestion()
    {
        var response = CopilotRuleEngine.Generate("Warum ist mein PC langsam?", SampleContext());

        Assert.Contains("Autostart", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(response.Actions, a => a.ActionId == CopilotActionId.NavigateStartup);
    }

    [Fact]
    public void Generate_ReturnsStorageResponse_ForCleanupQuestion()
    {
        var response = CopilotRuleEngine.Generate("Speicher bereinigen", SampleContext());

        Assert.Contains("GB", response.Message);
        Assert.Contains(response.Actions, a => a.ActionId == CopilotActionId.NavigateStorage);
    }

    [Fact]
    public void Generate_ReturnsSecurityResponse_ForDefenderQuestion()
    {
        var response = CopilotRuleEngine.Generate("Wie ist mein Defender Status?", SampleContext());

        Assert.Contains("92", response.Message);
        Assert.Contains(response.Actions, a => a.ActionId == CopilotActionId.NavigateSecurity);
    }

    [Fact]
    public void BuildDefaultActions_IncludesStartupStorageAndRam()
    {
        var actions = CopilotRuleEngine.BuildDefaultActions(SampleContext());

        Assert.Contains(actions, a => a.ActionId == CopilotActionId.NavigateStartup);
        Assert.Contains(actions, a => a.ActionId == CopilotActionId.NavigateStorage);
        Assert.Contains(actions, a => a.ActionId == CopilotActionId.NavigateDashboard);
    }

    [Fact]
    public void FormatRelativeBackup_FormatsRecentBackup()
    {
        var text = CopilotRuleEngine.FormatRelativeBackup(DateTimeOffset.UtcNow.AddHours(-2));
        Assert.Contains("Stunden", text);
    }
}
