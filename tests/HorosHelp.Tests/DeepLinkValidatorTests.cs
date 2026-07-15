using HorosHelp.Core.Services.Knowledge;

namespace HorosHelp.Tests;

public class DeepLinkValidatorTests
{
    [Theory]
    [InlineData("ms-settings:network-wifi", true)]
    [InlineData("control:appwiz", true)]
    [InlineData("control:unknown-applet", false)]
    [InlineData("", false)]
    [InlineData("https://example.com", false)]
    public void IsKnownDeepLink_ValidatesSchemes(string deepLink, bool expected)
    {
        Assert.Equal(expected, DeepLinkValidator.IsKnownDeepLink(deepLink));
    }

    [Fact]
    public void ValidateArticles_FlagsUnknownControlLinks()
    {
        var articles = new[]
        {
            new HorosHelp.Core.Models.Knowledge.KnowledgeArticle
            {
                Id = "good",
                CategoryId = "system",
                Title = "Gut",
                Subtitle = "",
                Description = "",
                DeepLink = "ms-settings:display",
                Steps = [],
            },
            new HorosHelp.Core.Models.Knowledge.KnowledgeArticle
            {
                Id = "bad",
                CategoryId = "system",
                Title = "Schlecht",
                Subtitle = "",
                Description = "",
                DeepLink = "control:does-not-exist",
                Steps = [],
            },
        };

        var results = DeepLinkValidator.ValidateArticles(articles);

        Assert.Contains(results, r => r.ArticleId == "good" && r.IsKnown);
        Assert.Contains(results, r => r.ArticleId == "bad" && !r.IsKnown);
    }

    [Fact]
    public void KnowledgeBaseService_LoadsArticlesWithKnownDeepLinks()
    {
        var service = new KnowledgeBaseService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<KnowledgeBaseService>.Instance);

        var articles = service.GetArticles();
        var validation = DeepLinkValidator.ValidateArticles(articles);
        var unknown = validation.Where(v => !v.IsKnown).ToList();

        Assert.True(unknown.Count == 0, $"Unknown deep links: {string.Join(", ", unknown.Select(u => u.DeepLink))}");
    }
}
