using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Network;
using HorosHelp.UI.Services;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class NetworkAdapterItem
{
    public string Id              { get; init; } = "";
    public string NetshName       { get; init; } = "";
    public string IconGlyph       { get; init; } = "⎔";
    public string Name            { get; init; } = "";
    public bool   IsConnected     { get; init; }
    public string StatusText      => IsConnected ? "Verbunden" : "Getrennt";
    public ObservableCollection<NetworkDetailItem> Details { get; init; } = new();
}

public sealed class NetworkDetailItem
{
    public string Label { get; init; } = "";
    public string Value { get; init; } = "";
}

public sealed class WlanNetworkItem
{
    public string Ssid         { get; init; } = "";
    public string StatusLabel  { get; init; } = "";
    public IBrush StatusBrush  { get; init; } = Brushes.Gray;
    public bool   IsConnected  { get; init; }
}

public sealed class DiagnoseToolItem
{
    public string IconGlyph { get; init; } = "●";
    public string Label     { get; init; } = "";
    public string CommandName { get; init; } = "";
}

public sealed partial class NetzwerkViewModel : ViewModelBase, IDisposable
{
    private const int LatencyPollIntervalMs = 2000;

    private static readonly IBrush BrushGreen = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushGray  = new SolidColorBrush(Color.Parse("#64748B"));

    private readonly INetworkService _networkService;
    private readonly INetworkRepairService _networkRepairService;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IUacDialogService _uacDialogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<NetzwerkViewModel> _logger;
    private readonly NetworkDiagnosticWizard _diagnosticWizard;
    private readonly LatencyHistoryBuffer _latencyHistory = new();
    private readonly Timer _latencyTimer;
    private bool _disposed;
    private bool _isDiagnosing;
    private bool _isWizardRunning;
    private bool _isRepairing;

    public string DiagnoseAssistantTitle => "Diagnose-Assistent";
    public string RepairActionsTitle => "Reparatur-Aktionen";

    [ObservableProperty]
    private string _wizardStepTitle = "Diagnose-Assistent";

    [ObservableProperty]
    private string _wizardStepDescription = "Starten Sie die Schritt-für-Schritt-Netzwerkdiagnose.";

    [ObservableProperty]
    private string _wizardResultText = "";

    [ObservableProperty]
    private string _wizardProgressText = "";

    [ObservableProperty]
    private bool _isWizardActive;

    [ObservableProperty]
    private string? _selectedAdapterNetshName;

    public string Title    => "Netzwerk";
    public string Subtitle => "Adapter, Diagnose und Verbindungsstatus.";

    public string DiagnoseTitle     => "Diagnose";
    public string WlanListTitle     => "Verfügbare WLAN-Netzwerke";
    public string LatencyGraphTitle => "Latenz (Live)";

    [ObservableProperty]
    private string _pingResultTitle = "Ping-Ergebnis";

    [ObservableProperty]
    private string _pingResultText = "Wird gemessen …";

    [ObservableProperty]
    private string _latencyGraphPoints = "";

    public ObservableCollection<NetworkAdapterItem> Adapters { get; } = [];
    public ObservableCollection<WlanNetworkItem> WlanNetworks { get; } = [];

    public ObservableCollection<DiagnoseToolItem> DiagnoseTools { get; } = new()
    {
        new() { IconGlyph = "◎", Label = "Ping",        CommandName = "Ping" },
        new() { IconGlyph = "⋯", Label = "Tracert",     CommandName = "Tracert" },
        new() { IconGlyph = "⊕", Label = "DNS-Lookup",  CommandName = "DnsLookup" },
        new() { IconGlyph = "↻", Label = "IP erneuern", CommandName = "RenewIp" },
    };

