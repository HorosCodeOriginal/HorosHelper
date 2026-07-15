using HorosHelp.Core.Services.ProblemScan;

namespace HorosHelp.Core.Services.Storage;

public sealed class BrowserCachePathInfo
{
    public required string BrowserId { get; init; }
    public required string DisplayName { get; init; }
    public required string CachePath { get; init; }
    public long EstimatedSizeBytes { get; init; }
    public bool PathExists { get; init; }
}

public static class DiskCleanupPaths
{
    public static string WindowsUpdateCachePath => RepairCommandBuilder.GetWindowsUpdateDownloadPath();

    public static IReadOnlyList<BrowserCachePathInfo> DetectBrowserCaches(Func<string, long>? sizeEstimator = null)
    {
        var estimator = sizeEstimator ?? (path => DirectoryCleanupHelper.EstimateDirectorySize(path, maxDepth: 2));
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var profiles = new List<(string Id, string Name, string RelativePath)>
        {
            ("chrome", "Google Chrome", Path.Combine("Google", "Chrome", "User Data", "Default", "Cache")),
            ("edge", "Microsoft Edge", Path.Combine("Microsoft", "Edge", "User Data", "Default", "Cache")),
            ("firefox", "Mozilla Firefox", Path.Combine("Mozilla", "Firefox", "Profiles")),
        };

        var results = new List<BrowserCachePathInfo>();

        foreach (var (id, name, relative) in profiles)
        {
            var fullPath = id switch
            {
                "firefox" => FindFirefoxCachePath(localAppData)
                            ?? Path.Combine(localAppData, "Mozilla", "Firefox", "Profiles"),
                _ => Path.Combine(localAppData, relative),
            };

            var exists = Directory.Exists(fullPath);
            results.Add(new BrowserCachePathInfo
            {
                BrowserId = id,
                DisplayName = name,
                CachePath = fullPath,
                PathExists = exists,
                EstimatedSizeBytes = exists ? estimator(fullPath) : 0,
            });
        }

        return results;
    }

    private static string? FindFirefoxCachePath(string localAppData)
    {
        var profilesRoot = Path.Combine(localAppData, "Mozilla", "Firefox", "Profiles");
        if (!Directory.Exists(profilesRoot))
            return null;

        foreach (var profile in Directory.EnumerateDirectories(profilesRoot))
        {
            var cache2 = Path.Combine(profile, "cache2");
            if (Directory.Exists(cache2))
                return cache2;
        }

        return null;
    }
}
