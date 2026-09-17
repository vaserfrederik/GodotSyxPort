using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Rendering;
using System.Collections.Generic;
using GodotSyxPort.Data;

namespace GodotSyxPort.Settlement;

public sealed partial class GridWorld : Node3D
{
    public event System.Action? NavigationChanged;
    // Data/Original/init/config/Sett.txt: DIMENSION = 768 (must be divisible by 64).
    public const int Width = 768;
    public const int Height = 768;
    public SettlementGenerationProfile? GenerationProfile { get; set; }
    public bool GenerateOnReady { get; set; } = true;

    public WorldGridData Data { get; } = new(Width, Height);
    private ChunkWallRenderer _walls = null!;
    private ChunkTileRenderer _roads = null!;
    private ChunkTileRenderer _roomFloors = null!;
    private ChunkTileRenderer _zones = null!;
    private ChunkTileRenderer _doors = null!;
    private ChunkWallRenderer _furniture = null!;
    private ChunkTileRenderer _draftArea = null!;
    private ChunkTileRenderer _draftPerimeter = null!;
    private ChunkTileRenderer _draftDoors = null!;
    private ChunkTileRenderer _draftFurniture = null!;
    private ChunkTileRenderer _draftFurnitureInvalid = null!;
    private readonly List<int> _wallCells = new();
    private readonly List<int> _roadCells = new();
    private readonly Dictionary<int, string> _roadKeys = new();
    private readonly List<int> _roomFloorCells = new();
    private readonly Dictionary<int, string> _roomFloorKeys = new();
    private readonly List<int> _zoneCells = new();
    private readonly List<int> _doorCells = new();
    private readonly List<int> _furnitureCells = new();
    private readonly List<MeshInstance3D> _landingMarkers = new();
    private ImageTexture _groundTexture = null!;
    private bool _renderingInitialized;

    public IReadOnlyList<int> WallCells => _wallCells;
    public IReadOnlyList<int> RoadCells => _roadCells;
    public IReadOnlyDictionary<int, string> RoadKeys => _roadKeys;
    public IReadOnlyList<int> RoomFloorCells => _roomFloorCells;
    public IReadOnlyDictionary<int, string> RoomFloorKeys => _roomFloorKeys;
    public IReadOnlyList<int> ZoneCells => _zoneCells;
    public IReadOnlyList<int> DoorCells => _doorCells;
    public IReadOnlyList<int> FurnitureCells => _furnitureCells;

    public override void _Ready()
    {
        if (!GenerateOnReady) return;
        GenerateTerrainData(SettlementGeneratorSettings.Load());
        InitializeRendering();
    }

    public void GenerateTerrainData(SettlementGeneratorSettings settings)
    {
        new SettlementTerrainGenerator().Generate(Data,
            GenerationProfile ?? SettlementGenerationProfile.Neutral(
                1, OriginalGameData.Current.Minables, OriginalGameData.Current.Growables), settings);
    }

    public void InitializeRendering()
    {
        if (_renderingInitialized) return;
        _renderingInitialized = true;
        CreateGround();
        _walls = new ChunkWallRenderer { Name = "ChunkWalls" };
        AddChild(_walls);
        _walls.Initialize(Width, Height);
        _roads = new ChunkTileRenderer { Name = "ChunkRoads" };
        AddChild(_roads);
        _roads.Initialize(Width, Height, new Color("8b7655"), 0.04f);
        var defaultRoad = OriginalGameData.Current.DefaultRoad();
        var generatedRoads = new List<GridCoord>();
        for (var z = 0; z < Height; z++)
        for (var x = 0; x < Width; x++)
        {
            var cell = new GridCoord(x, z);
            if (!Data.Has(cell, TileFlags.Road)) continue;
            var index = CellToIndex(cell);
            _roadCells.Add(index);
            _roadKeys[index] = defaultRoad.Key;
            Data.SetRoadSpeed(cell, defaultRoad.Speed);
            generatedRoads.Add(cell);
        }
        _roads.SetCells(generatedRoads);
        _roomFloors = new ChunkTileRenderer { Name = "ChunkRoomFloors" };
        AddChild(_roomFloors);
        _roomFloors.Initialize(Width, Height, new Color("756d5d"), 0.035f);
        _zones = new ChunkTileRenderer { Name = "ChunkZones" };
        AddChild(_zones);
        _zones.Initialize(Width, Height, new Color(0.2f, 0.55f, 0.95f, 0.38f), 0.025f);
        _doors = new ChunkTileRenderer { Name = "ChunkDoors" };
        AddChild(_doors);
        _doors.Initialize(Width, Height, new Color("d7ad49"), 0.06f);
        _furniture = new ChunkWallRenderer { Name = "ChunkFurniture" };
        AddChild(_furniture);
        _furniture.Initialize(Width, Height, new Vector3(0.72f, 0.7f, 0.72f), new Color("725338"), 0.35f);
        _draftArea = new ChunkTileRenderer { Name = "DraftArea" };
        AddChild(_draftArea);
        _draftArea.Initialize(Width, Height, new Color(0.18f, 0.55f, 1f, 0.44f), 0.08f);
        _draftPerimeter = new ChunkTileRenderer { Name = "DraftPerimeter" };
        AddChild(_draftPerimeter);
        _draftPerimeter.Initialize(Width, Height, new Color(0.96f, 0.97f, 1f, 0.9f), 0.1f);
        _draftDoors = new ChunkTileRenderer { Name = "DraftDoors" };
        AddChild(_draftDoors);
        _draftDoors.Initialize(Width, Height, new Color(1f, 0.82f, 0.2f, 0.8f), 0.12f);
        _draftFurniture = new ChunkTileRenderer { Name = "DraftFurniture" };
        AddChild(_draftFurniture);
        _draftFurniture.Initialize(Width, Height, new Color(0.12f, 0.48f, 1f, 0.86f), 0.14f);
        _draftFurnitureInvalid = new ChunkTileRenderer { Name = "DraftFurnitureInvalid" };
        AddChild(_draftFurnitureInvalid);
        _draftFurnitureInvalid.Initialize(Width, Height, new Color(0.92f, 0.16f, 0.12f, 0.9f), 0.145f);
    }

