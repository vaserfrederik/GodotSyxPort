using System;
using System.Collections.Generic;
using GodotSyxPort.Core;

namespace GodotSyxPort.Navigation;

public static class GridPathfinder
{
    public static List<GridCoord> FindPath(
        GridCoord start,
        GridCoord goal,
        int width,
        int height,
        Func<GridCoord, bool> isBlocked,
        Func<GridCoord, int>? traversalCost = null,
        int maxVisited = 65536)
    {
        var frontier = new PriorityQueue<GridCoord, int>();
        var cameFrom = new Dictionary<GridCoord, GridCoord>();
        var costs = new Dictionary<GridCoord, int>();
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costs[start] = 0;
        var visited = 0;

        while (frontier.Count > 0 && visited++ < maxVisited)
        {
            var current = frontier.Dequeue();
            if (current == goal) break;

            foreach (var offset in GridCoord.Cardinal)
            {
                var next = current + offset;
                if ((uint)next.X >= (uint)width || (uint)next.Z >= (uint)height || isBlocked(next)) continue;

                var nextCost = costs[current] + (traversalCost?.Invoke(next) ?? 10);
                if (costs.TryGetValue(next, out var knownCost) && knownCost <= nextCost) continue;

                costs[next] = nextCost;
                cameFrom[next] = current;
                var heuristic = Math.Abs(goal.X - next.X) + Math.Abs(goal.Z - next.Z);
                frontier.Enqueue(next, nextCost + heuristic * 6);
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
        path.Reverse();
        return path;
    }
}
