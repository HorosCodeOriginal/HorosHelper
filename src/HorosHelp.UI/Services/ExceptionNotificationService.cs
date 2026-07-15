using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HorosHelp.UI.ViewModels.Dialogs;
using HorosHelp.UI.Views.Dialogs;

namespace HorosHelp.UI.Services;

public interface IExceptionNotificationService
{
    void ShowError(string title, string message);
}

public sealed class ExceptionNotificationService : IExceptionNotificationService
{
    public void ShowError(string title, string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            var viewModel = new UacPromptViewModel(message, "Schließen", titleOverride: title, showShield: false);
            var window = new Window
            {
                Title = title,
                Width = 480,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false,
                Background = Avalonia.Media.Brushes.Transparent,
                Content = new UacPromptView { DataContext = viewModel },
            };

            var tcs = new TaskCompletionSource<bool>();
            viewModel.CloseRequested += _ =>
            {
                tcs.TrySetResult(true);
                window.Close();
            };
            window.Closed += (_, _) => tcs.TrySetResult(false);

            var owner = GetMainWindow();
            if (owner is not null)
                await window.ShowDialog(owner);
            else
                window.Show();

            await tcs.Task;
        });
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}
