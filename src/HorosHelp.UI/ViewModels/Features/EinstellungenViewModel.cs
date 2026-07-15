using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Logging;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class SettingsCategoryItem
{
    private static readonly IBrush AmberBrush = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush AmberBg = new SolidColorBrush(Color.FromArgb(0x18, 0xF5, 0x9E, 0x0B));
    private static readonly IBrush Transparent = Brushes.Transparent;
    private static readonly IBrush TextPrimary = new SolidColorBrush(Color.Parse("#F8FAFC"));
    private static readonly IBrush IconBg = new SolidColorBrush(Color.Parse("#334155"));
    private static readonly IBrush AmberIconBg = new SolidColorBrush(Color.Parse("#92400E"));

    public string IconGlyph { get; init; } = "";
    public string Name { get; init; } = "";
    public string Key { get; init; } = "";
    public bool IsActive { get; init; }

    public IBrush ItemBackground => IsActive ? AmberBg : Transparent;
    public IBrush ItemBorderBrush => IsActive ? AmberBrush : Transparent;
    public IBrush IconBackground => IsActive ? AmberIconBg : IconBg;
    public IBrush IconForeground => IsActive ? AmberBrush : TextPrimary;
    public IBrush NameForeground => IsActive ? AmberBrush : TextPrimary;
    public FontWeight NameWeight => IsActive ? FontWeight.SemiBold : FontWeight.Normal;
}

