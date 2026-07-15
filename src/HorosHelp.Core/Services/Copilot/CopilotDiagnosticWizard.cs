using HorosHelp.Core.Models.Copilot;

namespace HorosHelp.Core.Services.Copilot;

/// <summary>Guided diagnostic wizard with follow-up questions and optional tool execution.</summary>
public sealed class CopilotDiagnosticWizard
{
    private CopilotDiagnosticState _state = new() { Phase = CopilotDiagnosticPhase.Idle };

    public CopilotDiagnosticState? CurrentState =>
        _state.Phase == CopilotDiagnosticPhase.Idle ? null : _state;

    public bool IsActive => _state.Phase != CopilotDiagnosticPhase.Idle;

    public void Start()
    {
        _state = new CopilotDiagnosticState
        {
            Phase = CopilotDiagnosticPhase.AskSymptom,
            PendingQuestion = "Beschreiben Sie kurz Ihr Problem — z. B. „PC ist langsam“ oder „kein Internet“.",
        };
    }

    public void Cancel()
    {
        _state = new CopilotDiagnosticState { Phase = CopilotDiagnosticPhase.Idle };
    }

    public CopilotWizardStep ProcessMessage(string userMessage)
    {
        var text = userMessage.Trim();
        var lower = text.ToLowerInvariant();

        if (_state.Phase == CopilotDiagnosticPhase.Idle)
        {
            if (ContainsAny(lower, "diagnose", "diagnostik", "hilf mir", "analysiere"))
            {
                Start();
                return Question(_state.PendingQuestion!);
            }

            return CopilotWizardStep.NotHandled;
        }

        return _state.Phase switch
        {
            CopilotDiagnosticPhase.AskSymptom => HandleSymptom(text),
            CopilotDiagnosticPhase.AskCategory => HandleCategory(lower),
            CopilotDiagnosticPhase.ConfirmScan => HandleConfirm(lower),
            _ => CopilotWizardStep.NotHandled,
        };
    }

    public IReadOnlyList<CopilotToolId> GetToolsForCategory(CopilotDiagnosticCategory category) =>
        category switch
        {
            CopilotDiagnosticCategory.Network => [CopilotToolId.RunNetworkPing],
            CopilotDiagnosticCategory.Performance or CopilotDiagnosticCategory.General
                => [CopilotToolId.RunProblemScan],
            CopilotDiagnosticCategory.Storage or CopilotDiagnosticCategory.Security
                => [CopilotToolId.RunProblemScan],
            _ => [CopilotToolId.RunProblemScan],
        };

    public void MarkToolsRunning()
    {
        _state = _state with { Phase = CopilotDiagnosticPhase.RunningTools };
    }

    public CopilotWizardStep CompleteWithResults(IReadOnlyList<string> toolSummaries)
    {
        _state = _state with
        {
            Phase = CopilotDiagnosticPhase.Summarize,
            ToolResults = toolSummaries,
        };

        var lines = new List<string>
        {
            "Diagnose abgeschlossen. Ergebnisse:",
            "",
        };
        lines.AddRange(toolSummaries.Select(s => $"• {s}"));
        lines.Add("");
        lines.Add("Öffnen Sie bei Bedarf Problem-Fixer oder Netzwerk für weitere Schritte.");

        Cancel();
        return CopilotWizardStep.Respond(string.Join(Environment.NewLine, lines), isComplete: true);
    }

    private CopilotWizardStep HandleSymptom(string text)
    {
        _state = _state with
        {
            Phase = CopilotDiagnosticPhase.AskCategory,
            Symptom = text,
            PendingQuestion = "Welcher Bereich trifft am ehesten zu? (Leistung / Netzwerk / Speicher / Sicherheit / Allgemein)",
        };
        return Question(_state.PendingQuestion!);
    }

    private CopilotWizardStep HandleCategory(string lower)
    {
        var category = ClassifyCategory(lower);
        _state = _state with
        {
            Phase = CopilotDiagnosticPhase.ConfirmScan,
            Category = category,
            PendingQuestion = "Soll ich automatisch einen passenden Scan starten? (Ja/Nein)",
        };
        return Question(_state.PendingQuestion!);
    }

    private CopilotWizardStep HandleConfirm(string lower)
    {
        if (ContainsAny(lower, "nein", "no", "abbrechen", "stop"))
        {
            var symptom = _state.Symptom ?? "Ihr Problem";
            Cancel();
            return CopilotWizardStep.Respond(
                $"Verstanden. Für „{symptom}“ empfehle ich die passenden Bereiche in der Sidebar.",
                isComplete: true);
        }

        if (!ContainsAny(lower, "ja", "yes", "ok", "start", "scan"))
            return Question("Bitte antworten Sie mit „Ja“ oder „Nein“.");

        var tools = GetToolsForCategory(_state.Category);
        MarkToolsRunning();
        return CopilotWizardStep.RunTools(tools);
    }

    public static CopilotDiagnosticCategory ClassifyCategory(string lower)
    {
        if (ContainsAny(lower, "netz", "wlan", "internet", "verbind"))
            return CopilotDiagnosticCategory.Network;
        if (ContainsAny(lower, "speicher", "festplatte", "disk", "platz"))
            return CopilotDiagnosticCategory.Storage;
        if (ContainsAny(lower, "sicher", "defender", "firewall", "virus"))
            return CopilotDiagnosticCategory.Security;
        if (ContainsAny(lower, "leistung", "langsam", "performance", "ram", "cpu"))
            return CopilotDiagnosticCategory.Performance;
        if (ContainsAny(lower, "allgemein", "general"))
            return CopilotDiagnosticCategory.General;
        return CopilotDiagnosticCategory.General;
    }

    private static CopilotWizardStep Question(string text) =>
        CopilotWizardStep.Respond(text, isComplete: false);

    private static bool ContainsAny(string text, params string[] terms)
    {
        foreach (var term in terms)
        {
            if (text.Contains(term, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}

public sealed class CopilotWizardStep
{
    public bool Handled { get; init; }
    public string Message { get; init; } = "";
    public bool IsComplete { get; init; }
    public IReadOnlyList<CopilotToolId> ToolsToRun { get; init; } = [];

    public static CopilotWizardStep NotHandled => new() { Handled = false };

    public static CopilotWizardStep Respond(string message, bool isComplete) =>
        new() { Handled = true, Message = message, IsComplete = isComplete };

    public static CopilotWizardStep RunTools(IReadOnlyList<CopilotToolId> tools) =>
        new() { Handled = true, ToolsToRun = tools, IsComplete = false };
}
