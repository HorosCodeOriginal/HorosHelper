using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.UI;

namespace HorosHelp.UI.ViewModels.Dialogs;

public sealed partial class UacPromptViewModel : ObservableObject
{
    public UacPromptViewModel(
        string message,
        string actionDescription,
        string? titleOverride = null,
        bool showShield = true)
    {
        Message = message;
        ActionDescription = actionDescription;
        Title = titleOverride ?? "Administratorrechte erforderlich";
        ShowShield = showShield;
        ConfirmLabel = showShield ? "Als Administrator fortfahren" : actionDescription;
    }

    public string Message { get; }

    public string ActionDescription { get; }

    public string Title { get; }

    public bool ShowShield { get; }

    public string ShieldGlyph => AdminUiGlyphs.Shield;

    public string BodyText => ShowShield
        ? "Diese Aktion erfordert Administratorrechte. Windows zeigt einen UAC-Dialog, " +
          "um HorosHelper mit erhöhten Rechten neu zu starten."
        : Message;

    public string ConfirmLabel { get; }

    public string CancelLabel => "Abbrechen";

    [RelayCommand]
    private void Confirm() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);

    public event Action<bool>? CloseRequested;
}