public sealed partial class EinstellungenViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogViewerService _logViewerService;
    private readonly ILogger<EinstellungenViewModel> _logger;

    public string Title => "Einstellungen";
    public string Subtitle => "App-Konfiguration anpassen";

    [ObservableProperty] private string _selectedCategoryKey = "allgemein";

    public ObservableCollection<SettingsCategoryItem> Categories { get; } = new()
    {
        new SettingsCategoryItem { IconGlyph = "⚙", Name = "Allgemein", Key = "allgemein", IsActive = true },
        new SettingsCategoryItem { IconGlyph = "▣", Name = "Darstellung", Key = "darstellung", IsActive = false },
        new SettingsCategoryItem { IconGlyph = "⊙", Name = "Scans", Key = "scans", IsActive = false },
        new SettingsCategoryItem { IconGlyph = "🔒", Name = "Datenschutz", Key = "datenschutz", IsActive = false },
        new SettingsCategoryItem { IconGlyph = "📋", Name = "Protokolle", Key = "protokolle", IsActive = false },
        new SettingsCategoryItem { IconGlyph = "ⓘ", Name = "Über", Key = "ueber", IsActive = false },
    };

    [ObservableProperty] private bool _openOnStartup = true;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private bool _notificationsEnabled = true;
    [ObservableProperty] private string _selectedTheme = "Dunkel";
    [ObservableProperty] private string _selectedLanguage = "Deutsch";
    [ObservableProperty] private double _scanIntervalSeconds = 2;
    [ObservableProperty] private double _cpuWarnPercent = 80;
    [ObservableProperty] private double _ramWarnPercent = 80;
    [ObservableProperty] private double _diskWarnPercent = 85;

    [ObservableProperty] private string _saveStatusText = "";

    [ObservableProperty] private string _selectedLogFileName = "";
    [ObservableProperty] private string _logTailText = "";
    [ObservableProperty] private string _logsDirectoryText = "";

    public ObservableCollection<LogFileListItem> LogFiles { get; } = [];

    public ObservableCollection<string> ThemeOptions { get; } = new() { "Dunkel", "Hell", "System" };
    public ObservableCollection<string> LanguageOptions { get; } = new() { "Deutsch", "English" };

    public string ActiveCategoryTitle => SelectedCategoryKey switch
    {
        "darstellung" => "Darstellung",
        "scans" => "Scans",
        "datenschutz" => "Datenschutz",
        "protokolle" => "Protokolle",
        "ueber" => "Über",
        _ => "Allgemein",
    };

    public bool ShowAllgemeinForm => SelectedCategoryKey == "allgemein";
    public bool ShowDarstellungForm => SelectedCategoryKey == "darstellung";
    public bool ShowScansForm => SelectedCategoryKey == "scans";
    public bool ShowDatenschutzForm => SelectedCategoryKey == "datenschutz";
    public bool ShowProtokolleForm => SelectedCategoryKey == "protokolle";
    public bool ShowUeberForm => SelectedCategoryKey == "ueber";

    public string ScanIntervalText => $"Scan-Intervall: {ScanIntervalSeconds:0} Sekunden";
    public string CpuWarnText => $"CPU-Warnung ab {CpuWarnPercent:0}%";
    public string RamWarnText => $"RAM-Warnung ab {RamWarnPercent:0}%";
    public string DiskWarnText => $"Festplatten-Warnung ab {DiskWarnPercent:0}%";

    public string AppVersion => "HorosHelper v1.0.0";
    public string BuildInfo => "Build 2026.07.14 · HorosCode";

    public EinstellungenViewModel(
        ISettingsService settingsService,
        ILogViewerService logViewerService,
        ILogger<EinstellungenViewModel> logger)
    {
        _settingsService = settingsService;
        _logViewerService = logViewerService;
        _logger = logger;
        LogsDirectoryText = _logViewerService.LogsDirectory;
        ApplySettings(_settingsService.Load());
        RefreshLogFiles();
    }

    [RelayCommand]
    private void SelectCategory(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        SelectedCategoryKey = key;

        for (var i = 0; i < Categories.Count; i++)
        {
            var cat = Categories[i];
            Categories[i] = new SettingsCategoryItem
            {
                IconGlyph = cat.IconGlyph,
                Name = cat.Name,
                Key = cat.Key,
                IsActive = cat.Key == key,
            };
        }

        OnPropertyChanged(nameof(ActiveCategoryTitle));
        OnPropertyChanged(nameof(ShowAllgemeinForm));
        OnPropertyChanged(nameof(ShowDarstellungForm));
        OnPropertyChanged(nameof(ShowScansForm));
        OnPropertyChanged(nameof(ShowDatenschutzForm));
        OnPropertyChanged(nameof(ShowProtokolleForm));
        OnPropertyChanged(nameof(ShowUeberForm));

        if (key == "protokolle")
            RefreshLogFiles();
    }

    partial void OnScanIntervalSecondsChanged(double value) =>
        OnPropertyChanged(nameof(ScanIntervalText));

    partial void OnCpuWarnPercentChanged(double value) =>
        OnPropertyChanged(nameof(CpuWarnText));

    partial void OnRamWarnPercentChanged(double value) =>
        OnPropertyChanged(nameof(RamWarnText));

    partial void OnDiskWarnPercentChanged(double value) =>
        OnPropertyChanged(nameof(DiskWarnText));

    [RelayCommand]
    private void Save()
    {
        try
        {
            var settings = new AppSettings
            {
                OpenOnStartup = OpenOnStartup,
                StartMinimized = StartMinimized,
                Theme = SelectedTheme,
                Language = SelectedLanguage,
                ScanIntervalSeconds = ScanIntervalSeconds,
                NotificationsEnabled = NotificationsEnabled,
                HealthThresholds = new HealthThresholdSettings
                {
                    CpuWarn = CpuWarnPercent,
                    RamWarn = RamWarnPercent,
                    DiskWarn = DiskWarnPercent,
                },
                FavoriteArticleIds = _settingsService.Current.FavoriteArticleIds,
            };

            _settingsService.Save(settings);
            SaveStatusText = "Einstellungen gespeichert.";
            _logger.LogInformation("Settings saved via EinstellungenViewModel");
        }
        catch (Exception ex)
        {
            SaveStatusText = "Speichern fehlgeschlagen.";
            _logger.LogError(ex, "Failed to save settings");
        }
    }

    [RelayCommand]
    private void RefreshLogFiles()
    {
        LogFiles.Clear();
        foreach (var file in _logViewerService.GetRecentLogFiles())
        {
            LogFiles.Add(new LogFileListItem
            {
                FileName = file.FileName,
                SizeLabel = file.SizeLabel,
                LastModifiedLabel = file.LastModified.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
            });
        }

        if (LogFiles.Count == 0)
        {
            SelectedLogFileName = "";
            LogTailText = "Keine Protokolldateien gefunden.";
            return;
        }

        if (string.IsNullOrEmpty(SelectedLogFileName)
            || LogFiles.All(f => f.FileName != SelectedLogFileName))
            SelectLogFile(LogFiles[0].FileName);
    }

    [RelayCommand]
    private void SelectLogFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        SelectedLogFileName = fileName;
        LogTailText = _logViewerService.ReadTail(fileName, 200);
    }

    private void ApplySettings(AppSettings settings)
    {
        OpenOnStartup = settings.OpenOnStartup;
        StartMinimized = settings.StartMinimized;
        SelectedTheme = settings.Theme;
        SelectedLanguage = settings.Language;
        ScanIntervalSeconds = settings.ScanIntervalSeconds;
        NotificationsEnabled = settings.NotificationsEnabled;
        CpuWarnPercent = settings.HealthThresholds.CpuWarn;
        RamWarnPercent = settings.HealthThresholds.RamWarn;
        DiskWarnPercent = settings.HealthThresholds.DiskWarn;
    }
}

public sealed class LogFileListItem
{
    public string FileName { get; init; } = "";
    public string SizeLabel { get; init; } = "";
    public string LastModifiedLabel { get; init; } = "";
}
