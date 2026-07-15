using HorosHelp.Core.Models.ProblemScan;

namespace HorosHelp.Core.Services.ProblemScan;

public sealed class ScanProgress
{
    public double Percent { get; init; }
    public string StatusText { get; init; } = "";
    public string SubText { get; init; } = "";
    public ScanLogEntry? LogEntry { get; init; }
}
