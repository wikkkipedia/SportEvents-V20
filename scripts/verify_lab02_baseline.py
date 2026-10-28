from pathlib import Path
import subprocess
import tempfile
import sys
sys.stdout.reconfigure(encoding="utf-8")

root = Path(__file__).resolve().parents[1]
before_commit = "cdf1a81"
original_files = ("Code1.cs", "Program.cs", "LegacyStyle.csproj")

def execute(*args, cwd):
    p = subprocess.run(list(args), cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode:
        raise RuntimeError(f"{args}: exit={p.returncode}\n" + p.stdout.decode("utf-8", "replace") + p.stderr.decode("utf-8", "replace"))
    return p.stdout

with tempfile.TemporaryDirectory(prefix="sport_lab02_") as temp:
    folder = Path(temp)
    for filename in original_files:
        raw = execute("git", "show", f"{before_commit}:labs/lab02/{filename}", cwd=root)
        (folder / filename).write_bytes(raw)

    execute("dotnet", "build", "LegacyStyle.csproj", "-p:RunAnalyzers=false", "-v:q", cwd=folder)
    before = execute("dotnet", str(folder / "bin/Debug/net8.0/LegacyStyle.dll"), cwd=folder)
    after_folder = root / "labs/lab02"
    execute("dotnet", "build", "LegacyStyle.csproj", "-p:RunAnalyzers=false", "-v:q", cwd=after_folder)
    after = execute("dotnet", str(after_folder / "bin/Debug/net8.0/LegacyStyle.dll"), cwd=after_folder)

    output = root / "docs/lab02-completion"
    output.mkdir(parents=True, exist_ok=True)
    (output/"before.txt").write_bytes(before)
    (output/"after.txt").write_bytes(after)
    print(f"Baseline byte length: {len(before)}; after byte length: {len(after)}")
    print(f"Behavior identical byte-for-byte: {before == after}")
    print(after.decode("utf-8", "replace"))
    if before != after:
        raise SystemExit(1)
