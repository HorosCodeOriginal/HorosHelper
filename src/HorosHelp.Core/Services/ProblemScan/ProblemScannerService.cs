using System.Collections.Concurrent;
using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Admin;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.ProblemScan;

public sealed class ProblemScannerService : IProblemScannerService
{
    private readonly ILogger<ProblemScannerService> _logger;
    private readonly ProblemScannerThresholds _thresholds;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IReadOnlyDictionary<ProblemKind, IRepairAction> _repairActions;
    private readonly IReadOnlyDictionary<ProblemKind, IProblemCheck> _problemChecks;
    private readonly IRollbackStore _rollbackStore;
    private readonly RegistryRepairAction _registryRepairAction;
    private IReadOnlyList<RegistryIssue> _lastRegistryIssues = [];

    public ProblemScannerService(
        ILogger<ProblemScannerService> logger,
        IAdminElevationService adminElevationService,
        IEnumerable<IRepairAction> repairActions,
        IEnumerable<IProblemCheck> problemChecks,
        IRollbackStore rollbackStore,
        RegistryRepairAction registryRepairAction,
        ProblemScannerThresholds? thresholds = null)
    {
        _logger = logger;
        _adminElevationService = adminElevationService;
        _thresholds = thresholds ?? ProblemScannerThresholds.Default;
        _repairActions = repairActions.ToDictionary(action => action.Kind);
        _problemChecks = problemChecks.ToDictionary(check => check.Kind);
        _rollbackStore = rollbackStore;
        _registryRepairAction = registryRepairAction;
    }

    public async Task<ScanResult> ScanAsync(
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var logEntries = new List<ScanLogEntry>();
        var problems = new List<ProblemCard>();
        var usedMock = false;

        void Report(double percent, string status, string sub, ScanLogEntry? entry = null)
        {
            if (entry is not null)
                logEntries.Add(entry);

            progress?.Report(new ScanProgress
            {
                Percent = percent,
                StatusText = status,
                SubText = sub,
                LogEntry = entry,
            });
        }

        Report(0, "System wird gescannt...", "Bitte warten Sie einen Moment.",
            CreateLog("Scan gestartet", ScanLogStatus.InProgress));

        await Task.Delay(200, cancellationToken);

        Report(15, "System wird gescannt...", "Systeminformationen werden geladen.",
            CreateLog("Systeminformationen werden geladen", ScanLogStatus.Success));

        await Task.Delay(150, cancellationToken);

        Report(30, "Registry wird überprüft...", "Bekannte Muster werden geprüft.",
            CreateLog("Registry wird überprüft", ScanLogStatus.InProgress));

        var registryResult = RunCheck(ProblemKind.Registry);
        usedMock |= registryResult.UsedMockData;
        problems.Add(registryResult.Card);
        _lastRegistryIssues = ExtractRegistryIssues(registryResult);

        Report(40, "Registry wird überprüft...", registryResult.Card.Subtitle,
            CreateLog(registryResult.Card.Title, registryResult.Card.Severity == ProblemSeverity.Good
                ? ScanLogStatus.Success
                : ScanLogStatus.Warning));

        await Task.Delay(200, cancellationToken);

        Report(55, "Temp-Verzeichnisse werden gescannt...", "Benutzer-Temp wird analysiert.",
            CreateLog("Temp-Verzeichnisse werden gescannt", ScanLogStatus.InProgress));

        var tempCard = CheckTempFiles(out var tempMock);
        usedMock |= tempMock;
        problems.Add(tempCard);

        var tempLogStatus = tempCard.Severity switch
        {
            ProblemSeverity.Good => ScanLogStatus.Success,
            ProblemSeverity.Warning => ScanLogStatus.Warning,
            _ => ScanLogStatus.Error,
        };
        Report(70, "Temp-Verzeichnisse werden gescannt...", tempCard.Subtitle,
            CreateLog(tempCard.Subtitle, tempLogStatus));

        await Task.Delay(200, cancellationToken);

        Report(85, "Startup-Programme werden überprüft...", "Autostart-Einträge werden gezählt.",
            CreateLog("Startup-Programme werden überprüft", ScanLogStatus.InProgress));

        var startupCard = CheckStartupPrograms(out var startupMock);
        usedMock |= startupMock;
        problems.Add(startupCard);

        var startupLogStatus = startupCard.Severity switch
        {
            ProblemSeverity.Good => ScanLogStatus.Success,
            ProblemSeverity.Warning => ScanLogStatus.Warning,
            _ => ScanLogStatus.Error,
        };
        Report(95, "Startup-Programme werden überprüft...", startupCard.Subtitle,
            CreateLog(startupCard.Subtitle, startupLogStatus));

        await Task.Delay(150, cancellationToken);

        Report(97, "Netzwerk-Wartung...", "Optionale Reparaturen verfügbar.",
            CreateLog("Optionale Netzwerk-Reparaturen geladen", ScanLogStatus.Success));

        problems.Add(BuildOptionalRepairCard(
            ProblemKind.DnsFlush,
            "DNS-Cache leeren",
            "Optional — hilft bei DNS- und Verbindungsproblemen."));

        problems.Add(BuildOptionalRepairCard(
            ProblemKind.WinsockReset,
            "Winsock zurücksetzen",
            "Optional — Neustart erforderlich. Bei hartnäckigen Netzwerkfehlern."));

        problems.Add(BuildOptionalRepairCard(
            ProblemKind.WindowsUpdateCache,
            "Windows-Update-Cache leeren",
            "Optional — behebt häufige Update-Fehler. Administratorrechte erforderlich."));

        problems.Add(BuildOptionalRepairCard(
            ProblemKind.SystemFileCheck,
            "Systemdateien prüfen (SFC/DISM)",
            "Optional — kann 15–45 Minuten dauern. Administratorrechte erforderlich."));

        problems.Add(BuildOptionalRepairCard(
            ProblemKind.SearchIndexReset,
            "Windows-Suchdienst zurücksetzen",
            "Optional — behebt Suchprobleme. Administratorrechte erforderlich."));

        await Task.Delay(100, cancellationToken);

        Report(100, "Scan abgeschlossen", "Gefundene Probleme können repariert werden.",
            CreateLog("Scan abgeschlossen", ScanLogStatus.Success));

        var result = new ScanResult
        {
            Problems = problems,
            LogEntries = logEntries,
            IsComplete = true,
            UsedMockData = usedMock,
        };

        _logger.LogInformation(
            "Problem scan completed: {ProblemCount} findings, mock={UsedMock}",
            problems.Count(p => p.Severity != ProblemSeverity.Good),
            usedMock);

        return result;
    }

