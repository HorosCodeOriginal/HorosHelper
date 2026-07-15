using HorosHelp.Core.Models.ProblemScan;

namespace HorosHelp.Core.Services.ProblemScan;

public interface IProblemCheck
{
    ProblemKind Kind { get; }

    ProblemCheckResult Check();
}

public sealed class ProblemCheckResult
{
    public ProblemCard Card { get; init; } = new();
    public IReadOnlyList<ProblemItem> Items { get; init; } = [];
    public bool UsedMockData { get; init; }
}
