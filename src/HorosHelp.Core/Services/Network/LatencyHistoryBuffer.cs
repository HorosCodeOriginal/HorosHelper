using System.Globalization;

namespace HorosHelp.Core.Services.Network;

public sealed class LatencyHistoryBuffer
{
    public const int DefaultCapacity = 20;
    public const double DefaultMaxLatencyMs = 20;

    private readonly int _capacity;
    private readonly Queue<double> _values = new();

    public LatencyHistoryBuffer(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity;
    }

    public int Count => _values.Count;

    public IReadOnlyList<double> Values => _values.ToList();

    public void Add(double latencyMs)
    {
        _values.Enqueue(Math.Max(0, latencyMs));

        while (_values.Count > _capacity)
            _values.Dequeue();
    }

    public void Clear() => _values.Clear();

    public string BuildSparklinePoints(
        double width = 800,
        double minY = 8,
        double maxY = 72,
        double maxLatencyMs = DefaultMaxLatencyMs)
    {
        if (_values.Count == 0)
            return string.Empty;

        var values = _values.ToArray();
        var step = values.Length <= 1 ? 0 : width / (values.Length - 1);
        var range = Math.Max(1, maxY - minY);
        var parts = new string[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            var x = Math.Round(i * step, 0, MidpointRounding.AwayFromZero);
            var normalized = Math.Clamp(values[i] / maxLatencyMs, 0, 1);
            var y = Math.Round(minY + normalized * range, 0, MidpointRounding.AwayFromZero);
            parts[i] = string.Create(CultureInfo.InvariantCulture, $"{x},{y}");
        }

        return string.Join(' ', parts);
    }
}
