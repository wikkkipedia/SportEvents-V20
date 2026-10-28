using SportEvents.Core.Domain;
using Xunit;
namespace SportEvents.Tests;
public sealed class CompetitionDistanceTests
{
    [Fact]
    public void New_PositiveDistance_KeepsValue()
        => Assert.Equal(5000, new CompetitionDistance(5000).Meters);
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void New_NonPositiveDistance_Throws(int meters)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new CompetitionDistance(meters));
}
