using System.Text.Json;
using HorosHelp.Core.Models.Backup;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Security;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Backup;

public sealed class BackupService : IBackupService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<BackupService> _logger;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IRestorePointService _restorePointService;
    private readonly IBackupFileSystem _fileSystem;
    private readonly IFileHashService _hashService;
    private readonly IBackupManifestStore _manifestStore;
    private readonly IBackupEncryptionService _encryptionService;
    private readonly IBackupSchedulerService _schedulerService;
    private readonly string _configDirectory;
    private readonly string _profilesPath;

    public BackupService(
        ILogger<BackupService> logger,
        IAdminElevationService adminElevationService,
        IRestorePointService restorePointService,
        IBackupFileSystem fileSystem,
        IFileHashService hashService,
        IBackupManifestStore manifestStore,
        IBackupEncryptionService encryptionService,
        IBackupSchedulerService schedulerService,
        string? configDirectoryOverride = null)
    {
        _logger = logger;
        _adminElevationService = adminElevationService;
        _restorePointService = restorePointService;
        _fileSystem = fileSystem;
        _hashService = hashService;
        _manifestStore = manifestStore;
        _encryptionService = encryptionService;
        _schedulerService = schedulerService;
        _configDirectory = configDirectoryOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HorosHelper");
        _profilesPath = Path.Combine(_configDirectory, "backup-profiles.json");
    }

    public BackupSnapshot GetSnapshot()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("Backup APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var restorePoints = _restorePointService.GetRestorePoints();
            var profiles = LoadProfiles();

            if (restorePoints.Count == 0)
                restorePoints = BuildMockSnapshot().RestorePoints.ToList();

            return new BackupSnapshot
            {
                RestorePoints = restorePoints,
                Profiles = profiles,
                IsRunningAsAdmin = _adminElevationService.IsRunningAsAdmin,
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Backup snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public Task<BackupOperationResult> CreateRestorePointAsync(CancellationToken cancellationToken = default) =>
        _restorePointService.CreateRestorePointAsync(cancellationToken);

    public string GetRestorePointCreationMethod() => _restorePointService.LastCreationMethod;

    public async Task<BackupOperationResult> RunProfileBackupAsync(
        string profileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profiles = LoadProfiles();
            var profile = profiles.FirstOrDefault(p => p.Id.Equals(profileId, StringComparison.OrdinalIgnoreCase))
                          ?? profiles.FirstOrDefault();

            if (profile is null)
                return Fail("Kein Backup-Profil konfiguriert.");

            if (!InputSecurityValidator.IsValidFilePath(profile.DestinationFolder, out var destError))
                return Fail(destError);

            if (profile.SourceFolders.Count == 0)
                return Fail("Profil enthält keine Quellordner.");

            foreach (var source in profile.SourceFolders)
            {
                if (!InputSecurityValidator.IsValidFilePath(source, out var sourceError))
                    return Fail($"Ungültiger Quellordner: {sourceError}");
            }

            _fileSystem.CreateDirectory(profile.DestinationFolder);

            var previousManifest = _manifestStore.Load(profile.DestinationFolder, profile.Name);
            var previousEntries = previousManifest?.Files.ToDictionary(
                e => e.RelativePath,
                e => e,
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, BackupManifestEntry>(StringComparer.OrdinalIgnoreCase);

            long bytesCopied = 0;
            var filesCopied = 0;
            var filesSkipped = 0;
            var manifestEntries = new List<BackupManifestEntry>();

            foreach (var source in profile.SourceFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_fileSystem.DirectoryExists(source))
                {
                    _logger.LogDebug("Source folder missing: {Source}", source);
                    continue;
                }

                var sourceFolderName = Path.GetFileName(source.TrimEnd('\\', '/'));
                var targetRoot = Path.Combine(profile.DestinationFolder, profile.Name, sourceFolderName);
                _fileSystem.CreateDirectory(targetRoot);

                foreach (var file in _fileSystem.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relative = $"{sourceFolderName}/{Path.GetRelativePath(source, file).Replace('\\', '/')}";
                    var hash = _hashService.ComputeSha256(file);
                    var lastModified = _fileSystem.GetLastWriteTimeUtc(file);
                    var size = _fileSystem.GetFileLength(file);

                    if (previousEntries.TryGetValue(relative, out var existing) &&
                        existing.Sha256.Equals(hash, StringComparison.OrdinalIgnoreCase) &&
                        existing.SizeBytes == size)
                    {
                        manifestEntries.Add(existing);
                        filesSkipped++;
                        continue;
                    }

                    var target = Path.Combine(targetRoot, Path.GetRelativePath(source, file));
                    var targetDirectory = Path.GetDirectoryName(target);
                    if (!string.IsNullOrWhiteSpace(targetDirectory))
                        _fileSystem.CreateDirectory(targetDirectory);

                    if (profile.EncryptBackups && _encryptionService.IsEncryptionAvailable)
                    {
                        var encryptedTarget = target + ".horos.enc";
                        await _encryptionService.EncryptFileAsync(file, encryptedTarget, cancellationToken);
                        if (_fileSystem.FileExists(target))
                            _fileSystem.DeleteFile(target);
                    }
                    else
                    {
                        _fileSystem.CopyFile(file, target, overwrite: true);
                    }

                    manifestEntries.Add(new BackupManifestEntry
                    {
                        RelativePath = relative,
                        Sha256 = hash,
                        SizeBytes = size,
                        LastModifiedUtc = lastModified,
                        IsEncrypted = profile.EncryptBackups && _encryptionService.IsEncryptionAvailable,
                    });

                    bytesCopied += size;
                    filesCopied++;
                }
            }

            if (filesCopied == 0 && filesSkipped == 0)
                return Fail("Keine Dateien zum Sichern gefunden.");

            var manifest = new BackupManifest
            {
                ProfileId = profile.Id,
                ProfileName = profile.Name,
                CreatedUtc = previousManifest?.CreatedUtc ?? DateTimeOffset.UtcNow,
                LastRunUtc = DateTimeOffset.UtcNow,
                EncryptionEnabled = profile.EncryptBackups,
                Files = manifestEntries,
            };

            await _manifestStore.SaveAsync(manifest, profile.DestinationFolder, profile.Name, cancellationToken);

            await SaveProfileBackupMetaAsync(new BackupProfileConfig
            {
                Id = profile.Id,
                Name = profile.Name,
                SourceFolders = profile.SourceFolders,
                DestinationFolder = profile.DestinationFolder,
                Schedule = profile.Schedule,
                EncryptBackups = profile.EncryptBackups,
                LastBackupUtc = DateTimeOffset.UtcNow,
                LastBackupSizeBytes = bytesCopied > 0 ? bytesCopied : previousManifest?.Files.Sum(f => f.SizeBytes),
                LastBackupStatus = "Erfolgreich",
            }, cancellationToken);

            var message = filesCopied > 0
                ? $"Backup abgeschlossen — {filesCopied} Dateien kopiert, {filesSkipped} unverändert."
                : $"Backup abgeschlossen — alle {filesSkipped} Dateien unverändert (inkrementell).";

            return new BackupOperationResult
            {
                Success = true,
                Message = message,
                BytesCopied = bytesCopied,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Profile backup failed for {ProfileId}", profileId);
            return Fail("Backup fehlgeschlagen.");
        }
    }

    public bool RegisterProfileSchedule(string profileId, string executablePath)
    {
        if (!InputSecurityValidator.IsValidFilePath(executablePath, out _))
            return false;

        var profiles = LoadProfiles();
        var profile = profiles.FirstOrDefault(p => p.Id.Equals(profileId, StringComparison.OrdinalIgnoreCase));
        if (profile is null)
            return false;

        return _schedulerService.RegisterProfileSchedule(profile, executablePath);
    }

    public bool UnregisterProfileSchedule(string profileId) =>
        _schedulerService.UnregisterProfileSchedule(profileId);

    private List<BackupProfileConfig> LoadProfiles()
    {
        try
        {
            if (_fileSystem.FileExists(_profilesPath))
            {
                var json = _fileSystem.ReadAllText(_profilesPath);
                var profiles = JsonSerializer.Deserialize<List<BackupProfileConfig>>(json, JsonOptions);
                if (profiles is { Count: > 0 })
                    return profiles;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to load backup profiles; using defaults.");
        }

        var defaults = CreateDefaultProfiles();
        SaveProfiles(defaults);
        return defaults;
    }

    private void SaveProfiles(IReadOnlyList<BackupProfileConfig> profiles)
    {
        try
        {
            _fileSystem.CreateDirectory(_configDirectory);
            var json = JsonSerializer.Serialize(profiles, JsonOptions);
            _fileSystem.WriteAllTextAsync(_profilesPath, json).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save backup profiles.");
        }
    }

    private async Task SaveProfileBackupMetaAsync(BackupProfileConfig updated, CancellationToken cancellationToken)
    {
        var profiles = LoadProfiles().ToList();
        var index = profiles.FindIndex(p => p.Id.Equals(updated.Id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            profiles[index] = updated;
        else
            profiles.Add(updated);

        _fileSystem.CreateDirectory(_configDirectory);
        var json = JsonSerializer.Serialize(profiles, JsonOptions);
        await _fileSystem.WriteAllTextAsync(_profilesPath, json, cancellationToken);
    }

    private static List<BackupProfileConfig> CreateDefaultProfiles()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        var backupRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "HorosHelper-Backups");

        return
        [
            new()
            {
                Id = "documents",
                Name = "Dokumente",
                SourceFolders = [documents],
                DestinationFolder = backupRoot,
                EncryptBackups = true,
                Schedule = new BackupScheduleConfig { IsEnabled = false, Frequency = "Manuell" },
            },
            new()
            {
                Id = "desktop",
                Name = "Desktop",
                SourceFolders = [desktop],
                DestinationFolder = backupRoot,
                EncryptBackups = true,
                Schedule = new BackupScheduleConfig
                {
                    IsEnabled = true,
                    Frequency = "Taeglich",
                    TimeOfDay = "02:00",
                },
            },
            new()
            {
                Id = "pictures",
                Name = "Bilder",
                SourceFolders = [pictures],
                DestinationFolder = backupRoot,
                EncryptBackups = true,
                Schedule = new BackupScheduleConfig { IsEnabled = false, Frequency = "Manuell" },
            },
            new()
            {
                Id = "full",
                Name = "Vollbackup",
                SourceFolders = [documents, desktop, pictures],
                DestinationFolder = backupRoot,
                EncryptBackups = true,
                Schedule = new BackupScheduleConfig
                {
                    IsEnabled = true,
                    Frequency = "Woechentlich",
                    TimeOfDay = "03:00",
                    DayOfWeek = "Sunday",
                },
            },
        ];
    }

    private static BackupOperationResult Fail(string message) =>
        new() { Success = false, Message = message };

    private static BackupSnapshot BuildMockSnapshot() =>
        new()
        {
            RestorePoints =
            [
                new()
                {
                    SequenceNumber = 3,
                    CreatedAt = new DateTime(2026, 7, 15, 1, 30, 0),
                    Type = "Manuell",
                    Description = "Manuell",
                },
                new()
                {
                    SequenceNumber = 2,
                    CreatedAt = new DateTime(2026, 7, 14, 18, 0, 0),
                    Type = "Windows Update",
                    Description = "Windows Update",
                },
                new()
                {
                    SequenceNumber = 1,
                    CreatedAt = new DateTime(2026, 7, 12, 9, 15, 0),
                    Type = "App-Installation",
                    Description = "App-Installation",
                },
            ],
            Profiles = CreateDefaultProfiles(),
            IsRunningAsAdmin = false,
            IsMockData = true,
        };
}
