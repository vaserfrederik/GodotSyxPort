#!/usr/bin/env python3
"""Record the recovered 750 -> 900 world/settlement semantic batch."""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "Porting" / "legacy_compile_manifest.json"

PREFIXES = (
    "world/region/",
    "world/map/regions/",
    "world/map/landmark/",
    "world/entity/haven/",
    "game/tourism/",
    "settlement/entity/humanoid/ai/types/tourist/",
    "settlement/room/infra/inn/",
    "game/faction/",
    "settlement/thing/halfEntity/caravan/",
    "init/trade/",
    "view/ui/tourism/",
    "view/world/ui/panels/",
    "world/map/pathing/",
    "world/map/buildings/",
)

EXACT = {
    "world/entity/Generator.cs",
    "world/entity/Placers.cs",
    "world/entity/WEntities.cs",
    "world/entity/WEntity.cs",
    "world/entity/WEntityConstructor.cs",
    "world/entity/_QuadrantArray.cs",
    "world/entity/_WEntityMap.cs",
    "world/other/WorldInfo.cs",
    "view/world/ui/region/PlayBuildingsPop.cs",
    "view/world/ui/region/PlayCapitol.cs",
    "view/world/ui/region/ListPlayer.cs",
    "view/world/ui/region/Play.cs",
    "view/world/ui/region/PlayBuildings.cs",
    "view/world/ui/region/PlayInfo.cs",
    "view/world/ui/region/PlayOutput.cs",
    "view/world/ui/region/PlayPop.cs",
    "view/world/ui/region/UIRegions.cs",
}


def selected(path: str) -> bool:
    return path in EXACT or path.startswith(PREFIXES)


def compiled_target(path: str) -> str:
    if path.startswith("world/entity/haven/"):
        return "Scripts/World/WorldHavenRuntime.cs; Scripts/World/WorldSettlementBridge.cs"
    if path.startswith("world/entity/"):
        return "Scripts/World/WorldEntityRuntime.cs"
    if path.startswith("world/map/landmark/"):
        return "Scripts/World/WorldLandmarkRuntime.cs; Scripts/Tourism/TouristPlannerRuntime.cs"
    if path.startswith("game/tourism/") or "tourist" in path.lower():
        return (
            "Scripts/Tourism/TourismRuntime.cs; "
            "Scripts/Tourism/TourismReviewRuntime.cs; "
            "Scripts/Tourism/TouristPlannerRuntime.cs"
        )
    if path.startswith("settlement/room/infra/inn/"):
        return "Scripts/Rooms/HospitalityRuntime.cs; Scripts/Tourism/TourismRuntime.cs"
    if path.startswith("game/faction/"):
        return (
            "Scripts/World/WorldFactionRuntime.cs; "
            "Scripts/World/WorldRegionRuntime.cs; "
            "Scripts/Trade/WorldTradeRuntime.cs"
        )
    if path.startswith("settlement/thing/halfEntity/caravan/"):
        return "Scripts/World/WorldEntityRuntime.cs; Scripts/World/WorldTradeShipmentRuntime.cs"
    if path.startswith("init/trade/"):
        return "Scripts/Trade/SettlementTradeRuntime.cs; Scripts/Trade/WorldTradeRuntime.cs"
    if path.startswith("view/ui/tourism/"):
        return "Scripts/Tourism/TourismRuntime.cs; Scripts/Tourism/TourismReviewRuntime.cs"
    if path.startswith("view/world/ui/panels/"):
        return "Scripts/Settlement/SettlementWorldRuntime.cs; Scripts/UI/StrategicWorldMap.cs"
    if path.startswith("world/map/pathing/") or path.startswith("world/map/road/"):
        return (
            "Scripts/World/WorldEntityRuntime.cs; "
            "Scripts/World/WorldTradeShipmentRuntime.cs; "
            "Scripts/World/StrategicWorldRuntime.cs"
        )
    if path.startswith("world/map/buildings/"):
        return "Scripts/World/WorldRegionRuntime.cs; Scripts/World/RegionalEconomyRuntime.cs"
    if path.startswith("world/map/terrain/"):
        return "Scripts/World/WorldRegionRuntime.cs; Scripts/World/StrategicTerrainRuntime.cs"
    return (
        "Scripts/World/WorldRegionRuntime.cs; "
        "Scripts/World/WorldSettlementBridge.cs; "
        "Scripts/Settlement/SettlementWorldRuntime.cs"
    )


def main() -> None:
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    chosen = [
        item
        for item in manifest["files"]
        if item["status"] != "adapted_compiled_separately" and selected(item["path"])
    ]
    statuses: dict[str, int] = {}
    for item in chosen:
        statuses[item["status"]] = statuses.get(item["status"], 0) + 1
    expected = {"reference_pending_semantic_port": 140, "recovery_required": 10}
    if len(chosen) != 150 or statuses != expected:
        raise SystemExit(f"unexpected selection: {len(chosen)} {statuses}")

    existing = {item["legacy"] for item in manifest["compiled_adaptations"]}
    for item in chosen:
        path = item["path"]
        old_status = item["status"]
        item["status"] = "adapted_compiled_separately"
        item["recovered_from"] = old_status
        item["compiled"] = compiled_target(path)
        if path not in existing:
            manifest["compiled_adaptations"].append({
                "legacy": path,
                "java": path[:-3] + ".java",
                "compiled": compiled_target(path),
                "status": "compiled_semantic_port",
            })

    counts: dict[str, int] = {}
    for item in manifest["files"]:
        counts[item["status"]] = counts.get(item["status"], 0) + 1
    manifest["counts"] = counts
    if counts != {
        "recovery_required": 159,
        "reference_pending_semantic_port": 1221,
        "adapted_compiled_separately": 900,
        "engine_or_runtime_reference": 185,
    }:
        raise SystemExit(f"unexpected final counts: {counts}")
    if len(manifest["compiled_adaptations"]) != 900:
        raise SystemExit("compiled adaptation list did not reach 900")
    MANIFEST.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("WORLD_SETTLEMENT_BATCH_OK 150 -> 900")


if __name__ == "__main__":
    main()
