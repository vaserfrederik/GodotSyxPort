# External source data status

The supplied source and binary JARs contain code, but not every data definition
loaded by that code. The current port must not infer these values and call them
original behavior.

| Subsystem | Keys/data read by Java | Current status |
|---|---|---|
| Settlement time | `assets/init/config/Sett.txt`: `SECONDS_PER_HOUR`, `HOURS_PER_DAY` | **Received and parsed.** Scheduler parity remains pending. |
| Hunger | `assets/init/stats/need/_HUNGER.txt` and race modifiers: `RATE` | **Base need received and parsed.** Race modifiers/cadence remain pending. |
| Structures | `assets/init/settlement/structure/*.txt`: `RESOURCE`, `RESOURCE_AMOUNT`, `BUILD_TIME` | **Received and parsed.** Stone wall uses source values. |
| Floors/roads | `assets/init/settlement/floor/*.txt`: `ROAD.RESOURCE`, `RESOURCE_AMOUNT`, `SPEED`, `DURABILITY` | **Received and parsed.** Default Dirt road is selected through `_DEFAULT_ROAD`. |
| Industry rooms | `assets/init/room/WORKSHOP_*.txt` and `REFINER_*.txt`: `INDUSTRIES[].IN`, `OUT`, rates, boosts and layouts | **Received and parsed.** Bakery/Carpenter recipe 0 is wired into runtime. |
| Resource keys | `assets/init/resource/*.txt` | **Received:** 42 definitions catalogued. |

The previously requested gameplay data is now present under `Data/Original`.
No additional JSON/text upload is required for the current production slice.
Later visual/audio parity will require the matching original sprite sheets,
textures and sounds referenced by these definitions.

## Preferred upload

For future missing assets, the most reliable single source is the installed game file `base/data.zip`.
`PATHS.java` mounts that archive and then reads `data/assets/...`. A development
installation uses the equivalent extracted tree under
`zipdata/data/assets/...`. Supplying the complete `base/data.zip` avoids missed
indirect dependencies. Text/localization files can remain included; they will
later be needed to reproduce the original UI labels.
