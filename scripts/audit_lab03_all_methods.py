from pathlib import Path
import re
import csv
root=Path(__file__).resolve().parents[1]
source=root/"labs/lab03/LegacyProcessor.cs"
text=source.read_text(encoding="utf-8")
pattern=re.compile(r"\b(public|private)\s+static\s+[\w<>?]+\s+(\w+)\s*\(([^)]*)\)\s*\{",re.S)
rows=[]
for match in pattern.finditer(text):
    start=match.end()-1
    depth=0
    end=-1
    for i in range(start,len(text)):
        if text[i]=="{":depth+=1
        elif text[i]=="}":
            depth-=1
            if depth==0:
                end=i+1
                break
    if end<0:raise ValueError(match.group(2))
    body=text[start:end]
    stripped=re.sub(r'"(?:\\.|[^"\\])*"','""',body)
    stripped=re.sub(r'//[^\n]*','',stripped)
    stripped=re.sub(r'/\*.*?\*/','',stripped,flags=re.S)
    statements=len(re.findall(r'\b(?:if|else\s+if|for|foreach|while|catch|case)\b',stripped))
    # else if is counted through its 'if' token, so use regex for 'if' only
    statements=len(re.findall(r'\b(?:if|for|foreach|while|catch|case)\b',stripped))
    boolean_operators=stripped.count("&&")+stripped.count("||")+stripped.count("??")
    # C# pattern alternatives also fork control flow (e.g. status is "New" or "Paid").
    pattern_alternatives=len(re.findall(r'\s+or\s+(?:"|\w+)',stripped))
    complexity=1+statements+boolean_operators+pattern_alternatives
    max_depth=0
    depth=0
    for line in body.splitlines():
        depth+=line.count("{")-line.count("}")
        max_depth=max(max_depth,depth-1)
    parameters=match.group(3)
    count=len([p for p in parameters.split(",") if p.strip()])
    logical_lines=len([l for l in body.splitlines() if l.strip() and l.strip() not in ("{","}") and not l.strip().startswith("//")])
    rows.append((match.group(2),complexity,logical_lines,max_depth,count,"PASS" if complexity<=6 and count<=4 else "REVIEW"))
folder=root/"docs/lab03-completion"
folder.mkdir(exist_ok=True,parents=True)
with (folder/"all-methods-audit.csv").open("w",encoding="utf-8",newline="") as f:
    writer=csv.writer(f,delimiter=";",lineterminator="\n")
    writer.writerow(["Method","CC_manual_proxy","LogicalLOC","MaxNestedBlocks","Parameters","Threshold"])
    writer.writerows(rows)
result=["# ЛР №3 — перевірка всіх методів","", "Аудит проведено за правилами методички: CC = 1 + if/for/foreach/while/catch/case + логічні && та || + альтернативи C# pattern or. Метод має не більше 6 CC і не більше 4 параметрів.", "", "Підрахунок скриптовий за текстом C#, він не є офіційною метрикою Roslyn; для наведених методів структуру перевірено за вихідним кодом.", "", "| Метод | CC | Логічні рядки | Вкладеність | Параметри | Статус |", "|---|---:|---:|---:|---:|---|"]
for x in rows:result.append(f"| {x[0]} | {x[1]} | {x[2]} | {x[3]} | {x[4]} | {x[5]} |")
(folder/"all-methods-audit.md").write_text("\n".join(result)+"\n",encoding="utf-8")
for x in rows:print(*x)
if any(x[-1]!="PASS" for x in rows):raise SystemExit("Method threshold violated")
print(f"PASS: {len(rows)} methods meet the limits of CC<=6 and args<=4.")
