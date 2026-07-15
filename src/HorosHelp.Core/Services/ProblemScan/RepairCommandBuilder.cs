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

    public static RepairCommandSpec BuildStopWindowsUpdateService() =>
        new("net", "stop wuauserv", "Windows Update-Dienst stoppen");

    public static RepairCommandSpec BuildStartWindowsUpdateService() =>
        new("net", "start wuauserv", "Windows Update-Dienst starten");

    public static RepairCommandSpec BuildSfcScanNow() =>
        new("sfc", "/scannow", "Systemdateien prüfen (SFC)");

    public static RepairCommandSpec BuildDismRestoreHealth() =>
        new("dism", "/Online /Cleanup-Image /RestoreHealth", "Windows-Image reparieren (DISM)");

    public static string GetWindowsUpdateDownloadPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "SoftwareDistribution",
            "Download");

    public static RepairCommandSpec BuildStopWindowsSearchService() =>
        new("net", "stop wsearch", "Windows-Suchdienst stoppen");

    public static RepairCommandSpec BuildStartWindowsSearchService() =>
        new("net", "start wsearch", "Windows-Suchdienst starten");

    public static RepairCommandSpec BuildResetWindowsSearchIndex() =>
        new("powershell", "-NoProfile -Command \"Reset-WindowsSearchIndex -Force\"", "Windows-Suchindex zurücksetzen");

    public static string GetWindowsSearchDataPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Microsoft",
            "Search",
            "Data");

    public static RepairCommandSpec BuildDisableAdapter(string interfaceName) =>
        new(
            "netsh",
            $"interface set interface name=\"{interfaceName}\" admin=disable",
            $"Netzwerkadapter „{interfaceName}“ deaktivieren");

    public static RepairCommandSpec BuildEnableAdapter(string interfaceName) =>
        new(
            "netsh",
            $"interface set interface name=\"{interfaceName}\" admin=enable",
            $"Netzwerkadapter „{interfaceName}“ aktivieren");

    public static RepairCommandSpec BuildPingHost(string host, int count = 4) =>
        new("ping", $"-n {count} {host}", $"Ping {host}");
}
