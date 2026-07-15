using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Health;

namespace HorosHelp.Tests;

public class AppSettingsMapperTests
{
    [Fact]
    public void ToHealthThresholds_MapsWarningValues()
    {
        var thresholds = AppSettingsMapper.ToHealthThresholds(new HealthThresholdSettings
        {
            CpuWarn = 75,
            RamWarn = 72,
            DiskWarn = 88,
        });

        Assert.Equal(75, thresholds.CpuWarningPercent);
        Assert.Equal(72, thresholds.RamWarningPercent);
        Assert.Equal(88, thresholds.DiskWarningPercent);
        Assert.Equal(90, thresholds.CpuCriticalPercent);
    }
}
