#!/usr/bin/env python3
"""Extract literal FurnisherItem grids from the authoritative Java source JAR."""
import argparse
from pathlib import Path
import re
import zipfile

root = Path(__file__).resolve().parents[1]
target = root / "Data" / "Original" / "furnisher_layouts.tsv"
rows = ["family\tgroup\twidth\theight\tcost_multiplier\tstat_multiplier\tmask\troles\tfunctions"]
parser = argparse.ArgumentParser()
parser.add_argument("source_jar", type=Path)
args = parser.parse_args()
with zipfile.ZipFile(args.source_jar) as archive:
    sources = {
        name: archive.read(name).decode("utf-8-sig")
        for name in archive.namelist()
        if name.startswith("settlement/room/") and name.endswith(".java") and
        "/main/furnisher/" not in name and
        (name == "settlement/room/home/house/HomeContructor.java" or
         re.search(r"new\s+FurnisherItem\s*\(\s*new\s+FurnisherItemTile\s*\[\]\s*\[\]",
                   archive.read(name).decode("utf-8-sig")))
    }
directory_counts = {
    str(Path(name).parent): sum(Path(other).parent == Path(name).parent for other in sources)
    for name in sources
}

BLOCKING_AVAILABILITY = {
    "SOLID", "NOT_ACCESSIBLE", "AVOID_PASS", "AVOID_LIKE_FUCK",
    "PENALTY4", "ROOM_SOLID",
}


def split_arguments(source: str) -> list[str]:
    result, start, depth = [], 0, 0
    for index, value in enumerate(source):
        if value in "([{":
            depth += 1
        elif value in ")]}":
            depth -= 1
        elif value == "," and depth == 0:
            result.append(source[start:index].strip())
            start = index + 1
    result.append(source[start:].strip())
    return result


def tile_roles(source: str) -> dict[str, tuple[bool, bool, str]]:
    """Return identifier -> (blocking, reachable, function) from Java constructors."""
    result: dict[str, tuple[bool, bool, str]] = {}
    constructor = re.compile(r"new\s+(?:FurnisherItemTile|Aux)\s*\(")
    for match in constructor.finditer(source):
        prefix = source[max(0, match.start() - 180):match.start()]
        assignment = re.search(
            r"(?:FurnisherItemTile\s+)?([A-Za-z_$][\w$]*)\s*=\s*$", prefix)
        if assignment is None:
            continue
        opening = source.find("(", match.start())
        depth, closing = 0, -1
        for closing in range(opening, len(source)):
            if source[closing] == "(":
                depth += 1
            elif source[closing] == ")":
                depth -= 1
                if depth == 0:
                    break
        arguments = split_arguments(source[opening + 1:closing])
        availability = re.search(
            r"AVAILABILITY\.([A-Z0-9_]+)", source[opening + 1:closing])
        if availability is None:
            continue
        must_be_reachable = len(arguments) >= 5 and arguments[1] == "true"
        statement_end = source.find(";", closing)
        data = re.search(r"\.setData\(([^)]+)\)", source[closing:statement_end])
        marker = data.group(1).upper() if data else ""
        function = "w" if "WORK" in marker or "JOB" in marker else \
            "s" if "STORAGE" in marker or "CRATE" in marker else \
            "d" if marker else "p"
        result[assignment.group(1)] = (
            availability.group(1) in BLOCKING_AVAILABILITY,
            must_be_reachable,
            function,
        )
    return result


