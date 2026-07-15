using Avalonia.Controls;
using Avalonia.Input;
using HorosHelp.UI.ViewModels.Features;

namespace HorosHelp.UI.Views.Features;

public partial class CopilotView : UserControl
{
    public CopilotView()
    {
        InitializeComponent();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not CopilotViewModel vm)
            return;

        vm.SendCommand.Execute(null);
        e.Handled = true;
    }
}
