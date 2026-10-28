namespace SportEvents.Core.Application;

/// <summary>Запит на підрахунок часу відрізків одного учасника.</summary>
/// <param name="EventId">Номер змагання.</param>
/// <param name="ParticipantId">Номер учасника.</param>
/// <param name="Laps">Додатні тривалості відрізків.</param>
public sealed record LapSummaryRequest(int EventId, int ParticipantId, IReadOnlyList<TimeSpan> Laps);

/// <summary>Відповідь з обчисленим часом дистанції.</summary>
/// <param name="TotalElapsed">Підсумкова тривалість.</param>
/// <param name="LapCount">Кількість відрізків.</param>
public sealed record LapSummaryResult(TimeSpan TotalElapsed, int LapCount);

/// <summary>Чистий розрахунок підсумку, не виводить даних у консоль.</summary>
public sealed class LapSummaryCalculator
{
    private readonly ElapsedTimePolicy timePolicy;

    /// <summary>Initializes a new instance of the <see cref="LapSummaryCalculator"/> class.Створює розрахунок з окремою політикою часу.</summary>
    public LapSummaryCalculator(ElapsedTimePolicy timePolicy)
        => this.timePolicy = timePolicy ?? throw new ArgumentNullException(nameof(timePolicy));

    /// <summary>Перевіряє параметри та обчислює час дистанції.</summary>
    /// <returns></returns>
    public LapSummaryResult Calculate(LapSummaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.EventId <= 0 || request.ParticipantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Номер змагання та учасника має бути додатним.");
        }

        TimeSpan total = timePolicy.SumSplits(request.Laps);
        return new LapSummaryResult(total, request.Laps.Count);
    }
}
