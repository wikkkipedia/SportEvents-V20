from pathlib import Path
import re
p=Path(__file__).resolve().parents[1]/"labs/lab06/SportLegacyProcessor.cs"
s=p.read_text(encoding="utf-8")
old="BuildReport(docId, clientName, items, discount, ship, total, currency)"
new="""BuildReport(new SportReportData
        {
            DocId = docId, ClientName = clientName, Items = items,
            Discount = discount, Ship = ship, Total = total, Currency = currency
        })"""
assert s.count(old)==1
s=s.replace(old,new)
start=s.index("    private static string BuildReport(")
end=s.index("    private static decimal CalculateDiscount(",start)
method=s[start:end]
signature="""    private static string BuildReport(int docId, string clientName,
        List<SportsResultLine> items, decimal discount, decimal ship,
        decimal total, string currency)"""
assert signature in method
method=method.replace(signature,"    private static string BuildReport(SportReportData data)")
mapping={"docId":"DocId","clientName":"ClientName","items":"Items",
"discount":"Discount","ship":"Ship","total":"Total","currency":"Currency"}
for oldname,newname in mapping.items():
    method=re.sub(r"\b"+oldname+r"\b","data."+newname,method)
s=s[:start]+method+s[end:]
classdef="""    private sealed record SportReportData
    {
        public int DocId { get; init; }
        public string ClientName { get; init; } = "";
        public List<SportsResultLine> Items { get; init; } = new();
        public decimal Discount { get; init; }
        public decimal Ship { get; init; }
        public decimal Total { get; init; }
        public string Currency { get; init; } = "";
    }

"""
anchor="    private static string BuildReport(SportReportData data)"
assert s.count(anchor)==1
p.write_text(s.replace(anchor,classdef+anchor),encoding="utf-8",newline="\n")
print("Introduced report parameter object")
