using SportEvents.Lab03;
using Xunit;

namespace SportEvents.Tests;

public class Lab03LegacyProcessorTests
{
    private static RaceRequest Request(string status = "New", bool isRegular = false)
    {
        return new RaceRequest
        {
            DocId = 1, ClientName = "Іван", ClientEmail = "a@b.com", Status = status,
            IsRegular = isRegular, CreatedAt = new DateOnly(2026, 3, 10),
            DeliveryPrice = 60m, Lines = new List<RaceChargeLine>
            {
                new() { Code = "100m", Quantity = 12, UnitPrice = 100m },
                new() { Code = "200m", Quantity = 1, UnitPrice = 250m },
            },
        };
    }

    [Fact]
    public void Process_RegularClient_Returns1292()
    {
        RaceOutcome actual = LegacyProcessor.Process(Request(isRegular: true));
        Assert.Equal(1292.70m, actual.Total);
        Assert.Equal("Paid", actual.NewStatus);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        RaceRequest request = Request("Paid") with
        {
            Lines = new List<RaceChargeLine> { new() { Code = "100m", Quantity = 2, UnitPrice = 200m } },
        };
        RaceOutcome actual = LegacyProcessor.Process(request);
        Assert.Equal(460m, actual.Total);
        Assert.Equal("Shipped", actual.NewStatus);
    }

    [Fact]
    public void Process_CancelledDocument_ReturnsZero()
    {
        RaceOutcome actual = LegacyProcessor.Process(Request("Cancelled"));
        Assert.Equal(0m, actual.Total);
        Assert.Equal("Cancelled", actual.NewStatus);
    }
}
