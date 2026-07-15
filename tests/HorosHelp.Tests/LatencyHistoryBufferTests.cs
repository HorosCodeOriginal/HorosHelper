using HorosHelp.Core.Services.Network;

namespace HorosHelp.Tests;

public class LatencyHistoryBufferTests
{
    [Fact]
    public void Add_MaintainsRollingCapacity()
    {
        var buffer = new LatencyHistoryBuffer(capacity: 3);

        buffer.Add(10);
        buffer.Add(20);
        buffer.Add(30);
        buffer.Add(40);

        Assert.Equal(3, buffer.Count);
        Assert.Equal([20, 30, 40], buffer.Values);
    }

    [Fact]
    public void BuildSparklinePoints_ReturnsEmpty_WhenNoValues()
    {
        var buffer = new LatencyHistoryBuffer();
        Assert.Equal(string.Empty, buffer.BuildSparklinePoints());
    }

    [Fact]
    public void BuildSparklinePoints_MapsLatencyToPoints()
    {
        var buffer = new LatencyHistoryBuffer(capacity: 2);
        buffer.Add(0);
        buffer.Add(20);

        var points = buffer.BuildSparklinePoints(width: 100, minY: 0, maxY: 100, maxLatencyMs: 20);

        Assert.StartsWith("0,0", points, StringComparison.Ordinal);
        Assert.EndsWith("100,100", points, StringComparison.Ordinal);
    }
}
