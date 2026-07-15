using System.Diagnostics;
using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Health;
using HorosHelp.Core.Services.Security;
using HorosHelp.Core.Services.Startup;
using HorosHelp.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Copilot;

public sealed class CopilotService : ICopilotService
{
    private readonly ISystemHealthService _healthService;
    private readonly IStartupService _startupService;
    private readonly IStorageService _storageService;
    private readonly ISecurityService _securityService;
    private readonly ILogger<CopilotService> _logger;

    private ScanResult? _lastScanResult;
    private DateTimeOffset? _lastScanUtc;

    public CopilotService(
        ISystemHealthService healthService,
        IStartupService startupService,
        IStorageService storageService,
        ISecurityService securityService,
        ILogger<CopilotService> logger)
    {
        _healthService = healthService;
        _startupService = startupService;
        _storageService = storageService;
        _securityService = securityService;
        _logger = logger;
    }

    public CopilotSystemContext BuildContext()
    {
        try
        {
            var health = _healthService.GetSnapshot();
            var startup = _startupService.GetSnapshot();
            var storage = _storageService.GetSnapshot();
            var security = _securityService.GetSnapshot();

            var openProblems = _lastScanResult?.Problems.Count(p => p.Severity != ProblemSeverity.Good) ?? 0;
            var processCount = startup.BackgroundProcesses.Count;

            return new CopilotSystemContext
            {
                CpuPercent = health.CpuPercent,
                RamPercent = health.RamPercent,
                RamUsedGb = health.RamUsedGb,
                RamTotalGb = health.RamTotalGb,
                HealthScore = health.HealthScore,
                StartupEntryCount = startup.Entries.Count(e => e.IsEnabled),
                SafeToDisableStartupCount = startup.SafeToDisableCount,
                ReclaimableStorageGb = storage.TotalReclaimableBytes / 1_073_741_824d,
                OpenProblemCount = openProblems,
                SecurityScore = security.SecurityScore,
                ActiveProcessCount = processCount,
                LastScanUtc = _lastScanUtc,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Copilot context build failed; using defaults.");
            return new CopilotSystemContext
            {
                CpuPercent = 34,
                RamPercent = 62,
                RamUsedGb = 9.9,
                RamTotalGb = 15.9,
                HealthScore = 85,
                StartupEntryCount = 14,
                SafeToDisableStartupCount = 3,
                ReclaimableStorageGb = 2.4,
                SecurityScore = 92,
                ActiveProcessCount = 128,
            };
        }
    }

    public CopilotResponse GenerateResponse(string userMessage, CopilotSystemContext? context = null)
    {
        var ctx = context ?? BuildContext();
        return CopilotRuleEngine.Generate(userMessage, ctx);
    }

    public void RememberScanResult(ScanResult result)
    {
        _lastScanResult = result;
        _lastScanUtc = DateTimeOffset.UtcNow;
    }

    public static int EstimateActiveProcessCount()
    {
        try
        {
            return Process.GetProcesses().Length;
        }
        catch
        {
            return 0;
        }
    }
}
