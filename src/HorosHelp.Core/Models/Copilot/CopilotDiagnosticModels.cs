namespace HorosHelp.Core.Models.Copilot;

public enum CopilotDiagnosticPhase
{
    Idle,
    AskSymptom,
    AskCategory,
    ConfirmScan,
    RunningTools,
    Summarize,
}

public enum CopilotDiagnosticCategory
{
    Unknown,
    Performance,
    Network,
    Storage,
    Security,
    General,
}

public sealed record CopilotDiagnosticState
{
    public CopilotDiagnosticPhase Phase { get; init; }
    public string? Symptom { get; init; }
    public CopilotDiagnosticCategory Category { get; init; }
    public IReadOnlyList<string> ToolResults { get; init; } = [];
    public string? PendingQuestion { get; init; }
}

public enum CopilotToolId
{
    RunProblemScan,
    RunNetworkPing,
}

public sealed class CopilotToolResult
{
    public CopilotToolId ToolId { get; init; }
    public bool Success { get; init; }
    public string Summary { get; init; } = "";
}
