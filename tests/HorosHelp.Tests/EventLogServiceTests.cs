using HorosHelp.Core.Models;
using HorosHelp.Core.Services.Health;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class EventLogServiceTests
{
    [Fact]
    public void GetRecentErrors_OnNonWindows_ReturnsEmpty()
    {
        if (OperatingSystem.IsWindows())
            return;

        var service = new EventLogService(NullLogger<EventLogService>.Instance);
        var errors = service.GetRecentErrors();

        Assert.Empty(errors);
    }

    [Fact]
    public void GetRecentErrors_RespectsMaxCount()
    {
        var service = new FakeEventLogService(CreateSampleErrors(12));
        var errors = service.GetRecentErrors(maxCount: 5);

        Assert.Equal(5, errors.Count);
    }

    [Fact]
    public void GetRecentErrors_OrdersByNewestFirst()
    {
        var service = new FakeEventLogService(
        [
            new EventLogError { Source = "Old", Timestamp = DateTime.Now.AddHours(-10) },
            new EventLogError { Source = "New", Timestamp = DateTime.Now.AddMinutes(-5) },
        ]);

        var errors = service.GetRecentErrors(maxCount: 2);

        Assert.Equal("New", errors[0].Source);
        Assert.Equal("Old", errors[1].Source);
    }

    [Fact]
    public void GetRecentErrors_FiltersOutsideWindow()
    {
        var service = new FakeEventLogService(
        [
            new EventLogError { Source = "Recent", Timestamp = DateTime.Now.AddHours(-1) },
            new EventLogError { Source = "Stale", Timestamp = DateTime.Now.AddDays(-3) },
        ]);

        var errors = service.GetRecentErrors(TimeSpan.FromHours(24), maxCount: 10);

        Assert.Single(errors);
        Assert.Equal("Recent", errors[0].Source);
    }

    private static IReadOnlyList<EventLogError> CreateSampleErrors(int count)
    {
        var errors = new List<EventLogError>(count);
        for (var i = 0; i < count; i++)
        {
            errors.Add(new EventLogError
            {
                Source = $"Source-{i}",
                Message = $"Message-{i}",
                Timestamp = DateTime.Now.AddMinutes(-i),
                LogName = "System",
            });
        }

        return errors;
    }

    private sealed class FakeEventLogService(IReadOnlyList<EventLogError> seed) : IEventLogService
    {
        public IReadOnlyList<EventLogError> GetRecentErrors(TimeSpan? window = null, int maxCount = 10)
        {
            var effectiveWindow = window ?? TimeSpan.FromHours(24);
            var cutoff = DateTime.Now - effectiveWindow;
            var limit = Math.Clamp(maxCount, 1, 50);

            return seed
                .Where(error => error.Timestamp >= cutoff)
                .OrderByDescending(error => error.Timestamp)
                .Take(limit)
                .ToList();
        }
    }
}
