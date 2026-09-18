# Checkpoint A99 — source terrain palette, ice and fixed build cascade

This checkpoint corrects the terrain and build-menu behavior against the Java sources and the supplied original-game screenshot.

## Source behavior restored

- `GroundTypes.java`: settlement ground has `NORMAL`, `FOREST`, `PASTURE`, `INFERTILE`, `ROCK` and `SAND`. Moisture is a separate value; it is no longer converted into the invented `Wet` terrain during generation.
- `CLIMATE.txt` / `Ground.java`: normal ground is tinted between the selected climate's original `GROUND.DRY` and `GROUND.WET` colors.
- `TWater.java` / `Water.png`: water uses the original atlas coordinates, source water colors and deterministic ice coverage instead of flat square cells.
- `SettColors.txt`: the minimap uses the original tree, water, deep-water and mountain colors and displays frozen water.
- `BuildMain.java` / `Inter.java`: opening a submenu keeps the main menu fixed and expands the child column to its right, as in the original UI.

## Important compatibility note

The removed `Wet` assignment affects newly generated settlements. Existing saves can still contain the legacy enum value and remain loadable, but a new game is required to validate generation parity.

## Verification

`python3 Tools/verify_port.py` returns `PORT_STATIC_VERIFICATION_OK`.

The project was not compiled, per the project workflow. Exact animated water scrollers and the complete Java snow sprite layer remain separate rendering-parity work; this checkpoint deliberately does not replace them with invented effects.
