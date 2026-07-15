using System.Diagnostics;
using System.Text;
using HorosHelp.Core.Models.ProblemScan;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.ProblemScan;

public abstract class RepairActionBase : IRepairAction
{
    protected RepairActionBase(ILogger logger) => Logger = logger;

    protected ILogger Logger { get; }

    public abstract ProblemKind Kind { get; }

    public virtual bool RequiresAdmin => true;

    public abstract RepairCommandSpec BuildCommand();

    public virtual async Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog($"{BuildCommand().Description} wird ausgeführt...", ScanLogStatus.InProgress),
        };

        if (RequiresAdmin && !isRunningAsAdmin)
        {
            entries.Add(CreateLog(
                "Administratorrechte erforderlich — bitte HorosHelper mit UAC starten.",
                ScanLogStatus.Warning));
            return entries;
        }

        if (!OperatingSystem.IsWindows())
        {
            entries.Add(CreateLog("Nur unter Windows verfügbar.", ScanLogStatus.Warning));
            return entries;
        }

        try
        {
            var spec = BuildCommand();
            var psi = new ProcessStartInfo
            {
                FileName = spec.FileName,
                Arguments = spec.Arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                entries.Add(CreateLog("Prozess konnte nicht gestartet werden.", ScanLogStatus.Error));
                return entries;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                Logger.LogWarning(
                    "Repair {Kind} failed with exit code {ExitCode}: {Error}",
                    Kind, process.ExitCode, error);

                entries.Add(CreateLog(
                    $"{spec.Description} fehlgeschlagen (Code {process.ExitCode}).",
                    ScanLogStatus.Error));
                return entries;
            }

            entries.AddRange(BuildSuccessEntries(spec, output));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Repair {Kind} failed.", Kind);
            entries.Add(CreateLog($"{BuildCommand().Description} fehlgeschlagen.", ScanLogStatus.Error));
        }

        return entries;
    }

    protected virtual IReadOnlyList<ScanLogEntry> BuildSuccessEntries(string output) =>
        [CreateLog($"{BuildCommand().Description} erfolgreich abgeschlossen.", ScanLogStatus.Success)];

    protected virtual IReadOnlyList<ScanLogEntry> BuildSuccessEntries(RepairCommandSpec spec, string output) =>
        BuildSuccessEntries(output);

    protected static ScanLogEntry CreateLog(string message, ScanLogStatus status) =>
        new()
        {
            Timestamp = DateTime.Now,
            Message = message,
            Status = status,
        };
}

public sealed class DnsFlushRepair : RepairActionBase
{
    public DnsFlushRepair(ILogger<DnsFlushRepair> logger) : base(logger) { }

    public override ProblemKind Kind => ProblemKind.DnsFlush;

    public override RepairCommandSpec BuildCommand() => RepairCommandBuilder.BuildDnsFlush();
}

public sealed class WinsockResetRepair : RepairActionBase
{
    public WinsockResetRepair(ILogger<WinsockResetRepair> logger) : base(logger) { }

    public override ProblemKind Kind => ProblemKind.WinsockReset;

    public override RepairCommandSpec BuildCommand() => RepairCommandBuilder.BuildWinsockReset();

    protected override IReadOnlyList<ScanLogEntry> BuildSuccessEntries(RepairCommandSpec spec, string output)
    {
        return
        [
            CreateLog($"{spec.Description} erfolgreich abgeschlossen.", ScanLogStatus.Success),
            CreateLog("Ein Neustart des PCs wird empfohlen, damit die Änderung wirksam wird.", ScanLogStatus.Warning),
        ];
    }
}

public sealed class WindowsUpdateCacheRepair : RepairActionBase
{
    public WindowsUpdateCacheRepair(ILogger<WindowsUpdateCacheRepair> logger) : base(logger) { }

    public override ProblemKind Kind => ProblemKind.WindowsUpdateCache;

    public override RepairCommandSpec BuildCommand() => RepairCommandBuilder.BuildStopWindowsUpdateService();

