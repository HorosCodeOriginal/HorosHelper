using HorosHelp.Core.Services.ProblemScan;

namespace HorosHelp.Tests;

public class RepairCommandBuilderTests
{
    [Fact]
    public void BuildDnsFlush_ReturnsIpconfigFlushDns()
    {
        var spec = RepairCommandBuilder.BuildDnsFlush();

        Assert.Equal("ipconfig", spec.FileName);
        Assert.Equal("/flushdns", spec.Arguments);
        Assert.Contains("DNS", spec.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildWinsockReset_ReturnsNetshWinsockReset()
    {
        var spec = RepairCommandBuilder.BuildWinsockReset();

        Assert.Equal("netsh", spec.FileName);
        Assert.Equal("winsock reset", spec.Arguments);
        Assert.Contains("Winsock", spec.Description, StringComparison.OrdinalIgnoreCase);
    }
}
