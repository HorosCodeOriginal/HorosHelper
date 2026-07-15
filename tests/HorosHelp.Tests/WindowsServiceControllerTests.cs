using HorosHelp.Core.Models.Storage;
using HorosHelp.Core.Services.Storage;
using HorosHelp.Core.Services.Windows;

namespace HorosHelp.Tests;

public class WindowsServiceControllerTests
{
    [Fact]
    public void GetState_ReturnsUnknown_OnNonWindows()
    {
        if (OperatingSystem.IsWindows())
            return;

        var controller = new WindowsServiceController(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsServiceController>.Instance);

        Assert.Equal(WindowsServiceState.Unknown, controller.GetState("wuauserv"));
    }

    [Fact]
    public async Task StopAsync_ReturnsFailureMessage_WhenServiceMissing()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var controller = new WindowsServiceController(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsServiceController>.Instance);

        var result = await controller.StopAsync("HorosHelper-NonExistent-Service-XYZ");

        Assert.False(result.Success);
        Assert.Contains("HorosHelper-NonExistent-Service-XYZ", result.Message, StringComparison.Ordinal);
    }
}
