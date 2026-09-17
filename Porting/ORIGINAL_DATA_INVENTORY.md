# Original data inventory

The supplied `init` and `text` trees are stored unchanged under
`Data/Original/`. The runtime reads the source game's relaxed text format
directly; these files are not converted into invented Godot resources.

## Imported files

| Group | Count | Runtime use |
|---|---:|---|
| All `init` + `text` files | 890 | Source archive retained in project |
| Resource definitions | 42 | Resource key catalog and recipe mapping |
| Structure definitions | 6 | Wall resource, quantity and work time |
| Floor definitions | 24 | Default road, resource cost, speed and durability |
| Room definitions | 112 | Room catalog, storage and industry recipes |

## Values wired into the current vertical slice

| Source path | Read value | Current use |
|---|---|---|
| `init/config/Sett.txt` | 48 seconds/hour, 24 hours/day, dimension 768 | Time metadata and the original 768×768 settlement dimension are used by runtime |
| `init/stats/need/_HUNGER.txt` | `RATE: 0.5` | Loaded by the data registry; final cadence still depends on the humanoid update scheduler |
| `init/settlement/structure/STONE.txt` | 1 Stone, `BUILD_TIME: 0.1` | Stone wall construction job |
| `init/settlement/floor/_DEFAULT_ROAD.txt` → `DIRT.txt` | no material, speed 1, durability 0.2 | Default road material requirement |
| `init/room/REFINER_BAKERY.txt` recipe 0 | 6 Grain + 1 Wood → 6 Bread, storage 350 | Bakery production job; Bread maps to runtime Food |
| `init/room/WORKSHOP_CARPENTER.txt` recipe 0 | 2 Wood → 0.5 Furniture, storage 50 | Fractional source rate accumulated by the room |

`IndustryResource` converts recipe rates into per-second fractional production.
The current job loop now uses the exact source factor and the 45-second
`RoomResDeposit` cycle, retaining fractional room progress and emitting or
consuming only newly crossed whole units. Room/degradation/skill boosts remain
a separate semantic-port step.
