using HorosHelp.Core.Models;

namespace HorosHelp.Core.Services.Health;

public interface IEventLogService
{
  IReadOnlyList<EventLogError> GetRecentErrors(TimeSpan? window = null, int maxCount = 10);
}
