using System.Diagnostics;
using System.Net.NetworkInformation;
using HorosHelp.Core.Interop;
using HorosHelp.Core.Models;
using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Health;

public sealed class SystemHealthService : ISystemHealthService
{
    private readonly ILogger<SystemHealthService> _logger;
    private readonly ISettingsService _settingsService;
    private PerformanceCounter? _cpuCounter;
    private bool _cpuCounterPrimed;
    private bool _disposed;

    public SystemHealthService(
        ILogger<SystemHealthService> logger,
        ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
    }

    public SystemHealthSnapshot GetSnapshot()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("System health APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var cpuOk = TryGetCpuPercent(out var cpu);
            var cpuPercent = cpuOk ? cpu : GetFallbackMetric(34);

            var ramOk = TryGetRamMetrics(out var ram);
            var (ramPercent, ramUsedGb, ramTotalGb) = ramOk
                ? ram
                : (62.0, 9.9, 16.0);

            var diskOk = TryGetDiskMetrics(out var disk, out var diskVolumes);
            var (diskPercent, diskUsedGb, diskTotalGb) = diskOk
                ? disk
                : (71.0, 664.0, 931.0);

            var networkOk = TryGetNetworkStatus(out var ok) && ok;

            var usedMock = !cpuOk || !ramOk || !diskOk;
            if (usedMock)
                _logger.LogWarning(
                    "Partial system health read failure (cpu={CpuOk}, ram={RamOk}, disk={DiskOk}); using fallback values where needed.",
                    cpuOk, ramOk, diskOk);

            return BuildSnapshot(cpuPercent, ramPercent, ramUsedGb, ramTotalGb,
                diskPercent, diskUsedGb, diskTotalGb, diskVolumes, networkOk, usedMock);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "System health read failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _cpuCounter?.Dispose();
        _cpuCounter = null;
        _disposed = true;
    }

    private SystemHealthSnapshot BuildMockSnapshot() =>
        BuildSnapshot(34, 62, 9.9, 16, 71, 664, 931, [], true, isMock: true);

    private SystemHealthSnapshot BuildSnapshot(
        double cpuPercent,
        double ramPercent,
        double ramUsedGb,
        double ramTotalGb,
        double diskPercent,
        double diskUsedGb,
        double diskTotalGb,
        IReadOnlyList<DiskVolumeInfo> diskVolumes,
        bool networkOk,
        bool isMock)
    {
        var thresholds = AppSettingsMapper.ToHealthThresholds(_settingsService.Current.HealthThresholds);

        var healthScore = SystemHealthAnalyzer.CalculateHealthScore(
            cpuPercent, ramPercent, diskPercent, networkOk, thresholds);

        var warnings = SystemHealthAnalyzer.BuildWarnings(
            cpuPercent, ramPercent, diskPercent, diskUsedGb, diskTotalGb, networkOk, thresholds);

        return new SystemHealthSnapshot
        {
            CpuPercent = Round(cpuPercent),
            RamPercent = Round(ramPercent),
            RamUsedGb = Round(ramUsedGb, 1),
            RamTotalGb = Round(ramTotalGb, 1),
            DiskPercent = Round(diskPercent),
            DiskUsedGb = Round(diskUsedGb, 0),
            DiskTotalGb = Round(diskTotalGb, 0),
            DiskVolumes = diskVolumes,
            NetworkOk = networkOk,
            HealthScore = healthScore,
            Warnings = warnings,
            IsMockData = isMock,
        };
    }

    private bool TryGetCpuPercent(out double percent)
    {
        percent = 0;

        try
        {
            _cpuCounter ??= new PerformanceCounter("Processor", "% Processor Time", "_Total");

            if (!_cpuCounterPrimed)
            {
                _ = _cpuCounter.NextValue();
                _cpuCounterPrimed = true;
                Thread.Sleep(100);
            }

            percent = Math.Clamp(_cpuCounter.NextValue(), 0, 100);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "PerformanceCounter CPU read failed; trying WMI fallback.");

            try
            {
                percent = GetCpuPercentViaWmi();
                return percent >= 0;
            }
            catch (Exception wmiEx)
            {
                _logger.LogDebug(wmiEx, "WMI CPU read failed.");
                return false;
            }
        }
    }

    private static double GetCpuPercentViaWmi()
    {
        using var searcher = new System.Management.ManagementObjectSearcher(
            "SELECT LoadPercentage FROM Win32_Processor");

        double total = 0;
        var count = 0;

        foreach (var obj in searcher.Get())
        {
            if (obj["LoadPercentage"] is uint load)
            {
                total += load;
                count++;
            }
        }

        return count == 0 ? -1 : Math.Clamp(total / count, 0, 100);
    }

    private static bool TryGetRamMetrics(out (double Percent, double UsedGb, double TotalGb) metrics)
    {
        metrics = default;

        if (!NativeMemoryStatus.TryGetMemoryLoad(out var loadPercent, out var totalBytes, out var availBytes))
            return false;

        var usedBytes = totalBytes - availBytes;
        metrics = (
            loadPercent,
            BytesToGb(usedBytes),
            BytesToGb(totalBytes));
        return true;
    }

    private static bool TryGetDiskMetrics(
        out (double Percent, double UsedGb, double TotalGb) metrics,
        out IReadOnlyList<DiskVolumeInfo> volumes)
    {
        metrics = default;
        volumes = [];

        var readyDrives = DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed && d.TotalSize > 0)
            .ToList();

        if (readyDrives.Count == 0)
            return false;

        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        var systemDrive = readyDrives.FirstOrDefault(d =>
            d.Name.Equals(systemRoot, StringComparison.OrdinalIgnoreCase))
            ?? readyDrives[0];

        var largestDrive = readyDrives
            .OrderByDescending(d => d.TotalSize)
            .First();

        var selected = new List<DriveInfo> { systemDrive };
        if (!largestDrive.Name.Equals(systemDrive.Name, StringComparison.OrdinalIgnoreCase))
            selected.Add(largestDrive);

        volumes = selected
            .Select(ToDiskVolumeInfo)
            .ToList();

        var primary = volumes[0];
        metrics = (primary.Percent, primary.UsedGb, primary.TotalGb);
        return true;
    }

    private static DiskVolumeInfo ToDiskVolumeInfo(DriveInfo drive)
    {
        var used = drive.TotalSize - drive.AvailableFreeSpace;
        var percent = used / (double)drive.TotalSize * 100;

        return new DiskVolumeInfo
        {
            DriveLetter = drive.Name.TrimEnd('\\'),
            Percent = Math.Round(percent, 0, MidpointRounding.AwayFromZero),
            UsedGb = Math.Round(BytesToGb(used), 0, MidpointRounding.AwayFromZero),
            TotalGb = Math.Round(BytesToGb(drive.TotalSize), 0, MidpointRounding.AwayFromZero),
        };
    }

    private static bool TryGetNetworkStatus(out bool networkOk)
    {
        networkOk = false;

        try
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                networkOk = false;
                return true;
            }

            networkOk = NetworkInterface.GetAllNetworkInterfaces()
                .Any(n => n.OperationalStatus == OperationalStatus.Up
                          && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static double GetFallbackMetric(double mockValue) => mockValue;

    private static double BytesToGb(ulong bytes) => bytes / 1024d / 1024d / 1024d;

    private static double BytesToGb(long bytes) => bytes / 1024d / 1024d / 1024d;

    private static double Round(double value, int digits = 0) =>
        Math.Round(value, digits, MidpointRounding.AwayFromZero);
}
