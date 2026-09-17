using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Core;
using GodotSyxPort.Resources;

namespace GodotSyxPort.Rooms;

/// <summary>Shared RoomResStorage/Storage/StorageCrate state and reservations.</summary>
public sealed class RoomStorageSlot
{
    public GridCoord Cell { get; }
    public int Capacity { get; }
    public ResourceKind? Resource { get; private set; }
    public int Amount { get; private set; }
    public int ReservedPickup { get; private set; }
    public int ReservedSpace { get; private set; }
    public int ReservableResource => Math.Max(0, Amount - ReservedPickup);
    public int ReservableSpace => Resource is null ? 0 : Math.Max(0, Capacity - Amount - ReservedSpace);

    public RoomStorageSlot(GridCoord cell, int capacity)
    {
        Cell = cell;
        Capacity = Math.Max(1, capacity);
    }

    public bool SetResource(ResourceKind resource)
    {
        if (Resource is not null) return Resource == resource;
        Resource = resource;
        return true;
    }

    public bool ReservePickup(int amount = 1)
    {
        if (amount <= 0 || ReservableResource < amount) return false;
        ReservedPickup += amount;
        return true;
    }

    public void CancelPickup(int amount = 1) =>
        ReservedPickup = Math.Max(0, ReservedPickup - Math.Max(0, amount));

    public int Pickup(int amount = 1)
    {
        var picked = Math.Min(Math.Max(0, amount), Math.Min(Amount, ReservedPickup));
        Amount -= picked;
        ReservedPickup -= picked;
        return picked;
    }

    public bool ReserveSpace(int amount)
    {
        if (amount <= 0 || ReservableSpace < amount) return false;
        ReservedSpace += amount;
        return true;
    }

    public void CancelSpace(int amount) =>
        ReservedSpace = Math.Max(0, ReservedSpace - Math.Max(0, amount));

    public int Deposit(int amount)
    {
        var deposited = Math.Min(Math.Max(0, amount), Math.Min(ReservedSpace, Capacity - Amount));
        ReservedSpace -= deposited;
        Amount += deposited;
        return deposited;
    }

    public int DepositUnreserved(ResourceKind resource, int amount)
    {
        if (!SetResource(resource)) return 0;
        var deposited = Math.Min(Math.Max(0, amount), Capacity - Amount - ReservedSpace);
        Amount += deposited;
        return deposited;
    }

    public void RestoreContents(ResourceKind? resource, int amount)
    {
        Resource = resource;
        Amount = Math.Clamp(amount, 0, Capacity);
        ReservedPickup = 0;
        ReservedSpace = 0;
    }

    public (ResourceKind? Resource, int Amount) Clear()
    {
        var result = (Resource, Amount);
        Resource = null;
        Amount = ReservedPickup = ReservedSpace = 0;
        return result;
    }
}

public sealed class RoomStorageRuntime
{
    private readonly Dictionary<int, List<RoomStorageSlot>> _rooms = new();
    public bool HasRoom(int roomId) => _rooms.ContainsKey(roomId);

    public IReadOnlyList<RoomStorageSlot> Create(
        RoomInstanceRuntime room,
        IEnumerable<GridCoord> cells,
        int? capacity = null)
    {
        var slotCapacity = Math.Max(1, capacity ?? room.Blueprint.Rule.Storage);
        var slots = cells.Where(room.Contains).Distinct()
            .Select(cell => new RoomStorageSlot(cell, slotCapacity)).ToList();
        _rooms[room.Record.Id] = slots;
        return slots;
    }

    public IReadOnlyList<RoomStorageSlot> ForRoom(int roomId) =>
        _rooms.TryGetValue(roomId, out var slots) ? slots : Array.Empty<RoomStorageSlot>();

    public IEnumerable<(int RoomId, RoomStorageSlot Slot)> AllSlots =>
        _rooms.SelectMany(room => room.Value.Select(slot => (room.Key, slot)));

    public RoomStorageSlot? At(int roomId, GridCoord cell) =>
        ForRoom(roomId).FirstOrDefault(slot => slot.Cell == cell);

    public RoomStorageSlot? FindResource(ResourceKind resource) => _rooms.Values
        .SelectMany(slots => slots)
        .FirstOrDefault(slot => slot.Resource == resource && slot.ReservableResource > 0);

