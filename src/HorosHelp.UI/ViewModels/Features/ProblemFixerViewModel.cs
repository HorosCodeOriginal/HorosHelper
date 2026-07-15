using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Models.Knowledge;
using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Knowledge;
using HorosHelp.Core.Services.ProblemScan;
using HorosHelp.UI.Services;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class ProblemCardItem
{
    public ProblemKind Kind            { get; init; }
    public ProblemSeverity Severity    { get; init; }
    public string IconGlyph            { get; init; } = "";
    public IBrush IconBackground       { get; init; } = Brushes.Transparent;
    public string Title                { get; init; } = "";
    public string Subtitle             { get; init; } = "";
    public string StatusLabel          { get; init; } = "";
    public IBrush StatusBackground     { get; init; } = Brushes.Transparent;
    public IBrush StatusForeground     { get; init; } = Brushes.White;
    public IBrush StatusBorderBrush    { get; init; } = Brushes.Transparent;
    public double ProgressValue        { get; init; }
    public bool   ShowProgress         { get; init; }
    public IBrush ProgressFill         { get; init; } = Brushes.Gray;
}

public sealed class LogEntryItem
{
    public string Time        { get; init; } = "";
    public string Message     { get; init; } = "";
    public string StatusIcon  { get; init; } = "●";
    public IBrush StatusBrush { get; init; } = Brushes.Gray;
}

public sealed class RollbackEntryItem
{
    public string Id { get; init; } = "";
    public string Description { get; init; } = "";
    public string TimestampText { get; init; } = "";
    public string KindLabel { get; init; } = "";
}

public sealed partial class ProblemFixerViewModel : ViewModelBase, IDisposable
{
    private static readonly IBrush BrushGreen     = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushGreenBg   = new SolidColorBrush(Color.FromArgb(0x18, 0x22, 0xC5, 0x5E));
    private static readonly IBrush BrushIconGreen = new SolidColorBrush(Color.Parse("#15803D"));
    private static readonly IBrush BrushAmber     = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushAmberBg   = new SolidColorBrush(Color.FromArgb(0x18, 0xF5, 0x9E, 0x0B));
    private static readonly IBrush BrushIconAmber = new SolidColorBrush(Color.Parse("#B45309"));
    private static readonly IBrush BrushRed       = new SolidColorBrush(Color.Parse("#EF4444"));
    private static readonly IBrush BrushRedBg     = new SolidColorBrush(Color.FromArgb(0x18, 0xEF, 0x44, 0x44));
    private static readonly IBrush BrushIconRed   = new SolidColorBrush(Color.Parse("#B91C1C"));
    private static readonly IBrush BrushGray      = new SolidColorBrush(Color.Parse("#64748B"));

    private readonly IProblemScannerService _scannerService;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IUacDialogService _uacDialogService;
    private readonly ILogger<ProblemFixerViewModel> _logger;
    private CancellationTokenSource? _scanCts;
    private bool _isScanning;
    private bool _isRepairing;

    public string Title    => "Problem-Fixer";
    public string Subtitle => "Scannt und behebt Probleme, um die Leistung Ihres Systems zu verbessern.";

    [ObservableProperty] private double _scanProgress;
    [ObservableProperty] private string _scanProgressText = "0%";
    [ObservableProperty] private string _scanStatusText = "Bereit zum Scannen";
    [ObservableProperty] private string _scanSubText = "Klicken Sie auf „Jetzt reparieren“ nach dem Scan.";
    [ObservableProperty] private bool _showRepairAdminShield;

    public ObservableCollection<ProblemCardItem> ProblemCards { get; } = [];
    public ObservableCollection<LogEntryItem> LogEntries { get; } = [];
    public ObservableCollection<RollbackEntryItem> RollbackEntries { get; } = [];

    public ProblemFixerViewModel(
        IProblemScannerService scannerService,
        IAdminElevationService adminElevationService,
        IUacDialogService uacDialogService,
        ILogger<ProblemFixerViewModel> logger)
    {
        _scannerService = scannerService;
        _adminElevationService = adminElevationService;
        _uacDialogService = uacDialogService;
        _logger = logger;

        _adminElevationService.AdminStatusChanged += OnAdminStatusChanged;
        UpdateRepairAdminShield();
        RefreshRollbackEntries();

        Dispatcher.UIThread.Post(() => _ = ScanAsync());
    }

    public void Dispose() =>
        _adminElevationService.AdminStatusChanged -= OnAdminStatusChanged;

    private void OnAdminStatusChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(UpdateRepairAdminShield);

    private void UpdateRepairAdminShield() =>
        ShowRepairAdminShield = !_adminElevationService.IsRunningAsAdmin;

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (_isScanning)
            return;

        _isScanning = true;
        _scanCts?.Cancel();
        _scanCts = new CancellationTokenSource();

        ProblemCards.Clear();
        LogEntries.Clear();
        ScanProgress = 0;
        ScanProgressText = "0%";

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ScanProgress = p.Percent;
                    ScanProgressText = $"{p.Percent:F0}%";
                    ScanStatusText = p.StatusText;
                    ScanSubText = p.SubText;

