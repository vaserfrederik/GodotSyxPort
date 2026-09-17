using System;
using System.Collections.Generic;

namespace GodotSyxPort.World;

public enum StrategicWaterKind : byte { None, Lake, DeepLake, Ocean, DeepOcean, River, SmallRiver, Delta }
public enum StrategicTerrainEdit : byte
{
    Ground, Mountain, SinkMountain,
    Lake, DeepLake, Ocean, DeepOcean, River, SmallRiver, ClearWater,
    Forest, ClearForest, ColdClimate, TemperateClimate, HotClimate
}

public sealed record StrategicTerrainTemplate(string Name, int Width, int Height, byte[] Heights)
{
    public double Sample(int x, int y, int targetWidth, int targetHeight)
    {
        // WorldGenMapType.h(x,y,w,h): retain its four weighted source samples and
        // its world-dimension normalization instead of nearest-neighbour scaling.
        var xx = x / (double)targetWidth;
        var sourceX = Math.Clamp((int)(xx * Width), 0, Width - 1);
        xx -= (int)xx;
        var yy = y / (double)targetHeight;
        var sourceY = Math.Clamp((int)(yy * Height), 0, Height - 1);
        yy -= (int)yy;
        var area = 0.0;
        var result = 0.0;
        void Add(int sx, int sy, double weight)
        {
            if ((uint)sx >= (uint)Width || (uint)sy >= (uint)Height) return;
            result += weight * Heights[sx + sy * Width] / targetWidth;
            area += weight;
        }
        Add(sourceX, sourceY, (1 - xx) * (1 - yy));
        Add(sourceX + 1, sourceY, xx * (1 - yy));
        Add(sourceX, sourceY + 1, (1 - xx) * yy);
        Add(sourceX + 1, sourceY + 1, xx * yy);
        // WorldGenMapType.h() applies dd(): the template is a signed modifier
        // added to the independently generated HeightMap, not the final height.
        if (area <= 0) return 0;
        var sampled = result / area;
        return sampled < 0.5 ? -(0.5 - sampled) * 2.0 : (sampled - 0.5) * 2.0;
    }
}

public sealed record StrategicTerrainSnapshot(
    int Size,
    byte[] Height,
    byte[] Fertility,
    byte[] Forest,
    byte[] Moisture,
    byte[] Water,
    byte[] Mountain,
    byte[] MountainHeight,
    byte[] Climate);

/// <summary>
/// Dense 256x256 world terrain adapted from world.map.terrain generators. The original
/// rendering resources are replaced, while height shaping, 0.30 water line, component
/// oceans, large/small river counts, latitude climate and 0.30 forest coverage remain data.
/// </summary>
public sealed class StrategicTerrainRuntime
{
    public const double WaterLine = 0.30;
    public const int OceanComponentMinimum = 150;
    public const double ForestCoverage = 0.30;
    public const int LargeRiverReferenceCount = 10;
    public const int SmallRiverReferenceCount = 200;
    private readonly int _size;
    private readonly byte[] _height, _fertility, _forest, _moisture;
    private readonly StrategicWaterKind[] _water;
    private readonly bool[] _mountain;
    private readonly byte[] _mountainHeight;
    private readonly string[] _climate;

    public int Size => _size;
    public StrategicTerrainRuntime(int size)
    {
        _size = size;
        _height = new byte[size * size]; _fertility = new byte[size * size];
        _forest = new byte[size * size]; _moisture = new byte[size * size];
        _water = new StrategicWaterKind[size * size];
        _mountain = new bool[size * size]; _mountainHeight = new byte[size * size];
        _climate = new string[size * size];
    }

    public void Generate(int seed, StrategicTerrainTemplate? template = null, double latitude = 0.5)
    {
        GenerateHeight(seed, template);
        GenerateMountains();
        GenerateWater(seed);
        GenerateRivers(seed);
        GenerateMountainHeights();
        GenerateClimateAndFertility(seed, latitude);
        GenerateForest(seed);
    }

