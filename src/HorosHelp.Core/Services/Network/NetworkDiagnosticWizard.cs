using HorosHelp.Core.Models.Network;

namespace HorosHelp.Core.Services.Network;

public enum DiagnosticWizardStep
{
    Idle,
    DnsCheck,
    PingGateway,
    PingInternet,
    WinsockHint,
    Completed,
    Failed,
}

public sealed class DiagnosticWizardState
{
    public DiagnosticWizardStep Step { get; init; }
    public string StepTitle { get; init; } = "";
    public string StepDescription { get; init; } = "";
    public string ResultText { get; init; } = "";
    public bool StepPassed { get; init; }
    public bool IsRunning { get; init; }
    public int StepIndex { get; init; }
    public int TotalSteps { get; init; } = 4;
}

public sealed class NetworkDiagnosticWizard
{
    private const string InternetPingHost = "8.8.8.8";

    private readonly INetworkService _networkService;

    public NetworkDiagnosticWizard(INetworkService networkService)
    {
        _networkService = networkService;
    }

    public DiagnosticWizardState CurrentState { get; private set; } = new()
    {
        Step = DiagnosticWizardStep.Idle,
        StepTitle = "Diagnose-Assistent",
        StepDescription = "Starten Sie die Schritt-für-Schritt-Netzwerkdiagnose.",
    };

    public async Task<DiagnosticWizardState> RunFullDiagnosticAsync(CancellationToken cancellationToken = default)
    {
        var gateway = ResolveGatewayAddress();

        var dns = await RunStepAsync(
            DiagnosticWizardStep.DnsCheck,
            1,
            "DNS prüfen",
            "Es wird ein DNS-Lookup für google.de durchgeführt …",
            () => _networkService.DnsLookupAsync("google.de", cancellationToken),
            cancellationToken);

        if (!dns.StepPassed)
            return FailAt(dns);

        var gatewayPing = await RunStepAsync(
            DiagnosticWizardStep.PingGateway,
            2,
            "Gateway prüfen",
            gateway is null
                ? "Kein Gateway gefunden — Schritt wird übersprungen."
                : $"Ping zum Gateway {gateway} …",
            async () =>
            {
                if (gateway is null)
                {
                    return new NetworkDiagnosticResult
                    {
                        Success = true,
                        Title = "Gateway",
                        Output = "Kein Gateway konfiguriert — übersprungen.",
                    };
                }

                return await _networkService.PingAsync(gateway, cancellationToken);
            },
            cancellationToken);

        if (!gatewayPing.StepPassed)
            return FailAt(gatewayPing);

        var internet = await RunStepAsync(
            DiagnosticWizardStep.PingInternet,
            3,
            "Internetverbindung prüfen",
            $"Ping zu {InternetPingHost} …",
            () => _networkService.PingAsync(InternetPingHost, cancellationToken),
            cancellationToken);

        if (!internet.StepPassed)
            return FailAt(internet);

        CurrentState = new DiagnosticWizardState
        {
            Step = DiagnosticWizardStep.Completed,
            StepIndex = 4,
            TotalSteps = 4,
            StepTitle = "Winsock-Hinweis",
            StepDescription = "Alle Tests bestanden. Bei anhaltenden Problemen kann ein Winsock-Reset helfen.",
            ResultText = "Diagnose abgeschlossen — Verbindung wirkt stabil.",
            StepPassed = true,
            IsRunning = false,
        };

        return CurrentState;
    }

    public void Reset()
    {
        CurrentState = new DiagnosticWizardState
        {
            Step = DiagnosticWizardStep.Idle,
            StepTitle = "Diagnose-Assistent",
            StepDescription = "Starten Sie die Schritt-für-Schritt-Netzwerkdiagnose.",
        };
    }

    private async Task<DiagnosticWizardState> RunStepAsync(
        DiagnosticWizardStep step,
        int index,
        string title,
        string description,
        Func<Task<NetworkDiagnosticResult>> action,
        CancellationToken cancellationToken)
    {
        CurrentState = new DiagnosticWizardState
        {
            Step = step,
            StepIndex = index,
            TotalSteps = 4,
            StepTitle = title,
            StepDescription = description,
            ResultText = "Wird ausgeführt …",
            IsRunning = true,
        };

        cancellationToken.ThrowIfCancellationRequested();

        var result = await action();
        var passed = result.Success;

        CurrentState = new DiagnosticWizardState
        {
            Step = step,
            StepIndex = index,
            TotalSteps = 4,
            StepTitle = title,
            StepDescription = description,
            ResultText = passed ? SummarizeOutput(result.Output) : $"Fehler: {SummarizeOutput(result.Output)}",
            StepPassed = passed,
            IsRunning = false,
        };

        return CurrentState;
    }

    private DiagnosticWizardState FailAt(DiagnosticWizardState state)
    {
        CurrentState = new DiagnosticWizardState
        {
            Step = DiagnosticWizardStep.Failed,
            StepIndex = state.StepIndex,
            TotalSteps = state.TotalSteps,
            StepTitle = state.StepTitle,
            StepDescription = "Diagnose bei diesem Schritt fehlgeschlagen. Prüfen Sie Adapter und Kabel.",
            ResultText = state.ResultText,
            StepPassed = false,
            IsRunning = false,
        };
        return CurrentState;
    }

    private string? ResolveGatewayAddress()
    {
        var snapshot = _networkService.GetSnapshot();

        foreach (var adapter in snapshot.Adapters.Where(a => a.IsConnected))
        {
            var gateway = adapter.Details
                .FirstOrDefault(d => d.Label.Contains("Gateway", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(gateway?.Value))
                return gateway.Value;
        }

        return null;
    }

    private static string SummarizeOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return "Keine Ausgabe.";

        var line = output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return line ?? output;
    }
}
