using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using Xunit;
namespace SportEvents.Tests;
public sealed class ResultTableTests
{
    private readonly ResultTable _table = new();

    [Fact]
    public void Rank_MixedGroups_SortsByGroup()
    {
        var rows = new[] { Row(5, AgeGroup.Masters, 40), Row(7, AgeGroup.Adult, 50) };
        Assert.Equal(new[] { 7, 5 }, _table.Rank(rows).Select(x => x.ParticipantId));
    }

    [Fact]
    public void Rank_SameGroup_SortsByElapsed()
    {
        var rows = new[] { Row(5, AgeGroup.Adult, 70), Row(7, AgeGroup.Adult, 50) };
        Assert.Equal(new[] { 7, 5 }, _table.Rank(rows).Select(x => x.ParticipantId));
    }

    [Fact]
    public void Rank_SameTime_SortsById()
    {
        var rows = new[] { Row(7, AgeGroup.Adult, 50), Row(5, AgeGroup.Adult, 50) };
        Assert.Equal(new[] { 5, 7 }, _table.Rank(rows).Select(x => x.ParticipantId));
    }

    [Fact] public void Rank_Empty_ReturnsEmpty() => Assert.Empty(_table.Rank(Array.Empty<ProtocolRow>()));
    [Fact] public void Rank_Null_Throws() => Assert.Throws<ArgumentNullException>(() => _table.Rank(null!));

    private static ProtocolRow Row(int id, AgeGroup group, double seconds)
        => new(id, $"Тест {id}", group, TimeSpan.FromSeconds(seconds), 0);
}