                    if (p.LogEntry is not null)
                        LogEntries.Add(MapLogEntry(p.LogEntry));
                });
            });

            var result = await _scannerService.ScanAsync(progress, _scanCts.Token);

            Dispatcher.UIThread.Post(() =>
            {
                ProblemCards.Clear();
                foreach (var problem in result.Problems)
                    ProblemCards.Add(MapProblemCard(problem));

                ScanStatusText = "Scan abgeschlossen";
                ScanSubText = result.UsedMockData
                    ? "Einige Werte basieren auf Mock-Daten (eingeschränkter Zugriff)."
                    : "Gefundene Probleme können repariert werden.";
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Scan cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scan failed.");
            ScanStatusText = "Scan fehlgeschlagen";
            ScanSubText = "Bitte versuchen Sie es erneut.";
        }
        finally
        {
            _isScanning = false;
        }
    }

    [RelayCommand]
    private async Task RepairAsync()
    {
        if (_isRepairing || _isScanning)
            return;

        if (!_adminElevationService.IsRunningAsAdmin
            && ProblemCards.Any(p => p.Severity != ProblemSeverity.Good))
        {
            var confirmed = await _uacDialogService.ConfirmElevationAsync(
                "Einige Reparaturen erfordern Administratorrechte.",
                "HorosHelper wird mit UAC neu gestartet, um System-Reparaturen mit erhöhten Rechten auszuführen.");

            if (confirmed)
            {
                _adminElevationService.RequestElevation();
                return;
            }

            LogEntries.Add(new LogEntryItem
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Message = "Reparatur abgebrochen — Administratorrechte erforderlich.",
                StatusIcon = "△",
                StatusBrush = BrushAmber,
            });
            return;
        }

        _isRepairing = true;

        try
        {
            ScanStatusText = "Reparatur läuft...";
            ScanSubText = "Bitte warten Sie einen Moment.";

            var entries = await _scannerService.RepairAsync();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var entry in entries)
                    LogEntries.Add(MapLogEntry(entry));

                ScanStatusText = "Reparatur abgeschlossen";
                ScanSubText = "Führen Sie einen erneuten Scan durch, um den Status zu prüfen.";
            });

            await ScanAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repair failed.");
            LogEntries.Add(new LogEntryItem
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Message = "Reparatur fehlgeschlagen",
                StatusIcon = "✕",
                StatusBrush = BrushRed,
            });
        }
        finally
        {
            _isRepairing = false;
            RefreshRollbackEntries();
        }
    }

    [RelayCommand]
    private async Task RollbackAsync(string? rollbackId)
    {
        if (string.IsNullOrWhiteSpace(rollbackId) || _isRepairing || _isScanning)
            return;

        var confirmed = await _uacDialogService.ConfirmAsync(
            "Aktion rückgängig machen",
            "Möchten Sie diese Reparatur wirklich rückgängig machen?");

        if (!confirmed)
            return;

        _isRepairing = true;

        try
        {
            var entries = await _scannerService.RollbackAsync(rollbackId);

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var entry in entries)
                    LogEntries.Add(MapLogEntry(entry));
            });

            RefreshRollbackEntries();
            await ScanAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed for {RollbackId}", rollbackId);
        }
        finally
        {
            _isRepairing = false;
        }
    }

    private void RefreshRollbackEntries()
    {
        RollbackEntries.Clear();
        foreach (var entry in _scannerService.GetRecentRollbacks(5))
        {
            RollbackEntries.Add(new RollbackEntryItem
            {
                Id = entry.Id,
                Description = entry.Description,
                TimestampText = entry.Timestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                KindLabel = entry.RepairKind switch
                {
                    ProblemKind.Registry => "Registry",
                    ProblemKind.TempFiles => "Temp",
                    ProblemKind.SearchIndexReset => "Suchdienst",
                    _ => entry.RepairKind.ToString(),
                },
            });
        }
    }

    private static ProblemCardItem MapProblemCard(ProblemCard card)
    {
        var (statusLabel, statusBg, statusFg, statusBorder, iconBg, iconGlyph, progressFill) =
            card.Severity switch
            {
                ProblemSeverity.Good => ("✓  Gut", BrushGreenBg, BrushGreen, BrushGreen, BrushIconGreen, "⊞", BrushGreen),
                ProblemSeverity.Warning => ("⚠  Achtung", BrushAmberBg, BrushAmber, BrushAmber, BrushIconAmber, "⊟", BrushAmber),
                _ => ("✕  Kritisch", BrushRedBg, BrushRed, BrushRed, BrushIconRed, "⊞", BrushRed),
            };

        return new ProblemCardItem
        {
            Kind = card.Kind,
            Severity = card.Severity,
            IconGlyph = iconGlyph,
            IconBackground = iconBg,
            Title = card.Title,
            Subtitle = card.Subtitle,
            StatusLabel = statusLabel,
            StatusBackground = statusBg,
            StatusForeground = statusFg,
            StatusBorderBrush = statusBorder,
            ShowProgress = card.Severity != ProblemSeverity.Good,
            ProgressValue = card.ProgressValue,
            ProgressFill = progressFill,
        };
    }

    private static LogEntryItem MapLogEntry(ScanLogEntry entry)
    {
        var (icon, brush) = entry.Status switch
        {
            ScanLogStatus.Success => ("✓", BrushGreen),
            ScanLogStatus.Warning => ("△", BrushAmber),
            ScanLogStatus.Error => ("✕", BrushRed),
            ScanLogStatus.InProgress => ("⋯", BrushAmber),
            _ => ("●", BrushGray),
        };

        return new LogEntryItem
        {
            Time = entry.Timestamp.ToString("HH:mm:ss"),
            Message = entry.Message,
            StatusIcon = icon,
            StatusBrush = brush,
        };
    }
}