    public async Task<IReadOnlyList<ScanLogEntry>> RepairAsync(
        ProblemKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog("Reparatur gestartet", ScanLogStatus.InProgress),
        };

        var targets = kind.HasValue
            ? new[] { kind.Value }
            : new[]
            {
                ProblemKind.TempFiles,
                ProblemKind.Registry,
                ProblemKind.StartupPrograms,
                ProblemKind.DnsFlush,
                ProblemKind.WinsockReset,
                ProblemKind.WindowsUpdateCache,
                ProblemKind.SystemFileCheck,
                ProblemKind.SearchIndexReset,
            };

        foreach (var target in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (target == ProblemKind.Registry)
                _registryRepairAction.SetIssues(_lastRegistryIssues);

            if (_repairActions.TryGetValue(target, out var repairAction))
            {
                var repairEntries = await repairAction.ExecuteAsync(
                    _adminElevationService.IsRunningAsAdmin,
                    cancellationToken);
                entries.AddRange(repairEntries);
                continue;
            }

            switch (target)
            {
                case ProblemKind.TempFiles:
                    entries.AddRange(await RepairTempFilesAsync(cancellationToken));
                    break;
                case ProblemKind.Registry:
                    entries.Add(CreateLog("Registry-Reparatur nicht verfügbar.", ScanLogStatus.Warning));
                    await Task.Delay(300, cancellationToken);
                    break;
                case ProblemKind.StartupPrograms:
                    entries.Add(CreateLog("Startup-Hinweise aktualisiert — Verwaltung unter Startup.", ScanLogStatus.Warning));
                    await Task.Delay(200, cancellationToken);
                    break;
            }
        }

        entries.Add(CreateLog("Reparatur abgeschlossen", ScanLogStatus.Success));
        _logger.LogInformation("Repair completed for kinds: {Kinds}", string.Join(", ", targets));

