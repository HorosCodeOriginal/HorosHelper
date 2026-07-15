using HorosHelp.Core.Services.Health;

namespace HorosHelp.Core.Models.Settings;

public static class AppSettingsMapper
{
    public static SystemHealthThresholds ToHealthThresholds(HealthThresholdSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new SystemHealthThresholds
        {
            CpuWarningPercent = ClampPercent(settings.CpuWarn),
            RamWarningPercent = ClampPercent(settings.RamWarn),
            DiskWarningPercent = ClampPercent(settings.DiskWarn),
        };
    }

    private static double ClampPercent(double value) => Math.Clamp(value, 50, 99);
}
