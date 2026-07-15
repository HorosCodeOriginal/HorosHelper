using HorosHelp.Core.Navigation;
using HorosHelp.UI.Services;
using HorosHelp.UI.ViewModels.Features;
using HorosHelp.UI.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace HorosHelp.UI.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHorosHelpUi(this IServiceCollection services)
    {
        services.AddSingleton<IUacDialogService, UacDialogService>();
        services.AddSingleton<IExceptionNotificationService, ExceptionNotificationService>();

        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<ProblemFixerViewModel>();
        services.AddSingleton<WissenViewModel>();
        services.AddSingleton<SpeicherViewModel>();
        services.AddSingleton<StartupViewModel>();
        services.AddSingleton<NetzwerkViewModel>();
        services.AddSingleton<SicherheitViewModel>();
        services.AddSingleton<AppsViewModel>();
        services.AddSingleton<BackupViewModel>();
        services.AddSingleton<CopilotViewModel>();
        services.AddSingleton<EinstellungenViewModel>();

        services.AddSingleton<INavigationService>(sp => new NavigationService(route => route switch
        {
            NavigationRoutes.Dashboard => sp.GetRequiredService<DashboardViewModel>(),
            NavigationRoutes.ProblemFixer => sp.GetRequiredService<ProblemFixerViewModel>(),
            NavigationRoutes.Wissen => sp.GetRequiredService<WissenViewModel>(),
            NavigationRoutes.Speicher => sp.GetRequiredService<SpeicherViewModel>(),
            NavigationRoutes.Startup => sp.GetRequiredService<StartupViewModel>(),
            NavigationRoutes.Netzwerk => sp.GetRequiredService<NetzwerkViewModel>(),
            NavigationRoutes.Sicherheit => sp.GetRequiredService<SicherheitViewModel>(),
            NavigationRoutes.Apps => sp.GetRequiredService<AppsViewModel>(),
            NavigationRoutes.Backup => sp.GetRequiredService<BackupViewModel>(),
            NavigationRoutes.Copilot => sp.GetRequiredService<CopilotViewModel>(),
            NavigationRoutes.Einstellungen => sp.GetRequiredService<EinstellungenViewModel>(),
            _ => throw new ArgumentException($"Unbekannte Route: {route}", nameof(route))
        }));

        services.AddSingleton<SidebarViewModel>();
        services.AddSingleton<ShellViewModel>();

        return services;
    }
}
