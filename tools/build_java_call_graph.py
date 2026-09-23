"""Conservative Java AST call candidates; no receiver type inference from variables."""
import argparse
from collections import Counter, defaultdict
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
REGISTRY = ROOT / "Porting/parity_registry.json"
OUTPUT = ROOT / "Porting/java_call_graph.json"


def build(jar):
    registry = json.loads(REGISTRY.read_text())
    expected_hash = registry["source_archive_sha256"]
    actual_hash = hashlib.sha256(jar.read_bytes()).hexdigest()
    if actual_hash != expected_hash:
        raise ValueError("Source JAR hash differs from the reviewed registry")

    process = subprocess.run(["java", str(ROOT / "tools/JavaCallInventory.java"), str(jar)],
                             text=True, capture_output=True)
    if process.returncode:
        raise RuntimeError(process.stderr[-4000:])
    records = [json.loads(line) for line in process.stdout.splitlines()]
    units = {unit["JavaSource"]: unit for unit in registry["entries"]}
    if {r["JavaSource"] for r in records} != set(units):
        raise ValueError("Java call inventory does not cover every registered source")
    types = {}
    methods = set()
    for row in records:
        package = row["Package"]
        source = row["JavaSource"]
        for declaration in units[source]["JavaTypes"]:
            name = declaration.split(" ", 1)[1]
            types[".".join(filter(None, (package, name)))] = source
        for method in row["Methods"]:
            owner = ".".join(filter(None, (package, method["owner"])))
            methods.add((owner, method["name"], method["arity"]))

    edges = Counter()
    missing = Counter()
    unresolved = Counter()
    call_count = 0
    for row in records:
        source = row["JavaSource"]
        package = row["Package"]
        imports = {}
        for imported in units[source]["JavaDependencies"]:
            if imported.startswith("static ") or imported.endswith(".*"):
                continue
            if imported in types:
                imports[imported.rsplit(".", 1)[-1]] = imported
        for call in row["Calls"]:
            count = call["count"]
            call_count += count
            receiver = call["receiver"]
            owner = call["caller"].split("#", 1)[0]
            own_type = ".".join(filter(None, (package, owner)))
            candidates = []
            if receiver == "this" or (not receiver and call["kind"] == "invoke"):
                candidates.append(own_type)
            elif receiver:
                candidates.extend((receiver, imports.get(receiver, ""),
                                   ".".join(filter(None, (package, receiver)))))
            target = next((type_name for type_name in candidates if type_name in types), None)
            if target is None:
                unresolved["receiver-unbound"] += count
                continue
            if call["kind"] == "invoke" and (target, call["name"], call["arity"]) not in methods:
                unresolved["method-unbound"] += count
                continue
            # Constructor existence is proven by the type, but an implicit
            # constructor or overload still requires compiler resolution.
            if call["kind"] == "construct" and (target, "<init>", call["arity"]) not in methods:
                unresolved["constructor-signature-unbound"] += count
                continue
            target_source = types[target]
            if target_source == source:
                unresolved["same-source"] += count
                continue
            key = (source, call["caller"], target_source, target,
                   call["name"], call["arity"], call["kind"])
            edges[key] += count
            if units[source]["CSharpImplementation"] and not units[target_source]["CSharpImplementation"]:
                missing[target_source] += count

    file_edges = Counter()
    signatures = Counter()
    for (source, _caller, target_source, _type, _name, _arity, _kind), count in edges.items():
        file_edges[(source, target_source)] += count
        signatures[(source, target_source)] += 1
    edge_rows = [{"CallerJavaSource": source, "TargetJavaSource": target,
                  "CandidateCalls": count, "DistinctCallSignatures": signatures[source, target]}
                 for (source, target), count in sorted(file_edges.items())]
    return {
        "schema": 1,
        "source_archive_sha256": expected_hash,
        "scope": "File-level graph aggregated from AST calls whose simple receivers match declared types and method name/arity. No variable types, overload resolution, dynamic dispatch, inherited calls or behavior parity.",
        "summary": {"java_units": len(records), "declared_methods": len(methods),
                    "all_calls": call_count, "cross_source_candidate_file_edges": len(edge_rows),
                    "cross_source_candidate_call_signatures": len(edges),
                    "cross_source_candidate_calls": sum(edges.values()),
                    "unresolved_calls_by_reason": dict(sorted(unresolved.items())),
                    "unmapped_target_units_called_by_old_candidates": len(missing)},
        "unmapped_targets_called_by_old_candidates": [
            {"JavaSource": source, "CandidateCalls": count}
            for source, count in sorted(missing.items(), key=lambda item: (-item[1], item[0]))],
        "edges": edge_rows,
    }


def validate(data):
    registry = json.loads(REGISTRY.read_text())
    sources = {row["JavaSource"] for row in registry["entries"]}
    assert data["schema"] == 1
    assert data["source_archive_sha256"] == registry["source_archive_sha256"]
    assert data["summary"]["java_units"] == len(sources)
    assert data["summary"]["cross_source_candidate_file_edges"] == len(data["edges"])
    assert data["summary"]["cross_source_candidate_call_signatures"] == sum(row["DistinctCallSignatures"] for row in data["edges"])
    assert data["summary"]["cross_source_candidate_calls"] == sum(row["CandidateCalls"] for row in data["edges"])
    assert data["summary"]["all_calls"] == (sum(data["summary"]["unresolved_calls_by_reason"].values())
                                               + data["summary"]["cross_source_candidate_calls"])
    assert all(row["CallerJavaSource"] in sources and row["TargetJavaSource"] in sources
               and row["CallerJavaSource"] != row["TargetJavaSource"] for row in data["edges"])
    assert all(row["JavaSource"] in sources for row in data["unmapped_targets_called_by_old_candidates"])


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-jar", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    if args.source_jar:
        data = build(args.source_jar)
        validate(data)
        if args.check:
            if data != json.loads(OUTPUT.read_text()):
                sys.exit("Java call graph differs from supplied source JAR")
        else:
            OUTPUT.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n")
    elif args.check:
        data = json.loads(OUTPUT.read_text())
        validate(data)
    else:
        parser.error("--source-jar is required for generation")
    print(json.dumps(data["summary"], ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
