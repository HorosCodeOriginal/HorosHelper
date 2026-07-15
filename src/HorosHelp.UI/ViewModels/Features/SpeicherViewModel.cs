using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Models.Storage;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class DriveCardItem
{
    public string IconGlyph       { get; init; } = "💾";
    public string Title           { get; init; } = "";
    public string MediaType       { get; init; } = "";
    public string UsedText        { get; init; } = "";
    public string FreeText        { get; init; } = "";
    public int    PercentUsed     { get; init; }
    public string PercentText     { get; init; } = "";
    public double ProgressValue   { get; init; }
    public string SmartStatusLabel { get; init; } = "Unbekannt";
    public IBrush SmartStatusBrush { get; init; } = Brushes.Gray;
    public bool   ShowSmartBadge  { get; init; } = true;
}

public sealed class StorageCategoryItem
{
    public string Name          { get; init; } = "";
    public string SizeText      { get; init; } = "";
    public string PercentText   { get; init; } = "";
    public double ProgressValue { get; init; }
}

public sealed class CleanupSuggestionItem
{
    public string Name          { get; init; } = "";
    public string SizeText      { get; init; } = "";
    public double ProgressValue { get; init; }
}

public sealed class FolderTreeItem
{
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public string SizeText { get; init; } = "";
    public int Level { get; init; }
    public bool HasChildren { get; init; }
    public ObservableCollection<FolderTreeItem> Children { get; init; } = [];
}

public sealed partial class SpeicherViewModel : ViewModelBase
{
    private static readonly IBrush BrushGreen = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushAmber = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushGray  = new SolidColorBrush(Color.Parse("#64748B"));

    private readonly IStorageService _storageService;
    private readonly IDiskAnalyzerService _diskAnalyzerService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<SpeicherViewModel> _logger;
    private CancellationTokenSource? _scanCts;
    private bool _isCleaning;
    private bool _isScanning;

    public string Title    => "Speicher";
    public string Subtitle => "Übersicht über Festplatten und Speicherbelegung.";

    public string CleanupTitle       => "Bereinigungsvorschläge";
    public string CleanupDescription => "Wir haben Dateien gefunden, die sicher entfernt werden können, um Speicherplatz freizugeben.";

    public string FolderTreeTitle => "Ordner-Analyse";
    public string FolderTreeDescription => "Größte Verzeichnisse auf dem Systemlaufwerk.";

    [ObservableProperty]
    private string _cleanupButtonText = "Jetzt bereinigen";

    [ObservableProperty]
    private string _scanButtonText = "Ordner scannen";

    [ObservableProperty]
    private string _scanStatusText = "Noch nicht gescannt";

    [ObservableProperty]
    private double _scanProgress;

    [ObservableProperty]
    private bool _isScanInProgress;

    public ObservableCollection<DriveCardItem> DriveCards { get; } = [];
    public ObservableCollection<StorageCategoryItem> StorageCategories { get; } = [];
    public ObservableCollection<FolderTreeItem> FolderTree { get; } = [];

    public string BreakdownTitle => "Speicherbelegung";

    public string CleanupChartTitle => "Bereinigungsvorschläge";

    [ObservableProperty]
    private string _cleanupChartTotal = "—";

    [ObservableProperty]
    private double _cleanupChartPercent;

    public ObservableCollection<CleanupSuggestionItem> CleanupSuggestions { get; } = [];

    public SpeicherViewModel(
        IStorageService storageService,
        IDiskAnalyzerService diskAnalyzerService,
        INavigationService navigationService,
        ILogger<SpeicherViewModel> logger)
    {
        _storageService = storageService;
        _diskAnalyzerService = diskAnalyzerService;
        _navigationService = navigationService;
        _logger = logger;

        _navigationService.Navigated += OnNavigated;
        RefreshFromService();
    }

    [RelayCommand]
    private async Task CleanupAsync()
    {
        if (_isCleaning)
            return;

        _isCleaning = true;
        CleanupButtonText = "Bereinigung läuft …";

        try
        {
            var result = await _storageService.RunSafeCleanupAsync();
            _logger.LogInformation(
                "Storage cleanup finished: {FilesDeleted} files, {BytesFreed} bytes freed, {Errors} errors",
                result.FilesDeleted, result.BytesFreed, result.Errors);

            RefreshFromService();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Storage cleanup command failed.");
        }
        finally
        {
            _isCleaning = false;
            RefreshFromService();
        }
    }

    [RelayCommand]
    private async Task EmptyRecycleBinAsync()
    {
        if (_isCleaning)
            return;

        _isCleaning = true;
        CleanupButtonText = "Papierkorb wird geleert …";

        try
        {
            var result = await _storageService.EmptyRecycleBinAsync();
            _logger.LogInformation("Recycle bin empty result: {Messages}", string.Join("; ", result.Messages));
            RefreshFromService();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Recycle bin cleanup failed.");
        }
        finally
        {
            _isCleaning = false;
            CleanupButtonText = StorageAnalyzer.BuildCleanupButtonText(
                _storageService.GetSnapshot().TotalReclaimableBytes);
        }
    }

