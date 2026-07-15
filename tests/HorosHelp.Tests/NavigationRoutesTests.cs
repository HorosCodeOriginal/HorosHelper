using HorosHelp.Core.Navigation;

namespace HorosHelp.Tests;

public class NavigationRoutesTests
{
    [Fact]
    public void All_Contains_Eleven_Routes()
    {
        Assert.Equal(11, NavigationRoutes.All.Count);
    }

    [Fact]
    public void NavigationService_Resolves_Einstellungen_ViewModel()
    {
        var service = new NavigationService(_ => new object());

        service.NavigateTo(NavigationRoutes.Einstellungen);

        Assert.Equal(NavigationRoutes.Einstellungen, service.CurrentRoute);
        Assert.NotNull(service.CurrentViewModel);
    }

    [Fact]
    public void All_Contains_Einstellungen_As_Last_Route()
    {
        Assert.Equal(NavigationRoutes.Einstellungen, NavigationRoutes.All[^1]);
    }
}
