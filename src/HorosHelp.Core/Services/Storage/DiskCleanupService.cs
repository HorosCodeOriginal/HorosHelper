using HorosHelp.Core.Models.Storage;
using HorosHelp.Core.Services.ProblemScan;
using HorosHelp.Core.Services.Windows;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Storage;

public interface IDiskCleanupService
{
    IReadOnlyList<BrowserCachePathInfo> GetBrowserCaches();

    long EstimateWindowsUpdateCacheSize();

    Task<StorageCleanupResult> CleanWindowsUpdateCacheAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default);

    Task<StorageCleanupResult> CleanBrowserCacheAsync(
        string browserId,
        CancellationToken cancellationToken = default);
}

public sealed class DiskCleanupService : IDiskCleanupService
{
    private readonly IWindowsServiceController _serviceController;
    private readonly ILogger<DiskCleanupService> _logger;

    public DiskCleanupService(
        IWindowsServiceController serviceController,
        ILogger<DiskCleanupService> logger)
    {
        _serviceController = serviceController;
        _logger = logger;
    }

    public IReadOnlyList<BrowserCachePathInfo> GetBrowserCaches() =>
        DiskCleanupPaths.DetectBrowserCaches();

    public long EstimateWindowsUpdateCacheSize() =>
        DirectoryCleanupHelper.EstimateDirectorySize(DiskCleanupPaths.WindowsUpdateCachePath, maxDepth: 1);

    public async Task<StorageCleanupResult> CleanWindowsUpdateCacheAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<string>();

        if (!isRunningAsAdmin)
        {
            messages.Add("Administratorrechte erforderlich für Windows-Update-Cache.");
            return new StorageCleanupResult { Messages = messages, Errors = 1 };
        }

        if (!OperatingSystem.IsWindows())
        {
            messages.Add("Nur unter Windows verfügbar.");
            return new StorageCleanupResult { Messages = messages, Errors = 1 };
        }

        try
        {
            var stop = await _serviceController.StopAsync("wuauserv", cancellationToken: cancellationToken);
            messages.Add(stop.Message);

            var path = DiskCleanupPaths.WindowsUpdateCachePath;
            var (deleted, errors) = await Task.Run(
                () => DirectoryCleanupHelper.ClearDirectoryContents(path, cancellationToken),
                cancellationToken);

            messages.Add($"Windows-Update-Cache: {deleted} Elemente entfernt" +
                         (errors > 0 ? $", {errors} übersprungen" : "."));

            var start = await _serviceController.StartAsync("wuauserv", cancellationToken: cancellationToken);
            messages.Add(start.Message);

            return new StorageCleanupResult
            {
                FilesDeleted = deleted,
                Errors = errors + (stop.Success && start.Success ? 0 : 1),
                Messages = messages,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Windows Update cache cleanup failed.");
            messages.Add("Windows-Update-Cache konnte nicht geleert werden.");
            return new StorageCleanupResult { Messages = messages, Errors = 1 };
        }
    }

    public async Task<StorageCleanupResult> CleanBrowserCacheAsync(
        string browserId,
        CancellationToken cancellationToken = default)
    {
        var cache = GetBrowserCaches()
            .FirstOrDefault(c => c.BrowserId.Equals(browserId, StringComparison.OrdinalIgnoreCase));

        if (cache is null || !cache.PathExists)
        {
            return new StorageCleanupResult
            {
                Messages = [$"Browser-Cache für „{browserId}“ nicht gefunden."],
                Errors = 1,
            };
        }

        try
        {
            var (deleted, errors) = await Task.Run(
                () => DirectoryCleanupHelper.ClearDirectoryContents(cache.CachePath, cancellationToken),
                cancellationToken);

            return new StorageCleanupResult
            {
                FilesDeleted = deleted,
                Errors = errors,
                Messages = [$"{cache.DisplayName}-Cache bereinigt: {deleted} Elemente entfernt."],
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Browser cache cleanup failed for {BrowserId}", browserId);
            return new StorageCleanupResult
            {
                Messages = [$"{cache.DisplayName}-Cache konnte nicht bereinigt werden."],
                Errors = 1,
            };
        }
    }
}
