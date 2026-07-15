using HorosHelp.Core.Models.Apps;

namespace HorosHelp.Core.Services.Apps;

public interface IAppMaintenanceService
{
    AppMaintenanceSnapshot GetSnapshot();

    AppUninstallResult StartUninstall(string appId);

    DriverActionResult OpenDriverManager();
}
