namespace HorosHelp.Core.Navigation;

public static class NavigationRoutes
{
    public const string Dashboard = "dashboard";
    public const string ProblemFixer = "problem-fixer";
    public const string Wissen = "wissen";
    public const string Speicher = "speicher";
    public const string Startup = "startup";
    public const string Netzwerk = "netzwerk";
    public const string Sicherheit = "sicherheit";
    public const string Apps = "apps";
    public const string Backup = "backup";
    public const string Copilot = "copilot";
    public const string Einstellungen = "einstellungen";

    public static IReadOnlyList<string> All { get; } =
    [
        Dashboard,
        ProblemFixer,
        Wissen,
        Speicher,
        Startup,
        Netzwerk,
        Sicherheit,
        Apps,
        Backup,
        Copilot,
        Einstellungen
    ];
}
