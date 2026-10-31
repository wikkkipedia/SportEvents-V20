from pathlib import Path
import re,json
root=Path(__file__).resolve().parents[1]
folders=["SportEvents.Core","SportEvents.Console","SportEvents.Tests"]
files=[p for f in folders for p in (root/f).rglob("*.cs") if "bin" not in p.parts and "obj" not in p.parts]
method_re=re.compile(r"(?m)^[ \t]*(?:(?:public|private|protected|internal|static|virtual|override|async|sealed|new|partial|abstract)\s+)*(?:[A-Za-z_][\w<>,?\[\]. ]*?)\s+([A-Za-z_]\w*)\s*\(([^;{}]*)\)\s*(?:=>|{|$)")
methods=[]
lloc=0
lloc_no_braces=0
for f in files:
    text=f.read_text(encoding="utf-8")
    nonempty=[line for line in text.splitlines() if line.strip() and not line.lstrip().startswith("//") and not line.lstrip().startswith("///")]
    lloc+=len(nonempty)
    lloc_no_braces+=sum(line.strip() not in ("{","}","};",");") for line in nonempty)
    for m in method_re.finditer(text):
        sig=m.group(0)
        if any(x in sig for x in (" if ("," for ("," foreach ("," while ("," switch ("," catch (")):continue
        start=text.find("{",m.start())
        arrow=text.find("=>",m.start())
        if (arrow!=-1 and (start==-1 or arrow<start)):
            end=text.find(";",arrow)
            if end==-1:continue
            body=text[arrow:end]
            depth=0
        elif start!=-1:
            depth=1;end=start+1;maxdepth=1
            for idx in range(start+1,len(text)):
                if text[idx]=="{":depth+=1;maxdepth=max(maxdepth,depth)
                elif text[idx]=="}":
                    depth-=1
                    if depth==0:end=idx+1;break
            body=text[start:end]
            depth=maxdepth-1
        else:continue
        lines=sum(bool(x.strip()) and not x.lstrip().startswith("//") for x in body.splitlines())
        cc=1+len(re.findall(r"\b(?:if|for|foreach|while|case|catch)\b|&&|\|\||\?\?",body))
        methods.append({"file":str(f.relative_to(root)),"name":m.group(1),"lines":lines,"cc_proxy":cc,"nesting_proxy":depth})
    print(f.relative_to(root),"LLOC_PROXY",len(nonempty))
exceed=[x for x in methods if x["lines"]>30 or x["cc_proxy"]>8 or x["nesting_proxy"]>3]
report={"method_count":len(methods),"lloc_proxy_all_3_projects":lloc,"lloc_without_braces_proxy":lloc_no_braces,"max_method_lines":max(x["lines"] for x in methods),"max_cc_proxy":max(x["cc_proxy"] for x in methods),"max_nesting_proxy":max(x["nesting_proxy"] for x in methods),"exceed":sorted(exceed,key=lambda x:(x["cc_proxy"],x["lines"]),reverse=True)[:25],"warning":"Lexical estimates; comments, strings, lambda and nesting may shift exact Roslyn metrics."}
dst=root/"verification"/"metrics-sr03-estimate.json"
dst.parent.mkdir(exist_ok=True)
dst.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
print("SUMMARY",json.dumps(report,ensure_ascii=False)[:5500])
