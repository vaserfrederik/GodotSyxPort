# Checkpoint A89 — source RoomSprite construction placeholders

This checkpoint replaces the hunter furniture's solid per-cell preview blocks
with the Java `RoomSprite.renderPlaceholder` composition.

## Ported in this checkpoint

- `SheetType.cCombo.renderOverlay` four-bit N/E/S/W adjacency masks.
- `RoomSprite1x1.renderPlaceholder` single-cell mask handling.
- `UIConses.Big.filled` raw extraction from the original `sprite/ui/Cons.png`,
  using `ComposerSources.house` variant 9 coordinates.
- Existing placed furniture and the current E/Q variant are previewed together;
  changing group no longer replaces previously placed preview geometry.
- R rotation is applied before calculating combo neighbours.
- Valid source masks retain the blue placement tint and invalid cells use the
  red placement tint above existing geometry.
- Rooms without a ported RoomSprite mapping continue through the previous
  visible cell preview as an explicit fallback.

## Source correspondence

Hunter furniture tiles use `AVAILABILITY.ROOM_SOLID` in Java. Therefore their
placeholder selects `BIG.filled`, not the generic dashed outline. Final furniture
art remains the separate A88 `RoomSpriteCombo`/`RoomSprite1x1` path.

## Still required

- Map the other room constructors to their exact sprite types and source sheets.
- Port `RoomSpriteBoxN`, `RoomSpriteRot`, `RoomSpriteXxX`, 2x2 and 3x3 overlays.
- Port shadow halves, degradation layers, candle state and animated frames.

No Godot or C# compilation was run for this checkpoint.
