#!/usr/bin/env python3
"""Extract literal FurnisherItemGroup and FurnisherStat minima from Java constructors."""
import argparse
import ast
from pathlib import Path
import re
import zipfile

root = Path(__file__).resolve().parents[1]
target = root / "Data" / "Original" / "furnisher_constraints.tsv"
parser = argparse.ArgumentParser()
parser.add_argument("source_jar", type=Path)
args = parser.parse_args()


def number(expression: str, fallback: float = 0.0) -> float:
    expression = expression.strip()
    try:
        tree = ast.parse(expression, mode="eval")
        if not all(isinstance(node, (ast.Expression, ast.Constant, ast.BinOp,
                                     ast.Add, ast.Sub, ast.Mult, ast.Div, ast.USub,
                                     ast.UnaryOp)) for node in ast.walk(tree)):
            return fallback
        return float(eval(compile(tree, "<java-number>", "eval"), {"__builtins__": {}}, {}))
    except (SyntaxError, ValueError, ZeroDivisionError):
        return fallback


def split_arguments(source: str) -> list[str]:
    result, start, depth = [], 0, 0
    for index, value in enumerate(source):
        if value in "([{": depth += 1
        elif value in ")]}": depth -= 1
        elif value == "," and depth == 0:
            result.append(source[start:index].strip())
            start = index + 1
    result.append(source[start:].strip())
    return result


def calls(source: str, pattern: re.Pattern[str]):
    for match in pattern.finditer(source):
        opening = source.find("(", match.start())
        depth = 0
        for closing in range(opening, len(source)):
            if source[closing] == "(": depth += 1
            elif source[closing] == ")":
                depth -= 1
                if depth == 0:
                    yield match, split_arguments(source[opening + 1:closing])
                    break


with zipfile.ZipFile(args.source_jar) as archive:
    sources = {
        name: archive.read(name).decode("utf-8-sig")
        for name in archive.namelist()
        if name.startswith("settlement/room/") and name.endswith(".java") and
        "/main/furnisher/" not in name and "flush(" in archive.read(name).decode("utf-8-sig")
    }

directory_counts = {
    str(Path(name).parent): sum(Path(other).parent == Path(name).parent for other in sources)
    for name in sources
}
rows = ["family\tgroup\tminimum\tmaximum\tstat_minimums"]
flush_pattern = re.compile(r"(?<![\w.])flush\s*\(")
stat_pattern = re.compile(r"new\s+FurnisherStat(?:\.([A-Za-z0-9_]+))?\s*\(")

for name, source in sorted(sources.items()):
    path = Path(name)
    family = path.parent.relative_to("settlement/room").as_posix()
    if directory_counts[str(path.parent)] > 1:
        family += "/" + path.stem.lower()

    stat_minimums = []
    for match, arguments in calls(source, stat_pattern):
        if not arguments or arguments[0] != "this":
            continue
        kind = match.group(1) or "Raw"
        minimum = 0.0
        if kind in {"FurnisherStatI", "FurnisherStatEmployees"} and len(arguments) >= 2:
            minimum = number(arguments[1])
        elif kind in {"FurnisherStatServices", "FurnisherStatProduction",
                      "FurnisherStatProduction2"} and len(arguments) >= 3:
            minimum = number(arguments[-1])
        elif kind == "Raw" and len(arguments) in {2, 4}:
            minimum = number(arguments[-1])
        stat_minimums.append(minimum)

    group = 0
    for _, arguments in calls(source, flush_pattern):
        if not arguments or not all(re.fullmatch(r"[0-9]+", value.strip()) for value in arguments):
            continue
        values = [int(value) for value in arguments]
        minimum = values[0] if len(values) >= 2 else 0
        maximum = values[1] if len(values) >= 3 else 2147483647
        rows.append(f"{family}\t{group}\t{minimum}\t{maximum}\t" +
                    ",".join(f"{value:g}" for value in stat_minimums))
        group += 1

target.write_text("\n".join(rows) + "\n", encoding="utf-8")
print(f"wrote {len(rows)-1} group constraints to {target}")
