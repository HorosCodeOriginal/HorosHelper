using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Health;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class SystemHealthServiceEventLogTests
{
    [Fact]
    public void GetSnapshot_IncludesEventLogWarnings_FromInjectedService()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        var eventLog = new StubEventLogService(
        [
            new EventLogError
            {
                LogName = "System",
                Source = "Disk",
                Message = "Volume shadow copy failed.",
                Timestamp = new DateTime(2026, 7, 15, 10, 30, 0),
            },
        ]);

        var service = new SystemHealthService(
            NullLogger<SystemHealthService>.Instance,
            settings,
            eventLog,
            new StubMetricsProvider());

        var snapshot = service.GetSnapshot();

        Assert.Contains(snapshot.Warnings, warning =>
            warning.Title.Contains("Ereignisprotokoll", StringComparison.Ordinal)
            && warning.Title.Contains("Disk", StringComparison.Ordinal));
    }

    private sealed class StubEventLogService(IReadOnlyList<EventLogError> errors) : IEventLogService
    {
        public IReadOnlyList<EventLogError> GetRecentErrors(TimeSpan? window = null, int maxCount = 10) =>
            errors.Take(maxCount).ToList();
    }

    private sealed class StubMetricsProvider : ISystemMetricsProvider
    {
        public SystemMetricsSnapshot GetMetrics() => new()
        {
            CpuPercent = 20,
            RamPercent = 40,
            RamUsedGb = 8,
            RamTotalGb = 16,
            DiskPercent = 50,
            DiskUsedGb = 250,
            DiskTotalGb = 500,
            NetworkOk = true,
            IsMockData = false,
        };
    }
}
