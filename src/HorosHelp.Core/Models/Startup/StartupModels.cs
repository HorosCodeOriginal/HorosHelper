namespace HorosHelp.Core.Models.Startup;

public enum StartupEntrySource
{
    HkcuRun,
    HklmRun,
    StartupFolder,
}

public enum StartupImpact
{
    Niedrig,
    Mittel,
    Hoch,
}

public sealed class StartupEntryInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Publisher { get; init; }
    public required string Command { get; init; }
    public StartupEntrySource Source { get; init; }
    public StartupImpact Impact { get; init; }
    public bool IsEnabled { get; init; }
    public bool CanToggle { get; init; }
    public bool IsSafeToDisable { get; init; }
}

public sealed class BackgroundProcessInfo
{
    public required string Name { get; init; }
    public int ProcessId { get; init; }
    public double CpuPercent { get; init; }
    public long WorkingSetBytes { get; init; }
    public string SafetyLabel { get; init; } = "Unbekannt";
    public string SafetyLevel { get; init; } = "Unknown";
    public bool CanTerminate { get; init; } = true;
}

public sealed class StartupSnapshot
{
    public IReadOnlyList<StartupEntryInfo> Entries { get; init; } = [];
    public IReadOnlyList<BackgroundProcessInfo> BackgroundProcesses { get; init; } = [];
    public int SafeToDisableCount { get; init; }
    public bool IsRunningAsAdmin { get; init; }
    public bool IsMockData { get; init; }
}

public sealed class StartupToggleResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}
