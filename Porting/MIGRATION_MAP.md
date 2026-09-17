# Migration map

| Legacy subsystem/classes | Godot project target | Status |
|---|---|---|
| `settlement/main/SETT.cs` | `Scripts/Settlement`, simulation root | indexed; semantic extraction pending |
| `settlement/tilemap/**` | `WorldGridData`, chunk renderers | scalable base implemented; terrain layers pending |
| `settlement/job/JobBuild`, `JobBuildRoad`, `JobBuildStructure` | `Scripts/Simulation`, `Scripts/Jobs`, `Scripts/Construction` | road time + physical material delivery implemented; terrain clearing pending |
| `settlement/path/**` | `Scripts/Navigation` | local/hierarchical A* + pooled active paths implemented; portal cache pending |
| `settlement/entity/humanoid/**` | citizen simulation data, AI and rendering | node-free agents + batched rendering implemented; legacy AI extraction pending |
| `StatsNeeds`, `StatsFood`, `AIModule_Food`, `F_PlanEat`, `F_PlanStarve`, `AIModule_Work` | citizen hunger/food planning | chunk thresholds, priorities, eating and starvation ported; terrain/corpse fallback pending |
| `settlement/room/main/placement/**` | `RoomPlanner`, zones, perimeter and openings | base room planning flow implemented; irregular areas/furniture pending |
| placement preview/validation | transient chunked area/perimeter/door overlays | commit-safe preview implemented |
| `settlement/room/main/furnisher/**` | furniture jobs and room requirements | base furniture construction implemented; per-room layouts pending |
| workshop `Job`, `WorkshopInstance`, `Industry`, `IndustryResource`, `RoomResDeposit` | production jobs + resource ledger | source recipes, multi-input reservation, 45-second fractional rate accumulation and per-recipe progress implemented; boosts pending |
| `RoomResDeposit.jobResourceBitToFetch`, `jobResourcesNeeded`, packed `AMOUNTS` | `ProductionSupply` + per-workstation input buffers | physical Storage→workstation delivery, max fetch 15, capacity 31 and one-of-each gate implemented |
| `init.resources.RESOURCES`, room `INDUSTRIES` alternatives | 42-value `ResourceKind`, data-driven recipe selection | all source keys mapped; 2 Bakery and 5 Carpenter definitions selectable; acquisition chains pending |
| resource pickup/storage flow | loose-resource registry + hauling jobs + Storage capacity | base delivery loop and save/load implemented |
| room employment/work roles | automatic Laborer/Baker/Carpenter allocation + profession-filtered jobs | base staffing implemented |
| per-room employment controls | room picking + workstation-scaled worker limits | implemented; detailed room panel pending |
| job priority semantics | priority queues partitioned by 32×32 region | production/hauling/construction ordering implemented |
| remaining `settlement/room/**` | additional production and service behavior | indexed; extraction pending |
| `init/religion/**`, `spirit/shrine/**`, `spirit/temple/**` | `ReligionRuntime`, `TempleRuntime`, shared room service | four affiliations, religion-filtered visits, prayer timing and resource/animal altar supply implemented; conversion and prisoners pending |
| `game/time/**` | simulation clock/tick scheduler | indexed; pending |
| `game/save/GameSaver`, `AutoSaver` | versioned save/load | v12 snapshot, 42 resources, recipes/fractions, workstation buffers/supply jobs, recovery and autosave implemented |
| economy/resource history | `ResourceLedger`, `EconomyTracker` | totals + bounded 60-second samples implemented |
| `init/text` data trees | `Data/Original`, `SyxDataParser`, `OriginalGameData` | 890 files imported; core config/resources/structures/floors/rooms parsed at runtime |
| `snake2d.util.misc.CLAMP`, `snake2d.util.bit.Bits` | `Scripts/LegacyCompat` | compile-safe semantic ports integrated; per-file status in `legacy_compile_manifest.json` |
| `view/**`, room/map overlays | Godot `CanvasLayer` HUD, inspector, time controls, `RoomBuildPalette`, `RoomPolicyPanel`, `SettlementNotificationFeed`, `AdministrationDashboard` and `GlobalMapOverlay` | construction/economy/room/citizen state, five cached 768×768 map layers, clickable speeds, event notices, demographic summaries, resident/case browsers with map focus, editable law/technology/trade policies, universal room policies and a searchable 112-room catalog are implemented; localization remains pending |
| `snake2d` rendering/OpenGL | Godot renderer | replacement; not ported literally |
| `world/region/Gen`, `RDDistance`, factions/diplomacy/trade manager; buildings, races/prospects/edicts/outputs/religions; NPC stockpile; shipments | `StrategicWorldRuntime`, `RegionalEconomyRuntime`, `StrategicWorldMap` | source-sized graph, realms/diplomacy, buildings/requirements, biome-weighted prospects, multi-race population/loyalty, adjacent exile migration, local and realm edicts, regional religion/opposition, production, routed caravans and bidirectional NPC stockpile markets implemented |
| `world/map/terrain/**`, core `world/map/road/**` | `StrategicTerrainRuntime`, `StrategicWorldRuntime`, `StrategicWorldMap` | dense 256×256 height/water/mountain/forest/climate/fertility generation, regional aggregation, source-weighted roads/bridges and route-aware caravans implemented; sprite sheets and manual/debug placers remain renderer/deferred work |
| `settlement/battle/**`, battle maps | excluded from current city scope | deferred |
| `CRIMES`, `CRIME_PUNISHMENTS`, `stats/law/**`, `room/law/**`, crime/guard/prisoner AI plans | `SettlementLawRuntime`, citizen identity transitions and generic physical jobs | 12 crimes and seven punishments loaded; reporting/capture, court hearing, curfew, decrees, 32-day prison, stocks/stockade/execution, law/tyranny and guard/police coverage implemented; animations deferred |
| player credits/levels/titles, `game/nobility/**`, admin/embassy/monument/throne rooms | `SettlementGovernanceRuntime`, generalized `KnowledgeRuntime` | treasury categories/inflation/history, 14 levels, 34 titles, administration/diplomacy production, singleton throne, monuments and noble offices/ranks implemented; external royal courts remain world-layer work |
| military training/supply/artillery rooms, recruit plans, local `Div/DivInfo/DivMen` | `SettlementMilitaryRuntime`, logistics and citizen identities | 200-person/120-division limits, enlistment, barracks/archery progression, equipment targets, 80-unit military depots and six-person artillery loading implemented; battlefield targeting and invasions remain deferred |
| `game/event/engine/**`, event actions, citizen/disaster/killer/slave events, rioter AI | `SettlementEventRuntime`, citizen event adapters | condition/choice/action grammar, cooldowns/notices, emigration, 1.5-day strikes, delayed riots, epidemics, extreme temperature, serial murders and slave uprisings implemented; strategic-world actions and message UI remain explicit deferred adapters |

