using HorosHelp.Core.Models.Network;

namespace HorosHelp.Core.Services.Network;

public sealed class NetworkRepairResult
{
    public bool Success { get; init; }
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public bool RequiresReboot { get; init; }
}

public interface INetworkRepairService
{
    Task<NetworkRepairResult> FlushDnsAsync(CancellationToken cancellationToken = default);

    Task<NetworkRepairResult> ResetWinsockAsync(CancellationToken cancellationToken = default);

    Task<NetworkRepairResult> ToggleAdapterAsync(
        string interfaceName,
        CancellationToken cancellationToken = default);
}
