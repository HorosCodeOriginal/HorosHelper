using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Admin;

namespace HorosHelp.UI.ViewModels.Shell;

public partial class ShellViewModel : ViewModelBase, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IAdminElevationService _adminElevationService;
    private readonly Timer _adminRefreshTimer;
    private bool _disposed;

    public ShellViewModel(
        INavigationService navigationService,
        SidebarViewModel sidebar,
        IAdminElevationService adminElevationService)
    {
        _navigationService = navigationService;
        _adminElevationService = adminElevationService;
        Sidebar = sidebar;

        _navigationService.Navigated += OnNavigated;
        _navigationService.NavigateTo(NavigationRoutes.Dashboard);

        _adminElevationService.AdminStatusChanged += OnAdminStatusChanged;
        UpdateAdminBadge();
        _adminRefreshTimer = new Timer(
            _ => _adminElevationService.RefreshAdminStatus(),
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
    }

    public SidebarViewModel Sidebar { get; }

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private string _lastScanText = "Letzter Scan: —";

    [ObservableProperty]
    private string _versionText = "v0.1.0";

    [ObservableProperty]
    private string _adminBadgeText = "Admin";

    [ObservableProperty]
    private bool _showAdminBadge;

    public event Action? MinimizeRequested;

    public event Action? MaximizeRequested;

    public event Action? CloseRequested;

    public void Dispose()
    {
        if (_disposed)
            return;

        _adminElevationService.AdminStatusChanged -= OnAdminStatusChanged;
        _adminRefreshTimer.Dispose();
        _navigationService.Navigated -= OnNavigated;
        _disposed = true;
    }

    [RelayCommand]
    private void Minimize() => MinimizeRequested?.Invoke();

    [RelayCommand]
    private void Maximize() => MaximizeRequested?.Invoke();

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke();

    private void OnNavigated(object? sender, EventArgs e)
    {
        CurrentViewModel = _navigationService.CurrentViewModel;
    }

    private void OnAdminStatusChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(UpdateAdminBadge);

    private void UpdateAdminBadge()
    {
        ShowAdminBadge = _adminElevationService.IsRunningAsAdmin;
        AdminBadgeText = ShowAdminBadge ? "Admin" : "Standard";
    }
}
