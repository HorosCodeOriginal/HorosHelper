using HorosHelp.Core.Models.Apps;
using HorosHelp.Core.Services.Apps;

namespace HorosHelp.Tests;

public class InstalledAppParserTests
{
    [Fact]
    public void TryParseRegistryEntry_ReturnsFalse_WhenDisplayNameMissing()
    {
        var values = new Dictionary<string, object?>
        {
            ["DisplayVersion"] = "1.0.0",
        };

        var ok = InstalledAppParser.TryParseRegistryEntry("HKLM", "key", values, out var app);

        Assert.False(ok);
        Assert.Null(app);
    }

    [Fact]
    public void TryParseRegistryEntry_ParsesValidEntry()
    {
        var values = new Dictionary<string, object?>
        {
            ["DisplayName"] = "Google Chrome",
            ["DisplayVersion"] = "124.0.6367.91",
            ["Publisher"] = "Google LLC",
            ["InstallDate"] = "20240408",
            ["EstimatedSize"] = 890_000,
            ["UninstallString"] = "\"C:\\Program Files\\Google\\Chrome\\Application\\124.0.6367.91\\Installer\\setup.exe\" --uninstall",
        };

        var ok = InstalledAppParser.TryParseRegistryEntry("HKLM:test", "chrome", values, out var app);

        Assert.True(ok);
        Assert.NotNull(app);
        Assert.Equal("Google Chrome", app!.Name);
        Assert.Equal("124.0.6367.91", app.Version);
        Assert.Equal("08.04.2024", app.InstallDate);
        Assert.Equal("Google LLC", app.Publisher);
        Assert.Equal(890_000, app.EstimatedSizeKb);
        Assert.Contains("setup.exe", app.UninstallString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryParseRegistryEntry_SkipsSystemComponent()
    {
        var values = new Dictionary<string, object?>
        {
            ["DisplayName"] = "Hidden Component",
            ["SystemComponent"] = 1,
        };

        var ok = InstalledAppParser.TryParseRegistryEntry("HKLM", "sys", values, out _);

        Assert.False(ok);
    }

    [Theory]
    [InlineData(2_200_000, "2,1 GB")]
    [InlineData(512_000, "500 MB")]
    [InlineData(0, "—")]
    public void FormatSize_FormatsKilobytes(long kb, string expected)
    {
        Assert.Equal(expected, InstalledAppParser.FormatSize(kb));
    }

    [Fact]
    public void GetInitial_ReturnsUppercaseLetter()
    {
        Assert.Equal("V", InstalledAppParser.GetInitial("Visual Studio Code"));
    }

    [Fact]
    public void TryStartUninstallProcess_ReturnsFalse_ForEmptyCommand()
    {
        var ok = InstalledAppParser.TryStartUninstallProcess("   ", out var error);

        Assert.False(ok);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
