from pathlib import Path
import re
import csv
root = Path(__file__).resolve().parents[1]
source = (root / "labs/lab03/LegacyProcessor.cs").read_text(encoding="utf-8")
def measure(src, marker):
    pos = src.index(marker)
    brace = src.index("{", pos)
    depth = 0
    end = brace
    for i in range(brace, len(src)):
        depth += (src[i] == "{") - (src[i] == "}")
        if depth == 0:
            end = i + 1
            break
    body = src[brace:end]
    branch = len(re.findall(r"\b(if|for|foreach|while|case|catch)\b|&&|\|\|", body))
    lines = [v for v in body.splitlines() if v.strip() and not v.lstrip().startswith("//")]
    depth = 0
    maximum = 0
    for line in body.splitlines():
        depth += line.count("{")-line.count("}")
        maximum = max(maximum, depth)
    signature = src[pos:brace]
    params = signature.split("(", 1)[1].rsplit(")",1)[0]
    return [marker.strip(), len(lines), 1+branch, maximum, len(params.split(","))]
import sys
stage=sys.argv[1] if len(sys.argv)>1 else "before"
out=root/"docs/lab03-completion"
out.mkdir(exist_ok=True,parents=True)
marker="public static RaceOutcome Process(" if stage == "after" else "public static decimal Process("
result=measure(source,marker)
with (out/f"metrics-{stage}.csv").open("w",newline="",encoding="utf-8") as f:
    w=csv.writer(f);w.writerow(["method","nonempty_lines","approx_cyclomatic","max_block_depth","parameters"]);w.writerow(result)
print(result)
print("Approximate text-based proxy, not Roslyn control-flow metrics.")
