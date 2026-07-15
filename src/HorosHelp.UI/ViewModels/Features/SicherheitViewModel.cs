using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Privacy;
using HorosHelp.Core.Services.Security;
using HorosHelp.UI.Services;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class SecurityStatusCardItem
{
    public string IconGlyph         { get; init; } = "";
    public IBrush IconBackground    { get; init; } = Brushes.Transparent;
    public string Title             { get; init; } = "";
    public string StatusLabel       { get; init; } = "";
    public IBrush StatusForeground  { get; init; } = Brushes.White;
    public string Description       { get; init; } = "";
    public bool   ShowLed           { get; init; }
    public bool   ShowToggle        { get; init; }
    public bool   ToggleOn          { get; init; }
    public bool   ShowActionButton  { get; init; }
    public string ActionLabel       { get; init; } = "";
    public bool   ShowLargeValue    { get; init; }
    public string LargeValue        { get; init; } = "";
    public bool   ShowSparkline     { get; init; }
    public bool   ShowAdminShield   { get; init; }
}

public sealed class LiveProtectionItem
{
    public string Label           { get; init; } = "";
    public string StatusLabel     { get; init; } = "";
    public IBrush StatusForeground { get; init; } = Brushes.White;
}

public sealed class PrivacySettingItem
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public bool IsEnabled { get; init; }
    public string StatusLabel { get; init; } = "";
    public bool RequiresAdmin { get; init; }
    public bool ShowAdminShield { get; init; }
    public bool IsAppPermission { get; init; }
}

public sealed partial class SicherheitViewModel : ViewModelBase, IDisposable
{
    private static readonly IBrush BrushGreen     = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushGreenBg   = new SolidColorBrush(Color.FromArgb(0x18, 0x22, 0xC5, 0x5E));
    private static readonly IBrush BrushAmber     = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushAmberBg   = new SolidColorBrush(Color.FromArgb(0x18, 0xF5, 0x9E, 0x0B));
    private static readonly IBrush BrushRed       = new SolidColorBrush(Color.Parse("#EF4444"));
    private static readonly IBrush BrushIconAmber = new SolidColorBrush(Color.Parse("#B45309"));
    private static readonly IBrush BrushIconBlue  = new SolidColorBrush(Color.Parse("#1D4ED8"));

    private readonly ISecurityService _securityService;
    private readonly IPrivacyService _privacyService;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IUacDialogService _uacDialogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<SicherheitViewModel> _logger;
    private bool _disposed;
    private bool _isBusy;

    public string Title              => "Sicherheits-Zentrale";
    public string Subtitle           => "Überwachen und schützen Sie Ihr System in Echtzeit.";

    [ObservableProperty] private string _systemSecureTitle = "Status wird geladen …";
    [ObservableProperty] private string _systemSecureSub = "";

    [ObservableProperty] private double _scoreValue;
    [ObservableProperty] private string _scoreText = "—";
    public string ScoreMaxText       => "/100";
    [ObservableProperty] private string _scoreStatusLabel = "";
    [ObservableProperty] private string _scoreStatusSub = "";

    [ObservableProperty] private string _uacLevelLabel = "—";
    [ObservableProperty] private string _uacLevelDescription = "";
    [ObservableProperty] private string _windowsUpdateLabel = "—";
    [ObservableProperty] private string _windowsUpdateDescription = "";
    [ObservableProperty] private string _privacyStatusText = "";

    public string LiveProtectionTitle => "Echtzeit-Schutzfunktionen";
    public string PrivacySectionTitle => "Privatsphäre-Einstellungen";

    public ObservableCollection<SecurityStatusCardItem> StatusCards { get; } = [];
    public ObservableCollection<LiveProtectionItem> LiveProtectionItems { get; } = [];
    public ObservableCollection<PrivacySettingItem> PrivacySettings { get; } = [];

