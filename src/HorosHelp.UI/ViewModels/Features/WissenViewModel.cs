using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HorosHelp.Core.Models.Knowledge;
using HorosHelp.Core.Services.Knowledge;
using Microsoft.Extensions.Logging;
using CoreCategory = HorosHelp.Core.Models.Knowledge.KnowledgeCategory;

namespace HorosHelp.UI.ViewModels.Features;

public sealed class KnowledgeCategoryItem
{
    private static readonly IBrush AmberBrush = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush AmberBgBrush = new SolidColorBrush(Color.FromArgb(0x18, 0xF5, 0x9E, 0x0B));
    private static readonly IBrush AmberIconBg = new SolidColorBrush(Color.Parse("#92400E"));
    private static readonly IBrush IconBgBrush = new SolidColorBrush(Color.Parse("#334155"));
    private static readonly IBrush TransparentBrush = Brushes.Transparent;
    private static readonly IBrush TextPrimaryBrush = new SolidColorBrush(Color.Parse("#F8FAFC"));

    public string Id { get; init; } = "";
    public string IconGlyph { get; init; } = "";
    public string Name { get; init; } = "";
    public int ArticleCount { get; init; }
    public bool IsActive { get; init; }

    public string ArticleCountText => $"{ArticleCount} Artikel";

    public IBrush ItemBackground => IsActive ? AmberBgBrush : TransparentBrush;
    public IBrush ItemBorderBrush => IsActive ? AmberBrush : TransparentBrush;
    public IBrush IconBackground => IsActive ? AmberIconBg : IconBgBrush;
    public IBrush IconForeground => IsActive ? AmberBrush : TextPrimaryBrush;
    public IBrush NameForeground => IsActive ? AmberBrush : TextPrimaryBrush;
    public FontWeight NameWeight => IsActive ? FontWeight.SemiBold : FontWeight.Normal;
}

public sealed class KnowledgeArticleItem
{
    private static readonly IBrush AmberBrush = new SolidColorBrush(Color.Parse("#F59E0B"));
    private static readonly IBrush AmberBgBrush = new SolidColorBrush(Color.FromArgb(0x14, 0xF5, 0x9E, 0x0B));
    private static readonly IBrush BorderBrush = new SolidColorBrush(Color.Parse("#334155"));
    private static readonly IBrush MutedBrush = new SolidColorBrush(Color.Parse("#64748B"));

    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public bool IsFavorite { get; init; }
    public bool IsSelected { get; init; }

    public IBrush ItemBackground => IsSelected ? AmberBgBrush : Brushes.Transparent;
    public IBrush ItemBorderBrush => IsSelected ? AmberBrush : BorderBrush;
    public string StarGlyph => IsFavorite ? "★" : "☆";
    public IBrush StarForeground => IsFavorite ? AmberBrush : MutedBrush;
}

public sealed class ArticleStepItem
{
    public int Number { get; init; }
    public string Text { get; init; } = "";
}

public sealed partial class WissenViewModel : ViewModelBase
{
    private readonly IKnowledgeBaseService _knowledgeService;
    private readonly ILogger<WissenViewModel> _logger;
    private string _selectedCategoryId = "netzwerk";
    private string? _selectedArticleId = "wlan-settings";

    public string Title => "Windows-Wissens-Navigator";
    public string Subtitle => "Durchsuchbare Wissensdatenbank";

    [ObservableProperty] private string _searchQuery = "";

    public ObservableCollection<KnowledgeCategoryItem> Categories { get; } = [];
    public ObservableCollection<KnowledgeArticleItem> Articles { get; } = [];
    public ObservableCollection<ArticleStepItem> DetailSteps { get; } = [];

    [ObservableProperty] private string _breadcrumbCategory = "Netzwerk";
    [ObservableProperty] private string _breadcrumbArticle = "WLAN-Einstellungen öffnen";
    [ObservableProperty] private string _articlesHeader = "Artikel (0)";
    [ObservableProperty] private string _detailTitle = "";
    [ObservableProperty] private string _detailDescription = "";
    [ObservableProperty] private string _detailTip = "";