    public double Height(int x, int y) => InBounds(x, y) ? _height[Index(x, y)] / 255.0 : 0;
    public double Fertility(int x, int y) => InBounds(x, y) ? _fertility[Index(x, y)] / 255.0 : 0;
    public double Moisture(int x, int y) => InBounds(x, y) ? _moisture[Index(x, y)] / 255.0 : 0;
    public double Forest(int x, int y) => InBounds(x, y) ? _forest[Index(x, y)] / 255.0 : 0;
    public bool Mountain(int x, int y) => InBounds(x, y) && _mountain[Index(x, y)];
    public int MountainHeight(int x, int y) =>
        InBounds(x, y) ? _mountainHeight[Index(x, y)] : 0;
    public StrategicWaterKind Water(int x, int y) =>
        InBounds(x, y) ? _water[Index(x, y)] : StrategicWaterKind.None;
    public string Climate(int x, int y) =>
        InBounds(x, y) ? _climate[Index(x, y)] ?? "TEMPERATE" : "TEMPERATE";
    public bool IsWater(int x, int y) => Water(x, y) != StrategicWaterKind.None;

    public StrategicTerrainSnapshot Capture()
    {
        var water = new byte[_water.Length];
        var mountain = new byte[_mountain.Length];
        var climate = new byte[_climate.Length];
        for (var index = 0; index < _water.Length; index++)
        {
            water[index] = (byte)_water[index];
            mountain[index] = _mountain[index] ? (byte)1 : (byte)0;
            climate[index] = _climate[index] switch { "COLD" => 0, "HOT" => 2, _ => 1 };
        }
        return new StrategicTerrainSnapshot(
            _size,
            (byte[])_height.Clone(),
            (byte[])_fertility.Clone(),
            (byte[])_forest.Clone(),
            (byte[])_moisture.Clone(),
            water,
            mountain,
            (byte[])_mountainHeight.Clone(),
            climate);
    }

    public void Restore(StrategicTerrainSnapshot snapshot)
    {
        var expected = _size * _size;
        if (snapshot.Size != _size || snapshot.Height.Length != expected ||
            snapshot.Fertility.Length != expected || snapshot.Forest.Length != expected ||
            snapshot.Moisture.Length != expected || snapshot.Water.Length != expected ||
            snapshot.Mountain.Length != expected || snapshot.MountainHeight.Length != expected ||
            snapshot.Climate.Length != expected)
            throw new ArgumentException("Strategic terrain snapshot dimensions do not match the world.");
        Array.Copy(snapshot.Height, _height, expected);
        Array.Copy(snapshot.Fertility, _fertility, expected);
        Array.Copy(snapshot.Forest, _forest, expected);
        Array.Copy(snapshot.Moisture, _moisture, expected);
        Array.Copy(snapshot.MountainHeight, _mountainHeight, expected);
        for (var index = 0; index < expected; index++)
        {
            _water[index] = snapshot.Water[index] <= (byte)StrategicWaterKind.Delta
                ? (StrategicWaterKind)snapshot.Water[index] : StrategicWaterKind.None;
            _mountain[index] = snapshot.Mountain[index] != 0;
            _climate[index] = snapshot.Climate[index] switch
            {
                0 => "COLD", 2 => "HOT", _ => "TEMPERATE"
            };
        }
    }

    public bool ApplyEdit(StrategicTerrainEdit edit, int centerX, int centerY, int radius)
    {
        var changed = false;
        radius = Math.Clamp(radius, 1, 12);
        for (var y = centerY - radius + 1; y <= centerY + radius - 1; y++)
        for (var x = centerX - radius + 1; x <= centerX + radius - 1; x++)
        {
            if (!InBounds(x, y) || (x - centerX) * (x - centerX) +
                (y - centerY) * (y - centerY) >= radius * radius) continue;
            ApplyEditCell(edit, Index(x, y));
            changed = true;
        }
        if (changed) GenerateMountainHeights();
        return changed;
    }

