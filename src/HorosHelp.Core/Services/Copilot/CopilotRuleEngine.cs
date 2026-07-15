using System.Globalization;
using HorosHelp.Core.Models.Copilot;

namespace HorosHelp.Core.Services.Copilot;

public static class CopilotRuleEngine
{
    public static CopilotResponse Generate(string userMessage, CopilotSystemContext context)
    {
        var text = userMessage.Trim();
        var lower = text.ToLowerInvariant();

        if (ContainsAny(lower, "langsam", "performance", "leistung", "hängt", "ruckelt"))
            return BuildSlowPcResponse(context);

        if (ContainsAny(lower, "speicher", "festplatte", "platz", "disk", "cleanup", "bereinigen"))
            return BuildStorageResponse(context);

        if (ContainsAny(lower, "ram", "arbeitsspeicher", "speicher voll"))
            return BuildRamResponse(context);

        if (ContainsAny(lower, "startup", "autostart", "start", "boot"))
            return BuildStartupResponse(context);

        if (ContainsAny(lower, "sicherheit", "defender", "firewall", "virus"))
            return BuildSecurityResponse(context);

        if (ContainsAny(lower, "scan", "problem", "fehler", "reparieren"))
            return BuildScanResponse(context);

        if (ContainsAny(lower, "hallo", "hi", "hey", "guten tag"))
            return BuildGreetingResponse(context);

        return BuildDefaultResponse(context);
    }

    public static IReadOnlyList<CopilotActionSuggestion> BuildDefaultActions(CopilotSystemContext context)
    {
        var actions = new List<CopilotActionSuggestion>();

        if (context.SafeToDisableStartupCount > 0)
        {
            actions.Add(new CopilotActionSuggestion
            {
                ActionId = CopilotActionId.NavigateStartup,
                IconGlyph = "🚀",
                Title = "Startup optimieren",
                Description = $"Autostart prüfen — {context.SafeToDisableStartupCount} sichere Deaktivierungen möglich.",
            });
        }

        if (context.ReclaimableStorageGb >= 0.5)
        {
            actions.Add(new CopilotActionSuggestion
            {
                ActionId = CopilotActionId.NavigateStorage,
                IconGlyph = "🗑",
                Title = "Speicher bereinigen",
                Description = $"Bis zu {context.ReclaimableStorageGb:F1} GB freigeben.",
            });
        }

        actions.Add(new CopilotActionSuggestion
        {
            ActionId = CopilotActionId.NavigateDashboard,
            IconGlyph = "▣",
            Title = "RAM-Analyse",
            Description = $"RAM bei {context.RamPercent:F0}% — Dashboard für Details öffnen.",
        });

        return actions;
    }

    private static CopilotResponse BuildSlowPcResponse(CopilotSystemContext context)
    {
        var lines = new List<string>
        {
            "Ich habe Ihr System analysiert. Mögliche Ursachen für Verlangsamung:",
            "",
            $"• {context.StartupEntryCount} Programme im Autostart",
            $"• {context.ReclaimableStorageGb:F1} GB bereinigbarer Speicher",
            $"• CPU-Auslastung: {context.CpuPercent:F0}%",
            $"• RAM-Auslastung: {context.RamPercent:F0}%",
            "",
            "Empfehlung: Autostart optimieren und Speicher bereinigen.",
        };

        return new CopilotResponse
        {
            Message = string.Join(Environment.NewLine, lines),
            Actions = BuildDefaultActions(context),
        };
    }

    private static CopilotResponse BuildStorageResponse(CopilotSystemContext context) =>
        new()
        {
            Message = context.ReclaimableStorageGb >= 0.5
                ? $"Es sind ca. {context.ReclaimableStorageGb:F1} GB bereinigbar. "
                  + "Öffnen Sie den Speicher-Manager für eine sichere Bereinigung."
                : "Der Speicher sieht aktuell gut aus. Trotzdem können Sie den Speicher-Manager für Details öffnen.",
            Actions =
            [
                new()
                {
                    ActionId = CopilotActionId.NavigateStorage,
                    IconGlyph = "🗑",
                    Title = "Speicher bereinigen",
                    Description = "Temp-Dateien und sichere Kandidaten entfernen.",
                },
            ],
        };

