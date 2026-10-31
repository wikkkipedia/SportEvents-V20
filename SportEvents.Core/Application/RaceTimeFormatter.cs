using System.Globalization;

namespace SportEvents.Core.Application;

/// <summary>Точне форматування часу проходження дистанції.</summary>
public static class RaceTimeFormatter
{
    /// <summary>Форматує час до мілісекунд без локальної залежності.</summary>
    /// <returns></returns>
    public static string Format(TimeSpan duration)
    {
        ValidateDuration(duration);
        return duration.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
    }

    private static bool IsValidRaceDuration(TimeSpan duration)
        => duration > TimeSpan.Zero && duration < TimeSpan.FromDays(1);

    private static void ValidateDuration(TimeSpan duration)
    {
        if (!IsValidRaceDuration(duration))
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }
    }
}
