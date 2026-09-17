using System.Buffers;
using System.Collections.Generic;
using GodotSyxPort.Core;

namespace GodotSyxPort.Navigation;

/// <summary>Owns all active path buffers so agents only store a compact handle.</summary>
public sealed class SharedPathPool
{
    private sealed class Entry
    {
        public GridCoord[] Buffer = null!;
        public int Length;
        public int Index;
    }

    private readonly Dictionary<int, Entry> _entries = new();
    private readonly Stack<int> _freeHandles = new();
    private int _nextHandle = 1;

    public int ActiveCount => _entries.Count;

    public int Acquire(IReadOnlyList<GridCoord> path)
    {
        if (path.Count == 0) return 0;
        var handle = _freeHandles.Count > 0 ? _freeHandles.Pop() : _nextHandle++;
        var buffer = ArrayPool<GridCoord>.Shared.Rent(path.Count);
        for (var i = 0; i < path.Count; i++) buffer[i] = path[i];
        _entries[handle] = new Entry { Buffer = buffer, Length = path.Count };
        return handle;
    }

    public bool TryPeek(int handle, out GridCoord cell)
    {
        if (handle != 0 && _entries.TryGetValue(handle, out var entry) && entry.Index < entry.Length)
        {
            cell = entry.Buffer[entry.Index];
            return true;
        }
        cell = default;
        return false;
    }

    public bool Advance(int handle)
    {
        if (!_entries.TryGetValue(handle, out var entry)) return false;
        entry.Index++;
        if (entry.Index < entry.Length) return true;
        Release(handle);
        return false;
    }

    public void Release(int handle)
    {
        if (handle == 0 || !_entries.Remove(handle, out var entry)) return;
        ArrayPool<GridCoord>.Shared.Return(entry.Buffer);
        _freeHandles.Push(handle);
    }
}
