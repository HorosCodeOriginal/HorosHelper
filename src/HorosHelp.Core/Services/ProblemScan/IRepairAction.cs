using HorosHelp.Core.Models.ProblemScan;

namespace HorosHelp.Core.Services.ProblemScan;

public interface IRepairAction
{
    ProblemKind Kind { get; }

    bool RequiresAdmin { get; }

    RepairCommandSpec BuildCommand();

    Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default);
}
