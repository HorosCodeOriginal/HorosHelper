using HorosHelp.Core.Models.Apps;
using HorosHelp.Core.Services.Security;

namespace HorosHelp.Core.Services.Apps;

public interface IAppMaintenanceService
{
    AppMaintenanceSnapshot GetSnapshot();

    AppUninstallResult StartUninstall(string appId);

    DriverActionResult OpenDriverManager();

    IReadOnlyList<OrphanedRegistryEntry> ScanOrphanedRegistryEntries();

    RegistryCleanupResult RemoveOrphanedRegistryEntry(string registryPath);
}
