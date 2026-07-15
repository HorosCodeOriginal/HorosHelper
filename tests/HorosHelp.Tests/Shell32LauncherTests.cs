using HorosHelp.Core.Services.Knowledge;

namespace HorosHelp.Tests;

public class Shell32LauncherTests
{
    [Theory]
    [InlineData("control:appwiz", "rundll32.exe", "appwiz.cpl")]
    [InlineData("control:hdwwiz", "rundll32.exe", "hdwwiz.cpl")]
    [InlineData("control:sysdm", "rundll32.exe", "sysdm.cpl")]
    public void ResolveControlPanelCommand_MapsKnownApplets(string deepLink, string fileName, string cpl)
    {
        var launcher = new Shell32Launcher();
        var command = launcher.ResolveControlPanelCommand(deepLink);

        Assert.NotNull(command);
        Assert.Equal(fileName, command!.FileName);
        Assert.Contains(cpl, command.Arguments, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveControlPanelCommand_ReturnsNull_ForUnknownApplet()
    {
        var launcher = new Shell32Launcher();
        Assert.Null(launcher.ResolveControlPanelCommand("control:unknown-applet"));
    }

    [Fact]
    public void WindowsSettingsLauncher_RecognizesMsSettingsWithoutExecuting()
    {
        var launcher = new WindowsSettingsLauncher(new Shell32Launcher());
        Assert.False(launcher.TryLaunch(""));
        Assert.False(launcher.TryLaunch("not-a-valid-link"));
    }
}
