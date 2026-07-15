using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using HorosHelp.Core.Models.Backup;
using HorosHelp.Core.Services.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.Backup;

public sealed partial class BackupService : IBackupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<BackupService> _logger;
    private readonly IAdminElevationService _adminElevationService;
    private readonly string _configDirectory;
    private readonly string _profilesPath;

    public BackupService(ILogger<BackupService> logger, IAdminElevationService adminElevationService)
    {
        _logger = logger;
        _adminElevationService = adminElevationService;
        _configDirectory = Path.Combine(
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

            var restorePoints = ReadRestorePoints();
            var profiles = LoadProfiles();

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

    public async Task<BackupOperationResult> CreateRestorePointAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return Fail("Nur unter Windows verfügbar.");

            if (!_adminElevationService.IsRunningAsAdmin)
            {
                return new BackupOperationResult
                {
                    Success = false,
                    Message = "Wiederherstellungspunkte erfordern Administratorrechte (UAC).",
                };
            }

            var description = $"HorosHelper — {DateTime.Now:dd.MM.yyyy HH:mm}";
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"Checkpoint-Computer -Description '{description}' -RestorePointType MODIFY_SETTINGS\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
                return Fail("PowerShell konnte nicht gestartet werden.");

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Restore point creation failed: {Error}", error);
                return Fail("Wiederherstellungspunkt konnte nicht erstellt werden.");
            }

            return new BackupOperationResult
            {
                Success = true,
                Message = "Wiederherstellungspunkt wurde erstellt.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Create restore point failed.");
            return Fail("Wiederherstellungspunkt fehlgeschlagen.");
        }
    }

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

            if (profile.SourceFolders.Count == 0)
                return Fail("Profil enthält keine Quellordner.");

            Directory.CreateDirectory(profile.DestinationFolder);

            long bytesCopied = 0;
            var filesCopied = 0;

            foreach (var source in profile.SourceFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Directory.Exists(source))
                {
                    _logger.LogDebug("Source folder missing: {Source}", source);
                    continue;
                }

                var targetRoot = Path.Combine(profile.DestinationFolder, profile.Name, Path.GetFileName(source.TrimEnd('\\', '/')));
                Directory.CreateDirectory(targetRoot);

                foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relative = Path.GetRelativePath(source, file);
                    var target = Path.Combine(targetRoot, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);

                    File.Copy(file, target, overwrite: true);
                    var info = new FileInfo(file);
                    bytesCopied += info.Length;
                    filesCopied++;
                }
            }

            if (filesCopied == 0)
                return Fail("Keine Dateien zum Sichern gefunden.");

            await SaveProfileBackupMetaAsync(new BackupProfileConfig
            {
                Id = profile.Id,
                Name = profile.Name,
                SourceFolders = profile.SourceFolders,
                DestinationFolder = profile.DestinationFolder,
                LastBackupUtc = DateTimeOffset.UtcNow,
                LastBackupSizeBytes = bytesCopied,
                LastBackupStatus = "Erfolgreich",
            }, cancellationToken);

            return new BackupOperationResult
            {
                Success = true,
                Message = $"Backup abgeschlossen — {filesCopied} Dateien kopiert.",
                BytesCopied = bytesCopied,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Profile backup failed for {ProfileId}", profileId);
            return Fail("Backup fehlgeschlagen.");
        }
    }

    private List<RestorePointInfo> ReadRestorePoints()
    {
        var points = TryReadRestorePointsViaPowerShell();
        if (points.Count > 0)
            return points;

        return BuildMockSnapshot().RestorePoints.ToList();
    }

    private List<RestorePointInfo> TryReadRestorePointsViaPowerShell()
    {
        var points = new List<RestorePointInfo>();

        try
        {
            var output = RunProcessSync(
                "powershell",
                "-NoProfile -Command \"Get-ComputerRestorePoint | Select-Object SequenceNumber,CreationTime,RestorePointType,Description | ConvertTo-Json -Compress\"");

            if (string.IsNullOrWhiteSpace(output))
                return points;

            foreach (Match match in RestorePointBlockRegex().Matches(output))
            {
                if (!int.TryParse(match.Groups["seq"].Value, out var seq))
                    continue;

                var created = DateTime.TryParse(match.Groups["time"].Value, out var dt)
                    ? dt
                    : DateTime.Now;

                points.Add(new RestorePointInfo
                {
                    SequenceNumber = seq,
                    CreatedAt = created,
                    Type = MapRestoreType(match.Groups["type"].Value),
                    Description = match.Groups["desc"].Value.Trim(),
                });
            }

            if (points.Count == 0 && output.Contains("SequenceNumber", StringComparison.OrdinalIgnoreCase))
            {
                points.Add(ParseSingleRestorePoint(output));
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "PowerShell restore point read failed.");
        }

        return points
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToList();
    }

    private static RestorePointInfo ParseSingleRestorePoint(string json)
    {
        var seq = SequenceRegex().Match(json).Groups[1].Value;
        var time = TimeRegex().Match(json).Groups[1].Value;
        var type = TypeRegex().Match(json).Groups[1].Value;
        var desc = DescRegex().Match(json).Groups[1].Value;

        return new RestorePointInfo
        {
            SequenceNumber = int.TryParse(seq, out var s) ? s : 0,
            CreatedAt = DateTime.TryParse(time, out var dt) ? dt : DateTime.Now,
            Type = MapRestoreType(type),
            Description = string.IsNullOrWhiteSpace(desc) ? "Wiederherstellungspunkt" : desc,
        };
    }

    private static string MapRestoreType(string raw) => raw switch
    {
        "0" => "Anwendung",
        "1" => "Deinstallation",
        "10" => "Manuell",
        "12" => "Windows Update",
        _ => string.IsNullOrWhiteSpace(raw) ? "System" : raw,
    };

    private List<BackupProfileConfig> LoadProfiles()
    {
        try
        {
            if (File.Exists(_profilesPath))
            {
                var json = File.ReadAllText(_profilesPath);
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
            Directory.CreateDirectory(_configDirectory);
            var json = JsonSerializer.Serialize(profiles, JsonOptions);
            File.WriteAllText(_profilesPath, json);
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

        Directory.CreateDirectory(_configDirectory);
        var json = JsonSerializer.Serialize(profiles, JsonOptions);
        await File.WriteAllTextAsync(_profilesPath, json, cancellationToken);
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
            },
            new()
            {
                Id = "desktop",
                Name = "Desktop",
                SourceFolders = [desktop],
                DestinationFolder = backupRoot,
            },
            new()
            {
                Id = "pictures",
                Name = "Bilder",
                SourceFolders = [pictures],
                DestinationFolder = backupRoot,
            },
            new()
            {
                Id = "full",
                Name = "Vollbackup",
                SourceFolders = [documents, desktop, pictures],
                DestinationFolder = backupRoot,
            },
        ];
    }

    private static string RunProcessSync(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return "";

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return string.IsNullOrWhiteSpace(output) ? error : output;
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

    [GeneratedRegex(
        @"\{[^}]*""SequenceNumber""\s*:\s*(?<seq>\d+)[^}]*""CreationTime""\s*:\s*""(?<time>[^""]+)""[^}]*""RestorePointType""\s*:\s*(?<type>\d+)[^}]*""Description""\s*:\s*""(?<desc>[^""]*)""[^}]*\}",
        RegexOptions.IgnoreCase)]
    private static partial Regex RestorePointBlockRegex();

    [GeneratedRegex(@"""SequenceNumber""\s*:\s*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex SequenceRegex();

    [GeneratedRegex(@"""CreationTime""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"""RestorePointType""\s*:\s*(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex TypeRegex();

    [GeneratedRegex(@"""Description""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase)]
    private static partial Regex DescRegex();
}
