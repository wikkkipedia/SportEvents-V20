using SportEvents.Core.Application;
using SportEvents.Core.Domain;

namespace SportEvents.Core.Infrastructure;

/// <summary>Перевірка структури та зв'язків даних після читання зовнішнього JSON-файлу.</summary>
internal static class SnapshotValidator
{
    internal static DataSnapshot Validate(DataSnapshot? snapshot)
    {
        if (snapshot is null || snapshot.Participants is null || snapshot.Events is null ||
            snapshot.Registrations is null || snapshot.Results is null)
        {
            throw new InvalidDataException("JSON містить порожній або неповний стан.");
        }

        ValidateParticipants(snapshot.Participants);
        ValidateEvents(snapshot.Events);
        ValidateRegistrations(snapshot);
        ValidateResults(snapshot);
        return snapshot;
    }

    private static void ValidateParticipants(IEnumerable<Participant> participants)
    {
        HashSet<int> ids = new();
        foreach (Participant? participant in participants)
        {
            if (participant is null || participant.Id <= 0 || string.IsNullOrWhiteSpace(participant.FullName))
            {
                throw new InvalidDataException("JSON містить некоректного учасника.");
            }

            if (!ids.Add(participant.Id))
            {
                throw new InvalidDataException("JSON містить повторний номер учасника.");
            }
        }
    }

    private static void ValidateEvents(IEnumerable<SportsEvent> events)
    {
        HashSet<int> ids = new();
        foreach (SportsEvent? competition in events)
        {
            if (competition is null || competition.Id <= 0 || competition.DistanceMeters <= 0 ||
                string.IsNullOrWhiteSpace(competition.Title) || !Enum.IsDefined(competition.Status))
            {
                throw new InvalidDataException("JSON містить некоректне змагання.");
            }

            if (!ids.Add(competition.Id))
            {
                throw new InvalidDataException("JSON містить повторний номер змагання.");
            }
        }
    }

    private static void ValidateRegistrations(DataSnapshot snapshot)
    {
        Dictionary<int, Participant> people = snapshot.Participants.ToDictionary(p => p.Id);
        Dictionary<int, SportsEvent> events = snapshot.Events.ToDictionary(e => e.Id);
        HashSet<(int, int)> pairs = new();
        foreach (EventRegistration? item in snapshot.Registrations)
        {
            if (item is null || !people.TryGetValue(item.ParticipantId, out Participant? participant) ||
                !events.TryGetValue(item.EventId, out SportsEvent? competition) ||
                !pairs.Add((item.EventId, item.ParticipantId)))
            {
                throw new InvalidDataException("JSON містить некоректну реєстрацію.");
            }

            if (participant.BirthDate > competition.EventDate)
            {
                throw new InvalidDataException("JSON містить реєстрацію до народження учасника.");
            }
        }
    }

    private static void ValidateResults(DataSnapshot snapshot)
    {
        HashSet<(int, int)> registered = snapshot.Registrations
            .Select(r => (r.EventId, r.ParticipantId)).ToHashSet();
        HashSet<(int, int)> seen = new();
        foreach (FinishResult? item in snapshot.Results)
        {
            if (item is null || !registered.Contains((item.EventId, item.ParticipantId)) ||
                item.Elapsed <= TimeSpan.Zero || item.Elapsed >= TimeSpan.FromDays(1) ||
                item.Elapsed.Ticks % TimeSpan.TicksPerMillisecond != 0 ||
                !seen.Add((item.EventId, item.ParticipantId)))
            {
                throw new InvalidDataException("JSON містить некоректний фінішний результат.");
            }
        }
    }
}