Combat formations, invasions, artillery targeting and battlefield resolution of riots
and slave uprisings are a protected final-stage backlog. Until that stage, event rosters
remain simulation state and are not expanded into battle entities.

The non-combat humanoid AI core plus consume/danger/idle/subwalk/work/service plans
are consolidated into the dense `CitizenSystem`, `CitizenSocialRuntime` and shared
job/service adapters. Adults select nearby partners by race preference, remember friends,
perform 5–15 second interactions and retain decaying pair relationships. Retiree,
Deranged and Tourist lifecycles remain separate pending packages.


`ROOM_WATER`, pump/canal/drain/pool instances, `RoomIrrigated` and their updater now
map to `WaterInfrastructureRuntime` and `WorldGridData` moisture. Orthogonally connected
water infrastructure forms a finite-flow network: a staffed pump retains maximum output
100 and degradation factor 0.8, canals irrigate local cells, drains use radius 10, and
updates process at one tile per second. Farm production consumes the resulting moisture
alongside fertility and weather.

`ROOM_GATE` has a lightweight `GateRuntime`: operational gate cells and their
subject-lock state are retained independently of the deferred enemy/battle path rules.

`ROOM_STATION`, crates, tallies and jobs reuse `SettlementLogisticsRuntime`: each crate
holds 400 units and an operational station has the source limit of 15 workers. Its local
storage, resource selection and reservations are active; dispatch beyond the settlement
is deferred with the strategic world.

