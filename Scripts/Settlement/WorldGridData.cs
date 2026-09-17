using GodotSyxPort.Core;

namespace GodotSyxPort.Settlement;

[System.Flags]
public enum TileFlags : ushort
{
    None = 0,
    Wall = 1 << 0,
    Reserved = 1 << 1,
    Road = 1 << 2,
    Zone = 1 << 3,
    Door = 1 << 4,
    Furniture = 1 << 5,
    Vegetation = 1 << 6,
    ClearableTerrain = 1 << 7,
    EasilyClearableTerrain = 1 << 8,
    Water = 1 << 9,
    DeepWater = 1 << 10,
    Mountain = 1 << 11,
    Cave = 1 << 12,
    GroundWater = 1 << 13,
    FishSpot = 1 << 14,
    SaltWater = 1 << 15
}

public enum GroundKind : byte
{
    None, Soil, Wet, Forest, Mountain, FreshWater, SaltWater,
    Sand, Infertile, Pasture
}

public sealed class WorldGridMutableSnapshot
{
    public byte[] Roof { get; set; } = System.Array.Empty<byte>();
    public byte[] Vegetation { get; set; } = System.Array.Empty<byte>();
    public byte[] RoadDegradation { get; set; } = System.Array.Empty<byte>();
    public byte[] Moisture { get; set; } = System.Array.Empty<byte>();
    public byte[] MineralType { get; set; } = System.Array.Empty<byte>();
    public byte[] MineralAmount { get; set; } = System.Array.Empty<byte>();
    public byte[] FishAmount { get; set; } = System.Array.Empty<byte>();
    public byte[] GrowableType { get; set; } = System.Array.Empty<byte>();
    public byte[] GrowableAmount { get; set; } = System.Array.Empty<byte>();
    public byte[] Clearance { get; set; } = System.Array.Empty<byte>();
    public byte[] FurnitureRoles { get; set; } = System.Array.Empty<byte>();
    public byte[] RoomFloorSpeed { get; set; } = System.Array.Empty<byte>();
}

/// <summary>Dense simulation storage with no Godot Nodes or physics objects.</summary>
public sealed class WorldGridData
{
    private readonly ushort[] _tiles;
    private readonly byte[] _vegetation;
    private readonly byte[] _roadSpeed;
    private readonly byte[] _roadDegradation;
    private readonly byte[] _ground;
    private readonly byte[] _elevation;
    private readonly byte[] _fertility;
    private readonly byte[] _baseMoisture;
    private readonly byte[] _moisture;
    private readonly byte[] _mineralType;
    private readonly byte[] _mineralAmount;
    private readonly byte[] _fishAmount;
    private readonly byte[] _growableType;
    private readonly byte[] _growableAmount;
    private readonly byte[] _foundation;
    private readonly byte[] _roof;
    private readonly byte[] _furnitureRoles;
    private readonly byte[] _roomFloorSpeed;
    private ulong _revision;
    private ulong _terrainRevision;
    private ulong _infrastructureRevision;
    private ulong _fertilityRevision;
    private ulong _moistureRevision;
    private ulong _mineralRevision;
    public int Width { get; }
    public int Height { get; }
    public ulong Revision => _revision;
    public ulong TerrainRevision => _terrainRevision;
    public ulong InfrastructureRevision => _infrastructureRevision;
    public ulong FertilityRevision => _fertilityRevision;
    public ulong MoistureRevision => _moistureRevision;
    public ulong MineralRevision => _mineralRevision;

    public WorldGridData(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new ushort[width * height];
        _vegetation = new byte[width * height];
        _roadSpeed = new byte[width * height];
        _roadDegradation = new byte[width * height];
        _ground = new byte[width * height];
        _elevation = new byte[width * height];
        _fertility = new byte[width * height];
        _baseMoisture = new byte[width * height];
        _moisture = new byte[width * height];
        _mineralType = new byte[width * height];
        _mineralAmount = new byte[width * height];
        _fishAmount = new byte[width * height];
        _growableType = new byte[width * height];
        _growableAmount = new byte[width * height];
        _foundation = new byte[width * height];
        _roof = new byte[width * height];
        _furnitureRoles = new byte[width * height];
        _roomFloorSpeed = new byte[width * height];
    }

