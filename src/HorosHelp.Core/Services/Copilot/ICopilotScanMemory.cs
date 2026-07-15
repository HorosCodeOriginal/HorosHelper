using HorosHelp.Core.Models.ProblemScan;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>Stores recent scan results for Copilot context injection.</summary>
public interface ICopilotScanMemory
{
    void RememberScanResult(ScanResult result);
}
