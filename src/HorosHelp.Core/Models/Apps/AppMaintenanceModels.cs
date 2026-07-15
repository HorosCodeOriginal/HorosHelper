namespace HorosHelp.Core.Models.Apps;

public sealed class InstalledAppInfo
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string InstallDate { get; init; } = "";
    public long EstimatedSizeKb { get; init; }
    public string? UninstallString { get; init; }
    public string Publisher { get; init; } = "";
}

public sealed class DriverInfo
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string Provider { get; init; } = "";
    public string Installed { get; init; } = "";
    public string Status { get; init; } = "";
    public bool IsOutdated { get; init; }
}

public sealed class AppMaintenanceSnapshot
{
    public IReadOnlyList<InstalledAppInfo> Apps { get; init; } = [];
    public IReadOnlyList<DriverInfo> Drivers { get; init; } = [];
    public bool IsMockData { get; init; }
}

public sealed class AppUninstallResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}

public sealed class DriverActionResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}
