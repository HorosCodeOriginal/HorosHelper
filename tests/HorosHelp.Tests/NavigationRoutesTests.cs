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
    public void NavigationService_Resolves_Registered_ViewModel()
    {
        var service = new NavigationService(_ => new object());

        service.NavigateTo(NavigationRoutes.Dashboard);

        Assert.Equal(NavigationRoutes.Dashboard, service.CurrentRoute);
        Assert.NotNull(service.CurrentViewModel);
    }
}
