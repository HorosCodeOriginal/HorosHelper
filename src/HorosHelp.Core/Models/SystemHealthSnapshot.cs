namespace HorosHelp.Core.Models;

public sealed class SystemHealthWarning
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
}

public sealed class SystemHealthSnapshot
{
    public double CpuPercent { get; init; }
    public double RamPercent { get; init; }
    public double RamUsedGb { get; init; }
    public double RamTotalGb { get; init; }
    public double DiskPercent { get; init; }
    public double DiskUsedGb { get; init; }
    public double DiskTotalGb { get; init; }
    public bool NetworkOk { get; init; }
    public int HealthScore { get; init; }
    public IReadOnlyList<SystemHealthWarning> Warnings { get; init; } = [];
    public bool IsMockData { get; init; }
}
