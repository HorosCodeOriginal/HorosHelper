using HorosHelp.Core.Models.Storage;

namespace HorosHelp.Core.Services.Storage;

public static class StorageAnalyzer
{
    public const double CleanupChartMaxPercent = 100;

    public static IReadOnlyList<StorageCategoryInfo> BuildCategories(
        long totalUsedBytes,
        IReadOnlyDictionary<string, long> categorySizes)
    {
        if (totalUsedBytes <= 0)
        {
            return categorySizes
                .Select(kvp => new StorageCategoryInfo
                {
                    Name = kvp.Key,
                    SizeBytes = kvp.Value,
                    PercentOfUsed = 0,
                })
                .ToList();
        }

        return categorySizes
            .Select(kvp => new StorageCategoryInfo
            {
                Name = kvp.Key,
                SizeBytes = kvp.Value,
                PercentOfUsed = Math.Round(kvp.Value / (double)totalUsedBytes * 100, 1, MidpointRounding.AwayFromZero),
            })
            .OrderByDescending(c => c.SizeBytes)
            .ToList();
    }

    public static IReadOnlyList<CleanupCandidateInfo> BuildCleanupCandidates(
        IReadOnlyDictionary<string, (string Name, long SizeBytes)> candidates)
    {
        var total = candidates.Values.Sum(c => c.SizeBytes);
        if (total <= 0)
            return [];

        return candidates
            .Select(kvp => new CleanupCandidateInfo
            {
                CategoryId = kvp.Key,
                Name = kvp.Value.Name,
                SizeBytes = kvp.Value.SizeBytes,
                SharePercent = Math.Round(kvp.Value.SizeBytes / (double)total * 100, 0, MidpointRounding.AwayFromZero),
            })
            .OrderByDescending(c => c.SizeBytes)
            .ToList();
    }

    public static double CalculateCleanupChartPercent(long reclaimableBytes, long primaryDriveUsedBytes)
    {
        if (reclaimableBytes <= 0 || primaryDriveUsedBytes <= 0)
            return 0;

        var percent = reclaimableBytes / (double)primaryDriveUsedBytes * 100;
        return Math.Clamp(Math.Round(percent, 0, MidpointRounding.AwayFromZero), 0, CleanupChartMaxPercent);
    }

    public static string FormatSizeDe(long bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;

        if (bytes >= gb)
        {
            var gbValue = bytes / gb;
            return gbValue >= 10
                ? $"{gbValue:F0} GB".Replace('.', ',')
                : $"{gbValue:F1} GB".Replace('.', ',');
        }

        if (bytes >= mb)
            return $"{bytes / mb:F0} MB".Replace('.', ',');

        if (bytes >= kb)
            return $"{bytes / kb:F0} KB".Replace('.', ',');

        return $"{bytes} B";
    }

    public static string FormatUsedSummary(long usedBytes, long totalBytes)
    {
        return $"{FormatSizeDe(usedBytes)} von {FormatSizeDe(totalBytes)} verwendet";
    }

    public static string FormatFreeSummary(long freeBytes)
    {
        return $"{FormatSizeDe(freeBytes)} frei";
    }

    public static string FormatPercentText(double percent) =>
        $"{Math.Round(percent, 0, MidpointRounding.AwayFromZero)}%";

    public static string FormatPercentText(double percent, int digits) =>
        $"{Math.Round(percent, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}").Replace('.', ',')}%";

    public static string BuildCleanupButtonText(long reclaimableBytes) =>
        $"Jetzt bereinigen — {FormatSizeDe(reclaimableBytes)}";
}
