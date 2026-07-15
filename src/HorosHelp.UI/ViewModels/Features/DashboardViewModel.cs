using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Settings;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class KpiCardItem
{
    public string Label         { get; init; } = "";
    public string Value         { get; init; } = "";
    public IBrush ValueBrush    { get; init; } = Brushes.White;
    public string Unit          { get; init; } = "";
    public IBrush DotBrush      { get; init; } = Brushes.Gray;
    public bool   ShowSparkline { get; init; }
    public string SparklineData { get; init; } = "";
    public bool   ShowProgress  { get; init; }
    public double ProgressValue { get; init; }
    public IBrush ProgressBrush { get; init; } = Brushes.Gray;
    public string DetailText    { get; init; } = "";
    public bool   ShowDetail    { get; init; }
}

public sealed class WarningItem
{
    public string Title    { get; init; } = "";
    public string Subtitle { get; init; } = "";
}

public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private const int SparklinePointCount = 12;

    private static readonly IBrush BrushGreen = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushAmber = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush BrushRed   = new SolidColorBrush(Color.Parse("#EF4444"));

    private readonly ISystemHealthService _healthService;
    private readonly ISettingsService _settingsService;
    private Timer _pollTimer;
    private double _lastPollIntervalSeconds = -1;
    private readonly List<double> _cpuHistory = [];
    private readonly List<double> _networkHistory = [];
    private bool _disposed;

    public string Title    => "System-Gesundheit";
    public string Subtitle => "Übersicht Ihres Systems";
    public string HealthScoreLabel => "Gesundheits-Score";

    public ObservableCollection<KpiCardItem> KpiCards { get; } = [];
    public ObservableCollection<WarningItem> Warnings { get; } = [];

    [ObservableProperty]
    private double _healthScoreValue;

    [ObservableProperty]
    private string _healthScoreSubText = "Wird geladen …";

    [ObservableProperty]
    private string _warningsTitle = "Keine Warnungen";

    public DashboardViewModel(ISystemHealthService healthService, ISettingsService settingsService)
        : this(healthService, settingsService, null)
    {
    }

    internal DashboardViewModel(
        ISystemHealthService healthService,
        ISettingsService settingsService,
        SystemHealthThresholds? thresholdsOverride)
    {
        _healthService = healthService;
        _settingsService = settingsService;
        _thresholdsOverride = thresholdsOverride;

        RefreshFromService();
        _pollTimer = CreatePollTimer(_settingsService.Current.ScanIntervalSeconds);
        _lastPollIntervalSeconds = _settingsService.Current.ScanIntervalSeconds;
    }

    private readonly SystemHealthThresholds? _thresholdsOverride;

    public void Dispose()
    {
        if (_disposed)
            return;

        _pollTimer.Dispose();
        _disposed = true;
    }

    private void RefreshFromService()
    {
        if (_disposed)
            return;

        EnsurePollInterval();

        var snapshot = _healthService.GetSnapshot();
        ApplySnapshot(snapshot);
    }

    private void EnsurePollInterval()
    {
        var intervalSeconds = _settingsService.Current.ScanIntervalSeconds;
        if (Math.Abs(intervalSeconds - _lastPollIntervalSeconds) < 0.01)
            return;

        _lastPollIntervalSeconds = intervalSeconds;
        _pollTimer.Dispose();
        _pollTimer = CreatePollTimer(intervalSeconds);
    }

    private Timer CreatePollTimer(double intervalSeconds)
    {
        var interval = TimeSpan.FromSeconds(Math.Clamp(intervalSeconds, 1, 60));
        return new Timer(
            _ => Dispatcher.UIThread.Post(RefreshFromService),
            null,
            interval,
            interval);
    }

    private SystemHealthThresholds GetThresholds() =>
        _thresholdsOverride ?? AppSettingsMapper.ToHealthThresholds(_settingsService.Current.HealthThresholds);

    private void ApplySnapshot(SystemHealthSnapshot snapshot)
    {
        AppendHistory(_cpuHistory, snapshot.CpuPercent);
        AppendHistory(_networkHistory, snapshot.NetworkOk ? 20 : 80);

        HealthScoreValue = snapshot.HealthScore;
        HealthScoreSubText = SystemHealthAnalyzer.GetHealthSubText(snapshot.HealthScore);

        UpdateKpiCards(snapshot);
        UpdateWarnings(snapshot.Warnings);
    }

    private void UpdateKpiCards(SystemHealthSnapshot snapshot)
    {
        var cards = new[]
        {
            BuildCpuCard(snapshot),
            BuildRamCard(snapshot),
            BuildDiskCard(snapshot),
            BuildNetworkCard(snapshot),
        };

        EnsureKpiCardCount(cards.Length);
        for (var i = 0; i < cards.Length; i++)
            KpiCards[i] = cards[i];
    }

    private void EnsureKpiCardCount(int count)
    {
        while (KpiCards.Count < count)
            KpiCards.Add(new KpiCardItem());

        while (KpiCards.Count > count)
            KpiCards.RemoveAt(KpiCards.Count - 1);
    }

    private KpiCardItem BuildCpuCard(SystemHealthSnapshot snapshot)
    {
        var thresholds = GetThresholds();
        var (dot, progress) = StatusBrushes(snapshot.CpuPercent, thresholds.CpuWarningPercent, thresholds.CpuCriticalPercent);

        return new KpiCardItem
        {
            Label = "CPU",
            Value = $"{snapshot.CpuPercent:F0}%",
            ValueBrush = Brushes.White,
            Unit = "Auslastung",
            DotBrush = dot,
            ShowSparkline = true,
            SparklineData = BuildSparklineData(_cpuHistory),
            ShowProgress = false,
            ProgressBrush = progress,
        };
    }

    private KpiCardItem BuildRamCard(SystemHealthSnapshot snapshot)
    {
        var thresholds = GetThresholds();
        var (dot, progress) = StatusBrushes(snapshot.RamPercent, thresholds.RamWarningPercent, thresholds.RamCriticalPercent);

        return new KpiCardItem
        {
            Label = "RAM",
            Value = $"{snapshot.RamPercent:F0}%",
            ValueBrush = Brushes.White,
            Unit = "Auslastung",
            DotBrush = dot,
            ShowSparkline = false,
            ShowProgress = true,
            ProgressValue = snapshot.RamPercent,
            ProgressBrush = progress,
            DetailText = $"{snapshot.RamUsedGb:F1} GB / {snapshot.RamTotalGb:F1} GB",
            ShowDetail = true,
        };
    }

    private KpiCardItem BuildDiskCard(SystemHealthSnapshot snapshot)
    {
        var thresholds = GetThresholds();
        var (dot, progress) = StatusBrushes(snapshot.DiskPercent, thresholds.DiskWarningPercent, thresholds.DiskCriticalPercent);

        var detailText = snapshot.DiskVolumes.Count > 1
            ? string.Join(" · ", snapshot.DiskVolumes.Select(v => $"{v.DriveLetter}: {v.UsedGb}/{v.TotalGb} GB"))
            : $"{snapshot.DiskUsedGb:F0} GB / {snapshot.DiskTotalGb:F0} GB";

        return new KpiCardItem
        {
            Label = "Festplatte",
            Value = $"{snapshot.DiskPercent:F0}%",
            ValueBrush = Brushes.White,
            Unit = "Auslastung",
            DotBrush = dot,
            ShowSparkline = false,
            ShowProgress = true,
            ProgressValue = snapshot.DiskPercent,
            ProgressBrush = progress,
            DetailText = detailText,
            ShowDetail = true,
        };
    }

    private KpiCardItem BuildNetworkCard(SystemHealthSnapshot snapshot)
    {
        var ok = snapshot.NetworkOk;

        return new KpiCardItem
        {
            Label = "Netzwerk",
            Value = ok ? "OK" : "Offline",
            ValueBrush = ok ? BrushGreen : BrushRed,
            Unit = "Status",
            DotBrush = ok ? BrushGreen : BrushRed,
            ShowSparkline = true,
            SparklineData = BuildSparklineData(_networkHistory),
            ShowProgress = false,
        };
    }

    private void UpdateWarnings(IReadOnlyList<SystemHealthWarning> warnings)
    {
        Warnings.Clear();

        foreach (var warning in warnings)
        {
            Warnings.Add(new WarningItem
            {
                Title = warning.Title,
                Subtitle = warning.Subtitle,
            });
        }

        WarningsTitle = warnings.Count switch
        {
            0 => "Keine Warnungen",
            1 => "1 Warnung",
            _ => $"{warnings.Count} Warnungen",
        };
    }

    private static (IBrush Dot, IBrush Progress) StatusBrushes(double percent, double warning, double critical)
    {
        if (percent >= critical)
            return (BrushRed, BrushRed);

        if (percent >= warning)
            return (BrushAmber, BrushAmber);

        return (BrushGreen, BrushGreen);
    }

    private static void AppendHistory(List<double> history, double value)
    {
        history.Add(value);
        while (history.Count > SparklinePointCount)
            history.RemoveAt(0);
    }

    private static string BuildSparklineData(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return string.Empty;

        var step = values.Count <= 1 ? 0 : 218d / (values.Count - 1);
        var parts = new string[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            var x = Math.Round(i * step, 0, MidpointRounding.AwayFromZero);
            var y = Math.Round(4 + (values[i] / 100d) * 20, 0, MidpointRounding.AwayFromZero);
            parts[i] = string.Create(CultureInfo.InvariantCulture, $"{x},{y}");
        }

        return string.Join(' ', parts);
    }
}