    public bool IsInside(GridCoord cell) => Data.IsInside(cell);

    public GridCoord FindNearestWalkable(
        GridCoord requested,
        ISet<GridCoord>? excluded = null,
        int localSearchRadius = 128)
    {
        for (var radius = 0; radius <= localSearchRadius; radius++)
        for (var dx = -radius; dx <= radius; dx++)
        {
            var dz = radius - System.Math.Abs(dx);
            var first = new GridCoord(requested.X + dx, requested.Z + dz);
            if (IsInside(first) && !Data.IsBlocked(first) && excluded?.Contains(first) != true)
                return first;
            if (dz == 0) continue;
            var second = new GridCoord(requested.X + dx, requested.Z - dz);
            if (IsInside(second) && !Data.IsBlocked(second) && excluded?.Contains(second) != true)
                return second;
        }
        for (var z = 0; z < Height; z++)
        for (var x = 0; x < Width; x++)
        {
            var candidate = new GridCoord(x, z);
            if (!Data.IsBlocked(candidate) && excluded?.Contains(candidate) != true) return candidate;
        }
        throw new System.InvalidOperationException("Settlement terrain has no walkable cell.");
    }

    public void PlaceLandingMarker(GridCoord cell, Vector3 size, Color color, string name)
    {
        if (!IsInside(cell))
            throw new System.ArgumentOutOfRangeException(nameof(cell));
        var marker = new MeshInstance3D
        {
            Name = name,
            Position = CellToWorld(cell, size.Y * 0.5f),
            Mesh = new BoxMesh
            {
                Size = size,
                Material = new StandardMaterial3D
                {
                    AlbedoColor = color,
                    Roughness = 1f
                }
            }
        };
        _landingMarkers.Add(marker);
        AddChild(marker);
    }
    public Vector3 CellToWorld(GridCoord cell, float y = 0f) =>
        new(cell.X - Width / 2f + 0.5f, y, cell.Z - Height / 2f + 0.5f);
    public GridCoord WorldToCell(Vector3 position) =>
        new(Mathf.FloorToInt(position.X + Width / 2f), Mathf.FloorToInt(position.Z + Height / 2f));
    public bool CanPlanWall(GridCoord cell) =>
        IsInside(cell) && !Data.Has(cell, TileFlags.Wall | TileFlags.Reserved);
    public void ReserveWall(GridCoord cell) => Data.Set(cell, TileFlags.Reserved, true);

    public bool CanPlanRoad(GridCoord cell) =>
        IsInside(cell) && !Data.Has(cell, TileFlags.Wall | TileFlags.Road | TileFlags.Reserved);

    public void ReserveRoad(GridCoord cell) => Data.Set(cell, TileFlags.Reserved, true);
    public void CancelReservation(GridCoord cell) => Data.Set(cell, TileFlags.Reserved, false);

    public void BuildWall(GridCoord cell)
    {
        Data.Set(cell, TileFlags.Reserved, false);
        if (Data.Has(cell, TileFlags.Wall)) return;
        Data.Set(cell, TileFlags.Wall, true);
        _wallCells.Add(CellToIndex(cell));
        _walls.AddWall(cell);
        NavigationChanged?.Invoke();
    }

