using HorosHelp.Core.Models.Storage;

namespace HorosHelp.Core.Services.Storage;

public static class SmartDiskMapper
{
    public static SmartHealthStatus MapWmiStatus(string? status, bool? predictFailure)
    {
        if (predictFailure == true)
            return SmartHealthStatus.Warning;

        if (string.IsNullOrWhiteSpace(status))
            return SmartHealthStatus.Unknown;

        var normalized = status.Trim();

        if (normalized.Equals("OK", StringComparison.OrdinalIgnoreCase))
            return SmartHealthStatus.Ok;

        if (normalized.Contains("Degraded", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("Error", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("Fail", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("Critical", StringComparison.OrdinalIgnoreCase))
            return SmartHealthStatus.Warning;

        return SmartHealthStatus.Unknown;
    }

    public static string MapStatusLabel(SmartHealthStatus status) =>
        status switch
        {
            SmartHealthStatus.Ok => "OK",
            SmartHealthStatus.Warning => "Warnung",
            _ => "Unbekannt",
        };
}
