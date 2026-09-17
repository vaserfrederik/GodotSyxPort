using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;

namespace GodotSyxPort.Rooms;

/// <summary>
/// C# representation of Java FurnisherItem variations. A variation is a real occupied
/// footprint and separate source statistical/cost multipliers, not cosmetic scale settings.
/// </summary>
public sealed record FurnisherLayout(
    int Width,
    int Height,
    double CostMultiplier,
    double StatMultiplier,
    IReadOnlyList<GridCoord> Cells,
    IReadOnlyList<GridCoord> BlockerCells,
    IReadOnlyList<GridCoord> ReachableCells,
    IReadOnlyList<GridCoord> WorkCells,
    IReadOnlyList<GridCoord> StorageCells)
{
    private IReadOnlyList<GridCoord> Rotate(IReadOnlyList<GridCoord> cells, int rotation)
    {
        rotation = ((rotation % 4) + 4) % 4;
        return cells.Select(cell => rotation switch
        {
            1 => new GridCoord(Height - 1 - cell.Z, cell.X),
            2 => new GridCoord(Width - 1 - cell.X, Height - 1 - cell.Z),
            3 => new GridCoord(cell.Z, Width - 1 - cell.X),
            _ => cell
        }).ToArray();
    }

    public IReadOnlyList<GridCoord> RotatedCells(int rotation) => Rotate(Cells, rotation);
    public IReadOnlyList<GridCoord> RotatedBlockerCells(int rotation) => Rotate(BlockerCells, rotation);
    public IReadOnlyList<GridCoord> RotatedReachableCells(int rotation) => Rotate(ReachableCells, rotation);
    public IReadOnlyList<GridCoord> RotatedWorkCells(int rotation) => Rotate(WorkCells, rotation);
    public IReadOnlyList<GridCoord> RotatedStorageCells(int rotation) => Rotate(StorageCells, rotation);
    public GridCoord RotateCell(GridCoord cell, int rotation) => Rotate(new[] { cell }, rotation)[0];
    public int RotatedWidth(int rotation) => (rotation & 1) == 0 ? Width : Height;
    public int RotatedHeight(int rotation) => (rotation & 1) == 0 ? Height : Width;

    // PlacableFixedTool centres a fixed item on window.tile(), rather than using
    // the cursor as its upper-left corner.
    public GridCoord OriginAtCursor(GridCoord cursor, int rotation) => new(
        cursor.X - RotatedWidth(rotation) / 2,
        cursor.Z - RotatedHeight(rotation) / 2);
}

public sealed record FurnitureVisualPlacement(
    string RoomKey, int Group, int Variant, int Rotation, GridCoord Origin);

public static class FurnisherLayoutCatalog
{
    private static Dictionary<(string Family, int Group), List<FurnisherLayout>>? _sourceLayouts;
    private static FurnisherLayout Rectangle(int width, int height, double multiplier) =>
        new(width, height, multiplier, multiplier,
            Enumerable.Range(0, height).SelectMany(z =>
                Enumerable.Range(0, width).Select(x => new GridCoord(x, z))).ToArray(),
            Enumerable.Range(0, height).SelectMany(z =>
                Enumerable.Range(0, width).Select(x => new GridCoord(x, z))).ToArray(),
            Array.Empty<GridCoord>(), Array.Empty<GridCoord>(), Array.Empty<GridCoord>());

    public static IReadOnlyList<FurnisherLayout> Variants(string roomKey, int group)
    {
        _sourceLayouts ??= LoadSourceLayouts();
        var family = FamilyForRoom(roomKey);
        if (family is not null && _sourceLayouts.TryGetValue((family, group), out var layouts) && layouts.Count > 0)
            return layouts;
        return new[] { Rectangle(1, 1, 1) };
    }

    private static Dictionary<(string, int), List<FurnisherLayout>> LoadSourceLayouts()
    {
        var result = new Dictionary<(string, int), List<FurnisherLayout>>();
        var source = FileAccess.GetFileAsString("res://Data/Original/furnisher_layouts.tsv");
        foreach (var line in source.Split('\n').Skip(1))
        {
            var parts = line.Trim().Split('\t');
            if (parts.Length != 9 || !int.TryParse(parts[1], out var group) ||
                !int.TryParse(parts[2], out var width) || !int.TryParse(parts[3], out var height) ||
                !double.TryParse(parts[4], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var costMultiplier) ||
                !double.TryParse(parts[5], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var statMultiplier)) continue;
            var cells = parts[6].Split('/').SelectMany((row, z) => row.Select((value, x) => (value, x, z)))
                .Where(value => value.value == '1').Select(value => new GridCoord(value.x, value.z)).ToArray();
            var roles = parts[7].Split('/').SelectMany((row, z) =>
                row.Select((value, x) => (value, x, z))).ToArray();
            var blockers = roles.Where(value => value.value is 'b' or 'x')
                .Select(value => new GridCoord(value.x, value.z)).ToArray();
            var reachable = roles.Where(value => value.value is 'r' or 'x')
                .Select(value => new GridCoord(value.x, value.z)).ToArray();
            var functions = parts[8].Split('/').SelectMany((row, z) =>
                row.Select((value, x) => (value, x, z))).ToArray();
            var work = functions.Where(value => value.value == 'w')
                .Select(value => new GridCoord(value.x, value.z)).ToArray();
            var storage = functions.Where(value => value.value == 's')
                .Select(value => new GridCoord(value.x, value.z)).ToArray();
            var key = (parts[0], group);
            if (!result.TryGetValue(key, out var list)) result[key] = list = new List<FurnisherLayout>();
            list.Add(new FurnisherLayout(
                width, height, costMultiplier, statMultiplier,
                cells, blockers, reachable, work, storage));
        }
        return result;
    }

