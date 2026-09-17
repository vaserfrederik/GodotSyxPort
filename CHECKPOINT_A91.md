# Checkpoint A91 — source construction statistics and Russian labels

This checkpoint fixes the construction panel values that previously displayed
raw `ITEMS.STATS` accumulators instead of the Java `FurnisherStat.get` results.

## Ported in this checkpoint

- Hunter workers, furniture efficiency and projected output, including the
  Java `MAX_EMPLOYED` overhunting reduction.
- Fishery workers from shallow-water area and fish amount, storage capacity
  using `RoomResStorage(0b011111).max()`, auxiliary efficiency, deep-water
  access and projected production.
- Validation now uses the same source-derived placement statistics shown in
  the construction panel.
- The selected furniture preview shows the exact change and resulting value,
  rather than its unprocessed raw stat vector.
- Russian construction names for the hunter and fishery rooms, their furniture,
  statistics and construction resources.
- Russian hunter/fishery entries and resources in the room build palette.

## Source correspondence

- `settlement.room.food.hunter.Constructor`
- `settlement.room.food.hunter.ROOM_HUNTER.eBonus`
- `settlement.room.food.fish.Constructor`
- `settlement.room.main.job.RoomResStorage.max`
- `settlement.room.main.furnisher.FurnisherStat`

The hunter race bonus remains neutral in the planning estimate until the port
has a player-race selection connected to room construction statistics.

No Godot or C# compilation was run for this checkpoint.
