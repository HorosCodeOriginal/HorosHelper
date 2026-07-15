using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Models.Copilot;
using HorosHelp.Core.Navigation;
using HorosHelp.Core.Services.Copilot;
using HorosHelp.Core.Services.Health;
using Microsoft.Extensions.Logging;

namespace HorosHelp.UI.ViewModels.Features;

public sealed partial class ChatMessageItem : ObservableObject
{
    private static readonly IBrush UserBubbleBg = new SolidColorBrush(Color.Parse("#1E3A5F"));
    private static readonly IBrush AssistantBubbleBg = new SolidColorBrush(Color.Parse("#1E293B"));

    public bool IsUser { get; init; }
    [ObservableProperty] private string _text = "";
    public string Time { get; init; } = "";
    public bool ShowReadReceipt { get; init; }

    public IBrush BubbleBackground => IsUser ? UserBubbleBg : AssistantBubbleBg;
    public bool ShowAssistantIcon => !IsUser;
}

public sealed class ActionCardItem
{
    public CopilotActionId ActionId { get; init; }
    public string IconGlyph { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
}

public sealed class SystemContextCardItem
{
    public string Label { get; init; } = "";
    public string Value { get; init; } = "";
    public string SubValue { get; init; } = "";
    public string StatusText { get; init; } = "";
    public string StatusIcon { get; init; } = "";
    public IBrush StatusBrush { get; init; } = Brushes.Gray;
    public bool ShowSparkline { get; init; }
    public bool ShowProgress { get; init; }
    public double ProgressValue { get; init; }
    public bool ShowLoadingDots { get; init; }
    public bool ShowStatusRow => !string.IsNullOrEmpty(StatusText) && !ShowLoadingDots;
}

public sealed partial class CopilotViewModel : ViewModelBase, IDisposable
{
    private static readonly IBrush BrushGreen = new SolidColorBrush(Color.Parse("#22C55E"));
    private static readonly IBrush BrushAmber = new SolidColorBrush(Color.Parse("#F59E0B"));

    private readonly ICopilotService _copilotService;
    private readonly ISystemHealthService _healthService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<CopilotViewModel> _logger;
    private bool _disposed;

    public string Title => "HorosHelper Copilot";
    public string Subtitle => "Ihr intelligenter System-Assistent";

    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private bool _isTyping;
    [ObservableProperty] private bool _isDiagnosticMode;

    public ObservableCollection<ChatMessageItem> Messages { get; } = [];
    public ObservableCollection<ActionCardItem> ActionCards { get; } = [];
    public ObservableCollection<SystemContextCardItem> SystemContextCards { get; } = [];

    public CopilotViewModel(
        ICopilotService copilotService,
        ISystemHealthService healthService,
        INavigationService navigationService,
        ILogger<CopilotViewModel> logger)
    {
        _copilotService = copilotService;
        _healthService = healthService;
        _navigationService = navigationService;
        _logger = logger;

        _navigationService.Navigated += OnNavigated;
        InitializeWelcome();
        Dispatcher.UIThread.Post(RefreshContext);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _navigationService.Navigated -= OnNavigated;
        _disposed = true;
    }

    [RelayCommand]
    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        var userText = InputText.Trim();
        InputText = "";

        Messages.Add(new ChatMessageItem
        {
            IsUser = true,
            Text = userText,
            Time = DateTime.Now.ToString("HH:mm"),
            ShowReadReceipt = true,
        });

        IsTyping = true;
        try
        {
            var diagnosticState = _copilotService.GetDiagnosticState();
            if (diagnosticState is not null || IsDiagnosticMode)
            {
                await HandleDiagnosticOrWizardAsync(userText);
                return;
            }

            if (ContainsDiagnosticTrigger(userText))
            {
                _copilotService.StartDiagnosticMode();
                IsDiagnosticMode = true;
                await HandleDiagnosticOrWizardAsync(userText);
                return;
            }

            await StreamAssistantReplyAsync(userText);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Copilot send failed.");
            Messages.Add(new ChatMessageItem
            {
                IsUser = false,
                Text = "Entschuldigung — die Analyse ist fehlgeschlagen. Bitte erneut versuchen.",
                Time = DateTime.Now.ToString("HH:mm"),
            });
        }
        finally
        {
            IsTyping = false;
            IsDiagnosticMode = _copilotService.GetDiagnosticState() is not null;
        }
    }

    [RelayCommand]
    private void StartDiagnostic()
    {
        _copilotService.StartDiagnosticMode();
        IsDiagnosticMode = true;
        Messages.Add(new ChatMessageItem
        {
            IsUser = false,
            Text = "Diagnose-Modus gestartet. Beschreiben Sie kurz Ihr Problem.",
            Time = DateTime.Now.ToString("HH:mm"),
        });
    }

    [RelayCommand]
    private void CancelDiagnostic()
    {
        _copilotService.CancelDiagnosticMode();
        IsDiagnosticMode = false;
        Messages.Add(new ChatMessageItem
        {
            IsUser = false,
            Text = "Diagnose-Modus beendet.",
            Time = DateTime.Now.ToString("HH:mm"),
        });
    }

