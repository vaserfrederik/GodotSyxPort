"""Resolve only simple C# AST call receivers to declared project methods."""
import argparse
from collections import Counter, defaultdict
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
INDEX = ROOT / "Porting/csharp_call_index.json"
OUTPUT = ROOT / "Porting/csharp_call_graph.json"


def build():
    data = json.loads(INDEX.read_text())
    types = json.loads((ROOT / "Porting/csharp_type_index.json").read_text())
    deps = json.loads((ROOT / "Porting/dependency_graph.json").read_text())
    reviewed = json.loads((ROOT / "Porting/reviewed_type_bindings.json").read_text())
    java_graph = json.loads((ROOT / "Porting/java_call_graph.json").read_text())
    source_by_type = {entry["CSharpType"]: entry["CSharpFile"] for entry in types["types"]}
    method_names = set()
    for file in data["files"]:
        for method in file["Methods"]:
            method_names.add((method["Caller"].split("#", 1)[0], method["Name"], method["Arity"]))
    bound = {entry["CSharpType"]: entry["JavaSource"] for entry in reviewed["bindings"]}
    java_edges = {(entry["CallerJavaSource"], entry["TargetJavaSource"])
                  for entry in java_graph["edges"]}
    csharp_edges = Counter()
    signatures = defaultdict(set)
    unresolved = Counter()
    raw_calls = 0
    for file in data["files"]:
        path = file["CSharpFile"]
        imported = deps["csharp"][path]["using_namespaces"]
        for call in file["Calls"]:
            count = call["Count"]
            raw_calls += count
            receiver = call["Receiver"]
            caller_type = call["Caller"].split("#", 1)[0]
            if call["Name"] == "nameof" and not receiver:
                unresolved["language-construct"] += count
                continue
            candidates = []
            if receiver in ("", "this") and call["Kind"] == "invoke":
                candidates = [caller_type]
            elif receiver:
                namespace = caller_type.rsplit(".", 1)[0] if "." in caller_type else ""
                candidates = [receiver, namespace + "." + receiver]
                candidates += [imported_name + "." + receiver for imported_name in imported]
            matches = {candidate for candidate in candidates if candidate in source_by_type}
            if len(matches) != 1:
                unresolved["ambiguous-type" if len(matches) > 1 else "receiver-unbound"] += count
                continue
            target_type = matches.pop()
            if (target_type, call["Name"], call["Arity"]) not in method_names:
                unresolved["method-unbound"] += count
                continue
            target_file = source_by_type[target_type]
            if target_file == path:
                unresolved["same-file"] += count
                continue
            pair = (path, target_file)
            csharp_edges[pair] += count
            signatures[pair].add((call["Caller"], target_type, call["Name"], call["Arity"], call["Kind"]))

    edges = []
    for (caller, target), count in sorted(csharp_edges.items()):
        edges.append({"CallerCSharpFile": caller, "TargetCSharpFile": target,
                      "CandidateCalls": count, "DistinctCallSignatures": len(signatures[caller, target])})
    bound_files = defaultdict(set)
    for csharp_type, java_source in bound.items():
        bound_files[source_by_type[csharp_type]].add(java_source)
    compared = []
    for edge in edges:
        callers = bound_files[edge["CallerCSharpFile"]]
        targets = bound_files[edge["TargetCSharpFile"]]
        if len(callers) == 1 and len(targets) == 1:
            java_pair = (next(iter(callers)), next(iter(targets)))
            compared.append({"CallerCSharpFile": edge["CallerCSharpFile"],
                             "TargetCSharpFile": edge["TargetCSharpFile"],
                             "JavaCaller": java_pair[0], "JavaTarget": java_pair[1],
                             "JavaSyntaxCandidate": java_pair in java_edges})
    return {
        "schema": 1,
        "source_archive_sha256": java_graph["source_archive_sha256"],
        "scope": "C# Roslyn syntax candidates from named type receivers and declared method name/arity; untyped instance receivers, overload resolution, inheritance, Godot virtual dispatch and Java equivalence are unverified.",
        "summary": {"csharp_files": len(data["files"]), "declared_methods": len(method_names),
                    "all_syntax_calls": raw_calls,
                    "cross_file_candidate_edges": len(edges),
                    "cross_file_candidate_calls": sum(csharp_edges.values()),
                    "unresolved_calls_by_reason": dict(sorted(unresolved.items())),
                    "cross_language_reviewed_file_pairs": len(compared)},
        "edges": edges,
        "reviewed_cross_language_pairs": compared,
    }


def validate(graph):
    index = json.loads(INDEX.read_text())
    types = json.loads((ROOT / "Porting/csharp_type_index.json").read_text())
    paths = {entry["CSharpFile"] for entry in types["types"]}
    assert graph["schema"] == 1 and index["schema"] == 1
    assert graph["summary"]["csharp_files"] == len(index["files"]) == types["csharp_file_count"]
    assert graph["summary"]["cross_file_candidate_edges"] == len(graph["edges"])
    assert graph["summary"]["cross_file_candidate_calls"] == sum(e["CandidateCalls"] for e in graph["edges"])
    assert graph["summary"]["all_syntax_calls"] == (graph["summary"]["cross_file_candidate_calls"]
                                                 + sum(graph["summary"]["unresolved_calls_by_reason"].values()))
    assert {f["CSharpFile"] for f in index["files"]} == paths
    assert all(e["CallerCSharpFile"] in paths and e["TargetCSharpFile"] in paths
               and e["CallerCSharpFile"] != e["TargetCSharpFile"] for e in graph["edges"])


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    graph = build()
    validate(graph)
    if args.check:
        if graph != json.loads(OUTPUT.read_text()):
            sys.exit("C# call graph differs from Roslyn inventory or reviewed bindings")
    else:
        OUTPUT.write_text(json.dumps(graph, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps(graph["summary"], ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
