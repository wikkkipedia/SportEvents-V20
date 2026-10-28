using SportEvents.Core.Application;
using Xunit;
namespace SportEvents.Tests;

public sealed class ElapsedTimePolicyTests
{
    [Fact]
    public void SumSplits_TwoSegments_ReturnsSum()
    {
        TimeSpan result = new ElapsedTimePolicy().SumSplits(new[] {
            TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(11.5) });
        Assert.Equal(TimeSpan.FromSeconds(21.5), result);
    }

    [Fact]
    public void SumSplits_Empty_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => new ElapsedTimePolicy().SumSplits(Array.Empty<TimeSpan>()));

    [Fact]
    public void SumSplits_Negative_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new ElapsedTimePolicy().SumSplits(new[] { TimeSpan.FromSeconds(-1) }));
}
