# A95 — BuildMain construction and jobs menus

This checkpoint replaces the two placeholder bottom-bar actions with the menu
branches and ordering from `view.sett.ui.bottom.BuildMain`.

## Ported

- The bottom build bar now has the six original branches in source order:
  Agriculture, Work, Service, Government, Construction and Jobs.
- Only the selected first column opens. A second column opens on hover for a
  subcategory; Construction uses this for DECOR.
- Government no longer contains `MAIN_INFRA.misc` or DECOR. They are in the
  Construction branch, matching the arguments passed to `BuildMain.append`.
- Construction order is: move throne, infrastructure misc rooms, fences,
  roads, structures, decor and fortifications.
- Jobs order is: forage, hunt, fell trees, clear rock, clear all, remove water
  and dig tunnel. Return-water and cave-fill are excluded as in Java.
- Roads select the existing real road placer.
- Forage, tree clearing, rock clearing, water removal and tunnel digging are
  distinct jobs performed by citizens. `JobClear` work times and repeated
  forage/tree steps follow the Java control flow.
- Terrain jobs were appended to `BuildKind`, preserving all persisted enum
  values used by older saves.

## Deliberately not substituted

The following rows are visible in their correct source positions but report
the missing dependency instead of silently invoking an unrelated tool:

- move throne — needs relocation of the existing throne room;
- fences — needs a separate `JobBuildFence` representation;
- structures — needs the `JobBuildStructure` material/wall/roof/convert selector;
- fortifications — needs `JobBuildFort`, fortification terrain and stairs;
- manual hunt — needs wild-animal entities and `huntMark` state.

These are subsequent semantic ports, not aliases for walls or hunter rooms.

## Verification

`python3 Tools/verify_port.py` returns `PORT_STATIC_VERIFICATION_OK`.
Per project workflow, no Godot/.NET compilation was run.
