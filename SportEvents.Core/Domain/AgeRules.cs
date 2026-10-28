namespace SportEvents.Core.Domain;

/// <summary>Правила обчислення повного віку та вибору групи.</summary>
public static class AgeRules
{
    private const int AdultStartingAge = 18;
    private const int MastersStartingAge = 40;
    /// <summary>Повертає вікову групу саме на дату проведення змагання.</summary>
    /// <param name="birthDate">Дата народження учасника.</param>
    /// <param name="eventDate">Дата, на яку визначається вік.</param>
    /// <returns>Junior, Adult або Masters.</returns>
    /// <exception cref="ArgumentException">Дата народження в майбутньому відносно старту.</exception>
    public static AgeGroup GetGroup(DateOnly birthDate, DateOnly eventDate)
    {
        if (birthDate > eventDate)
        {
            throw new ArgumentException("Дата народження пізніша за дату змагання.");
        }

        int age = eventDate.Year - birthDate.Year;
        if (birthDate > eventDate.AddYears(-age))
        {
            age--;
        }

        return age switch
        {
            < AdultStartingAge => AgeGroup.Junior,
            < MastersStartingAge => AgeGroup.Adult,
            _ => AgeGroup.Masters,
        };
    }
}
