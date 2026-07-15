namespace HorosHelp.Core.Navigation;

public interface INavigationService
{
    object? CurrentViewModel { get; }

    string? CurrentRoute { get; }

    event EventHandler? Navigated;

    void NavigateTo(string route);
}
