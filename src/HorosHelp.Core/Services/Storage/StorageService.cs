using HorosHelp.Core.Models.Storage;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Storage;

public sealed class StorageService : IStorageService
{
    private readonly ILogger<StorageService> _logger;

    public StorageService(ILogger<StorageService> logger)
    {
        _logger = logger;
    }

    public StorageSnapshot GetSnapshot()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("Storage APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var drives = ReadDrives();
            if (drives.Count == 0)
            {
                _logger.LogWarning("No ready drives found; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var primary = drives[0];
            var categorySizes = EstimateCategorySizes(primary.Letter);
            var categories = StorageAnalyzer.BuildCategories(primary.UsedBytes, categorySizes);

            var cleanupSizes = EstimateCleanupCandidates();
            var cleanupCandidates = StorageAnalyzer.BuildCleanupCandidates(cleanupSizes);
            var reclaimable = cleanupCandidates.Sum(c => c.SizeBytes);
            var chartPercent = StorageAnalyzer.CalculateCleanupChartPercent(reclaimable, primary.UsedBytes);

            return new StorageSnapshot
            {
                Drives = drives,
                Categories = categories,
                CleanupCandidates = cleanupCandidates,
                TotalReclaimableBytes = reclaimable,
                CleanupChartPercent = chartPercent,
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Storage snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public async Task<StorageCleanupResult> RunSafeCleanupAsync(CancellationToken cancellationToken = default)
    {
        var messages = new List<string>();
        var filesDeleted = 0;
        var errors = 0;
        long bytesFreed = 0;

        try
        {
            var tempPaths = new[]
            {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
            };

            foreach (var tempPath in tempPaths.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Directory.Exists(tempPath))
                    continue;

                foreach (var file in Directory.EnumerateFiles(tempPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var info = new FileInfo(file);
                        if (info.LastWriteTimeUtc >= DateTime.UtcNow.AddDays(-7))
                            continue;

                        var length = info.Length;
                        info.Delete();
                        filesDeleted++;
                        bytesFreed += length;
                    }
                    catch
                    {
                        errors++;
                    }

                    if (filesDeleted % 50 == 0 && filesDeleted > 0)
                        await Task.Delay(10, cancellationToken);
                }
            }

            messages.Add(
                $"Temp-Bereinigung: {filesDeleted} Dateien entfernt" +
                (errors > 0 ? $", {errors} übersprungen" : ""));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Safe storage cleanup failed.");
            messages.Add("Bereinigung teilweise fehlgeschlagen.");
        }

        return new StorageCleanupResult
        {
            BytesFreed = bytesFreed,
            FilesDeleted = filesDeleted,
            Errors = errors,
            Messages = messages,
        };
    }

    private static IReadOnlyList<DriveStorageInfo> ReadDrives()
    {
        var drives = new List<DriveStorageInfo>();

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            var percent = drive.TotalSize > 0
                ? used / (double)drive.TotalSize * 100
                : 0;

            drives.Add(new DriveStorageInfo
            {
                Letter = drive.Name.TrimEnd('\\'),
                Label = string.IsNullOrWhiteSpace(drive.VolumeLabel)
                    ? $"Lokaler Datenträger ({drive.Name.TrimEnd('\\')})"
                    : $"{drive.VolumeLabel} ({drive.Name.TrimEnd('\\')})",
                DriveType = MapDriveType(drive.DriveType),
                FileSystem = drive.DriveFormat,
                TotalBytes = drive.TotalSize,
                UsedBytes = used,
                FreeBytes = drive.AvailableFreeSpace,
                PercentUsed = Math.Round(percent, 0, MidpointRounding.AwayFromZero),
                IsReady = true,
            });
        }

        return drives
            .OrderByDescending(d => string.Equals(d.Letter, "C:", StringComparison.OrdinalIgnoreCase))
            .ThenBy(d => d.Letter, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string MapDriveType(DriveType driveType) =>
        driveType switch
        {
            DriveType.Fixed => "SSD/HDD",
            DriveType.Removable => "Wechselmedium",
            DriveType.Network => "Netzwerk",
            DriveType.CDRom => "CD/DVD",
            _ => "Laufwerk",
        };

    private Dictionary<string, long> EstimateCategorySizes(string driveLetter)
    {
        var root = driveLetter.EndsWith('\\') ? driveLetter : driveLetter + "\\";
        var sizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["Apps & Programme"] = SumDirectorySizes(
                Path.Combine(root, "Program Files"),
                Path.Combine(root, "Program Files (x86)")),
            ["Betriebssystem"] = SumDirectorySize(Path.Combine(root, "Windows")),
            ["Benutzerdateien"] = SumDirectorySize(Path.Combine(root, "Users")),
            ["Spiele"] = SumDirectorySizes(
                Path.Combine(root, "Program Files (x86)", "Steam"),
                Path.Combine(root, "XboxGames")),
            ["Temporäre Dateien"] = SumDirectorySizes(
                Path.GetTempPath(),
                Path.Combine(root, "Windows", "Temp")),
            ["Papierkorb"] = EstimateRecycleBinSize(root),
            ["Systemreserviert"] = SumDirectorySize(Path.Combine(root, "System Volume Information")),
        };

        var known = sizes.Values.Sum();
        var primary = DriveInfo.GetDrives()
            .FirstOrDefault(d => d.IsReady && string.Equals(d.Name.TrimEnd('\\'), driveLetter.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase));

        if (primary is not null)
        {
            var used = primary.TotalSize - primary.AvailableFreeSpace;
            var remainder = Math.Max(0, used - known);
            sizes["Sonstiges"] = remainder;
        }
        else
        {
            sizes["Sonstiges"] = 0;
        }

        return sizes;
    }

    private Dictionary<string, (string Name, long SizeBytes)> EstimateCleanupCandidates()
    {
        var tempBytes = SumDirectorySizes(
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"));

        var recycleBytes = 0L;
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var root = drive.Name;
            recycleBytes += EstimateRecycleBinSize(root);
        }

        var downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
        var downloadsBytes = SumDirectorySize(downloadsPath);

        return new Dictionary<string, (string, long)>(StringComparer.OrdinalIgnoreCase)
        {
            ["temp"] = ("Temporäre Dateien", tempBytes),
            ["recycle"] = ("Papierkorb", recycleBytes),
            ["downloads"] = ("Downloads-Cache", downloadsBytes),
        };
    }

    private long EstimateRecycleBinSize(string driveRoot)
    {
        try
        {
            var recyclePath = Path.Combine(driveRoot.TrimEnd('\\'), "$Recycle.Bin");
            return SumDirectorySize(recyclePath, maxDepth: 2);
        }
        catch
        {
            return 0;
        }
    }

    private static long SumDirectorySizes(params string[] paths) =>
        paths.Sum(p => SumDirectorySize(p));

    private static long SumDirectorySize(string path, int maxDepth = 1)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return 0;

        return SumDirectorySizeRecursive(path, maxDepth);
    }

    private static long SumDirectorySizeRecursive(string path, int depthRemaining)
    {
        long total = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // skip locked files
                }
            }

            if (depthRemaining <= 0)
                return total;

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                try
                {
                    total += SumDirectorySizeRecursive(dir, depthRemaining - 1);
                }
                catch
                {
                    // skip inaccessible directories
                }
            }
        }
        catch
        {
            return total;
        }

        return total;
    }

    private static StorageSnapshot BuildMockSnapshot()
    {
        var drives = new List<DriveStorageInfo>
        {
            new()
            {
                Letter = "C:",
                Label = "Lokaler Datenträger (C:)",
                DriveType = "SSD",
                FileSystem = "NTFS",
                TotalBytes = (long)(299.3 * 1024 * 1024 * 1024),
                UsedBytes = (long)(211.3 * 1024 * 1024 * 1024),
                FreeBytes = (long)(88.0 * 1024 * 1024 * 1024),
                PercentUsed = 71,
                IsReady = true,
            },
            new()
            {
                Letter = "D:",
                Label = "Daten (D:)",
                DriveType = "HDD",
                FileSystem = "NTFS",
                TotalBytes = (long)(931.5 * 1024 * 1024 * 1024),
                UsedBytes = (long)(414.6 * 1024 * 1024 * 1024),
                FreeBytes = (long)(516.9 * 1024 * 1024 * 1024),
                PercentUsed = 44,
                IsReady = true,
            },
        };

        var categorySizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["Apps & Programme"] = (long)(128.3 * 1024 * 1024 * 1024),
            ["Betriebssystem"] = (long)(82.7 * 1024 * 1024 * 1024),
            ["Benutzerdateien"] = (long)(64.2 * 1024 * 1024 * 1024),
            ["Spiele"] = (long)(45.1 * 1024 * 1024 * 1024),
            ["Temporäre Dateien"] = (long)(21.8 * 1024 * 1024 * 1024),
            ["Sonstiges"] = (long)(13.7 * 1024 * 1024 * 1024),
            ["Papierkorb"] = (long)(6.7 * 1024 * 1024 * 1024),
            ["Systemreserviert"] = (long)(2.8 * 1024 * 1024 * 1024),
        };

        var used = drives[0].UsedBytes;
        var categories = StorageAnalyzer.BuildCategories(used, categorySizes);

        var cleanupSizes = new Dictionary<string, (string, long)>(StringComparer.OrdinalIgnoreCase)
        {
            ["temp"] = ("Temporäre Dateien", (long)(2.1 * 1024 * 1024 * 1024)),
            ["recycle"] = ("Papierkorb", (long)(1.4 * 1024 * 1024 * 1024)),
            ["downloads"] = ("Downloads-Cache", (long)(0.7 * 1024 * 1024 * 1024)),
        };

        var cleanupCandidates = StorageAnalyzer.BuildCleanupCandidates(cleanupSizes);
        var reclaimable = cleanupCandidates.Sum(c => c.SizeBytes);

        return new StorageSnapshot
        {
            Drives = drives,
            Categories = categories,
            CleanupCandidates = cleanupCandidates,
            TotalReclaimableBytes = reclaimable,
            CleanupChartPercent = StorageAnalyzer.CalculateCleanupChartPercent(reclaimable, used),
            IsMockData = true,
        };
    }
}
