using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace HorosHelp.UI.Controls;

public partial class ProgressRing : UserControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Value));

    public static readonly StyledProperty<double> DiameterProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Diameter), 160d);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StrokeThickness), 14d);

    public static readonly StyledProperty<IBrush> TrackBrushProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush>(
            nameof(TrackBrush),
            new SolidColorBrush(Color.Parse("#334155")));

    public static readonly StyledProperty<IBrush> ProgressBrushProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush>(
            nameof(ProgressBrush),
            Brushes.Orange);

    private readonly Canvas _canvas;
    private readonly ShapePath _trackPath;
    private readonly ShapePath _progressPath;

    static ProgressRing()
    {
        ValueProperty.Changed.AddClassHandler<ProgressRing>((ring, _) => ring.UpdateRing());
        DiameterProperty.Changed.AddClassHandler<ProgressRing>((ring, _) => ring.UpdateRing());
        StrokeThicknessProperty.Changed.AddClassHandler<ProgressRing>((ring, _) => ring.UpdateRing());
        TrackBrushProperty.Changed.AddClassHandler<ProgressRing>((ring, _) => ring.UpdateRing());
        ProgressBrushProperty.Changed.AddClassHandler<ProgressRing>((ring, _) => ring.UpdateRing());
    }

    public ProgressRing()
    {
        InitializeComponent();
        _canvas = this.FindControl<Canvas>("RootCanvas")
            ?? throw new InvalidOperationException("RootCanvas not found.");
        _trackPath = this.FindControl<ShapePath>("TrackPath")
            ?? throw new InvalidOperationException("TrackPath not found.");
        _progressPath = this.FindControl<ShapePath>("ProgressPath")
            ?? throw new InvalidOperationException("ProgressPath not found.");
        UpdateRing();
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public IBrush ProgressBrush
    {
        get => GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }

    private void UpdateRing()
    {
        var diameter = Diameter;
        var thickness = StrokeThickness;
        var layoutSize = diameter + thickness;
        var clamped = Math.Clamp(Value, 0, 100);
        var center = layoutSize / 2.0;
        var radius = diameter / 2.0;

        _canvas.Width = layoutSize;
        _canvas.Height = layoutSize;
        Width = layoutSize;
        Height = layoutSize;

        _trackPath.StrokeThickness = thickness;
        _trackPath.Stroke = TrackBrush;
        _trackPath.Data = CreateFullCircleGeometry(center, radius);

        _progressPath.StrokeThickness = thickness;
        _progressPath.Stroke = ProgressBrush;
        _progressPath.Data = clamped <= 0
            ? null
            : CreateArcGeometry(center, radius, clamped);
    }

    private static Geometry CreateFullCircleGeometry(double center, double radius)
    {
        var top = new Point(center, center - radius);
        var bottom = new Point(center, center + radius);

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(top, false);
            context.ArcTo(bottom, new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            context.ArcTo(top, new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        }

        return geometry;
    }

    private static Geometry CreateArcGeometry(double center, double radius, double percent)
    {
        if (percent >= 100)
            return CreateFullCircleGeometry(center, radius);

        var start = new Point(center, center - radius);
        var end = PolarToCartesian(center, center, radius, (-Math.PI / 2.0) + (2.0 * Math.PI * percent / 100.0));

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(start, false);
            context.ArcTo(
                end,
                new Size(radius, radius),
                0,
                percent > 50.0,
                SweepDirection.Clockwise);
        }

        return geometry;
    }

    private static Point PolarToCartesian(double centerX, double centerY, double radius, double angleRadians)
    {
        return new Point(
            centerX + (radius * Math.Cos(angleRadians)),
            centerY + (radius * Math.Sin(angleRadians)));
    }
}
