using Avalonia.Controls;
using Avalonia.Input;

namespace HorosHelp.UI.Views.Shell;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
        TitleBarHost.PointerPressed += OnTitleBarPointerPressed;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            window?.BeginMoveDrag(e);
        }
    }
}
