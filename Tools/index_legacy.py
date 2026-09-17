#!/usr/bin/env python3
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

TYPE_RE = re.compile(r"\b(class|interface|enum|struct|record)\s+([A-Za-z_][A-Za-z0-9_]*)")
NAMESPACE_RE = re.compile(r"^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)", re.MULTILINE)
USING_RE = re.compile(r"^\s*using\s+(?:static\s+)?([A-Za-z_][A-Za-z0-9_.]*)\s*;", re.MULTILINE)
JAVA_MARKERS = re.compile(r"\bextends\b|\bimplements\b|^\s*package\s+|^\s*import\s+java\.", re.MULTILINE)
DESCRIPTION = re.compile(r"^(This (code|is|Java)|The (provided )?code|It (seems|appears))", re.IGNORECASE)
PLACEHOLDER = re.compile(r"Hello, World!|public\s+class\s+Program\b")


def subsystem(relative: Path) -> str:
    return relative.parts[0] if relative.parts else "root"


def main() -> None:
    if len(sys.argv) not in (3, 4):
        raise SystemExit("usage: index_legacy.py SOURCE_ROOT OUTPUT_DIR [JAVA_SOURCE_ROOT]")
    source = Path(sys.argv[1]).resolve()
    output = Path(sys.argv[2]).resolve()
    output.mkdir(parents=True, exist_ok=True)

    classes = []
    dependencies = {}
    broken = []
    counts = Counter()
    subsystem_files = defaultdict(list)

    for path in sorted(source.rglob("*.cs")):
        relative = path.relative_to(source)
        rel = relative.as_posix()
        text = path.read_text(encoding="utf-8-sig", errors="replace")
        namespace_match = NAMESPACE_RE.search(text)
        namespace = namespace_match.group(1) if namespace_match else None
        declared = [{"kind": kind, "name": name} for kind, name in TYPE_RE.findall(text)]
        imports = sorted(set(USING_RE.findall(text)))
        system = subsystem(relative)
        counts[system] += 1
        subsystem_files[system].append(rel)

        classes.append({
            "path": rel,
            "namespace": namespace,
            "types": declared,
            "line_count": text.count("\n") + 1,
            "subsystem": system,
        })
        dependencies[rel] = imports

        reasons = []
        first = text.lstrip().splitlines()[0] if text.strip() else ""
        if not text.strip(): reasons.append("empty")
        if DESCRIPTION.search(first): reasons.append("prose_description")
        if JAVA_MARKERS.search(text): reasons.append("java_syntax_marker")
        if PLACEHOLDER.search(text): reasons.append("placeholder_program")
        if not declared: reasons.append("no_type_declaration")
        elif relative.stem not in {item["name"] for item in declared} and relative.stem not in {"package-info", "module-info"}:
            reasons.append("filename_type_mismatch")
        if reasons: broken.append({"path": rel, "reasons": reasons})

    subsystem_report = {
        name: {
            "file_count": counts[name],
            "sample_files": files[:25],
        }
        for name, files in sorted(subsystem_files.items())
    }
    recovery_reasons = {"placeholder_program", "prose_description", "java_syntax_marker", "no_type_declaration", "empty"}
    recovery = [
        item for item in broken
        if any(reason in recovery_reasons for reason in item["reasons"])
    ]
    source_recovery_map = None
    if len(sys.argv) == 4:
        java_root = Path(sys.argv[3]).resolve()
        mappings = []
        for item in recovery:
            java_relative = Path(item["path"]).with_suffix(".java")
            available = (java_root / java_relative).is_file()
            mappings.append({
                "damaged_cs": item["path"],
                "java_source": java_relative.as_posix() if available else None,
                "available": available,
                "reasons": item["reasons"],
            })
        source_recovery_map = {
            "count": len(mappings),
            "matched": sum(item["available"] for item in mappings),
            "missing": sum(not item["available"] for item in mappings),
            "files": mappings,
        }

    payloads = {
        "class_index.json": {"source": str(source), "file_count": len(classes), "files": classes},
        "dependencies.json": dependencies,
        "broken_files.json": {"count": len(broken), "files": broken},
        "subsystems.json": subsystem_report,
        "source_recovery_required.json": {"count": len(recovery), "files": recovery},
    }
    for name, payload in payloads.items():
        (output / name).write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
    if source_recovery_map is not None:
        (output / "source_recovery_map.json").write_text(
            json.dumps(source_recovery_map, ensure_ascii=False, indent=2), encoding="utf-8")

    print(json.dumps({
        "files": len(classes),
        "declared_types": sum(len(item["types"]) for item in classes),
        "flagged": len(broken),
        "subsystems": dict(counts),
    }, ensure_ascii=False))


if __name__ == "__main__":
    main()
