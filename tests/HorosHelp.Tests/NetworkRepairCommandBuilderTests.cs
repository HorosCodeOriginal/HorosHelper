using HorosHelp.Core.Services.ProblemScan;

namespace HorosHelp.Tests;

public class NetworkRepairCommandBuilderTests
{
    [Theory]
    [InlineData("Ethernet")]
    [InlineData("WLAN")]
    public void BuildDisableAdapter_UsesNetshDisable(string interfaceName)
    {
        var spec = RepairCommandBuilder.BuildDisableAdapter(interfaceName);

        Assert.Equal("netsh", spec.FileName);
        Assert.Contains("admin=disable", spec.Arguments, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(interfaceName, spec.Arguments, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Ethernet")]
    public void BuildEnableAdapter_UsesNetshEnable(string interfaceName)
    {
        var spec = RepairCommandBuilder.BuildEnableAdapter(interfaceName);

        Assert.Equal("netsh", spec.FileName);
        Assert.Contains("admin=enable", spec.Arguments, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(interfaceName, spec.Arguments, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildPingHost_ReturnsPingCommand()
    {
        var spec = RepairCommandBuilder.BuildPingHost("8.8.8.8", count: 2);

        Assert.Equal("ping", spec.FileName);
        Assert.Contains("8.8.8.8", spec.Arguments, StringComparison.Ordinal);
        Assert.Contains("-n 2", spec.Arguments, StringComparison.Ordinal);
    }
}