        return entries;
    }

    public IReadOnlyList<RollbackEntry> GetRecentRollbacks(int maxCount = 5) =>
        _rollbackStore.GetRecentEntries(maxCount);

    public Task<IReadOnlyList<ScanLogEntry>> RollbackAsync(
        string rollbackId,
        CancellationToken cancellationToken = default) =>
        _rollbackStore.RollbackAsync(rollbackId, cancellationToken);

    private ProblemCheckResult RunCheck(ProblemKind kind) =>
        _problemChecks.TryGetValue(kind, out var check)
            ? check.Check()
            : new ProblemCheckResult
            {
                Card = new ProblemCard
                {
                    Kind = kind,
                    Severity = ProblemSeverity.Good,
                    Title = kind.ToString(),
                    Subtitle = "Kein Check registriert.",
                },
            };

    private static IReadOnlyList<RegistryIssue> ExtractRegistryIssues(ProblemCheckResult result)
    {
        return result.Items
            .Where(i => i.IsRepairable)
            .Select(i =>
            {
                var parts = i.Id.Split('|', 2);
                return new RegistryIssue(
                    RegistryIssueKind.MissingExecutable,
                    parts.Length == 2 ? parts[0] : "",
                    parts.Length == 2 ? parts[1] : i.Title,
                    "",
                    i.Description);
            })
            .ToList();
    }

    private ProblemCard CheckTempFiles(out bool usedMock)
    {
        usedMock = false;

        try
        {
            var tempPath = Path.GetTempPath();
            if (!Directory.Exists(tempPath))
            {
                usedMock = true;
                return BuildTempCard(ProblemSeverity.Good, 0, "Temp-Verzeichnis nicht verfügbar.");
            }

            var sizeBytes = GetDirectorySizeSafe(tempPath);
            var sizeGb = sizeBytes / 1024d / 1024d / 1024d;

            var severity = ClassifyTempSeverity(sizeGb);
            var subtitle = severity == ProblemSeverity.Good
                ? "Keine übermäßigen temporären Dateien."
                : $"{FormatGb(sizeGb)} temporäre Dateien gefunden.";

            return BuildTempCard(severity, sizeGb, subtitle);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Temp scan failed; using mock values.");
            usedMock = true;
            return BuildTempCard(ProblemSeverity.Warning, 2.4, "2,4 GB temporäre Dateien gefunden (Mock).");
        }
    }

    private ProblemCard CheckStartupPrograms(out bool usedMock)
    {
        usedMock = false;

        try
        {
            if (!OperatingSystem.IsWindows())
            {
                usedMock = true;
                return BuildStartupCard(ProblemSeverity.Critical, 14, "14 Programme verlangsamen den Start (Mock).");
            }

            var count = CountStartupEntries();

            var severity = ClassifyStartupSeverity(count);
            var subtitle = severity == ProblemSeverity.Good
                ? "Autostart im normalen Bereich."
                : $"{count} Programme verlangsamen den Start.";

            return BuildStartupCard(severity, count, subtitle);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Startup scan failed; using mock count.");
            usedMock = true;
            return BuildStartupCard(ProblemSeverity.Critical, 14, "14 Programme verlangsamen den Start (Mock).");
        }
    }

    private static int CountStartupEntries()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void CollectFromKey(Microsoft.Win32.RegistryKey baseKey, string subKeyPath)
        {
            try
            {
                using var key = baseKey.OpenSubKey(subKeyPath);
                if (key is null)
                    return;

                foreach (var name in key.GetValueNames())
                {
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }
            }
            catch
            {
                // HKCU-only — no admin required; ignore inaccessible keys
            }
        }

        CollectFromKey(Microsoft.Win32.Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run");
        CollectFromKey(Microsoft.Win32.Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run");

        try
        {
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(startupFolder))
            {
                foreach (var file in Directory.EnumerateFileSystemEntries(startupFolder))
                    names.Add(Path.GetFileName(file));
            }
        }
        catch
        {
            // ignore
        }

        return names.Count;
    }

    private async Task<IReadOnlyList<ScanLogEntry>> RepairTempFilesAsync(CancellationToken cancellationToken)
    {
        var entries = new List<ScanLogEntry>
        {
            CreateLog("Temporäre Dateien werden bereinigt...", ScanLogStatus.InProgress),
        };

        var snapshotDir = _rollbackStore.CreateSnapshotDirectory(ProblemKind.TempFiles, "Temp-Dateien bereinigt");
        var deletedFiles = new List<string>();

        try
        {
            var tempPath = Path.GetTempPath();
            if (!Directory.Exists(tempPath))
            {
                entries.Add(CreateLog("Temp-Verzeichnis nicht gefunden", ScanLogStatus.Warning));
                return entries;
            }

            var deleted = 0L;
            var errors = 0;

            foreach (var file in Directory.EnumerateFiles(tempPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var info = new FileInfo(file);
                    if (info.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-7))
                    {
                        deletedFiles.Add(file);
                        info.Delete();
                        deleted++;
                    }
                }
                catch
                {
                    errors++;
                }

                if (deleted % 50 == 0 && deleted > 0)
                    await Task.Delay(10, cancellationToken);
            }

            var listPath = Path.Combine(snapshotDir, "deleted-files.txt");
            await File.WriteAllLinesAsync(listPath, deletedFiles, cancellationToken);
            _rollbackStore.SaveManifest(new RollbackManifest
            {
                Id = Path.GetFileName(snapshotDir),
                RepairKind = ProblemKind.TempFiles,
                Description = $"Temp-Bereinigung: {deleted} Dateien",
                Timestamp = DateTime.UtcNow,
                Items =
                [
                    new RollbackManifestItem
                    {
                        Kind = RollbackEntryKind.FileList,
                        RelativePath = "deleted-files.txt",
                        Metadata = $"{deleted} Dateien",
                    },
                ],
            });

            entries.Add(CreateLog(
                $"Temp-Bereinigung abgeschlossen: {deleted} Dateien entfernt" +
                (errors > 0 ? $", {errors} übersprungen (in Verwendung)" : "") +
                " (Backup-Liste erstellt).",
                ScanLogStatus.Success));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Temp repair failed.");
            entries.Add(CreateLog("Temp-Bereinigung teilweise fehlgeschlagen", ScanLogStatus.Warning));
        }

        return entries;
    }

    private ProblemSeverity ClassifyTempSeverity(double sizeGb)
    {
        if (sizeGb >= _thresholds.TempCriticalGb)
            return ProblemSeverity.Critical;

        if (sizeGb >= _thresholds.TempWarningGb)
            return ProblemSeverity.Warning;

        return ProblemSeverity.Good;
    }

    private ProblemSeverity ClassifyStartupSeverity(int count)
    {
        if (count >= _thresholds.StartupCriticalCount)
            return ProblemSeverity.Critical;

        if (count >= _thresholds.StartupWarningCount)
            return ProblemSeverity.Warning;

        return ProblemSeverity.Good;
    }

    private static ProblemCard BuildTempCard(ProblemSeverity severity, double sizeGb, string subtitle) =>
        new()
        {
            Kind = ProblemKind.TempFiles,
            Severity = severity,
            Title = "Temp-Dateien",
            Subtitle = subtitle,
            ProgressValue = Math.Clamp(sizeGb / 3.0, 0, 1),
            IsRepairable = severity != ProblemSeverity.Good,
        };

    private static ProblemCard BuildStartupCard(ProblemSeverity severity, int count, string subtitle) =>
        new()
        {
            Kind = ProblemKind.StartupPrograms,
            Severity = severity,
            Title = "Startup-Programme",
            Subtitle = subtitle,
            ProgressValue = SeverityToProgress(severity),
            IsRepairable = false,
        };

    private static ProblemCard BuildOptionalRepairCard(ProblemKind kind, string title, string subtitle) =>
        new()
        {
            Kind = kind,
            Severity = ProblemSeverity.Good,
            Title = title,
            Subtitle = subtitle,
            ProgressValue = 0,
            IsRepairable = true,
        };

    private static double SeverityToProgress(ProblemSeverity severity) =>
        severity switch
        {
            ProblemSeverity.Critical => 1.0,
            ProblemSeverity.Warning => 0.72,
            _ => 0,
        };

    private static long GetDirectorySizeSafe(string path)
    {
        long total = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
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
        }
        catch
        {
            return 0;
        }

        return total;
    }

    private static string FormatGb(double gb) =>
        gb >= 10 ? $"{gb:F0} GB" : $"{gb:F1} GB".Replace('.', ',');

    private static ScanLogEntry CreateLog(string message, ScanLogStatus status) =>
        new()
        {
            Timestamp = DateTime.Now,
            Message = message,
            Status = status,
        };
}