    public SicherheitViewModel(
        ISecurityService securityService,
        IPrivacyService privacyService,
        IAdminElevationService adminElevationService,
        IUacDialogService uacDialogService,
        INavigationService navigationService,
        ILogger<SicherheitViewModel> logger)
    {
        _securityService = securityService;
        _privacyService = privacyService;
        _adminElevationService = adminElevationService;
        _uacDialogService = uacDialogService;
        _navigationService = navigationService;
        _logger = logger;

        _navigationService.Navigated += OnNavigated;
        _adminElevationService.AdminStatusChanged += OnAdminStatusChanged;
        Dispatcher.UIThread.Post(RefreshFromService);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _navigationService.Navigated -= OnNavigated;
        _adminElevationService.AdminStatusChanged -= OnAdminStatusChanged;
        _disposed = true;
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Sicherheit, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshFromService);
    }

    private void OnAdminStatusChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(RefreshFromService);

    [RelayCommand]
    private async Task Refresh()
    {
        if (_isBusy)
            return;

        _isBusy = true;
        try
        {
            await Task.Run(RefreshFromService);
        }
        finally
        {
            _isBusy = false;
        }
    }

    [RelayCommand]
    private async Task TogglePrivacySetting(PrivacySettingItem? item)
    {
        if (item is null || _isBusy)
            return;

        var newValue = !item.IsEnabled;
        var action = newValue ? "aktivieren" : "deaktivieren";

        var confirmed = await _uacDialogService.ConfirmAsync(
            "Privatsphäre ändern",
            $"Möchten Sie \"{item.Name}\" wirklich {action}?");

        if (!confirmed)
            return;

        if (item.RequiresAdmin && !_adminElevationService.IsRunningAsAdmin)
        {
            var elevate = await _uacDialogService.ConfirmElevationAsync(
                "Diese Einstellung erfordert Administratorrechte.",
                "HorosHelper wird mit UAC neu gestartet, um die Änderung anzuwenden.");

            if (elevate)
                _adminElevationService.RequestElevation();

            PrivacyStatusText = "Administratorrechte erforderlich.";
            return;
        }

        _isBusy = true;
        try
        {
            var result = await Task.Run(() => _privacyService.ApplySetting(item.Id, newValue));
            PrivacyStatusText = result.Message;

            if (result.Success)
                RefreshFromService();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void RefreshFromService()
    {
        var snapshot = _securityService.GetSnapshot();
        var privacy = _privacyService.GetSnapshot();

        ScoreValue = snapshot.SecurityScore;
        ScoreText = snapshot.SecurityScore.ToString(CultureInfo.InvariantCulture);
        ScoreStatusLabel = SecurityScoreCalculator.GetScoreStatusLabel(snapshot.SecurityScore);
        ScoreStatusSub = SecurityScoreCalculator.GetScoreStatusSubtext(snapshot.SecurityScore);

        SystemSecureTitle = snapshot.SecurityScore >= 75
            ? "Ihr System ist sicher"
            : "Sicherheit prüfen";
        SystemSecureSub = snapshot.SecurityScore >= 75
            ? "Alle Sicherheitssysteme aktiv"
            : "Einige Schutzkomponenten benötigen Aufmerksamkeit";

        UacLevelLabel = snapshot.UacLevel.Label;
        UacLevelDescription = snapshot.UacLevel.Description;
        WindowsUpdateLabel = snapshot.WindowsUpdate.Label;
        WindowsUpdateDescription = snapshot.WindowsUpdate.Description;

        StatusCards.Clear();
        StatusCards.Add(BuildFirewallCard(snapshot));
        StatusCards.Add(BuildDefenderCard(snapshot));
        StatusCards.Add(BuildRealTimeCard(snapshot));
        StatusCards.Add(BuildLastScanCard(snapshot));
        StatusCards.Add(BuildThreatsCard(snapshot));
        StatusCards.Add(BuildUpdatesCard(snapshot));
        StatusCards.Add(BuildUacCard(snapshot));

        LiveProtectionItems.Clear();
        foreach (var feature in snapshot.LiveProtection)
        {
            LiveProtectionItems.Add(new LiveProtectionItem
            {
                Label = feature.Name,
                StatusLabel = feature.IsActive ? "Aktiv" : "Inaktiv",
                StatusForeground = feature.IsActive ? BrushGreen : BrushAmber,
            });
        }

        PrivacySettings.Clear();
        foreach (var setting in privacy.Settings)
        {
            PrivacySettings.Add(new PrivacySettingItem
            {
                Id = setting.Id,
                Name = setting.Name,
                Description = setting.Description,
                IsEnabled = setting.IsEnabled,
                StatusLabel = setting.IsEnabled ? "Aktiv" : "Inaktiv",
                RequiresAdmin = setting.RequiresAdmin,
                ShowAdminShield = setting.RequiresAdmin && !snapshot.IsRunningAsAdmin,
                IsAppPermission = false,
            });
        }

        foreach (var permission in privacy.AppPermissions)
        {
            PrivacySettings.Add(new PrivacySettingItem
            {
                Id = permission.Kind switch
                {
                    "webcam" => PrivacySettingIds.Camera,
                    "microphone" => PrivacySettingIds.Microphone,
                    "location" => PrivacySettingIds.Location,
                    _ => permission.Kind,
                },
                Name = permission.Name,
                Description = $"App-Zugriff auf {permission.Name.ToLowerInvariant()}.",
                IsEnabled = permission.IsAllowed,
                StatusLabel = permission.Label,
                RequiresAdmin = false,
                ShowAdminShield = false,
                IsAppPermission = true,
            });
        }

        if (snapshot.IsMockData || privacy.IsMockData)
            _logger.LogDebug("Security view loaded mock snapshot.");
    }

    private SecurityStatusCardItem BuildFirewallCard(Core.Models.Security.SecuritySnapshot snapshot)
    {
        var ok = snapshot.Firewall.IsEnabled;
        return new SecurityStatusCardItem
        {
            IconGlyph = "▦",
            IconBackground = BrushIconAmber,
            Title = "Windows Firewall",
            StatusLabel = snapshot.Firewall.Label,
            StatusForeground = ok ? BrushGreen : BrushRed,
            Description = ok ? "Die Firewall schützt Ihr System." : "Firewall ist deaktiviert.",
            ShowLed = true,
        };
    }

    private SecurityStatusCardItem BuildDefenderCard(Core.Models.Security.SecuritySnapshot snapshot)
    {
        var ok = snapshot.Defender.IsActive;
        return new SecurityStatusCardItem
        {
            IconGlyph = "⛨",
            IconBackground = BrushIconAmber,
            Title = snapshot.Defender.ProductName,
            StatusLabel = ok ? "Geschützt" : "Inaktiv",
            StatusForeground = ok ? BrushGreen : BrushRed,
            Description = ok ? "Echtzeitschutz ist aktiv." : "Defender ist nicht aktiv.",
            ShowLed = true,
        };
    }

    private SecurityStatusCardItem BuildRealTimeCard(Core.Models.Security.SecuritySnapshot snapshot)
    {
        var on = snapshot.Defender.RealTimeProtectionEnabled;
        var desc = snapshot.RealTimeProtectionToggleWritable
            ? "Bedrohungen werden in Echtzeit erkannt und blockiert."
            : "Schreibgeschützt ohne Administratorrechte.";

        return new SecurityStatusCardItem
        {
            IconGlyph = "⛨",
            IconBackground = BrushIconBlue,
            Title = "Echtzeitschutz",
            StatusLabel = on ? "Aktiv" : "Inaktiv",
            StatusForeground = on ? BrushGreen : BrushAmber,
            Description = desc,
            ShowToggle = true,
            ToggleOn = on,
            ShowAdminShield = !snapshot.RealTimeProtectionToggleWritable,
        };
    }

    private SecurityStatusCardItem BuildLastScanCard(Core.Models.Security.SecuritySnapshot snapshot)
    {
        var scan = snapshot.Defender.LastQuickScanTime;
        var label = scan.HasValue
            ? scan.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) + " Uhr"
            : "Kein Scan bekannt";

        return new SecurityStatusCardItem
        {
            IconGlyph = "⌕",
            IconBackground = BrushIconAmber,
            Title = "Letzter Scan",
            StatusLabel = label,
            StatusForeground = scan.HasValue ? BrushGreen : BrushAmber,
            Description = scan.HasValue ? "Schnellscan durchgeführt." : "Kein Scan-Zeitstempel verfügbar.",
            ShowActionButton = true,
            ActionLabel = "Scan starten",
        };
    }

    private SecurityStatusCardItem BuildThreatsCard(Core.Models.Security.SecuritySnapshot snapshot) =>
        new()
        {
            IconGlyph = "◎",
            IconBackground = new SolidColorBrush(Color.Parse("#1D4ED8")),
            Title = "Bedrohungen blockiert",
            StatusLabel = "",
            StatusForeground = BrushGreen,
            Description = snapshot.Defender.ThreatsBlocked == 0
                ? "Keine Bedrohungen erkannt."
                : $"{snapshot.Defender.ThreatsBlocked} Bedrohungen blockiert.",
            ShowLargeValue = true,
            LargeValue = snapshot.Defender.ThreatsBlocked.ToString(CultureInfo.InvariantCulture),
            ShowSparkline = true,
        };

    private SecurityStatusCardItem BuildUpdatesCard(Core.Models.Security.SecuritySnapshot snapshot)
    {
        var current = snapshot.SecurityUpdatesCurrent;
        var pending = snapshot.WindowsUpdate.PendingSecurityUpdates;
        return new SecurityStatusCardItem
        {
            IconGlyph = "↻",
            IconBackground = BrushIconAmber,
            Title = "Sicherheitsaktualisierungen",
            StatusLabel = snapshot.WindowsUpdate.Label,
            StatusForeground = current ? BrushGreen : BrushAmber,
            Description = pending > 0
                ? $"{pending} ausstehende Sicherheitsupdate(s). {snapshot.WindowsUpdate.Description}"
                : snapshot.WindowsUpdate.Description,
            ShowLed = true,
        };
    }

    private SecurityStatusCardItem BuildUacCard(Core.Models.Security.SecuritySnapshot snapshot) =>
        new()
        {
            IconGlyph = "🛡",
            IconBackground = BrushIconBlue,
            Title = "UAC-Stufe",
            StatusLabel = snapshot.UacLevel.Label,
            StatusForeground = snapshot.UacLevel.IsEnabled ? BrushGreen : BrushRed,
            Description = snapshot.UacLevel.Description,
            ShowLed = true,
        };
}
