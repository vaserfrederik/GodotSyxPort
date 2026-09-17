# Project completion estimate

This estimate separates three different meanings of “finished”. A single
percentage would hide the difference between a scalable engine foundation, a
playable settlement slice and feature parity with the complete source game.

| Scope | Current estimate | Evidence |
|---|---:|---|
| Godot simulation foundation | 72–80% | 768×768 dense terrain, fixed ticks, hierarchical paths, jobs, data-oriented citizens, room/data catalogs, logistics, technology, law, governance, local military and settlement events exist; runtime profiling, portal invalidation and full asset renderer remain |
| Playable settlement/economy slice | 49–57% | construction, production/services, trade, health, housing, childhood, education, law, administration, nobility, local military and major settlement events exist; battle control remains |
| Full Songs of Syx mechanical parity | 10–14% | Target C# runtime files consolidate reviewed legacy classes. The Godot HUD, layered settlement map, shared inspector, event feed, administration dashboard, 112-room build palette and first peaceful strategic-world slice exist; rendered battles remain absent |

These are engineering estimates, not percentages derived by dividing file
counts. Godot replaces entire Snake2D/LWJGL groups, while some single original
classes represent large intertwined systems.

## Remaining large milestones

> **Deferred-final boundary:** settlement/world combat runtime, invasions, battle
> formations and combat resolution for riots/uprisings are intentionally postponed
> until all non-combat settlement mechanics and their UI are assembled. They must not
> be selected by intermediate macro batches.

1. Validate/build/profile in Godot 4 .NET and repair runtime-only issues.
2. Original resource logistics, stockpiles, room-local input/output storage,
   carry limits and compatible-job batching.
3. Terrain, water, fertility, vegetation, minerals, clearing and settlement
   generation on the full map.
4. Full citizen model: races, classes, stats, needs, employment, homes,
   relationships, health, deaths and AI plans.
5. Remaining room-specific mechanics plus terrain/job/path integration gaps.
6. Godot UI: settlement dashboard, room/citizen inspectors, build palette, notifications and localization. The full-grid global map is the base, not a literal LWJGL UI port.
7. Strategic world: region map, settlements, diplomacy and non-combat trade, built over the Godot global-map interaction layer.
8. Original visual/audio asset import, animation, effects, music and sound.
9. Save migration, mod/data compatibility, performance testing and parity QA.
10. Final deferred package: settlement/world battles, formations, invasions,
    artillery targeting and combat resolution for riots/uprisings.

## Reordered UI and world-map track

UI is now an active parallel milestone because the runtime has enough live state to inspect
and control. The implementation targets Godot `Control`/`CanvasLayer`, not legacy
Snake2D/LWJGL widgets.

1. **Operational HUD and inspect panels** — population, resources, selected room/citizen,
   jobs and construction/economy state are visible; clickable pause/×1/×5/×25/×250 controls
   and the shared cell-room-citizen inspector are active. Law/event notices and build-command
   widgets remain the next UI slice.
2. **Full settlement global map** — terrain/resource overlays, roads, water, rooms,
   camera navigation and map filters. `GlobalMapOverlay` now caches a 300×300 Godot texture,
   uses independent terrain/infrastructure/fertility/moisture/mineral revisions, supplies
   click-to-focus/selection markers and full/compact modes over 768×768 cells.
3. **Data-driven build/room UI** — the first slice now lists all 112 parsed definitions with
   categories, search, technology locks, construction data, services and recipes, and routes
   selection through `RoomPlacementRuntime`. Generic furnishing counts, technology-checked
   upgrade choice, live multi-resource cost and placement validation are also connected.
   Parsed furnisher-group costs/stats are exposed without invented item names. A shared room-policy
   panel controls staffing, recipes, tool targets and technology-checked upgrades. Visual furnisher
   shapes still require the original layout/sprite data.
4. **Citizen and administration UI** — the F2 dashboard reads population/housing/education,
   active law cases/facilities, treasury/governance, technology currencies and actual trade quotes/
   transactions. It edits race/class crime decrees, purchases technology levels through the existing
   currency/prerequisite runtime, and configures per-resource import/export thresholds. The resident
   browser lists every live identity with class/type, profession, age, home, religion, health, job and
   needs, then focuses its grid cell. The law page lists custody/sentence/facility state and focuses
   the accused citizen. The event feed consumes immutable runtime notices by sequence.
5. **Strategic world map** — the first non-combat runtime now uses the source 256×256 world,
   average region area 50, 64-faction ceiling, eight-region average realm and 3/4 initial fill.
   It owns region adjacency, capitals, realm spread, the seven source diplomacy stances, capital
   distance and market-to-settlement quote publication. The F3 Godot map selects regions and edits
   peaceful relations; it deliberately exposes no war command. The next slice loads all 21 explicit
   world-building files and four industry generators, retains source building levels/credit costs/
   boosts, simulates regional population targets and production, dispatches 50-unit tax caravans to
   capitals and publishes NPC capital stock as real settlement trade markets. F3 exposes building
   levels, regional stock and caravans. The current slice also assigns the source average of two
   production prospects per region at strengths 1–3, applies the `0.75 + 0.5×prospect` efficiency,
   enforces parsed kingdom-count `LESS` requirements, advances packed loyalty by at most 8/255 per
   day, and moves caravans one adjacent region per day along visible routes. Deeper biome-weighted
   occurrence now weights prospects with regional water/fertility/moisture. SANCTION, EXILE and
   MASSACRE apply the source loyalty/growth multipliers to the majority race. NPC stockpiles use
   `capital×0.25 + realm×0.15`, the source `9/resources` pile scaling, target/amount price clamps
   0.1–10 and ±0.4 buy/sell adjustments, enabling both imports and exports. F3 adds fertility,
   moisture, water, prospect and loyalty layers plus per-race edict controls. The current slice adds
   multi-race populations, exile migration, distant realm-edict accumulation, four-religion regional
   mixtures/opposition and a dense 256×256 terrain generator. Height, oceans/lakes, large/small rivers,
   mountains, forest, climate and fertility aggregate into regions; weighted roads connect the graph
   and caravans use Dijkstra routes. Fine landmarks and settlement-site selection remain.

## Iteration-scale forecast

- Current city/economy prototype becoming a robust playable alpha: roughly
  15–30 more large implementation batches.
- Broad settlement-layer parity without the strategic world/battle layers:
  roughly 80–150 large batches.
- Whole-game mechanical parity: likely 200–400 large batches plus sustained
  runtime testing. This range will narrow after the first real Godot build and
  after room/AI families are ported through reusable generic frameworks.