    private void ApplyEditCell(StrategicTerrainEdit edit, int index)
    {
        switch (edit)
        {
            case StrategicTerrainEdit.Ground:
                _height[index] = (byte)Math.Max(_height[index], Byte(WaterLine + 0.08));
                _mountain[index] = false; _water[index] = StrategicWaterKind.None;
                break;
            case StrategicTerrainEdit.Mountain:
                _height[index] = (byte)Math.Max(_height[index], Byte(0.82));
                _mountain[index] = true; _water[index] = StrategicWaterKind.None;
                break;
            case StrategicTerrainEdit.SinkMountain:
                _height[index] = (byte)Math.Min(_height[index], Byte(0.62));
                _mountain[index] = false;
                break;
            case StrategicTerrainEdit.Lake: SetWater(index, StrategicWaterKind.Lake, 0.27); break;
            case StrategicTerrainEdit.DeepLake: SetWater(index, StrategicWaterKind.DeepLake, 0.18); break;
            case StrategicTerrainEdit.Ocean: SetWater(index, StrategicWaterKind.Ocean, 0.27); break;
            case StrategicTerrainEdit.DeepOcean: SetWater(index, StrategicWaterKind.DeepOcean, 0.18); break;
            case StrategicTerrainEdit.River: SetWater(index, StrategicWaterKind.River, 0.34); break;
            case StrategicTerrainEdit.SmallRiver: SetWater(index, StrategicWaterKind.SmallRiver, 0.34); break;
            case StrategicTerrainEdit.ClearWater:
                _water[index] = StrategicWaterKind.None;
                _height[index] = (byte)Math.Max(_height[index], Byte(WaterLine + 0.04));
                break;
            case StrategicTerrainEdit.Forest:
                if (_water[index] == StrategicWaterKind.None) _forest[index] = byte.MaxValue;
                break;
            case StrategicTerrainEdit.ClearForest: _forest[index] = 0; break;
            case StrategicTerrainEdit.ColdClimate: _climate[index] = "COLD"; break;
            case StrategicTerrainEdit.TemperateClimate: _climate[index] = "TEMPERATE"; break;
            case StrategicTerrainEdit.HotClimate: _climate[index] = "HOT"; break;
        }
    }

    private void SetWater(int index, StrategicWaterKind kind, double height)
    {
        _water[index] = kind;
        _height[index] = Byte(height);
        _mountain[index] = false;
        _forest[index] = 0;
    }

    private void GenerateHeight(int seed, StrategicTerrainTemplate? template)
    {
        // Java: new HeightMap(TWIDTH, THEIGHT, TWIDTH/8, 4). HeightMap sums
        // octave noise with divisors 1,2,3,4 and then normalizes the whole map.
        var generated = new double[_height.Length];
        var minimum = double.MaxValue;
        var maximum = double.MinValue;
        for (var y = 0; y < _size; y++)
        for (var x = 0; x < _size; x++)
        {
            var value = Noise(x, y, Math.Max(4, _size / 8), seed, 3) +
                        Noise(x, y, Math.Max(4, _size / 16), seed, 5) / 2.0 +
                        Noise(x, y, Math.Max(4, _size / 32), seed, 7) / 3.0 +
                        Noise(x, y, 4, seed, 11) / 4.0;
            generated[Index(x, y)] = value;
            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
        }
        var range = Math.Max(0.000001, maximum - minimum);
        for (var y = 0; y < _size; y++)
        for (var x = 0; x < _size; x++)
        {
            var value = (generated[Index(x, y)] - minimum) / range;
            if (template is not null)
                value += template.Sample(x, y, _size, _size);
            _height[Index(x, y)] = Byte(value);
        }
    }

    private void GenerateMountains()
    {
        for (var i = 0; i < _height.Length; i++) _mountain[i] = _height[i] / 255.0 >= 0.75;
    }

