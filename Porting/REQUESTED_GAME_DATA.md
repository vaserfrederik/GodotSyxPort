# Original game data request — fulfilled

The requested `init` and `text` trees were received and imported unchanged into
`Data/Original`. The list below is retained as provenance for the path mapping;
it is no longer an outstanding request for the current milestone.

The original request accepted either of these alternatives:

1. Preferred: `<Songs of Syx installation>/base/data.zip`.
2. If the game is a development/extracted install: the complete
   `<Songs of Syx installation>/zipdata/data/assets/` directory, archived as ZIP.

The Java path chain is:

- `PATHS.java`: mounts `base/data.zip`, selects `data`, then creates the
  `assets` root;
- `RoomInitData.java`: reads `assets/init/room/<ROOM_KEY>.txt`;
- `RoomsCreator.java`: discovers `WORKSHOP_*`, `REFINER_*` and other room keys;
- `Industry.java`: reads `INDUSTRIES`, `IN`, `OUT`, `PLAYER`, `AI_RATE` and
  `AI_RECOVERY` from each room file;
- `STRUCTURES.java` / `Structure.java`: reads
  `assets/init/settlement/structure/*.txt`;
- `Floors.java`: reads `assets/init/settlement/floor/*.txt`;
- `Config.java`: reads `assets/init/config/Sett.txt`;
- `NEEDS.java`: reads `assets/init/stats/need/*.txt`;
- `RESOURCES.java`: reads `assets/init/resource/*.txt`.

Minimum subset if `data.zip` cannot be uploaded:

```text
assets/init/config/Sett.txt
assets/init/stats/need/*.txt
assets/init/resource/**/*.txt
assets/init/settlement/structure/*.txt
assets/init/settlement/floor/*.txt
assets/init/room/*.txt
assets/text/room/*.txt
assets/text/resource/**/*.txt
assets/text/settlement/structure/*.txt
assets/text/settlement/floor/*.txt
```

The supplied subset covers the gameplay definitions currently wired into the
runtime. A later asset-rendering stage may still need `base/data.zip` because
room definitions reference sprite sheets, textures and sounds transitively.
