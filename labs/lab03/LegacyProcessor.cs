namespace SportEvents.Lab03;

/// <summary>Рядок обліку дистанції у навчальному прикладі.</summary>
public sealed class RaceChargeLine
{
    public string Code { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>Обробник, який після рефакторингу делегує розрахунки малим методам.</summary>
public static class LegacyProcessor
{
    /// <summary>Розраховує підсумок і переходить до наступного стану.</summary>
    public static RaceOutcome Process(RaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string? error = Validate(request);
        if (error is not null)
        {
            Console.WriteLine(error);
            return new RaceOutcome(-1m, request.Status);
        }

        decimal total = CalculateLines(request.Lines);
        total = ApplyDiscounts(total, request);
        total = ApplyDelivery(total, request);
        string next = TransitionStatus(request.Status, total);
        if (next == "Cancelled")
        {
            total = 0m;
        }

        if (request.PrintToConsole)
        {
            Console.Write(BuildReport(request, total, next));
        }

        return new RaceOutcome(total, next);
    }

    private static string? Validate(RaceRequest request)
    {
        if (request.DocId <= 0) return "Помилка: номер документа";
        if (string.IsNullOrEmpty(request.ClientName)) return "Помилка: немає клієнта";
        if (request.ClientEmail == null || !request.ClientEmail.Contains("@"))
        {
            return "Помилка: пошта клієнта";
        }

        if (request.Lines == null || request.Lines.Count == 0) return "Помилка: немає позицій";
        if (request.Status != "New" && request.Status != "Paid" &&
            request.Status != "Shipped" && request.Status != "Cancelled")
        {
            return "Помилка: невідомий стан";
        }

        return ValidateLines(request.Lines);
    }

    private static string? ValidateLines(IReadOnlyList<RaceChargeLine> lines)
    {
        foreach (RaceChargeLine line in lines)
        {
            if (line.Quantity <= 0) return "Помилка: кількість";
            if (line.UnitPrice < 0m) return "Помилка: ціна";
        }

        return null;
    }

    private static decimal CalculateLines(IReadOnlyList<RaceChargeLine> lines)
    {
        decimal total = 0m;
        foreach (RaceChargeLine line in lines)
        {
            decimal sum = line.Quantity * line.UnitPrice;
            if (line.Quantity >= 10) sum *= 0.95m;
            total += sum;
        }

        return total;
    }

    private static decimal ApplyDiscounts(decimal total, RaceRequest request)
    {
        if (request.IsRegular) total *= 0.93m;
        if (total > 5000m) total *= 0.9m;
        else if (total > 2000m) total *= 0.95m;
        if (request.CouponCode == "SALE10") total -= total * 0.1m;
        else if (request.CouponCode == "MINUS200" && total > 1000m) total -= 200m;
        if (request.CreatedAt.DayOfWeek == DayOfWeek.Sunday) total *= 0.98m;
        return total;
    }

    private static decimal ApplyDelivery(decimal total, RaceRequest request)
    {
        if (total < 1000m) total += request.DeliveryPrice;
        if (total < 0m) total = 0m;
        return Math.Round(total, 2);
    }

    private static string TransitionStatus(string status, decimal total)
    {
        if (status == "New" && total > 0m) return "Paid";
        if (status == "Paid") return "Shipped";
        return status;
    }

    private static string BuildReport(RaceRequest request, decimal total, string next)
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("Документ № " + request.DocId);
        report.AppendLine("Клієнт: " + request.ClientName);
        report.AppendLine("Пошта: " + request.ClientEmail);
        foreach (RaceChargeLine line in request.Lines)
        {
            report.AppendLine(line.Code + " x " + line.Quantity);
        }

        report.AppendLine("Разом: " + total);
        report.AppendLine("Стан: " + next);
        return report.ToString();
    }
}
