namespace SportEvents.Lab03;
public sealed class RaceChargeLine
{
    public string Code { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public static class LegacyProcessor
{
    public static decimal Process(
        int docId, string clientName, string clientEmail,
        bool isRegular, List<RaceChargeLine> lines,
        DateOnly createdAt, string couponCode, string status,
        bool printToConsole, decimal deliveryPrice,
        out string newStatus)
    {
        newStatus = status;
        decimal total = 0;
        if (docId <= 0)
        {
            Console.WriteLine("Помилка: номер документа");
            return -1;
        }
        if (clientName == null || clientName == "")
        {
            Console.WriteLine("Помилка: немає клієнта");
            return -1;
        }
        if (clientEmail == null || !clientEmail.Contains("@"))
        {
            Console.WriteLine("Помилка: пошта клієнта");
            return -1;
        }
        if (lines == null || lines.Count == 0)
        {
            Console.WriteLine("Помилка: немає позицій");
            return -1;
        }
        if (status != "New" && status != "Paid" &&
            status != "Shipped" && status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1;
        }
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Quantity <= 0)
            {
                Console.WriteLine("Помилка: кількість");
                return -1;
            }
            if (lines[i].UnitPrice < 0)
            {
                Console.WriteLine("Помилка: ціна");
                return -1;
            }
            decimal sum = lines[i].Quantity * lines[i].UnitPrice;
            if (lines[i].Quantity >= 10)
                sum = sum * 0.95m;
            total = total + sum;
        }
        if (isRegular)
            total = total * 0.93m;
        if (total > 5000)
            total = total * 0.9m;
        else if (total > 2000)
            total = total * 0.95m;
        if (couponCode == "SALE10")
            total = total - total * 0.1m;
        else if (couponCode == "MINUS200" && total > 1000)
            total = total - 200;
        if (createdAt.DayOfWeek == DayOfWeek.Sunday)
            total = total * 0.98m;
        if (total < 1000)
            total = total + deliveryPrice;
        if (total < 0)
            total = 0;
        total = Math.Round(total, 2);
        if (status == "New" && total > 0)
            newStatus = "Paid";
        else if (status == "Paid")
            newStatus = "Shipped";
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0;
        }
        if (printToConsole)
        {
            Console.WriteLine("Документ № " + docId);
            Console.WriteLine("Клієнт: " + clientName);
            Console.WriteLine("Пошта: " + clientEmail);
            for (int i = 0; i < lines.Count; i++)
                Console.WriteLine(lines[i].Code + " x " + lines[i].Quantity);
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
