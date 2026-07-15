using HorosHelp.Core.Models.Knowledge;

namespace HorosHelp.Core.Services.Knowledge;

public sealed class DeepLinkValidationResult
{
    public required string DeepLink { get; init; }
    public required string ArticleId { get; init; }
    public bool IsKnown { get; init; }
    public string Scheme { get; init; } = "";
}

public static class DeepLinkValidator
{
    private static readonly HashSet<string> KnownMsSettingsPrefixes =
    [
        "ms-settings:",
    ];

    public static bool IsKnownDeepLink(string deepLink)
    {
        if (string.IsNullOrWhiteSpace(deepLink))
            return false;

        if (deepLink.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase))
            return true;

        if (deepLink.StartsWith("control:", StringComparison.OrdinalIgnoreCase))
        {
            var applet = deepLink["control:".Length..];
            return Shell32AppletCatalog.Applets.ContainsKey(applet);
        }

        if (deepLink.StartsWith("control.exe", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    public static IReadOnlyList<DeepLinkValidationResult> ValidateArticles(IReadOnlyList<KnowledgeArticle> articles)
    {
        var results = new List<DeepLinkValidationResult>();

        foreach (var article in articles)
        {
            if (string.IsNullOrWhiteSpace(article.DeepLink))
                continue;

            var scheme = ResolveScheme(article.DeepLink);
            results.Add(new DeepLinkValidationResult
            {
                ArticleId = article.Id,
                DeepLink = article.DeepLink,
                Scheme = scheme,
                IsKnown = IsKnownDeepLink(article.DeepLink),
            });
        }

        return results;
    }

    private static string ResolveScheme(string deepLink)
    {
        var colon = deepLink.IndexOf(':');
        return colon > 0 ? deepLink[..(colon + 1)] : deepLink;
    }
}
