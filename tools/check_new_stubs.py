"""Reject explicit placeholders and new empty methods in compiled C# sources.

This is a narrow guard. It does not prove behavioral completeness or identify
plausible but incorrect fallback values.
"""
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
POLICY = ROOT / "Porting" / "known_stubs.json"
MARKER = re.compile(r"\b(?:TODO|FIXME|NotImplementedException)\b")
EMPTY_METHOD = re.compile(
    r"\b(?:void|Task|ValueTask)\s+(\w+)\s*\(([^()]*)\)\s*\{\s*\}",
    re.MULTILINE,
)
EMPTY_EXPRESSION = re.compile(
    r"\b(?:void|Task|ValueTask)\s+(\w+)\s*\(([^()]*)\)\s*=>\s*(?:Task\.CompletedTask|default)\s*;",
    re.MULTILINE,
)


def scan(path, body):
    findings = set()
    for match in MARKER.finditer(body):
        # Count the marker in source comments too: every new TODO needs review.
        line = body.count("\n", 0, match.start()) + 1
        findings.add(f"{path}:{line}:{match.group()}")
    for pattern in (EMPTY_METHOD, EMPTY_EXPRESSION):
        for match in pattern.finditer(body):
            arguments = " ".join(match.group(2).split())
            findings.add(f"{path}::{match.group(1)}({arguments})")
    return findings


def main():
    policy = json.loads(POLICY.read_text())
    assert policy["schema"] == 1
    allowed = set(policy["allowed"])
    found = set()
    for source in sorted((ROOT / "Scripts").rglob("*.cs")):
        found |= scan(source.relative_to(ROOT).as_posix(), source.read_text(encoding="utf-8-sig"))
    new = found - allowed
    stale = allowed - found
    if new or stale:
        for item in sorted(new): print("New placeholder:", item, file=sys.stderr)
        for item in sorted(stale): print("Remove resolved waiver:", item, file=sys.stderr)
        sys.exit(1)
    print(f"No new explicit placeholders; {len(allowed)} known exception(s)")


if __name__ == "__main__":
    main()
