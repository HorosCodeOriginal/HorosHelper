using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class DriveCardItem
{
    public string IconGlyph      { get; init; } = "💾";
    public string Title          { get; init; } = "";
    public string MediaType      { get; init; } = "";
    public string UsedText       { get; init; } = "";
    public string FreeText       { get; init; } = "";
    public int    PercentUsed    { get; init; }
    public string PercentText    { get; init; } = "";
    public double ProgressValue  { get; init; }
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

public sealed partial class SpeicherViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<SpeicherViewModel> _logger;
    private bool _isCleaning;

    public string Title    => "Speicher";
    public string Subtitle => "Übersicht über Festplatten und Speicherbelegung.";

    public string CleanupTitle       => "Bereinigungsvorschläge";
    public string CleanupDescription => "Wir haben Dateien gefunden, die sicher entfernt werden können, um Speicherplatz freizugeben.";

    [ObservableProperty]
    private string _cleanupButtonText = "Jetzt bereinigen";

    public ObservableCollection<DriveCardItem> DriveCards { get; } = [];
    public ObservableCollection<StorageCategoryItem> StorageCategories { get; } = [];

    public string BreakdownTitle => "Speicherbelegung";

    public string CleanupChartTitle => "Bereinigungsvorschläge";

    [ObservableProperty]
    private string _cleanupChartTotal = "—";

    [ObservableProperty]
    private double _cleanupChartPercent;

    public ObservableCollection<CleanupSuggestionItem> CleanupSuggestions { get; } = [];

    public SpeicherViewModel(
        IStorageService storageService,
        INavigationService navigationService,
        ILogger<SpeicherViewModel> logger)
    {
        _storageService = storageService;
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
}
