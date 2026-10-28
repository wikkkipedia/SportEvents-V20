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

    public string Handle(int docId, int clientId, string clientName,
        string? clientMail, string clientKind, int clientDone,
        List<SportsResultLine>? items, string state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (items is null) return "ERR: null";
        if (items.Count == 0) return "ERR: empty";
        if (state != "new" && state != "paid") return "ERR: state";
        if (clientMail is null || !clientMail.Contains("@")) return "ERR: mail";
        _log.Add("ok " + docId);

        decimal subtotal = CalculateSubtotal(items);

        decimal discount = CalculateDiscount(subtotal, clientKind, clientDone);

        decimal ship = 0m;
        if (subtotal - discount < FreeShippingThreshold) ship = ShippingCost;
        decimal total = subtotal - discount + ship;

        string txt = "Документ #" + docId + "\n";
        txt += "Клієнт: " + clientName + "\n";
        for (int i = 0; i < items.Count; i++)
        {
            txt += items[i].Code + " x" + items[i].Qty + " = " +
                (items[i].Qty * items[i].Price).ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + discount.ToString("0.00") + " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") + " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") + " " + currency + "\n";

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
        decimal sh = 0m;
        if (s - d < FreeShippingThreshold) sh = ShippingCost;
        return s - d + sh;
    }

    public string DescribeClient(SportsParticipant c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == "vip") s += " [VIP]";
        if (c.DoneCount > 10) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
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
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
