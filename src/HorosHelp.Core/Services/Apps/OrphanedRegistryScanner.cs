namespace HorosHelp.Core.Services.Apps;

/// <summary>
/// Detects uninstall registry keys whose InstallLocation no longer exists on disk.
/// </summary>
public static class OrphanedRegistryScanner
{
    public static IReadOnlyList<Models.Apps.OrphanedRegistryEntry> Scan(
        IEnumerable<RegistryUninstallEntry> entries,
        Func<string, bool> directoryExists)
    {
        var orphans = new List<Models.Apps.OrphanedRegistryEntry>();

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.InstallLocation))
                continue;

            var location = entry.InstallLocation.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(location))
                continue;

            if (directoryExists(location))
                continue;

            orphans.Add(new Models.Apps.OrphanedRegistryEntry
            {
                RegistryPath = entry.RegistryPath,
                DisplayName = entry.DisplayName,
                InstallLocation = location,
                Reason = "Installationsordner existiert nicht mehr",
            });
        }

        return orphans;
    }
}

public sealed class RegistryUninstallEntry
{
    public string RegistryPath { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? InstallLocation { get; init; }
}