for name, text in sorted(sources.items()):
    path = Path(name)
    family = path.parent.relative_to("settlement/room").as_posix()
    if directory_counts[str(path.parent)] > 1:
        family += "/" + path.stem.lower()
    group = 0
    cursor = 0
    roles_by_identifier = tile_roles(text)
    item_pattern = re.compile(
        r"new\s+FurnisherItem\s*\(\s*new\s+FurnisherItemTile\s*\[\]\s*\[\]\s*")
    while True:
        match = item_pattern.search(text, cursor)
        item = match.start() if match else -1
        flush = text.find("flush(", cursor)
        if flush >= 0 and (item < 0 or flush < item):
            group += 1
            cursor = flush + 6
            continue
        if item < 0:
            break
        opening = text.find("{", match.end())
        depth = 0
        closing = opening
        for closing in range(opening, len(text)):
            if text[closing] == "{": depth += 1
            elif text[closing] == "}":
                depth -= 1
                if depth == 0: break
        grid = text[opening + 1:closing]
        grid_rows = re.findall(r"\{([^{}]*)\}", grid, re.S)
        parsed = [[cell.strip() for cell in row.split(",") if cell.strip()] for row in grid_rows]
        if parsed:
            width, height = max(map(len, parsed)), len(parsed)
            mask_rows, role_rows, function_rows = [], [], []
            for row in parsed:
                mask_row, role_row, function_row = "", "", ""
                for cell in row:
                    if cell == "null":
                        mask_row += "0"
                        role_row += "0"
                        function_row += "0"
                        continue
                    blocker, reachable, function = roles_by_identifier.get(cell, (False, False, "p"))
                    mask_row += "1"
                    role_row += "x" if blocker and reachable else \
                        "b" if blocker else "r" if reachable else "p"
                    function_row += function
                mask_rows.append(mask_row.ljust(width, "0"))
                role_rows.append(role_row.ljust(width, "0"))
                function_rows.append(function_row.ljust(width, "0"))
            mask = "/".join(mask_rows)
            roles = "/".join(role_rows)
            functions = "/".join(function_rows)
            tail = text[closing + 1:text.find(");", closing) + 2]
            nums = re.findall(r",\s*([0-9]+(?:\.[0-9]+)?)", tail)
            cost_multiplier = float(nums[0]) if nums else 1.0
            stat_multiplier = float(nums[1]) if len(nums) > 1 else cost_multiplier
            rows.append(
                f"{family}\t{group}\t{width}\t{height}\t{cost_multiplier:g}\t"
                f"{stat_multiplier:g}\t{mask}\t{roles}\t{functions}")
        cursor = closing + 1

# HomeContructor creates three fixed, repeating house footprints procedurally.
# Each FurnisherItem is the complete room: apartment 3x3 (1..9 modules), house
# 3x5 (1..15), and longhouse 5x6 (1..30).  The entrance tile is reachable and
# the internal floor tiles are passable; the remaining item tiles are walls.
home = sources.get("settlement/room/home/house/HomeContructor.java", "")
if "final int[][] maxOccupants" in home and "create(new FurnisherItemTile[][]" in home:
    home_groups = [
        (3, ["bbb", "bpb", "brb"], 9),
        (3, ["bbb", "bpb", "bpb", "bpb", "brb"], 15),
        (5, ["bbbbb", "bpppb", "bpppb", "bpppb", "bpppb", "bbrbb"], 30),
    ]
    for group, (module_width, module_roles, maximum) in enumerate(home_groups):
        for modules in range(1, maximum + 1):
            width = module_width * modules
            height = len(module_roles)
            mask = "/".join("1" * width for _ in range(height))
            roles = "/".join(row * modules for row in module_roles)
            functions = "/".join("p" * width for _ in range(height))
            rows.append(
                f"home/house\t{group}\t{width}\t{height}\t{modules}\t{modules}\t"
                f"{mask}\t{roles}\t{functions}")

# industry/workshop/Constructor.java builds its storage group procedurally:
# height 1..2, width 2..10, every tile occupied, multiplier width*height.
workshop = sources.get("settlement/room/industry/workshop/Constructor.java", "")
if "for (int height = 1; height <= 2; height++)" in workshop and \
        "for (int width = sw; width <= ew; width++)" in workshop:
    for height in range(1, 3):
        for width in range(2, 11):
            mask = "/".join("1" * width for _ in range(height))
            roles = "/".join("b" * width for _ in range(height))
            functions = "/".join("p" + "s" * (width - 1) for _ in range(height))
            rows.append(f"industry/workshop\t0\t{width}\t{height}\t{width*height}\t{width*height}\t{mask}\t{roles}\t{functions}")
target.write_text("\n".join(rows) + "\n", encoding="utf-8")
print(f"wrote {len(rows)-1} layouts to {target}")