    [RelayCommand]
    private void ExecuteAction(ActionCardItem? card)
    {
        if (card is null)
            return;

        var route = card.ActionId switch
        {
            CopilotActionId.NavigateStartup => NavigationRoutes.Startup,
            CopilotActionId.NavigateStorage => NavigationRoutes.Speicher,
            CopilotActionId.NavigateDashboard => NavigationRoutes.Dashboard,
            CopilotActionId.NavigateProblemFixer => NavigationRoutes.ProblemFixer,
            CopilotActionId.NavigateSecurity => NavigationRoutes.Sicherheit,
            _ => null,
        };

        Messages.Add(new ChatMessageItem
        {
            IsUser = false,
            Text = route is not null
                ? $"Ich öffne jetzt: {card.Title} …"
                : $"Aktion „{card.Title}“ wird vorbereitet …",
            Time = DateTime.Now.ToString("HH:mm"),
        });

        if (route is not null)
            _navigationService.NavigateTo(route);
    }

    private async Task HandleDiagnosticOrWizardAsync(string userText)
    {
        var response = await _copilotService.ProcessMessageAsync(userText);
        Messages.Add(new ChatMessageItem
        {
            IsUser = false,
            Text = response.Message,
            Time = DateTime.Now.ToString("HH:mm"),
        });
        ApplyActionCards(response.Actions);
        RefreshContext();
    }

    private async Task StreamAssistantReplyAsync(string userText)
    {
        var assistantMessage = new ChatMessageItem
        {
            IsUser = false,
            Text = "",
            Time = DateTime.Now.ToString("HH:mm"),
        };
        Messages.Add(assistantMessage);

        var builder = new StringBuilder();
        var context = _copilotService.BuildContext();

        await foreach (var chunk in _copilotService.StreamResponseAsync(userText, context))
        {
            builder.Append(chunk);
            assistantMessage.Text = builder.ToString();
        }

        var response = _copilotService.GenerateResponse(userText, context);
        ApplyActionCards(response.Actions);
        RefreshContext();
    }

    private static bool ContainsDiagnosticTrigger(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains("diagnose", StringComparison.Ordinal)
            || lower.Contains("diagnostik", StringComparison.Ordinal)
            || lower.Contains("hilf mir", StringComparison.Ordinal);
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        if (string.Equals(_navigationService.CurrentRoute, NavigationRoutes.Copilot, StringComparison.Ordinal))
            Dispatcher.UIThread.Post(RefreshContext);
    }

    private void InitializeWelcome()
    {
        var context = _copilotService.BuildContext();
        var greeting = _copilotService.GenerateResponse("Hallo", context);

        Messages.Add(new ChatMessageItem
        {
            IsUser = false,
            Text = greeting.Message,
            Time = DateTime.Now.ToString("HH:mm"),
        });

        ApplyActionCards(greeting.Actions);
    }

    private void RefreshContext()
    {
        var context = _copilotService.BuildContext();
        var health = _healthService.GetSnapshot();

        SystemContextCards.Clear();
        SystemContextCards.Add(new SystemContextCardItem
        {
            Label = "CPU-Auslastung",
            Value = $"{health.CpuPercent:F0}%",
            SubValue = "Live",
            ShowSparkline = true,
        });
        SystemContextCards.Add(new SystemContextCardItem
        {
            Label = "RAM-Auslastung",
            Value = $"{health.RamPercent:F0}%",
            SubValue = $"{health.RamUsedGb:F1} / {health.RamTotalGb:F1} GB",
            ShowProgress = true,
            ProgressValue = health.RamPercent,
        });
        SystemContextCards.Add(new SystemContextCardItem
        {
            Label = "Letzter System-Scan",
            Value = context.LastScanUtc?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "Heute",
            StatusText = context.OpenProblemCount == 0
                ? "Keine Probleme gefunden"
                : $"{context.OpenProblemCount} Probleme offen",
            StatusIcon = context.OpenProblemCount == 0 ? "✓" : "⚠",
            StatusBrush = context.OpenProblemCount == 0 ? BrushGreen : BrushAmber,
        });
        SystemContextCards.Add(new SystemContextCardItem
        {
            Label = "Aktive Prozesse",
            Value = context.ActiveProcessCount.ToString(),
            StatusText = "wird aktualisiert",
            ShowLoadingDots = false,
        });

        if (ActionCards.Count == 0)
            ApplyActionCards(CopilotRuleEngine.BuildDefaultActions(context));
    }

    private void ApplyActionCards(IReadOnlyList<CopilotActionSuggestion> actions)
    {
        ActionCards.Clear();
        foreach (var action in actions)
        {
            ActionCards.Add(new ActionCardItem
            {
                ActionId = action.ActionId,
                IconGlyph = action.IconGlyph,
                Title = action.Title,
                Description = action.Description,
            });
        }
    }
}
