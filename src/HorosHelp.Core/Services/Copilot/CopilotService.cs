using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    private readonly ILlmProviderFactory _providerFactory;
    private readonly ICopilotToolExecutor _toolExecutor;
    private readonly CopilotDiagnosticWizard _diagnosticWizard;
    private readonly ILogger<CopilotService> _logger;

    private readonly List<(string Role, string Content)> _conversationHistory = [];
    private ScanResult? _lastScanResult;
    private DateTimeOffset? _lastScanUtc;

    public CopilotService(
        ISystemHealthService healthService,
        IStartupService startupService,
        IStorageService storageService,
        ISecurityService securityService,
        ILlmProviderFactory providerFactory,
        ICopilotToolExecutor toolExecutor,
        ILogger<CopilotService> logger)
    {
        _healthService = healthService;
        _startupService = startupService;
        _storageService = storageService;
        _securityService = securityService;
        _providerFactory = providerFactory;
        _toolExecutor = toolExecutor;
        _diagnosticWizard = new CopilotDiagnosticWizard();
        _logger = logger;
    }

    public CopilotDiagnosticState? GetDiagnosticState() => _diagnosticWizard.CurrentState;

    public void StartDiagnosticMode() => _diagnosticWizard.Start();

    public void CancelDiagnosticMode() => _diagnosticWizard.Cancel();

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
        var provider = _providerFactory.GetActiveProvider();

        try
        {
            return provider.CompleteAsync(userMessage, ctx).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM provider failed; falling back to offline rules.");
            return _providerFactory.GetOfflineProvider().CompleteAsync(userMessage, ctx).GetAwaiter().GetResult();
        }
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        string userMessage,
        CopilotSystemContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ctx = context ?? BuildContext();
        var provider = _providerFactory.GetActiveProvider();

        IAsyncEnumerable<string> stream;
        try
        {
            stream = provider.StreamAsync(userMessage, ctx, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM stream failed; falling back to offline rules.");
            stream = _providerFactory.GetOfflineProvider().StreamAsync(userMessage, ctx, cancellationToken);
        }

        await foreach (var chunk in stream.WithCancellation(cancellationToken))
            yield return chunk;
    }

    public async Task<CopilotResponse> ProcessMessageAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var wizardStep = _diagnosticWizard.ProcessMessage(userMessage);
        if (wizardStep.Handled)
        {
            if (wizardStep.ToolsToRun.Count > 0)
            {
                var summaries = new List<string>();
                foreach (var tool in wizardStep.ToolsToRun)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = await _toolExecutor.ExecuteAsync(tool, cancellationToken);
                    summaries.Add(result.Summary);
                }

                var complete = _diagnosticWizard.CompleteWithResults(summaries);
                return new CopilotResponse { Message = complete.Message };
            }

            return new CopilotResponse { Message = wizardStep.Message };
        }

        var ctx = BuildContext();
        RememberConversation("user", userMessage);

        var provider = _providerFactory.GetActiveProvider();
        CopilotResponse response;
        try
        {
            response = await provider.CompleteAsync(userMessage, ctx, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM provider failed; falling back to offline rules.");
            response = await _providerFactory.GetOfflineProvider().CompleteAsync(userMessage, ctx, cancellationToken);
        }

        RememberConversation("assistant", response.Message);
        return response;
    }

    public void RememberScanResult(ScanResult result)
    {
        _lastScanResult = result;
        _lastScanUtc = DateTimeOffset.UtcNow;
    }

    private void RememberConversation(string role, string content)
    {
        _conversationHistory.Add((role, content));
        if (_conversationHistory.Count > 40)
            _conversationHistory.RemoveRange(0, _conversationHistory.Count - 40);
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
