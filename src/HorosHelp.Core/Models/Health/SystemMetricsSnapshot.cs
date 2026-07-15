using HorosHelp.Core.Models;

namespace HorosHelp.Core.Models.Health;

public sealed class SystemMetricsSnapshot
{
    public double CpuPercent { get; init; }
    public double RamPercent { get; init; }
    public double RamUsedGb { get; init; }
    public double RamTotalGb { get; init; }
    public double DiskPercent { get; init; }
    public double DiskUsedGb { get; init; }
    public double DiskTotalGb { get; init; }
    public IReadOnlyList<DiskVolumeInfo> DiskVolumes { get; init; } = [];
    public bool NetworkOk { get; init; }
    public bool IsMockData { get; init; }
}
