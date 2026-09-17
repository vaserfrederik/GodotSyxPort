using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.Rooms;

/// <summary>Shared JobPositions/JobIterator search state for a room instance.</summary>
public sealed class RoomJobPositionsRuntime
{
    private readonly RoomInstanceRuntime _room;
    private readonly List<GridCoord> _positions;
    private readonly HashSet<ResourceKind> _searchMask = new();
    private readonly HashSet<ResourceKind> _notFound = new();
    private int _searchIndex;
    private bool _searchedAll;
    private bool _alwaysNew;

    public int Count => _positions.Count;
    public bool IsSearching => !_searchedAll;

    public RoomJobPositionsRuntime(RoomInstanceRuntime room, IEnumerable<GridCoord> positions)
    {
        _room = room;
        _positions = positions.Where(room.Contains).Distinct().ToList();
    }

    public BuildJob? Find(
        GridCoord? preferred,
        Func<GridCoord, BuildJob?> get,
        Func<BuildJob, bool> reservable)
    {
        if (_searchedAll || _positions.Count == 0) return null;
        if (!_alwaysNew && preferred is { } cell)
        {
            var direct = Candidate(cell, get, reservable);
            if (direct is not null) return direct;
            foreach (var position in _positions.Where(position =>
                         Math.Abs(position.X - cell.X) + Math.Abs(position.Z - cell.Z) < 5))
            {
                var nearby = Candidate(position, get, reservable);
                if (nearby is not null) return nearby;
            }
        }
        for (var checkedCount = 0; checkedCount < _positions.Count; checkedCount++)
        {
            if (_searchIndex >= _positions.Count) _searchIndex = 0;
            var job = Candidate(_positions[_searchIndex++], get, reservable);
            if (job is not null) return job;
        }
        _searchedAll = true;
        return null;
    }

    public void ReportResourceMissing(ResourceKind resource)
    {
        _searchMask.Add(resource);
        _notFound.Add(resource);
        _searchedAll = false;
    }

    public void ReportResourceFound(ResourceKind resource)
    {
        _searchMask.Remove(resource);
        _notFound.Remove(resource);
    }

    public bool ResourceShouldSearch(ResourceKind resource) => !_searchMask.Contains(resource);
    public bool ResourceReachable(ResourceKind resource) => !_notFound.Contains(resource);
    public void SearchAgain(bool resetResources = true)
    {
        _searchedAll = false;
        if (resetResources) _searchMask.Clear();
    }
    public void ResetResourceSearch() { _searchMask.Clear(); _notFound.Clear(); }
    public void StopSearching() => _searchedAll = true;
    public void SetAlwaysNew() => _alwaysNew = true;

    private BuildJob? Candidate(
        GridCoord cell,
        Func<GridCoord, BuildJob?> get,
        Func<BuildJob, bool> reservable)
    {
        if (!_room.Contains(cell)) return null;
        var job = get(cell);
        return job is not null && reservable(job) ? job : null;
    }
}
