using Avalonia.Controls;
using HorosHelp.UI.ViewModels.Shell;

namespace HorosHelp.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void BindShell(ShellViewModel shellViewModel)
    {
        DataContext = shellViewModel;

        shellViewModel.MinimizeRequested += () => WindowState = WindowState.Minimized;
        shellViewModel.MaximizeRequested += () =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        shellViewModel.CloseRequested += Close;
    }
}
