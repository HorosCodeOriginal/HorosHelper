namespace HorosHelp.Core.Services.ProblemScan;

/// <summary>
/// Schwellenwerte für Feature 2 (Problem-Fixer), abgeleitet aus Mockup-02:
/// Temp 2,4 GB = Achtung; 14 Startup-Programme = kritisch.
/// </summary>
public sealed class ProblemScannerThresholds
{
    /// <summary>Temp-Dateien ab diesem Wert (GB) → Achtung.</summary>
    public double TempWarningGb { get; init; } = 1.0;

    /// <summary>Temp-Dateien ab diesem Wert (GB) → Kritisch.</summary>
    public double TempCriticalGb { get; init; } = 2.0;

    /// <summary>Startup-Einträge ab dieser Anzahl → Achtung.</summary>
    public int StartupWarningCount { get; init; } = 8;

    /// <summary>Startup-Einträge ab dieser Anzahl → Kritisch.</summary>
    public int StartupCriticalCount { get; init; } = 12;

    public static ProblemScannerThresholds Default { get; } = new();
}
