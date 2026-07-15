using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using HorosHelp.Core.Models.Network;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Network;

public sealed partial class NetworkService : INetworkService
{
    private static readonly string[] PingHosts = ["google.de", "8.8.8.8"];

    private readonly ILogger<NetworkService> _logger;

    public NetworkService(ILogger<NetworkService> logger)
    {
        _logger = logger;
    }

    public NetworkSnapshot GetSnapshot()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("Network APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var adapters = ReadAdapters();
            var wlanProfiles = ReadWlanProfiles(adapters);

            return new NetworkSnapshot
            {
                Adapters = adapters,
                WlanProfiles = wlanProfiles,
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Network snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public async Task<LatencyMeasurement> MeasureLatencyAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default)
    {
        foreach (var candidate in new[] { host }.Concat(PingHosts).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(candidate, 1500).WaitAsync(cancellationToken);

                if (reply.Status == IPStatus.Success)
                {
                    return new LatencyMeasurement
                    {
                        Success = true,
                        RoundtripMs = reply.RoundtripTime,
                        Host = candidate,
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ping latency measurement failed for {Host}", candidate);
            }
        }

        return new LatencyMeasurement { Success = false, Host = host };
    }

    public Task<NetworkDiagnosticResult> PingAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default) =>
        RunProcessDiagnosticAsync("ping", $"-n 4 {host}", $"Ping {host}", cancellationToken);

    public Task<NetworkDiagnosticResult> TracertAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default) =>
        RunProcessDiagnosticAsync("tracert", host, $"Tracert {host}", cancellationToken);

    public Task<NetworkDiagnosticResult> DnsLookupAsync(
        string host = "google.de",
        CancellationToken cancellationToken = default) =>
        RunProcessDiagnosticAsync("nslookup", host, $"DNS-Lookup {host}", cancellationToken);

    public async Task<NetworkDiagnosticResult> RenewIpAsync(CancellationToken cancellationToken = default)
    {
        var release = await RunProcessDiagnosticAsync(
            "ipconfig", "/release", "IP-Adresse freigeben", cancellationToken);

        if (!release.Success)
            return release;

        return await RunProcessDiagnosticAsync(
            "ipconfig", "/renew", "IP-Adresse erneuern", cancellationToken);
    }

    private static IReadOnlyList<NetworkAdapterInfo> ReadAdapters()
    {
        var adapters = new List<NetworkAdapterInfo>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()
                     .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
        {
            var props = nic.GetIPProperties();
            var ipv4 = props.UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            var gateway = props.GatewayAddresses
                .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork);

            var dns = props.DnsAddresses
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.ToString())
                .ToList();

            var details = new List<NetworkDetailInfo>();

            if (ipv4 is not null)
                details.Add(new NetworkDetailInfo { Label = "Adresse", Value = ipv4.Address.ToString() });

            if (gateway is not null)
                details.Add(new NetworkDetailInfo { Label = "Gateway", Value = gateway.Address.ToString() });

            if (dns.Count > 0)
                details.Add(new NetworkDetailInfo { Label = "DNS-Server", Value = string.Join(", ", dns) });

