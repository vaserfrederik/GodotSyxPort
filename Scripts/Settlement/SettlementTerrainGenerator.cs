using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.World;

namespace GodotSyxPort.Settlement;

[Flags]
public enum SettlementOceanSides : byte { None = 0, North = 1, East = 2, South = 4, West = 8 }
[Flags]
public enum SettlementWaterSides : byte { None = 0, North = 1, East = 2, South = 4, West = 8 }

public sealed record SettlementWorldTileSample(
    double Height, double Moisture, double Fertility, double Forest,
    bool Mountain, StrategicWaterKind Water, string Climate, bool Road = false,
    SettlementWaterSides RiverConnections = SettlementWaterSides.None,
    SettlementOceanSides OceanConnections = SettlementOceanSides.None,
    SettlementWaterSides MountainConnections = SettlementWaterSides.None,
    SettlementWaterSides RoadConnections = SettlementWaterSides.None);

public sealed record SettlementGeneratorSettings(
    double CaveAmount, double CaveSize, double CaveTunnels, double MountainSize,
    double MountainRandom, int RiverWidth, double LakeSize, double LakeSmall,
    double LakeIslands, double ForestAmount, double ForestDensity,
    double MineralsAmount, double EdiblesAmount)
{
    public static SettlementGeneratorSettings Load()
    {
        var path = ProjectSettings.GlobalizePath("res://Data/Original/init/config/GenerationSettlement.txt");
        if (!File.Exists(path)) throw new FileNotFoundException("Original settlement generation config", path);
        var generation = SyxDataParser.Parse(File.ReadAllText(path)).Get("GENERATION") ??
            throw new InvalidDataException("GenerationSettlement.txt has no GENERATION object");
        double Number(string key) => generation.Get(key)?.Number() ??
            throw new InvalidDataException($"GenerationSettlement.txt has no {key}");
        return new SettlementGeneratorSettings(
            Number("CAVE_AMOUNT"), Number("CAVE_SIZE"), Number("CAVE_TUNNELS"),
            Number("MOUNTAIN_SIZE"), Number("MOUNTAIN_RANDOM"), (int)Number("RIVER_WIDTH"),
            Number("LAKE_SIZE"), Number("LAKE_SMALL"), Number("LAKE_ISLANDS"),
            Number("FOREST_AMOUNT"), Number("FOREST_DENSITY"),
            Number("MINERALS_AMOUNT"), Number("EDIBLES_AMOUNT"));
    }
}

