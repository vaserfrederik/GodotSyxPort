using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;

namespace GodotSyxPort.Rendering;

public sealed partial class ChunkTileRenderer : Node3D
{
    private readonly Dictionary<GridCoord, List<GridCoord>> _cellsByChunk = new();
    private readonly Dictionary<GridCoord, MultiMeshInstance3D> _instances = new();
    private Mesh _mesh = null!;
    private int _mapWidth;
    private int _mapHeight;

    public void Initialize(int mapWidth, int mapHeight, Color color, float height)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        _mesh = new BoxMesh
        {
            Size = new Vector3(0.94f, height, 0.94f),
            Material = new StandardMaterial3D
            {
                AlbedoColor = color,
                Transparency = color.A < 0.999f ? BaseMaterial3D.TransparencyEnum.Alpha : BaseMaterial3D.TransparencyEnum.Disabled,
                Roughness = 1f
            }
        };
    }

    public void AddCell(GridCoord cell)
    {
        var chunk = new GridCoord(cell.X / ChunkWallRenderer.ChunkSize, cell.Z / ChunkWallRenderer.ChunkSize);
        if (!_cellsByChunk.TryGetValue(chunk, out var cells))
        {
            cells = new List<GridCoord>();
            _cellsByChunk[chunk] = cells;
        }
        if (cells.Contains(cell)) return;
        cells.Add(cell);
        Rebuild(chunk, cells);
    }

    public void AddCells(IEnumerable<GridCoord> cells)
    {
        foreach (var group in cells.Distinct().GroupBy(cell =>
                     new GridCoord(cell.X / ChunkWallRenderer.ChunkSize,
                         cell.Z / ChunkWallRenderer.ChunkSize)))
        {
            if (!_cellsByChunk.TryGetValue(group.Key, out var chunkCells))
            {
                chunkCells = new List<GridCoord>();
                _cellsByChunk[group.Key] = chunkCells;
            }
            var known = chunkCells.ToHashSet();
            var changed = false;
            foreach (var cell in group)
                if (known.Add(cell))
                {
                    chunkCells.Add(cell);
                    changed = true;
                }
            if (changed) Rebuild(group.Key, chunkCells);
        }
    }

    public void RemoveCell(GridCoord cell)
    {
        var chunk = new GridCoord(cell.X / ChunkWallRenderer.ChunkSize,
            cell.Z / ChunkWallRenderer.ChunkSize);
        if (!_cellsByChunk.TryGetValue(chunk, out var cells) || !cells.Remove(cell)) return;
        Rebuild(chunk, cells);
    }

    public void RemoveCells(IEnumerable<GridCoord> cells)
    {
        foreach (var group in cells.Distinct().GroupBy(cell =>
                     new GridCoord(cell.X / ChunkWallRenderer.ChunkSize,
                         cell.Z / ChunkWallRenderer.ChunkSize)))
        {
            if (!_cellsByChunk.TryGetValue(group.Key, out var chunkCells)) continue;
            var removed = group.ToHashSet();
            if (chunkCells.RemoveAll(removed.Contains) > 0) Rebuild(group.Key, chunkCells);
        }
    }

    public void SetCells(IEnumerable<GridCoord> cells)
    {
        var nextByChunk = cells.Distinct().GroupBy(cell =>
                new GridCoord(cell.X / ChunkWallRenderer.ChunkSize, cell.Z / ChunkWallRenderer.ChunkSize))
            .ToDictionary(group => group.Key, group => group
                .OrderBy(cell => cell.Z).ThenBy(cell => cell.X).ToList());

        // A dragged room normally changes only one edge of one or two chunks. Reusing
        // every unchanged MultiMesh avoids rebuilding the whole blueprint for every
        // mouse-motion event.
        foreach (var oldChunk in _cellsByChunk.Keys.Where(chunk => !nextByChunk.ContainsKey(chunk)).ToArray())
        {
            _cellsByChunk.Remove(oldChunk);
            if (_instances.TryGetValue(oldChunk, out var renderer)) renderer.Visible = false;
        }
        foreach (var pair in nextByChunk)
        {
            if (_cellsByChunk.TryGetValue(pair.Key, out var previous) && previous.SequenceEqual(pair.Value))
            {
                if (_instances.TryGetValue(pair.Key, out var renderer)) renderer.Visible = true;
                continue;
            }
            _cellsByChunk[pair.Key] = pair.Value;
            Rebuild(pair.Key, pair.Value);
        }
    }

    public void Clear() => SetCells(System.Array.Empty<GridCoord>());

    private void Rebuild(GridCoord chunk, List<GridCoord> cells)
    {
        if (!_instances.TryGetValue(chunk, out var renderer))
        {
            renderer = new MultiMeshInstance3D
            {
                Name = $"Tiles_{chunk.X}_{chunk.Z}",
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            _instances[chunk] = renderer;
            AddChild(renderer);
        }
        renderer.Visible = true;
        var multiMesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = _mesh,
            InstanceCount = cells.Count
        };
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var world = new Vector3(cell.X - _mapWidth / 2f + 0.5f, 0.025f, cell.Z - _mapHeight / 2f + 0.5f);
            multiMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, world));
        }
        renderer.Multimesh = multiMesh;
    }
}
