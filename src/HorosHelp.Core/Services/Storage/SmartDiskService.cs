using System.Management;
using HorosHelp.Core.Models.Storage;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Storage;

public sealed class SmartDiskService : ISmartDiskService
{
    private readonly ILogger<SmartDiskService> _logger;

    public SmartDiskService(ILogger<SmartDiskService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<DriveSmartInfo> GetDriveHealth()
    {
        if (!OperatingSystem.IsWindows())
            return BuildMockHealth();

        try
        {
            return QueryWmiDiskHealth();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "S.M.A.R.T. WMI query failed; returning unknown status.");
            return BuildUnknownForReadyDrives();
        }
    }

    internal static SmartHealthStatus MapWmiStatus(string? status, bool? predictFailure) =>
        SmartDiskMapper.MapWmiStatus(status, predictFailure);

    internal static string MapStatusLabel(SmartHealthStatus status) =>
        SmartDiskMapper.MapStatusLabel(status);

    private List<DriveSmartInfo> QueryWmiDiskHealth()
    {
        var results = new List<DriveSmartInfo>();
        var letterByModel = MapDriveLetters();

        using var searcher = new ManagementObjectSearcher(
            "SELECT Model, Status, PredictFailure, DeviceID FROM Win32_DiskDrive");

        foreach (var obj in searcher.Get().Cast<ManagementObject>())
        {
            using (obj)
            {
                var model = obj["Model"]?.ToString()?.Trim() ?? "Unbekanntes Laufwerk";
                var status = obj["Status"]?.ToString();
                var predictFailure = obj["PredictFailure"] as bool?;
                var deviceId = obj["DeviceID"]?.ToString() ?? "";

                var health = MapWmiStatus(status, predictFailure);
                var letter = letterByModel.TryGetValue(NormalizeDeviceId(deviceId), out var mapped)
                    ? mapped
                    : GuessLetterFromPartitions(deviceId);

                results.Add(new DriveSmartInfo
                {
                    DriveLetter = letter,
                    Model = model,
                    Status = health,
                    StatusLabel = MapStatusLabel(health),
                    DetailText = BuildDetailText(status, predictFailure),
                });
            }
        }

        if (results.Count == 0)
            return BuildUnknownForReadyDrives();

        return results;
    }

    private static Dictionary<string, string> MapDriveLetters()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Antecedent, Dependent FROM Win32_LogicalDiskToPartition");

            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                using (obj)
                {
                    var antecedent = obj["Antecedent"]?.ToString() ?? "";
                    var dependent = obj["Dependent"]?.ToString() ?? "";
                    var diskId = ExtractQuotedValue(antecedent);
                    var letter = ExtractQuotedValue(dependent).TrimEnd(':');

                    if (!string.IsNullOrWhiteSpace(diskId) && !string.IsNullOrWhiteSpace(letter))
                        map.TryAdd(NormalizeDeviceId(diskId), letter + ":");
                }
            }
        }
        catch
        {
            // best effort
        }

        return map;
    }

    private static string GuessLetterFromPartitions(string deviceId)
    {
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            if (drive.DriveType == DriveType.Fixed)
                return drive.Name.TrimEnd('\\');
        }

        return deviceId;
    }

    private static string BuildDetailText(string? status, bool? predictFailure)
    {
        if (predictFailure == true)
            return "WMI meldet möglichen Ausfall (PredictFailure).";

        if (!string.IsNullOrWhiteSpace(status))
            return $"WMI-Status: {status}";

        return "Keine S.M.A.R.T.-Details verfügbar.";
    }

    private static string NormalizeDeviceId(string deviceId) =>
        deviceId.Replace(@"\\", @"\", StringComparison.Ordinal);

    private static string ExtractQuotedValue(string wmiPath)
    {
        var start = wmiPath.LastIndexOf('"');
        var end = wmiPath.LastIndexOf('"', start - 1);
        if (start > end && end >= 0)
            return wmiPath[(end + 1)..start];

        return "";
    }

    private static List<DriveSmartInfo> BuildUnknownForReadyDrives() =>
        DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .Select(d => new DriveSmartInfo
            {
                DriveLetter = d.Name.TrimEnd('\\'),
                Model = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "Lokales Laufwerk" : d.VolumeLabel,
                Status = SmartHealthStatus.Unknown,
                StatusLabel = MapStatusLabel(SmartHealthStatus.Unknown),
                DetailText = "S.M.A.R.T.-Daten nicht verfügbar.",
            })
            .ToList();

    private static List<DriveSmartInfo> BuildMockHealth() =>
    [
        new()
        {
            DriveLetter = "C:",
            Model = "Mock SSD",
            Status = SmartHealthStatus.Ok,
            StatusLabel = MapStatusLabel(SmartHealthStatus.Ok),
            DetailText = "Mock-Daten (nicht Windows).",
        },
    ];
}
