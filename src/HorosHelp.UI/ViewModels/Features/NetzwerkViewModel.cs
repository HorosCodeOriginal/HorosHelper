using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Network;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class NetworkAdapterItem
{
    public string IconGlyph   { get; init; } = "⎔";
    public string Name        { get; init; } = "";
    public bool   IsConnected { get; init; }
    public string StatusText  => IsConnected ? "Verbunden" : "Getrennt";
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
    private readonly INavigationService _navigationService;
    private readonly ILogger<NetzwerkViewModel> _logger;
    private readonly LatencyHistoryBuffer _latencyHistory = new();
    private readonly Timer _latencyTimer;
    private bool _disposed;
    private bool _isDiagnosing;

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
        INavigationService navigationService,
        ILogger<NetzwerkViewModel> logger)
    {
        _networkService = networkService;
        _navigationService = navigationService;
        _logger = logger;

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
