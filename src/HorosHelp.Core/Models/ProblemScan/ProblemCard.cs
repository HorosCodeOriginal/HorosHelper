namespace HorosHelp.Core.Models.ProblemScan;

public sealed class ProblemCard
{
    public ProblemKind Kind { get; init; }
    public ProblemSeverity Severity { get; init; }
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public double ProgressValue { get; init; }
    public bool IsRepairable { get; init; }
}
