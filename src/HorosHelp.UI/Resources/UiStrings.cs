using System.Globalization;
using System.Resources;

namespace HorosHelp.UI.Resources;

/// <summary>Localized UI strings — Shell nav + Copilot settings. Remaining German UI is intentional MVP.</summary>
public static class UiStrings
{
    private static readonly ResourceManager ResourceManager = new(
        "HorosHelp.UI.Resources.Strings",
        typeof(UiStrings).Assembly);

    public static string Get(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;

    public static string NavDashboard => Get("Nav_Dashboard");
    public static string NavProblemFixer => Get("Nav_ProblemFixer");
    public static string NavWissen => Get("Nav_Wissen");
    public static string NavSpeicher => Get("Nav_Speicher");
    public static string NavStartup => Get("Nav_Startup");
    public static string NavNetzwerk => Get("Nav_Netzwerk");
    public static string NavSicherheit => Get("Nav_Sicherheit");
    public static string NavApps => Get("Nav_Apps");
    public static string NavBackup => Get("Nav_Backup");
    public static string NavCopilot => Get("Nav_Copilot");
    public static string NavEinstellungen => Get("Nav_Einstellungen");

    public static string CopilotProviderOffline => Get("Copilot_Provider_Offline");
    public static string CopilotProviderOpenAi => Get("Copilot_Provider_OpenAi");
    public static string CopilotProviderOllama => Get("Copilot_Provider_Ollama");
    public static string CopilotSettingsTitle => Get("Copilot_Settings_Title");
    public static string CopilotSettingsSubtitle => Get("Copilot_Settings_Subtitle");
    public static string CopilotBaseUrl => Get("Copilot_BaseUrl");
    public static string CopilotModel => Get("Copilot_Model");
    public static string CopilotApiKey => Get("Copilot_ApiKey");
    public static string CopilotApiKeyHint => Get("Copilot_ApiKey_Hint");

    public static void ApplyCulture(string? languageSetting)
    {
        var cultureName = languageSetting switch
        {
            "English" => "en",
            _ => "de-DE",
        };

        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
}
