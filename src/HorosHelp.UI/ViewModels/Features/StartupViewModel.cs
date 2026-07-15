using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HorosHelp.Core.Models.Startup;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Startup;
using HorosHelp.UI.Services;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public enum StartupImpactLevel
{
    Hoch,
    Mittel,
    Niedrig,
}

public partial class StartupProgramItem : ObservableObject
{
    public string EntryId      { get; init; } = "";
    public string IconGlyph    { get; init; } = "●";
    public string Name         { get; init; } = "";
    public string Publisher    { get; init; } = "";
    public StartupImpactLevel Impact { get; init; }
    public bool CanToggle      { get; init; }

    public bool ShowAdminShield => !CanToggle;

    public string ImpactLabel => Impact switch
    {
        StartupImpactLevel.Hoch   => "Hoch",
        StartupImpactLevel.Mittel => "Mittel",
        _                         => "Niedrig",
    };

    public IBrush ImpactForeground => Impact switch
    {
        StartupImpactLevel.Niedrig => BrushGreen,
        _                          => BrushAmber,
    };

    public IBrush ImpactBackground => Impact switch
    {
        StartupImpactLevel.Niedrig => BrushGreenBg,
        _                          => BrushAmberBg,
    };

    public IBrush ImpactBorderBrush => Impact switch
    {
        StartupImpactLevel.Niedrig => BrushGreen,
        _                          => BrushAmber,
    };

    [ObservableProperty] private bool _isEnabled = true;

    private bool _suppressToggle;
    internal Action<StartupProgramItem, bool>? ToggleHandler { get; init; }

    partial void OnIsEnabledChanged(bool value)
    {
        if (!_suppressToggle)
            ToggleHandler?.Invoke(this, value);
    }

    internal void SetEnabledSilent(bool enabled)
    {
        _suppressToggle = true;
        IsEnabled = enabled;
        _suppressToggle = false;
    }

    private static readonly IBrush BrushGreen   = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushGreenBg = new SolidColorBrush(Color.FromArgb(0x18, 0x22, 0xC5, 0x5E));
    private static readonly IBrush BrushAmber   = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushAmberBg = new SolidColorBrush(Color.FromArgb(0x18, 0xF5, 0x9E, 0x0B));
}

public sealed class BackgroundProcessItem
{
    public string IconGlyph     { get; init; } = "●";
    public string Name          { get; init; } = "";
    public string CpuText       { get; init; } = "";
    public string RamText       { get; init; } = "";
    public double CpuProgress   { get; init; }
    public double RamProgress   { get; init; }
    public IBrush CpuBarBrush   { get; init; } = Brushes.Gray;
    public IBrush RamBarBrush   { get; init; } = Brushes.Gray;
}

public sealed partial class StartupViewModel : ViewModelBase, IDisposable
{
    private const int ProcessRefreshIntervalMs = 3000;

    private static readonly IBrush BrushGreen = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushAmber = new SolidColorBrush(Color.Parse("#F59E0B"));

    private readonly IStartupService _startupService;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IUacDialogService _uacDialogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<StartupViewModel> _logger;
    private readonly Timer _processRefreshTimer;
    private bool _disposed;

    public string Title    => "Autostart und Prozesse optimieren";
    public string Subtitle => "Verwalten Sie Programme, die beim Systemstart geladen werden.";

    [ObservableProperty]
    private string _recommendationText = "Empfohlene Optimierungen werden geladen …";

    public string AutostartSectionTitle => "Autostart-Programme";
    public string ProcessSectionTitle   => "Aktive Hintergrundprozesse";

    public ObservableCollection<StartupProgramItem> StartupPrograms { get; } = [];
    public ObservableCollection<BackgroundProcessItem> BackgroundProcesses { get; } = [];

