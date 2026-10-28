using System.Globalization;

namespace SportEvents.Core.Application;

/// <summary>Перевіряє час із консолі до передавання в предметне ядро.</summary>
public static class RaceTimeInput
{
    private static readonly string[] AllowedFormats = { @"mm\:ss\.fff", @"hh\:mm\:ss\.fff" };

    /// <summary>Розбирає часову позначку у форматі mm:ss.fff або hh:mm:ss.fff.</summary>
    /// <returns></returns>
    public static OperationResult<TimeSpan> TryParse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return OperationResult<TimeSpan>.Fail("Введіть час дистанції.");
        }

        if (!TimeSpan.TryParseExact(input.Trim(), AllowedFormats, CultureInfo.InvariantCulture,
            out TimeSpan elapsed))
        {
            return OperationResult<TimeSpan>.Fail("Неправильний формат часу. Приклад: 01:25.500");
        }

        if (elapsed <= TimeSpan.Zero || elapsed >= TimeSpan.FromDays(1))
        {
            return OperationResult<TimeSpan>.Fail("Час повинен бути додатним і меншим за добу.");
        }

        return OperationResult<TimeSpan>.Ok(elapsed);
    }
}
