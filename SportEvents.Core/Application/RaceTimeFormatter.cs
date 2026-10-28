using System.Globalization;
namespace SportEvents.Core.Application;

/// <summary>Точне форматування часу проходження дистанції.</summary>
public static class RaceTimeFormatter
{
    /// <summary>Форматує час до мілісекунд без локальної залежності.</summary>
    public static string Format(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero || duration >= TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }
        return duration.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
    }
}
