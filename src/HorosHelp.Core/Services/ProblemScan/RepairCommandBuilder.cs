namespace HorosHelp.Core.Services.ProblemScan;

public sealed record RepairCommandSpec(
    string FileName,
    string Arguments,
    string Description);

public static class RepairCommandBuilder
{
    public static RepairCommandSpec BuildDnsFlush() =>
        new("ipconfig", "/flushdns", "DNS-Cache leeren");

    public static RepairCommandSpec BuildWinsockReset() =>
        new("netsh", "winsock reset", "Winsock-Katalog zurücksetzen");
}
