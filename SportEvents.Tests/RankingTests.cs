using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using Xunit;

namespace SportEvents.Tests;

public sealed class RankingTests
{
    [Theory]
    [InlineData(2009, 11, 6, AgeGroup.Junior)]
    [InlineData(2008, 11, 5, AgeGroup.Adult)]
    [InlineData(1986, 11, 6, AgeGroup.Adult)]
    [InlineData(1986, 11, 5, AgeGroup.Masters)]
    [InlineData(1950, 1, 1, AgeGroup.Masters)]
    public void GetGroup_BoundaryDates_ReturnsExpected(int y, int m, int d, AgeGroup expected)
    {
        AgeGroup actual = AgeRules.GetGroup(new DateOnly(y, m, d), new DateOnly(2026, 11, 5));
        Assert.Equal(expected, actual);
    }

    [Fact] public void GetGroup_BirthAfterEvent_Throws()
    {
        Assert.Throws<ArgumentException>(() => AgeRules.GetGroup(new DateOnly(2027, 1, 1), new DateOnly(2026, 11, 5)));
    }

    [Fact] public void Ranking_AscendingTime_FirstIsFastest()
    {
        IReadOnlyList<ProtocolRow> rows = new RankingPolicy().Build(Data((1, 240), (2, 200), (3, 300)), 1);
        Assert.Equal(new[] { 2, 1, 3 }, rows.Select(r => r.ParticipantId).ToArray());
    }

    [Fact] public void Ranking_SameTime_SharedRankWithGap()
    {
        IReadOnlyList<ProtocolRow> rows = new RankingPolicy().Build(Data((1, 200), (2, 200), (3, 220)), 1);
        Assert.Equal(new[] { 1, 1, 3 }, rows.Select(r => r.Place).ToArray());
    }

    [Fact] public void Ranking_EmptyResults_IsEmpty()
    {
        Assert.Empty(new RankingPolicy().Build(Data(), 1));
    }

    [Fact] public void Ranking_SeparatesAgeGroups_EachStartsAtOne()
    {
        DataSnapshot data = Data((1, 200), (2, 210));
        data.Participants[1] = data.Participants[1] with { BirthDate = new DateOnly(2010, 1, 1) };
        IReadOnlyList<ProtocolRow> rows = new RankingPolicy().Build(data, 1);
        Assert.All(rows, row => Assert.Equal(1, row.Place));
    }

    [Fact] public void Ranking_FiltersOtherEvents()
    {
        DataSnapshot data = Data((1, 200));
        data.Results.Add(new FinishResult(2, 2, TimeSpan.FromSeconds(100)));
        Assert.Single(new RankingPolicy().Build(data, 1));
    }

    [Fact] public void Ranking_IgnoresUnregisteredResult()
    {
        DataSnapshot data = Data((1, 200));
        data.Results.Add(new FinishResult(1, 999, TimeSpan.FromSeconds(50)));
        Assert.Single(new RankingPolicy().Build(data, 1));
    }

    private static DataSnapshot Data(params (int Person, int Seconds)[] finishes)
    {
        DataSnapshot data = new();
        data.Events.Add(new(1, "Тест", new DateOnly(2026, 11, 5), 1000, EventStatus.Finished));
        for (int i = 1; i <= 3; i++)
        {
            data.Participants.Add(new(i, "Учасник " + i, new DateOnly(2000, 1, 1)));
            data.Registrations.Add(new(1, i, new DateOnly(2026, 10, 24)));
        }

        foreach (var item in finishes)
        {
            data.Results.Add(new(1, item.Person, TimeSpan.FromSeconds(item.Seconds)));
        }

        return data;
    }
}
