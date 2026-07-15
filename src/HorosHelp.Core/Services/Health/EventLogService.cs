using System.Diagnostics;
using HorosHelp.Core.Models;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Health;

public sealed class EventLogService : IEventLogService
{
    private static readonly string[] LogNames = ["System", "Application"];
    private readonly ILogger<EventLogService> _logger;

    public EventLogService(ILogger<EventLogService> logger) => _logger = logger;

    public IReadOnlyList<EventLogError> GetRecentErrors(TimeSpan? window = null, int maxCount = 10)
    {
        if (!OperatingSystem.IsWindows())
            return [];

        var effectiveWindow = window ?? TimeSpan.FromHours(24);
        var cutoff = DateTime.Now - effectiveWindow;
        var limit = Math.Clamp(maxCount, 1, 50);

        try
        {
            var errors = new List<EventLogError>();

            foreach (var logName in LogNames)
            {
                try
                {
                    errors.AddRange(ReadErrorsFromLog(logName, cutoff));
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to read event log {LogName}.", logName);
                }
            }

            return errors
                .OrderByDescending(error => error.Timestamp)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Event log read failed; returning empty result.");
            return [];
        }
    }

    private static IEnumerable<EventLogError> ReadErrorsFromLog(string logName, DateTime cutoff)
    {
        using var eventLog = new EventLog(logName);

        for (var i = eventLog.Entries.Count - 1; i >= 0; i--)
        {
            var entry = eventLog.Entries[i];
            if (entry.TimeGenerated < cutoff)
                yield break;

            if (entry.EntryType is not (EventLogEntryType.Error or EventLogEntryType.FailureAudit))
                continue;

            yield return new EventLogError
            {
                LogName = logName,
                Source = entry.Source,
                Message = TruncateMessage(entry.Message),
                Timestamp = entry.TimeGenerated,
            };
        }
    }

    private static string TruncateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Keine Details verfügbar.";

        var normalized = message.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= 160 ? normalized : normalized[..157] + "...";
    }
}
