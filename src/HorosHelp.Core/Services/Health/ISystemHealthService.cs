using HorosHelp.Core.Models;

namespace HorosHelp.Core.Services.Health;

public interface ISystemHealthService : IDisposable
{
    SystemHealthSnapshot GetSnapshot();
}
