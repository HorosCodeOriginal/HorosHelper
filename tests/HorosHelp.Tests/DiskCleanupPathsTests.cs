using HorosHelp.Core.Services.Storage;

namespace HorosHelp.Tests;

public class DiskCleanupPathsTests
{
    [Fact]
    public void WindowsUpdateCachePath_PointsToSoftwareDistributionDownload()
    {
        Assert.Contains("SoftwareDistribution", DiskCleanupPaths.WindowsUpdateCachePath, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("Download", DiskCleanupPaths.WindowsUpdateCachePath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DetectBrowserCaches_ReturnsChromeEdgeFirefoxEntries()
    {
        var caches = DiskCleanupPaths.DetectBrowserCaches(_ => 1024);

        Assert.Contains(caches, c => c.BrowserId == "chrome");
        Assert.Contains(caches, c => c.BrowserId == "edge");
        Assert.Contains(caches, c => c.BrowserId == "firefox");
    }

    [Fact]
    public void DetectBrowserCaches_UsesCustomSizeEstimator()
    {
        const long expected = 42_000;
        var caches = DiskCleanupPaths.DetectBrowserCaches(_ => expected);

        foreach (var cache in caches.Where(c => c.PathExists))
            Assert.Equal(expected, cache.EstimatedSizeBytes);
    }

    [Fact]
    public void DetectBrowserCaches_ChromePathUnderLocalAppData()
    {
        var chrome = DiskCleanupPaths.DetectBrowserCaches().First(c => c.BrowserId == "chrome");

        Assert.Contains("Google", chrome.CachePath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chrome", chrome.CachePath, StringComparison.OrdinalIgnoreCase);
    }
}
