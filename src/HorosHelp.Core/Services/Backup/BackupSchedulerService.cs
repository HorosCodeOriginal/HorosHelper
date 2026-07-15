using System.Diagnostics;
using HorosHelp.Core.Models.Backup;
using HorosHelp.Core.Services.Security;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Backup;

public interface ITaskSchedulerClient
{
    bool TaskExists(string taskName);
    bool CreateOrUpdateTask(string taskName, string executablePath, string arguments, BackupScheduleConfig schedule);
    bool DeleteTask(string taskName);
    IReadOnlyList<string> ListHorosHelperTaskNames();
}

public sealed class SchTasksSchedulerClient : ITaskSchedulerClient
{
    private const string TaskPrefix = "HorosHelper-Backup-";
    private readonly ILogger<SchTasksSchedulerClient> _logger;

    public SchTasksSchedulerClient(ILogger<SchTasksSchedulerClient> logger)
    {
        _logger = logger;
    }

    public bool TaskExists(string taskName)
    {
        if (!InputSecurityValidator.IsValidTaskName(taskName, out _))
            return false;

        var output = RunSchTasks($"/Query /TN \"{taskName}\" /FO LIST");
        return output.Contains("TaskName", StringComparison.OrdinalIgnoreCase) &&
               !output.Contains("ERROR", StringComparison.OrdinalIgnoreCase);
    }

    public bool CreateOrUpdateTask(string taskName, string executablePath, string arguments, BackupScheduleConfig schedule)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        if (!InputSecurityValidator.IsValidTaskName(taskName, out var taskError))
        {
            _logger.LogWarning("Invalid task name: {Error}", taskError);
            return false;
        }

        if (!InputSecurityValidator.IsValidFilePath(executablePath, out var pathError))
        {
            _logger.LogWarning("Invalid executable path: {Error}", pathError);
            return false;
        }

        if (schedule is not { IsEnabled: true })
            return DeleteTask(taskName);

        if (TaskExists(taskName))
            DeleteTask(taskName);

        var scheduleArgs = BuildScheduleArguments(schedule);
        if (scheduleArgs is null)
            return false;

        var tr = $"\"{executablePath}\" {arguments}";
        var createArgs =
            $"/Create /F /TN \"{taskName}\" /TR \"{tr}\" {scheduleArgs} /RL LIMITED";

        var output = RunSchTasks(createArgs);
        var success = !output.Contains("ERROR", StringComparison.OrdinalIgnoreCase);

        if (!success)
            _logger.LogWarning("schtasks create failed: {Output}", output);

        return success;
    }

    public bool DeleteTask(string taskName)
    {
        if (!InputSecurityValidator.IsValidTaskName(taskName, out _))
            return false;

        if (!TaskExists(taskName))
            return true;

        var output = RunSchTasks($"/Delete /TN \"{taskName}\" /F");
        return !output.Contains("ERROR", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> ListHorosHelperTaskNames()
    {
        var output = RunSchTasks("/Query /FO CSV /NH");
        if (string.IsNullOrWhiteSpace(output))
            return [];

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Trim('"').Split(',')[0].Trim('"'))
            .Where(name => name.StartsWith(TaskPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static string BuildTaskName(string profileId) =>
        $"{TaskPrefix}{profileId}";

    public static string? BuildScheduleArguments(BackupScheduleConfig schedule)
    {
        if (!TimeOnly.TryParse(schedule.TimeOfDay, out var time))
            time = new TimeOnly(2, 0);

        var timeArg = $"{time:HH:mm}";

        return schedule.Frequency switch
        {
            "Taeglich" or "Täglich" => $"/SC DAILY /ST {timeArg}",
            "Woechentlich" or "Wöchentlich" => BuildWeeklySchedule(schedule, timeArg),
            _ => null,
        };
    }

    private static string? BuildWeeklySchedule(BackupScheduleConfig schedule, string timeArg)
    {
        if (!Enum.TryParse<DayOfWeek>(schedule.DayOfWeek ?? "Monday", true, out var day))
            day = DayOfWeek.Monday;

        var dayName = day switch
        {
            DayOfWeek.Monday => "MON",
            DayOfWeek.Tuesday => "TUE",
            DayOfWeek.Wednesday => "WED",
            DayOfWeek.Thursday => "THU",
            DayOfWeek.Friday => "FRI",
            DayOfWeek.Saturday => "SAT",
            DayOfWeek.Sunday => "SUN",
            _ => "MON",
        };

        return $"/SC WEEKLY /D {dayName} /ST {timeArg}";
    }

    private static string RunSchTasks(string arguments)
    {
        if (!InputSecurityValidator.IsValidProcessFileName("schtasks", out _))
            return "ERROR: invalid process";

        var psi = new ProcessStartInfo
        {
            FileName = "schtasks",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return "ERROR: process start failed";

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return string.IsNullOrWhiteSpace(output) ? error : output + error;
    }
}

public interface IBackupSchedulerService
{
    bool RegisterProfileSchedule(BackupProfileConfig profile, string executablePath);
    bool UnregisterProfileSchedule(string profileId);
    bool IsScheduleRegistered(string profileId);
    IReadOnlyList<string> GetRegisteredProfileIds();
    string FormatScheduleLabel(BackupScheduleConfig? schedule);
}

public sealed class BackupSchedulerService : IBackupSchedulerService
{
    private readonly ITaskSchedulerClient _taskScheduler;
    private readonly ILogger<BackupSchedulerService> _logger;

    public BackupSchedulerService(ITaskSchedulerClient taskScheduler, ILogger<BackupSchedulerService> logger)
    {
        _taskScheduler = taskScheduler;
        _logger = logger;
    }

    public bool RegisterProfileSchedule(BackupProfileConfig profile, string executablePath)
    {
        if (profile.Schedule is not { IsEnabled: true })
            return UnregisterProfileSchedule(profile.Id);

        var taskName = SchTasksSchedulerClient.BuildTaskName(profile.Id);
        var arguments = $"--backup-run {profile.Id}";

        var success = _taskScheduler.CreateOrUpdateTask(taskName, executablePath, arguments, profile.Schedule);
        if (success)
            _logger.LogInformation("Scheduled backup registered for profile {ProfileId}", profile.Id);
        else
            _logger.LogWarning("Failed to register scheduled backup for profile {ProfileId}", profile.Id);

        return success;
    }

    public bool UnregisterProfileSchedule(string profileId)
    {
        var taskName = SchTasksSchedulerClient.BuildTaskName(profileId);
        return _taskScheduler.DeleteTask(taskName);
    }

    public bool IsScheduleRegistered(string profileId) =>
        _taskScheduler.TaskExists(SchTasksSchedulerClient.BuildTaskName(profileId));

    public IReadOnlyList<string> GetRegisteredProfileIds() =>
        _taskScheduler.ListHorosHelperTaskNames()
            .Select(name => name.StartsWith("HorosHelper-Backup-", StringComparison.OrdinalIgnoreCase)
                ? name["HorosHelper-Backup-".Length..]
                : name)
            .ToList();

    public string FormatScheduleLabel(BackupScheduleConfig? schedule)
    {
        if (schedule is not { IsEnabled: true })
            return "Manuell";

        var time = schedule.TimeOfDay;
        return schedule.Frequency switch
        {
            "Taeglich" or "Täglich" => $"Täglich um {time}",
            "Woechentlich" or "Wöchentlich" => $"Wöchentlich ({schedule.DayOfWeek ?? "Montag"}) um {time}",
            _ => "Manuell",
        };
    }
}