    public bool IsInside(GridCoord cell) =>
        (uint)cell.X < (uint)Width && (uint)cell.Z < (uint)Height;

    public bool Has(GridCoord cell, TileFlags flags) =>
        IsInside(cell) && (((TileFlags)_tiles[Index(cell)]) & flags) != 0;

    public void Set(GridCoord cell, TileFlags flags, bool enabled)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var previous = _tiles[index];
        if (enabled) _tiles[index] |= (ushort)flags;
        else _tiles[index] &= (ushort)~flags;
        if (_tiles[index] != previous)
        {
            _revision++;
            if ((flags & (TileFlags.Wall | TileFlags.Road | TileFlags.Zone |
                          TileFlags.Door | TileFlags.Furniture)) != 0) _infrastructureRevision++;
            if ((flags & (TileFlags.Water | TileFlags.DeepWater | TileFlags.Mountain |
                          TileFlags.Cave | TileFlags.Vegetation | TileFlags.SaltWater)) != 0)
                _terrainRevision++;
        }
    }

    public void SetVegetation(GridCoord cell, int amount)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var value = (byte)System.Math.Clamp(amount, 0, byte.MaxValue);
        if (_vegetation[index] != value)
        {
            _vegetation[index] = value; _revision++; _terrainRevision++;
        }
        Set(cell, TileFlags.Vegetation, amount > 0);
    }

    public int VegetationAmount(GridCoord cell) =>
        IsInside(cell) ? _vegetation[Index(cell)] : 0;

    public void GrowVegetation(GridCoord cell, int amount) =>
        SetVegetation(cell, System.Math.Min(15, VegetationAmount(cell) + amount));

    public bool ClearVegetationStep(GridCoord cell)
    {
        if (!IsInside(cell)) return true;
        SetVegetation(cell, System.Math.Max(0, _vegetation[Index(cell)] - 4));
        return _vegetation[Index(cell)] == 0;
    }

    public void ClearTerrain(GridCoord cell) =>
        Set(cell, TileFlags.ClearableTerrain | TileFlags.EasilyClearableTerrain, false);

    public bool IsBlocked(GridCoord cell) =>
        Has(cell, TileFlags.Wall) ||
        (Has(cell, TileFlags.DeepWater) && !Has(cell, TileFlags.Road)) ||
        (Has(cell, TileFlags.Mountain) && !Has(cell, TileFlags.Cave)) ||
        FurnitureBlocks(cell);

    public bool FurnitureBlocks(GridCoord cell) =>
        IsInside(cell) && (_furnitureRoles[Index(cell)] & 1) != 0;

    public bool FurnitureMustBeReachable(GridCoord cell) =>
        IsInside(cell) && (_furnitureRoles[Index(cell)] & 2) != 0;

    public bool FurnitureWorkstation(GridCoord cell) =>
        IsInside(cell) && (_furnitureRoles[Index(cell)] & 20) == 4;

    public bool FurnitureStorage(GridCoord cell) =>
        IsInside(cell) && (_furnitureRoles[Index(cell)] & 24) == 8;

    public bool FurnitureBroken(GridCoord cell) =>
        IsInside(cell) && (_furnitureRoles[Index(cell)] & 16) != 0;

    public void SetFurnitureRole(
        GridCoord cell, bool blocker, bool mustBeReachable,
        bool workstation = false, bool storage = false)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var broken = _furnitureRoles[index] & 16;
        var role = (byte)(broken | (blocker ? 1 : 0) | (mustBeReachable ? 2 : 0) |
                          (workstation ? 4 : 0) | (storage ? 8 : 0));
        if (_furnitureRoles[index] == role) return;
        _furnitureRoles[index] = role;
        _revision++;
        _infrastructureRevision++;
    }

    public void SetFurnitureBroken(GridCoord cell, bool broken)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var role = broken ? (byte)(_furnitureRoles[index] | 16) :
            (byte)(_furnitureRoles[index] & ~16);
        if (_furnitureRoles[index] == role) return;
        _furnitureRoles[index] = role;
        _revision++;
        _infrastructureRevision++;
    }

    public void ClearFurnitureRole(GridCoord cell)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        if (_furnitureRoles[index] == 0) return;
        _furnitureRoles[index] = 0;
        _revision++;
        _infrastructureRevision++;
    }

    public GroundKind Ground(GridCoord cell) => IsInside(cell) ? (GroundKind)_ground[Index(cell)] : GroundKind.None;
    public int Elevation(GridCoord cell) => IsInside(cell) ? _elevation[Index(cell)] : 0;
    public int Fertility(GridCoord cell) => IsInside(cell) ? _fertility[Index(cell)] : 0;
    public double FertilityD(GridCoord cell) => Fertility(cell) / 15.0;
    public int BaseMoisture(GridCoord cell) => IsInside(cell) ? _baseMoisture[Index(cell)] : 0;
    public int Moisture(GridCoord cell) => IsInside(cell) ? _moisture[Index(cell)] : 0;
    public int MineralType(GridCoord cell) => IsInside(cell) ? _mineralType[Index(cell)] - 1 : -1;
    public int MineralAmount(GridCoord cell) => IsInside(cell) ? _mineralAmount[Index(cell)] : 0;
    public int FishAmount(GridCoord cell) => IsInside(cell) ? _fishAmount[Index(cell)] : 0;
    public int GrowableType(GridCoord cell) => IsInside(cell) ? _growableType[Index(cell)] - 1 : -1;
    public int GrowableAmount(GridCoord cell) => IsInside(cell) ? _growableAmount[Index(cell)] : 0;
    public int Foundation(GridCoord cell) => IsInside(cell) ? _foundation[Index(cell)] : 0;
    public bool HasRoof(GridCoord cell) => IsInside(cell) && _roof[Index(cell)] != 0;
    public void SetRoof(GridCoord cell, bool roof)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var value = roof ? (byte)1 : (byte)0;
        if (_roof[index] == value) return;
        _roof[index] = value;
        _revision++;
        _infrastructureRevision++;
    }
    public double FoundationD(GridCoord cell) => Ground(cell) == GroundKind.Mountain
        ? 0.5
        : Foundation(cell) / 3.0;

    public WorldGridMutableSnapshot CaptureMutableState()
    {
        var clearance = new byte[_tiles.Length];
        for (var index = 0; index < _tiles.Length; index++)
        {
            var flags = (TileFlags)_tiles[index];
            if ((flags & TileFlags.ClearableTerrain) != 0) clearance[index] |= 1;
            if ((flags & TileFlags.EasilyClearableTerrain) != 0) clearance[index] |= 2;
        }
        return new WorldGridMutableSnapshot
        {
            Roof = (byte[])_roof.Clone(),
            Vegetation = (byte[])_vegetation.Clone(),
            RoadDegradation = (byte[])_roadDegradation.Clone(),
            Moisture = (byte[])_moisture.Clone(),
            MineralType = (byte[])_mineralType.Clone(),
            MineralAmount = (byte[])_mineralAmount.Clone(),
            FishAmount = (byte[])_fishAmount.Clone(),
            GrowableType = (byte[])_growableType.Clone(),
            GrowableAmount = (byte[])_growableAmount.Clone(),
            Clearance = clearance,
            FurnitureRoles = (byte[])_furnitureRoles.Clone(),
            RoomFloorSpeed = (byte[])_roomFloorSpeed.Clone()
        };
    }

    public bool RestoreMutableState(WorldGridMutableSnapshot snapshot)
    {
        var length = _tiles.Length;
        if ((snapshot.Roof.Length != 0 && snapshot.Roof.Length != length) ||
            snapshot.Vegetation.Length != length || snapshot.RoadDegradation.Length != length ||
            snapshot.Moisture.Length != length || snapshot.MineralType.Length != length ||
            snapshot.MineralAmount.Length != length || snapshot.FishAmount.Length != length ||
            snapshot.GrowableType.Length != length || snapshot.GrowableAmount.Length != length ||
            snapshot.Clearance.Length != length ||
            (snapshot.FurnitureRoles.Length != 0 && snapshot.FurnitureRoles.Length != length)) return false;
        if (snapshot.RoomFloorSpeed.Length != 0 && snapshot.RoomFloorSpeed.Length != length) return false;
        if (snapshot.Roof.Length == length) System.Array.Copy(snapshot.Roof, _roof, length);
        System.Array.Copy(snapshot.Vegetation, _vegetation, length);
        System.Array.Copy(snapshot.RoadDegradation, _roadDegradation, length);
        System.Array.Copy(snapshot.Moisture, _moisture, length);
        System.Array.Copy(snapshot.MineralType, _mineralType, length);
        System.Array.Copy(snapshot.MineralAmount, _mineralAmount, length);
        System.Array.Copy(snapshot.FishAmount, _fishAmount, length);
        System.Array.Copy(snapshot.GrowableType, _growableType, length);
        System.Array.Copy(snapshot.GrowableAmount, _growableAmount, length);
        if (snapshot.FurnitureRoles.Length == length)
            System.Array.Copy(snapshot.FurnitureRoles, _furnitureRoles, length);
        else
            System.Array.Clear(_furnitureRoles);
        if (snapshot.RoomFloorSpeed.Length == length)
            System.Array.Copy(snapshot.RoomFloorSpeed, _roomFloorSpeed, length);
        else
            System.Array.Clear(_roomFloorSpeed);
        for (var index = 0; index < length; index++)
        {
            _tiles[index] &= (ushort)~(TileFlags.Vegetation | TileFlags.ClearableTerrain |
                                        TileFlags.EasilyClearableTerrain);
            if (_vegetation[index] > 0) _tiles[index] |= (ushort)TileFlags.Vegetation;
            if ((snapshot.Clearance[index] & 1) != 0) _tiles[index] |= (ushort)TileFlags.ClearableTerrain;
            if ((snapshot.Clearance[index] & 2) != 0) _tiles[index] |= (ushort)TileFlags.EasilyClearableTerrain;
        }
        _revision++;
        _terrainRevision++;
        _infrastructureRevision++;
        _moistureRevision++;
        _mineralRevision++;
        return true;
    }

    public void SetFertility(GridCoord cell, int fertility)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var value = (byte)System.Math.Clamp(fertility, 0, 15);
        if (_fertility[index] != value)
        {
            _fertility[index] = value; _revision++; _fertilityRevision++;
        }
    }

    /// <summary>Applies irrigation/drainage without changing the generated terrain baseline.</summary>
    public void SetMoisture(GridCoord cell, int moisture)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var value = (byte)System.Math.Clamp(moisture, 0, 15);
        if (_moisture[index] != value)
        {
            _moisture[index] = value; _revision++; _moistureRevision++;
        }
    }

    public void SetTerrain(GridCoord cell, GroundKind ground, int elevation, int fertility, int moisture)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        if (_ground[index] != (byte)ground || _elevation[index] != elevation ||
            _fertility[index] != fertility || _baseMoisture[index] != moisture)
        {
            _revision++; _terrainRevision++; _fertilityRevision++; _moistureRevision++;
        }
        _ground[index] = (byte)ground;
        _elevation[index] = (byte)System.Math.Clamp(elevation, 0, 255);
        _fertility[index] = (byte)System.Math.Clamp(fertility, 0, 15);
        _baseMoisture[index] = (byte)System.Math.Clamp(moisture, 0, 15);
        _moisture[index] = _baseMoisture[index];
        Set(cell, TileFlags.Water, ground is GroundKind.FreshWater or GroundKind.SaltWater);
        Set(cell, TileFlags.Mountain, ground == GroundKind.Mountain);
    }

    public void SetDeepWater(GridCoord cell, bool deep) => Set(cell, TileFlags.DeepWater, deep);
    public void SetCave(GridCoord cell, bool cave) => Set(cell, TileFlags.Cave, cave);
    public void SetMineral(GridCoord cell, int type, int amount)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var previousType = _mineralType[index];
        var previousAmount = _mineralAmount[index];
        _mineralType[index] = amount > 0 ? (byte)System.Math.Clamp(type + 1, 1, byte.MaxValue) : (byte)0;
        _mineralAmount[index] = (byte)System.Math.Clamp(amount, 0, 63);
        if (previousType != _mineralType[index] || previousAmount != _mineralAmount[index])
        {
            _revision++; _mineralRevision++;
        }
    }

    public int ExtractMineral(GridCoord cell, int amount)
    {
        if (!IsInside(cell) || amount <= 0) return 0;
        var index = Index(cell);
        var extracted = System.Math.Min(amount, _mineralAmount[index]);
        _mineralAmount[index] -= (byte)extracted;
        if (_mineralAmount[index] == 0) _mineralType[index] = 0;
        if (extracted > 0) { _revision++; _mineralRevision++; }
        return extracted;
    }

    public void SetFish(GridCoord cell, int amount, bool spot)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var value = (byte)System.Math.Clamp(amount, 0, 15);
        if (_fishAmount[index] != value)
        {
            _fishAmount[index] = value; _revision++; _terrainRevision++;
        }
        Set(cell, TileFlags.FishSpot, spot && amount > 0);
    }

    public void SetGrowable(GridCoord cell, int type, int amount)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var previousType = _growableType[index];
        var previousAmount = _growableAmount[index];
        _growableType[index] = amount > 0 ? (byte)System.Math.Clamp(type + 1, 1, byte.MaxValue) : (byte)0;
        _growableAmount[index] = (byte)System.Math.Clamp(amount, 0, 15);
        if (previousType != _growableType[index] || previousAmount != _growableAmount[index])
        {
            _revision++; _terrainRevision++;
        }
    }

    public void SetFoundation(GridCoord cell, int value)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var normalized = (byte)System.Math.Clamp(value, 0, 3);
        if (_foundation[index] != normalized) { _foundation[index] = normalized; _revision++; }
    }
    public void SetRoadSpeed(GridCoord cell, int sourceSpeed)
    {
        if (IsInside(cell)) _roadSpeed[Index(cell)] = (byte)System.Math.Clamp(sourceSpeed, 0, 4);
    }

    public void SetRoomFloorSpeed(GridCoord cell, int sourceSpeed)
    {
        if (IsInside(cell)) _roomFloorSpeed[Index(cell)] =
            (byte)(System.Math.Clamp(sourceSpeed, 0, 4) + 1);
    }

    public void ClearRoomFloorSpeed(GridCoord cell)
    {
        if (IsInside(cell)) _roomFloorSpeed[Index(cell)] = 0;
    }

    public float MovementSpeedMultiplier(GridCoord cell) =>
        Has(cell, TileFlags.Road) && RoadDegradation(cell) < 15
            ? 1f + _roadSpeed[Index(cell)] * 0.05f
            : IsInside(cell) && _roomFloorSpeed[Index(cell)] > 0
                ? 1f + (_roomFloorSpeed[Index(cell)] - 1) * 0.05f
                : 1f;

    public int RoadDegradation(GridCoord cell) =>
        IsInside(cell) ? _roadDegradation[Index(cell)] : 0;

    public void SetRoadDegradation(GridCoord cell, int value)
    {
        if (!IsInside(cell)) return;
        var index = Index(cell);
        var normalized = (byte)System.Math.Clamp(value, 0, 15);
        if (_roadDegradation[index] == normalized) return;
        _roadDegradation[index] = normalized;
        _revision++;
        _infrastructureRevision++;
    }

    public int MovementCost(GridCoord cell) =>
        (int)System.MathF.Round(100f / MovementSpeedMultiplier(cell));
    private int Index(GridCoord cell) => cell.Z * Width + cell.X;
}
