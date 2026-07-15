namespace HorosHelp.Core.Models.Network;

public sealed class NetworkAdapterInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public bool IsConnected { get; init; }
    public string InterfaceType { get; init; } = "";
    public string NetshInterfaceName { get; init; } = "";
    public IReadOnlyList<NetworkDetailInfo> Details { get; init; } = [];
}

public sealed class NetworkDetailInfo
{
    public required string Label { get; init; }
    public required string Value { get; init; }
}

public sealed class WlanProfileInfo
{
    public required string Ssid { get; init; }
    public bool IsConnected { get; init; }
    public string StatusLabel { get; init; } = "";
}

public sealed class NetworkDiagnosticResult
{
    public bool Success { get; init; }
    public string Title { get; init; } = "";
    public string Output { get; init; } = "";
}

public sealed class NetworkSnapshot
{
    public IReadOnlyList<NetworkAdapterInfo> Adapters { get; init; } = [];
    public IReadOnlyList<WlanProfileInfo> WlanProfiles { get; init; } = [];
    public bool IsMockData { get; init; }
}

public sealed class LatencyMeasurement
{
    public bool Success { get; init; }
    public long RoundtripMs { get; init; }
    public string Host { get; init; } = "";
}
