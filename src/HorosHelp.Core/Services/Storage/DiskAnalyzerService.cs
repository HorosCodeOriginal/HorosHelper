using HorosHelp.Core.Interop;
using HorosHelp.Core.Models.Storage;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Storage;

public interface IDiskAnalyzerService
{
    Task<DiskAnalysisResult> ScanAsync(
        string rootPath,
        IProgress<DiskAnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

public sealed class DiskAnalyzerService : IDiskAnalyzerService
{
    private readonly ILogger<DiskAnalyzerService> _logger;
    private readonly IFileSystemScanner _scanner;

    public DiskAnalyzerService(
        ILogger<DiskAnalyzerService> logger,
        IFileSystemScanner? scanner = null)
    {
        _logger = logger;
        _scanner = scanner ?? new PhysicalFileSystemScanner();
    }

    public async Task<DiskAnalysisResult> ScanAsync(
        string rootPath,
        IProgress<DiskAnalysisProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || !_scanner.DirectoryExists(rootPath))
        {
            _logger.LogWarning("Disk analysis root not found: {Path}", rootPath);
            return new DiskAnalysisResult();
        }

        try
        {
            return await Task.Run(() =>
            {
                progress?.Report(new DiskAnalysisProgress
                {
                    Percent = 0,
                    CurrentPath = rootPath,
                });

                var root = DiskTreeBuilder.BuildTree(rootPath, _scanner, maxDepth: 2, maxChildren: 8, cancellationToken);

                progress?.Report(new DiskAnalysisProgress
                {
                    Percent = 100,
                    CurrentPath = rootPath,
                    ScannedFolders = CountNodes(root),
                });

                return new DiskAnalysisResult
                {
                    Root = root,
                    WasCancelled = false,
                    TotalFoldersScanned = CountNodes(root),
                };
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return new DiskAnalysisResult { WasCancelled = true };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Disk analysis failed for {Path}", rootPath);
            return new DiskAnalysisResult();
        }
    }

    private static int CountNodes(FolderTreeNode node) =>
        1 + node.Children.Sum(CountNodes);
}
