namespace HorosHelp.Core.Services.Windows;

public enum WindowsServiceState
{
    Unknown,
    Stopped,
    Running,
    Paused,
    StartPending,
    StopPending,
}

public sealed class ServiceOperationResult
{
    public bool Success { get; init; }
    public string ServiceName { get; init; } = "";
    public WindowsServiceState StateBefore { get; init; }
    public WindowsServiceState StateAfter { get; init; }
    public string Message { get; init; } = "";
}

public interface IWindowsServiceController
{
    WindowsServiceState GetState(string serviceName);

    Task<ServiceOperationResult> StopAsync(string serviceName, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    Task<ServiceOperationResult> StartAsync(string serviceName, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
}
