using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using Xunit;

namespace SportEvents.Tests;

public sealed class EventServiceTests
{
    private static readonly DateOnly Day = new(2026, 11, 5);

    [Fact]
    public void CreateEvent_Valid_IsPlanned()
    {
        var (service, repo) = Create();
        int id = service.CreateEvent("Осінній забіг", Day, 5000);
        Assert.Equal(EventStatus.Planned, repo.Load().Events.Single(e => e.Id == id).Status);
    }

    [Fact]
    public void CreateEvent_InvalidDistance_Throws()
    {
        var (service, _) = Create();
        Assert.Throws<ArgumentException>(() => service.CreateEvent("Забіг", Day, 0));
    }

    [Fact]
    public void CreateEvent_EmptyTitle_Throws()
    {
        var (service, _) = Create();
        Assert.Throws<ArgumentException>(() => service.CreateEvent(" ", Day, 5000));
    }

    [Fact]
    public void CreateEvent_Twice_GivesDifferentIds()
    {
        var (service, _) = Create();
        Assert.NotEqual(service.CreateEvent("A", Day, 1000), service.CreateEvent("B", Day, 2000));
    }

    [Fact]
    public void OpenEvent_Planned_BecomesOpen()
    {
        var (service, repo) = Create();
        int id = service.CreateEvent("A", Day, 1000);
        service.OpenEvent(id);
        Assert.Equal(EventStatus.Open, repo.Load().Events.Single().Status);
    }

    [Fact]
    public void OpenEvent_AlreadyOpen_Throws()
    {
        var (service, _) = Create();
        int id = service.CreateEvent("A", Day, 1000);
        service.OpenEvent(id);
        Assert.Throws<DomainRuleException>(() => service.OpenEvent(id));
    }

    [Fact]
    public void OpenEvent_UnknownId_Throws()
    {
        var (service, _) = Create();
        Assert.Throws<DomainRuleException>(() => service.OpenEvent(999));
    }

    [Fact]
    public void AddParticipant_Valid_Saved()
    {
        var (service, repo) = Create();
        service.AddParticipant(1, "Марко", new DateOnly(2000, 1, 1));
        Assert.Single(repo.Load().Participants);
    }

    [Fact]
    public void AddParticipant_RepeatedId_Throws()
    {
        var (service, _) = Create();
        service.AddParticipant(1, "Марко", new DateOnly(2000, 1, 1));
        Assert.Throws<DomainRuleException>(() => service.AddParticipant(1, "Ігор", new DateOnly(2000, 1, 1)));
    }

    [Fact]
    public void AddParticipant_FutureBirthday_Throws()
    {
        var (service, _) = Create();
        Assert.Throws<ArgumentException>(() => service.AddParticipant(1, "Ігор", new DateOnly(2030, 1, 1)));
    }

    [Fact]
    public void AddParticipant_BlankName_Throws()
    {
        var (service, _) = Create();
        Assert.Throws<ArgumentException>(() => service.AddParticipant(1, " ", new DateOnly(2000, 1, 1)));
    }

    [Fact]
    public void Enroll_Planned_Throws()
    {
        var (service, _) = Create();
        int id = service.CreateEvent("A", Day, 1000);
        service.AddParticipant(1, "Перший", new DateOnly(2000, 1, 1));
        Assert.Throws<DomainRuleException>(() => service.Enroll(id, 1));
    }

    [Fact]
    public void Enroll_Open_AddsRegistration()
    {
        var (service, repo) = Started();
        service.Enroll(1, 1);
        Assert.Single(repo.Load().Registrations);
    }

    [Fact]
    public void Enroll_Duplicate_Throws()
    {
        var (service, _) = Started();
        service.Enroll(1, 1);
        Assert.Throws<DomainRuleException>(() => service.Enroll(1, 1));
    }

    [Fact]
    public void Enroll_NonExistingParticipant_Throws()
    {
        var (service, _) = Started();
        Assert.Throws<DomainRuleException>(() => service.Enroll(1, 999));
    }

    [Fact]
    public void Enroll_ParticipantNotBorn_Throws()
    {
        var (service, _) = Create();
        int id = service.CreateEvent("A", new DateOnly(1990, 1, 1), 1000);
        service.AddParticipant(1, "Перший", new DateOnly(2000, 1, 1));
        service.OpenEvent(id);
        Assert.Throws<ArgumentException>(() => service.Enroll(id, 1));
    }

    [Fact]
    public void RecordResult_Valid_Saved()
    {
        var (service, repo) = Started();
        service.Enroll(1, 1);
        service.RecordResult(1, 1, TimeSpan.FromMinutes(4));
        Assert.Single(repo.Load().Results);
    }

