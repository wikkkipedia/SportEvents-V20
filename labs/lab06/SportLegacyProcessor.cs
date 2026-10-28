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
    private decimal _subtotal;
    private decimal _discount;
    private readonly List<string> _log = new();

    public string Handle(int docId, int clientId, string clientName,
        string? clientMail, string clientKind, int clientDone,
        List<SportsResultLine>? items, string state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (items != null)
        {
            if (items.Count > 0)
            {
                if (state == "new" || state == "paid")
                {
                    if (clientMail != null && clientMail.Contains("@"))
                    {
                        _log.Add("ok " + docId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _subtotal = 0m;
        for (int i = 0; i < items.Count; i++)
        {
            _subtotal += items[i].Qty * items[i].Price;
        }

        _discount = 0m;
        if (clientKind == "vip")
        {
            _discount = _subtotal * 0.15m;
            if (_discount > 500m) _discount = 500m;
        }
        else if (clientKind == "staff")
        {
            _discount = _subtotal * 0.30m;
            if (_discount > 500m) _discount = 500m;
        }
        else if (clientDone > 10)
        {
            _discount = _subtotal * 0.05m;
            if (_discount > 500m) _discount = 500m;
        }

        decimal ship = 0m;
        if (_subtotal - _discount < 1000m) ship = 60m;
        decimal total = _subtotal - _discount + ship;

        string txt = "Документ #" + docId + "\n";
        txt += "Клієнт: " + clientName + "\n";
        for (int i = 0; i < items.Count; i++)
        {
            txt += items[i].Code + " x" + items[i].Qty + " = " +
                (items[i].Qty * items[i].Price).ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + _discount.ToString("0.00") + " " + currency + "\n";
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
        decimal s = 0m;
        foreach (var it in items)
        {
            s += it.Qty * it.Price;
        }
        decimal d = 0m;
        if (clientKind == "vip")
        {
            d = s * 0.15m;
            if (d > 500m) d = 500m;
        }
        else if (clientKind == "staff")
        {
            d = s * 0.30m;
            if (d > 500m) d = 500m;
        }
        else if (clientDone > 10)
        {
            d = s * 0.05m;
            if (d > 500m) d = 500m;
        }
        decimal sh = 0m;
        if (s - d < 1000m) sh = 60m;
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

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
