using HorosHelp.Core.Services.Knowledge;

namespace HorosHelp.Tests;

public class KnowledgeBaseServiceTests
{
    private readonly KnowledgeBaseService _service = new(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<KnowledgeBaseService>.Instance);

    [Fact]
    public void GetCategories_ReturnsFourCategories()
    {
        var categories = _service.GetCategories();

        Assert.Equal(4, categories.Count);
        Assert.Contains(categories, c => c.Id == "netzwerk");
    }

    [Fact]
    public void GetArticles_FiltersByCategory()
    {
        var articles = _service.GetArticles("netzwerk");

        Assert.NotEmpty(articles);
        Assert.All(articles, a => Assert.Equal("netzwerk", a.CategoryId));
    }

    [Theory]
    [InlineData("wlan", "wlan-settings")]
    [InlineData("DNS", "dns-settings")]
    [InlineData("Firewall", "firewall-settings")]
    public void GetArticles_FiltersBySearchQuery(string query, string expectedId)
    {
        var articles = _service.GetArticles(searchQuery: query);

        Assert.Contains(articles, a => a.Id == expectedId);
    }

    [Fact]
    public void GetArticle_ReturnsArticleWithDeepLink()
    {
        var article = _service.GetArticle("wlan-settings");

        Assert.NotNull(article);
        Assert.StartsWith("ms-settings:", article!.DeepLink);
        Assert.NotEmpty(article.Steps);
    }

    [Fact]
    public void ArticleCount_IsAtLeastFifty()
    {
        Assert.True(_service.ArticleCount >= 50);
    }
}
