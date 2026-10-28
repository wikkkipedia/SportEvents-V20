namespace SportEvents.Core.Domain;

/// <summary>Незмінна відстань дистанції у метрах.</summary>
public sealed record CompetitionDistance
{
    /// <summary>Довжина дистанції у метрах, завжди додатна.</summary>
    public int Meters { get; }

    /// <summary>Створює тільки коректну дистанцію.</summary>
    public CompetitionDistance(int meters)
    {
        if (meters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(meters));
        }
        Meters = meters;
    }
}
