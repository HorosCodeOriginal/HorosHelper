namespace HorosHelp.Core.Models.Settings;

/// <summary>Copilot LLM provider configuration (API keys stored separately via DPAPI).</summary>
public sealed class CopilotSettings
{
    /// <summary>Offline (rule-based), OpenAiCompatible, or Ollama.</summary>
    public string Provider { get; set; } = "Offline";

    public string BaseUrl { get; set; } = "";

    public string Model { get; set; } = "";

    /// <summary>When true, diagnostic wizard may run automated scans.</summary>
    public bool DiagnosticModeEnabled { get; set; } = true;
}
