"""Validate the Roslyn type index against the conservative Java registry."""
from collections import defaultdict
import json
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
registry = json.loads((ROOT / "Porting/parity_registry.json").read_text())
index = json.loads((ROOT / "Porting/csharp_type_index.json").read_text())
reviewed = json.loads((ROOT / "Porting/reviewed_type_bindings.json").read_text())
assert index["schema"] == 1
assert reviewed["schema"] == 1
assert index["source_archive_sha256"] == registry["source_archive_sha256"]
assert index["csharp_type_count"] == len(index["types"])
assert index["csharp_file_count"] == len({t["CSharpFile"] for t in index["types"]})

candidates = defaultdict(set)
for entry in registry["entries"]:
    for path in entry["CSharpImplementation"]:
        candidates[path].add(entry["JavaSource"])

seen = set()
for entry in index["types"]:
    path = entry["CSharpFile"]
    identity = (path, entry["CSharpType"])
    assert identity not in seen, identity
    seen.add(identity)
    assert (ROOT / path).is_file(), path
    assert entry["JavaCandidates"] == sorted(candidates[path]), identity
    assert entry["Status"] == ("UnverifiedCandidate" if candidates[path] else "NoCandidate"), identity

java_sources = {entry["JavaSource"] for entry in registry["entries"]}
indexed_types = {entry["CSharpType"] for entry in index["types"]}
bound = set()
for binding in reviewed["bindings"]:
    csharp = binding["CSharpType"]
    assert csharp in indexed_types and csharp not in bound, csharp
    bound.add(csharp)
    assert binding["JavaSource"] in java_sources, binding["JavaSource"]
    assert binding["Status"] in {"Porting", "ParityPassed", "EngineReplacementPassed"}
    assert binding["Tests"] and all((ROOT / p).is_file() for p in binding["Tests"])
    assert binding["Status"] != "ParityPassed" or not binding["KnownDifferences"]

print(f"Validated {len(seen)} C# declarations, {len(bound)} reviewed binding(s)")
