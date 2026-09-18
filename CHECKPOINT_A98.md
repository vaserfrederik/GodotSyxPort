# Checkpoint A98 — source terrain flood and fixed build cascade

## Settlement terrain

- `GeneratorOcean.generateWater` now keeps Java's separate radial distance and queue value.
- Ocean height cost is `margin * height^3`; it is no longer cumulatively added with a `0.08` multiplier.
- The capital area's water table is derived from the selected 3x3 world's average moisture using
  `0.05 + moisture * 0.1`, matching `CapitolArea.getWatertabe()`.
- Small rivers retain the source four-tile expansion scale while large rivers use the source
  twenty-tile scale.
- Settlement water texture samples only the 128x128 payload of `textures/Water.png`; the transparent
  six-pixel composer border no longer appears as straight bright grid lines.

## Build menu

- Opening a submenu no longer rebuilds or moves the main category column.
- The room submenu opens on the left and is populated on hover or click.
- Housing remains in the source `Service -> Home` category and exposes `_HOME`, `_HOME_CHAMBER`, and
  `RESTHOME_*` blueprints.

The project was not compiled here. `Tools/verify_port.py` is the static verification gate.
