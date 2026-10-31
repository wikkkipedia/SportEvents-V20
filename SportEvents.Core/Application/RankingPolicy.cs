using SportEvents.Core.Domain;

namespace SportEvents.Core.Application;

/// <summary>Присвоює місця за часом, окремо для кожної вікової групи.</summary>
public sealed class RankingPolicy : IResultRanking
{
    /// <inheritdoc />
    public IReadOnlyList<ProtocolRow> Build(DataSnapshot data, int eventId)
    {
        ArgumentNullException.ThrowIfNull(data);
        SportsEvent competition = data.Events.Single(e => e.Id == eventId);
        var participants = data.Participants.ToDictionary(p => p.Id);
        var registered = data.Registrations
            .Where(r => r.EventId == eventId)
            .Select(r => r.ParticipantId)
            .ToHashSet();
        var completed = data.Results
            .Where(r => r.EventId == eventId && registered.Contains(r.ParticipantId))
            .Select(r => (Result: r, Participant: participants[r.ParticipantId]))
            .GroupBy(x => AgeRules.GetGroup(x.Participant.BirthDate, competition.EventDate));
        List<ProtocolRow> protocol = new();
        foreach (var group in completed.OrderBy(g => g.Key))
        {
            AppendGroupRows(protocol, group.Key, group);
        }

        return protocol;
    }

    private static void AppendGroupRows(
        List<ProtocolRow> protocol,
        AgeGroup group,
        IEnumerable<(FinishResult Result, Participant Participant)> items)
    {
        TimeSpan? previous = null;
        int place = 0;
        int index = 0;
        foreach (var item in items.OrderBy(x => x.Result.Elapsed).ThenBy(x => x.Participant.Id))
        {
            index++;
            if (previous != item.Result.Elapsed)
            {
                place = index;
            }

            previous = item.Result.Elapsed;
            protocol.Add(new(item.Participant.Id, item.Participant.FullName,
                group, item.Result.Elapsed, place));
        }
    }
}
