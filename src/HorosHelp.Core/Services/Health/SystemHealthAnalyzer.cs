using HorosHelp.Core.Models;

namespace HorosHelp.Core.Services.Health;

public static class SystemHealthAnalyzer
{
    public static int CalculateHealthScore(
        double cpuPercent,
        double ramPercent,
        double diskPercent,
        bool networkOk,
        SystemHealthThresholds thresholds)
    {
        var score = 100.0;

        score -= MetricPenalty(cpuPercent, thresholds.CpuWarningPercent, thresholds.CpuCriticalPercent, 10, 20);
        score -= MetricPenalty(ramPercent, thresholds.RamWarningPercent, thresholds.RamCriticalPercent, 10, 20);
        score -= MetricPenalty(diskPercent, thresholds.DiskWarningPercent, thresholds.DiskCriticalPercent, 15, 25);

        if (!networkOk)
            score -= 30;

        return (int)Math.Clamp(Math.Round(score), 0, 100);
    }

    public static IReadOnlyList<SystemHealthWarning> BuildWarnings(
        double cpuPercent,
        double ramPercent,
        double diskPercent,
        double diskUsedGb,
        double diskTotalGb,
        bool networkOk,
        SystemHealthThresholds thresholds)
    {
        var warnings = new List<SystemHealthWarning>();

        if (cpuPercent >= thresholds.CpuWarningPercent)
        {
            var level = cpuPercent >= thresholds.CpuCriticalPercent ? "kritisch" : "erhöht";
            warnings.Add(new SystemHealthWarning
            {
                Title = "Hohe CPU-Auslastung",
                Subtitle = $"CPU-Auslastung ist {level} ({cpuPercent:F0}%).",
            });
        }

        if (ramPercent >= thresholds.RamWarningPercent)
        {
            var level = ramPercent >= thresholds.RamCriticalPercent ? "kritisch" : "erhöht";
            warnings.Add(new SystemHealthWarning
            {
                Title = "Hohe RAM-Nutzung",
                Subtitle = $"Arbeitsspeicher ist {level} ({ramPercent:F0}% belegt).",
            });
        }

        if (diskPercent >= thresholds.DiskWarningPercent)
        {
            var freePercent = 100 - diskPercent;
            warnings.Add(new SystemHealthWarning
            {
                Title = "Wenig freier Speicherplatz",
                Subtitle = freePercent < 15
                    ? $"Weniger als 15% freier Speicherplatz auf dem Systemlaufwerk ({diskUsedGb:F0} / {diskTotalGb:F0} GB)."
                    : $"Festplatte zu {diskPercent:F0}% belegt ({diskUsedGb:F0} / {diskTotalGb:F0} GB).",
            });
        }

        if (!networkOk)
        {
            warnings.Add(new SystemHealthWarning
            {
                Title = "Keine Netzwerkverbindung",
                Subtitle = "Es wurde keine aktive Netzwerkverbindung erkannt.",
            });
        }

        return warnings;
    }

    public static string GetHealthSubText(int healthScore) => healthScore switch
    {
        >= 80 => "Ihr System ist in gutem Zustand.",
        >= 60 => "Einige Bereiche benötigen Aufmerksamkeit.",
        _ => "Kritische Probleme erkannt — bitte Warnungen prüfen.",
    };

    private static double MetricPenalty(
        double value,
        double warningThreshold,
        double criticalThreshold,
        double warningPenalty,
        double criticalPenalty)
    {
        if (value >= criticalThreshold)
            return criticalPenalty;

        if (value >= warningThreshold)
            return warningPenalty;

        return 0;
    }
}
