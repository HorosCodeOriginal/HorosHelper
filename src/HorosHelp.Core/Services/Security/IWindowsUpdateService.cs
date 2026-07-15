using HorosHelp.Core.Models.Security;

namespace HorosHelp.Core.Services.Security;

public interface IWindowsUpdateService
{
    WindowsUpdateStatus GetStatus();
}
