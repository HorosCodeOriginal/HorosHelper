using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.Network;
using HorosHelp.Core.Services.ProblemScan;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>Executes Copilot diagnostic tools (problem scan, network ping).</summary>
public interface ICopilotToolExecutor
{
    Task<CopilotToolResult> ExecuteAsync(CopilotToolId toolId, CancellationToken cancellationToken = default);
}

public sealed class CopilotToolExecutor : ICopilotToolExecutor
{
    private readonly IProblemScannerService _problemScanner;
    private readonly INetworkService _networkService;
    private readonly ICopilotScanMemory _scanMemory;
    private readonly ILogger<CopilotToolExecutor> _logger;

    public CopilotToolExecutor(
        IProblemScannerService problemScanner,
        INetworkService networkService,
        ICopilotScanMemory scanMemory,
        ILogger<CopilotToolExecutor> logger)
    {
        _problemScanner = problemScanner;
        _networkService = networkService;
        _scanMemory = scanMemory;
        _logger = logger;
    }

    public async Task<CopilotToolResult> ExecuteAsync(
        CopilotToolId toolId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return toolId switch
            {
                CopilotToolId.RunProblemScan => await RunProblemScanAsync(cancellationToken),
                CopilotToolId.RunNetworkPing => await RunNetworkPingAsync(cancellationToken),
                _ => new CopilotToolResult
                {
                    ToolId = toolId,
                    Success = false,
                    Summary = "Unbekanntes Werkzeug.",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Copilot tool {Tool} failed.", toolId);
            return new CopilotToolResult
            {
                ToolId = toolId,
                Success = false,
                Summary = $"Werkzeug fehlgeschlagen: {ex.Message}",
            };
        }
    }

    private async Task<CopilotToolResult> RunProblemScanAsync(CancellationToken cancellationToken)
    {
        var result = await _problemScanner.ScanAsync(cancellationToken: cancellationToken);
        _scanMemory.RememberScanResult(result);

        var issues = result.Problems.Count(p => p.Severity != ProblemSeverity.Good);
        var summary = issues == 0
            ? "System-Scan abgeschlossen: Keine Probleme gefunden."
            : $"System-Scan abgeschlossen: {issues} Problem(e) erkannt.";

        return new CopilotToolResult
        {
            ToolId = CopilotToolId.RunProblemScan,
            Success = true,
            Summary = summary,
        };
    }

    private async Task<CopilotToolResult> RunNetworkPingAsync(CancellationToken cancellationToken)
    {
        var ping = await _networkService.PingAsync("8.8.8.8", cancellationToken);
        var detail = string.IsNullOrWhiteSpace(ping.Output) ? ping.Title : ping.Output;
        var summary = ping.Success
            ? $"Netzwerk-Ping erfolgreich: {detail}"
            : $"Netzwerk-Ping fehlgeschlagen: {detail}";

        return new CopilotToolResult
        {
            ToolId = CopilotToolId.RunNetworkPing,
            Success = ping.Success,
            Summary = summary,
        };
    }
}
