"""Conservative Java AST call candidates including declared inheritance."""
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


def resolve_base(name, owner, package, imports, types):
    """Resolve only names anchored by the declaring type, package or explicit import."""
    parts = owner.split(".")
    candidates = [name]
    candidates.extend(".".join(filter(None, (package, ".".join(parts[:n]), name)))
                      for n in range(len(parts), 0, -1))
    candidates.append(".".join(filter(None, (package, name))))
    head, _, tail = name.partition(".")
    if head in imports:
        candidates.append(imports[head] + ("." + tail if tail else ""))
    return next((candidate for candidate in candidates if candidate in types), None)


def declaring_method(target, name, arity, methods, parents):
    """Find a unique nearest declaration; multiple interface paths remain unknown."""
    pending = {target}
    visited = set()
    while pending:
        matches = sorted(owner for owner in pending if (owner, name, arity) in methods)
        if matches:
            return (matches[0], "") if len(matches) == 1 else (None, "inherited-ambiguous")
        visited.update(pending)
        pending = {base for owner in pending for base in parents.get(owner, ())} - visited
    return None, "method-unbound"


def declaring_field_type(target, name, fields, parents, owners, imports, types):
    """Follow only a uniquely declared field at the nearest inheritance level."""
    pending = {target}
    visited = set()
    while pending:
        matches = sorted(owner for owner in pending if name in fields.get(owner, {}))
        if matches:
            if len(matches) != 1:
                return None, "member-ambiguous"
            owner = matches[0]
            source, package, local_owner = owners[owner]
            field_type = resolve_base(fields[owner][name], local_owner, package,
                                      imports[source], types)
            return (field_type, "") if field_type else (None, "member-type-unbound")
        visited.update(pending)
        pending = {base for owner in pending for base in parents.get(owner, ())} - visited
    return None, "member-unbound"


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
    class_types = set()
    methods = set()
    fields = {}
    owners = {}
    for row in records:
        package = row["Package"]
        source = row["JavaSource"]
        for declaration in units[source]["JavaTypes"]:
            kind, name = declaration.split(" ", 1)
            qualified = ".".join(filter(None, (package, name)))
            types[qualified] = source
            owners[qualified] = (source, package, name)
            if kind == "CLASS":
                class_types.add(qualified)
        for owner, declared_fields in row["Fields"].items():
            qualified = ".".join(filter(None, (package, owner)))
            fields[qualified] = declared_fields
        for method in row["Methods"]:
            owner = ".".join(filter(None, (package, method["owner"])))
            methods.add((owner, method["name"], method["arity"]))

    parents = {}
    imports_by_source = {}
    for row in records:
        package = row["Package"]
        imports = {item.rsplit(".", 1)[-1]: item
                   for item in units[row["JavaSource"]]["JavaDependencies"] if item in types}
        imports_by_source[row["JavaSource"]] = imports
        for owner, bases in row["Parents"].items():
            qualified_owner = ".".join(filter(None, (package, owner)))
            parents[qualified_owner] = tuple(filter(None, (
                resolve_base(base, owner, package, imports, types) for base in bases)))

    edges = Counter()
    missing = Counter()
    unresolved = Counter()
    call_count = 0
    method_cache = {}
    inherited_calls = 0
    super_calls = 0
    for row in records:
        source = row["JavaSource"]
        package = row["Package"]
        imports = imports_by_source[source]
        for call in row["Calls"]:
            count = call["count"]
            call_count += count
            receiver = call["receiver"]
            owner = call["caller"].split("#", 1)[0]
            own_type = ".".join(filter(None, (package, owner)))
            candidates = []
            if receiver == "super" and call["kind"] == "invoke":
                bases = parents.get(own_type, ())
                candidates.extend(base for base in bases[:1] if base in class_types)
            elif receiver == "this" or (not receiver and call["kind"] == "invoke"):
                candidates.append(own_type)
            elif receiver:
                receiver_type = call.get("receiverTypeHint") or receiver
                candidates.extend((receiver_type, imports.get(receiver_type, ""),
                                   ".".join(filter(None, (package, receiver_type)))))
            target = next((type_name for type_name in candidates if type_name in types), None)
            if target is None:
                unresolved["receiver-unbound"] += count
                continue
            member_path = call.get("receiverMemberPath", "")
            if member_path:
                for member in member_path.split("."):
                    target, reason = declaring_field_type(
                        target, member, fields, parents, owners, imports_by_source, types)
                    if reason:
                        unresolved[reason] += count
                        break
                if target is None:
                    continue
            receiver_type = target
            if call["kind"] == "invoke":
                key = target, call["name"], call["arity"]
                if key not in method_cache:
                    method_cache[key] = declaring_method(*key, methods, parents)
                declaration, reason = method_cache[key]
                if reason:
                    unresolved[reason] += count
                    continue
                target = declaration
            # Constructor existence is proven by the type, but an implicit
            # constructor or overload still requires compiler resolution.
            if call["kind"] == "construct" and (target, "<init>", call["arity"]) not in methods:
                unresolved["constructor-signature-unbound"] += count
                continue
            target_source = types[target]
            if target_source == source:
                unresolved["same-source"] += count
                continue
            if target != receiver_type:
                inherited_calls += count
            if receiver == "super":
                super_calls += count
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
        "scope": "File-level Java AST candidates from declared types, scoped receiver hints (including var initialized directly by new), uniquely declared field chains, explicit super and uniquely declared inherited methods, matched by name/arity. Overloads, dynamic dispatch and behavior parity remain unverified.",
        "summary": {"java_units": len(records), "declared_methods": len(methods),
                    "all_calls": call_count, "cross_source_candidate_file_edges": len(edge_rows),
                    "cross_source_candidate_call_signatures": len(edges),
                    "cross_source_candidate_calls": sum(edges.values()),
                    "inherited_cross_source_candidate_calls": inherited_calls,
                    "explicit_super_cross_source_candidate_calls": super_calls,
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
    assert 0 <= data["summary"]["inherited_cross_source_candidate_calls"] <= data["summary"]["cross_source_candidate_calls"]
    assert 0 <= data["summary"]["explicit_super_cross_source_candidate_calls"] <= data["summary"]["cross_source_candidate_calls"]
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