    public NetzwerkViewModel(
        INetworkService networkService,
        INetworkRepairService networkRepairService,
        IAdminElevationService adminElevationService,
        IUacDialogService uacDialogService,
        INavigationService navigationService,
        ILogger<NetzwerkViewModel> logger)
    {
        _networkService = networkService;
        _networkRepairService = networkRepairService;
        _adminElevationService = adminElevationService;
        _uacDialogService = uacDialogService;
        _navigationService = navigationService;
        _logger = logger;
        _diagnosticWizard = new NetworkDiagnosticWizard(_networkService);

        _navigationService.Navigated += OnNavigated;
        RefreshAdapters();

        _latencyTimer = new Timer(
            _ => Dispatcher.UIThread.Post(() => _ = MeasureLatencyAsync()),
            null,
            LatencyPollIntervalMs,
            LatencyPollIntervalMs);

        Dispatcher.UIThread.Post(() => _ = MeasureLatencyAsync());
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _navigationService.Navigated -= OnNavigated;
        _latencyTimer.Dispose();
        _disposed = true;
    }

    [RelayCommand]
    private async Task StartDiagnosticWizardAsync()
    {
        if (_isWizardRunning)
            return;

        _isWizardRunning = true;
        IsWizardActive = true;
        WizardProgressText = "Schritt 1 von 4";
        WizardResultText = "";

        try
        {
            _diagnosticWizard.Reset();
            var state = await _diagnosticWizard.RunFullDiagnosticAsync();
            ApplyWizardState(state);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Diagnostic wizard failed.");
            WizardResultText = "Diagnose fehlgeschlagen.";
        }
        finally
        {
            _isWizardRunning = false;
        }
    }

    [RelayCommand]
    private async Task FlushDnsAsync()
    {
        await RunRepairAsync(
            "DNS-Cache leeren",
            () => _networkRepairService.FlushDnsAsync(),
            requiresAdmin: true);
    }

    [RelayCommand]
    private async Task ResetWinsockAsync()
    {
        var confirmed = await _uacDialogService.ConfirmAsync(
            "Winsock zurücksetzen",
            "Der Winsock-Katalog wird zurückgesetzt. Ein Neustart wird empfohlen. Fortfahren?");

        if (!confirmed)
            return;

        await RunRepairAsync(
            "Winsock zurücksetzen",
            () => _networkRepairService.ResetWinsockAsync(),
            requiresAdmin: true);
    }

    [RelayCommand]
    private async Task ToggleAdapterAsync()
    {
        var adapterName = SelectedAdapterNetshName
                          ?? Adapters.FirstOrDefault(a => a.IsConnected)?.NetshName;

        if (string.IsNullOrWhiteSpace(adapterName))
        {
            PingResultTitle = "Adapter zurücksetzen";
            PingResultText = "Kein Adapter ausgewählt.";
            return;
        }

        var confirmed = await _uacDialogService.ConfirmAsync(
            "Adapter zurücksetzen",
            $"Der Adapter „{adapterName}“ wird kurz deaktiviert und wieder aktiviert. Fortfahren?");

        if (!confirmed)
            return;

        await RunRepairAsync(
            "Adapter zurücksetzen",
            () => _networkRepairService.ToggleAdapterAsync(adapterName),
            requiresAdmin: true);

        RefreshAdapters();
    }

    [RelayCommand]
    private void SelectAdapter(string? netshName) =>
        SelectedAdapterNetshName = netshName;

    private async Task RunRepairAsync(
        string title,
        Func<Task<NetworkRepairResult>> action,
        bool requiresAdmin)
    {
        if (_isRepairing)
            return;

        if (requiresAdmin && !_adminElevationService.IsRunningAsAdmin)
        {
            var elevate = await _uacDialogService.ConfirmElevationAsync(
                "Diese Aktion benötigt Administratorrechte.",
                "HorosHelper wird mit UAC neu gestartet.");

            if (elevate)
                _adminElevationService.RequestElevation();

            return;
        }

        _isRepairing = true;
        PingResultTitle = title;
        PingResultText = "Wird ausgeführt …";

        try
        {
            var result = await action();
            PingResultTitle = result.Title;
            PingResultText = result.Message;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Network repair failed: {Title}", title);
            PingResultText = $"{title} fehlgeschlagen.";
        }
        finally
        {
            _isRepairing = false;
        }
    }

    private void ApplyWizardState(DiagnosticWizardState state)
    {
        WizardStepTitle = state.StepTitle;
        WizardStepDescription = state.StepDescription;
        WizardResultText = state.ResultText;
        WizardProgressText = state.StepIndex > 0
            ? $"Schritt {state.StepIndex} von {state.TotalSteps}"
            : "";
        IsWizardActive = state.Step != DiagnosticWizardStep.Idle;

        PingResultTitle = state.StepTitle;
        PingResultText = state.ResultText;
    }

