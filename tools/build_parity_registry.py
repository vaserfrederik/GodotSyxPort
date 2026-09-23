"""Generate a conservative source inventory from a user supplied source JAR.

The repository stores metadata only. The copyrighted source archive stays outside Git.
"""
import argparse
import hashlib
import json
import pathlib
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Porting" / "parity_registry.json"
STATUSES = {"NotStarted", "Porting", "PortedUntested", "ParityPassed", "EngineReplacementPassed", "Blocked"}


def validate(data):
    entries = data["entries"]
    assert data["schema"] == 1
    assert len(entries) == data["java_source_count"]
    assert len({e["JavaSource"] for e in entries}) == len(entries)
    for e in entries:
        assert e["Status"] in STATUSES
        assert e["JavaSource"].endswith(".java")
        assert all((ROOT / p).is_file() for p in e["CSharpImplementation"])
        assert e["Status"] not in {"ParityPassed", "EngineReplacementPassed"} or (
            e["Tests"] and all((ROOT / p).is_file() for p in e["Tests"])
        )
        assert e["Status"] != "EngineReplacementPassed" or e["EngineReplacement"]


def generate(jar):
    old = json.loads((ROOT / "Porting" / "legacy_compile_manifest.json").read_text())
    mappings = {}
    for row in old["compiled_adaptations"]:
        paths = [s.strip() for s in row["compiled"].split(";")]
        if row["java"] in mappings:
            raise ValueError("duplicate mapping: " + row["java"])
        mappings[row["java"]] = paths
    output = subprocess.run(
        ["java", str(ROOT / "tools" / "JavaAstInventory.java"), str(jar)],
        capture_output=True, text=True,
    )
    if output.returncode:
        raise RuntimeError(f"Java AST parser failed:\n{output.stderr[-6000:]}")
    entries = []
    for line in output.stdout.splitlines():
        e = json.loads(line)
        impl = mappings.pop(e["JavaSource"], [])
        imports = e.pop("JavaImports")
        entries.append({
            **e,
            "JavaDependencies": imports,
            "CSharpImplementation": impl,
            "Status": "PortedUntested" if impl else "NotStarted",
            "Tests": [],
            "EngineReplacement": None,
            "KnownDifferences": [],
        })
    data = {
        "schema": 1,
        "source_archive_sha256": hashlib.sha256(jar.read_bytes()).hexdigest(),
        "java_source_count": len(entries),
        "dependency_scope": "Explicit Java imports from parsed AST only; calls and same-package references remain unverified.",
        "mapping_scope": "Unverified candidate mappings from legacy_compile_manifest.json; all start PortedUntested.",
        "unmatched_legacy_mappings": sorted(mappings),
        "entries": sorted(entries, key=lambda e: e["JavaSource"]),
    }
    validate(data)
    return data


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-jar", type=pathlib.Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    existing = json.loads(OUTPUT.read_text()) if args.check else None
    if args.source_jar:
        data = generate(args.source_jar)
        if args.check:
            if data != existing:
                sys.exit("Registry differs from parsed Java archive")
        else:
            OUTPUT.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n")
    elif args.check:
        validate(existing)
    else:
        parser.error("--source-jar is required to generate registry")
    print(f"Java units: {len(data['entries'] if args.source_jar else existing['entries'])}; "
          f"candidate mappings: {sum(e['Status'] == 'PortedUntested' for e in (data if args.source_jar else existing)['entries'])}")


if __name__ == "__main__":
    main()
