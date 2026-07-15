using HorosHelp.Core.Models.Backup;
using HorosHelp.Core.Services.Backup;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public sealed class MockTaskSchedulerClient : ITaskSchedulerClient
{
    public readonly Dictionary<string, (string Exe, string Args, BackupScheduleConfig Schedule)> Tasks = new(StringComparer.OrdinalIgnoreCase);

    public bool TaskExists(string taskName) => Tasks.ContainsKey(taskName);

    public bool CreateOrUpdateTask(string taskName, string executablePath, string arguments, BackupScheduleConfig schedule)
    {
        if (!schedule.IsEnabled)
        {
            Tasks.Remove(taskName);
            return true;
        }

        Tasks[taskName] = (executablePath, arguments, schedule);
        return true;
    }

    public bool DeleteTask(string taskName)
    {
        Tasks.Remove(taskName);
        return true;
    }

    public IReadOnlyList<string> ListHorosHelperTaskNames() =>
        Tasks.Keys.Where(k => k.StartsWith("HorosHelper-Backup-", StringComparison.OrdinalIgnoreCase)).ToList();
}

public class BackupSchedulerServiceTests
{
    private readonly MockTaskSchedulerClient _client = new();
    private readonly BackupSchedulerService _service;

    public BackupSchedulerServiceTests()
    {
        _service = new BackupSchedulerService(_client, NullLogger<BackupSchedulerService>.Instance);
    }

    [Theory]
    [InlineData("Taeglich", "02:00", null, "/SC DAILY /ST 02:00")]
    [InlineData("Woechentlich", "03:30", "Monday", "/SC WEEKLY /D MON /ST 03:30")]
    public void BuildScheduleArguments_FormatsSchTasksArgs(string frequency, string time, string? day, string expectedPart)
    {
        var schedule = new BackupScheduleConfig
        {
            IsEnabled = true,
            Frequency = frequency,
            TimeOfDay = time,
            DayOfWeek = day,
        };

        var args = SchTasksSchedulerClient.BuildScheduleArguments(schedule);

        Assert.NotNull(args);
        Assert.Contains(expectedPart, args);
    }

    [Fact]
    public void RegisterProfileSchedule_CreatesTask()
    {
        var profile = new BackupProfileConfig
        {
            Id = "documents",
            Name = "Dokumente",
            Schedule = new BackupScheduleConfig
            {
                IsEnabled = true,
                Frequency = "Taeglich",
                TimeOfDay = "02:00",
            },
        };

        var success = _service.RegisterProfileSchedule(profile, @"C:\HorosHelper\HorosHelp.exe");

        Assert.True(success);
        Assert.True(_client.TaskExists("HorosHelper-Backup-documents"));
    }

    [Fact]
    public void UnregisterProfileSchedule_DeletesTask()
    {
        RegisterProfileSchedule_CreatesTask();
        var success = _service.UnregisterProfileSchedule("documents");

        Assert.True(success);
        Assert.False(_client.TaskExists("HorosHelper-Backup-documents"));
    }

    [Fact]
    public void FormatScheduleLabel_ReturnsGermanLabels()
    {
        Assert.Equal("Manuell", _service.FormatScheduleLabel(null));
        Assert.Equal("Täglich um 02:00", _service.FormatScheduleLabel(new BackupScheduleConfig
        {
            IsEnabled = true,
            Frequency = "Taeglich",
            TimeOfDay = "02:00",
        }));
    }
}