    public override async Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog("Windows-Update-Cache wird geleert...", ScanLogStatus.InProgress),
        };

        if (!isRunningAsAdmin)
        {
            entries.Add(CreateLog(
                "Administratorrechte erforderlich — bitte HorosHelper mit UAC starten.",
                ScanLogStatus.Warning));
            return entries;
        }

        if (!OperatingSystem.IsWindows())
        {
            entries.Add(CreateLog("Nur unter Windows verfügbar.", ScanLogStatus.Warning));
            return entries;
        }

        try
        {
            var stopSpec = RepairCommandBuilder.BuildStopWindowsUpdateService();
            entries.Add(CreateLog(stopSpec.Description + "...", ScanLogStatus.InProgress));
            var stopResult = await RepairProcessRunner.RunAsync(stopSpec, cancellationToken);
            if (!stopResult.Success)
            {
                entries.Add(CreateLog(
                    $"{stopSpec.Description} fehlgeschlagen (Code {stopResult.ExitCode}).",
                    ScanLogStatus.Warning));
            }
            else
            {
                entries.Add(CreateLog($"{stopSpec.Description} abgeschlossen.", ScanLogStatus.Success));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var downloadPath = RepairCommandBuilder.GetWindowsUpdateDownloadPath();
            entries.Add(CreateLog($"Download-Cache wird gelöscht: {downloadPath}", ScanLogStatus.InProgress));

            var (deleted, errors) = await Task.Run(
                () => ClearDirectoryContents(downloadPath, cancellationToken),
                cancellationToken);

            entries.Add(CreateLog(
                $"Download-Cache bereinigt: {deleted} Elemente entfernt" +
                (errors > 0 ? $", {errors} übersprungen" : "") + ".",
                ScanLogStatus.Success));

            cancellationToken.ThrowIfCancellationRequested();

            var startSpec = RepairCommandBuilder.BuildStartWindowsUpdateService();
            entries.Add(CreateLog(startSpec.Description + "...", ScanLogStatus.InProgress));
            var startResult = await RepairProcessRunner.RunAsync(startSpec, cancellationToken);
            if (!startResult.Success)
            {
                entries.Add(CreateLog(
                    $"{startSpec.Description} fehlgeschlagen (Code {startResult.ExitCode}).",
                    ScanLogStatus.Error));
                return entries;
            }

            entries.Add(CreateLog("Windows-Update-Cache erfolgreich geleert.", ScanLogStatus.Success));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Repair {Kind} failed.", Kind);
            entries.Add(CreateLog("Windows-Update-Cache konnte nicht geleert werden.", ScanLogStatus.Error));
        }

        return entries;
    }

    private static (int Deleted, int Errors) ClearDirectoryContents(string path, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(path))
            return (0, 0);

        var deleted = 0;
        var errors = 0;

        foreach (var entry in Directory.EnumerateFileSystemEntries(path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (Directory.Exists(entry))
                    Directory.Delete(entry, recursive: true);
                else
                    File.Delete(entry);

                deleted++;
            }
            catch
            {
                errors++;
            }
        }

        return (deleted, errors);
    }
}

public sealed class SfcDismRepair : RepairActionBase
{
    public SfcDismRepair(ILogger<SfcDismRepair> logger) : base(logger) { }

    public override ProblemKind Kind => ProblemKind.SystemFileCheck;

    public override RepairCommandSpec BuildCommand() => RepairCommandBuilder.BuildSfcScanNow();

