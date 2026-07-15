using HorosHelp.Core.Models.Startup;

namespace HorosHelp.Core.Services.Startup;

public static class StartupImpactAnalyzer
{
    private static readonly HashSet<string> HighImpactNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "spotify", "discord", "onedrive", "dropbox", "steam", "epic", "teams", "slack", "zoom",
    };

    private static readonly HashSet<string> LowImpactNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "nvidia", "intel", "amd", "realtek", "windows", "microsoft", "defender", "security",
        "update", "driver", "audio", "synaptics", "logitech",
    };

    private static readonly HashSet<string> SafeToDisableNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "spotify", "discord", "steam", "epic", "teams", "slack", "zoom", "skype", "itunes",
        "adobe", "ccleaner", "utorrent", "qbittorrent",
    };

    public static StartupImpact ClassifyImpact(string name, string command)
    {
        var haystack = $"{name} {command}";

        if (ContainsAny(haystack, HighImpactNames) || ContainsAny(name, HighImpactNames))
            return StartupImpact.Hoch;

        if (ContainsAny(haystack, LowImpactNames) || ContainsAny(name, LowImpactNames))
            return StartupImpact.Niedrig;

        if (command.Contains("system32", StringComparison.OrdinalIgnoreCase)
            || command.Contains("windows", StringComparison.OrdinalIgnoreCase))
            return StartupImpact.Niedrig;

        return StartupImpact.Mittel;
    }

    public static bool IsSafeToDisable(string name, string command, StartupImpact impact)
    {
        if (impact == StartupImpact.Niedrig)
            return false;

        if (ContainsAny(name, SafeToDisableNames) || ContainsAny(command, SafeToDisableNames))
            return true;

        return impact == StartupImpact.Hoch;
    }

    public static int CountSafeToDisable(IEnumerable<StartupEntryInfo> entries) =>
        entries.Count(e => e.IsEnabled && e.IsSafeToDisable);

    private static bool ContainsAny(string value, IEnumerable<string> needles)
    {
        foreach (var needle in needles)
        {
            if (value.Contains(needle, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
