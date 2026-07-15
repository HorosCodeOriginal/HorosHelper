using System.Collections.ObjectModel;
using HorosHelp.Core.Navigation;
using HorosHelp.UI.Resources;

namespace HorosHelp.UI.ViewModels.Shell;

public sealed class SidebarViewModel : ViewModelBase
{
    private static readonly (string Route, string ResourceKey, string Icon)[] NavDefinitions =
    [
        (NavigationRoutes.Dashboard, nameof(UiStrings.NavDashboard), "M3 3h7v9H3z M14 3h7v5h-7z M14 12h7v9h-7z M3 16h7v5H3z"),
        (NavigationRoutes.ProblemFixer, nameof(UiStrings.NavProblemFixer), "M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"),
        (NavigationRoutes.Wissen, nameof(UiStrings.NavWissen), "M4 19.5A2.5 2.5 0 0 1 6.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"),
        (NavigationRoutes.Speicher, nameof(UiStrings.NavSpeicher), "M22 12H2M5.45 5.11L2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z"),
        (NavigationRoutes.Startup, nameof(UiStrings.NavStartup), "M4.5 16.5c-1.5 1.26-2 5-2 5s3.74-.5 5-2c.71-.84.7-2.13-.09-2.91a2.18 2.18 0 0 0-2.91-.09zM12 15l-3-3a22 22 0 0 1 2-3.95A12.88 12.88 0 0 1 22 2c0 2.72-.78 7.5-6 11a22.35 22.35 0 0 1-4 2z"),
        (NavigationRoutes.Netzwerk, nameof(UiStrings.NavNetzwerk), "M5 12.55a11 11 0 0 1 14.08 0M8.53 16.11a6 6 0 0 1 6.95 0M12 20h.01"),
        (NavigationRoutes.Sicherheit, nameof(UiStrings.NavSicherheit), "M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"),
        (NavigationRoutes.Apps, nameof(UiStrings.NavApps), "M3 3h7v7H3z M14 3h7v7h-7z M14 14h7v7h-7z M3 14h7v7H3z"),
        (NavigationRoutes.Backup, nameof(UiStrings.NavBackup), "M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z"),
        (NavigationRoutes.Copilot, nameof(UiStrings.NavCopilot), "M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z"),
        (NavigationRoutes.Einstellungen, nameof(UiStrings.NavEinstellungen), "M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z")
    ];

    public SidebarViewModel(INavigationService navigationService)
    {
        Items = new ObservableCollection<NavItemViewModel>(
            NavDefinitions.Select(def => new NavItemViewModel(
                def.Route,
                ResolveLabel(def.ResourceKey),
                def.Icon,
                navigationService,
                UpdateActiveRoute)));

        navigationService.Navigated += (_, _) => SyncActiveRoute(navigationService.CurrentRoute);
    }

    public ObservableCollection<NavItemViewModel> Items { get; }

    private static string ResolveLabel(string resourceKey) => resourceKey switch
    {
        nameof(UiStrings.NavDashboard) => UiStrings.NavDashboard,
        nameof(UiStrings.NavProblemFixer) => UiStrings.NavProblemFixer,
        nameof(UiStrings.NavWissen) => UiStrings.NavWissen,
        nameof(UiStrings.NavSpeicher) => UiStrings.NavSpeicher,
        nameof(UiStrings.NavStartup) => UiStrings.NavStartup,
        nameof(UiStrings.NavNetzwerk) => UiStrings.NavNetzwerk,
        nameof(UiStrings.NavSicherheit) => UiStrings.NavSicherheit,
        nameof(UiStrings.NavApps) => UiStrings.NavApps,
        nameof(UiStrings.NavBackup) => UiStrings.NavBackup,
        nameof(UiStrings.NavCopilot) => UiStrings.NavCopilot,
        nameof(UiStrings.NavEinstellungen) => UiStrings.NavEinstellungen,
        _ => resourceKey,
    };

    private string? _activeRoute;

    private void UpdateActiveRoute(string route)
    {
        _activeRoute = route;
        SyncActiveRoute(route);
    }

    private void SyncActiveRoute(string? route)
    {
        foreach (var item in Items)
        {
            item.IsActive = string.Equals(item.Route, route ?? _activeRoute, StringComparison.Ordinal);
        }
    }
}