/// <summary>
/// Dense settlement terrain pipeline. Stage order follows settlement.tilemap.generator.Generator.
/// Strategic-region inputs are explicit so the future world map can replace Neutral without rewriting generation.
/// </summary>
public sealed record SettlementGenerationProfile(
    int Seed,
    double BaseFertility,
    double MountainAmount,
    double WaterAmount,
    bool IncludeRareMinables,
    IReadOnlyList<MinableRule> Minables,
    IReadOnlyList<GrowableRule> Growables,
    string Climate,
    double EdiblesAmount,
    SettlementOceanSides OceanSides,
    SettlementWaterSides RiverSides,
    SettlementWaterSides SmallRiverSides,
    double LakeAmount,
    IReadOnlyList<SettlementWorldTileSample>? WorldTiles = null,
    int WorldTileDimension = StrategicWorldRuntime.CapitalFootprintDimension)
{
    public SettlementWorldTileSample? WorldTileAtSettlement(int x, int z, int width, int height)
    {
        if (WorldTiles is null || WorldTiles.Count != WorldTileDimension * WorldTileDimension)
            return null;
        var sx = Math.Clamp(x * WorldTileDimension / Math.Max(1, width), 0, WorldTileDimension - 1);
        var sz = Math.Clamp(z * WorldTileDimension / Math.Max(1, height), 0, WorldTileDimension - 1);
        return WorldTiles[sx + sz * WorldTileDimension];
    }

    public static SettlementGenerationProfile Neutral(
        int seed, IReadOnlyList<MinableRule> minables, IReadOnlyList<GrowableRule> growables) =>
        new(seed, 0.55, 0.18, 0.12, false, minables, growables, "TEMPERATE", 0.5,
            SettlementOceanSides.None, SettlementWaterSides.None, SettlementWaterSides.None, 0);

    public static SettlementGenerationProfile FromRegion(
        int worldSeed,
        StrategicWorldRuntime world,
        StrategicRegion region,
        IReadOnlyList<MinableRule> minables,
        IReadOnlyList<GrowableRule> growables)
    {
        var oceanSides = SettlementOceanSides.None;
        if (HasOceanEdge(world, region, SettlementOceanSides.North)) oceanSides |= SettlementOceanSides.North;
        if (HasOceanEdge(world, region, SettlementOceanSides.East)) oceanSides |= SettlementOceanSides.East;
        if (HasOceanEdge(world, region, SettlementOceanSides.South)) oceanSides |= SettlementOceanSides.South;
        if (HasOceanEdge(world, region, SettlementOceanSides.West)) oceanSides |= SettlementOceanSides.West;
        // System.HashCode is process-randomized; saves need the same terrain seed after restart.
        var seed = unchecked(worldSeed * 486187739 ^ region.Id * 16777619 ^
                             region.X * 73856093 ^ region.Y * 19349663);
        return new SettlementGenerationProfile(
            seed,
            Math.Clamp(region.Fertility, 0.08, 0.95),
            Math.Clamp(0.04 + region.Mountain * 0.58, 0.04, 0.62),
            Math.Clamp(0.04 + Math.Max(region.Water, region.River) * 0.42, 0.04, 0.42),
            region.Mountain > 0.32 || region.Elevation > 0.62,
            minables,
            growables,
            string.IsNullOrWhiteSpace(region.Climate) ? "TEMPERATE" : region.Climate,
            Math.Clamp(0.15 + region.Fertility * 0.65 + region.Forest * 0.2, 0.1, 1.0),
            oceanSides,
            region.River > 0.02 ? SettlementWaterSides.North | SettlementWaterSides.South : SettlementWaterSides.None,
            SettlementWaterSides.None,
            Math.Clamp(region.Water - region.River - region.Ocean, 0, 1));
    }

    /// <summary>
    /// CreateFromWorldMap receives the selected source 3x3 world-centre footprint, not only
    /// a realm-wide average. Preserve the region overload for restored legacy saves,
    /// while new games derive their settlement terrain from the actual chosen tiles.
    /// </summary>
    public static SettlementGenerationProfile FromWorldSite(
        int worldSeed,
        StrategicWorldRuntime world,
        StrategicRegion region,
        int centerX,
        int centerY,
        IReadOnlyList<MinableRule> minables,
        IReadOnlyList<GrowableRule> growables)
    {
        if (!world.CanPlaceCapitalAt(centerX, centerY))
            return FromRegion(worldSeed, world, region, minables, growables);
        var fertility = 0.0; var moisture = 0.0; var forest = 0.0; var elevation = 0.0;
        var mountain = 0; var water = 0; var samples = 0;
        var climates = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var oceanSides = SettlementOceanSides.None;
        var riverSides = SettlementWaterSides.None;
        var smallRiverSides = SettlementWaterSides.None;
        var hasRiver = false;
        var hasSmallRiver = false;
        var lakes = 0;
        var worldTiles = new List<SettlementWorldTileSample>(
            StrategicWorldRuntime.CapitalFootprintDimension * StrategicWorldRuntime.CapitalFootprintDimension);
        var half = StrategicWorldRuntime.CapitalFootprintDimension / 2;
        static bool Rivery(StrategicWaterKind kind) => kind is
            StrategicWaterKind.River or StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta;
        static bool Ocean(StrategicWaterKind kind) => kind is
            StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean;
        SettlementWaterSides WaterConnections(int x, int y)
        {
            var sides = SettlementWaterSides.None;
            if (Rivery(world.Terrain.Water(x, y - 1))) sides |= SettlementWaterSides.North;
            if (Rivery(world.Terrain.Water(x + 1, y))) sides |= SettlementWaterSides.East;
            if (Rivery(world.Terrain.Water(x, y + 1))) sides |= SettlementWaterSides.South;
            if (Rivery(world.Terrain.Water(x - 1, y))) sides |= SettlementWaterSides.West;
            return sides;
        }
        SettlementOceanSides OceanConnections(int x, int y)
        {
            var sides = SettlementOceanSides.None;
            if (Ocean(world.Terrain.Water(x, y - 1))) sides |= SettlementOceanSides.North;
            if (Ocean(world.Terrain.Water(x + 1, y))) sides |= SettlementOceanSides.East;
            if (Ocean(world.Terrain.Water(x, y + 1))) sides |= SettlementOceanSides.South;
            if (Ocean(world.Terrain.Water(x - 1, y))) sides |= SettlementOceanSides.West;
            return sides;
        }
        SettlementWaterSides MountainConnections(int x, int y)
        {
            var sides = SettlementWaterSides.None;
            if (world.Terrain.Mountain(x, y - 1)) sides |= SettlementWaterSides.North;
            if (world.Terrain.Mountain(x + 1, y)) sides |= SettlementWaterSides.East;
            if (world.Terrain.Mountain(x, y + 1)) sides |= SettlementWaterSides.South;
            if (world.Terrain.Mountain(x - 1, y)) sides |= SettlementWaterSides.West;
            return sides;
        }
        SettlementWaterSides RoadConnections(int x, int y)
        {
            var sides = SettlementWaterSides.None;
            if (world.HasRoad(x, y - 1)) sides |= SettlementWaterSides.North;
            if (world.HasRoad(x + 1, y)) sides |= SettlementWaterSides.East;
            if (world.HasRoad(x, y + 1)) sides |= SettlementWaterSides.South;
            if (world.HasRoad(x - 1, y)) sides |= SettlementWaterSides.West;
            return sides;
        }
        for (var dz = -half; dz < StrategicWorldRuntime.CapitalFootprintDimension - half; dz++)
        for (var dx = -half; dx < StrategicWorldRuntime.CapitalFootprintDimension - half; dx++)
        {
            var x = centerX + dx; var y = centerY + dz;
            samples++;
            fertility += world.Terrain.Fertility(x, y);
            moisture += world.Terrain.Moisture(x, y);
            forest += world.Terrain.Forest(x, y);
            elevation += world.Terrain.Height(x, y);
            if (world.Terrain.Mountain(x, y)) mountain++;
            var kind = world.Terrain.Water(x, y);
            if (kind != StrategicWaterKind.None) water++;
            if (kind is StrategicWaterKind.Lake or StrategicWaterKind.DeepLake) lakes++;
            if (kind is StrategicWaterKind.River or StrategicWaterKind.Delta)
            {
                hasRiver = true;
                riverSides |= SideFor(dx, dz, half);
            }
            if (kind == StrategicWaterKind.SmallRiver)
            {
                hasSmallRiver = true;
                smallRiverSides |= SideFor(dx, dz, half);
            }
            if (kind is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean)
            {
                if (dz == -half) oceanSides |= SettlementOceanSides.North;
                if (dx == StrategicWorldRuntime.CapitalFootprintDimension - half - 1)
                    oceanSides |= SettlementOceanSides.East;
                if (dz == StrategicWorldRuntime.CapitalFootprintDimension - half - 1)
                    oceanSides |= SettlementOceanSides.South;
                if (dx == -half) oceanSides |= SettlementOceanSides.West;
            }
            var climate = world.Terrain.Climate(x, y);
            climates[climate] = climates.GetValueOrDefault(climate) + 1;
            worldTiles.Add(new SettlementWorldTileSample(
                world.Terrain.Height(x, y), world.Terrain.Moisture(x, y),
                world.Terrain.Fertility(x, y), world.Terrain.Forest(x, y),
                world.Terrain.Mountain(x, y), kind, climate, world.HasRoad(x, y)));
            worldTiles[^1] = worldTiles[^1] with
            {
                RiverConnections = WaterConnections(x, y),
                OceanConnections = OceanConnections(x, y),
                MountainConnections = MountainConnections(x, y),
                RoadConnections = RoadConnections(x, y)
            };
        }
        var seed = unchecked(worldSeed * 486187739 ^ centerX * 73856093 ^ centerY * 19349663);
        var averageFertility = fertility / samples;
        var averageMoisture = moisture / samples;
        var averageForest = forest / samples;
        var averageElevation = elevation / samples;
        if (hasRiver && riverSides == SettlementWaterSides.None)
            riverSides = SettlementWaterSides.North | SettlementWaterSides.South;
        if (hasSmallRiver && smallRiverSides == SettlementWaterSides.None)
            smallRiverSides = SettlementWaterSides.West | SettlementWaterSides.East;
        return new SettlementGenerationProfile(
            seed,
            Math.Clamp(averageFertility, 0.05, 0.95),
            Math.Clamp(0.04 + mountain / (double)samples * 0.58, 0.04, 0.62),
            Math.Clamp(0.04 + water / (double)samples * 0.42, 0.04, 0.42),
            mountain / (double)samples > 0.32 || averageElevation > 0.62,
            minables,
            growables,
            climates.OrderByDescending(value => value.Value).First().Key,
            Math.Clamp(0.15 + averageFertility * 0.65 + averageForest * 0.2, 0.1, 1.0),
            oceanSides, riverSides, smallRiverSides, lakes / (double)samples,
            worldTiles, StrategicWorldRuntime.CapitalFootprintDimension);
    }

    private static SettlementWaterSides SideFor(int dx, int dz, int edge)
    {
        var side = SettlementWaterSides.None;
        if (dz == -edge) side |= SettlementWaterSides.North;
        if (dz == StrategicWorldRuntime.CapitalFootprintDimension - edge - 1)
            side |= SettlementWaterSides.South;
        if (dx == -edge) side |= SettlementWaterSides.West;
        if (dx == StrategicWorldRuntime.CapitalFootprintDimension - edge - 1)
            side |= SettlementWaterSides.East;
        return side;
    }

    private static bool HasOceanEdge(
        StrategicWorldRuntime world, StrategicRegion region, SettlementOceanSides side)
    {
        var size = StrategicWorldRuntime.TileDimension;
        var offset = side switch
        {
            SettlementOceanSides.North => (X: 0, Y: -1),
            SettlementOceanSides.East => (X: 1, Y: 0),
            SettlementOceanSides.South => (X: 0, Y: 1),
            _ => (X: -1, Y: 0)
        };
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            if (world.RegionIdAtTile(x, y) != region.Id) continue;
            var nx = x + offset.X; var ny = y + offset.Y;
            if (world.RegionIdAtTile(nx, ny) == region.Id) continue;
            if (world.Terrain.Water(nx, ny) is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean)
                return true;
        }
        return false;
    }
}

public sealed class SettlementTerrainGenerator
{
    public void Generate(WorldGridData world, SettlementGenerationProfile profile)
    {
        var settings = SettlementGeneratorSettings.Load();
        Generate(world, profile, settings);
    }

    public void Generate(WorldGridData world, SettlementGenerationProfile profile,
        SettlementGeneratorSettings settings)
    {
        // Original order: fertility init, mountain/cave, rivers/lakes/water, minerals,
        // ground, fertility final, growth/edibles. Growth and edibles remain separate systems.
        GenerateBaseAndFertility(world, profile);
        GenerateMountains(world, profile, settings);
        GenerateCaves(world, profile, settings);
        GenerateWater(world, profile, settings);
        FinishWater(world);
        GenerateOcean(world, profile);
        GenerateLakeExtra(world, profile);
        GenerateFish(world, profile);
        GenerateMinerals(world, profile, settings);
        FinishGroundAndFertility(world, profile, settings);
        GenerateGrowth(world, profile);
        GenerateEdibles(world, profile, settings);
        GenerateRoads(world, profile);
        GenerateFoundation(world, profile);
    }

