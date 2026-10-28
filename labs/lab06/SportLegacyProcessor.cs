using System;
using System.Collections.Generic;

namespace SportEvents.Lab06;

public class SportsParticipant
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public string Kind = "regular";
    public int DoneCount;
    public DateTime SinceUtc;

    public string Describe()
    {
        string text = Name.Trim().ToUpper();
        if (Kind == "vip") text += " [VIP]";
        if (DoneCount > 10) text += " [ЛОЯЛЬНИЙ]";
        text += " <" + Email.ToLower() + ">";
        int years = DateTime.Now.Year - SinceUtc.Year;
        text += " стаж " + years;
        return text;
    }
}

public class SportsResultLine
{
    public string Code = "";
    public int Qty;
    public decimal Price;
    public string Group = "";
}

public class SportLegacyProcessor
{
    private const decimal MaxDiscount = 500m;
    private const decimal FreeShippingThreshold = 1000m;
    private const decimal ShippingCost = 60m;
    private readonly List<string> _log = new();

    public string Handle(SportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        int docId = request.DocId;
        string clientName = request.ClientName;
        string? clientMail = request.ClientMail;
        string clientKind = request.ClientKind;
        int clientDone = request.ClientDone;
        List<SportsResultLine>? items = request.Items;
        string state = request.State;
        string currency = request.Currency;
        bool sendMail = request.SendMail;

        if (items is null) return "ERR: null";
        if (items.Count == 0) return "ERR: empty";
        if (state != "new" && state != "paid") return "ERR: state";
        if (clientMail is null || !clientMail.Contains("@")) return "ERR: mail";
        _log.Add("ok " + docId);

        decimal subtotal = CalculateSubtotal(items);

        decimal discount = CalculateDiscount(subtotal, clientKind, clientDone);

        decimal ship = CalculateShipping(subtotal - discount);
        decimal total = subtotal - discount + ship;

        string txt = BuildReport(new SportReportData
        {
            DocId = docId, ClientName = clientName, Items = items,
            Discount = discount, Ship = ship, Total = total, Currency = currency
        });

        if (sendMail)
        {
            _log.Add("mail -> " + clientMail);
        }
        return txt;
    }

    public decimal Preview(string clientKind, int clientDone, List<SportsResultLine> items)
    {
        decimal s = CalculateSubtotal(items);
        decimal d = CalculateDiscount(s, clientKind, clientDone);
        decimal sh = CalculateShipping(s - d);
        return s - d + sh;
    }

    public string DescribeClient(SportsParticipant participant)
    {
        return participant.Describe();
    }

    private sealed record SportReportData
    {
        public int DocId { get; init; }
        public string ClientName { get; init; } = "";
        public List<SportsResultLine> Items { get; init; } = new();
        public decimal Discount { get; init; }
        public decimal Ship { get; init; }
        public decimal Total { get; init; }
        public string Currency { get; init; } = "";
    }

    private static string BuildReport(SportReportData data)
    {
        string txt = "Документ #" + data.DocId + "\n";
        txt += "Клієнт: " + data.ClientName + "\n";
        for (int i = 0; i < data.Items.Count; i++)
        {
            txt += data.Items[i].Code + " x" + data.Items[i].Qty + " = " +
                (data.Items[i].Qty * data.Items[i].Price).ToString("0.00") + " " + data.Currency + "\n";
        }
        txt += "Знижка: " + data.Discount.ToString("0.00") + " " + data.Currency + "\n";
        txt += "Доставка: " + data.Ship.ToString("0.00") + " " + data.Currency + "\n";
        txt += "Разом: " + data.Total.ToString("0.00") + " " + data.Currency + "\n";

        return txt;
    }

    private static decimal CalculateDiscount(decimal subtotal, string kind, int done)
    {
        decimal discount = 0m;
        if (kind == "vip")
        {
            discount = subtotal * 0.15m;
            if (discount > MaxDiscount) discount = MaxDiscount;
        }
        else if (kind == "staff")
        {
            discount = subtotal * 0.30m;
            if (discount > MaxDiscount) discount = MaxDiscount;
        }
        else if (done > 10)
        {
            discount = subtotal * 0.05m;
            if (discount > MaxDiscount) discount = MaxDiscount;
        }

        return discount;
    }

    private static decimal CalculateShipping(decimal amount)
    {
        return amount < FreeShippingThreshold ? ShippingCost : 0m;
    }

    private static decimal CalculateSubtotal(List<SportsResultLine> items)
    {
        decimal total = 0m;
        foreach (var line in items)
        {
            total += line.Qty * line.Price;
        }

        return total;
    }

    public string DumpLog()
    {
        var builder = new System.Text.StringBuilder();
        foreach (string entry in _log)
        {
            builder.Append(entry).Append('\n');
        }

        return builder.ToString();
    }
}
