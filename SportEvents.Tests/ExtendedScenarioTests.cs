using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using SportEvents.Core.Infrastructure;
using Xunit;

namespace SportEvents.Tests;

public sealed class ExtendedScenarioTests
{
    private static readonly DateOnly EventDate = new(2026, 11, 5);

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void AddParticipant_InvalidId_Rejects(int id)
    {
        var (service, _) = Make();
        Assert.Throws<ArgumentException>(() => service.AddParticipant(id, "Тест", new DateOnly(2000, 1, 1)));
    }

    [Fact]
    public void AddParticipant_NameWithSpaces_IsTrimmed()
    {
        var (service, repo) = Make();
        service.AddParticipant(6, "  Петро  ", new DateOnly(2000, 1, 1));
        Assert.Equal("Петро", repo.Load().Participants.Single().FullName);
    }

    [Fact]
    public void CreateEvent_NameWithSpaces_IsTrimmed()
    {
        var (service, repo) = Make();
        service.CreateEvent("  Марафон  ", EventDate, 42000);
        Assert.Equal("Марафон", repo.Load().Events.Single().Title);
    }

    [Fact]
    public void CreateEvent_NegativeDistance_Rejects()
    {
        var (service, _) = Make();
        Assert.Throws<ArgumentException>(() => service.CreateEvent("Марафон", EventDate, -1));
    }

    [Fact]
    public void CreateEvent_HighDistance_IsRecordedExactly()
    {
        var (service, repo) = Make();
        service.CreateEvent("Ультрамарафон", EventDate, 100000);
        Assert.Equal(100000, repo.Load().Events.Single().DistanceMeters);
    }

    [Fact]
    public void StartEvent_Cancelled_CannotReopen()
    {
        var (service, _) = Make();
        service.CreateEvent("А", EventDate, 1000);
        service.CancelEvent(1);
        Assert.Throws<DomainRuleException>(() => service.OpenEvent(1));
    }

    [Fact]
    public void FinishEvent_Planned_CannotFinish()
    {
        var (service, _) = Make();
        service.CreateEvent("А", EventDate, 1000);
        Assert.Throws<DomainRuleException>(() => service.FinishEvent(1));
    }

    [Fact]
    public void FinishEvent_Cancelled_CannotFinish()
    {
        var (service, _) = Make();
        service.CreateEvent("А", EventDate, 1000);
        service.CancelEvent(1);
        Assert.Throws<DomainRuleException>(() => service.FinishEvent(1));
    }

    [Fact]
    public void CancelEvent_Finished_CannotCancel()
    {
        var (service, _) = ReadyWithFinish();
        service.FinishEvent(1);
        Assert.Throws<DomainRuleException>(() => service.CancelEvent(1));
    }

    [Fact]
    public void FinishEvent_Twice_SecondRejected()
    {
        var (service, _) = ReadyWithFinish();
        service.FinishEvent(1);
        Assert.Throws<DomainRuleException>(() => service.FinishEvent(1));
    }

