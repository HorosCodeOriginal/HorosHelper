namespace HorosHelp.Core.Models.Copilot;

public enum CopilotActionId
{
    None,
    NavigateStartup,
    NavigateStorage,
    NavigateDashboard,
    NavigateProblemFixer,
    NavigateSecurity,
}

public sealed class CopilotActionSuggestion
{
    public CopilotActionId ActionId { get; init; }
    public string IconGlyph { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
}

public sealed class CopilotResponse
{
    public string Message { get; init; } = "";
    public IReadOnlyList<CopilotActionSuggestion> Actions { get; init; } = [];
}

public sealed class CopilotSystemContext
{
    public double CpuPercent { get; init; }
    public double RamPercent { get; init; }
    public double RamUsedGb { get; init; }
    public double RamTotalGb { get; init; }
    public int HealthScore { get; init; }
    public int StartupEntryCount { get; init; }
    public int SafeToDisableStartupCount { get; init; }
    public double ReclaimableStorageGb { get; init; }
    public int OpenProblemCount { get; init; }
    public int SecurityScore { get; init; }
    public int ActiveProcessCount { get; init; }
    public DateTimeOffset? LastScanUtc { get; init; }
}
