from pathlib import Path
import subprocess
import tempfile
import shutil
import textwrap
import csv
import sys

root = Path(__file__).resolve().parents[1]
old_commit = "bac0999"

def check(*args, cwd=None):
    p = subprocess.run(args, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode:
        raise RuntimeError(f"{args}, code={p.returncode}\n" + p.stdout.decode("utf-8","replace") + "\n" + p.stderr.decode("utf-8","replace"))
    return p.stdout

shared = r"""
using System;
using System.Collections.Generic;
using System.Globalization;
using SportEvents.Lab03;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

for (int caseId = 0; caseId < 30; caseId++)
{
    int docId = 1;
    string name = "Іван";
    string email = "a@b.com";
    bool regular = false;
    List<RaceChargeLine>? lines = new()
    {
        new() { Code = "100m", Quantity = 12, UnitPrice = 100m },
        new() { Code = "200m", Quantity = 1, UnitPrice = 250m },
    };
    DateOnly date = new(2026, 3, 10);
    string coupon = "";
    string status = "New";
    bool print = false;
    decimal delivery = 60m;

    switch(caseId)
    {
        case 1: docId = -3; break;
        case 2: name = ""; break;
        case 3: email = "invalid"; break;
        case 4: lines = null; break;
        case 5: lines = new(); break;
        case 6: status = "Unknown"; break;
        case 7: lines![0].Quantity = 0; break;
        case 8: lines![0].UnitPrice = -1m; break;
        case 9: regular = true; break;
        case 10: lines![0].UnitPrice = 450m; break;
        case 11: lines![0].UnitPrice = 250m; break;
        case 12: coupon = "SALE10"; break;
        case 13: coupon = "MINUS200"; break;
        case 14: coupon = "MINUS200"; lines![0].UnitPrice=40m; break;
        case 15: date = new DateOnly(2026,3,1); break;
        case 16: lines![0].UnitPrice = 10m; lines[1].UnitPrice=2m; break;
        case 17: status = "Paid"; break;
        case 18: status = "Shipped"; break;
        case 19: status = "Cancelled"; break;
        case 20: print = true; break;
        case 21: regular = true; coupon="SALE10"; date = new DateOnly(2026,3,1); print=true; break;
        case 22: lines![0].Quantity=9; break;
        case 23: lines![0].Quantity=10; break;
        case 24: lines![0].Quantity=1; lines[0].UnitPrice=1000m; lines[1].UnitPrice=0m; break;
        case 25: delivery = -10m; lines![0].UnitPrice = 1m; lines[1].UnitPrice=0m; break;
        case 26: regular = true; lines![0].UnitPrice=1000m; break;
        case 27: coupon="SALE10"; status="Paid"; print=true; break;
        case 28: lines![0].UnitPrice=0m; lines[1].UnitPrice=0m; break;
        case 29: lines![0].Quantity=3; lines[0].UnitPrice=333.333m; break;
    }

    Console.WriteLine("CASE|" + caseId);
    try
    {
        CALL_HERE
        Console.WriteLine("RESULT|" + total.ToString(CultureInfo.InvariantCulture) + "|" + resultStatus);
    }
    catch(Exception ex)
    {
        Console.WriteLine("EXCEPTION|" + ex.GetType().Name);
    }
    Console.WriteLine("ENDCASE");
}
"""
calls = {
    "before": """string resultStatus;
        decimal total = LegacyProcessor.Process(docId,name,email,regular,
            lines!,date,coupon,status,print,delivery,out resultStatus);""",
    "after": """var outcome = LegacyProcessor.Process(new RaceRequest
        {
            DocId=docId, ClientName=name, ClientEmail=email, IsRegular=regular,
            Lines=lines!, CreatedAt=date, CouponCode=coupon, Status=status,
            PrintToConsole=print, DeliveryPrice=delivery,
        });
        decimal total = outcome.Total;
        string resultStatus = outcome.NewStatus;""",
}

with tempfile.TemporaryDirectory(prefix="sport_lab03_compare_") as temp:
    base = Path(temp)
    results = {}
    for stage in ("before","after"):
        work = base / stage
        work.mkdir()
        sources = ["LegacyProcessor.cs"] if stage == "before" else ["LegacyProcessor.cs","RaceRequest.cs","RaceOutcome.cs"]
        for name in sources:
            if stage=="before":
                data = check("git","show",f"{old_commit}:labs/lab03/{name}",cwd=root)
            else:
                data = (root/"labs/lab03"/name).read_bytes()
            (work/name).write_bytes(data)
        project = "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><RunAnalyzers>false</RunAnalyzers><TreatWarningsAsErrors>false</TreatWarningsAsErrors></PropertyGroup></Project>"
        (work/"Compare.csproj").write_text(project,encoding="utf-8")
        (work/"Program.cs").write_text(shared.replace("CALL_HERE",calls[stage]),encoding="utf-8")
        check("dotnet","build","Compare.csproj","-c","Release","-v:q",cwd=work)
        results[stage] = check("dotnet",str(work/"bin/Release/net8.0/Compare.dll"),cwd=work)
    old, new = results["before"],results["after"]
    out = root/"docs/lab03-completion"
    out.mkdir(parents=True,exist_ok=True)
    (out/"differential-before.log").write_bytes(old)
    (out/"differential-after.log").write_bytes(new)
    total_cases = old.count(b"CASE|")
    print(f"Compared {total_cases} complete scenarios.")
    if old != new:
        a=old.decode("utf-8","replace").splitlines()
        b=new.decode("utf-8","replace").splitlines()
        import difflib
        diff="\n".join(difflib.unified_diff(a,b,fromfile="before",tofile="after"))
        (out/"differential-failure.diff").write_text(diff,encoding="utf-8")
        print(diff[:3000])
        raise SystemExit(1)
    (out/"DIFFERENTIAL_RESULT.md").write_text(
        "# Lab 3 differential comparison\n\n"
        f"Original source: Git commit {old_commit}.\n"
        f"Scenarios: {total_cases}. Entire stdout and results match byte-for-byte.\n"
        "Coverage: validation, status transitions, totals, caps, coupons, weekends, output.\n"
        "This proves these inputs match, not equivalence for all possible inputs.\n",encoding="utf-8"
    )
    print(f"PASS: {len(old)} bytes of output match byte-for-byte.")
