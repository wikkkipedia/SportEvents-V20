"""Safe live red/green demonstration for Lab 7, in an isolated Git worktree."""

from __future__ import annotations

from pathlib import Path
import shutil
import subprocess
import tempfile


REPO = Path(__file__).resolve().parents[1]
ORIGINAL = 'duration.ToString(@"hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture)'
BROKEN = 'duration.ToString(@"hh\\:mm\\:ss", CultureInfo.InvariantCulture)'


def run(args: list[str], cwd: Path) -> subprocess.CompletedProcess[str]:
    return subprocess.run(args, cwd=cwd, text=True, encoding="utf-8", errors="replace",
                          capture_output=True, check=False)


def main() -> int:
    directory = Path(tempfile.mkdtemp(prefix="sportevents_lab07_redgreen_"))
    temp = directory / "checkout"
    created = False
    try:
        checkout = run(["git", "worktree", "add", "--detach", str(temp), "HEAD"], REPO)
        if checkout.returncode != 0:
            print(checkout.stderr)
            return 1
        created = True
        source = temp / "SportEvents.Core" / "Application" / "RaceTimeFormatter.cs"
        content = source.read_text(encoding="utf-8")
        if content.count(ORIGINAL) != 1:
            print("Unable to locate the format string in the isolated copy.")
            return 1
        source.write_text(content.replace(ORIGINAL, BROKEN), encoding="utf-8")
        command = ["dotnet", "test", "SportEvents.Tests/SportEvents.Tests.csproj",
                   "-c", "Release",
                   "--filter", "FullyQualifiedName~Format_ValidMilliseconds_UsesFixedPrecision"]
        red = run(command, temp)
        print("=== RED: deliberately remove milliseconds in an isolated copy ===")
        print("\n".join(red.stdout.splitlines()[-22:]))
        if red.returncode == 0:
            print("Expected a failing test; the demonstration did not fail.")
            return 2
        source.write_text(content, encoding="utf-8")
        green = run(command, temp)
        print("=== GREEN: restore production code and rerun ===")
        print("\n".join(green.stdout.splitlines()[-16:]))
        if green.returncode != 0:
            print(green.stderr)
            return 3
        print("VERIFIED: red then green. The main working tree was never edited.")
        return 0
    finally:
        if created:
            run(["git", "worktree", "remove", "--force", str(temp)], REPO)
        shutil.rmtree(directory, ignore_errors=True)


if __name__ == "__main__":
    raise SystemExit(main())
