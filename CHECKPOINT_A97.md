# Checkpoint A97 — source-shaped child AI

This checkpoint replaces the daily child-school/nursery shortcut with the plan lifecycle from
`settlement.entity.humanoid.ai.types.child.AIModule_Child`.

## Ported behavior

- Child sleep is triggered by night, hunger, or exposure without consulting the adult race sleep flag.
- School and nursery places are reserved before pathfinding and released when a plan is cancelled.
- Children now walk to the selected operational room instead of merely displaying a school/nursery state.
- School study advances in five-second resumer steps and applies education only after the child work period.
- Nursery attendance runs in five-second steps with the original 120-second play cycle.
- Failed school/nursery activation sets the original two-day timeout.
- Three failed school days allow an adult-age child to grow up, matching `schoolTimeoutDay > 2`.
- Daily population advancement no longer grants remote school or nursery results.

The Godot project was not compiled in this environment; `Tools/verify_port.py` is the static gate.
