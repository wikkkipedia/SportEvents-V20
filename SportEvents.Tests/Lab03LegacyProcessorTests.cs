using SportEvents.Lab03;
using Xunit;

namespace SportEvents.Tests;

public class Lab03LegacyProcessorTests
{
    private static List<RaceChargeLine> Lines() => new()
    {
        new RaceChargeLine { Code = "100m", Quantity = 12, UnitPrice = 100m },
        new RaceChargeLine { Code = "200m", Quantity = 1, UnitPrice = 250m },
    };

    [Fact]
    public void Process_RegularClient_Returns1292()
    {
        decimal actual = LegacyProcessor.Process(1, "Іван", "a@b.com", true, Lines(),
            new DateOnly(2026, 3, 10), "", "New", false, 60m, out string state);
        Assert.Equal(1292.70m, actual);
        Assert.Equal("Paid", state);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        List<RaceChargeLine> lines = new()
        {
            new RaceChargeLine { Code = "100m", Quantity = 2, UnitPrice = 200m },
        };
        decimal actual = LegacyProcessor.Process(2, "Іван", "a@b.com", false, lines,
            new DateOnly(2026, 3, 10), "", "Paid", false, 60m, out string state);
        Assert.Equal(460m, actual);
        Assert.Equal("Shipped", state);
    }

    [Fact]
    public void Process_CancelledDocument_ReturnsZero()
    {
        decimal actual = LegacyProcessor.Process(3, "Іван", "a@b.com", false, Lines(),
            new DateOnly(2026, 3, 10), "", "Cancelled", false, 60m, out string state);
        Assert.Equal(0m, actual);
        Assert.Equal("Cancelled", state);
    }
}
