namespace HorosHelp.Core.Models.ProblemScan;

public sealed class ProblemItem
{
    public required string Id { get; init; }
    public ProblemKind Kind { get; init; }
    public ProblemSeverity Severity { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public bool IsRepairable { get; init; }
    public string? RepairHint { get; init; }
}
