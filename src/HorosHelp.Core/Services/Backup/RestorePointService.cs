using System.Text.Json;
using System.Text.RegularExpressions;
using HorosHelp.Core.Interop;
using HorosHelp.Core.Models.Backup;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Security;
using HorosHelp.Core.Services.Windows;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Backup;

public interface IRestorePointService
{
    IReadOnlyList<RestorePointInfo> GetRestorePoints();
    Task<BackupOperationResult> CreateRestorePointAsync(CancellationToken cancellationToken = default);
    string LastCreationMethod { get; }
}

public sealed partial class RestorePointService : IRestorePointService
{
    private readonly IPowerShellQuery _powerShellQuery;
    private readonly IAdminElevationService _adminElevationService;
    private readonly ILogger<RestorePointService> _logger;

    public string LastCreationMethod { get; private set; } = "";

    public RestorePointService(
        IPowerShellQuery powerShellQuery,
        IAdminElevationService adminElevationService,
        ILogger<RestorePointService> logger)
    {
        _powerShellQuery = powerShellQuery;
        _adminElevationService = adminElevationService;
        _logger = logger;
    }

    public IReadOnlyList<RestorePointInfo> GetRestorePoints()
    {
        try
        {
            var output = _powerShellQuery.Execute(
                "Get-ComputerRestorePoint | Select-Object SequenceNumber,CreationTime,RestorePointType,Description | ConvertTo-Json -Compress");

            if (string.IsNullOrWhiteSpace(output))
                return [];

            return ParseRestorePoints(output);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Restore point read failed.");
            return [];
        }
    }

    public async Task<BackupOperationResult> CreateRestorePointAsync(CancellationToken cancellationToken = default)
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

        if (!InputSecurityValidator.IsValidPowerShellLiteral(description, out _))
            return Fail("Ungültige Beschreibung.");

        if (SystemRestoreInterop.TryCreateRestorePoint(description, out var pInvokeError))
        {
            LastCreationMethod = "SRSetRestorePoint (P/Invoke srclient.dll)";
            _logger.LogInformation("Restore point created via {Method}", LastCreationMethod);
            return new BackupOperationResult
            {
                Success = true,
                Message = "Wiederherstellungspunkt wurde erstellt (SRSetRestorePoint).",
            };
        }

        _logger.LogDebug("SRSetRestorePoint failed ({Error}); falling back to Checkpoint-Computer.", pInvokeError);
        return await CreateViaPowerShellAsync(description, cancellationToken);
    }

    private async Task<BackupOperationResult> CreateViaPowerShellAsync(string description, CancellationToken cancellationToken)
    {
        if (!InputSecurityValidator.IsValidPowerShellLiteral(description, out var validationError))
            return Fail(validationError);

        var script = $"Checkpoint-Computer -Description '{description}' -RestorePointType MODIFY_SETTINGS";

        try
        {
            var output = await Task.Run(() => _powerShellQuery.Execute(script), cancellationToken);

            if (output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                output.Contains("Checkpoint-Computer", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Checkpoint-Computer failed: {Output}", output);
                return Fail("Wiederherstellungspunkt konnte nicht erstellt werden.");
            }

            LastCreationMethod = "Checkpoint-Computer (PowerShell-Fallback)";
            _logger.LogInformation("Restore point created via {Method}", LastCreationMethod);
            return new BackupOperationResult
            {
                Success = true,
                Message = "Wiederherstellungspunkt wurde erstellt (Checkpoint-Computer).",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Checkpoint-Computer failed.");
            return Fail("Wiederherstellungspunkt fehlgeschlagen.");
        }
    }

    private static List<RestorePointInfo> ParseRestorePoints(string output)
    {
        var points = new List<RestorePointInfo>();

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
            points.Add(ParseSingleRestorePoint(output));

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

    private static BackupOperationResult Fail(string message) =>
        new() { Success = false, Message = message };

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
