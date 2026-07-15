namespace HorosHelp.Core.Models.Settings;

public sealed class AppSettings
{
    public bool OpenOnStartup { get; set; } = true;
    public bool StartMinimized { get; set; }
    public string Theme { get; set; } = "Dunkel";
    public string Language { get; set; } = "Deutsch";
    public double ScanIntervalSeconds { get; set; } = 2;
    public bool NotificationsEnabled { get; set; } = true;
    public HealthThresholdSettings HealthThresholds { get; set; } = new();
    public List<string> FavoriteArticleIds { get; set; } = [];

    public CopilotSettings Copilot { get; set; } = new();
}

