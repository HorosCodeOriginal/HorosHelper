using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Health;

public sealed class SystemHealthService : ISystemHealthService
{
    private readonly ILogger<SystemHealthService> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IEventLogService _eventLogService;
    private readonly ISystemMetricsProvider _metricsProvider;

    public SystemHealthService(
        ILogger<SystemHealthService> logger,
        ISettingsService settingsService,
        IEventLogService eventLogService,
        ISystemMetricsProvider metricsProvider)
    {
        _logger = logger;
        _settingsService = settingsService;
        _eventLogService = eventLogService;
        _metricsProvider = metricsProvider;
    }

    public SystemHealthSnapshot GetSnapshot()
    {
        try
        {
            var metrics = _metricsProvider.GetMetrics();
            return BuildSnapshot(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "System health read failed; returning mock snapshot.");
            return BuildSnapshot(new Models.Health.SystemMetricsSnapshot
            {
                CpuPercent = 34,
                RamPercent = 62,
                RamUsedGb = 9.9,
                RamTotalGb = 16,
                DiskPercent = 71,
                DiskUsedGb = 664,
                DiskTotalGb = 931,
                NetworkOk = true,
                IsMockData = true,
            });
        }
    }

    public void Dispose()
    {
        if (_metricsProvider is IDisposable disposable)
            disposable.Dispose();
    }

    internal SystemHealthSnapshot BuildSnapshot(Models.Health.SystemMetricsSnapshot metrics)
    {
        var thresholds = AppSettingsMapper.ToHealthThresholds(_settingsService.Current.HealthThresholds);

        var healthScore = SystemHealthAnalyzer.CalculateHealthScore(
            metrics.CpuPercent, metrics.RamPercent, metrics.DiskPercent, metrics.NetworkOk, thresholds);

        var warnings = SystemHealthAnalyzer.BuildWarnings(
            metrics.CpuPercent, metrics.RamPercent, metrics.DiskPercent,
            metrics.DiskUsedGb, metrics.DiskTotalGb, metrics.NetworkOk, thresholds);

        var eventErrors = _eventLogService.GetRecentErrors();
        warnings = MergeEventLogWarnings(warnings, eventErrors);

        if (eventErrors.Count > 0)
            healthScore = Math.Max(0, healthScore - Math.Min(15, eventErrors.Count * 3));

        return new SystemHealthSnapshot
        {
            CpuPercent = Round(metrics.CpuPercent),
            RamPercent = Round(metrics.RamPercent),
            RamUsedGb = Round(metrics.RamUsedGb, 1),
            RamTotalGb = Round(metrics.RamTotalGb, 1),
            DiskPercent = Round(metrics.DiskPercent),
            DiskUsedGb = Round(metrics.DiskUsedGb, 0),
            DiskTotalGb = Round(metrics.DiskTotalGb, 0),
            DiskVolumes = metrics.DiskVolumes,
            NetworkOk = metrics.NetworkOk,
            HealthScore = healthScore,
            Warnings = warnings,
            IsMockData = metrics.IsMockData,
        };
    }

    private static double Round(double value, int digits = 0) =>
        Math.Round(value, digits, MidpointRounding.AwayFromZero);

    private static IReadOnlyList<SystemHealthWarning> MergeEventLogWarnings(
        IReadOnlyList<SystemHealthWarning> baseWarnings,
        IReadOnlyList<EventLogError> eventErrors)
    {
        if (eventErrors.Count == 0)
            return baseWarnings;

        var merged = new List<SystemHealthWarning>(baseWarnings);
        foreach (var error in eventErrors)
        {
            merged.Add(new SystemHealthWarning
            {
                Title = $"Ereignisprotokoll ({error.LogName}): {error.Source}",
                Subtitle = $"{error.Timestamp:dd.MM.yyyy HH:mm} — {error.Message}",
            });
        }

        return merged;
    }
}
