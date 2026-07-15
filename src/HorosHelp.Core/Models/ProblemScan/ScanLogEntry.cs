namespace HorosHelp.Core.Models.ProblemScan;

public enum ScanLogStatus
{
    Info,
    Success,
    Warning,
    Error,
    InProgress,
}

public sealed class ScanLogEntry
{
    public DateTime Timestamp { get; init; }
    public string Message { get; init; } = "";
    public ScanLogStatus Status { get; init; }
}
