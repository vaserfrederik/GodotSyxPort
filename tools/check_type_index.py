"""Validate the Roslyn type index against the conservative Java registry."""
from collections import defaultdict
import json
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
registry = json.loads((ROOT / "Porting/parity_registry.json").read_text())
index = json.loads((ROOT / "Porting/csharp_type_index.json").read_text())
reviewed = json.loads((ROOT / "Porting/reviewed_type_bindings.json").read_text())
static = json.loads((ROOT / "Porting/static_type_bindings.json").read_text())
assert index["schema"] == 1
assert reviewed["schema"] == 1
assert static["schema"] == 1
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
type_entries = {entry["CSharpType"]: entry for entry in index["types"]}
java_units = {entry["JavaSource"]: entry for entry in registry["entries"]}
bound = set()
for binding in reviewed["bindings"]:
    csharp = binding["CSharpType"]
    assert csharp in indexed_types and csharp not in bound, csharp
    bound.add(csharp)
    assert binding["JavaSource"] in java_sources, binding["JavaSource"]
    assert binding["Status"] in {"Porting", "ParityPassed", "EngineReplacementPassed"}
    assert binding["Tests"] and all((ROOT / p).is_file() for p in binding["Tests"])
    assert binding["Status"] != "ParityPassed" or not binding["KnownDifferences"]

static_bound = set()


def check_static_source(csharp, source, java_type):
    assert source in java_units, (csharp, source)
    assert source in type_entries[csharp]["JavaCandidates"], (csharp, source)
    assert f"CLASS {java_type}" in java_units[source]["JavaTypes"], (csharp, source, java_type)


for binding in static["bindings"]:
    csharp = binding["CSharpType"]
    assert csharp in type_entries and csharp not in static_bound, csharp
    static_bound.add(csharp)
    assert binding["Status"] in {"Partial", "Representation"}
    assert binding["Evidence"] and binding["KnownGaps"]
    assert binding["JavaTypes"]
    sources = set()
    for java_type in binding["JavaTypes"]:
        source = java_type["JavaSource"]
        assert source in java_units and source not in sources, (csharp, source)
        sources.add(source)
        check_static_source(csharp, source, java_type["JavaType"])

groups = set()
for group in static.get("groups", []):
    assert group["Group"] not in groups and group["Bindings"]
    groups.add(group["Group"])
    assert group["Status"] == "SourceMapped" and group["KnownGaps"]
    member_sources = defaultdict(set)
    for csharp, source, java_type in group["Bindings"]:
        assert csharp in type_entries and csharp not in static_bound, csharp
        assert (source, java_type) not in member_sources[csharp], (csharp, source, java_type)
        member_sources[csharp].add((source, java_type))
        check_static_source(csharp, source, java_type)
    static_bound.update(member_sources)

print(f"Validated {len(seen)} C# declarations, {len(bound)} fixture-backed binding(s), "
      f"{len(static_bound)} static type association(s)")
