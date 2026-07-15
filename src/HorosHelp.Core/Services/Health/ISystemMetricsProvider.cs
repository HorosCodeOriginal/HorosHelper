using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Health;

namespace HorosHelp.Core.Services.Health;

public interface ISystemMetricsProvider
{
    SystemMetricsSnapshot GetMetrics();
}
