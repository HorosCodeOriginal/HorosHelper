using System.Diagnostics;
using HorosHelp.Core.Models.Startup;
using HorosHelp.Core.Services.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.Startup;

public sealed class StartupService : IStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunDisabledKeyPath = @"Software\Microsoft\Windows\CurrentVersion\RunDisabledByHorosHelper";

    private readonly ILogger<StartupService> _logger;
    private readonly IAdminElevationService _adminElevationService;
    private readonly Dictionary<int, (TimeSpan CpuTime, DateTime Timestamp)> _processCpuSnapshots = new();
    private DateTime _lastCpuSnapshotUtc = DateTime.MinValue;

    public StartupService(ILogger<StartupService> logger, IAdminElevationService adminElevationService)
    {
        _logger = logger;
        _adminElevationService = adminElevationService;
    }

    public StartupSnapshot GetSnapshot()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("Startup APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var isAdmin = _adminElevationService.IsRunningAsAdmin;
            var entries = ReadStartupEntries(isAdmin);
            var processes = ReadBackgroundProcesses();

            return new StartupSnapshot
            {
                Entries = entries,
                BackgroundProcesses = processes,
                SafeToDisableCount = StartupImpactAnalyzer.CountSafeToDisable(entries),
                IsRunningAsAdmin = isAdmin,
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Startup snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public StartupToggleResult SetEntryEnabled(string entryId, bool enabled)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return new StartupToggleResult { Success = false, Message = "Nur unter Windows verfügbar." };

            if (!TryParseEntryId(entryId, out var source, out var name))
                return new StartupToggleResult { Success = false, Message = "Unbekannter Autostart-Eintrag." };

            return source switch
            {
                StartupEntrySource.HkcuRun => ToggleRegistryEntry(Registry.CurrentUser, RunKeyPath, RunDisabledKeyPath, name, enabled),
                StartupEntrySource.HklmRun when _adminElevationService.IsRunningAsAdmin =>
                    ToggleRegistryEntry(Registry.LocalMachine, RunKeyPath, RunDisabledKeyPath, name, enabled),
                StartupEntrySource.HklmRun =>
                    new StartupToggleResult { Success = false, Message = "HKLM-Einträge erfordern Administratorrechte." },
                StartupEntrySource.StartupFolder => ToggleStartupFolderEntry(name, enabled),
                _ => new StartupToggleResult { Success = false, Message = "Quelle nicht unterstützt." },
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to toggle startup entry {EntryId} to {Enabled}", entryId, enabled);
            return new StartupToggleResult { Success = false, Message = "Änderung fehlgeschlagen." };
        }
    }

    private List<StartupEntryInfo> ReadStartupEntries(bool isAdmin)
    {
        var entries = new List<StartupEntryInfo>();

        CollectRegistryEntries(Registry.CurrentUser, RunKeyPath, StartupEntrySource.HkcuRun, canToggle: true, entries);
        CollectRegistryEntries(Registry.LocalMachine, RunKeyPath, StartupEntrySource.HklmRun, canToggle: isAdmin, entries);
        CollectStartupFolderEntries(entries, isAdmin);

        return entries
            .OrderByDescending(e => e.Impact)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void CollectRegistryEntries(
        RegistryKey root,
        string subKeyPath,
        StartupEntrySource source,
        bool canToggle,
        List<StartupEntryInfo> entries)
    {
        try
        {
            using var key = root.OpenSubKey(subKeyPath);
            if (key is null)
                return;

            foreach (var name in key.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var command = key.GetValue(name)?.ToString() ?? "";
                var publisher = TryGetPublisher(command);
                var impact = StartupImpactAnalyzer.ClassifyImpact(name, command);

                entries.Add(new StartupEntryInfo
                {
                    Id = BuildEntryId(source, name),
                    Name = name,
                    Publisher = publisher,
                    Command = command,
                    Source = source,
                    Impact = impact,
                    IsEnabled = true,
                    CanToggle = canToggle,
                    IsSafeToDisable = StartupImpactAnalyzer.IsSafeToDisable(name, command, impact),
                });
            }

            var disabledPath = source == StartupEntrySource.HkcuRun
                ? RunDisabledKeyPath
                : RunDisabledKeyPath;

            using var disabledKey = root.OpenSubKey(disabledPath);
            if (disabledKey is null)
                return;

            foreach (var name in disabledKey.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var command = disabledKey.GetValue(name)?.ToString() ?? "";
                var publisher = TryGetPublisher(command);
                var impact = StartupImpactAnalyzer.ClassifyImpact(name, command);

                entries.Add(new StartupEntryInfo
                {
                    Id = BuildEntryId(source, name),
                    Name = name,
                    Publisher = publisher,
                    Command = command,
                    Source = source,
                    Impact = impact,
                    IsEnabled = false,
                    CanToggle = canToggle,
                    IsSafeToDisable = StartupImpactAnalyzer.IsSafeToDisable(name, command, impact),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed reading registry startup entries from {Root}\\{Path}", root.Name, subKeyPath);
        }
    }

    private void CollectStartupFolderEntries(List<StartupEntryInfo> entries, bool isAdmin)
    {
        try
        {
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (!Directory.Exists(startupFolder))
                return;

            foreach (var file in Directory.EnumerateFileSystemEntries(startupFolder))
            {
                var fileName = Path.GetFileName(file);
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                var isDisabled = fileName.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);
                var displayName = isDisabled
                    ? fileName[..^".disabled".Length]
                    : fileName;

                var command = file;
                var publisher = TryGetPublisher(file);
                var impact = StartupImpactAnalyzer.ClassifyImpact(displayName, command);

                entries.Add(new StartupEntryInfo
                {
                    Id = BuildEntryId(StartupEntrySource.StartupFolder, displayName),
                    Name = displayName,
                    Publisher = publisher,
                    Command = command,
                    Source = StartupEntrySource.StartupFolder,
                    Impact = impact,
                    IsEnabled = !isDisabled,
                    CanToggle = true,
                    IsSafeToDisable = StartupImpactAnalyzer.IsSafeToDisable(displayName, command, impact),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed reading startup folder entries.");
        }
    }

    private IReadOnlyList<BackgroundProcessInfo> ReadBackgroundProcesses()
    {
        var now = DateTime.UtcNow;
        var elapsedSeconds = _lastCpuSnapshotUtc == DateTime.MinValue
            ? 0
            : Math.Max(0.5, (now - _lastCpuSnapshotUtc).TotalSeconds);

        var processes = Process.GetProcesses()
            .Where(p =>
            {
                try
                {
                    return !string.IsNullOrWhiteSpace(p.ProcessName)
                           && p.Id != 0
                           && p.WorkingSet64 >= 5 * 1024 * 1024;
                }
                catch
                {
                    return false;
                }
            })
            .Select(p =>
            {
                try
                {
                    var cpuPercent = CalculateCpuPercent(p, elapsedSeconds);
                    return new BackgroundProcessInfo
                    {
                        Name = $"{p.ProcessName}.exe",
                        ProcessId = p.Id,
                        CpuPercent = cpuPercent,
                        WorkingSetBytes = p.WorkingSet64,
                    };
                }
                catch
                {
                    return null;
                }
                finally
                {
                    p.Dispose();
                }
            })
            .Where(p => p is not null)
            .Cast<BackgroundProcessInfo>()
            .OrderByDescending(p => p.WorkingSetBytes)
            .ThenByDescending(p => p.CpuPercent)
            .Take(8)
            .ToList();

        _lastCpuSnapshotUtc = now;
        return processes;
    }

    private double CalculateCpuPercent(Process process, double elapsedSeconds)
    {
        try
        {
            var cpuTime = process.TotalProcessorTime;
            if (!_processCpuSnapshots.TryGetValue(process.Id, out var previous))
            {
                _processCpuSnapshots[process.Id] = (cpuTime, DateTime.UtcNow);
                return 0;
            }

            var delta = (cpuTime - previous.CpuTime).TotalMilliseconds;
            var cpuPercent = delta / (elapsedSeconds * 1000 * Environment.ProcessorCount) * 100;
            _processCpuSnapshots[process.Id] = (cpuTime, DateTime.UtcNow);

            return Math.Clamp(Math.Round(cpuPercent, 0, MidpointRounding.AwayFromZero), 0, 100);
        }
        catch
        {
            return 0;
        }
    }

    private StartupToggleResult ToggleRegistryEntry(
        RegistryKey root,
        string runKeyPath,
        string disabledKeyPath,
        string name,
        bool enabled)
    {
        using var runKey = root.CreateSubKey(runKeyPath);
        using var disabledKey = root.CreateSubKey(disabledKeyPath);

        if (runKey is null || disabledKey is null)
            return new StartupToggleResult { Success = false, Message = "Registry-Zugriff fehlgeschlagen." };

        if (enabled)
        {
            var disabledValue = disabledKey.GetValue(name)?.ToString();
            if (string.IsNullOrWhiteSpace(disabledValue))
                return new StartupToggleResult { Success = false, Message = "Eintrag nicht im deaktivierten Speicher gefunden." };

            runKey.SetValue(name, disabledValue);
            disabledKey.DeleteValue(name, throwOnMissingValue: false);
            return new StartupToggleResult { Success = true, Message = $"{name} aktiviert." };
        }

        var currentValue = runKey.GetValue(name)?.ToString();
        if (string.IsNullOrWhiteSpace(currentValue))
            return new StartupToggleResult { Success = false, Message = "Eintrag nicht gefunden." };

        disabledKey.SetValue(name, currentValue);
        runKey.DeleteValue(name, throwOnMissingValue: false);
        return new StartupToggleResult { Success = true, Message = $"{name} deaktiviert." };
    }

    private StartupToggleResult ToggleStartupFolderEntry(string name, bool enabled)
    {
        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var activePath = Path.Combine(startupFolder, name);
        var disabledPath = activePath + ".disabled";

        if (enabled)
        {
            if (!File.Exists(disabledPath))
                return new StartupToggleResult { Success = false, Message = "Deaktivierte Datei nicht gefunden." };

            File.Move(disabledPath, activePath);
            return new StartupToggleResult { Success = true, Message = $"{name} aktiviert." };
        }

        if (!File.Exists(activePath))
            return new StartupToggleResult { Success = false, Message = "Autostart-Datei nicht gefunden." };

        if (File.Exists(disabledPath))
            File.Delete(disabledPath);

        File.Move(activePath, disabledPath);
        return new StartupToggleResult { Success = true, Message = $"{name} deaktiviert." };
    }

    private static string TryGetPublisher(string command)
    {
        var exePath = ExtractExecutablePath(command);
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return "Unbekannt";

        try
        {
            var info = FileVersionInfo.GetVersionInfo(exePath);
            return string.IsNullOrWhiteSpace(info.CompanyName)
                ? info.ProductName ?? "Unbekannt"
                : info.CompanyName;
        }
        catch
        {
            return "Unbekannt";
        }
    }

    private static string? ExtractExecutablePath(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return null;

        command = command.Trim();
        if (command.StartsWith('"'))
        {
            var end = command.IndexOf('"', 1);
            return end > 1 ? command[1..end] : null;
        }

        var space = command.IndexOf(' ');
        return space > 0 ? command[..space] : command;
    }

    private static string BuildEntryId(StartupEntrySource source, string name) =>
        $"{source}:{name}";

    private static bool TryParseEntryId(string entryId, out StartupEntrySource source, out string name)
    {
        source = default;
        name = "";

        var separator = entryId.IndexOf(':');
        if (separator <= 0 || separator >= entryId.Length - 1)
            return false;

        var sourceText = entryId[..separator];
        name = entryId[(separator + 1)..];

        return Enum.TryParse(sourceText, ignoreCase: true, out source);
    }

    private static StartupSnapshot BuildMockSnapshot() =>
        new()
        {
            Entries =
            [
                CreateMockEntry("Spotify", "Spotify AB", StartupImpact.Hoch, true, true),
                CreateMockEntry("Discord", "Discord Inc.", StartupImpact.Mittel, true, true),
                CreateMockEntry("Steam", "Valve Corporation", StartupImpact.Niedrig, true, false),
                CreateMockEntry("OneDrive", "Microsoft Corporation", StartupImpact.Hoch, true, true),
                CreateMockEntry("NVIDIA", "NVIDIA Corporation", StartupImpact.Niedrig, true, false),
            ],
            BackgroundProcesses =
            [
                new() { Name = "Chrome.exe", ProcessId = 1, CpuPercent = 12, WorkingSetBytes = 512L * 1024 * 1024 },
                new() { Name = "Teams.exe", ProcessId = 2, CpuPercent = 8, WorkingSetBytes = 342L * 1024 * 1024 },
                new() { Name = "explorer.exe", ProcessId = 3, CpuPercent = 2, WorkingSetBytes = 128L * 1024 * 1024 },
            ],
            SafeToDisableCount = 3,
            IsRunningAsAdmin = false,
            IsMockData = true,
        };

    private static StartupEntryInfo CreateMockEntry(
        string name,
        string publisher,
        StartupImpact impact,
        bool enabled,
        bool safeToDisable) =>
        new()
        {
            Id = BuildEntryId(StartupEntrySource.HkcuRun, name),
            Name = name,
            Publisher = publisher,
            Command = name,
            Source = StartupEntrySource.HkcuRun,
            Impact = impact,
            IsEnabled = enabled,
            CanToggle = true,
            IsSafeToDisable = safeToDisable,
        };
}
