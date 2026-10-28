using SportEvents.Core.Application;
using Xunit;

namespace SportEvents.Tests;

public sealed class RaceTimeInputTests
{
    [Theory]
    [InlineData("01:25.500", 85500)]
    [InlineData("00:02:01.000", 121000)]
    public void TryParse_Valid_ReturnsElapsed(string text, int expectedMilliseconds)
    {
        OperationResult<TimeSpan> actual = RaceTimeInput.TryParse(text);
        Assert.True(actual.IsSuccess);
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), actual.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("не час")]
    [InlineData("00:00.000")]
    public void TryParse_Invalid_ReturnsError(string raw)
    {
        OperationResult<TimeSpan> result = RaceTimeInput.TryParse(raw);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }
}
