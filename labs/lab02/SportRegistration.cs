using System;
using System.Collections.Generic;

namespace SportEvents.Lab02;

/// <summary>Реєстрація учасника в навчальному модулі SportEvents.</summary>
public class SportRegistration
{
    private const int StatusNew = 0;
    private const int StatusOpen = 1;
    private const int StatusFinished = 2;
    private const int StatusCancelled = 3;
    private const int MinParticipantNameLength = 3;
    private const int MaxLineCount = 100;
    private const int BulkLineCount = 10;
    private const decimal BulkDiscountAmount = 100m;
    private const decimal RegularDiscountThreshold = 1000m;
    private const decimal RegularDiscountFactor = 0.9m;
    private const decimal LargeOrderThreshold = 5000m;
    private const decimal LargeOrderFactor = 0.85m;
    private const decimal FeeRate = 0.2m;
    private readonly List<string[]> _lines = new();

    /// <summary>Створює запис реєстрації.</summary>
    /// <param name="id">Номер реєстрації.</param>
    /// <param name="participantName">Ім'я учасника.</param>
    public SportRegistration(string id, string participantName)
    {
        Id = id;
        ParticipantName = participantName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>Отримує номер реєстрації.</summary>
    public string Id { get; private set; }

    /// <summary>Отримує ім'я учасника.</summary>
    public string ParticipantName { get; private set; }

    /// <summary>Отримує поточний стан реєстрації.</summary>
    public int Status { get; private set; }

    /// <summary>Отримує час створення.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Додає запис часу на дистанції.</summary>
    /// <param name="code">Позначення дистанції.</param>
    /// <param name="lapCount">Кількість кіл.</param>
    /// <param name="lapTime">Час на коло.</param>
    public void AddLine(string code, int lapCount, decimal lapTime)
    {
        string[] line = new string[3];
        line[0] = code;
        line[1] = lapCount.ToString();
        line[2] = lapTime.ToString();
        _lines.Add(line);
    }

    /// <summary>Обчислює суму навчального документа за історичними правилами.</summary>
    /// <param name="isRegular">Чи застосувати знижку за навчальним правилом.</param>
    /// <returns>Остаточний підсумок.</returns>
    public decimal CalculateTotal(bool isRegular)
    {
        decimal total = 0;
        int lineCount = 0;
        for (int index = 0; index < _lines.Count; index++)
        {
            int lapCount = int.Parse(_lines[index][1]);
            decimal lapTime = decimal.Parse(_lines[index][2]);
            total += lapCount * lapTime;
            lineCount++;
        }

        if (isRegular && total > RegularDiscountThreshold)
        {
            total *= RegularDiscountFactor;
        }
        else if (total > LargeOrderThreshold)
        {
            total *= LargeOrderFactor;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

        // Важливо: збережено порядок операцій та масштаб decimal із вихідного коду.
        total += total * FeeRate;
        return Math.Round(total, 2);
    }

    /// <summary>Змінює стан, тільки якщо перехід дозволено.</summary>
    /// <param name="targetStatus">Бажаний стан.</param>
    /// <returns>true для дозволеного переходу; інакше false.</returns>
    public bool TryChangeStatus(int targetStatus)
    {
        if (Status == StatusNew && targetStatus == StatusOpen)
        {
            Status = StatusOpen;
            return true;
        }

        if (Status == StatusOpen && targetStatus == StatusFinished)
        {
            Status = StatusFinished;
            return true;
        }

        if (Status == StatusNew && targetStatus == StatusCancelled)
        {
            Status = StatusCancelled;
            return true;
        }

        return false;
    }

    /// <summary>Перевіряє, чи реєстрація містить необхідні дані.</summary>
    /// <returns>true, якщо поля відповідають вимогам навчального модуля.</returns>
    public bool IsValid()
    {
        return Id is not null && Id.Length > 0
            && ParticipantName is not null
            && ParticipantName.Length >= MinParticipantNameLength
            && _lines.Count > 0 && _lines.Count < MaxLineCount
            && Status >= StatusNew && Status <= StatusCancelled;
    }

    /// <summary>Формує текстовий звіт без друку в консоль.</summary>
    /// <returns>Текстовий звіт про дистанції та підсумок.</returns>
    public string BuildReport()
    {
        string report = "";
        for (int index = 0; index < _lines.Count; index++)
        {
            report += "Дистанція: " + _lines[index][0]
                + "; кіл: " + _lines[index][1]
                + "; час: " + _lines[index][2]
                + "; сума: "
                + (int.Parse(_lines[index][1]) * decimal.Parse(_lines[index][2]))
                + "\n";
        }

        report += "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }

    /// <summary>Знаходить запис реєстрації за ідентифікатором.</summary>
    /// <param name="registrations">Список реєстрацій.</param>
    /// <param name="id">Шуканий ідентифікатор.</param>
    /// <returns>Знайдений запис або null.</returns>
    public static SportRegistration? FindById(List<SportRegistration> registrations, string id)
    {
        for (int index = 0; index < registrations.Count; index++)
        {
            if (registrations[index].Id == id)
            {
                return registrations[index];
            }
        }

        return null;
    }
}
