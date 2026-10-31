using SportEvents.Core.Application;
using Xunit;

namespace SportEvents.Tests;

public sealed class RaceTimeFormatterTests
{
    [Theory]
    [InlineData(1, "00:00:00.001")]
    [InlineData(2, "00:00:00.002")]
    [InlineData(999, "00:00:00.999")]
    [InlineData(1000, "00:00:01.000")]
    [InlineData(78523, "00:01:18.523")]
    [InlineData(3661000, "01:01:01.000")]
    [InlineData(86399999, "23:59:59.999")]
    public void Format_ValidMilliseconds_UsesFixedPrecision(int milliseconds, string expected)
        => Assert.Equal(expected, RaceTimeFormatter.Format(TimeSpan.FromMilliseconds(milliseconds)));

    [Fact]
    public void Format_Negative_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RaceTimeFormatter.Format(TimeSpan.FromMilliseconds(-1)));
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(86400000)]
    public void Format_InvalidMilliseconds_ThrowsWithDurationParameter(int milliseconds)
    {
        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(
            () => RaceTimeFormatter.Format(TimeSpan.FromMilliseconds(milliseconds)));

        Assert.Equal("duration", error.ParamName);
    }

    [Fact]
    public void Format_Zero_ThrowsBecauseRaceTimeMustBePositive()
        => Assert.Throws<ArgumentOutOfRangeException>(() => RaceTimeFormatter.Format(TimeSpan.Zero));

}
