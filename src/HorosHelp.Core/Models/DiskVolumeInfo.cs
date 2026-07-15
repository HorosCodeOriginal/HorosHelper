namespace HorosHelp.Core.Models;

public sealed class DiskVolumeInfo
{
    public string DriveLetter { get; init; } = "";
    public double Percent { get; init; }
    public double UsedGb { get; init; }
    public double TotalGb { get; init; }
}
