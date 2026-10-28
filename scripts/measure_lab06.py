from pathlib import Path
import subprocess, re, csv
root=Path(__file__).resolve().parents[1]
source=root/"labs/lab06/SportLegacyProcessor.cs"
old=subprocess.run(["git","show","9288077:labs/lab06/SportLegacyProcessor.cs"],
                   cwd=root,check=True,stdout=subprocess.PIPE).stdout.decode("utf-8")
new=source.read_text(encoding="utf-8")

def metrics(text, signature):
    start=text.index(signature)
    op=text.index("{",start)
    level=0
    for index in range(op,len(text)):
        level+=(text[index]=="{")-(text[index]=="}")
        if level==0:
            end=index+1
            break
    block=text[op:end]
    logical=[x for x in block.splitlines() if x.strip() and not x.lstrip().startswith("//")]
    metric=1+len(re.findall(r"\b(?:if|for|foreach|while|case|catch)\b|&&|\|\|",block))
    dep=0;highest=0
    for line in block.splitlines():
        dep+=line.count("{")-line.count("}")
        highest=max(highest,dep)
    signature_part=text[start:op]
    parameters=len([x for x in signature_part.split("(",1)[1].rsplit(")",1)[0].split(",") if x.strip()])
    return [len(logical),metric,highest,parameters]

b=metrics(old,"public string Handle(")
a=metrics(new,"public string Handle(")
out=root/"docs/lab06-completion"
out.mkdir(parents=True,exist_ok=True)
with (out/"metrics.csv").open("w",encoding="utf-8",newline="") as f:
    w=csv.writer(f); w.writerow(["stage","handle_nonblank_lines","approx_cyclomatic","max_block_depth","parameters"])
    w.writerow(["before",*b]);w.writerow(["after",*a])
print("before",b)
print("after",a)
print("These are reproducible text-based proxies, not Roslyn data-flow metrics.")
