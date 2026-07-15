using System.Diagnostics;
using HorosHelp.Core.Models.ProblemScan;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.ProblemScan;

public abstract class RepairActionBase : IRepairAction
{
    protected RepairActionBase(ILogger logger) => Logger = logger;

    protected ILogger Logger { get; }

    public abstract ProblemKind Kind { get; }

    public virtual bool RequiresAdmin => true;

    public abstract RepairCommandSpec BuildCommand();

    public async Task<IReadOnlyList<ScanLogEntry>> ExecuteAsync(
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
