using System;
using System.Collections.Generic;
using GodotSyxPort.Core;

namespace GodotSyxPort.Navigation;

public static class HierarchicalGridPathfinder
{
    public const int RegionSize = 32;

    public static List<GridCoord> FindPath(
        GridCoord start,
        GridCoord goal,
        int width,
        int height,
        Func<GridCoord, bool> isBlocked,
        Func<GridCoord, int> movementCost)
    {
        var distance = Math.Abs(goal.X - start.X) + Math.Abs(goal.Z - start.Z);
        if (distance <= RegionSize * 3)
            return GridPathfinder.FindPath(start, goal, width, height, isBlocked, movementCost);

        var startRegion = Region(start);
        var goalRegion = Region(goal);
        var regionsWide = (width + RegionSize - 1) / RegionSize;
        var regionsHigh = (height + RegionSize - 1) / RegionSize;
        var regionPath = FindRegionPath(startRegion, goalRegion, regionsWide, regionsHigh);
        if (regionPath.Count == 0) return new List<GridCoord>();

        var corridor = new HashSet<GridCoord>();
        foreach (var region in regionPath)
        {
            corridor.Add(region);
            foreach (var offset in GridCoord.Cardinal)
            {
                var neighbor = region + offset;
                if ((uint)neighbor.X < (uint)regionsWide && (uint)neighbor.Z < (uint)regionsHigh)
                    corridor.Add(neighbor);
            }
        }

        var detailed = GridPathfinder.FindPath(
            start,
            goal,
            width,
            height,
            cell => isBlocked(cell) || !corridor.Contains(Region(cell)),
            movementCost,
            Math.Min(width * height, corridor.Count * RegionSize * RegionSize));
        if (detailed.Count > 0) return detailed;

        // A wall can force a detour outside the first corridor. Keep a bounded fallback
        // until portal connectivity is cached per region.
        return GridPathfinder.FindPath(
            start, goal, width, height, isBlocked, movementCost, Math.Min(width * height, 262144));
    }

    private static List<GridCoord> FindRegionPath(
        GridCoord start,
        GridCoord goal,
        int width,
        int height)
    {
        var frontier = new PriorityQueue<GridCoord, int>();
        var cameFrom = new Dictionary<GridCoord, GridCoord> { [start] = start };
        var costs = new Dictionary<GridCoord, int> { [start] = 0 };
        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == goal) break;
            foreach (var offset in GridCoord.Cardinal)
            {
                var next = current + offset;
                if ((uint)next.X >= (uint)width || (uint)next.Z >= (uint)height) continue;
                var nextCost = costs[current] + 1;
                if (costs.TryGetValue(next, out var known) && known <= nextCost) continue;
                costs[next] = nextCost;
                cameFrom[next] = current;
                var heuristic = Math.Abs(goal.X - next.X) + Math.Abs(goal.Z - next.Z);
                frontier.Enqueue(next, nextCost + heuristic);
            }
        }

        if (!cameFrom.ContainsKey(goal)) return new List<GridCoord>();
        var path = new List<GridCoord>();
        var step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    private static GridCoord Region(GridCoord cell) =>
        new(cell.X / RegionSize, cell.Z / RegionSize);
}
