using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Apps;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class AppTableRowItem
{
    public string AppId         { get; init; } = "";
    public string IconGlyph    { get; init; } = "";
    public IBrush IconBackground { get; init; } = Brushes.Transparent;
    public string Name         { get; init; } = "";
    public string Version      { get; init; } = "";
    public string Size         { get; init; } = "";
    public string Installed    { get; init; } = "";
}

public sealed class DriverTableRowItem
{
    public string DriverId        { get; init; } = "";
    public string IconGlyph       { get; init; } = "";
    public IBrush IconBackground  { get; init; } = Brushes.Transparent;
    public string Name            { get; init; } = "";
    public string Version         { get; init; } = "";
    public string Provider        { get; init; } = "";
    public string Installed       { get; init; } = "";
    public string StatusLabel     { get; init; } = "";
    public IBrush StatusForeground { get; init; } = Brushes.White;
    public IBrush StatusBackground { get; init; } = Brushes.Transparent;
    public IBrush StatusBorder    { get; init; } = Brushes.Transparent;
    public bool   ShowUpdateAction { get; init; }
}

public sealed partial class AppsViewModel : ViewModelBase, IDisposable
{
    private static readonly IBrush BrushGreen    = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushGreenBg  = new SolidColorBrush(Color.FromArgb(0x18, 0x22, 0xC5, 0x5E));
    private static readonly IBrush BrushAmber    = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushAmberBg  = new SolidColorBrush(Color.FromArgb(0x18, 0xF5, 0x9E, 0x0B));
    private static readonly IBrush BrushIconBlue = new SolidColorBrush(Color.Parse("#1D4ED8"));
    private static readonly IBrush BrushIconRed  = new SolidColorBrush(Color.Parse("#DC2626"));
    private static readonly IBrush BrushIconGreen = new SolidColorBrush(Color.Parse("#15803D"));
    private static readonly IBrush BrushIconPurple = new SolidColorBrush(Color.Parse("#7C3AED"));

    private readonly IAppMaintenanceService _appMaintenanceService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<AppsViewModel> _logger;
    private readonly List<AppTableRowItem> _allApps = [];
    private bool _disposed;

    public string Title    => "App- & Treiber-Pflege";
    public string Subtitle => "Installierte Programme und Treiber verwalten";

    [ObservableProperty] private bool _isAppsTabSelected = true;

    public bool IsDriversTabSelected
    {
        get => !IsAppsTabSelected;
        set => IsAppsTabSelected = !value;
    }

    partial void OnIsAppsTabSelectedChanged(bool value) =>
        OnPropertyChanged(nameof(IsDriversTabSelected));

    [ObservableProperty] private string _searchText = "";

    partial void OnSearchTextChanged(string value) => ApplyAppFilter();

    public ObservableCollection<AppTableRowItem> Apps { get; } = [];
    public ObservableCollection<DriverTableRowItem> Drivers { get; } = [];

    public AppsViewModel(
        IAppMaintenanceService appMaintenanceService,
        INavigationService navigationService,
        ILogger<AppsViewModel> logger)
    {
        _appMaintenanceService = appMaintenanceService;
        _navigationService = navigationService;
        _logger = logger;

        _navigationService.Navigated += OnNavigated;
        Dispatcher.UIThread.Post(RefreshFromService);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _navigationService.Navigated -= OnNavigated;
        _disposed = true;
    }

    [RelayCommand]
    private void SelectAppsTab() => IsAppsTabSelected = true;

    [RelayCommand]
    private void SelectDriversTab() => IsAppsTabSelected = false;

    [RelayCommand]
    private void UninstallApp(AppTableRowItem? app)
    {
        if (app is null)
            return;

        var result = _appMaintenanceService.StartUninstall(app.AppId);
        if (!result.Success)
            _logger.LogWarning("Uninstall failed for {AppId}: {Message}", app.AppId, result.Message);
        else
            _logger.LogInformation("Uninstall started for {AppName}", app.Name);
    }

    [RelayCommand]
    private void UpdateDriver(DriverTableRowItem? driver)
    {
        var result = _appMaintenanceService.OpenDriverManager();
        if (!result.Success)
            _logger.LogWarning("Driver manager open failed: {Message}", result.Message);
        else if (driver is not null)
            _logger.LogInformation("Device manager opened for driver {DriverName}", driver.Name);
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Apps, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshFromService);
    }

    private void RefreshFromService()
    {
        var snapshot = _appMaintenanceService.GetSnapshot();

        _allApps.Clear();
        foreach (var app in snapshot.Apps)
        {
            _allApps.Add(new AppTableRowItem
            {
                AppId = app.Id,
                IconGlyph = InstalledAppParser.GetInitial(app.Name),
                IconBackground = PickAppColor(app.Name),
                Name = app.Name,
                Version = app.Version,
                Size = InstalledAppParser.FormatSize(app.EstimatedSizeKb),
                Installed = app.InstallDate,
            });
        }

        ApplyAppFilter();

        Drivers.Clear();
        foreach (var driver in snapshot.Drivers)
        {
            var isCurrent = !driver.IsOutdated;
            Drivers.Add(new DriverTableRowItem
            {
                DriverId = driver.Id,
                IconGlyph = InstalledAppParser.GetInitial(driver.Name),
                IconBackground = isCurrent ? BrushIconGreen : BrushIconBlue,
                Name = driver.Name,
                Version = driver.Version,
                Provider = driver.Provider,
                Installed = driver.Installed,
                StatusLabel = isCurrent ? "Aktuell" : "Veraltet",
                StatusForeground = isCurrent ? BrushGreen : BrushAmber,
                StatusBackground = isCurrent ? BrushGreenBg : BrushAmberBg,
                StatusBorder = isCurrent ? BrushGreen : BrushAmber,
                ShowUpdateAction = !isCurrent,
            });
        }
    }

    private void ApplyAppFilter()
    {
        Apps.Clear();
        var query = SearchText.Trim();

        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allApps
            : _allApps.Where(a =>
                a.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || a.Version.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var app in filtered)
            Apps.Add(app);
    }

    private static IBrush PickAppColor(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("microsoft") || lower.Contains("visual"))
            return BrushIconBlue;
        if (lower.Contains("google") || lower.Contains("adobe"))
            return BrushIconRed;
        if (lower.Contains("steam") || lower.Contains("game"))
            return BrushIconPurple;
        return BrushIconBlue;
    }
}
