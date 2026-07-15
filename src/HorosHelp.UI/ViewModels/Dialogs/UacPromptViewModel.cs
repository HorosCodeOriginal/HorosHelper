using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.UI;

namespace HorosHelp.UI.ViewModels.Dialogs;

public sealed partial class UacPromptViewModel : ObservableObject
{
    public UacPromptViewModel(string message, string actionDescription)
    {
        Message = message;
        ActionDescription = actionDescription;
    }

    public string Message { get; }

    public string ActionDescription { get; }

    public string Title => "Administratorrechte erforderlich";

    public string ShieldGlyph => AdminUiGlyphs.Shield;

    public string BodyText =>
        "Diese Aktion erfordert Administratorrechte. Windows zeigt einen UAC-Dialog, " +
        "um HorosHelper mit erhöhten Rechten neu zu starten.";

    public string ConfirmLabel => "Als Administrator fortfahren";

    public string CancelLabel => "Abbrechen";

    [RelayCommand]
    private void Confirm() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);

    public event Action<bool>? CloseRequested;
}
