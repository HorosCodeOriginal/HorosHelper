namespace HorosHelp.Core.Services.Health;

/// <summary>
/// Default warning thresholds for Feature 1 (System-Gesundheits-Dashboard).
/// Disk warning at 85% aligns with mockup text "weniger als 15% freier Speicherplatz".
/// </summary>
public sealed class SystemHealthThresholds
{
    public double CpuWarningPercent { get; init; } = 80;
    public double CpuCriticalPercent { get; init; } = 90;
    public double RamWarningPercent { get; init; } = 80;
    public double RamCriticalPercent { get; init; } = 90;
    public double DiskWarningPercent { get; init; } = 85;
    public double DiskCriticalPercent { get; init; } = 95;

    public static SystemHealthThresholds Default { get; } = new();
}