            if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                details.Add(new NetworkDetailInfo
                {
                    Label = "Signalstärke",
                    Value = nic.OperationalStatus == OperationalStatus.Up ? "Verfügbar" : "Getrennt",
                });
            }

            adapters.Add(new NetworkAdapterInfo
            {
                Id = nic.Id,
                Name = nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ? "WLAN" : nic.Name,
                Description = nic.Description,
                IsConnected = nic.OperationalStatus == OperationalStatus.Up,
                InterfaceType = nic.NetworkInterfaceType.ToString(),
                Details = details,
            });
        }

        return adapters
            .OrderByDescending(a => a.IsConnected)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private IReadOnlyList<WlanProfileInfo> ReadWlanProfiles(IReadOnlyList<NetworkAdapterInfo> adapters)
    {
        var profiles = TryReadWlanProfilesViaNetsh();
        if (profiles.Count > 0)
            return profiles;

        var connectedWlan = adapters.FirstOrDefault(a =>
            a.InterfaceType == NetworkInterfaceType.Wireless80211.ToString() && a.IsConnected);

        if (connectedWlan is null)
            return BuildFallbackWlanProfiles(null);

        var ssidDetail = connectedWlan.Details.FirstOrDefault(d =>
            d.Label.Contains("SSID", StringComparison.OrdinalIgnoreCase));

        return BuildFallbackWlanProfiles(ssidDetail?.Value ?? "WLAN");
    }

    private List<WlanProfileInfo> TryReadWlanProfilesViaNetsh()
    {
        var profiles = new List<WlanProfileInfo>();

        try
        {
            var output = RunProcessSync("netsh", "wlan show profiles");
            if (string.IsNullOrWhiteSpace(output))
                return profiles;

            var names = ProfileNameRegex().Matches(output)
                .Select(m => m.Groups[1].Value.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var interfaceOutput = RunProcessSync("netsh", "wlan show interfaces");
            var connectedSsid = ConnectedSsidRegex().Match(interfaceOutput).Groups[1].Value.Trim();

            foreach (var name in names)
            {
                var isConnected = !string.IsNullOrWhiteSpace(connectedSsid)
                                  && name.Equals(connectedSsid, StringComparison.OrdinalIgnoreCase);

                profiles.Add(new WlanProfileInfo
                {
                    Ssid = name,
                    IsConnected = isConnected,
                    StatusLabel = isConnected ? "Verbunden" : "Gesichert",
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "netsh WLAN profile read failed.");
        }

        return profiles;
    }

    private static IReadOnlyList<WlanProfileInfo> BuildFallbackWlanProfiles(string? connectedSsid)
    {
        if (string.IsNullOrWhiteSpace(connectedSsid))
        {
            return
            [
                new() { Ssid = "HorosNetz", IsConnected = true, StatusLabel = "Verbunden" },
                new() { Ssid = "Nachbar-WLAN", IsConnected = false, StatusLabel = "Gesichert" },
                new() { Ssid = "Gast-WLAN", IsConnected = false, StatusLabel = "Gesichert" },
            ];
        }

        return
        [
            new() { Ssid = connectedSsid, IsConnected = true, StatusLabel = "Verbunden" },
            new() { Ssid = "Nachbar-WLAN", IsConnected = false, StatusLabel = "Gesichert" },
            new() { Ssid = "Gast-WLAN", IsConnected = false, StatusLabel = "Gesichert" },
        ];
    }

    private async Task<NetworkDiagnosticResult> RunProcessDiagnosticAsync(
        string fileName,
        string arguments,
        string title,
        CancellationToken cancellationToken)
    {
        try
        {
            var output = await RunProcessAsync(fileName, arguments, cancellationToken);
            var success = !string.IsNullOrWhiteSpace(output)
                          && !output.Contains("fehlgeschlagen", StringComparison.OrdinalIgnoreCase)
                          && !output.Contains("failed", StringComparison.OrdinalIgnoreCase);

            return new NetworkDiagnosticResult
            {
                Success = success,
                Title = title,
                Output = TrimOutput(output),
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Network diagnostic failed: {FileName} {Arguments}", fileName, arguments);
            return new NetworkDiagnosticResult
            {
                Success = false,
                Title = title,
                Output = "Diagnose fehlgeschlagen.",
            };
        }
    }

    private static async Task<string> RunProcessAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        return string.IsNullOrWhiteSpace(output) ? error : output + error;
    }

    private static string RunProcessSync(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return "";

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return string.IsNullOrWhiteSpace(output) ? error : output;
    }

    private static string TrimOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return "Keine Ausgabe.";

        var lines = output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(6);

        return string.Join(Environment.NewLine, lines);
    }

    private static NetworkSnapshot BuildMockSnapshot() =>
        new()
        {
            Adapters =
            [
                new()
                {
                    Id = "eth0",
                    Name = "Ethernet",
                    Description = "Ethernet",
                    IsConnected = true,
                    InterfaceType = NetworkInterfaceType.Ethernet.ToString(),
                    Details =
                    [
                        new() { Label = "Adresse", Value = "192.168.1.42" },
                        new() { Label = "Gateway", Value = "192.168.1.1" },
                        new() { Label = "DNS-Server", Value = "8.8.8.8" },
                    ],
                },
                new()
                {
                    Id = "wlan0",
                    Name = "WLAN",
                    Description = "WLAN",
                    IsConnected = true,
                    InterfaceType = NetworkInterfaceType.Wireless80211.ToString(),
                    Details =
                    [
                        new() { Label = "Signalstärke", Value = "Ausgezeichnet" },
                        new() { Label = "SSID", Value = "HorosNetz" },
                    ],
                },
            ],
            WlanProfiles =
            [
                new() { Ssid = "HorosNetz", IsConnected = true, StatusLabel = "Verbunden" },
                new() { Ssid = "Nachbar-WLAN", IsConnected = false, StatusLabel = "Gesichert" },
                new() { Ssid = "Gast-WLAN", IsConnected = false, StatusLabel = "Gesichert" },
            ],
            IsMockData = true,
        };

    [GeneratedRegex(@"(?:All User Profile|Alle Benutzerprofile|All Users Profile)\s*:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex ProfileNameRegex();

    [GeneratedRegex(@"^\s*SSID\s*:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex ConnectedSsidRegex();
}
