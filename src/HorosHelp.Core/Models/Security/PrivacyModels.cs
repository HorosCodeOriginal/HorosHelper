namespace HorosHelp.Core.Models.Security;

public sealed class UacLevelInfo
{
    public bool IsEnabled { get; init; }
    public int ConsentPromptBehavior { get; init; }
    public string Label { get; init; } = "";
    public string Description { get; init; } = "";
}

public sealed class WindowsUpdateStatus
{
    public int PendingSecurityUpdates { get; init; }
    public DateTimeOffset? LastCheckTime { get; init; }
    public bool IsCurrent { get; init; }
    public string Label { get; init; } = "";
    public string Description { get; init; } = "";
}

public sealed class PrivacySettingInfo
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public bool IsEnabled { get; init; }
    public bool CanWrite { get; init; }
    public bool RequiresAdmin { get; init; }
}

public sealed class AppPermissionInfo
{
    public string Kind { get; init; } = "";
    public string Name { get; init; } = "";
    public bool IsAllowed { get; init; }
    public string Label { get; init; } = "";
    public bool CanWrite { get; init; }
}

public sealed class PrivacySnapshot
{
    public IReadOnlyList<PrivacySettingInfo> Settings { get; init; } = [];
    public IReadOnlyList<AppPermissionInfo> AppPermissions { get; init; } = [];
    public bool IsMockData { get; init; }
}

public sealed class PrivacyWriteResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public bool RequiresAdmin { get; init; }
}
