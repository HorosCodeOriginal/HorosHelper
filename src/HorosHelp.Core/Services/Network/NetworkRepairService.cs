using HorosHelp.Core.Services.ProblemScan;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Network;

public sealed class NetworkRepairService : INetworkRepairService
{
    private readonly ILogger<NetworkRepairService> _logger;

    public NetworkRepairService(ILogger<NetworkRepairService> logger)
    {
        _logger = logger;
    }

    public Task<NetworkRepairResult> FlushDnsAsync(CancellationToken cancellationToken = default) =>
        RunRepairAsync(RepairCommandBuilder.BuildDnsFlush(), requiresReboot: false, cancellationToken);

    public Task<NetworkRepairResult> ResetWinsockAsync(CancellationToken cancellationToken = default) =>
        RunRepairAsync(RepairCommandBuilder.BuildWinsockReset(), requiresReboot: true, cancellationToken);

    public async Task<NetworkRepairResult> ToggleAdapterAsync(
        string interfaceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(interfaceName))
        {
            return new NetworkRepairResult
            {
                Success = false,
                Title = "Adapter zurücksetzen",
                Message = "Kein Adapter ausgewählt.",
            };
        }

        var disable = await RunRepairAsync(
            RepairCommandBuilder.BuildDisableAdapter(interfaceName),
            requiresReboot: false,
            cancellationToken);

        if (!disable.Success)
        {
            return new NetworkRepairResult
            {
                Success = false,
                Title = "Adapter zurücksetzen",
                Message = disable.Message,
            };
        }

        await Task.Delay(1500, cancellationToken);

        var enable = await RunRepairAsync(
            RepairCommandBuilder.BuildEnableAdapter(interfaceName),
            requiresReboot: false,
            cancellationToken);

        return new NetworkRepairResult
        {
            Success = enable.Success,
            Title = "Adapter zurücksetzen",
            Message = enable.Success
                ? $"Adapter „{interfaceName}“ wurde deaktiviert und wieder aktiviert."
                : enable.Message,
        };
    }

    private async Task<NetworkRepairResult> RunRepairAsync(
        RepairCommandSpec spec,
        bool requiresReboot,
        CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return new NetworkRepairResult
            {
                Success = false,
                Title = spec.Description,
                Message = "Nur unter Windows verfügbar.",
            };
        }

        try
        {
            var result = await RepairProcessRunner.RunAsync(spec, cancellationToken);
            if (!result.Success)
            {
                _logger.LogWarning(
                    "Network repair failed: {Description} exit={ExitCode}",
                    spec.Description,
                    result.ExitCode);

                return new NetworkRepairResult
                {
                    Success = false,
                    Title = spec.Description,
                    Message = $"{spec.Description} fehlgeschlagen (Code {result.ExitCode}).",
                    RequiresReboot = false,
                };
            }

            var message = $"{spec.Description} erfolgreich abgeschlossen.";
            if (requiresReboot)
                message += " Ein Neustart wird empfohlen.";

            return new NetworkRepairResult
            {
                Success = true,
                Title = spec.Description,
                Message = message,
                RequiresReboot = requiresReboot,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Network repair failed: {Description}", spec.Description);
            return new NetworkRepairResult
            {
                Success = false,
                Title = spec.Description,
                Message = $"{spec.Description} fehlgeschlagen.",
            };
        }
    }
}