    public StartupViewModel(
        IStartupService startupService,
        IAdminElevationService adminElevationService,
        IUacDialogService uacDialogService,
        INavigationService navigationService,
        ILogger<StartupViewModel> logger)
    {
        _startupService = startupService;
        _adminElevationService = adminElevationService;
        _uacDialogService = uacDialogService;
        _navigationService = navigationService;
        _logger = logger;

        _navigationService.Navigated += OnNavigated;
        RefreshFromService();

        _processRefreshTimer = new Timer(
            _ => Dispatcher.UIThread.Post(RefreshProcesses),
            null,
            ProcessRefreshIntervalMs,
            ProcessRefreshIntervalMs);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _navigationService.Navigated -= OnNavigated;
        _processRefreshTimer.Dispose();
        _disposed = true;
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Startup, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshFromService);
    }

    private void RefreshFromService()
    {
        var snapshot = _startupService.GetSnapshot();

        StartupPrograms.Clear();
        foreach (var entry in snapshot.Entries)
        {
            var item = new StartupProgramItem
            {
                EntryId = entry.Id,
                IconGlyph = GetIconGlyph(entry.Name),
                Name = entry.Name,
                Publisher = entry.Publisher,
                Impact = MapImpact(entry.Impact),
                CanToggle = entry.CanToggle,
                ToggleHandler = (item, enabled) => _ = OnStartupToggleAsync(item, enabled),
            };
            item.SetEnabledSilent(entry.IsEnabled);
            StartupPrograms.Add(item);
        }

        RecommendationText = snapshot.SafeToDisableCount switch
        {
            0 => "Keine sicheren Deaktivierungen gefunden.",
            1 => "Empfohlene Optimierungen — 1 Eintrag kann sicher deaktiviert werden",
            _ => $"Empfohlene Optimierungen — {snapshot.SafeToDisableCount} Einträge können sicher deaktiviert werden",
        };

        ApplyProcesses(snapshot.BackgroundProcesses);
    }

    private void RefreshProcesses()
    {
        if (_disposed)
            return;

        if (!string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Startup, StringComparison.Ordinal))
            return;

        var snapshot = _startupService.GetSnapshot();
        ApplyProcesses(snapshot.BackgroundProcesses);
    }

    private void ApplyProcesses(IReadOnlyList<BackgroundProcessInfo> processes)
    {
        var maxRam = processes.Count > 0 ? processes.Max(p => p.WorkingSetBytes) : 1;

        BackgroundProcesses.Clear();
        foreach (var process in processes)
        {
            var ramProgress = maxRam > 0
                ? process.WorkingSetBytes / (double)maxRam * 100
                : 0;

            var cpuBrush = process.CpuPercent >= 10 ? BrushAmber : BrushGreen;
            var ramBrush = ramProgress >= 50 ? BrushAmber : BrushGreen;

            BackgroundProcesses.Add(new BackgroundProcessItem
            {
                IconGlyph = GetProcessIcon(process.Name),
                Name = process.Name,
                CpuText = $"{process.CpuPercent:F0}%",
                RamText = FormatRam(process.WorkingSetBytes),
                CpuProgress = process.CpuPercent,
                RamProgress = ramProgress,
                CpuBarBrush = cpuBrush,
                RamBarBrush = ramBrush,
            });
        }
    }

    private async Task OnStartupToggleAsync(StartupProgramItem item, bool enabled)
    {
        if (!item.CanToggle)
        {
            item.SetEnabledSilent(!enabled);
            return;
        }

        if (item.EntryId.StartsWith("HklmRun:", StringComparison.OrdinalIgnoreCase)
            && !_adminElevationService.IsRunningAsAdmin)
        {
            item.SetEnabledSilent(!enabled);

            var confirmed = await _uacDialogService.ConfirmElevationAsync(
                "HKLM-Autostart-Einträge erfordern Administratorrechte.",
                "HorosHelper wird mit UAC neu gestartet, um systemweite Autostart-Einträge zu ändern.");

            if (confirmed)
                _adminElevationService.RequestElevation();

            return;
        }

        var result = _startupService.SetEntryEnabled(item.EntryId, enabled);
        if (!result.Success)
        {
            _logger.LogWarning("Startup toggle failed for {EntryId}: {Message}", item.EntryId, result.Message);

            if (result.Message.Contains("Administrator", StringComparison.OrdinalIgnoreCase)
                && !_adminElevationService.IsRunningAsAdmin)
            {
                var confirmed = await _uacDialogService.ConfirmElevationAsync(
                    "Diese Autostart-Änderung erfordert Administratorrechte.",
                    result.Message);

                if (confirmed)
                    _adminElevationService.RequestElevation();
            }

            item.SetEnabledSilent(!enabled);
            return;
        }

        RefreshFromService();
    }

    private static StartupImpactLevel MapImpact(StartupImpact impact) =>
        impact switch
        {
            StartupImpact.Hoch => StartupImpactLevel.Hoch,
            StartupImpact.Mittel => StartupImpactLevel.Mittel,
            _ => StartupImpactLevel.Niedrig,
        };

    private static string FormatRam(long bytes)
    {
        var mb = bytes / 1024d / 1024d;
        return mb >= 1024
            ? $"{mb / 1024:F1} GB".Replace('.', ',')
            : $"{mb:F0} MB";
    }

    private static string GetIconGlyph(string name) =>
        name.ToLowerInvariant() switch
        {
            var n when n.Contains("spotify") => "♫",
            var n when n.Contains("discord") => "💬",
            var n when n.Contains("steam") => "🎮",
            var n when n.Contains("onedrive") => "☁",
            var n when n.Contains("nvidia") => "▲",
            _ => "●",
        };

    private static string GetProcessIcon(string name) =>
        name.ToLowerInvariant() switch
        {
            var n when n.Contains("chrome") => "🌐",
            var n when n.Contains("teams") => "👥",
            var n when n.Contains("explorer") => "📁",
            _ => "●",
        };
}
