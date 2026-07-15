using System.Text.Json;
using HorosHelp.Core.Models.ProblemScan;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.ProblemScan;

public interface IRollbackStore
{
    string CreateSnapshotDirectory(ProblemKind repairKind, string description);

    void SaveManifest(RollbackManifest manifest);

    IReadOnlyList<RollbackEntry> GetRecentEntries(int maxCount = 10);

    Task<IReadOnlyList<ScanLogEntry>> RollbackAsync(string rollbackId, CancellationToken cancellationToken = default);
}

public sealed class RollbackStore : IRollbackStore
{
    private const int MaxStoredRollbacks = 20;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<RollbackStore> _logger;
    private readonly string _rootPath;

    public RollbackStore(ILogger<RollbackStore> logger)
    {
        _logger = logger;
        _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HorosHelper",
            "rollback");
        Directory.CreateDirectory(_rootPath);
    }

    public RollbackStore(ILogger<RollbackStore> logger, string rootPath)
    {
        _logger = logger;
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public string CreateSnapshotDirectory(ProblemKind repairKind, string description)
    {
        var id = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{repairKind}-{Guid.NewGuid():N}"[..Math.Min(64, 48 + repairKind.ToString().Length)];
        var path = Path.Combine(_rootPath, id);
        Directory.CreateDirectory(path);
        _logger.LogDebug("Created rollback snapshot directory {Path} for {Kind}", path, repairKind);
        return path;
    }

    public void SaveManifest(RollbackManifest manifest)
    {
        var dir = Path.Combine(_rootPath, manifest.Id);
        Directory.CreateDirectory(dir);
        var manifestPath = Path.Combine(dir, "manifest.json");
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions));
        TrimOldRollbacks();
    }

    public IReadOnlyList<RollbackEntry> GetRecentEntries(int maxCount = 10)
    {
        if (!Directory.Exists(_rootPath))
            return [];

        var entries = new List<RollbackEntry>();

        foreach (var dir in Directory.EnumerateDirectories(_rootPath).OrderByDescending(Path.GetFileName))
        {
            var manifestPath = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifestPath))
                continue;

            try
            {
                var manifest = JsonSerializer.Deserialize<RollbackManifest>(File.ReadAllText(manifestPath), JsonOptions);
                if (manifest is null)
                    continue;

                entries.Add(new RollbackEntry
                {
                    Id = manifest.Id,
                    RepairId = manifest.Id,
                    RepairKind = manifest.RepairKind,
                    EntryKind = manifest.Items.FirstOrDefault()?.Kind ?? RollbackEntryKind.FileList,
                    Description = manifest.Description,
                    Timestamp = manifest.Timestamp,
                    SnapshotPath = dir,
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed reading rollback manifest in {Dir}", dir);
            }

            if (entries.Count >= maxCount)
                break;
        }

        return entries;
    }

    public async Task<IReadOnlyList<ScanLogEntry>> RollbackAsync(
        string rollbackId,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog($"Rollback wird ausgeführt: {rollbackId}", ScanLogStatus.InProgress),
        };

        var dir = Path.Combine(_rootPath, rollbackId);
        var manifestPath = Path.Combine(dir, "manifest.json");

        if (!File.Exists(manifestPath))
        {
            entries.Add(CreateLog("Rollback-Snapshot nicht gefunden.", ScanLogStatus.Error));
            return entries;
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<RollbackManifest>(
                await File.ReadAllTextAsync(manifestPath, cancellationToken),
                JsonOptions);

            if (manifest is null)
            {
                entries.Add(CreateLog("Rollback-Manifest ungültig.", ScanLogStatus.Error));
                return entries;
            }

            foreach (var item in manifest.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var itemPath = Path.Combine(dir, item.RelativePath);

                switch (item.Kind)
                {
                    case RollbackEntryKind.RegistryValue:
                        if (RestoreRegistryValue(itemPath, item.Metadata))
                            entries.Add(CreateLog($"Registry wiederhergestellt: {item.Metadata}", ScanLogStatus.Success));
                        else
                            entries.Add(CreateLog($"Registry-Wiederherstellung fehlgeschlagen: {item.Metadata}", ScanLogStatus.Warning));
                        break;

                    case RollbackEntryKind.FileList:
                        entries.Add(CreateLog($"Dateiliste gesichert unter {item.RelativePath} (manuell prüfen).", ScanLogStatus.Warning));
                        break;

                    case RollbackEntryKind.ServiceState:
                        entries.Add(CreateLog($"Dienstzustand dokumentiert: {item.Metadata}", ScanLogStatus.Warning));
                        break;
                }
            }

            entries.Add(CreateLog("Rollback abgeschlossen.", ScanLogStatus.Success));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rollback failed for {RollbackId}", rollbackId);
            entries.Add(CreateLog("Rollback fehlgeschlagen.", ScanLogStatus.Error));
        }

        return entries;
    }

    public static bool RestoreRegistryValue(string exportPath, string? metadata)
    {
        if (!File.Exists(exportPath) || string.IsNullOrWhiteSpace(metadata))
            return false;

        var parts = metadata.Split('|', 3);
        if (parts.Length < 3)
            return false;

        var hive = parts[0];
        var subKey = parts[1];
        var valueName = parts[2];
        var valueData = File.ReadAllText(exportPath);

        var root = hive.Equals("HKLM", StringComparison.OrdinalIgnoreCase)
            ? Microsoft.Win32.Registry.LocalMachine
            : Microsoft.Win32.Registry.CurrentUser;

        using var key = root.CreateSubKey(subKey);
        key?.SetValue(valueName, valueData);
        return true;
    }

    internal void SaveRegistryValueSnapshot(
        string snapshotDir,
        string hive,
        string subKey,
        string valueName,
        string valueData,
        RollbackManifest manifest,
        List<RollbackManifestItem> items)
    {
        var fileName = $"registry-{items.Count + 1}.txt";
        var filePath = Path.Combine(snapshotDir, fileName);
        File.WriteAllText(filePath, valueData);
        items.Add(new RollbackManifestItem
        {
            Kind = RollbackEntryKind.RegistryValue,
            RelativePath = fileName,
            Metadata = $"{hive}|{subKey}|{valueName}",
        });
    }

    private void TrimOldRollbacks()
    {
        try
        {
            var dirs = Directory.EnumerateDirectories(_rootPath)
                .OrderByDescending(Path.GetFileName)
                .Skip(MaxStoredRollbacks)
                .ToList();

            foreach (var dir in dirs)
                Directory.Delete(dir, recursive: true);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed trimming old rollback directories.");
        }
    }

    private static ScanLogEntry CreateLog(string message, ScanLogStatus status) =>
        new()
        {
            Timestamp = DateTime.Now,
            Message = message,
            Status = status,
        };
}
