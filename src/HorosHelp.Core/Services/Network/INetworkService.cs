using HorosHelp.Core.Models.Network;

namespace HorosHelp.Core.Services.Network;

public interface INetworkService
{
    NetworkSnapshot GetSnapshot();

    Task<LatencyMeasurement> MeasureLatencyAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default);

    Task<NetworkDiagnosticResult> PingAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default);

    Task<NetworkDiagnosticResult> TracertAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default);

    Task<NetworkDiagnosticResult> DnsLookupAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default);

    Task<NetworkDiagnosticResult> RenewIpAsync(CancellationToken cancellationToken = default);
}
