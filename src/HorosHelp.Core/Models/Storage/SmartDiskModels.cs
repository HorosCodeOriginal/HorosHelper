namespace HorosHelp.Core.Models.Storage;

public enum SmartHealthStatus
{
    Ok,
    Warning,
    Unknown,
}

public sealed class DriveSmartInfo
{
    public required string DriveLetter { get; init; }
    public required string Model { get; init; }
    public SmartHealthStatus Status { get; init; }
    public string StatusLabel { get; init; } = "Unbekannt";
    public string DetailText { get; init; } = "";
}
