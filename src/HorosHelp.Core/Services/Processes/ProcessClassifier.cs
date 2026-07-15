using HorosHelp.Core.Models.Processes;

namespace HorosHelp.Core.Services.Processes;

public static class ProcessClassifier
{
    private static readonly HashSet<string> SystemProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "system", "idle", "registry", "smss", "csrss", "wininit", "services", "lsass",
        "svchost", "dwm", "winlogon", "explorer", "fontdrvhost", "sihost", "taskhostw",
        "runtimebroker", "searchindexer", "securityhealthservice", "msmpeng",
    };

    private static readonly HashSet<string> SafeBackgroundNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "spotify", "discord", "steam", "teams", "slack", "zoom", "notepad", "code",
        "chrome", "msedge", "firefox", "opera", "brave",
    };

    public static ProcessSafetyLevel Classify(string processName)
    {
        var normalized = NormalizeName(processName);

        if (SystemProcessNames.Contains(normalized))
            return ProcessSafetyLevel.System;

        if (SafeBackgroundNames.Contains(normalized))
            return ProcessSafetyLevel.Safe;

        return ProcessSafetyLevel.Unknown;
    }

    public static string GetSafetyLabel(ProcessSafetyLevel level) => level switch
    {
        ProcessSafetyLevel.System => "System",
        ProcessSafetyLevel.Safe => "Sicher",
        _ => "Unbekannt",
    };

    public static bool CanTerminate(ProcessSafetyLevel level) =>
        level is ProcessSafetyLevel.Safe or ProcessSafetyLevel.Unknown;

    private static string NormalizeName(string processName)
    {
        var name = processName.Trim();
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        return name;
    }
}
