using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace HorosHelp.UI.Controls;

public partial class Sparkline : UserControl
{
    public static readonly StyledProperty<string> PointsDataProperty =
        AvaloniaProperty.Register<Sparkline, string>(nameof(PointsData), string.Empty);

    public static readonly StyledProperty<IBrush> StrokeBrushProperty =
        AvaloniaProperty.Register<Sparkline, IBrush>(nameof(StrokeBrush), Brushes.LimeGreen);

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<Sparkline, double>(nameof(StrokeWidth), 1.5);

    private readonly Canvas _canvas;
    private readonly Polyline _line;

    static Sparkline()
    {
        PointsDataProperty.Changed.AddClassHandler<Sparkline>((s, _) => s.UpdateLine());
        StrokeBrushProperty.Changed.AddClassHandler<Sparkline>((s, _) => s.UpdateLine());
        StrokeWidthProperty.Changed.AddClassHandler<Sparkline>((s, _) => s.UpdateLine());
    }

    public Sparkline()
    {
        InitializeComponent();
        _canvas = this.FindControl<Canvas>("RootCanvas")
            ?? throw new InvalidOperationException("RootCanvas not found.");
        _line = this.FindControl<Polyline>("Line")
            ?? throw new InvalidOperationException("Line not found.");
        UpdateLine();
    }

    public string PointsData
    {
        get => GetValue(PointsDataProperty);
        set => SetValue(PointsDataProperty, value);
    }

    public IBrush StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 200 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 28 : availableSize.Height;
        return new Size(Math.Max(1, width), Math.Max(1, height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _canvas.Width = finalSize.Width;
        _canvas.Height = finalSize.Height;
        UpdateLine();
        return finalSize;
    }

    private void UpdateLine()
    {
        _line.Stroke = StrokeBrush;
        _line.StrokeThickness = StrokeWidth;

        var width = _canvas.Width > 0 ? _canvas.Width : Width;
        var height = _canvas.Height > 0 ? _canvas.Height : Height;
        if (double.IsNaN(width) || width <= 0)
            width = 200;
        if (double.IsNaN(height) || height <= 0)
            height = 28;

        _line.Points = ParsePoints(PointsData, width, height);
        IsVisible = _line.Points.Count >= 2;
    }

    private static Points ParsePoints(string? value, double width, double height)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Points();

        var raw = new List<Point>();
        var segments = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            var parts = segment.Split(',');
            if (parts.Length != 2)
                continue;

            raw.Add(new Point(
                double.Parse(parts[0], CultureInfo.InvariantCulture),
                double.Parse(parts[1], CultureInfo.InvariantCulture)));
        }

        if (raw.Count < 2)
            return new Points();

        var maxX = raw.Max(p => p.X);
        var maxY = raw.Max(p => p.Y);
        var minY = raw.Min(p => p.Y);
        var yRange = Math.Max(1, maxY - minY);

        var scaled = raw.Select(p => new Point(
            maxX <= 0 ? 0 : p.X / maxX * width,
            height - ((p.Y - minY) / yRange * (height - 4)) - 2)).ToList();

        return new Points(scaled);
    }
}
