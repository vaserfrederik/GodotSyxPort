# Checkpoint A100 — Java ground moisture, tree palette and services menu

## Terrain

- `GeneratorGround` parity: per-tile `MOISTURE_BASE` now comes from the final local fertility field (`util.fer`), rather than storing strategic-world moisture directly.
- `GroundType` parity: the original diffuse ground atlas is multiplied by the climate colour, matching Java `COLOR.bind()` instead of replacing the atlas RGB with a synthetic luminance colour.
- `TForest` / `TColors` parity: trees are multiplied by the fertile row of the original `TreeColors.png` palette. The previously visible white/magenta atlas sprites are no longer left uncoloured.

## Build menu

- Opening Services no longer throws while looking for a nonexistent `Services / Misc` category.
- Category lookup returns an empty direct-room list when a Java menu category contains only expandable subcategories.
- The palette is raised above later HUD siblings, and its submenu scroll panel explicitly captures mouse input.

## Verification

`python3 Tools/verify_port.py` returns `PORT_STATIC_VERIFICATION_OK`.

The project was not compiled. A newly generated settlement is required to verify the corrected terrain baseline.
