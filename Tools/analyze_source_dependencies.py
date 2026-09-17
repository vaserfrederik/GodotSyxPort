#!/usr/bin/env python3
"""Build a deterministic subsystem/dependency index for Java and converted C#."""

from __future__ import annotations

import json
import re
from collections import Counter, defaultdict
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[1]
WORKSPACE = PROJECT.parent
JAVA = WORKSPACE / "source_reference" / "java"
CS = PROJECT / "LegacySource"
MANIFEST = PROJECT / "Porting" / "legacy_compile_manifest.json"
OUTPUT = PROJECT / "Porting" / "source_dependency_map.json"


def subsystem(relative: str) -> str:
    parts = relative.split("/")
    directories = parts[:-1]
    return "/".join(directories[:3])


def imported_subsystem(name: str, namespace: bool = False) -> str:
    parts = name.rstrip(".*").split(".")
    if not namespace and parts:
        parts = parts[:-1]
    return "/".join(parts[:3])


def java_info(path: Path) -> tuple[str, list[str]]:
    source = path.read_text(encoding="utf-8-sig", errors="ignore")
    package = re.search(r"(?m)^\s*package\s+([\w.]+)\s*;", source)
    imports = re.findall(r"(?m)^\s*import\s+(?:static\s+)?([\w.*]+)\s*;", source)
    return package.group(1) if package else "", imports


def cs_info(path: Path) -> tuple[str, list[str]]:
    source = path.read_text(encoding="utf-8-sig", errors="ignore")
    namespace = re.search(r"(?m)^\s*namespace\s+([\w.]+)", source)
    usings = re.findall(r"(?m)^\s*using\s+([\w.]+)\s*;", source)
    return namespace.group(1) if namespace else "", usings


def main() -> None:
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    statuses = {entry["path"]: entry["status"] for entry in manifest["files"]}
    groups: dict[str, dict[str, object]] = defaultdict(lambda: {
        "java_files": 0,
        "converted_cs_files": 0,
        "statuses": Counter(),
        "dependencies": Counter(),
    })

    for path in JAVA.rglob("*.java"):
        relative = path.relative_to(JAVA).as_posix()
        group = subsystem(relative)
        _, imports = java_info(path)
        groups[group]["java_files"] += 1
        for dependency in imports:
            target = imported_subsystem(dependency)
            if target and target != group:
                groups[group]["dependencies"][target] += 1

    for path in CS.rglob("*.cs"):
        relative = path.relative_to(CS).as_posix()
        group = subsystem(relative)
        _, usings = cs_info(path)
        groups[group]["converted_cs_files"] += 1
        groups[group]["statuses"][statuses.get(relative, "untracked")] += 1
        for dependency in usings:
            target = imported_subsystem(dependency, namespace=True)
            if target and target != group:
                groups[group]["dependencies"][target] += 1

    result = {
        "schema": 1,
        "policy": "Group related sources for batch semantic ports; UI/debug/platform groups are deferred.",
        "subsystems": [],
    }
    for name, raw in sorted(groups.items()):
        dependencies = raw["dependencies"]
        result["subsystems"].append({
            "name": name,
            "java_files": raw["java_files"],
            "converted_cs_files": raw["converted_cs_files"],
            "statuses": dict(sorted(raw["statuses"].items())),
            "top_dependencies": [
                {"subsystem": target, "references": count}
                for target, count in dependencies.most_common(12)
            ],
        })
    OUTPUT.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(f"DEPENDENCY_MAP_OK {len(result['subsystems'])} subsystems -> {OUTPUT}")


if __name__ == "__main__":
    main()
