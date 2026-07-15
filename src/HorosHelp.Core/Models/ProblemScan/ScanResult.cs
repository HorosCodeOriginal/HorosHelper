namespace HorosHelp.Core.Models.ProblemScan;

public sealed class ScanResult
{
    public IReadOnlyList<ProblemCard> Problems { get; init; } = [];
    public IReadOnlyList<ScanLogEntry> LogEntries { get; init; } = [];
    public bool IsComplete { get; init; }
    public bool UsedMockData { get; init; }
}
