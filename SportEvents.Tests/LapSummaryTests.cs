using SportEvents.Core.Application;
using Xunit;

namespace SportEvents.Tests;

public sealed class LapSummaryTests
{
    [Fact]
    public void Calculate_ValidLaps_ReturnsResult()
    {
        LapSummaryCalculator calculator = new(new ElapsedTimePolicy());
        var request = new LapSummaryRequest(1, 2, new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) });
        Assert.Equal(new LapSummaryResult(TimeSpan.FromSeconds(50), 2), calculator.Calculate(request));
    }

    [Fact]
    public void Calculate_InvalidEvent_Throws()
    {
        LapSummaryCalculator calculator = new(new ElapsedTimePolicy());
        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(
            new LapSummaryRequest(0, 2, new[] { TimeSpan.FromSeconds(10) })));
    }
}
