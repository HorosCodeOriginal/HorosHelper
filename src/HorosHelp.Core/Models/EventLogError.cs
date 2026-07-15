namespace HorosHelp.Core.Models;

public sealed class EventLogError
{
    public string LogName { get; init; } = "";
    public string Source { get; init; } = "";
    public string Message { get; init; } = "";
    public DateTime Timestamp { get; init; }
}