    public bool RemoveWall(GridCoord cell)
    {
        if (!Data.Has(cell, TileFlags.Wall)) return false;
        Data.Set(cell, TileFlags.Wall, false);
        Data.Set(cell, TileFlags.Reserved, false);
        _wallCells.Remove(CellToIndex(cell));
        _walls.RemoveWall(cell);
        NavigationChanged?.Invoke();
        return true;
    }

    public void BuildRoad(GridCoord cell) => BuildRoad(cell, OriginalGameData.Current.DefaultRoad());

    public void BuildRoad(GridCoord cell, FloorRule road)
    {
        Data.Set(cell, TileFlags.Reserved, false);
        if (Data.Has(cell, TileFlags.Road | TileFlags.Wall)) return;
        Data.Set(cell, TileFlags.Road, true);
        Data.SetRoadSpeed(cell, road.Speed);
        var index = CellToIndex(cell);
        _roadCells.Add(index);
        _roadKeys[index] = road.Key;
        _roads.AddCell(cell);
    }

    public bool RemoveRoad(GridCoord cell)
    {
        if (!Data.Has(cell, TileFlags.Road)) return false;
        Data.Set(cell, TileFlags.Road, false);
        Data.SetRoadSpeed(cell, 1);
        var index = CellToIndex(cell);
        _roadCells.Remove(index);
        _roadKeys.Remove(index);
        _roads.RemoveCell(cell);
        return true;
    }

    public void BuildRoomFloor(GridCoord cell, FloorRule floor)
    {
        if (!IsInside(cell)) return;
        var index = CellToIndex(cell);
        if (!_roomFloorCells.Contains(index)) _roomFloorCells.Add(index);
        _roomFloorKeys[index] = floor.Key;
        Data.SetRoomFloorSpeed(cell, floor.Speed);
        _roomFloors.AddCell(cell);
    }

    public bool RemoveRoomFloor(GridCoord cell)
    {
        var index = CellToIndex(cell);
        if (!_roomFloorCells.Remove(index)) return false;
        _roomFloorKeys.Remove(index);
        Data.ClearRoomFloorSpeed(cell);
        _roomFloors.RemoveCell(cell);
        return true;
    }

    public void SetZone(GridCoord cell)
    {
        if (!IsInside(cell) || Data.Has(cell, TileFlags.Wall | TileFlags.Zone)) return;
        Data.Set(cell, TileFlags.Zone, true);
        _zoneCells.Add(CellToIndex(cell));
        _zones.AddCell(cell);
    }

    public bool ClearZone(GridCoord cell)
    {
        if (!Data.Has(cell, TileFlags.Zone)) return false;
        Data.Set(cell, TileFlags.Zone, false);
        _zoneCells.Remove(CellToIndex(cell));
        _zones.RemoveCell(cell);
        return true;
    }

    public void SetDoor(GridCoord cell)
    {
        if (!IsInside(cell) || Data.Has(cell, TileFlags.Door)) return;
        Data.Set(cell, TileFlags.Door, true);
        _doorCells.Add(CellToIndex(cell));
        _doors.AddCell(cell);
    }

    public bool RemoveDoor(GridCoord cell)
    {
        if (!Data.Has(cell, TileFlags.Door)) return false;
        Data.Set(cell, TileFlags.Door, false);
        _doorCells.Remove(CellToIndex(cell));
        _doors.RemoveCell(cell);
        return true;
    }

    public void ShowRoomPreview(
        IEnumerable<GridCoord> area,
        IEnumerable<GridCoord> perimeter,
        IEnumerable<GridCoord> doors,
        IEnumerable<GridCoord>? furniture = null,
        IEnumerable<GridCoord>? invalidFurniture = null)
    {
        _draftArea.SetCells(area);
        _draftPerimeter.SetCells(perimeter);
        _draftDoors.SetCells(doors);
        _draftFurniture.SetCells(furniture ?? System.Array.Empty<GridCoord>());
        _draftFurnitureInvalid.SetCells(invalidFurniture ?? System.Array.Empty<GridCoord>());
    }

    /// <summary>
    /// Door editing changes only the opening mask. Keeping the already rendered area,
    /// perimeter and furniture mirrors PlacerDoor's local update and avoids rebuilding
    /// the complete room preview for one doorway click.
    /// </summary>
    public void ShowDoorPreview(IEnumerable<GridCoord> doors) => _draftDoors.SetCells(doors);

    public void ClearRoomPreview()
    {
        _draftArea.Clear();
        _draftPerimeter.Clear();
        _draftDoors.Clear();
        _draftFurniture.Clear();
        _draftFurnitureInvalid.Clear();
    }

