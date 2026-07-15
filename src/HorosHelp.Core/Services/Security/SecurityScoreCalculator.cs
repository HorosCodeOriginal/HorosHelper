namespace HorosHelp.Core.Services.Security;

public sealed class SecurityScoreInput
{
    public bool FirewallEnabled { get; init; } = true;
    public bool DefenderActive { get; init; } = true;
    public bool RealTimeProtectionEnabled { get; init; } = true;
    public bool SecurityUpdatesCurrent { get; init; } = true;
    public bool RecentScan { get; init; } = true;
}

public static class SecurityScoreCalculator
{
    public static int Calculate(SecurityScoreInput input)
    {
        var score = 100.0;

        if (!input.FirewallEnabled)
            score -= 25;

        if (!input.DefenderActive)
            score -= 30;
        else if (!input.RealTimeProtectionEnabled)
            score -= 15;

        if (!input.SecurityUpdatesCurrent)
            score -= 8;

        if (!input.RecentScan)
            score -= 5;

        return (int)Math.Clamp(Math.Round(score), 0, 100);
    }

    public static string GetScoreStatusLabel(int score) => score switch
    {
        >= 90 => "Ausgezeichnet",
        >= 75 => "Gut",
        >= 60 => "Ausreichend",
        _ => "Verbesserung nötig",
    };

    public static string GetScoreStatusSubtext(int score) => score switch
    {
        >= 90 => "Ihr System ist optimal geschützt.",
        >= 75 => "Grundschutz aktiv — einzelne Bereiche prüfen.",
        >= 60 => "Sicherheitslücken möglich — Einstellungen überprüfen.",
        _ => "Dringend handeln — mehrere Schutzkomponenten inaktiv.",
    };

    public static bool HasRecentScan(DateTimeOffset? lastScanUtc) =>
        lastScanUtc.HasValue && lastScanUtc.Value >= DateTimeOffset.UtcNow.AddDays(-7);
}