    [Fact]
    public void RecordResult_Unregistered_Throws()
    {
        var (service, _) = Started();
        Assert.Throws<DomainRuleException>(() => service.RecordResult(1, 1, TimeSpan.FromMinutes(4)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecordResult_NonPositiveTime_Throws(int seconds)
    {
        var (service, _) = Started();
        service.Enroll(1, 1);
        Assert.Throws<ArgumentException>(() => service.RecordResult(1, 1, TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void RecordResult_SubMillisecond_Throws()
    {
        var (service, _) = Started();
        service.Enroll(1, 1);
        Assert.Throws<ArgumentException>(() => service.RecordResult(1, 1, TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void RecordResult_FullDay_Throws()
    {
        var (service, _) = Started();
        service.Enroll(1, 1);
        Assert.Throws<ArgumentException>(() => service.RecordResult(1, 1, TimeSpan.FromDays(1)));
    }

    [Fact]
    public void RecordResult_Duplicate_Throws()
    {
        var (service, _) = Started();
        service.Enroll(1, 1);
        service.RecordResult(1, 1, TimeSpan.FromMinutes(4));
        Assert.Throws<DomainRuleException>(() => service.RecordResult(1, 1, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void FinishEvent_WithoutResult_Throws()
    {
        var (service, _) = Started();
        Assert.Throws<DomainRuleException>(() => service.FinishEvent(1));
    }

    [Fact]
    public void FinishEvent_WithResult_ChangesState()
    {
        var (service, repo) = Started();
        service.Enroll(1, 1);
        service.RecordResult(1, 1, TimeSpan.FromMinutes(4));
        service.FinishEvent(1);
        Assert.Equal(EventStatus.Finished, repo.Load().Events.Single().Status);
    }

    [Fact]
    public void CancelEvent_Planned_ChangesState()
    {
        var (service, repo) = Create();
        int id = service.CreateEvent("A", Day, 1000);
        service.CancelEvent(id);
        Assert.Equal(EventStatus.Cancelled, repo.Load().Events.Single().Status);
    }

    [Fact]
    public void CancelEvent_Open_ChangesState()
    {
        var (service, repo) = Started();
        service.CancelEvent(1);
        Assert.Equal(EventStatus.Cancelled, repo.Load().Events.Single().Status);
    }

    [Fact]
    public void CancelEvent_Twice_Throws()
    {
        var (service, _) = Started();
        service.CancelEvent(1);
        Assert.Throws<DomainRuleException>(() => service.CancelEvent(1));
    }

    [Fact]
    public void GenerateProtocol_Planned_Throws()
    {
        var (service, _) = Create();
        int id = service.CreateEvent("A", Day, 1000);
        Assert.Throws<DomainRuleException>(() => service.GenerateProtocol(id));
    }

    [Fact]
    public void FindEvents_Title_Filters()
    {
        var (service, _) = Create();
        service.CreateEvent("Осінній", Day, 1000);
        service.CreateEvent("Весняний", Day, 1000);
        Assert.Single(service.FindEvents("осін", null));
    }

    [Fact]
    public void FindEvents_Date_Filters()
    {
        var (service, _) = Create();
        service.CreateEvent("A", Day, 1000);
        service.CreateEvent("B", Day.AddDays(1), 1000);
        Assert.Single(service.FindEvents(null, Day));
    }

    [Fact]
    public void FindEvents_NameAndDate_Filters()
    {
        var (service, _) = Create();
        service.CreateEvent("Осінній", Day, 1000);
        service.CreateEvent("Осінній", Day.AddDays(1), 1000);
        Assert.Single(service.FindEvents("Осін", Day));
    }

    [Fact]
    public void SeedDemoParticipants_Empty_Creates60()
    {
        var (service, _) = Create();
        service.SeedDemoParticipants();
        Assert.Equal(60, service.ParticipantCount());
    }

    [Fact]
    public void SeedDemoParticipants_Twice_DoesNotDuplicate()
    {
        var (service, _) = Create();
        service.SeedDemoParticipants();
        service.SeedDemoParticipants();
        Assert.Equal(60, service.ParticipantCount());
    }

    [Fact]
    public void Enroll_UsesInjectedClock_RegistrationDateIsFixed()
    {
        var (service, repo) = Started();
        service.Enroll(1, 1);
        Assert.Equal(new DateOnly(2026, 10, 24), repo.Load().Registrations.Single().RegisteredOn);
    }

    private static (EventService Service, MemoryRepository Repo) Create()
    {
        MemoryRepository repo = new();
        EventService service = new(repo, new FixedClock(), new RankingPolicy(), new NullLogger());
        return (service, repo);
    }

    private static (EventService Service, MemoryRepository Repo) Started()
    {
        var (service, repo) = Create();
        service.AddParticipant(1, "Перший", new DateOnly(2000, 1, 1));
        service.CreateEvent("A", Day, 1000);
        service.OpenEvent(1);
        return (service, repo);
    }
}

internal sealed class MemoryRepository : IEventRepository
{
    private DataSnapshot _data = new();
    public DataSnapshot Load() => _data;
    public void Save(DataSnapshot data) => _data = data;
}

internal sealed class FixedClock : IClock
{
    public DateTimeOffset Now => new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
}

internal sealed class NullLogger : IAuditLogger
{
    public void Write(string level, string scenario, string message) { }
}
