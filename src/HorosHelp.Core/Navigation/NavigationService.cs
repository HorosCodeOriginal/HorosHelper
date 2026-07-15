namespace HorosHelp.Core.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly Func<string, object> _viewModelResolver;

    public NavigationService(Func<string, object> viewModelResolver)
    {
        _viewModelResolver = viewModelResolver;
    }

    public object? CurrentViewModel { get; private set; }

    public string? CurrentRoute { get; private set; }

    public event EventHandler? Navigated;

    public void NavigateTo(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);

        if (string.Equals(CurrentRoute, route, StringComparison.Ordinal))
        {
            return;
        }

        CurrentRoute = route;
        CurrentViewModel = _viewModelResolver(route);
        Navigated?.Invoke(this, EventArgs.Empty);
    }
}