    public override async Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog("SFC/DISM-Systemreparatur wird gestartet...", ScanLogStatus.InProgress),
            CreateLog(
                "Hinweis: SFC und DISM können 15–45 Minuten dauern. Bitte HorosHelper geöffnet lassen.",
                ScanLogStatus.Warning),
        };

        if (!isRunningAsAdmin)
        {
            entries.Add(CreateLog(
                "Administratorrechte erforderlich — bitte HorosHelper mit UAC starten.",
                ScanLogStatus.Warning));
            return entries;
        }

        if (!OperatingSystem.IsWindows())
        {
            entries.Add(CreateLog("Nur unter Windows verfügbar.", ScanLogStatus.Warning));
            return entries;
        }

        try
        {
            var sfcSpec = RepairCommandBuilder.BuildSfcScanNow();
            entries.Add(CreateLog($"{sfcSpec.Description} wird ausgeführt...", ScanLogStatus.InProgress));

            var sfcResult = await RepairProcessRunner.RunWithProgressAsync(
                sfcSpec,
                line =>
                {
                    if (IsProgressLine(line))
                        entries.Add(CreateLog(line.Trim(), ScanLogStatus.InProgress));
                },
                cancellationToken);

            entries.Add(sfcResult.Success
                ? CreateLog($"{sfcSpec.Description} abgeschlossen.", ScanLogStatus.Success)
                : CreateLog($"{sfcSpec.Description} fehlgeschlagen (Code {sfcResult.ExitCode}).", ScanLogStatus.Warning));

            cancellationToken.ThrowIfCancellationRequested();

            var dismSpec = RepairCommandBuilder.BuildDismRestoreHealth();
            entries.Add(CreateLog($"{dismSpec.Description} wird ausgeführt...", ScanLogStatus.InProgress));

            var dismResult = await RepairProcessRunner.RunWithProgressAsync(
                dismSpec,
                line =>
                {
                    if (IsProgressLine(line))
                        entries.Add(CreateLog(line.Trim(), ScanLogStatus.InProgress));
                },
                cancellationToken);

            if (!dismResult.Success)
            {
                entries.Add(CreateLog(
                    $"{dismSpec.Description} fehlgeschlagen (Code {dismResult.ExitCode}).",
                    ScanLogStatus.Error));
                return entries;
            }

            entries.Add(CreateLog("SFC/DISM-Systemreparatur erfolgreich abgeschlossen.", ScanLogStatus.Success));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Repair {Kind} failed.", Kind);
            entries.Add(CreateLog("SFC/DISM-Systemreparatur fehlgeschlagen.", ScanLogStatus.Error));
        }

        return entries;
    }

    private static bool IsProgressLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        return line.Contains('%', StringComparison.Ordinal)
               || line.Contains("progress", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Fortschritt", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Verification", StringComparison.OrdinalIgnoreCase)
               || line.Contains("Prüfung", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class RegistryRepairAction : IRepairAction
{
    private readonly ILogger<RegistryRepairAction> _logger;
    private readonly IRollbackStore _rollbackStore;
    private readonly IRegistryReader _registryReader;
    private IReadOnlyList<RegistryIssue> _lastIssues = [];

    public RegistryRepairAction(
        ILogger<RegistryRepairAction> logger,
        IRollbackStore rollbackStore,
        IRegistryReader? registryReader = null)
    {
        _logger = logger;
        _rollbackStore = rollbackStore;
        _registryReader = registryReader ?? new WindowsRegistryReader();
    }

    public ProblemKind Kind => ProblemKind.Registry;

    public bool RequiresAdmin => false;

    public void SetIssues(IReadOnlyList<RegistryIssue> issues) => _lastIssues = issues;

    public RepairCommandSpec BuildCommand() =>
        new("reg", "export (intern)", "Defekte Registry-Einträge entfernen");

    public async Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog("Registry-Reparatur wird ausgeführt...", ScanLogStatus.InProgress),
        };

        if (!OperatingSystem.IsWindows())
        {
            entries.Add(CreateLog("Nur unter Windows verfügbar.", ScanLogStatus.Warning));
            return entries;
        }

        var repairable = _lastIssues.Where(RegistryPatternAnalyzer.IsReversibleIssue).ToList();
        if (repairable.Count == 0)
        {
            repairable = DiscoverRepairableIssues();
        }

        if (repairable.Count == 0)
        {
            entries.Add(CreateLog("Keine reparierbaren Registry-Einträge gefunden.", ScanLogStatus.Warning));
            return entries;
        }

        var snapshotDir = _rollbackStore.CreateSnapshotDirectory(ProblemKind.Registry, "Registry-Einträge entfernt");
        var manifestItems = new List<RollbackManifestItem>();
        var removed = 0;

        foreach (var issue in repairable)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryParseKeyPath(issue.KeyPath, out var hive, out var subKey))
                continue;

            var values = _registryReader.ReadValues(hive, subKey)
                .FirstOrDefault(v => v.Name.Equals(issue.ValueName, StringComparison.OrdinalIgnoreCase));

            if (values.Name is null)
                continue;

            if (_rollbackStore is RollbackStore concreteStore)
            {
                concreteStore.SaveRegistryValueSnapshot(
                    snapshotDir, hive, subKey, issue.ValueName, values.Data ?? "",
                    new RollbackManifest
                    {
                        Id = Path.GetFileName(snapshotDir),
                        RepairKind = ProblemKind.Registry,
                        Description = "Registry-Einträge entfernt",
                        Timestamp = DateTime.UtcNow,
                        Items = manifestItems,
                    },
                    manifestItems);
            }

            if (TryDeleteRegistryValue(hive, subKey, issue.ValueName))
            {
                removed++;
                entries.Add(CreateLog($"Entfernt: {issue.ValueName} ({issue.KeyPath})", ScanLogStatus.Success));
            }
        }

        _rollbackStore.SaveManifest(new RollbackManifest
        {
            Id = Path.GetFileName(snapshotDir),
            RepairKind = ProblemKind.Registry,
            Description = $"{removed} Registry-Einträge entfernt",
            Timestamp = DateTime.UtcNow,
            Items = manifestItems,
        });

        entries.Add(CreateLog($"Registry-Reparatur abgeschlossen: {removed} Einträge entfernt (Backup erstellt).", ScanLogStatus.Success));
        await Task.CompletedTask;
        return entries;
    }

    private List<RegistryIssue> DiscoverRepairableIssues()
    {
        var check = new RegistryProblemCheck(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RegistryProblemCheck>.Instance,
            _registryReader);
        var result = check.Check();

        var issues = new List<RegistryIssue>();
        foreach (var item in result.Items.Where(i => i.IsRepairable))
        {
            var parts = item.Id.Split('|', 2);
            if (parts.Length != 2)
                continue;

            issues.Add(new RegistryIssue(
                RegistryIssueKind.MissingExecutable,
                parts[0],
                parts[1],
                "",
                item.Description));
        }

        return issues;
    }

    private static bool TryParseKeyPath(string keyPath, out string hive, out string subKey)
    {
        hive = "";
        subKey = "";

        var separator = keyPath.IndexOf('\\');
        if (separator <= 0)
            return false;

        hive = keyPath[..separator];
        subKey = keyPath[(separator + 1)..];
        return true;
    }

    private static bool TryDeleteRegistryValue(string hive, string subKey, string valueName)
    {
        try
        {
            var root = hive.Equals("HKLM", StringComparison.OrdinalIgnoreCase)
                ? Registry.LocalMachine
                : Registry.CurrentUser;

            using var key = root.OpenSubKey(subKey, writable: true);
            key?.DeleteValue(valueName, throwOnMissingValue: false);
            return true;
        }
        catch
        {
            return false;
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

public sealed class SearchIndexResetRepair : RepairActionBase
{
    private readonly IRollbackStore _rollbackStore;

    public SearchIndexResetRepair(
        ILogger<SearchIndexResetRepair> logger,
        IRollbackStore rollbackStore) : base(logger)
    {
        _rollbackStore = rollbackStore;
    }

    public override ProblemKind Kind => ProblemKind.SearchIndexReset;

    public override RepairCommandSpec BuildCommand() => RepairCommandBuilder.BuildStopWindowsSearchService();

    public override async Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
        bool isRunningAsAdmin,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog("Windows-Suchdienst wird zurückgesetzt...", ScanLogStatus.InProgress),
        };

        if (!isRunningAsAdmin)
        {
            entries.Add(CreateLog(
                "Administratorrechte erforderlich — bitte HorosHelper mit UAC starten.",
                ScanLogStatus.Warning));
            return entries;
        }

        if (!OperatingSystem.IsWindows())
        {
            entries.Add(CreateLog("Nur unter Windows verfügbar.", ScanLogStatus.Warning));
            return entries;
        }

        try
        {
            var snapshotDir = _rollbackStore.CreateSnapshotDirectory(
                ProblemKind.SearchIndexReset,
                "Windows-Suchindex zurückgesetzt");

            var serviceStatePath = Path.Combine(snapshotDir, "service-state.txt");
            await File.WriteAllTextAsync(serviceStatePath, "WSearch", cancellationToken);

            _rollbackStore.SaveManifest(new RollbackManifest
            {
                Id = Path.GetFileName(snapshotDir),
                RepairKind = ProblemKind.SearchIndexReset,
                Description = "Windows-Suchindex zurückgesetzt",
                Timestamp = DateTime.UtcNow,
                Items =
                [
                    new RollbackManifestItem
                    {
                        Kind = RollbackEntryKind.ServiceState,
                        RelativePath = "service-state.txt",
                        Metadata = "WSearch",
                    },
                ],
            });

            var stopSpec = RepairCommandBuilder.BuildStopWindowsSearchService();
            entries.Add(CreateLog(stopSpec.Description + "...", ScanLogStatus.InProgress));
            var stopResult = await RepairProcessRunner.RunAsync(stopSpec, cancellationToken);
            if (!stopResult.Success)
            {
                entries.Add(CreateLog(
                    $"{stopSpec.Description} fehlgeschlagen (Code {stopResult.ExitCode}).",
                    ScanLogStatus.Warning));
            }
            else
            {
                entries.Add(CreateLog($"{stopSpec.Description} abgeschlossen.", ScanLogStatus.Success));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var indexPath = RepairCommandBuilder.GetWindowsSearchDataPath();
            if (Directory.Exists(indexPath))
            {
                entries.Add(CreateLog($"Suchindex-Verzeichnis wird bereinigt: {indexPath}", ScanLogStatus.InProgress));
                try
                {
                    foreach (var entry in Directory.EnumerateFileSystemEntries(indexPath))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            if (Directory.Exists(entry))
                                Directory.Delete(entry, recursive: true);
                            else
                                File.Delete(entry);
                        }
                        catch
                        {
                            // skip locked files
                        }
                    }

                    entries.Add(CreateLog("Suchindex-Verzeichnis bereinigt.", ScanLogStatus.Success));
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed clearing search index path.");
                    entries.Add(CreateLog("Suchindex-Verzeichnis teilweise bereinigt.", ScanLogStatus.Warning));
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            var startSpec = RepairCommandBuilder.BuildStartWindowsSearchService();
            entries.Add(CreateLog(startSpec.Description + "...", ScanLogStatus.InProgress));
            var startResult = await RepairProcessRunner.RunAsync(startSpec, cancellationToken);
            if (!startResult.Success)
            {
                entries.Add(CreateLog(
                    $"{startSpec.Description} fehlgeschlagen (Code {startResult.ExitCode}).",
                    ScanLogStatus.Error));
                return entries;
            }

            entries.Add(CreateLog("Windows-Suchdienst erfolgreich zurückgesetzt.", ScanLogStatus.Success));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Repair {Kind} failed.", Kind);
            entries.Add(CreateLog("Windows-Suchdienst konnte nicht zurückgesetzt werden.", ScanLogStatus.Error));
        }

        return entries;
    }
}

internal static class RepairProcessRunner
{
    internal sealed record ProcessRunResult(int ExitCode, string Output, string Error, bool Success);

    public static async Task<ProcessRunResult> RunAsync(
        RepairCommandSpec spec,
        CancellationToken cancellationToken) =>
        await RunWithProgressAsync(spec, _ => { }, cancellationToken);

    public static async Task<ProcessRunResult> RunWithProgressAsync(
        RepairCommandSpec spec,
        Action<string> onLine,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = spec.FileName,
            Arguments = spec.Arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return new ProcessRunResult(-1, "", "Process start failed", false);

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        var outputTask = PumpLinesAsync(process.StandardOutput, outputBuilder, onLine, cancellationToken);
        var errorTask = PumpLinesAsync(process.StandardError, errorBuilder, onLine, cancellationToken);

        await Task.WhenAll(outputTask, errorTask);
        await process.WaitForExitAsync(cancellationToken);

        return new ProcessRunResult(
            process.ExitCode,
            outputBuilder.ToString(),
            errorBuilder.ToString(),
            process.ExitCode == 0);
    }

    private static async Task PumpLinesAsync(
        StreamReader reader,
        StringBuilder collector,
        Action<string> onLine,
        CancellationToken cancellationToken)
    {
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                break;

            collector.AppendLine(line);
            onLine(line);
        }
    }
}
