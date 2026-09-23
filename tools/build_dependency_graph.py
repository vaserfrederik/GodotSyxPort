"""Build syntax-level dependency evidence without claiming call or behavior parity."""
import argparse
from collections import Counter, defaultdict
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
REGISTRY = ROOT / "Porting" / "parity_registry.json"
OUTPUT = ROOT / "Porting" / "dependency_graph.json"
NAMESPACE = re.compile(r"^\s*namespace\s+([\w.]+)\s*[;{]", re.MULTILINE)
USING = re.compile(r"^\s*(?:global\s+)?using\s+(?!var\b)(?:static\s+)?([\w.]+)\s*;", re.MULTILINE)


def generate():
    registry = json.loads(REGISTRY.read_text())
    units = {e["JavaSource"]: e for e in registry["entries"]}
    types = {}
    for e in registry["entries"]:
        for declaration in e["JavaTypes"]:
            qualified = ".".join(filter(None, (e["Package"], declaration.split(" ", 1)[1])))
            if qualified in types and types[qualified] != e["JavaSource"]:
                raise ValueError(f"duplicate Java type: {qualified}")
            types[qualified] = e["JavaSource"]

    java = {}
    missing_candidates = Counter()
    for path, unit in sorted(units.items()):
        dependencies = set()
        external = set()
        wildcards = set()
        for imported in unit["JavaDependencies"]:
            name = imported.removeprefix("static ")
            if name.endswith(".*"):
                wildcards.add(imported)
                continue
            parts = name.split(".")
            resolved = next((types[prefix] for n in range(len(parts), 0, -1)
                             if (prefix := ".".join(parts[:n])) in types), None)
            if resolved and resolved != path:
                dependencies.add(resolved)
            elif not resolved:
                external.add(imported)
        if unit["CSharpImplementation"]:
            for target in dependencies:
                if not units[target]["CSharpImplementation"]:
                    missing_candidates[target] += 1
        java[path] = {
            "resolved_imports": sorted(dependencies),
            "unresolved_imports": sorted(external),
            "wildcard_imports": sorted(wildcards),
        }

    csharp = {}
    namespaces = defaultdict(list)
    scripts = sorted((ROOT / "Scripts").rglob("*.cs"))
    for source in scripts:
        path = source.relative_to(ROOT).as_posix()
        body = source.read_text(encoding="utf-8-sig")
        namespace = NAMESPACE.search(body)
        declared = namespace.group(1) if namespace else ""
        imports = sorted(set(USING.findall(body)))
        csharp[path] = {"namespace": declared, "using_namespaces": imports}
        namespaces[declared].append(path)
    for path, item in csharp.items():
        item["candidate_namespace_files"] = sorted({p for namespace in item["using_namespaces"]
            for p in namespaces.get(namespace, []) if p != path})

    return {
        "schema": 1,
        "source_archive_sha256": registry["source_archive_sha256"],
        "scope": "Java edges resolve explicit AST imports to source types. C# edges are namespace candidates from using directives, not proven symbol uses or call edges.",
        "summary": {
            "java_units": len(java),
            "java_resolved_import_edges": sum(len(x["resolved_imports"]) for x in java.values()),
            "java_unresolved_imports": sum(len(x["unresolved_imports"]) for x in java.values()),
            "java_wildcard_imports": sum(len(x["wildcard_imports"]) for x in java.values()),
            "csharp_files": len(csharp),
            "mapped_sources_with_unmapped_import": sum(bool(units[p]["CSharpImplementation"] and
                any(not units[d]["CSharpImplementation"] for d in e["resolved_imports"])) for p, e in java.items()),
        },
        "unmapped_java_imported_by_candidates": [
            {"JavaSource": path, "candidate_importers": count}
            for path, count in sorted(missing_candidates.items(), key=lambda x: (-x[1], x[0]))
        ],
        "java": java,
        "csharp": csharp,
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    graph = generate()
    if args.check:
        if graph != json.loads(OUTPUT.read_text()):
            sys.exit("Dependency graph differs from registry or C# sources")
    else:
        OUTPUT.write_text(json.dumps(graph, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps(graph["summary"], indent=2))


if __name__ == "__main__":
    main()
