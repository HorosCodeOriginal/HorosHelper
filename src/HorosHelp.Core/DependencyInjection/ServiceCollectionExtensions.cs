using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Apps;
using HorosHelp.Core.Services.Backup;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Knowledge;
using HorosHelp.Core.Services.Network;
using HorosHelp.Core.Services.Processes;
using HorosHelp.Core.Services.ProblemScan;
using HorosHelp.Core.Services.Security;
using HorosHelp.Core.Services.Settings;
using HorosHelp.Core.Services.Startup;
using HorosHelp.Core.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace HorosHelp.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHorosHelpCore(this IServiceCollection services)
    {
        services.AddSingleton<IAdminElevationService, AdminElevationService>();

        services.AddSingleton<IEventLogService, EventLogService>();
        services.AddSingleton<ISystemMetricsProvider, WindowsSystemMetricsProvider>();
        services.AddSingleton<ISystemHealthService, SystemHealthService>();

        services.AddSingleton<IRollbackStore, RollbackStore>();
        services.AddSingleton<IProblemCheck, RegistryProblemCheck>();
        services.AddSingleton<RegistryRepairAction>();
        services.AddSingleton<IRepairAction>(sp => sp.GetRequiredService<RegistryRepairAction>());
        services.AddSingleton<IRepairAction, DnsFlushRepair>();
        services.AddSingleton<IRepairAction, WinsockResetRepair>();
        services.AddSingleton<IRepairAction, WindowsUpdateCacheRepair>();
        services.AddSingleton<IRepairAction, SfcDismRepair>();
        services.AddSingleton<IRepairAction, SearchIndexResetRepair>();
        services.AddSingleton(ProblemScannerThresholds.Default);
        services.AddSingleton<IProblemScannerService, ProblemScannerService>();

        services.AddSingleton<IShell32Launcher, Shell32Launcher>();
        services.AddSingleton<IWindowsSettingsLauncher, WindowsSettingsLauncher>();
        services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddSingleton<IFileSystemScanner, PhysicalFileSystemScanner>();
        services.AddSingleton<IDiskAnalyzerService, DiskAnalyzerService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IProcessManagerService, ProcessManagerService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<INetworkService, NetworkService>();

        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddSingleton<IAppMaintenanceService, AppMaintenanceService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<ICopilotService, CopilotService>();

        return services;
    }
}
