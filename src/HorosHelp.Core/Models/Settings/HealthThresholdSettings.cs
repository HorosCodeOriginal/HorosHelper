namespace HorosHelp.Core.Models.Settings;

public sealed class HealthThresholdSettings
{
    public double CpuWarn { get; set; } = 80;
    public double RamWarn { get; set; } = 80;
    public double DiskWarn { get; set; } = 85;
}
