using HorosHelp.Core.Services.Security;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public sealed class MockPowerShellQuery : HorosHelp.Core.Services.Windows.IPowerShellQuery
{
    public string ScriptToReturn { get; set; } = """{"PendingSecurityUpdates":2}""";
    public List<string> ExecutedScripts { get; } = [];

    public string Execute(string script)
    {
        ExecutedScripts.Add(script);
        return ScriptToReturn;
    }
}

public class WindowsUpdateServiceTests
{
    [Fact]
    public void ParsePendingSecurityUpdates_ParsesJson()
    {
        Assert.Equal(3, WindowsUpdateStatusParser.ParsePendingSecurityUpdates("""{"PendingSecurityUpdates":3}"""));
        Assert.Equal(0, WindowsUpdateStatusParser.ParsePendingSecurityUpdates("""{"PendingSecurityUpdates":0}"""));
        Assert.Equal(0, WindowsUpdateStatusParser.ParsePendingSecurityUpdates(null));
    }

    [Fact]
    public void GetStatus_UsesPowerShellOutput_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var service = new WindowsUpdateService(
            new MockPowerShellQuery { ScriptToReturn = """{"PendingSecurityUpdates":2}""" },
            NullLogger<WindowsUpdateService>.Instance);

        var status = service.GetStatus();

        Assert.Equal(2, status.PendingSecurityUpdates);
        Assert.False(status.IsCurrent);
    }
}
