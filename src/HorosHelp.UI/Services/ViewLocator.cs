using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace HorosHelp.UI.Services;

public sealed class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Type> ViewModelToView = BuildMap();

    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        var viewModelType = param.GetType();
        if (!ViewModelToView.TryGetValue(viewModelType, out var viewType))
        {
            return new TextBlock
            {
                Text = $"Keine View für {viewModelType.Name}",
                Foreground = Avalonia.Media.Brushes.OrangeRed
            };
        }

        return Activator.CreateInstance(viewType) as Control;
    }

    public bool Match(object? data) => data is ViewModels.ViewModelBase;

    private static Dictionary<Type, Type> BuildMap()
    {
        var uiAssembly = typeof(ViewLocator).Assembly;
        var viewModels = uiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel", StringComparison.Ordinal))
            .ToList();

        var map = new Dictionary<Type, Type>();
        foreach (var viewModel in viewModels)
        {
            var viewName = viewModel.FullName!.Replace(".ViewModels.", ".Views.", StringComparison.Ordinal)
                .Replace("ViewModel", "View", StringComparison.Ordinal);
            var viewType = uiAssembly.GetType(viewName);
            if (viewType is not null)
            {
                map[viewModel] = viewType;
            }
        }

        return map;
    }
}
