namespace HorosHelp.Core.Services.Knowledge;

public sealed record LauncherCommand(string FileName, string Arguments, string Description);

public interface IShell32Launcher
{
    LauncherCommand? ResolveControlPanelCommand(string appletId);

    bool TryLaunch(string deepLink);
}

public static class Shell32AppletCatalog
{
    private static readonly Dictionary<string, (string Cpl, string Description)> Map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["appwiz"] = ("appwiz.cpl", "Programme und Features"),
            ["sysdm"] = ("sysdm.cpl", "Systemeigenschaften"),
            ["timedate"] = ("timedate.cpl", "Datum und Uhrzeit"),
            ["main"] = ("main.cpl", "Maus"),
            ["desk"] = ("desk.cpl", "Anzeige"),
            ["inetcpl"] = ("inetcpl.cpl", "Internetoptionen"),
            ["powercfg"] = ("powercfg.cpl", "Energieoptionen"),
            ["firewall"] = ("firewall.cpl", "Windows-Firewall"),
            ["bthprops"] = ("bthprops.cpl", "Bluetooth"),
            ["joy"] = ("joy.cpl", "Gamecontroller"),
            ["mmsys"] = ("mmsys.cpl", "Sound"),
            ["ncpa"] = ("ncpa.cpl", "Netzwerkverbindungen"),
            ["telephon"] = ("telephon.cpl", "Telefon und Modem"),
            ["intl"] = ("intl.cpl", "Region"),
            ["access"] = ("access.cpl", "Erleichterte Bedienung"),
            ["sticpl"] = ("sticpl.cpl", "Scanners und Kameras"),
            ["hdwwiz"] = ("hdwwiz.cpl", "Geräte-Manager"),
        };

    public static IReadOnlyDictionary<string, (string Cpl, string Description)> Applets => Map;

    public static string BuildRundllArguments(string cplFile) =>
        $"shell32.dll,Control_RunDLL {cplFile}";
}

public sealed class Shell32Launcher : IShell32Launcher
{
    public LauncherCommand? ResolveControlPanelCommand(string appletId)
    {
        if (string.IsNullOrWhiteSpace(appletId))
            return null;

        var key = appletId.StartsWith("control:", StringComparison.OrdinalIgnoreCase)
            ? appletId["control:".Length..]
            : appletId;

        if (!Shell32AppletCatalog.Applets.TryGetValue(key, out var applet))
            return null;

        return new LauncherCommand(
            "rundll32.exe",
            Shell32AppletCatalog.BuildRundllArguments(applet.Cpl),
            applet.Description);
    }

    public bool TryLaunch(string deepLink)
    {
        var command = ResolveControlPanelCommand(deepLink);
        if (command is null)
            return false;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = command.FileName,
            Arguments = command.Arguments,
            UseShellExecute = true,
        });

        return true;
    }
}

public interface IWindowsSettingsLauncher
{
    bool TryLaunch(string deepLink);
}

public sealed class WindowsSettingsLauncher : IWindowsSettingsLauncher
{
    private readonly IShell32Launcher _shell32Launcher;

    public WindowsSettingsLauncher(IShell32Launcher shell32Launcher)
    {
        _shell32Launcher = shell32Launcher;
    }

    public bool TryLaunch(string deepLink)
    {
        if (string.IsNullOrWhiteSpace(deepLink))
            return false;

        if (deepLink.StartsWith("control:", StringComparison.OrdinalIgnoreCase))
            return _shell32Launcher.TryLaunch(deepLink);

        if (deepLink.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(deepLink)
            {
                UseShellExecute = true,
            });
            return true;
        }

        if (deepLink.StartsWith("control.exe", StringComparison.OrdinalIgnoreCase))
        {
            var parts = deepLink.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = parts[0],
                Arguments = parts.Length > 1 ? parts[1] : "",
                UseShellExecute = true,
            });
            return true;
        }

        return _shell32Launcher.TryLaunch(deepLink);
    }
}
