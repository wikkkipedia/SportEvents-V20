namespace SportEvents.Core.Application;

/// <summary>Валідує та підсумовує час проходження окремих відрізків дистанції.</summary>
public sealed class ElapsedTimePolicy
{
    /// <summary>Повертає суму додатних часових відрізків.</summary>
    /// <param name="splits">Перелік відрізків у мілісекундній точності.</param>
    /// <returns>Загальна тривалість дистанції.</returns>
    public TimeSpan SumSplits(IReadOnlyList<TimeSpan> splits)
    {
        ArgumentNullException.ThrowIfNull(splits);
        if (splits.Count == 0)
        {
            throw new ArgumentException("Потрібен хоча б один відрізок.", nameof(splits));
        }

        TimeSpan totalDuration = TimeSpan.Zero;
        foreach (TimeSpan split in splits)
        {
            if (split <= TimeSpan.Zero || split >= TimeSpan.FromDays(1))
            {
                throw new ArgumentOutOfRangeException(nameof(splits), "Час відрізка має бути додатним і меншим за добу.");
            }

            totalDuration += split;
        }

        return totalDuration;
    }
}
