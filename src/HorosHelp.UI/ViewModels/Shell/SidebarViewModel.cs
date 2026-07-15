using System.Collections.ObjectModel;
using HorosHelp.Core.Navigation;
using HorosHelp.UI.Resources;

namespace HorosHelp.UI.ViewModels.Shell;

public sealed class SidebarViewModel : ViewModelBase
{
    private static readonly (string Route, string ResourceKey, string Icon)[] NavDefinitions =
    [
        (NavigationRoutes.Dashboard, nameof(UiStrings.NavDashboard), "M4 12a8 8 0 0 1 16 0v1H4v-1zm2 3h12v2H6v-2z"),
        (NavigationRoutes.ProblemFixer, nameof(UiStrings.NavProblemFixer), "M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l4.3-6.3a4 4 0 0 0 5.4-5.4z"),
        (NavigationRoutes.Wissen, nameof(UiStrings.NavWissen), "M4 5h16v2H4V5zm0 6h10v2H4v-2zm0 6h16v2H4v-2z"),
        (NavigationRoutes.Speicher, nameof(UiStrings.NavSpeicher), "M4 6h16v12H4V6zm2 2v8h12V8H6z"),
        (NavigationRoutes.Startup, nameof(UiStrings.NavStartup), "M12 3l8 5v8l-8 5-8-5V8l8-5zm0 3.2L7 9v5l5 3.1L17 14V9l-5-2.8z"),
        (NavigationRoutes.Netzwerk, nameof(UiStrings.NavNetzwerk), "M2 12h4l2-5 4 10 2-5h8"),
        (NavigationRoutes.Sicherheit, nameof(UiStrings.NavSicherheit), "M12 2l8 4v6c0 5-3.5 9.7-8 10-4.5-.3-8-5-8-10V6l8-4z"),
        (NavigationRoutes.Apps, nameof(UiStrings.NavApps), "M4 4h6v6H4V4zm10 0h6v6h-6V4zM4 14h6v6H4v-6zm10 0h6v6h-6v-6z"),
        (NavigationRoutes.Backup, nameof(UiStrings.NavBackup), "M12 3v12m0 0l-4-4m4 4l4-4M5 19h14"),
        (NavigationRoutes.Copilot, nameof(UiStrings.NavCopilot), "M12 3l1.5 4.5L18 9l-4.5 1.5L12 15l-1.5-4.5L6 9l4.5-1.5L12 3z"),
        (NavigationRoutes.Einstellungen, nameof(UiStrings.NavEinstellungen), "M12 8a4 4 0 1 1 0 8 4 4 0 0 1 0-8zm8.5 4a7.5 7.5 0 0 1-.2 1.7l2 1.5-2 3.5-2.4-1a7.6 7.6 0 0 1-1.5.9l-.4 2.6H9l-.4-2.6a7.6 7.6 0 0 1-1.5-.9l-2.4 1-2-3.5 2-1.5A7.5 7.5 0 0 1 3.5 12c0-.6.1-1.1.2-1.7l-2-1.5 2-3.5 2.4 1c.5-.4 1-.7 1.5-.9l.4-2.6h6l.4 2.6c.5.2 1 .5 1.5.9l2.4-1 2 3.5-2 1.5c.1.6.2 1.1.2 1.7z")
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
