#!/usr/bin/env python3
"""Select a large related semantic-port batch without pulling in old UI/engine code."""

from __future__ import annotations

import argparse
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "Porting/legacy_compile_manifest.json"
DEFER_PARTS = (
    "/Gui.cs", "/UI", "/view/", "/sprite/", "/spirte/", "/debug/",
    "Renderer.cs", "Constructor.cs", "Placer.cs",
    "TechIcon.cs", "Gui.cs", "UI.cs", "Sprite", "Hoverer.cs", "Tests.cs", "/tests/",
)
FINAL_STAGE_PREFIXES = (
    "game/battle/",
    "settlement/battle/",
    "settlement/entity/humanoid/ai/battle/",
)
FINAL_STAGE_FILES = {
    "settlement/entity/humanoid/ai/subject/PlanJoinArmy.cs",
    "settlement/entity/humanoid/ai/types/slave/PlanUprise.cs",
}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("prefix", nargs="+", help="LegacySource path prefixes")
    parser.add_argument("--limit", type=int, default=120)
    parser.add_argument("--output", default="Porting/next_macro_batch.json")
    args = parser.parse_args()

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    candidates = []
    deferred = []
    for item in manifest["files"]:
        path = item["path"]
        if item["status"] == "adapted_compiled_separately":
            continue
        if not any(path.startswith(prefix.rstrip("/") + "/") for prefix in args.prefix):
            continue
        final_stage = path.startswith(FINAL_STAGE_PREFIXES) or path in FINAL_STAGE_FILES
        target = deferred if final_stage or any(
            part.lower() in path.lower() for part in DEFER_PARTS) else candidates
        target.append({
            "path": path,
            "status": item["status"],
            "java": path[:-3] + ".java",
            "recovery_reasons": item.get("reasons", []),
        })

    candidates.sort(key=lambda item: (
        item["status"] != "recovery_required", item["path"].count("/"), item["path"]))
    selected = candidates[: max(1, args.limit)]
    payload = {
        "schema": 1,
        "strategy": "shared runtime systems; converted C# first; Java only for recovery/ambiguity",
        "prefixes": args.prefix,
        "requested_limit": args.limit,
        "selected_count": len(selected),
        "selected": selected,
        "deferred_ui_render_constructor_count": len(deferred),
        "deferred_ui_render_constructor": deferred,
    }
    output = ROOT / args.output
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"SEMANTIC_BATCH_OK {len(selected)} -> {output}")


if __name__ == "__main__":
    main()
