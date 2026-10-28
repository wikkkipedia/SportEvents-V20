using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using SportEvents.Core.Infrastructure;
using Xunit;

namespace SportEvents.Tests;

public sealed class AdditionalRulesTests
{
    [Theory]
    [InlineData(2008, 11, 4, AgeGroup.Adult)]
    [InlineData(2008, 11, 6, AgeGroup.Junior)]
    [InlineData(1986, 11, 4, AgeGroup.Masters)]
    [InlineData(1986, 11, 6, AgeGroup.Adult)]
    public void AgeRules_DayAroundBirthday_CorrectGroup(int year, int month, int day, AgeGroup expected)
    {
        DateOnly eventDate = new(2026, 11, 5);
        AgeGroup actual = AgeRules.GetGroup(new DateOnly(year, month, day), eventDate);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AgeRules_LeapDayBeforeBirthday_RemainsJunior()
    {
        AgeGroup actual = AgeRules.GetGroup(new DateOnly(2008, 2, 29), new DateOnly(2026, 2, 28));
        Assert.Equal(AgeGroup.Junior, actual);
    }

    [Fact]
    public void AgeRules_LeapDayAfterBirthday_IsAdult()
    {
        AgeGroup actual = AgeRules.GetGroup(new DateOnly(2008, 2, 29), new DateOnly(2026, 3, 1));
        Assert.Equal(AgeGroup.Adult, actual);
    }

    [Fact]
    public void AgeRules_Exactly40Years_IsMasters()
    {
        AgeGroup actual = AgeRules.GetGroup(new DateOnly(1986, 11, 5), new DateOnly(2026, 11, 5));
        Assert.Equal(AgeGroup.Masters, actual);
    }

    [Fact]
    public void AgeRules_Exactly18Years_IsAdult()
    {
        AgeGroup actual = AgeRules.GetGroup(new DateOnly(2008, 11, 5), new DateOnly(2026, 11, 5));
        Assert.Equal(AgeGroup.Adult, actual);
    }

    [Fact]
    public void JsonRepository_RoundtripDuration_KeepsMilliseconds()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            JsonEventRepository repo = new(path);
            DataSnapshot snapshot = new();
            snapshot.Results.Add(new FinishResult(1, 1, TimeSpan.FromMilliseconds(240123)));
            repo.Save(snapshot);
            Assert.Equal(TimeSpan.FromMilliseconds(240123), repo.Load().Results.Single().Elapsed);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void JsonRepository_SaveTwice_OverwritesOldSnapshot()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            JsonEventRepository repo = new(path);
            DataSnapshot snapshot = new();
            snapshot.Participants.Add(new(1, "A", new DateOnly(2000, 1, 1)));
            repo.Save(snapshot);
            repo.Save(new DataSnapshot());
            Assert.Empty(repo.Load().Participants);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void JsonRepository_Subdirectory_CreatedOnSave()
    {
        string dir = Path.Combine(Path.GetTempPath(), "sportevents_" + Guid.NewGuid());
        string path = Path.Combine(dir, "nested", "data.json");
        try
        {
            new JsonEventRepository(path).Save(new DataSnapshot());
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void JsonRepository_EmptyPath_Rejects()
    {
        Assert.Throws<ArgumentException>(() => new JsonEventRepository(""));
    }

    [Fact]
    public void Ranking_TimeAcrossSeveralHours_SortsCorrectly()
    {
        DataSnapshot data = Input((1, 7200000), (2, 14400000));
        IReadOnlyList<ProtocolRow> rows = new RankingPolicy().Build(data, 1);
        Assert.Equal(1, rows[0].ParticipantId);
        Assert.Equal(1, rows[0].Place);
        Assert.Equal(2, rows[1].Place);
    }

    [Fact]
    public void Ranking_UsesParticipantIdForStableOrderOfTie()
    {
        DataSnapshot data = Input((3, 240000), (1, 240000), (2, 240000));
        IReadOnlyList<ProtocolRow> rows = new RankingPolicy().Build(data, 1);
        Assert.Equal(new[] { 1, 2, 3 }, rows.Select(r => r.ParticipantId).ToArray());
    }

    [Fact]
    public void Ranking_EmptyGroupCannotAffectOtherGroup()
    {
        DataSnapshot data = Input((1, 240000), (2, 250000));
        data.Participants[1] = data.Participants[1] with { BirthDate = new DateOnly(1960, 1, 1) };
        IReadOnlyList<ProtocolRow> rows = new RankingPolicy().Build(data, 1);
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal(1, r.Place));
    }

    [Fact]
    public void Ranking_UnknownEvent_ThrowsToCaller()
    {
        DataSnapshot data = Input((1, 240000));
        Assert.Throws<InvalidOperationException>(() => new RankingPolicy().Build(data, 99));
    }

    [Fact]
    public void Ranking_ResultFromOtherEvent_DoesNotAppear()
    {
        DataSnapshot data = Input((1, 240000));
        data.Results.Add(new FinishResult(2, 2, TimeSpan.FromMinutes(1)));
        Assert.Single(new RankingPolicy().Build(data, 1));
    }

    private static DataSnapshot Input(params (int Person, int Milliseconds)[] finishes)
    {
        DataSnapshot data = new();
        data.Events.Add(new(1, "Тест", new DateOnly(2026, 11, 5), 1000, EventStatus.Open));
        for (int i = 1; i <= 3; i++)
        {
            data.Participants.Add(new(i, "Тест " + i, new DateOnly(2000, 1, 1)));
            data.Registrations.Add(new(1, i, new DateOnly(2026, 10, 24)));
        }

        foreach (var finish in finishes)
        {
            data.Results.Add(new FinishResult(1, finish.Person,
                TimeSpan.FromMilliseconds(finish.Milliseconds)));
        }

        return data;
    }
}