    [RelayCommand]
    private async Task PingAsync()
    {
        if (_isDiagnosing)
            return;

        _isDiagnosing = true;
        try
        {
            var result = await _networkService.PingAsync();
            PingResultTitle = result.Title;
            PingResultText = ExtractPingSummary(result.Output);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ping command failed.");
            PingResultText = "Ping fehlgeschlagen.";
        }
        finally
        {
            _isDiagnosing = false;
        }
    }

    [RelayCommand]
    private async Task TracertAsync()
    {
        if (_isDiagnosing)
            return;

        _isDiagnosing = true;
        try
        {
            var result = await _networkService.TracertAsync();
            PingResultTitle = result.Title;
            PingResultText = result.Output;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Tracert command failed.");
            PingResultText = "Tracert fehlgeschlagen.";
        }
        finally
        {
            _isDiagnosing = false;
        }
    }

    [RelayCommand]
    private async Task DnsLookupAsync()
    {
        if (_isDiagnosing)
            return;

        _isDiagnosing = true;
        try
        {
            var result = await _networkService.DnsLookupAsync();
            PingResultTitle = result.Title;
            PingResultText = result.Output;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DNS lookup command failed.");
            PingResultText = "DNS-Lookup fehlgeschlagen.";
        }
        finally
        {
            _isDiagnosing = false;
        }
    }

    [RelayCommand]
    private async Task RenewIpAsync()
    {
        if (_isDiagnosing)
            return;

        _isDiagnosing = true;
        try
        {
            var result = await _networkService.RenewIpAsync();
            PingResultTitle = result.Title;
            PingResultText = result.Output;
            RefreshAdapters();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IP renew command failed.");
            PingResultText = "IP-Erneuerung fehlgeschlagen.";
        }
        finally
        {
            _isDiagnosing = false;
        }
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Netzwerk, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshAdapters);
    }

    private void RefreshAdapters()
    {
        var snapshot = _networkService.GetSnapshot();

        Adapters.Clear();
        foreach (var adapter in snapshot.Adapters)
        {
            var item = new NetworkAdapterItem
            {
                Id = adapter.Id,
                NetshName = adapter.NetshInterfaceName,
                IconGlyph = adapter.InterfaceType.Contains("Wireless", StringComparison.OrdinalIgnoreCase) ? "⌁" : "⎔",
                Name = adapter.Name,
                IsConnected = adapter.IsConnected,
            };

            foreach (var detail in adapter.Details)
            {
                item.Details.Add(new NetworkDetailItem
                {
                    Label = detail.Label,
                    Value = detail.Value,
                });
            }

            Adapters.Add(item);
        }

        WlanNetworks.Clear();
        foreach (var profile in snapshot.WlanProfiles)
        {
            WlanNetworks.Add(new WlanNetworkItem
            {
                Ssid = profile.Ssid,
                StatusLabel = profile.StatusLabel,
                StatusBrush = profile.IsConnected ? BrushGreen : BrushGray,
                IsConnected = profile.IsConnected,
            });
        }
    }

    private async Task MeasureLatencyAsync()
    {
        if (_disposed)
            return;

        try
        {
            var measurement = await _networkService.MeasureLatencyAsync();
            if (measurement.Success)
            {
                _latencyHistory.Add(measurement.RoundtripMs);
                PingResultTitle = "Ping-Ergebnis";
                PingResultText = $"{measurement.Host} — {measurement.RoundtripMs}ms";
            }
            else
            {
                _latencyHistory.Add(LatencyHistoryBuffer.DefaultMaxLatencyMs);
                PingResultText = "Keine Antwort";
            }

            LatencyGraphPoints = _latencyHistory.BuildSparklinePoints();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Latency measurement failed.");
        }
    }

    private static string ExtractPingSummary(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return "Keine Ausgabe.";

        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Contains("ms", StringComparison.OrdinalIgnoreCase)
                && (line.Contains("Zeit", StringComparison.OrdinalIgnoreCase)
                    || line.Contains("time", StringComparison.OrdinalIgnoreCase)))
                return line.Trim();
        }

        return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? output;
    }
}