    private static CopilotResponse BuildRamResponse(CopilotSystemContext context)
    {
        var status = context.RamPercent >= 85 ? "kritisch hoch" : context.RamPercent >= 70 ? "erhöht" : "normal";

        return new CopilotResponse
        {
            Message = $"Die RAM-Auslastung liegt bei {context.RamPercent:F0}% "
                      + $"({context.RamUsedGb:F1} / {context.RamTotalGb:F1} GB) — Status: {status}. "
                      + $"Aktive Prozesse: ca. {context.ActiveProcessCount}.",
            Actions =
            [
                new()
                {
                    ActionId = CopilotActionId.NavigateDashboard,
                    IconGlyph = "▣",
                    Title = "RAM-Analyse",
                    Description = "Dashboard mit Live-KPIs öffnen.",
                },
                new()
                {
                    ActionId = CopilotActionId.NavigateStartup,
                    IconGlyph = "🚀",
                    Title = "Hintergrundprozesse",
                    Description = "Autostart und Prozesse prüfen.",
                },
            ],
        };
    }

    private static CopilotResponse BuildStartupResponse(CopilotSystemContext context) =>
        new()
        {
            Message = $"Es sind {context.StartupEntryCount} Autostart-Einträge aktiv. "
                      + $"{context.SafeToDisableStartupCount} können laut Heuristik sicher deaktiviert werden.",
            Actions =
            [
                new()
                {
                    ActionId = CopilotActionId.NavigateStartup,
                    IconGlyph = "🚀",
                    Title = "Startup optimieren",
                    Description = "Autostart-Programme verwalten.",
                },
            ],
        };

    private static CopilotResponse BuildSecurityResponse(CopilotSystemContext context) =>
        new()
        {
            Message = $"Sicherheits-Score: {context.SecurityScore}/100. "
                      + "Öffnen Sie die Sicherheits-Zentrale für Firewall, Defender und Echtzeitschutz.",
            Actions =
            [
                new()
                {
                    ActionId = CopilotActionId.NavigateSecurity,
                    IconGlyph = "🛡",
                    Title = "Sicherheit prüfen",
                    Description = "Firewall und Defender-Status anzeigen.",
                },
            ],
        };

    private static CopilotResponse BuildScanResponse(CopilotSystemContext context) =>
        new()
        {
            Message = context.OpenProblemCount > 0
                ? $"Es wurden {context.OpenProblemCount} offene Probleme erkannt. Starten Sie den Problem-Fixer für Reparaturen."
                : "Keine kritischen Probleme im letzten Kontext. Sie können trotzdem einen vollständigen Scan starten.",
            Actions =
            [
                new()
                {
                    ActionId = CopilotActionId.NavigateProblemFixer,
                    IconGlyph = "🔧",
                    Title = "Problem-Fixer",
                    Description = "System scannen und reparieren.",
                },
            ],
        };

    private static CopilotResponse BuildGreetingResponse(CopilotSystemContext context) =>
        new()
        {
            Message = "Hallo! Ich bin HorosHelper Copilot — lokal auf Ihrem System, ohne externe KI. "
                      + $"Aktueller Gesundheits-Score: {context.HealthScore}/100. Wobei kann ich helfen?",
            Actions = BuildDefaultActions(context),
        };

    private static CopilotResponse BuildDefaultResponse(CopilotSystemContext context) =>
        new()
        {
            Message = "Ich kann bei Startup, Speicher, RAM, Sicherheit und Systemproblemen helfen. "
                      + "Fragen Sie z. B. „Warum ist mein PC langsam?“ oder „Speicher bereinigen“.",
            Actions = BuildDefaultActions(context),
        };

    private static bool ContainsAny(string text, params string[] terms) =>
        terms.Any(t => text.Contains(t, StringComparison.Ordinal));

    public static string FormatRelativeBackup(DateTimeOffset? utc)
    {
        if (!utc.HasValue)
            return "Noch kein Backup";

        var diff = DateTimeOffset.UtcNow - utc.Value;
        if (diff.TotalHours < 1)
            return "Letztes Backup: Gerade eben";
        if (diff.TotalHours < 24)
            return $"Letztes Backup: vor {(int)diff.TotalHours} Stunden";
        if (diff.TotalDays < 2)
            return "Letztes Backup: Gestern";
        return $"Letztes Backup: vor {(int)diff.TotalDays} Tagen";
    }

    public static string FormatSizeGb(long bytes)
    {
        if (bytes <= 0)
            return "—";

        return $"{bytes / 1_073_741_824d:F1} GB".Replace(".", ",");
    }
}
