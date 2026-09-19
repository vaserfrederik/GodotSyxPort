# Building parity audit

This register is checked in source order. A building is marked complete only after its
constructor, placement workflow, construction jobs, runtime behaviour, menu branch and
source sprites have all been compared with the Java implementation.

| Order | C# key | Java source | Status | Verified behaviour |
|---:|---|---|---|---|
| 1 | `_HOME` | `settlement.room.home.house.HomeContructor`, `PlacerItemSingle` | Ported in A102 | Three house groups, source dimensions, size variants, rotation, outer structure, repeated entrances, `secretReplacementItem` section splitting, movable/applied blueprint states, compact fixed-item construction menu. |
| 2 | `_HOME_CHAMBER` | `settlement.room.home.chamber.Constructor`, `ChamberInstance`, `Work` | Ported in A103 | Single 6×7 fixed item, no house-section splitting, source furniture footprint, four required servants, one noble occupant, chamber preview and source sprite sheets. |
| 3 | Farms and orchards | `settlement.room.food.farm`, `settlement.room.food.orchard` | Next | Area rules, fertility, crops, workers, seasonal behaviour, construction menu. |
| 4 | Pastures | `settlement.room.food.pasture` | Pending | Indoor/outdoor variants, livestock, fence/opening rules, workers. |
| 5 | Hunter and fishery | `settlement.room.food.hunter`, `settlement.room.food.fish` | Pending | Source item matrices, jobs, output, preview and built sprites. |
| 6 | Mines | `settlement.room.industry.mine` | Pending | Mineral-only placement, employment and output. |
| 7 | Refineries and workshops | `settlement.room.industry.refiner`, `settlement.room.industry.workshop` | Pending | Input/output industry, workstations, auxiliaries, upgrades and bonuses. |
| 8 | Storage and logistics | stockpile, hauler, import, export, transport | Pending | Crates, fetch/deliver rules, radius and worker controls. |
| 9 | Civic and services | health, hygiene, food service, entertainment, education, law | Pending | Service slots, supplies, employment, access and menus. |
| 10 | Religion, burial, military and monuments | spirit, military, monument constructors | Pending | Fixed/area placement variants, dedicated jobs, sprites and UI. |

The terrain renderer and global construction-category hierarchy remain separate audits;
they are not considered correct merely because an individual building passes this table.