`ROOM_INN`, inn beds, `ROOM_RESTHOME` and resthome jobs share `HospitalityRuntime`.
Occupancy is a real per-citizen reservation, inn staffing retains the source 1 worker per
8 beds rule, and a retired citizen cannot claim ordinary work while holding a resthome place.
Tourist arrival, reviews and strategic tourism revenue stay deferred.

`ROOM_BUILDER` and `ROOM_RADIUS` are consolidated in `BuilderInfrastructureRuntime`:
each office retains source default radius 32, a configurable byte-sized radius and maximum
20 employees. This is intentionally state-only until construction routing is separately
integrated, preserving existing global construction jobs.

`ROOM_ASYLUM` now admits/releases real `DERANGED` citizen identities through reserved
cells, while retaining the original ration and treatment contract. `_CANNIBAL` is linked
to the law pipeline: `HARVEST` reserves a race-permitted cage, waits two source days,
runs its 45-second guard job and grants exactly that race's parsed `RESOURCE` outputs.
It is no longer treated as a normal execution.

Godot-native UI work has begun independently of the legacy LWJGL interface. The HUD remains
live; `GlobalMapOverlay` caches terrain, infrastructure, fertility, moisture and mineral
textures over the full 768×768 grid. Per-layer revisions prevent moisture updates from
rebuilding unrelated layers. It displays camera/selection markers and supports layer
buttons, click-to-focus and M expand/collapse. A shared inspector exposes the selected
cell, room recipe/employment/degradation and nearest citizen identity/health/needs, while
Godot buttons drive pause and all four source simulation speeds.

`RoomBuildPalette` reads `RoomBlueprintCatalog` directly. Its category/search list therefore
covers all 112 unchanged room definitions, reflects technology locks and displays parsed area
costs, indoor requirements, upgrades, services and recipes. Generic furnisher group quantities,
technology-checked upgrade selection, live cost totals and validation are connected to the
shared `RoomPlacementRuntime`; area, perimeter, door validation and construction remain
simulation logic rather than UI-specific copies. Visual furnisher shapes and room policies are
the next palette layer.

`STAT`, `STATData`, `StatsEvent`, `StatDecree`, stat boosters and JSON stat metadata now
share `SettlementStatRegistry`. It stores named values, race/class projections, event flags,
bounded 48-day history and clamped decrees. Existing settlement, standing and event runtimes
write into this shared registry; UI graph rendering remains deferred.

## Source archive note

`GodotSyxProject(2).zip` contains 2,465 `.cs` files. Compared with the previous archive, `snake2d/GlHelper.cs` is absent. The previous copy is a corrupted OpenGL/export artifact with Markdown fencing and large repeated blocks; it is not required by the Godot renderer and is recorded as an intentional renderer replacement rather than imported code.

The repaired archive still contains generated placeholders: `JobBuildStructure.cs`, for example, is only a `Hello, World!` `Program` class. `source_recovery_required.json` tracks every file whose content must eventually be recovered from the original/decompiled game rather than trusted as translated source.

The provided source JAR resolves 161 of those 184 entries. All 23 unmatched entries are `META-INF` or external `org.lwjgl.system.jemalloc` wrappers and are excluded from the Godot implementation. `source_recovery_map.json` is the authoritative mapping.