    [Fact]
    public void RecordResult_Finished_RejectsFurtherChanges()
    {
        var (service, _) = ReadyWithFinish();
        service.AddParticipant(2, "Другий", new DateOnly(2001, 1, 1));
        service.Enroll(1, 2);
        service.FinishEvent(1);
        Assert.Throws<DomainRuleException>(() => service.RecordResult(1, 2, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Enroll_Finished_RejectsNewAthlete()
    {
        var (service, _) = ReadyWithFinish();
        service.AddParticipant(2, "Другий", new DateOnly(2001, 1, 1));
        service.FinishEvent(1);
        Assert.Throws<DomainRuleException>(() => service.Enroll(1, 2));
    }

    [Fact]
    public void Enroll_Cancelled_RejectsNewAthlete()
    {
        var (service, _) = ReadyWithFinish();
        service.CancelEvent(1);
        Assert.Throws<DomainRuleException>(() => service.Enroll(1, 1));
    }

    [Fact]
    public void GenerateProtocol_Open_ReturnsRegisteredFinish()
    {
        var (service, _) = ReadyWithFinish();
        Assert.Equal(1, service.GenerateProtocol(1).Single().Place);
    }

    [Fact]
    public void GenerateProtocol_Finished_StillWorks()
    {
        var (service, _) = ReadyWithFinish();
        service.FinishEvent(1);
        Assert.Single(service.GenerateProtocol(1));
    }

    [Fact]
    public void GenerateProtocol_DoesNotModifyStorage()
    {
        var (service, repo) = ReadyWithFinish();
        int registrations = repo.Load().Registrations.Count;
        int results = repo.Load().Results.Count;
        _ = service.GenerateProtocol(1);
        Assert.Equal(registrations, repo.Load().Registrations.Count);
        Assert.Equal(results, repo.Load().Results.Count);
    }

    [Fact]
    public void GenerateProtocol_Cancelled_RejectsReport()
    {
        var (service, _) = ReadyWithFinish();
        service.CancelEvent(1);
        Assert.Throws<DomainRuleException>(() => service.GenerateProtocol(1));
    }

    [Fact]
    public void GenerateProtocol_UnknownEvent_RejectsReport()
    {
        var (service, _) = Make();
        Assert.Throws<DomainRuleException>(() => service.GenerateProtocol(999));
    }

    [Fact]
    public void FindEvents_EmptyRepository_ReturnsEmptyList()
    {
        var (service, _) = Make();
        Assert.Empty(service.FindEvents(null, null));
    }

    [Fact]
    public void FindEvents_TwoDates_AreChronological()
    {
        var (service, _) = Make();
        service.CreateEvent("Пізніший", EventDate.AddDays(1), 1000);
        service.CreateEvent("Раніший", EventDate, 1000);
        Assert.Equal("Раніший", service.FindEvents(null, null)[0].Title);
    }

    [Fact]
    public void FindEvents_CaseInsensitive_Matches()
    {
        var (service, _) = Make();
        service.CreateEvent("Marathon", EventDate, 1000);
        Assert.Single(service.FindEvents("MARATHON", null));
    }

    [Fact]
    public void FindEvents_UnknownName_ReturnsNothing()
    {
        var (service, _) = Make();
        service.CreateEvent("Marathon", EventDate, 1000);
        Assert.Empty(service.FindEvents("Triathlon", null));
    }

    [Fact]
    public void FindEvents_NoDateMatch_ReturnsNothing()
    {
        var (service, _) = Make();
        service.CreateEvent("Marathon", EventDate, 1000);
        Assert.Empty(service.FindEvents(null, EventDate.AddYears(1)));
    }

    [Fact]
    public void SeedDemoParticipants_GeneratedAcrossAllGroups()
    {
        var (service, repo) = Make();
        service.SeedDemoParticipants();
        AgeGroup[] groups = repo.Load().Participants
            .Select(p => AgeRules.GetGroup(p.BirthDate, EventDate))
            .Distinct().ToArray();
        Assert.Equal(3, groups.Length);
    }

    [Fact]
    public void SeedDemoParticipants_ExistingOne_DoesNotReplaceCatalog()
    {
        var (service, repo) = Make();
        service.AddParticipant(100, "Існуючий", new DateOnly(2000, 1, 1));
        service.SeedDemoParticipants();
        Assert.Single(repo.Load().Participants);
    }

    [Fact]
    public void Ranking_BirthOnEventDate_IsInJuniorGroup()
    {
        var (service, _) = Make();
        DateOnly today = new(2026, 10, 24);
        service.AddParticipant(1, "Новонароджений", today);
        service.CreateEvent("А", today, 1000);
        service.OpenEvent(1);
        service.Enroll(1, 1);
        service.RecordResult(1, 1, TimeSpan.FromMinutes(5));
        Assert.Equal(AgeGroup.Junior, service.GenerateProtocol(1).Single().Group);
    }

    [Fact]
    public void Ranking_TwoGroups_CalculateSeparatePlaces()
    {
        var (service, _) = ReadyWithFinish();
        service.AddParticipant(2, "Юніор", new DateOnly(2012, 1, 1));
        service.Enroll(1, 2);
        service.RecordResult(1, 2, TimeSpan.FromMinutes(3));
        Assert.All(service.GenerateProtocol(1), row => Assert.Equal(1, row.Place));
    }

    [Fact]
    public void Ranking_MillisecondsDifferent_SortStrictly()
    {
        var (service, _) = ReadyWithFinish();
        service.AddParticipant(2, "Другий", new DateOnly(2000, 1, 1));
        service.Enroll(1, 2);
        service.RecordResult(1, 2, TimeSpan.FromMilliseconds(240001));
        Assert.Equal(new[] { 1, 2 }, service.GenerateProtocol(1).Select(r => r.ParticipantId).ToArray());
    }

    [Fact]
    public void Ranking_ExactlySameMilliseconds_AssignsTie()
    {
        var (service, _) = ReadyWithFinish();
        service.AddParticipant(2, "Другий", new DateOnly(2000, 1, 1));
        service.Enroll(1, 2);
        service.RecordResult(1, 2, TimeSpan.FromMinutes(4));
        Assert.All(service.GenerateProtocol(1), row => Assert.Equal(1, row.Place));
    }

    [Fact]
    public void AddParticipant_FailedDuplicate_DoesNotOverwrite()
    {
        var (service, repo) = Make();
        service.AddParticipant(1, "Початкове", new DateOnly(2000, 1, 1));
        Assert.Throws<DomainRuleException>(() => service.AddParticipant(1, "Змінене", new DateOnly(2000, 1, 1)));
        Assert.Equal("Початкове", repo.Load().Participants.Single().FullName);
    }

    [Fact]
    public void RecordResult_FailedDuplicate_LeavesFirstTime()
    {
        var (service, repo) = ReadyWithFinish();
        Assert.Throws<DomainRuleException>(() => service.RecordResult(1, 1, TimeSpan.FromMinutes(5)));
        Assert.Equal(TimeSpan.FromMinutes(4), repo.Load().Results.Single().Elapsed);
    }

    private static (EventService Service, MemoryRepository Repo) Make()
    {
        MemoryRepository repo = new();
        EventService service = new(repo, new FixedClock(), new RankingPolicy(), new NullLogger());
        return (service, repo);
    }

    private static (EventService Service, MemoryRepository Repo) ReadyWithFinish()
    {
        var (service, repo) = Make();
        service.AddParticipant(1, "Перший", new DateOnly(2000, 1, 1));
        service.CreateEvent("А", EventDate, 1000);
        service.OpenEvent(1);
        service.Enroll(1, 1);
        service.RecordResult(1, 1, TimeSpan.FromMinutes(4));
        return (service, repo);
    }
}
