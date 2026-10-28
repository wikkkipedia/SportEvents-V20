from pathlib import Path
p=Path(__file__).resolve().parents[1]/"labs/lab06/SportLegacyProcessor.cs"
s=p.read_text(encoding="utf-8")
start=s.index("        decimal discount = 0m;")
end=s.index("        decimal ship = 0m;",start)
s=s[:start]+"        decimal discount = CalculateDiscount(subtotal, clientKind, clientDone);\n\n"+s[end:]
start=s.index("        decimal d = 0m;")
end=s.index("        decimal sh = 0m;",start)
s=s[:start]+"        decimal d = CalculateDiscount(s, clientKind, clientDone);\n"+s[end:]
anchor="    private static decimal CalculateSubtotal(List<SportsResultLine> items)"
method="""    private static decimal CalculateDiscount(decimal subtotal, string kind, int done)
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

"""
assert s.count(anchor)==1
p.write_text(s.replace(anchor,method+anchor),encoding="utf-8",newline="\n")
print("Extracted duplicated discount code")