    public static string? FamilyForRoom(string key) => key.ToUpperInvariant() switch
    {
        var value when value.StartsWith("HUNTER_") => "food/hunter",
        var value when value.StartsWith("FISHERY_") => "food/fish",
        var value when value.StartsWith("ORCHARD_") => "food/orchard",
        "_CANNIBAL" => "food/cannibal",
        "_ASYLUM" => "health/asylum",
        "_HOSPITAL" => "health/hospital",
        var value when value.StartsWith("PHYSICIAN_") => "health/physician",
        "_HOME_CHAMBER" => "home/chamber",
        "_BENCH" => "infra/bench",
        "_BUILDER" => "infra/builder",
        "_EMBASSY" => "infra/embassy",
        "_EXPORT" => "infra/export",
        "_STATION" => "infra/station",
        var value when value.StartsWith("ADMIN_") => "infra/admin",
        var value when value.StartsWith("GATEHOUSE_") => "infra/gate",
        var value when value.StartsWith("RESTHOME_") => "infra/elderly",
        "_HAULER" => "infra/hauler",
        "_IMPORT" => "infra/importt",
        "_INN" => "infra/inn",
        "_JANITOR" => "infra/janitor",
        "_STOCKPILE" => "infra/stockpile",
        "_TRANSPORT" => "infra/transport",
        var value when value.StartsWith("MONUMENT_TORCH") => "infra/monument/torch",
        var value when value.StartsWith("MONUMENT_") => "infra/monument/imp",
        var value when value.StartsWith("LABORATORY_") => "knowledge/laboratory",
        var value when value.StartsWith("LIBRARY_") => "knowledge/library",
        var value when value.StartsWith("SCHOOL_") => "knowledge/school",
        var value when value.StartsWith("UNIVERSITY_") => "knowledge/university",
        "_COURT" => "law/court",
        "_EXECUTION" => "law/execution",
        "_GUARD" => "law/guard",
        "_POLICE" => "law/police",
        "_PRISON" => "law/prison",
        "_STOCKADE" => "law/stockade",
        "_STOCKS" => "law/stocks",
        var value when value.StartsWith("MINE_") => "industry/mine",
        var value when value.StartsWith("REFINER_") => "industry/refiner",
        var value when value.StartsWith("WORKSHOP_") => "industry/workshop",
        "_WOODCUTTER" => "industry/woodcutter",
        "_MILITARY_SUPPLY" => "military/supply",
        var value when value.StartsWith("ARCHERY_") => "military/training/archery",
        var value when value.StartsWith("BARRACKS_") => "military/training/barracks",
        var value when value.StartsWith("ARENAG_") => "service/arena/grand",
        var value when value.StartsWith("FIGHTPIT_") => "service/arena/pit",
        var value when value.StartsWith("BARBER_") => "service/barber",
        var value when value.StartsWith("CANTEEN_") => "service/food/canteen",
        var value when value.StartsWith("EATERY_") => "service/food/eatery",
        "_HEARTH" => "service/hearth",
        var value when value.StartsWith("BATH_") => "service/hygine/bath",
        var value when value.StartsWith("LAVATORY_") || value == "_LAVATORY" => "service/lavatory",
        var value when value.StartsWith("PLEASURE_") => "service/pleasure",
        var value when value.StartsWith("BREEDER_") => "service/breeder",
        var value when value.StartsWith("NURSERY_") => "service/nursery",
        var value when value.StartsWith("SPEAKER_") => "service/speaker",
        var value when value.StartsWith("STAGE_") => "service/stage",
        "PASTURE_BALTI" => "food/pasture/constructorindoor",
        var value when value.StartsWith("PASTURE_") => "food/pasture/constructoroutdoor",
        var value when value.StartsWith("ARTILLERY_") => "military/artillery",
        var value when value.StartsWith("TAVERN_") => "service/food/tavern",
        var value when value.StartsWith("MARKET_") => "service/market",
        var value when value.StartsWith("WELL_") => "service/hygine/well",
        var value when value.StartsWith("POOL_") => "water/pool",
        var value when value.StartsWith("GRAVEYARD_") => "spirit/grave/cgraveyard",
        var value when value.StartsWith("TOMB_") => "spirit/grave/ctomb",
        var value when value.StartsWith("TEMPLE_") => "spirit/temple",
        "_WATERCANAL" => "water/canal",
        "_WATERDRAIN" => "water/drain",
        _ => null
    };
}
