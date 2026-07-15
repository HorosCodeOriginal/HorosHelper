using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Health;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class SystemHealthServiceTests
{
    [Fact]
    public void GetSnapshot_CalculatesHealthScore_FromInjectedMetrics()
    {
        var service = CreateService(
            new SystemMetricsSnapshot
            {
                CpuPercent = 20,
                RamPercent = 40,
                RamUsedGb = 8,
                RamTotalGb = 16,
                DiskPercent = 50,
                DiskUsedGb = 250,
                DiskTotalGb = 500,
                DiskVolumes =
                [
                    new DiskVolumeInfo { DriveLetter = "C:", Percent = 50, UsedGb = 250, TotalGb = 500 },
                ],
                NetworkOk = true,
                IsMockData = false,
            },
            []);

        var snapshot = service.GetSnapshot();

        Assert.True(snapshot.HealthScore >= 80);
        Assert.Empty(snapshot.Warnings);
        Assert.False(snapshot.IsMockData);
    }

    [Fact]
    public void GetSnapshot_AddsThresholdWarnings_ForHighCpuRamDisk()
    {
        var service = CreateService(
            new SystemMetricsSnapshot
            {
                CpuPercent = 92,
                RamPercent = 91,
                RamUsedGb = 14,
                RamTotalGb = 16,
                DiskPercent = 93,
                DiskUsedGb = 900,
                DiskTotalGb = 1000,
                NetworkOk = false,
                IsMockData = false,
            },
            []);

        var snapshot = service.GetSnapshot();

        Assert.True(snapshot.HealthScore < 60);
        Assert.Contains(snapshot.Warnings, w => w.Title.Contains("CPU", StringComparison.Ordinal));
        Assert.Contains(snapshot.Warnings, w => w.Title.Contains("RAM", StringComparison.Ordinal));
        Assert.Contains(snapshot.Warnings, w => w.Title.Contains("Speicherplatz", StringComparison.Ordinal));
        Assert.Contains(snapshot.Warnings, w => w.Title.Contains("Netzwerk", StringComparison.Ordinal));
    }

    [Fact]
    public void GetSnapshot_IncludesMultiDriveVolumes()
    {
        var service = CreateService(
            new SystemMetricsSnapshot
            {
                CpuPercent = 10,
                RamPercent = 20,
                RamUsedGb = 4,
                RamTotalGb = 16,
                DiskPercent = 60,
                DiskUsedGb = 300,
                DiskTotalGb = 500,
                DiskVolumes =
                [
                    new DiskVolumeInfo { DriveLetter = "C:", Percent = 60, UsedGb = 300, TotalGb = 500 },
                    new DiskVolumeInfo { DriveLetter = "D:", Percent = 40, UsedGb = 400, TotalGb = 1000 },
                ],
                NetworkOk = true,
                IsMockData = false,
            },
            []);

        var snapshot = service.GetSnapshot();

        Assert.Equal(2, snapshot.DiskVolumes.Count);
        Assert.Equal("D:", snapshot.DiskVolumes[1].DriveLetter);
    }

    [Fact]
    public void GetSnapshot_AppliesEventLogPenalty()
    {
        var service = CreateService(
            new SystemMetricsSnapshot
            {
                CpuPercent = 20,
                RamPercent = 30,
                RamUsedGb = 5,
                RamTotalGb = 16,
                DiskPercent = 40,
                DiskUsedGb = 200,
                DiskTotalGb = 500,
                NetworkOk = true,
                IsMockData = false,
            },
            [
                new EventLogError { LogName = "System", Source = "Disk", Message = "Error", Timestamp = DateTime.UtcNow },
                new EventLogError { LogName = "Application", Source = "App", Message = "Error", Timestamp = DateTime.UtcNow },
            ]);

        var withoutEvents = CreateService(
            new SystemMetricsSnapshot
            {
                CpuPercent = 20,
                RamPercent = 30,
                RamUsedGb = 5,
                RamTotalGb = 16,
                DiskPercent = 40,
                DiskUsedGb = 200,
                DiskTotalGb = 500,
                NetworkOk = true,
                IsMockData = false,
            },
            []).GetSnapshot();

        var withEvents = service.GetSnapshot();

        Assert.True(withEvents.HealthScore < withoutEvents.HealthScore);
        Assert.Contains(withEvents.Warnings, w => w.Title.Contains("Ereignisprotokoll", StringComparison.Ordinal));
    }

    private static SystemHealthService CreateService(
        SystemMetricsSnapshot metrics,
        IReadOnlyList<EventLogError> eventErrors)
    {
        return new SystemHealthService(
            NullLogger<SystemHealthService>.Instance,
            new SettingsService(NullLogger<SettingsService>.Instance),
            new StubEventLogService(eventErrors),
            new StubMetricsProvider(metrics));
    }

    private sealed class StubMetricsProvider(SystemMetricsSnapshot metrics) : ISystemMetricsProvider
    {
        public SystemMetricsSnapshot GetMetrics() => metrics;
    }

    private sealed class StubEventLogService(IReadOnlyList<EventLogError> errors) : IEventLogService
    {
        public IReadOnlyList<EventLogError> GetRecentErrors(TimeSpan? window = null, int maxCount = 10) =>
            errors.Take(maxCount).ToList();
    }
}
