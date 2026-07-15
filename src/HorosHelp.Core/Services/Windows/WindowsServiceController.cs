using System.ServiceProcess;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Windows;

public sealed class WindowsServiceController : IWindowsServiceController
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private readonly ILogger<WindowsServiceController> _logger;

    public WindowsServiceController(ILogger<WindowsServiceController> logger)
    {
        _logger = logger;
    }

    public WindowsServiceState GetState(string serviceName)
    {
        if (!OperatingSystem.IsWindows())
            return WindowsServiceState.Unknown;

        try
        {
            using var controller = new ServiceController(serviceName);
            return MapState(controller.Status);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not read service state for {ServiceName}", serviceName);
            return WindowsServiceState.Unknown;
        }
    }

    public Task<ServiceOperationResult> StopAsync(
        string serviceName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default) =>
        ChangeStateAsync(serviceName, desiredRunning: false, timeout, cancellationToken);

    public Task<ServiceOperationResult> StartAsync(
        string serviceName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default) =>
        ChangeStateAsync(serviceName, desiredRunning: true, timeout, cancellationToken);

    private async Task<ServiceOperationResult> ChangeStateAsync(
        string serviceName,
        bool desiredRunning,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return new ServiceOperationResult
            {
                Success = false,
                ServiceName = serviceName,
                Message = "Nur unter Windows verfügbar.",
            };
        }

        var waitTimeout = timeout ?? DefaultTimeout;

        try
        {
            using var controller = new ServiceController(serviceName);
            var stateBefore = MapState(controller.Status);

            if (desiredRunning)
            {
                if (controller.Status == ServiceControllerStatus.Running)
                {
                    return new ServiceOperationResult
                    {
                        Success = true,
                        ServiceName = serviceName,
                        StateBefore = stateBefore,
                        StateAfter = WindowsServiceState.Running,
                        Message = $"Dienst „{serviceName}“ läuft bereits.",
                    };
                }

                controller.Start();
                await controller.WaitForStatusAsync(ServiceControllerStatus.Running, waitTimeout, cancellationToken);
            }
            else
            {
                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    return new ServiceOperationResult
                    {
                        Success = true,
                        ServiceName = serviceName,
                        StateBefore = stateBefore,
                        StateAfter = WindowsServiceState.Stopped,
                        Message = $"Dienst „{serviceName}“ ist bereits gestoppt.",
                    };
                }

                controller.Stop();
                await controller.WaitForStatusAsync(ServiceControllerStatus.Stopped, waitTimeout, cancellationToken);
            }

            var stateAfter = MapState(controller.Status);
            var success = desiredRunning
                ? stateAfter == WindowsServiceState.Running
                : stateAfter == WindowsServiceState.Stopped;

            return new ServiceOperationResult
            {
                Success = success,
                ServiceName = serviceName,
                StateBefore = stateBefore,
                StateAfter = stateAfter,
                Message = success
                    ? $"Dienst „{serviceName}“ {(desiredRunning ? "gestartet" : "gestoppt")}."
                    : $"Dienst „{serviceName}“ konnte nicht {(desiredRunning ? "gestartet" : "gestoppt")} werden.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service {ServiceName} state change failed (running={Running})", serviceName, desiredRunning);
            return new ServiceOperationResult
            {
                Success = false,
                ServiceName = serviceName,
                Message = $"Dienst „{serviceName}“ — Fehler: {ex.Message}",
            };
        }
    }

    private static WindowsServiceState MapState(ServiceControllerStatus status) =>
        status switch
        {
            ServiceControllerStatus.Running => WindowsServiceState.Running,
            ServiceControllerStatus.Stopped => WindowsServiceState.Stopped,
            ServiceControllerStatus.Paused => WindowsServiceState.Paused,
            ServiceControllerStatus.StartPending => WindowsServiceState.StartPending,
            ServiceControllerStatus.StopPending => WindowsServiceState.StopPending,
            _ => WindowsServiceState.Unknown,
        };
}

internal static class ServiceControllerExtensions
{
    public static async Task WaitForStatusAsync(
        this ServiceController controller,
        ServiceControllerStatus targetStatus,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            controller.Refresh();

            if (controller.Status == targetStatus)
                return;

            await Task.Delay(250, cancellationToken);
        }

        controller.Refresh();
        if (controller.Status != targetStatus)
            throw new System.TimeoutException($"Service did not reach {targetStatus} within {timeout.TotalSeconds}s.");
    }
}