    public string BreadcrumbRoot => "Wissen";

    public WissenViewModel(
        IKnowledgeBaseService knowledgeService,
        ILogger<WissenViewModel> logger)
    {
        _knowledgeService = knowledgeService;
        _logger = logger;
        RefreshAll();
    }

    partial void OnSearchQueryChanged(string value) => RefreshArticles();

    [RelayCommand]
    private void SelectCategory(string? categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
            return;

        _selectedCategoryId = categoryId;
        var articles = _knowledgeService.GetArticles(categoryId);
        _selectedArticleId = articles.FirstOrDefault()?.Id;
        RefreshAll();
    }

    [RelayCommand]
    private void SelectArticle(string? articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return;

        _selectedArticleId = articleId;
        RefreshArticles();
        RefreshDetail();
    }

    [RelayCommand]
    private void OpenDirectly()
    {
        var article = _selectedArticleId is not null
            ? _knowledgeService.GetArticle(_selectedArticleId)
            : null;

        if (article is null || string.IsNullOrWhiteSpace(article.DeepLink))
        {
            _logger.LogWarning("No deep link for article {ArticleId}", _selectedArticleId);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(article.DeepLink) { UseShellExecute = true });
            _logger.LogInformation("Opened deep link {DeepLink} for article {ArticleId}",
                article.DeepLink, article.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open deep link {DeepLink}", article.DeepLink);
        }
    }

    private void RefreshAll()
    {
        RefreshCategories();
        RefreshArticles();
        RefreshDetail();
    }

    private void RefreshCategories()
    {
        Categories.Clear();
        foreach (var category in _knowledgeService.GetCategories())
        {
            var count = _knowledgeService.GetArticles(category.Id, SearchQuery).Count;
            Categories.Add(MapCategory(category, count, category.Id == _selectedCategoryId));
        }
    }

    private void RefreshArticles()
    {
        Articles.Clear();
        var articles = _knowledgeService.GetArticles(_selectedCategoryId, SearchQuery);

        if (_selectedArticleId is null || articles.All(a => a.Id != _selectedArticleId))
            _selectedArticleId = articles.FirstOrDefault()?.Id;

        foreach (var article in articles)
        {
            Articles.Add(new KnowledgeArticleItem
            {
                Id = article.Id,
                Title = article.Title,
                Subtitle = article.Subtitle,
                IsFavorite = article.IsFavorite,
                IsSelected = article.Id == _selectedArticleId,
            });
        }

        ArticlesHeader = $"Artikel ({articles.Count})";

        var category = _knowledgeService.GetCategories()
            .FirstOrDefault(c => c.Id == _selectedCategoryId);
        BreadcrumbCategory = category?.Name ?? _selectedCategoryId;
    }

    private void RefreshDetail()
    {
        DetailSteps.Clear();

        if (_selectedArticleId is null)
        {
            DetailTitle = "Kein Artikel ausgewählt";
            DetailDescription = "Wählen Sie einen Artikel aus der Liste.";
            DetailTip = "";
            BreadcrumbArticle = "";
            return;
        }

        var article = _knowledgeService.GetArticle(_selectedArticleId);
        if (article is null)
            return;

        DetailTitle = article.Title;
        DetailDescription = article.Description;
        DetailTip = article.Tip;
        BreadcrumbArticle = article.Title;

        foreach (var step in article.Steps)
        {
            DetailSteps.Add(new ArticleStepItem
            {
                Number = step.Number,
                Text = step.Text,
            });
        }
    }

    private static KnowledgeCategoryItem MapCategory(CoreCategory category, int count, bool isActive) =>
        new()
        {
            Id = category.Id,
            IconGlyph = category.IconGlyph,
            Name = category.Name,
            ArticleCount = count,
            IsActive = isActive,
        };
}
