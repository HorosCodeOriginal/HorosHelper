using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Backup;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.UI.Services;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class RestorePointItem
{
    public string DateTimeLabel  { get; init; } = "";
    public string Reason         { get; init; } = "";
    public bool   IsCurrent      { get; init; }
    public bool   IsFirst        { get; init; }
    public bool   IsLast         { get; init; }
    public double DotSize        => IsFirst ? 14 : 10;
    public bool   ShowConnector  => !IsLast;
}

public sealed class BackupProfileItem
{
    public string ProfileId       { get; init; } = "";
    public string IconGlyph       { get; init; } = "";
    public IBrush IconBackground  { get; init; } = Brushes.Transparent;
    public string Name            { get; init; } = "";
    public string LastBackupText  { get; init; } = "";
    public string StatusLabel     { get; init; } = "";
    public IBrush StatusForeground { get; init; } = Brushes.White;
    public string StatusIcon      { get; init; } = "✓";
    public string Size            { get; init; } = "";
}

public sealed partial class BackupViewModel : ViewModelBase, IDisposable
{
    private static readonly IBrush BrushGreen    = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushAmber    = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushBlue     = new SolidColorBrush(Color.Parse("#3B82F6"));
    private static readonly IBrush BrushIconAmber  = new SolidColorBrush(Color.Parse("#B45309"));
    private static readonly IBrush BrushIconBlue   = new SolidColorBrush(Color.Parse("#1D4ED8"));
    private static readonly IBrush BrushIconGreen  = new SolidColorBrush(Color.Parse("#15803D"));
    private static readonly IBrush BrushIconPurple = new SolidColorBrush(Color.Parse("#7C3AED"));

    private readonly IBackupService _backupService;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IUacDialogService _uacDialogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<BackupViewModel> _logger;
    private bool _disposed;
    private bool _isBusy;

    public string Title    => "Backup & Wiederherstellung";
    public string Subtitle => "Daten sichern und System wiederherstellen";

    public ObservableCollection<RestorePointItem> RestorePoints { get; } = [];

    public ObservableCollection<BackupProfileItem> BackupProfiles { get; } = [];

    [ObservableProperty] private string _lastBackupStatus = "Letztes Backup: wird geladen … ";
    [ObservableProperty] private string _lastBackupResult = "";
    [ObservableProperty] private bool _showRestorePointAdminHint;
    [ObservableProperty] private string _restorePointAdminHint =
        "Erfordert Administratorrechte (UAC)";

    public BackupViewModel(
        IBackupService backupService,
        IAdminElevationService adminElevationService,
        IUacDialogService uacDialogService,
        INavigationService navigationService,
        ILogger<BackupViewModel> logger)
    {
        _backupService = backupService;
        _adminElevationService = adminElevationService;
        _uacDialogService = uacDialogService;
        _navigationService = navigationService;
        _logger = logger;

        _adminElevationService.AdminStatusChanged += OnAdminStatusChanged;
        _navigationService.Navigated += OnNavigated;
        Dispatcher.UIThread.Post(RefreshFromService);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _adminElevationService.AdminStatusChanged -= OnAdminStatusChanged;
        _navigationService.Navigated -= OnNavigated;
        _disposed = true;
    }

    private void OnAdminStatusChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(RefreshAdminShieldState);