    public bool CanPlanFurniture(GridCoord cell) =>
        IsInside(cell) && Data.Has(cell, TileFlags.Zone) &&
        !Data.Has(cell, TileFlags.Wall | TileFlags.Furniture | TileFlags.Reserved);

    public bool CanStandForConstruction(GridCoord cell) =>
        IsInside(cell) && !Data.IsBlocked(cell) &&
        !Data.Has(cell, TileFlags.Wall | TileFlags.Furniture | TileFlags.Reserved);

    public void ReserveFurniture(GridCoord cell) => Data.Set(cell, TileFlags.Reserved, true);

    public void BuildFurniture(
        GridCoord cell, bool? blocker = null, bool? mustBeReachable = null,
        bool workstation = false, bool storage = false)
    {
        Data.Set(cell, TileFlags.Reserved, false);
        if (blocker.HasValue || mustBeReachable.HasValue)
            Data.SetFurnitureRole(
                cell, blocker ?? false, mustBeReachable ?? false, workstation, storage);
        if (Data.Has(cell, TileFlags.Furniture | TileFlags.Wall)) return;
        Data.Set(cell, TileFlags.Furniture, true);
        _furnitureCells.Add(CellToIndex(cell));
        _furniture.AddWall(cell);
    }

    public bool RemoveFurniture(GridCoord cell)
    {
        var existed = Data.Has(cell, TileFlags.Furniture);
        Data.Set(cell, TileFlags.Reserved, false);
        Data.ClearFurnitureRole(cell);
        if (!existed) return false;
        Data.Set(cell, TileFlags.Furniture, false);
        _furnitureCells.Remove(CellToIndex(cell));
        _furniture.RemoveWall(cell);
        return true;
    }

    public GridCoord FromIndex(int index) => new(index % Width, index / Width);
    public int CellToIndex(GridCoord cell) => cell.Z * Width + cell.X;

    public void ApplyGroundOverlay(System.Func<GridCoord, Color>? colorizer)
    {
        if (colorizer is null)
        {
            _groundTexture.Update(OriginalSettlementTerrainTextureBuilder.Build(
                Data, GenerationProfile?.Seed ?? 1));
            return;
        }
        var scale = OriginalSettlementTerrainTextureBuilder.PixelsPerTile;
        var textureWidth = Width * scale;
        var textureHeight = Height * scale;
        var pixels = new byte[textureWidth * textureHeight * 4];
        for (var z = 0; z < Height; z++)
        for (var x = 0; x < Width; x++)
        {
            var cell = new GridCoord(x, z);
            var color = colorizer(cell);
            for (var py = 0; py < scale; py++)
            for (var px = 0; px < scale; px++)
            {
                var offset = (((z * scale + py) * textureWidth) + x * scale + px) * 4;
                pixels[offset] = (byte)color.R8;
                pixels[offset + 1] = (byte)color.G8;
                pixels[offset + 2] = (byte)color.B8;
                pixels[offset + 3] = 255;
            }
        }
        var image = Image.CreateFromData(textureWidth, textureHeight, false, Image.Format.Rgba8, pixels);
        _groundTexture.Update(image);
    }

    private void CreateGround()
    {
        var image = OriginalSettlementTerrainTextureBuilder.Build(
            Data, GenerationProfile?.Seed ?? 1);
        _groundTexture = ImageTexture.CreateFromImage(image);
        var material = new StandardMaterial3D
        {
            AlbedoTexture = _groundTexture,
            Roughness = 1f,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest
        };
        AddChild(new MeshInstance3D
        {
            Name = "Ground",
            Mesh = new PlaneMesh
            {
                Size = new Vector2(Width, Height),
                Material = material
            }
        });

        var body = new StaticBody3D { Name = "GroundCollider" };
        body.AddChild(new CollisionShape3D
        {
            Shape = new BoxShape3D { Size = new Vector3(Width, 0.1f, Height) },
            Position = new Vector3(0, -0.05f, 0)
        });
        AddChild(body);
    }

    private Color TerrainPlaceholderColor(GridCoord cell)
    {
        if (Data.Has(cell, TileFlags.DeepWater)) return new Color("174d73");
        if (Data.Has(cell, TileFlags.Water)) return new Color("287cad");
        var color = Data.Ground(cell) switch
        {
            GroundKind.Mountain => new Color("686a66"),
            GroundKind.Forest => new Color("254c2b"),
            GroundKind.Wet => new Color("496645"),
            GroundKind.Sand => new Color("b9a66c"),
            GroundKind.Infertile => new Color("5b574b"),
            GroundKind.Pasture => new Color("778052"),
            GroundKind.Soil => new Color("657747"),
            _ => new Color("49583b")
        };
        return color.Lerp(new Color("789653"), Data.Fertility(cell) / 15f * 0.32f);
    }
}
