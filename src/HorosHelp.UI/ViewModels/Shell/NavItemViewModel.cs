using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;
using HorosHelp.Core.Navigation;

namespace HorosHelp.UI.ViewModels.Shell;

public partial class NavItemViewModel : ViewModelBase
{
    private static readonly IBrush AccentBrush = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush MutedBrush  = new SolidColorBrush(Color.Parse("#64748B"));

    private readonly INavigationService _navigationService;
    private readonly Action<string> _onNavigate;

    public NavItemViewModel(
        string route,
        string label,
        string iconGeometry,
        INavigationService navigationService,
        Action<string> onNavigate)
    {
        Route = route;
        Label = label;
        IconGeometry = iconGeometry;
        IconPath = StreamGeometry.Parse(iconGeometry);
        _navigationService = navigationService;
        _onNavigate = onNavigate;
    }

    public string Route { get; }

    public string Label { get; }

    public string IconGeometry { get; }

    public Geometry IconPath { get; }

    [ObservableProperty]
    private bool _isActive;

    public IBrush NavIconForeground => IsActive ? AccentBrush : MutedBrush;

    partial void OnIsActiveChanged(bool value) => OnPropertyChanged(nameof(NavIconForeground));

    [RelayCommand]
    private void Navigate()
    {
        _navigationService.NavigateTo(Route);
        _onNavigate(Route);
    }
}