    [RelayCommand]
    private async Task ScanFoldersAsync()
    {
        if (_isScanning)
            return;

        _isScanning = true;
        _scanCts?.Cancel();
        _scanCts = new CancellationTokenSource();
        IsScanInProgress = true;
        ScanButtonText = "Scan läuft …";
        ScanProgress = 0;
        FolderTree.Clear();

        try
        {
            var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var progress = new Progress<DiskAnalysisProgress>(p =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ScanProgress = p.Percent;
                    ScanStatusText = string.IsNullOrWhiteSpace(p.CurrentPath)
                        ? "Scan läuft …"
                        : p.CurrentPath;
                });
            });

            var result = await _diskAnalyzerService.ScanAsync(rootPath, progress, _scanCts.Token);

            Dispatcher.UIThread.Post(() =>
            {
                FolderTree.Clear();
                if (result.Root is not null)
                {
                    foreach (var child in MapTree(result.Root, level: 0).Children)
                        FolderTree.Add(child);
                }

                ScanStatusText = result.WasCancelled
                    ? "Scan abgebrochen"
                    : $"{result.TotalFoldersScanned} Ordner analysiert";
            });
        }
        catch (OperationCanceledException)
        {
            ScanStatusText = "Scan abgebrochen";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Folder scan failed.");
            ScanStatusText = "Scan fehlgeschlagen";
        }
        finally
        {
            _isScanning = false;
            IsScanInProgress = false;
            ScanButtonText = "Ordner scannen";
            ScanProgress = 0;
        }
    }

    [RelayCommand]
    private void CancelScan()
    {
        _scanCts?.Cancel();
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Speicher, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshFromService);
    }

    private void RefreshFromService()
    {
        var snapshot = _storageService.GetSnapshot();

        DriveCards.Clear();
        foreach (var drive in snapshot.Drives)
        {
            DriveCards.Add(new DriveCardItem
            {
                IconGlyph = drive.DriveType.Contains("HDD", StringComparison.OrdinalIgnoreCase) ? "🖴" : "💾",
                Title = drive.Label,
                MediaType = $"{drive.DriveType} • {drive.FileSystem}",
                UsedText = StorageAnalyzer.FormatUsedSummary(drive.UsedBytes, drive.TotalBytes),
                FreeText = StorageAnalyzer.FormatFreeSummary(drive.FreeBytes),
                PercentUsed = (int)drive.PercentUsed,
                PercentText = StorageAnalyzer.FormatPercentText(drive.PercentUsed),
                ProgressValue = drive.PercentUsed,
                SmartStatusLabel = $"S.M.A.R.T. {drive.SmartStatusLabel}",
                SmartStatusBrush = MapSmartBrush(drive.SmartStatus),
                ShowSmartBadge = true,
            });
        }

        StorageCategories.Clear();
        foreach (var category in snapshot.Categories)
        {
            StorageCategories.Add(new StorageCategoryItem
            {
                Name = category.Name,
                SizeText = StorageAnalyzer.FormatSizeDe(category.SizeBytes),
                PercentText = StorageAnalyzer.FormatPercentText(category.PercentOfUsed, 1),
                ProgressValue = category.PercentOfUsed,
            });
        }

        CleanupSuggestions.Clear();
        foreach (var candidate in snapshot.CleanupCandidates)
        {
            CleanupSuggestions.Add(new CleanupSuggestionItem
            {
                Name = candidate.Name,
                SizeText = StorageAnalyzer.FormatSizeDe(candidate.SizeBytes),
                ProgressValue = candidate.SharePercent,
            });
        }

        CleanupChartTotal = StorageAnalyzer.FormatSizeDe(snapshot.TotalReclaimableBytes);
        CleanupChartPercent = snapshot.CleanupChartPercent;
        CleanupButtonText = StorageAnalyzer.BuildCleanupButtonText(snapshot.TotalReclaimableBytes);
    }

    private static FolderTreeItem MapTree(FolderTreeNode node, int level)
    {
        var item = new FolderTreeItem
        {
            Name = node.Name,
            Path = node.Path,
            SizeText = StorageAnalyzer.FormatSizeDe(node.SizeBytes),
            Level = level,
            HasChildren = node.Children.Count > 0,
        };

        foreach (var child in node.Children.OrderByDescending(c => c.SizeBytes))
            item.Children.Add(MapTree(child, level + 1));

        return item;
    }

    private static IBrush MapSmartBrush(SmartHealthStatus status) =>
        status switch
        {
            SmartHealthStatus.Ok => BrushGreen,
            SmartHealthStatus.Warning => BrushAmber,
            _ => BrushGray,
        };
}
