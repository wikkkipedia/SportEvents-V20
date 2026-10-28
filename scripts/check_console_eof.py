"""End-to-end regression: redirected stdin must not cause a busy infinite menu loop."""
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
PROGRAM = ROOT / "SportEvents.Console" / "SportEvents.Console.csproj"

for label, content in (("EOF", ""), ("Exit option", "0\n")):
    completed = subprocess.run(
        ["dotnet", "run", "--project", str(PROGRAM), "-c", "Release", "--no-build"],
        cwd=ROOT,
        input=content.encode("utf-8"),
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        timeout=12,
        check=False,
    )
    output = completed.stdout.decode("utf-8", "replace")
    menu_count = output.count("Новий учасник")
    if completed.returncode != 0 or menu_count != 1:
        print(f"FAIL {label}: exit={completed.returncode}, menu={menu_count}")
        print(output[-700:])
        print(completed.stderr.decode("utf-8", "replace")[-500:])
        raise SystemExit(1)
    print(f"PASS {label}: exit=0, menu displayed once, {len(completed.stdout)} output bytes")

print("Console EOF regression passed for both redirected-input cases.")
