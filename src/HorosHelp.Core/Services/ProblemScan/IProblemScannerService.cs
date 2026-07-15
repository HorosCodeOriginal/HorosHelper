using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Admin;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.ProblemScan;

public interface IProblemScannerService
{
    Task<ScanResult> ScanAsync(
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScanLogEntry>> RepairAsync(
        ProblemKind? kind = null,
        CancellationToken cancellationToken = default);

    IReadOnlyList<RollbackEntry> GetRecentRollbacks(int maxCount = 5);

    Task<IReadOnlyList<ScanLogEntry>> RollbackAsync(
        string rollbackId,
        CancellationToken cancellationToken = default);
}