    /// <summary>
    /// WorldMountain.setHeight: mountain height is distance from the massif edge,
    /// capped at the original fifteen levels. This produces the layered ridges
    /// used by the Java renderer instead of one flat mountain colour.
    /// </summary>
    private void GenerateMountainHeights()
    {
        var queue = new Queue<int>();
        for (var y = 0; y < _size; y++)
        for (var x = 0; x < _size; x++)
        {
            var index = Index(x, y);
            if (!_mountain[index] || _water[index] != StrategicWaterKind.None)
            {
                _mountainHeight[index] = 0;
                queue.Enqueue(index);
            }
            else _mountainHeight[index] = 15;
        }
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            var x = cell % _size; var y = cell / _size;
            foreach (var next in Orthogonal(x, y))
            {
                if (!_mountain[next]) continue;
                var candidate = Math.Min(15, _mountainHeight[cell] + 1);
                if (candidate >= _mountainHeight[next]) continue;
                _mountainHeight[next] = (byte)candidate;
                queue.Enqueue(next);
            }
        }
    }

    private void GenerateWater(int seed)
    {
        var visited = new bool[_water.Length];
        for (var y = 0; y < _size; y++)
        for (var x = 0; x < _size; x++)
        {
            var index = Index(x, y);
            if (_mountain[index] || Height(x, y) >= WaterLine || visited[index]) continue;
            var component = FloodLow(x, y, visited);
            var oceanChance = Math.Clamp((component.Count - OceanComponentMinimum) / 2500.0, 0, 1);
            var ocean = Hash(component[0], seed, 31) < oceanChance;
            foreach (var cell in component)
            {
                var deep = _height[cell] / 255.0 < WaterLine * 0.75;
                _water[cell] = ocean
                    ? deep ? StrategicWaterKind.DeepOcean : StrategicWaterKind.Ocean
                    : deep ? StrategicWaterKind.DeepLake : StrategicWaterKind.Lake;
            }
        }
    }

    private List<int> FloodLow(int sx, int sy, bool[] visited)
    {
        var result = new List<int>(); var queue = new Queue<int>();
        var start = Index(sx, sy); visited[start] = true; queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue(); result.Add(cell);
            var x = cell % _size; var y = cell / _size;
            foreach (var next in Orthogonal(x, y))
                if (!visited[next] && !_mountain[next] && _height[next] / 255.0 < WaterLine)
                { visited[next] = true; queue.Enqueue(next); }
        }
        return result;
    }

    private void GenerateRivers(int seed)
    {
        var scale = _size * _size / (224.0 * 224.0);
        var large = (int)Math.Round(LargeRiverReferenceCount * scale);
        var small = (int)Math.Round(SmallRiverReferenceCount * scale);
        for (var i = 0; i < large; i++) TraceRiver(seed + i * 97, 16 + (int)(Hash(i, seed, 43) * (_size - 1)), false);
        for (var i = 0; i < small; i++) TraceRiver(seed + i * 31, 24, true);
    }

    private void TraceRiver(int seed, int maximumLength, bool small)
    {
        var start = (int)(Hash(seed, seed, 47) * _height.Length) % _height.Length;
        for (var probe = 0; probe < 96 && (!_mountain[start] || _water[start] != StrategicWaterKind.None); probe++)
            start = (start + 997) % _height.Length;
        var current = start;
        for (var length = 0; length < maximumLength; length++)
        {
            if (_water[current] is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
                StrategicWaterKind.Lake or StrategicWaterKind.DeepLake)
            { _water[current] = StrategicWaterKind.Delta; break; }
            _water[current] = small ? StrategicWaterKind.SmallRiver : StrategicWaterKind.River;
            var x = current % _size; var y = current / _size;
            var next = current; var score = double.MaxValue;
            foreach (var candidate in Orthogonal(x, y))
            {
                var jitter = Hash(candidate, seed, length) * 0.08;
                var candidateScore = _height[candidate] / 255.0 + jitter;
                if (candidateScore < score) { score = candidateScore; next = candidate; }
            }
            if (next == current) break;
            current = next;
        }
    }

    private void GenerateClimateAndFertility(int seed, double latitude)
    {
        var waterDistance = BuildWaterDistances(8);
        var equator = Math.Clamp(latitude, 0.05, 0.95);
        for (var y = 0; y < _size; y++)
        for (var x = 0; x < _size; x++)
        {
            var row = y / (_size - 1.0);
            var season = row <= equator ? row / equator : 1 - (row - equator) / (1 - equator);
            var climateBand = season * 0.8 + (1 - Noise(x, y, 48, seed, 59)) * 0.2;
            var index = Index(x, y);
            _climate[index] = row <= equator
                ? climateBand < 0.4 ? "COLD" : "TEMPERATE"
                : climateBand < 0.3 ? "HOT" : "TEMPERATE";
            var water = waterDistance[index];
            var moisture = 1 - water / 8.0;
            _moisture[index] = Byte(moisture);
            var value = Noise(x, y, 20, seed, 61) * 0.40 + moisture * 0.30 + season * 0.30;
            if (_mountain[index] || IsWater(x, y)) value *= 0.2;
            _fertility[index] = Byte(value);
        }
    }

    public double SettlementPattern(int x, int y, int seed) =>
        (Noise(x, y, 16, seed, 83) + Noise(x, y, 8, seed, 89) * 0.5 +
         Noise(x, y, 4, seed, 97) / 3.0) / (1.0 + 0.5 + 1.0 / 3.0);

    private void GenerateForest(int seed)
    {
        for (var y = 0; y < _size; y++)
        for (var x = 0; x < _size; x++)
        {
            var index = Index(x, y);
            if (IsWater(x, y) || Moisture(x, y) < 0.15) continue;
            // Direct shape of GeneratorForest.place(): fine noise is modulated by
            // large forest masses, nearby moisture and climate fertility.
            var value = Noise(x, y, 16, seed, 71) *
                        (0.35 + 0.65 * Noise(x, y, 64, seed, 73));
            value = Math.Min(0.8, value);
            if (_water[index] is StrategicWaterKind.River or StrategicWaterKind.SmallRiver)
                value += value * 0.05;
            var neighbourMoisture = 0.0;
            var neighbours = 0;
            foreach (var neighbour in AllNeighbours(x, y))
            {
                neighbourMoisture += _moisture[neighbour] / 255.0;
                neighbours++;
                if (_water[neighbour] is StrategicWaterKind.River or StrategicWaterKind.SmallRiver or
                    StrategicWaterKind.Lake or StrategicWaterKind.DeepLake) value += 0.01;
            }
            value += 0.2 * neighbourMoisture / Math.Max(1, neighbours);
            value *= 0.5 + Fertility(x, y) * 1.2;
            value = Math.Pow(Math.Max(0, value), 1.5);
            if (_mountain[index])
            {
                var mountainHeight = _mountainHeight[index];
                if (mountainHeight > 3) continue;
                value -= 0.3 * (1.0 - mountainHeight / 3.0);
            }
            var threshold = 1.0 - ForestCoverage;
            if (value > threshold)
                value = (value - threshold) / (1.0 - threshold);
            else if (value > 0.6)
                value = (value - 0.6) * 1.66 - (0.75 - Moisture(x, y));
            else value = 0;
            if (_mountain[index]) value *= 1.0 - _mountainHeight[index] / 3.0;
            _forest[index] = Byte(value);
        }
    }

    private int[] BuildWaterDistances(int maximum)
    {
        var distance = new int[_water.Length]; Array.Fill(distance, maximum);
        var queue = new Queue<int>();
        for (var i = 0; i < _water.Length; i++)
            if (_water[i] != StrategicWaterKind.None) { distance[i] = 0; queue.Enqueue(i); }
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            if (distance[cell] >= maximum) continue;
            foreach (var next in Orthogonal(cell % _size, cell / _size))
                if (distance[next] > distance[cell] + 1)
                { distance[next] = distance[cell] + 1; queue.Enqueue(next); }
        }
        return distance;
    }

    private double Noise(double x, double y, int scale, int seed, int salt)
    {
        var x0 = (int)Math.Floor(x / scale); var y0 = (int)Math.Floor(y / scale);
        var tx = Smooth(x / scale - x0); var ty = Smooth(y / scale - y0);
        var a = Hash(x0 + y0 * 4099, seed, salt);
        var b = Hash(x0 + 1 + y0 * 4099, seed, salt);
        var c = Hash(x0 + (y0 + 1) * 4099, seed, salt);
        var d = Hash(x0 + 1 + (y0 + 1) * 4099, seed, salt);
        return Lerp(Lerp(a, b, tx), Lerp(c, d, tx), ty);
    }

    private IEnumerable<int> Orthogonal(int x, int y)
    {
        if (x > 0) yield return Index(x - 1, y); if (x + 1 < _size) yield return Index(x + 1, y);
        if (y > 0) yield return Index(x, y - 1); if (y + 1 < _size) yield return Index(x, y + 1);
    }

    private IEnumerable<int> AllNeighbours(int x, int y)
    {
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
            if ((dx != 0 || dy != 0) && InBounds(x + dx, y + dy))
                yield return Index(x + dx, y + dy);
    }

    private int Index(int x, int y) => x + y * _size;
    private bool InBounds(int x, int y) => (uint)x < (uint)_size && (uint)y < (uint)_size;
    private static byte Byte(double value) => (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
    private static double Smooth(double value) => value * value * (3 - 2 * value);
    private static double Lerp(double a, double b, double value) => a + (b - a) * value;
    private static double Hash(int id, int seed, int salt)
    {
        uint value = unchecked((uint)(id * 374761393 + seed * 668265263 + salt * 1442695041));
        value = (value ^ (value >> 13)) * 1274126177; value ^= value >> 16;
        return value / (double)uint.MaxValue;
    }
}
