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

    [Fact]
    public void BuildStopWindowsUpdateService_ReturnsNetStopWuauserv()
    {
        var spec = RepairCommandBuilder.BuildStopWindowsUpdateService();

        Assert.Equal("net", spec.FileName);
        Assert.Equal("stop wuauserv", spec.Arguments);
        Assert.Contains("Windows Update", spec.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildStartWindowsUpdateService_ReturnsNetStartWuauserv()
    {
        var spec = RepairCommandBuilder.BuildStartWindowsUpdateService();

        Assert.Equal("net", spec.FileName);
        Assert.Equal("start wuauserv", spec.Arguments);
        Assert.Contains("Windows Update", spec.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildSfcScanNow_ReturnsSfcScannow()
    {
        var spec = RepairCommandBuilder.BuildSfcScanNow();

        Assert.Equal("sfc", spec.FileName);
        Assert.Equal("/scannow", spec.Arguments);
        Assert.Contains("SFC", spec.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDismRestoreHealth_ReturnsDismRestoreHealth()
    {
        var spec = RepairCommandBuilder.BuildDismRestoreHealth();

        Assert.Equal("dism", spec.FileName);
        Assert.Contains("/RestoreHealth", spec.Arguments, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("DISM", spec.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildStopWindowsSearchService_ReturnsNetStopWsearch()
    {
        var spec = RepairCommandBuilder.BuildStopWindowsSearchService();

        Assert.Equal("net", spec.FileName);
        Assert.Equal("stop wsearch", spec.Arguments);
        Assert.Contains("Suchdienst", spec.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildStartWindowsSearchService_ReturnsNetStartWsearch()
    {
        var spec = RepairCommandBuilder.BuildStartWindowsSearchService();

        Assert.Equal("net", spec.FileName);
        Assert.Equal("start wsearch", spec.Arguments);
    }

    [Fact]
    public void GetWindowsSearchDataPath_PointsToProgramDataSearch()
    {
        var path = RepairCommandBuilder.GetWindowsSearchDataPath();

        Assert.Contains("Search", path, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Data", path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetWindowsUpdateDownloadPath_PointsToSoftwareDistributionDownload()
    {
        var path = RepairCommandBuilder.GetWindowsUpdateDownloadPath();

        Assert.Contains("SoftwareDistribution", path, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("Download", path, StringComparison.OrdinalIgnoreCase);
    }
}
