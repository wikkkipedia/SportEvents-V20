using System;
using System.Collections.Generic;
namespace SportEvents.Lab02;

public class SportRegistration
{
    private readonly List<string[]> _lines = new();

    public SportRegistration(string id, string participantName)
    {
        Id = id;
        ParticipantName = participantName;
        CreatedAt = DateTime.Now;
    }

    public string Id { get; private set; }
    public string ParticipantName { get; private set; }
    public int Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void AddLine(string code, int lapCount, decimal lapTime)
    {
        string[] line = new string[3];
        line[0] = code;
        line[1] = lapCount.ToString();
        line[2] = lapTime.ToString();
        _lines.Add(line);
    }

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

        if (isRegular && total > 1000)
        {
            total *= 0.9m;
        }
        else if (total > 5000)
        {
            total *= 0.85m;
        }

        if (lineCount > 10)
        {
            total -= 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        total += total * 0.2m;
        return Math.Round(total, 2);
    }

    public bool TryChangeStatus(int targetStatus)
    {
        if (Status == 0 && targetStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && targetStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && targetStatus == 3)
        {
            Status = 3;
            return true;
        }

        return false;
    }

    public bool IsValid()
    {
        return Id is not null && Id.Length > 0
            && ParticipantName is not null && ParticipantName.Length > 2
            && _lines.Count > 0 && _lines.Count < 100
            && Status >= 0 && Status <= 3;
    }

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