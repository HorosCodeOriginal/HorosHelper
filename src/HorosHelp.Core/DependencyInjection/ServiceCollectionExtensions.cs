using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Apps;
using HorosHelp.Core.Services.Backup;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Knowledge;
using HorosHelp.Core.Services.Network;
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

        services.AddSingleton<ISystemHealthService, SystemHealthService>();

        services.AddSingleton<IRepairAction, DnsFlushRepair>();
        services.AddSingleton<IRepairAction, WinsockResetRepair>();
        services.AddSingleton(ProblemScannerThresholds.Default);
        services.AddSingleton<IProblemScannerService, ProblemScannerService>();

        services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<INetworkService, NetworkService>();

        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddSingleton<IAppMaintenanceService, AppMaintenanceService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<ICopilotService, CopilotService>();

        return services;
    }
}
