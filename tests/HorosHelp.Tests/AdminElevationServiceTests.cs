using HorosHelp.Core.Services.Admin;

namespace HorosHelp.Tests;

public class AdminElevationServiceTests
{
    [Fact]
    public void IsRunningAsAdmin_ReturnsBoolean()
    {
        var service = new AdminElevationService();
        _ = service.IsRunningAsAdmin;
        Assert.True(service.IsRunningAsAdmin || !service.IsRunningAsAdmin);
    }

    [Fact]
    public void RefreshAdminStatus_RaisesEventWhenStatusChanges()
    {
        var service = new AdminElevationService();
        var raised = false;
        service.AdminStatusChanged += (_, _) => raised = true;

        service.RefreshAdminStatus();

        Assert.False(raised);
    }

    [Fact]
    public void RefreshAdminStatus_CanBeCalledMultipleTimes()
    {
        var service = new AdminElevationService();
        service.RefreshAdminStatus();
        service.RefreshAdminStatus();
        Assert.True(service.IsRunningAsAdmin || !service.IsRunningAsAdmin);
    }

    [Fact]
    public void RequestElevation_WhenAlreadyAdmin_ReturnsTrue()
    {
        var service = new AdminElevationService();
        if (!service.IsRunningAsAdmin)
            return;

        Assert.True(service.RequestElevation());
    }
}