    public RoomStorageSlot? FindSpace(ResourceKind resource) => _rooms.Values
        .SelectMany(slots => slots)
        .FirstOrDefault(slot => (slot.Resource is null || slot.Resource == resource) &&
                                (slot.Resource is null ? slot.Capacity : slot.ReservableSpace) > 0);

    public RoomStorageSlot? FindResource(int roomId, ResourceKind resource) =>
        ForRoom(roomId).FirstOrDefault(slot =>
            slot.Resource == resource && slot.ReservableResource > 0);

    public RoomStorageSlot? FindSpace(int roomId, ResourceKind resource) =>
        ForRoom(roomId).FirstOrDefault(slot =>
            (slot.Resource is null || slot.Resource == resource) &&
            (slot.Resource is null ? slot.Capacity : slot.ReservableSpace) > 0);

    public IReadOnlyList<(ResourceKind Resource, int Amount, GridCoord Cell)> ClearRoom(int roomId)
    {
        var result = new List<(ResourceKind Resource, int Amount, GridCoord Cell)>();
        foreach (var slot in ForRoom(roomId))
        {
            var cleared = slot.Clear();
            if (cleared.Resource is { } resource && cleared.Amount > 0)
                result.Add((resource, cleared.Amount, slot.Cell));
        }
        _rooms.Remove(roomId);
        return result;
    }

    public int AvailableSpace(int roomId, ResourceKind resource) => ForRoom(roomId)
        .Where(slot => slot.Resource is null || slot.Resource == resource)
        .Sum(slot => slot.Resource is null ? slot.Capacity : slot.ReservableSpace);

    /// <summary>Reserves output capacity across as many room slots as required.</summary>
    public IReadOnlyList<(RoomStorageSlot Slot, int Amount)> ReserveSpace(
        int roomId,
        ResourceKind resource,
        int amount)
    {
        var reservations = new List<(RoomStorageSlot Slot, int Amount)>();
        var remaining = Math.Max(0, amount);
        foreach (var slot in ForRoom(roomId)
                     .Where(slot => slot.Resource == resource)
                     .Concat(ForRoom(roomId).Where(slot => slot.Resource is null)))
        {
            if (remaining == 0) break;
            if (!slot.SetResource(resource)) continue;
            var reserved = Math.Min(remaining, slot.ReservableSpace);
            if (reserved <= 0 || !slot.ReserveSpace(reserved)) continue;
            reservations.Add((slot, reserved));
            remaining -= reserved;
        }
        if (remaining == 0) return reservations;
        foreach (var reservation in reservations)
            reservation.Slot.CancelSpace(reservation.Amount);
        return Array.Empty<(RoomStorageSlot Slot, int Amount)>();
    }

    public void CancelSpace(IEnumerable<(RoomStorageSlot Slot, int Amount)> reservations)
    {
        foreach (var reservation in reservations)
            reservation.Slot.CancelSpace(reservation.Amount);
    }

    /// <summary>Consumes existing reservations and returns the amount not deposited.</summary>
    public int Deposit(
        IEnumerable<(RoomStorageSlot Slot, int Amount)> reservations,
        int amount)
    {
        var remaining = Math.Max(0, amount);
        foreach (var reservation in reservations)
        {
            var accepted = reservation.Slot.Deposit(Math.Min(remaining, reservation.Amount));
            remaining -= accepted;
            var unused = reservation.Amount - accepted;
            if (unused > 0) reservation.Slot.CancelSpace(unused);
        }
        return remaining;
    }

    /// <summary>Deposits without a prior job reservation and returns the remainder.</summary>
    public int DepositUnreserved(int roomId, ResourceKind resource, int amount)
    {
        var remaining = Math.Max(0, amount);
        foreach (var slot in ForRoom(roomId)
                     .Where(slot => slot.Resource == resource)
                     .Concat(ForRoom(roomId).Where(slot => slot.Resource is null)))
        {
            if (remaining == 0) break;
            remaining -= slot.DepositUnreserved(resource, remaining);
        }
        return remaining;
    }

    public int Total(ResourceKind resource) => _rooms.Values.SelectMany(slots => slots)
        .Where(slot => slot.Resource == resource).Sum(slot => slot.Amount);
}
