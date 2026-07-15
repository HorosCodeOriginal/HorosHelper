using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HorosHelp.UI.ViewModels.Dialogs;
using HorosHelp.UI.Views.Dialogs;

namespace HorosHelp.UI.Services;

public interface IUacDialogService
{
    Task<bool> ConfirmElevationAsync(string message, string actionDescription);

    Task<bool> ConfirmAsync(string title, string message);
}

public sealed class UacDialogService : IUacDialogService
{
    public Task<bool> ConfirmAsync(string title, string message) =>
        ShowConfirmationAsync(title, message, confirmLabel: "Bestätigen", isElevation: false);

    public async Task<bool> ConfirmElevationAsync(string message, string actionDescription) =>
        await ShowConfirmationAsync(
            "Administratorrechte erforderlich",
            $"{message}\n\n{actionDescription}",
            confirmLabel: "Mit UAC fortfahren",
            isElevation: true);

    private static async Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        string confirmLabel,
        bool isElevation)
    {
        var viewModel = isElevation
            ? new UacPromptViewModel(message, confirmLabel)
            : new UacPromptViewModel(message, confirmLabel, titleOverride: title, showShield: false);

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
