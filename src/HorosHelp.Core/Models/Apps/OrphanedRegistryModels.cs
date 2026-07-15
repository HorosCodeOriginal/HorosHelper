namespace HorosHelp.Core.Models.Apps;

public sealed class OrphanedRegistryEntry
{
    public string RegistryPath { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? InstallLocation { get; init; }
    public string Reason { get; init; } = "";
}

public sealed class RegistryCleanupResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}
