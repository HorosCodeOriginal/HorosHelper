using HorosHelp.Core.Models.ProblemScan;

namespace HorosHelp.Core.Services.ProblemScan;

public interface IProblemScannerService
{
    Task<ScanResult> ScanAsync(
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScanLogEntry>> RepairAsync(
        ProblemKind? kind = null,
        CancellationToken cancellationToken = default);
}
