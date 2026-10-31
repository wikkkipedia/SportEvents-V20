using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using Xunit;

namespace SportEvents.Tests;

public sealed class Lab07ResultTableComparisonTests
{
    private readonly ResultTable table = new();

    [Fact]
    public void Compare_DifferentAgeGroups_PrioritizesJuniorGroup()
    {
        ProtocolRow junior = Row(1, AgeGroup.Junior, 20);
        ProtocolRow adult = Row(2, AgeGroup.Adult, 10);

        Assert.True(table.Compare(junior, adult) < 0);
        Assert.True(table.Compare(adult, junior) > 0);
    }

    [Fact]
    public void Compare_SameGroup_PrioritizesFasterTime()
    {
        ProtocolRow fast = Row(1, AgeGroup.Adult, 20);
        ProtocolRow slow = Row(2, AgeGroup.Adult, 30);

        Assert.True(table.Compare(fast, slow) < 0);
        Assert.True(table.Compare(slow, fast) > 0);
    }

    [Fact]
    public void Compare_SameGroupAndTime_UsesParticipantIdentifier()
    {
        ProtocolRow first = Row(1, AgeGroup.Adult, 20);
        ProtocolRow second = Row(2, AgeGroup.Adult, 20);

        Assert.True(table.Compare(first, second) < 0);
        Assert.Equal(0, table.Compare(first, first));
    }

    [Fact]
    public void Compare_NullRows_ThrowsArgumentNull()
    {
        ProtocolRow row = Row(1, AgeGroup.Adult, 20);

        Assert.Throws<ArgumentNullException>(() => table.Compare(null!, row));
        Assert.Throws<ArgumentNullException>(() => table.Compare(row, null!));
    }

    private static ProtocolRow Row(int id, AgeGroup group, int seconds)
        => new(id, "Runner", group, TimeSpan.FromSeconds(seconds), 1);
}