    private static void GenerateBaseAndFertility(WorldGridData world, SettlementGenerationProfile profile)
    {
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            var noiseHeight = Fractal(x, z, profile.Seed, 5);
            var mappedHeight = SampleWorld(profile, x, z, world.Width, world.Height,
                sample => sample.Height, noiseHeight);
            var height = profile.WorldTiles is null ? noiseHeight :
                Math.Clamp(mappedHeight * 0.72 + noiseHeight * 0.28, 0, 1);
            var noiseMoisture = Fractal(x + 1703, z - 927, profile.Seed ^ 0x41a7, 4);
            var mappedMoisture = SampleWorld(profile, x, z, world.Width, world.Height,
                sample => sample.Moisture, profile.BaseFertility);
            var localMoisture = profile.WorldTiles is null ? noiseMoisture :
                Math.Clamp(mappedMoisture * 0.82 + noiseMoisture * 0.18, 0, 1);
            // GeneratorFertilityInit.get: bilinear world moisture, HeightMap(32,2),
            // then base*0.7 + pow(1-height, 1+8*(1-base)) - 0.2*fertilityNoise.
            var baseValue = Math.Clamp(mappedMoisture, 0, 1) * 0.7;
            var fertilityHeight = 1.0 - noiseHeight;
            var fertilityNoise = Fractal(x - 419, z + 733, profile.Seed ^ 0x61c5, 2);
            var fertility = Math.Pow(fertilityHeight, 1 + 8 * (1 - baseValue));
            fertility = Math.Clamp(baseValue + fertility - 0.2 * fertilityNoise, 0, 1);
            world.SetTerrain(cell, GroundKind.Soil, ToByte(height), ToNibble(fertility), ToNibble(localMoisture));
        }
    }

    private static void GenerateMountains(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        var anyMappedMountain = profile.WorldTiles?.Any(sample => sample.Mountain) == true;
        var threshold = profile.WorldTiles is null
            ? 1.0 - Math.Clamp(profile.MountainAmount * (0.8 + settings.MountainSize), 0.02, 0.75)
            : 0.43 - settings.MountainSize * 0.12;
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            var ridge = Fractal(x - 400, z + 211, profile.Seed ^ 0x721d, 3);
            if (profile.WorldTiles is not null)
            {
                if (!anyMappedMountain) continue;
                // WorldMountain.AreaTileMountain is an edge/corner topology, not a
                // 256x256 boolean fill. Bilinear centre weights reproduce that topology
                // from the selected 3x3 world samples and the two noise bands refine it.
                var topology = SampleWorld(profile, x, z, world.Width, world.Height,
                    sample => sample.Mountain ? 1.0 : 0.0, 0);
                var warped = Fractal(x + (int)((ridge - 0.5) * 52),
                    z + (int)((ridge - 0.5) * 37), profile.Seed ^ 0x2bf1, 3);
                ridge = topology * 0.82 + warped * 0.24 - 0.10;
            }
            ridge += (Noise(x, z, profile.Seed ^ 0x3281) - 0.5) *
                     (0.08 + settings.MountainRandom * 0.22);
            if (ridge < threshold) continue;
            world.SetTerrain(cell, GroundKind.Mountain, world.Elevation(cell), 0, world.Moisture(cell));
            world.Set(cell, TileFlags.ClearableTerrain, true);
        }
    }

    /// <summary>
    /// GeneratorCave: CAVE_AMOUNT*300 attempts, CAVE_SIZE*30 cluster points and
    /// CAVE_TUNNELS-controlled links. Candidates that escape mountain terrain are rejected.
    /// </summary>
    private static void GenerateCaves(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        var random = new Random(profile.Seed ^ 0x332b);
        var attempts = Math.Max(0, (int)(settings.CaveAmount * 300));
        var caveSize = Math.Max(1, (int)(settings.CaveSize * 30));
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var start = new GridCoord(random.Next(world.Width), random.Next(world.Height));
            if (!world.Has(start, TileFlags.Mountain)) continue;
            var points = new List<GridCoord>();
            var count = random.Next(caveSize);
            for (var i = 0; i < count; i++)
            {
                var point = new GridCoord(start.X + random.Next(-10 - caveSize, 11 + caveSize),
                    start.Z + random.Next(-10 - caveSize, 11 + caveSize));
                if (!world.IsInside(point) || !world.Has(point, TileFlags.Mountain)) continue;
                points.Add(point);
            }
            if (points.Count == 0) continue;
            CarveCaveLinks(world, start, points);
            var tunnels = random.Next(1 + (int)(points.Count * settings.CaveTunnels * 10));
            for (var i = 0; i < tunnels; i++)
                CarveCaveTunnel(world, points[i % points.Count], random);
        }
    }

    private static void CarveCaveLinks(WorldGridData world, GridCoord start, IEnumerable<GridCoord> points)
    {
        foreach (var point in points)
        {
            var x = start.X; var z = start.Z;
            while (x != point.X || z != point.Z)
            {
                if (!world.Has(new GridCoord(x, z), TileFlags.Mountain)) break;
                world.SetCave(new GridCoord(x, z), true);
                if (x != point.X) x += Math.Sign(point.X - x);
                else z += Math.Sign(point.Z - z);
            }
            if (world.Has(point, TileFlags.Mountain)) world.SetCave(point, true);
        }
    }

    private static void CarveCaveTunnel(WorldGridData world, GridCoord start, Random random)
    {
        var cell = start;
        var maximum = 60 + random.Next(60);
        for (var step = 0; step < maximum && world.IsInside(cell) &&
             world.Has(cell, TileFlags.Mountain); step++)
        {
            world.SetCave(cell, true);
            var direction = GridCoord.Cardinal[random.Next(GridCoord.Cardinal.Length)];
            cell += direction;
        }
    }

    private static void GenerateWater(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        if (profile.WorldTiles is not null)
        {
            GenerateMappedFreshWater(world, profile, settings);
            return;
        }
        GenerateRiver(world, profile.RiverSides,
            Math.Max(1, settings.RiverWidth), profile.Seed ^ 0x7117);
        GenerateRiver(world, profile.SmallRiverSides, 1, profile.Seed ^ 0x5117);
        if (profile.LakeAmount <= 0) return;
        var random = new Random(profile.Seed ^ 0x19f1);
        var lakes = Math.Max(1, (int)Math.Ceiling(profile.LakeAmount * 4));
        var radius = Math.Max(12, (int)(settings.LakeSize * 200));
        for (var lake = 0; lake < lakes; lake++)
        {
            var center = new GridCoord(random.Next(radius, world.Width - radius),
                random.Next(radius, world.Height - radius));
            for (var z = center.Z - radius; z <= center.Z + radius; z++)
            for (var x = center.X - radius; x <= center.X + radius; x++)
            {
                var dx = x - center.X; var dz = z - center.Z;
                var edgeNoise = 0.72 + Noise(x / 4, z / 4, profile.Seed ^ lake * 7919) * 0.35;
                if (dx * dx + dz * dz > radius * radius * edgeNoise) continue;
                var cell = new GridCoord(x, z);
                world.SetTerrain(cell, GroundKind.FreshWater, world.Elevation(cell), 0, 15);
            }
        }
    }

    private static void GenerateMappedFreshWater(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        var dimension = profile.WorldTileDimension;
        var quadWidth = world.Width / dimension;
        var quadHeight = world.Height / dimension;
        bool Rivery(int x, int z)
        {
            if ((uint)x >= dimension || (uint)z >= dimension) return false;
            return profile.WorldTiles![x + z * dimension].Water is
                StrategicWaterKind.River or StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta;
        }
        bool Lake(int x, int z)
        {
            if ((uint)x >= dimension || (uint)z >= dimension) return false;
            return profile.WorldTiles![x + z * dimension].Water is
                StrategicWaterKind.Lake or StrategicWaterKind.DeepLake;
        }

        // GeneratorLake walks SettlementGrid.Tile.getDirs().  That direction set
        // owns every centre/edge/corner exactly once, and stamps a lake only when
        // the corresponding neighbouring world tile is also lake.  Painting one
        // arbitrary disc at every quadrant centre loses the selected world shape.
        var lakeRadius = Math.Max(12, (int)Math.Round(settings.LakeSize * 200));
        var innerOffsetX = quadWidth * 3 / 8;
        var innerOffsetZ = quadHeight * 3 / 8;
        var ownedDirections = new (int X, int Z)[]
        {
            (1, 0), (1, 1), (0, 1), (0, 0)
        };
        for (var z = 0; z < dimension; z++)
        for (var x = 0; x < dimension; x++)
        {
            var kind = profile.WorldTiles![x + z * dimension].Water;
            var sample = profile.WorldTiles[x + z * dimension];
            var center = new GridCoord(x * quadWidth + quadWidth / 2, z * quadHeight + quadHeight / 2);
            if (kind is StrategicWaterKind.Lake or StrategicWaterKind.DeepLake)
            {
                var directions = new List<(int X, int Z)>(ownedDirections);
                if (x == 0) { directions.Add((-1, 0)); directions.Add((-1, 1)); }
                if (z == 0) { directions.Add((1, -1)); directions.Add((0, -1)); }
                if (x == 0 && z == 0) directions.Add((-1, -1));
                foreach (var direction in directions)
                {
                    if (!Lake(x + direction.X, z + direction.Z)) continue;
                    var lakeCenter = new GridCoord(center.X + direction.X * innerOffsetX,
                        center.Z + direction.Z * innerOffsetZ);
                    PaintLakeDisc(world, lakeCenter, lakeRadius);
                }
                continue;
            }
            if (kind is not (StrategicWaterKind.River or StrategicWaterKind.SmallRiver or StrategicWaterKind.Delta))
                continue;
            var width = kind == StrategicWaterKind.SmallRiver ? 1 : Math.Max(1, settings.RiverWidth);
            var endpoints = new List<GridCoord>();
            foreach (var offset in new[]
                     {
                         (X: 0, Z: -1), (X: 1, Z: 0), (X: 0, Z: 1), (X: -1, Z: 0)
                     })
            {
                var side = offset switch
                {
                    (0, -1) => SettlementWaterSides.North,
                    (1, 0) => SettlementWaterSides.East,
                    (0, 1) => SettlementWaterSides.South,
                    _ => SettlementWaterSides.West
                };
                // The Java generator queries WORLD.WATER outside the selected
                // 3x3 CapitolArea too.  Persisted directional connectivity keeps
                // rivers entering/leaving the settlement at the same world edge.
                if (!sample.RiverConnections.HasFlag(side) && !Rivery(x + offset.X, z + offset.Z))
                    continue;
                endpoints.Add(new GridCoord(center.X + offset.X * quadWidth / 2,
                    center.Z + offset.Z * quadHeight / 2));
            }
            if (endpoints.Count == 0)
                endpoints.Add(center);
            var hub = endpoints.Count == 1 ? center : new GridCoord(
                (int)endpoints.Average(point => point.X), (int)endpoints.Average(point => point.Z));
            foreach (var endpoint in endpoints)
                PaintWaterLine(world, endpoint, hub, width,
                    profile.Seed ^ (x * 7919 + z * 104729));
        }
    }

    private static void PaintLakeDisc(WorldGridData world, GridCoord center, int radius)
    {
        // GeneratorLake.sink is a circle written through a four-tile Polymap.
        // Quantising the test to that same four-tile lattice preserves its source
        // outline without inventing per-pixel noise.
        const int sample = 4;
        var radiusSquared = radius * radius;
        for (var z = center.Z - radius; z < center.Z + radius; z++)
        for (var x = center.X - radius; x < center.X + radius; x++)
        {
            var qx = (x / sample) * sample + sample / 2;
            var qz = (z / sample) * sample + sample / 2;
            var dx = qx - center.X; var dz = qz - center.Z;
            if (dx * dx + dz * dz >= radiusSquared) continue;
            var cell = new GridCoord(x, z);
            if (world.IsInside(cell) && world.Elevation(cell) / 255.0 < 0.8)
                world.SetTerrain(cell, GroundKind.FreshWater, world.Elevation(cell), 0, 15);
        }
    }

    private static void PaintWaterLine(
        WorldGridData world, GridCoord start, GridCoord end, int width, int seed = 0)
    {
        var steps = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Z - start.Z));
        for (var step = 0; step <= steps; step++)
        {
            var t = step / (double)Math.Max(1, steps);
            var bend = (ValueNoise(step, seed & 255, seed ^ 0x5123, 19) - 0.5) * 9 *
                       Math.Sin(t * Math.PI);
            var dx = end.X - start.X; var dz = end.Z - start.Z;
            var length = Math.Max(1.0, Math.Sqrt(dx * dx + dz * dz));
            PaintWaterDisc(world, new GridCoord(
                (int)Math.Round(start.X + dx * t - dz / length * bend),
                (int)Math.Round(start.Z + dz * t + dx / length * bend)), width, seed ^ step);
        }
    }

    private static void PaintWaterDisc(WorldGridData world, GridCoord center, int radius, int seed)
    {
        for (var dz = -radius; dz <= radius; dz++)
        for (var dx = -radius; dx <= radius; dx++)
        {
            var cell = new GridCoord(center.X + dx, center.Z + dz);
            if (!world.IsInside(cell) || dx * dx + dz * dz > radius * radius *
                (0.82 + Noise(cell.X, cell.Z, seed) * 0.28)) continue;
            world.SetTerrain(cell, GroundKind.FreshWater, world.Elevation(cell), 0, 15);
        }
    }

    private static void GenerateRiver(WorldGridData world,
        SettlementWaterSides sides, int width, int seed)
    {
        var endpoints = Enum.GetValues<SettlementWaterSides>().Where(side => side != SettlementWaterSides.None &&
            sides.HasFlag(side)).Select(side => WaterEndpoint(world, side, seed)).ToList();
        if (endpoints.Count == 0) return;
        if (endpoints.Count == 1) endpoints.Add(new GridCoord(world.Width / 2, world.Height / 2));
        var hub = new GridCoord((int)endpoints.Average(point => point.X), (int)endpoints.Average(point => point.Z));
        foreach (var endpoint in endpoints) PaintRiverBranch(world, endpoint, hub, width, seed);
    }

    private static GridCoord WaterEndpoint(WorldGridData world, SettlementWaterSides side, int seed)
    {
        var offset = (int)(Noise((int)side, 0, seed) * 0.5 * world.Width + world.Width * 0.25);
        return side switch
        {
            SettlementWaterSides.North => new GridCoord(offset, 0),
            SettlementWaterSides.South => new GridCoord(offset, world.Height - 1),
            SettlementWaterSides.West => new GridCoord(0, offset),
            _ => new GridCoord(world.Width - 1, offset)
        };
    }

    private static void PaintRiverBranch(
        WorldGridData world, GridCoord start, GridCoord end, int width, int seed)
    {
        var steps = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Z - start.Z));
        for (var step = 0; step <= steps; step++)
        {
            var t = step / (double)Math.Max(1, steps);
            var bend = (Noise(step / 12, 0, seed) - 0.5) * Math.Min(world.Width, world.Height) * 0.045;
            var x = (int)Math.Round(start.X + (end.X - start.X) * t + bend * Math.Sin(t * Math.PI));
            var z = (int)Math.Round(start.Z + (end.Z - start.Z) * t + bend * Math.Cos(t * Math.PI));
            for (var dz = -width; dz <= width; dz++)
            for (var dx = -width; dx <= width; dx++)
            {
                var cell = new GridCoord(x + dx, z + dz);
                if (!world.IsInside(cell) || dx * dx + dz * dz > width * width) continue;
                world.SetTerrain(cell, GroundKind.FreshWater, world.Elevation(cell), 0, 15);
            }
        }
    }

    private static void GenerateMinerals(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        for (var type = 0; type < profile.Minables.Count; type++)
        {
            var rule = profile.Minables[type];
            if (!rule.OnEveryMap && !profile.IncludeRareMinables) continue;
            var candidates = new List<(GridCoord Cell, double Affinity)>();
            var affinityTotal = 0.0;
            // GeneratorMinerals first ranks coarse Polymap areas by terrain affinity.
            // A 16-tile lattice is the matching representation in the dense C# grid.
            for (var z = 8; z < world.Height - 8; z += 16)
            for (var x = 8; x < world.Width - 8; x += 16)
            {
                var cell = new GridCoord(x, z);
                var affinity = Affinity(rule, world.Ground(cell), world.Moisture(cell));
                if (affinity <= 0) continue;
                affinityTotal += affinity;
                candidates.Add((cell, affinity));
            }
            if (candidates.Count == 0) continue;
            candidates.Sort((a, b) => MineralRank(b, rule, type, profile.Seed)
                .CompareTo(MineralRank(a, rule, type, profile.Seed)));
            var meanAffinity = affinityTotal / candidates.Count;
            var remaining = Math.Max(1, (int)Math.Round(
                (38 + 2000 * meanAffinity) * settings.MineralsAmount));
            foreach (var candidate in candidates)
            {
                if (remaining <= 0) break;
                var variation = 0.5 + Noise(candidate.Cell.X, candidate.Cell.Z,
                    profile.Seed ^ (type * 7919)) * 0.5;
                var requested = Math.Min(remaining, Math.Max(6, (int)(remaining * variation)));
                remaining -= GrowMineralDeposit(world, candidate.Cell, type, rule, requested,
                    profile.Seed ^ (type * 104729));
            }
        }
    }

    private static double MineralRank(
        (GridCoord Cell, double Affinity) candidate, MinableRule rule, int type, int seed) =>
        candidate.Affinity + Fractal(candidate.Cell.X + type * 977,
            candidate.Cell.Z - type * 613, seed ^ (type * 7919), 3) * 0.12;

    private static int GrowMineralDeposit(
        WorldGridData world, GridCoord start, int type, MinableRule rule, int requested, int seed)
    {
        if (!CanPlaceMineral(world, start, rule)) return 0;
        var queue = new PriorityQueue<(GridCoord Cell, int Distance), double>();
        var visited = new HashSet<GridCoord>();
        queue.Enqueue((start, 0), 0);
        var placed = new List<GridCoord>();
        var normal = Fractal(start.X, start.Z, seed ^ 0x517d, 3);
        while (queue.Count > 0 && placed.Count < requested)
        {
            var (cell, distance) = queue.Dequeue();
            if (!visited.Add(cell) || !CanPlaceMineral(world, cell, rule)) continue;
            placed.Add(cell);
            var richness = 0.5 + Fractal(cell.X, cell.Z, seed ^ 0x2d31, 4) * 0.75;
            world.SetMineral(cell, type, (int)Math.Round(richness * 42));
            world.SetFertility(cell, world.Fertility(cell) +
                (int)Math.Round(rule.FertilityIncrease * 15));
            foreach (var direction in GridCoord.AllDirections)
            {
                var next = cell + direction;
                if (!world.IsInside(next) || visited.Contains(next)) continue;
                var heightCost = Math.Abs(Fractal(next.X, next.Z, seed ^ 0x517d, 3) - normal);
                var cost = heightCost + (distance + 1) / 64.0;
                queue.Enqueue((next, distance + 1), cost);
            }
        }
        // GeneratorMinerals.blurEdges fades the outer four tiles of every deposit.
        foreach (var cell in placed)
        {
            var edge = GridCoord.AllDirections.Count(direction =>
                !world.IsInside(cell + direction) || world.MineralType(cell + direction) != type);
            if (edge > 0) world.SetMineral(cell, type,
                Math.Max(1, world.MineralAmount(cell) * Math.Max(1, 4 - edge) / 4));
        }
        return placed.Count;
    }

    private static bool CanPlaceMineral(WorldGridData world, GridCoord cell, MinableRule rule) =>
        world.IsInside(cell) && !world.Has(cell, TileFlags.DeepWater) &&
        world.MineralAmount(cell) == 0 && Affinity(rule, world.Ground(cell), world.Moisture(cell)) > 0;

    private static void FinishWater(WorldGridData world)
    {
        var distanceToLand = new byte[world.Width * world.Height];
        Array.Fill(distanceToLand, byte.MaxValue);
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
            if (!world.Has(new GridCoord(x, z), TileFlags.Water)) distanceToLand[z * world.Width + x] = 0;
        DistancePass(distanceToLand, world.Width, world.Height);
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            if (!world.Has(cell, TileFlags.Water)) continue;
            var distance = distanceToLand[z * world.Width + x];
            if (distance < 4)
                world.SetTerrain(cell, GroundKind.Soil, world.Elevation(cell),
                    world.Fertility(cell), world.Moisture(cell));
            else
                world.SetDeepWater(cell, distance * Math.Pow(world.Elevation(cell) / 255.0, 1.7) > 4);
        }

        var distanceToWater = new byte[world.Width * world.Height];
        Array.Fill(distanceToWater, byte.MaxValue);
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            if (world.Has(cell, TileFlags.Water)) distanceToWater[z * world.Width + x] = 0;
        }
        DistancePass(distanceToWater, world.Width, world.Height);
        for (var z = world.Height - 1; z >= 0; z--)
        for (var x = world.Width - 1; x >= 0; x--)
            if (distanceToWater[z * world.Width + x] <= 8)
                world.Set(new GridCoord(x, z), TileFlags.GroundWater, true);
    }

    private static void DistancePass(byte[] distance, int width, int height)
    {
        for (var z = 0; z < height; z++)
        for (var x = 0; x < width; x++)
        {
            var i = z * width + x;
            if (x > 0) distance[i] = Math.Min(distance[i], (byte)Math.Min(255, distance[i - 1] + 1));
            if (z > 0) distance[i] = Math.Min(distance[i], (byte)Math.Min(255, distance[i - width] + 1));
        }
        for (var z = height - 1; z >= 0; z--)
        for (var x = width - 1; x >= 0; x--)
        {
            var i = z * width + x;
            if (x + 1 < width) distance[i] = Math.Min(distance[i], (byte)Math.Min(255, distance[i + 1] + 1));
            if (z + 1 < height) distance[i] = Math.Min(distance[i], (byte)Math.Min(255, distance[i + width] + 1));
        }
    }

    private static void GenerateOcean(WorldGridData world, SettlementGenerationProfile profile)
    {
        if (profile.WorldTiles is not null)
        {
            GenerateMappedOcean(world, profile);
            return;
        }
        if (profile.OceanSides == SettlementOceanSides.None) return;
        // GeneratorOcean.MARGIN = QUAD_SIZE/2.2; fallback for old saves without the 3x3 source matrix.
        var margin = Math.Max(12, (int)(Math.Min(world.Width, world.Height) / 3.0 / 2.2));
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var edge = int.MaxValue;
            if (profile.OceanSides.HasFlag(SettlementOceanSides.North)) edge = Math.Min(edge, z);
            if (profile.OceanSides.HasFlag(SettlementOceanSides.South)) edge = Math.Min(edge, world.Height - 1 - z);
            if (profile.OceanSides.HasFlag(SettlementOceanSides.West)) edge = Math.Min(edge, x);
            if (profile.OceanSides.HasFlag(SettlementOceanSides.East)) edge = Math.Min(edge, world.Width - 1 - x);
            var height = world.Elevation(new GridCoord(x, z)) / 255.0;
            var coastValue = 0.8 * edge + margin * height * height * height;
            coastValue += (Noise(x / 8, z / 8, profile.Seed ^ 0x715f) - 0.5) * 6;
            if (coastValue >= margin) continue;
            var cell = new GridCoord(x, z);
            var deep = coastValue <= margin * 4.5 / 5.0;
            world.SetTerrain(cell, GroundKind.SaltWater, world.Elevation(cell), 0, 15);
            world.SetDeepWater(cell, deep);
            world.Set(cell, TileFlags.SaltWater, true);
            world.Set(cell, TileFlags.GroundWater, true);
        }
        GenerateBeachBand(world, profile, margin);
    }

    private static void GenerateMappedOcean(WorldGridData world, SettlementGenerationProfile profile)
    {
        var dimension = profile.WorldTileDimension;
        var quadWidth = world.Width / dimension;
        var quadHeight = world.Height / dimension;
        var margin = Math.Max(12, Math.Min(quadWidth, quadHeight) / 2.2);
        var distance = new double[world.Width * world.Height];
        Array.Fill(distance, double.PositiveInfinity);
        var queue = new PriorityQueue<GridCoord, double>();

        bool Ocean(int qx, int qz) => (uint)qx < dimension && (uint)qz < dimension &&
            profile.WorldTiles![qx + qz * dimension].Water is
                StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean;
        void Seed(int x, int z)
        {
            x = Math.Clamp(x, 0, world.Width - 1); z = Math.Clamp(z, 0, world.Height - 1);
            var index = z * world.Width + x;
            if (distance[index] <= 0) return;
            distance[index] = 0;
            queue.Enqueue(new GridCoord(x, z), 0);
        }

        // GeneratorOcean seeds every ocean world-tile centre and every connected
        // cardinal edge, then floods by radius and the shared height map.
        for (var qz = 0; qz < dimension; qz++)
        for (var qx = 0; qx < dimension; qx++)
        {
            if (!Ocean(qx, qz)) continue;
            var cx = qx * quadWidth + quadWidth / 2;
            var cz = qz * quadHeight + quadHeight / 2;
            var connections = profile.WorldTiles![qx + qz * dimension].OceanConnections;
            Seed(cx, cz);
            if (connections.HasFlag(SettlementOceanSides.North) || Ocean(qx, qz - 1))
                Seed(cx, qz * quadHeight);
            if (connections.HasFlag(SettlementOceanSides.East) || Ocean(qx + 1, qz))
                Seed((qx + 1) * quadWidth - 1, cz);
            if (connections.HasFlag(SettlementOceanSides.South) || Ocean(qx, qz + 1))
                Seed(cx, (qz + 1) * quadHeight - 1);
            if (connections.HasFlag(SettlementOceanSides.West) || Ocean(qx - 1, qz))
                Seed(qx * quadWidth, cz);
        }

        while (queue.TryDequeue(out var cell, out var value))
        {
            var index = cell.Z * world.Width + cell.X;
            if (value > distance[index] || value >= margin + 15) continue;
            if (value < margin)
            {
                world.SetTerrain(cell, GroundKind.SaltWater, world.Elevation(cell), 0, 15);
                world.SetDeepWater(cell, value <= margin * 4.5 / 5.0);
                world.Set(cell, TileFlags.SaltWater | TileFlags.GroundWater, true);
            }
            foreach (var direction in GridCoord.AllDirections)
            {
                var next = cell + direction;
                if (!world.IsInside(next)) continue;
                var radiusStep = direction.X == 0 || direction.Z == 0 ? 1.0 : Math.Sqrt(2);
                var height = world.Elevation(next) / 255.0;
                var nextValue = value + 0.8 * radiusStep + margin * height * height * height * 0.08;
                var nextIndex = next.Z * world.Width + next.X;
                if (nextValue >= distance[nextIndex]) continue;
                distance[nextIndex] = nextValue;
                queue.Enqueue(next, nextValue);
            }
        }
        GenerateBeachBand(world, profile, (int)margin);
    }

    private static void GenerateBeachBand(
        WorldGridData world, SettlementGenerationProfile profile, int margin)
    {
        const int beachWidth = 15;
        var saltDistance = new byte[world.Width * world.Height];
        Array.Fill(saltDistance, byte.MaxValue);
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
            if (world.Has(new GridCoord(x, z), TileFlags.SaltWater))
                saltDistance[z * world.Width + x] = 0;
        DistancePass(saltDistance, world.Width, world.Height);
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            if (world.Has(cell, TileFlags.Water | TileFlags.Mountain)) continue;
            var nearestSalt = saltDistance[z * world.Width + x];
            if (nearestSalt > beachWidth) continue;
            var strength = 1 - nearestSalt / (double)beachWidth;
            world.SetFertility(cell, world.Fertility(cell) - (int)Math.Round(strength * 3));
            if (world.Fertility(cell) < 2 || Noise(x, z, profile.Seed ^ margin) < strength)
                world.SetTerrain(cell, GroundKind.Sand, world.Elevation(cell),
                    world.Fertility(cell), world.Moisture(cell));
        }
    }

    private static void GenerateLakeExtra(WorldGridData world, SettlementGenerationProfile profile)
    {
        var water = 0;
        for (var z = 0; z < world.Height && water <= 100; z++)
        for (var x = 0; x < world.Width && water <= 100; x++)
            if (world.Has(new GridCoord(x, z), TileFlags.Water | TileFlags.GroundWater)) water++;
        if (water > 100) return;
        const int clearance = 48;
        var candidates = new List<GridCoord>();
        for (var z = clearance; z < world.Height - clearance; z += 8)
        for (var x = clearance; x < world.Width - clearance; x += 8)
        {
            var cell = new GridCoord(x, z);
            if (world.Has(cell, TileFlags.Water | TileFlags.Mountain)) continue;
            candidates.Add(cell);
        }
        if (candidates.Count == 0) return;
        var center = candidates[(int)(Noise(17, 31, profile.Seed) * candidates.Count) % candidates.Count];
        const int radius = 12;
        for (var dz = -radius; dz <= radius; dz++)
        for (var dx = -radius; dx <= radius; dx++)
        {
            if (dx * dx + dz * dz >= radius * radius) continue;
            var cell = new GridCoord(center.X + dx, center.Z + dz);
            world.SetTerrain(cell, GroundKind.FreshWater, world.Elevation(cell), 0, 15);
            world.Set(cell, TileFlags.GroundWater, true);
        }
    }

    private static void GenerateFish(WorldGridData world, SettlementGenerationProfile profile)
    {
        var visited = new bool[world.Width * world.Height];
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            var index = z * world.Width + x;
            if (visited[index] || !world.Has(cell, TileFlags.DeepWater)) continue;
            var component = FloodWaterComponent(world, cell, visited);
            var weighted = component.Sum(value => world.Has(value, TileFlags.SaltWater) ? 1.0 : 0.5);
            var spots = Math.Min(component.Count, weighted / 200.0);
            if (spots <= 0) continue;
            var spacing = component.Count / spots;
            var nextSpot = Noise(x, z, profile.Seed ^ 0x63d5) * spacing;
            var amount = Math.Clamp(30.0 / spacing, 0, 15);
            for (var i = 0; i < component.Count; i++)
            {
                var value = (int)amount;
                if (Noise(component[i].X, component[i].Z, profile.Seed ^ 0x2f91) < amount - value) value++;
                var spot = i >= nextSpot;
                if (spot) nextSpot += Math.Max(1, Noise(i, component.Count, profile.Seed) * spacing);
                world.SetFish(component[i], value, spot);
            }
        }
    }

    private static List<GridCoord> FloodWaterComponent(
        WorldGridData world, GridCoord start, bool[] visited)
    {
        var result = new List<GridCoord>();
        var queue = new Queue<GridCoord>();
        queue.Enqueue(start);
        visited[start.Z * world.Width + start.X] = true;
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            result.Add(cell);
            foreach (var direction in GridCoord.Cardinal)
            {
                var next = cell + direction;
                if (!world.IsInside(next)) continue;
                var index = next.Z * world.Width + next.X;
                if (visited[index] || !world.Has(next, TileFlags.DeepWater)) continue;
                visited[index] = true;
                queue.Enqueue(next);
            }
        }
        return result;
    }

    private static void FinishGroundAndFertility(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            if (world.Ground(cell) != GroundKind.Soil) continue;
            var mappedForest = SampleWorld(profile, x, z, world.Width, world.Height,
                sample => sample.Forest, 0.67);
            if (world.Moisture(cell) >= 11)
                world.SetTerrain(cell, GroundKind.Wet, world.Elevation(cell), world.Fertility(cell), world.Moisture(cell));
            else if (world.Fertility(cell) >= 8 &&
                     Fractal(x + 101, z - 73, profile.Seed ^ 0x57c9, 4) >
                     1 - Math.Clamp(settings.ForestAmount *
                         (profile.WorldTiles is null ? 0.67 : 0.25 + mappedForest * 1.25), 0.05, 0.95))
            {
                world.SetTerrain(cell, GroundKind.Forest, world.Elevation(cell), world.Fertility(cell), world.Moisture(cell));
                world.SetVegetation(cell, (int)Math.Round(world.Fertility(cell) * settings.ForestDensity));
            }
            else
            {
                var groundMap = Fractal(x + 311, z - 887, profile.Seed ^ 0x6711, 4);
                if (groundMap < 0.25)
                {
                    var worst = profile.Climate.Equals("HOT", StringComparison.OrdinalIgnoreCase)
                        ? GroundKind.Sand : GroundKind.Infertile;
                    world.SetTerrain(cell, worst, world.Elevation(cell),
                        Math.Max(0, world.Fertility(cell) - 10), world.Moisture(cell));
                }
                else if (groundMap < 0.5)
                    world.SetTerrain(cell, GroundKind.Pasture, world.Elevation(cell),
                        Math.Max(0, world.Fertility(cell) - 4), world.Moisture(cell));
            }
        }
    }

    private static void GenerateGrowth(WorldGridData world, SettlementGenerationProfile profile)
    {
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var cell = new GridCoord(x, z);
            if (world.Has(cell, TileFlags.Water | TileFlags.Mountain)) continue;
            var field = Fractal(x + 233, z - 719, profile.Seed ^ 0x4a11, 3);
            // GeneratorGrowth maps forest height ranges to bush/tree/bush. The dense
            // terrain stores their shared density; renderers may select the sprite band.
            var amount = world.Ground(cell) switch
            {
                GroundKind.Forest when field < 0.10 => (int)Math.Round(field / 0.10 * 6),
                GroundKind.Forest when field < 0.25 => 7 + (int)Math.Round((field - 0.10) / 0.15 * 8),
                GroundKind.Forest when field < 0.35 => 6 + (int)Math.Round((field - 0.25) / 0.10 * 5),
                GroundKind.Pasture when field > 0.70 => (int)Math.Round((field - 0.70) / 0.20 * 8),
                GroundKind.Soil when field > 0.65 => (int)Math.Round((field - 0.65) / 0.20 * 5),
                _ => 0
            };
            if (amount > world.VegetationAmount(cell)) world.SetVegetation(cell, amount);
        }
    }

    private static void GenerateEdibles(
        WorldGridData world, SettlementGenerationProfile profile, SettlementGeneratorSettings settings)
    {
        if (profile.Growables.Count == 0 || profile.EdiblesAmount <= 0) return;
        var spots = new List<GridCoord>();
        var suitabilityTotal = 0.0;
        var suitableTiles = 0;
        // Original Polymap supplies one ranked starting point per local area.
        for (var z = 8; z < world.Height - 8; z += 16)
        for (var x = 8; x < world.Width - 8; x += 16)
        {
            var cell = new GridCoord(x, z);
            var suitability = EdibleSuitability(world, cell);
            if (suitability < 0.1) continue;
            suitabilityTotal += suitability;
            suitableTiles++;
            spots.Add(cell);
        }
        if (spots.Count == 0) return;
        spots.Sort((a, b) => Noise(b.X, b.Z, profile.Seed ^ 0x6d31)
            .CompareTo(Noise(a.X, a.Z, profile.Seed ^ 0x6d31)));
        var average = suitabilityTotal / suitableTiles;
        var commonBudget = Math.Sqrt(average) * profile.EdiblesAmount *
                           settings.EdiblesAmount * 4000 / profile.Growables.Count;
        var remaining = profile.Growables.Select(growable =>
        {
            var climate = growable.ClimateBonus.TryGetValue(profile.Climate, out var bonus) ? bonus : 0;
            return (int)Math.Round(Math.Sqrt(Math.Max(0, growable.GrowthValue)) * climate * commonBudget);
        }).ToArray();
        var offset = (int)(Noise(7, 19, profile.Seed) * profile.Growables.Count);
        foreach (var spot in spots)
        {
            var selected = -1;
            for (var i = 0; i < remaining.Length; i++)
            {
                var type = (offset + i) % remaining.Length;
                if (remaining[type] <= 0) continue;
                selected = type;
                offset = type + 1;
                break;
            }
            if (selected < 0) break;
            remaining[selected] -= GrowEdiblePatch(world, spot, selected,
                remaining[selected], profile.Seed ^ (selected * 3571 + 0x1b7d));
        }
    }

    private static int GrowEdiblePatch(
        WorldGridData world, GridCoord start, int type, int budget, int seed)
    {
        var baseSuitability = EdibleSuitability(world, start);
        if (baseSuitability < 0) return 0;
        var radius = 1.0 / (2 + Noise(start.X, start.Z, seed) * 10);
        var queue = new PriorityQueue<(GridCoord Cell, double Value), double>();
        var visited = new HashSet<GridCoord>();
        queue.Enqueue((start, 1), -1);
        var total = 0;
        while (queue.Count > 0 && total < budget)
        {
            var (cell, value) = queue.Dequeue();
            if (!visited.Add(cell)) continue;
            var suitability = EdibleSuitability(world, cell);
            if (suitability < 0) continue;
            var amount = Math.Clamp((int)Math.Round(value * suitability * 8), 1, 15);
            amount = Math.Min(amount, budget - total);
            world.SetGrowable(cell, type, amount);
            world.SetVegetation(cell, Math.Max(world.VegetationAmount(cell), amount));
            world.SetFertility(cell, world.Fertility(cell) + (int)Math.Round(0.5 + amount / 8.0));
            total += amount;
            var decay = (0.5 + Math.Abs(baseSuitability - suitability) * 32) * radius;
            foreach (var direction in GridCoord.AllDirections)
            {
                var next = cell + direction;
                var nextValue = value - decay;
                if (nextValue > 0 && world.IsInside(next) && !visited.Contains(next))
                    queue.Enqueue((next, nextValue), -nextValue);
            }
        }
        return total;
    }

    private static double EdibleSuitability(WorldGridData world, GridCoord cell)
    {
        if (!world.IsInside(cell) || world.Has(cell,
                TileFlags.Water | TileFlags.Mountain | TileFlags.Furniture) ||
            world.MineralAmount(cell) > 0 || world.GrowableAmount(cell) > 0) return -1;
        var moisture = world.Moisture(cell) / 15.0;
        return moisture > 0.2 ? (moisture - 0.2) / 0.8 : -1;
    }

    /// <summary>
    /// GeneratorRoads creates the approach roads during map generation. They are
    /// finished terrain, never construction orders for the starting population.
    /// </summary>
    private static void GenerateRoads(WorldGridData world, SettlementGenerationProfile profile)
    {
        if (profile.WorldTiles is null || profile.WorldTiles.All(sample => !sample.Road)) return;
        var dimension = profile.WorldTileDimension;
        var quadWidth = world.Width / dimension;
        var quadHeight = world.Height / dimension;
        var network = new List<GridCoord>();
        var boundary = new List<GridCoord>();
        bool Road(int qx, int qz) => (uint)qx < dimension && (uint)qz < dimension &&
                                     profile.WorldTiles![qx + qz * dimension].Road;

        for (var qz = 0; qz < dimension; qz++)
        for (var qx = 0; qx < dimension; qx++)
        {
            if (!Road(qx, qz)) continue;
            var center = new GridCoord(qx * quadWidth + quadWidth / 2,
                qz * quadHeight + quadHeight / 2);
            var connections = profile.WorldTiles![qx + qz * dimension].RoadConnections;
            network.Add(center);
            var links = new List<GridCoord>();
            if (connections.HasFlag(SettlementWaterSides.North) || Road(qx, qz - 1))
                links.Add(new GridCoord(center.X, qz * quadHeight));
            if (connections.HasFlag(SettlementWaterSides.East) || Road(qx + 1, qz))
                links.Add(new GridCoord((qx + 1) * quadWidth - 1, center.Z));
            if (connections.HasFlag(SettlementWaterSides.South) || Road(qx, qz + 1))
                links.Add(new GridCoord(center.X, (qz + 1) * quadHeight - 1));
            if (connections.HasFlag(SettlementWaterSides.West) || Road(qx - 1, qz))
                links.Add(new GridCoord(qx * quadWidth, center.Z));
            foreach (var endpoint in links.Distinct())
            {
                PaintGeneratedRoadLine(world, center, endpoint,
                    profile.Seed ^ qx * 7919 ^ qz * 104729);
                if (endpoint.X == 0 || endpoint.Z == 0 || endpoint.X == world.Width - 1 ||
                    endpoint.Z == world.Height - 1) boundary.Add(endpoint);
            }
        }

        // Old or reduced world-road data can contain only an interior capital tile.
        // GeneratorRoads.adjust still guarantees a usable entry; connect its nearest edge.
        if (boundary.Count == 0 && network.Count > 0)
        {
            var start = network.OrderBy(cell => Math.Min(Math.Min(cell.X, cell.Z),
                Math.Min(world.Width - 1 - cell.X, world.Height - 1 - cell.Z))).First();
            var distances = new[]
            {
                (start.Z, new GridCoord(start.X, 0)),
                (world.Width - 1 - start.X, new GridCoord(world.Width - 1, start.Z)),
                (world.Height - 1 - start.Z, new GridCoord(start.X, world.Height - 1)),
                (start.X, new GridCoord(0, start.Z))
            };
            PaintGeneratedRoadLine(world, start, distances.OrderBy(value => value.Item1).First().Item2,
                profile.Seed ^ 0x751d);
        }
    }

    private static void PaintGeneratedRoadLine(
        WorldGridData world, GridCoord start, GridCoord end, int seed)
    {
        var steps = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Z - start.Z));
        for (var step = 0; step <= steps; step++)
        {
            var t = step / (double)Math.Max(1, steps);
            var dx = end.X - start.X; var dz = end.Z - start.Z;
            var length = Math.Max(1.0, Math.Sqrt(dx * dx + dz * dz));
            var bend = (ValueNoise(step, seed & 127, seed, 24) - 0.5) * 7 * Math.Sin(t * Math.PI);
            var center = new GridCoord(
                (int)Math.Round(start.X + dx * t - dz / length * bend),
                (int)Math.Round(start.Z + dz * t + dx / length * bend));
            foreach (var offset in GridCoord.AllDirections.Prepend(new GridCoord(0, 0)))
            {
                var cell = center + offset;
                if (!world.IsInside(cell)) continue;
                if (world.Has(cell, TileFlags.Mountain))
                {
                    world.SetCave(cell, false);
                    world.SetTerrain(cell, GroundKind.Soil, world.Elevation(cell),
                        world.Fertility(cell), world.Moisture(cell));
                }
                world.SetVegetation(cell, 0);
                world.Set(cell, TileFlags.ClearableTerrain | TileFlags.EasilyClearableTerrain, false);
                world.Set(cell, TileFlags.Road, true);
            }
        }
    }

    private static void GenerateFoundation(WorldGridData world, SettlementGenerationProfile profile)
    {
        for (var z = 0; z < world.Height; z++)
        for (var x = 0; x < world.Width; x++)
        {
            var d = Fractal(x + 1291, z - 1877, profile.Seed ^ 0x7d41, 4);
            d = d < 0.5 ? d * d : Math.Sqrt(d);
            world.SetFoundation(new GridCoord(x, z), (int)Math.Round(d * 3));
        }
    }

    private static double Affinity(MinableRule rule, GroundKind ground, int moisture)
    {
        var key = ground switch
        {
            GroundKind.Mountain => "MOUNTAIN",
            GroundKind.Forest => "FOREST",
            GroundKind.Wet => "WET",
            _ when moisture >= 10 => "WET",
            _ => "NONE"
        };
        return rule.Terrain.TryGetValue(key, out var value) ? Math.Max(0, value) : 0;
    }

    private static int ToByte(double value) => (int)Math.Round(Math.Clamp(value, 0, 1) * 255);
    private static int ToNibble(double value) => (int)Math.Round(Math.Clamp(value, 0, 1) * 15);

    /// <summary>Exact centre-weighted interpolation used by GeneratorFertilityInit.getBase.</summary>
    private static double SampleWorld(
        SettlementGenerationProfile profile, int x, int z, int width, int height,
        Func<SettlementWorldTileSample, double> selector, double fallback)
    {
        if (profile.WorldTiles is null ||
            profile.WorldTiles.Count != profile.WorldTileDimension * profile.WorldTileDimension)
            return fallback;
        var dimension = profile.WorldTileDimension;
        var quadWidth = Math.Max(1, width / dimension);
        var quadHeight = Math.Max(1, height / dimension);
        var qx = Math.Clamp(x / quadWidth, 0, dimension - 1);
        var qz = Math.Clamp(z / quadHeight, 0, dimension - 1);
        var dx = x % quadWidth - quadWidth / 2;
        var dz = z % quadHeight - quadHeight / 2;
        var nx = Math.Clamp(qx + Math.Sign(dx), 0, dimension - 1);
        var nz = Math.Clamp(qz + Math.Sign(dz), 0, dimension - 1);
        var tx = Math.Abs(dx) / (double)quadWidth;
        var tz = Math.Abs(dz) / (double)quadHeight;
        double At(int sx, int sz) => selector(profile.WorldTiles[sx + sz * dimension]);
        return Lerp(Lerp(At(qx, qz), At(nx, qz), tx),
            Lerp(At(qx, nz), At(nx, nz), tx), tz);
    }

    private static double Fractal(int x, int z, int seed, int octaves)
    {
        var sum = 0.0;
        var weight = 0.0;
        var amplitude = 1.0;
        var scale = 96;
        for (var octave = 0; octave < octaves; octave++)
        {
            sum += ValueNoise(x, z, seed + octave * 104729, scale) * amplitude;
            weight += amplitude;
            amplitude *= 0.5;
            scale = Math.Max(3, scale / 2);
        }
        return sum / weight;
    }

    private static double ValueNoise(int x, int z, int seed, int scale)
    {
        var x0 = FloorDiv(x, scale);
        var z0 = FloorDiv(z, scale);
        var tx = Smooth((x - x0 * scale) / (double)scale);
        var tz = Smooth((z - z0 * scale) / (double)scale);
        var a = Lerp(Noise(x0, z0, seed), Noise(x0 + 1, z0, seed), tx);
        var b = Lerp(Noise(x0, z0 + 1, seed), Noise(x0 + 1, z0 + 1, seed), tx);
        return Lerp(a, b, tz);
    }

    private static double Noise(int x, int z, int seed)
    {
        unchecked
        {
            uint value = (uint)(x * 0x1f123bb5 ^ z * 0x5f356495 ^ seed);
            value ^= value >> 16; value *= 0x7feb352d; value ^= value >> 15;
            value *= 0x846ca68b; value ^= value >> 16;
            return value / (double)uint.MaxValue;
        }
    }

    private static int FloorDiv(int value, int divisor) => value >= 0 ? value / divisor : -((-value + divisor - 1) / divisor);
    private static double Smooth(double value) => value * value * (3 - 2 * value);
    private static double Lerp(double a, double b, double value) => a + (b - a) * value;
}
