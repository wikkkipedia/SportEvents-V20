from pathlib import Path
p=Path(__file__).resolve().parents[1]/"labs/lab06/SportLegacyProcessor.cs"
s=p.read_text(encoding="utf-8")
start=s.index('        string txt = "Документ #"')
end=s.index('        if (sendMail)',start)
body=s[start:end]
s=s[:start]+"""        string txt = BuildReport(docId, clientName, items, discount, ship, total, currency);

"""+s[end:]
anchor="    private static decimal CalculateDiscount("
method="""    private static string BuildReport(int docId, string clientName,
        List<SportsResultLine> items, decimal discount, decimal ship,
        decimal total, string currency)
    {
"""+body+"""        return txt;
    }

"""
assert s.count(anchor)==1
p.write_text(s.replace(anchor,method+anchor),encoding="utf-8",newline="\n")
print("Extracted seven-parameter report builder (before Parameter Object)")
