namespace HorosHelp.Core.Models.Processes;

public enum ProcessSafetyLevel
{
    System,
    Unknown,
    Safe,
}

public sealed class ManagedProcessInfo
{
    public required string Name { get; init; }
    public int ProcessId { get; init; }
    public double CpuPercent { get; init; }
    public long WorkingSetBytes { get; init; }
    public ProcessSafetyLevel SafetyLevel { get; init; }
    public required string SafetyLabel { get; init; }
}

public sealed class ProcessTerminateResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}
