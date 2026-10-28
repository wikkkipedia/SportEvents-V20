namespace SportEvents.Core.Application;

/// <summary>Чистий порівнювач результатів для сортування протоколу.</summary>
public sealed class ResultTable
{
    /// <summary>Сортує протокол за групою, часом і номером без зміни вхідного списку.</summary>
    /// <returns></returns>
    public IReadOnlyList<ProtocolRow> Rank(IEnumerable<ProtocolRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        return rows.OrderBy(row => row.Group)
            .ThenBy(row => row.Elapsed)
            .ThenBy(row => row.ParticipantId)
            .ToArray();
    }

    /// <summary>Порівнює два результати в порядку протоколу.</summary>
    /// <returns></returns>
    public int Compare(ProtocolRow left, ProtocolRow right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        int groupResult = left.Group.CompareTo(right.Group);
        if (groupResult != 0)
        {
            return groupResult;
        }

        int timeResult = left.Elapsed.CompareTo(right.Elapsed);
        return timeResult != 0 ? timeResult : left.ParticipantId.CompareTo(right.ParticipantId);
    }
}
