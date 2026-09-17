using System.Collections.Generic;
using Godot;
using GodotSyxPort.Core;

namespace GodotSyxPort.Rendering;

/// <summary>Renders walls in one MultiMesh per active 32x32 chunk.</summary>
public sealed partial class ChunkWallRenderer : Node3D
{
    public const int ChunkSize = 32;
    private readonly Dictionary<GridCoord, List<GridCoord>> _cellsByChunk = new();
    private readonly Dictionary<GridCoord, MultiMeshInstance3D> _instances = new();
    private BoxMesh _wallMesh = null!;
    private int _mapWidth;
    private int _mapHeight;

    public void Initialize(int mapWidth, int mapHeight) =>
        Initialize(mapWidth, mapHeight, new Vector3(1f, 2f, 1f), new Color("8a7965"), 1f);

    public void Initialize(int mapWidth, int mapHeight, Vector3 size, Color color, float y)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        _wallMesh = new BoxMesh
        {
            Size = size,
            Material = new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 1f
            }
        };
        _instanceY = y;
    }

    private float _instanceY = 1f;

    public void AddWall(GridCoord cell)
    {
        var chunk = new GridCoord(cell.X / ChunkSize, cell.Z / ChunkSize);
        if (!_cellsByChunk.TryGetValue(chunk, out var cells))
        {
            cells = new List<GridCoord>();
            _cellsByChunk[chunk] = cells;
        }
        cells.Add(cell);
        RebuildChunk(chunk, cells);
    }

    public void RemoveWall(GridCoord cell)
    {
        var chunk = new GridCoord(cell.X / ChunkSize, cell.Z / ChunkSize);
        if (!_cellsByChunk.TryGetValue(chunk, out var cells) || !cells.Remove(cell)) return;
        RebuildChunk(chunk, cells);
    }

    private void RebuildChunk(GridCoord chunk, List<GridCoord> cells)
    {
        if (!_instances.TryGetValue(chunk, out var renderer))
        {
            renderer = new MultiMeshInstance3D { Name = $"Walls_{chunk.X}_{chunk.Z}" };
            _instances[chunk] = renderer;
            AddChild(renderer);
        }

        var multiMesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = _wallMesh,
            InstanceCount = cells.Count
        };
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var world = new Vector3(cell.X - _mapWidth / 2f + 0.5f, _instanceY, cell.Z - _mapHeight / 2f + 0.5f);
            multiMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, world));
        }
        renderer.Multimesh = multiMesh;
    }
}
