namespace HorosHelp.Core.Models.Knowledge;

public sealed class KnowledgeCategory
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string IconGlyph { get; init; } = "";
}

public sealed class ArticleStep
{
    public int Number { get; init; }
    public string Text { get; init; } = "";
}

public sealed class KnowledgeArticle
{
    public string Id { get; init; } = "";
    public string CategoryId { get; init; } = "";
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string Description { get; init; } = "";
    public IReadOnlyList<ArticleStep> Steps { get; init; } = [];
    public string Tip { get; init; } = "";
    public string DeepLink { get; init; } = "";
    public bool IsFavorite { get; init; }
}
