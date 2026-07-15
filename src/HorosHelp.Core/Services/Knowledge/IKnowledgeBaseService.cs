using HorosHelp.Core.Models.Knowledge;

namespace HorosHelp.Core.Services.Knowledge;

public interface IKnowledgeBaseService
{
    IReadOnlyList<KnowledgeCategory> GetCategories();

    IReadOnlyList<KnowledgeArticle> GetArticles(string? categoryId = null, string? searchQuery = null);

    KnowledgeArticle? GetArticle(string articleId);

    int ArticleCount { get; }
}
