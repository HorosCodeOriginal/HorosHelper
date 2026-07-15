using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Models.Storage;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Settings;
using HorosHelp.Core.Services.Storage;
using HorosHelp.UI.ViewModels.Features;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class ViewModelBindingTests
{
    [Fact]
    public void SpeicherViewModel_BindsCleanupChartTotal_AndSmartBadge_FromStorageSnapshot()
    {
        var vm = new SpeicherViewModel(
            new FakeStorageService(2_100_000_000),
            new FakeDiskAnalyzerService(),
            new FakeNavigationService(),
            NullLogger<SpeicherViewModel>.Instance);

        Assert.Contains("GB", vm.CleanupChartTotal, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(vm.DriveCards);
        Assert.Equal("S.M.A.R.T. OK", vm.DriveCards[0].SmartStatusLabel);
        Assert.NotEmpty(vm.CleanupSuggestions);
    }

    [Fact]
    public void SpeicherViewModel_CleanupButtonText_ReflectsReclaimableBytes()
    {
        var vm = new SpeicherViewModel(
            new FakeStorageService(4_200_000_000),
            new FakeDiskAnalyzerService(),
            new FakeNavigationService(),
            NullLogger<SpeicherViewModel>.Instance);

        Assert.Contains("bereinigen", vm.CleanupButtonText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GB", vm.CleanupButtonText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DashboardViewModel_BindsHealthScoreValue_FromHealthServiceSnapshot()
    {
        using var vm = new DashboardViewModel(
            new FakeHealthService(healthScore: 72),
            new FakeSettingsService());

        Assert.Equal(72, vm.HealthScoreValue);
        Assert.False(string.IsNullOrWhiteSpace(vm.HealthScoreSubText));
        Assert.Equal(4, vm.KpiCards.Count);
    }

    private sealed class FakeStorageService : IStorageService
    {
        public FakeStorageService(long totalReclaimable) => TotalReclaimable = totalReclaimable;

        public long TotalReclaimable { get; }

        public StorageSnapshot GetSnapshot()
        {
            var drive = new DriveStorageInfo
            {
                Letter = "C:",
                Label = "Test (C:)",
                DriveType = "SSD",
                FileSystem = "NTFS",
                TotalBytes = 100_000_000_000,
                UsedBytes = 60_000_000_000,
                FreeBytes = 40_000_000_000,
                PercentUsed = 60,
                IsReady = true,
                SmartStatus = SmartHealthStatus.Ok,
                SmartStatusLabel = "OK",
            };

            var cleanup = StorageAnalyzer.BuildCleanupCandidates(new Dictionary<string, (string, long)>
            {
                ["temp"] = ("Temporäre Dateien", TotalReclaimable),
            });

            return new StorageSnapshot
            {
                Drives = [drive],
                Categories = [],
                CleanupCandidates = cleanup,
                TotalReclaimableBytes = TotalReclaimable,
                CleanupChartPercent = 10,
            };
        }

        public Task<StorageCleanupResult> RunSafeCleanupAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new StorageCleanupResult());

        public Task<StorageCleanupResult> EmptyRecycleBinAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new StorageCleanupResult());
    }

    private sealed class FakeDiskAnalyzerService : IDiskAnalyzerService
    {
        public Task<DiskAnalysisResult> ScanAsync(
            string rootPath,
            IProgress<DiskAnalysisProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new DiskAnalysisResult());
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public string CurrentRoute { get; private set; } = NavigationRoutes.Dashboard;

        public object? CurrentViewModel { get; private set; }

        public event EventHandler? Navigated;

        public void NavigateTo(string route) => Navigate(route);

        public void Navigate(string route)
        {
            CurrentRoute = route;
            Navigated?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class FakeHealthService : ISystemHealthService
    {
        public FakeHealthService(int healthScore) => HealthScore = healthScore;

        public int HealthScore { get; set; }

        public void Dispose() { }

        public SystemHealthSnapshot GetSnapshot() =>
            new()
            {
                HealthScore = HealthScore,
                CpuPercent = 10,
                RamPercent = 20,
                RamUsedGb = 4,
                RamTotalGb = 16,
                DiskPercent = 30,
                DiskUsedGb = 100,
                DiskTotalGb = 500,
                NetworkOk = true,
                Warnings = [],
            };
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public AppSettings Current { get; } = new()
        {
            ScanIntervalSeconds = 3600,
        };

        public AppSettings Load() => Current;

        public void Save(AppSettings settings) { }
    }
}
