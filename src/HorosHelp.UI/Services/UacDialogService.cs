using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HorosHelp.UI.ViewModels.Dialogs;
using HorosHelp.UI.Views.Dialogs;

namespace HorosHelp.UI.Services;

public interface IUacDialogService
{
    Task<bool> ConfirmElevationAsync(string message, string actionDescription);
}

public sealed class UacDialogService : IUacDialogService
{
    public async Task<bool> ConfirmElevationAsync(string message, string actionDescription)
    {
        var viewModel = new UacPromptViewModel(message, actionDescription);
        var window = new Window
        {
            Title = viewModel.Title,
            Width = 460,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Background = Avalonia.Media.Brushes.Transparent,
            Content = new UacPromptView { DataContext = viewModel },
        };

        var tcs = new TaskCompletionSource<bool>();
        viewModel.CloseRequested += confirmed =>
        {
            tcs.TrySetResult(confirmed);
            window.Close();
        };
        window.Closed += (_, _) => tcs.TrySetResult(false);

        var owner = GetMainWindow();
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();

        return await tcs.Task;
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}