    [RelayCommand]
    private async Task CreateRestorePoint()
    {
        if (_isBusy)
            return;

        if (!_adminElevationService.IsRunningAsAdmin)
        {
            var confirmed = await _uacDialogService.ConfirmElevationAsync(
                "Wiederherstellungspunkte erfordern Administratorrechte.",
                "HorosHelper wird mit UAC neu gestartet, um einen Systemwiederherstellungspunkt zu erstellen.");

            if (confirmed)
            {
                if (!_adminElevationService.RequestElevation())
                {
                    LastBackupStatus = "Hinweis: ";
                    LastBackupResult = "UAC-Elevation abgebrochen.";
                }

                return;
            }

            LastBackupStatus = "Hinweis: ";
            LastBackupResult = "Administratorrechte erforderlich.";
            return;
        }

        _isBusy = true;
        try
        {
            var result = await _backupService.CreateRestorePointAsync();
            LastBackupResult = result.Success ? "Erfolgreich" : result.Message;
            LastBackupStatus = result.Success
                ? "Wiederherstellungspunkt erstellt — "
                : "Hinweis: ";

            if (!result.Success)
                _logger.LogWarning("Create restore point: {Message}", result.Message);
            else
                RefreshFromService();
        }
        finally
        {
            _isBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartNewBackup()
    {
        if (_isBusy)
            return;

        _isBusy = true;
        try
        {
            var profileId = BackupProfiles.FirstOrDefault()?.ProfileId ?? "documents";
            var result = await _backupService.RunProfileBackupAsync(profileId);

            LastBackupStatus = result.Success ? "Letztes Backup: gerade eben — " : "Backup: ";
            LastBackupResult = result.Success ? "Erfolgreich" : result.Message;

            if (result.Success)
                RefreshFromService();
            else
                _logger.LogWarning("Backup failed: {Message}", result.Message);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Backup, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshFromService);
    }

    private void RefreshFromService()
    {
        var snapshot = _backupService.GetSnapshot();

        RefreshAdminShieldState();

        RestorePoints.Clear();
        var ordered = snapshot.RestorePoints.OrderByDescending(p => p.CreatedAt).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var point = ordered[i];
            RestorePoints.Add(new RestorePointItem
            {
                DateTimeLabel = point.CreatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                Reason = string.IsNullOrWhiteSpace(point.Description) ? point.Type : point.Description,
                IsCurrent = i == 0,
                IsFirst = i == 0,
                IsLast = i == ordered.Count - 1,
            });
        }

        BackupProfiles.Clear();
        foreach (var profile in snapshot.Profiles)
        {
            var (status, brush, icon) = MapProfileStatus(profile);
            BackupProfiles.Add(new BackupProfileItem
            {
                ProfileId = profile.Id,
                IconGlyph = MapProfileIcon(profile.Name),
                IconBackground = MapProfileColor(profile.Name),
                Name = profile.Name,
                LastBackupText = CopilotRuleEngine.FormatRelativeBackup(profile.LastBackupUtc),
                StatusLabel = status,
                StatusForeground = brush,
                StatusIcon = icon,
                Size = profile.LastBackupSizeBytes.HasValue
                    ? CopilotRuleEngine.FormatSizeGb(profile.LastBackupSizeBytes.Value)
                    : "—",
            });
        }

        var latest = snapshot.Profiles
            .Where(p => p.LastBackupUtc.HasValue)
            .OrderByDescending(p => p.LastBackupUtc)
            .FirstOrDefault();

        if (latest?.LastBackupUtc is not null)
        {
            LastBackupStatus = CopilotRuleEngine.FormatRelativeBackup(latest.LastBackupUtc) + " — ";
            LastBackupResult = latest.LastBackupStatus ?? "Erfolgreich";
        }
        else
        {
            LastBackupStatus = "Noch kein Backup — ";
            LastBackupResult = "Bereit";
        }
    }

    private void RefreshAdminShieldState()
    {
        ShowRestorePointAdminHint = !_adminElevationService.IsRunningAsAdmin;
        RestorePointAdminHint = ShowRestorePointAdminHint
            ? "Erfordert Administratorrechte (UAC)"
            : "";
    }

    private static (string Status, IBrush Brush, string Icon) MapProfileStatus(Core.Models.Backup.BackupProfileConfig profile)
    {
        if (profile.LastBackupStatus == "Erfolgreich")
            return ("Erfolgreich", new SolidColorBrush(Color.Parse("#22C55E")), "✓");

        if (!profile.LastBackupUtc.HasValue)
            return ("Geplant", new SolidColorBrush(Color.Parse("#3B82F6")), "◷");

        var age = DateTimeOffset.UtcNow - profile.LastBackupUtc.Value;
        if (age.TotalDays > 3)
            return ("Achtung erforderlich", new SolidColorBrush(Color.Parse("#F59E0B")), "⚠");

        return ("Erfolgreich", new SolidColorBrush(Color.Parse("#22C55E")), "✓");
    }

    private static string MapProfileIcon(string name) => name.ToLowerInvariant() switch
    {
        var n when n.Contains("dokument") => "📁",
        var n when n.Contains("desktop") => "🖥",
        var n when n.Contains("bild") => "🖼",
        _ => "⛁",
    };

    private static IBrush MapProfileColor(string name) => name.ToLowerInvariant() switch
    {
        var n when n.Contains("dokument") => new SolidColorBrush(Color.Parse("#B45309")),
        var n when n.Contains("desktop") => new SolidColorBrush(Color.Parse("#1D4ED8")),
        var n when n.Contains("bild") => new SolidColorBrush(Color.Parse("#15803D")),
        _ => new SolidColorBrush(Color.Parse("#7C3AED")),
    };
}
