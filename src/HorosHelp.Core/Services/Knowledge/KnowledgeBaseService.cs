using System.Text.Json;
using HorosHelp.Core.Models.Knowledge;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Knowledge;

public sealed class KnowledgeBaseService : IKnowledgeBaseService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly IReadOnlyList<KnowledgeCategory> Categories =
    [
        new() { Id = "system", Name = "System", IconGlyph = "▣" },
        new() { Id = "netzwerk", Name = "Netzwerk", IconGlyph = "⊕" },
        new() { Id = "sicherheit", Name = "Sicherheit", IconGlyph = "⊗" },
        new() { Id = "apps", Name = "Apps", IconGlyph = "⊞" },
    ];

    private readonly ILogger<KnowledgeBaseService> _logger;
    private readonly IReadOnlyList<KnowledgeArticle> _articles;

    public KnowledgeBaseService(ILogger<KnowledgeBaseService> logger)
    {
        _logger = logger;
        _articles = LoadArticles(logger);
    }

    public IReadOnlyList<KnowledgeCategory> GetCategories() => Categories;

    public IReadOnlyList<KnowledgeArticle> GetArticles(string? categoryId = null, string? searchQuery = null)
    {
        IEnumerable<KnowledgeArticle> query = _articles;

        if (!string.IsNullOrWhiteSpace(categoryId))
            query = query.Where(a => a.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var tokens = searchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(a => tokens.All(t => Matches(a, t)));
        }

        var result = query.ToList();
        _logger.LogDebug("Knowledge search returned {Count} articles (category={Category}, query={Query})",
            result.Count, categoryId ?? "*", searchQuery ?? "");

        return result;
    }

    public KnowledgeArticle? GetArticle(string articleId) =>
        _articles.FirstOrDefault(a => a.Id.Equals(articleId, StringComparison.OrdinalIgnoreCase));

    public int ArticleCount => _articles.Count;

    private static bool Matches(KnowledgeArticle article, string token)
    {
        var comparison = StringComparison.OrdinalIgnoreCase;
        return article.Title.Contains(token, comparison)
               || article.Subtitle.Contains(token, comparison)
               || article.Description.Contains(token, comparison);
    }

    private static IReadOnlyList<KnowledgeArticle> LoadArticles(ILogger logger)
    {
        try
        {
            var json = ReadArticlesJson();
            var document = JsonSerializer.Deserialize<KnowledgeArticlesDocument>(json, JsonOptions);
            var articles = document?.Articles ?? [];

            if (articles.Count < 50)
                logger.LogWarning("Knowledge base contains only {Count} articles; expected at least 50.", articles.Count);

            return articles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load knowledge articles; using empty catalog.");
            return [];
        }
    }

    private static string ReadArticlesJson()
    {
        var assembly = typeof(KnowledgeBaseService).Assembly;
        const string resourceName = "HorosHelp.Core.Data.knowledge-articles.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is not null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "knowledge-articles.json");
        if (File.Exists(filePath))
            return File.ReadAllText(filePath);

        throw new FileNotFoundException("Knowledge articles file not found.", filePath);
    }

    private sealed class KnowledgeArticlesDocument
    {
        public List<KnowledgeArticle> Articles { get; set; } = [];
    }
}
