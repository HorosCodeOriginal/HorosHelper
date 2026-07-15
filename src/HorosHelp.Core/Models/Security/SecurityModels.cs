namespace HorosHelp.Core.Models.Security;

public sealed class FirewallStatus
{
    public bool IsEnabled { get; init; }
    public string Label { get; init; } = "";
}

public sealed class DefenderStatus
{
    public bool IsActive { get; init; }
    public bool RealTimeProtectionEnabled { get; init; }
    public DateTimeOffset? LastQuickScanTime { get; init; }
    public int ThreatsBlocked { get; init; }
    public string ProductName { get; init; } = "Windows Defender";
}

public sealed class LiveProtectionFeatureInfo
{
    public string Name { get; init; } = "";
    public bool IsActive { get; init; }
}

public sealed class SecuritySnapshot
{
    public int SecurityScore { get; init; }
    public FirewallStatus Firewall { get; init; } = new();
    public DefenderStatus Defender { get; init; } = new();
    public bool SecurityUpdatesCurrent { get; init; }
    public WindowsUpdateStatus WindowsUpdate { get; init; } = new();
    public UacLevelInfo UacLevel { get; init; } = new();
    public IReadOnlyList<LiveProtectionFeatureInfo> LiveProtection { get; init; } = [];
    public bool RealTimeProtectionToggleWritable { get; init; }
    public bool IsRunningAsAdmin { get; init; }
    public bool IsMockData { get; init; }
}

public sealed class SecurityToggleResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}
