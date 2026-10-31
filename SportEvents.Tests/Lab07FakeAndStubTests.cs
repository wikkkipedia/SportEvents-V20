using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using Xunit;

namespace SportEvents.Tests;

public sealed class Lab07FakeAndStubTests
{
    private static readonly DateOnly EventDate = new(2026, 11, 5);

    [Fact]
    public void CreateEvent_FakeRepository_SavesCompetitionOnce()
    {
        RecordingMemoryRepository repository = new();
        EventService service = CreateService(repository);

        int id = service.CreateEvent("Sprint", EventDate, 1000);

        SportsEvent saved = Assert.Single(repository.Load().Events);
        Assert.Equal(id, saved.Id);
        Assert.Equal("Sprint", saved.Title);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public void Enroll_FixedClockStub_StoresSpecifiedDate()
    {
        RecordingMemoryRepository repository = new();
        EventService service = CreateService(repository);
        service.AddParticipant(1, "Runner", new DateOnly(2000, 1, 1));
        int eventId = service.CreateEvent("Sprint", EventDate, 1000);
        service.OpenEvent(eventId);

        service.Enroll(eventId, 1);

        EventRegistration registration = Assert.Single(repository.Load().Registrations);
        Assert.Equal(new DateOnly(2026, 10, 24), registration.RegisteredOn);
    }

    [Fact]
    public void AddParticipant_DuplicateIdentifier_DoesNotSaveAgain()
    {
        RecordingMemoryRepository repository = new();
        EventService service = CreateService(repository);
        service.AddParticipant(1, "Runner", new DateOnly(2000, 1, 1));
        int saveCallsBeforeFailure = repository.SaveCalls;

        Assert.Throws<DomainRuleException>(
            () => service.AddParticipant(1, "Other Runner", new DateOnly(2000, 1, 1)));

        Assert.Equal(saveCallsBeforeFailure, repository.SaveCalls);
        Assert.Single(repository.Load().Participants);
    }

    private static EventService CreateService(RecordingMemoryRepository repository)
    {
        // The in-memory repository is a fake; FixedClock returns a fixed value as a stub.
        return new EventService(repository, new FixedClock(), new RankingPolicy(), new NullLogger());
    }
}

internal sealed class RecordingMemoryRepository : IEventRepository
{
    private DataSnapshot _snapshot = new();

    public int SaveCalls { get; private set; }

    public DataSnapshot Load() => _snapshot;

    public void Save(DataSnapshot data)
    {
        _snapshot = data;
        SaveCalls++;
    }
}
