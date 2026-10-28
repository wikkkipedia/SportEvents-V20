namespace SportEvents.Core.Domain;

/// <summary>Незмінна відстань дистанції у метрах.</summary>
public sealed record CompetitionDistance
{
    /// <summary>Gets довжина дистанції у метрах, завжди додатна.</summary>
    public int Meters { get; }

    /// <summary>Initializes a new instance of the <see cref="CompetitionDistance"/> class.Створює тільки коректну дистанцію.</summary>
    public CompetitionDistance(int meters)
    {
        if (meters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(meters));
        }

        Meters = meters;
    }
}
