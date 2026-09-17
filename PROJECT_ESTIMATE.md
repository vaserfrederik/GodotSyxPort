# Project completion estimate

## Final-stage exclusion

Battle runtime, invasions, formations and combat resolution for riots and slave
uprisings are explicitly deferred until the end of the project. Intermediate semantic
batches must continue with non-combat citizen, room, terrain, path, job and UI systems.

## Approved 12-stage roadmap (2026-09-09)

This sequence is authoritative for subsequent large batches. Converted C# remains the
extraction base, Java is consulted for damaged or ambiguous behavior, and original data
drives shared runtime systems. Existing code must be extended rather than replaced.

1. **Complete settlement terrain.** Consolidate caves, lakes and rivers, fish, snow,
   plant growth, terrain clearing, cave filling, fences and fortifications; connect them
   to construction and physical jobs.
2. **Pathfinding and object search.** Complete hierarchical components, passability
   invalidation and shared finders for homes, resources, services, water, maintenance
   and jobs instead of compiling the Java finder hierarchy literally.
3. **Physical objects and peaceful logistics.** Consolidate loose resources, rubbish,
   carts, transport entities, boats, local caravans and reservation-safe movement.
4. **Animals and ecology.** Add wild species, spawning, lifecycle/reproduction, hunting
   and their interaction with pasture and settlement terrain.
5. **Unified boost/effect runtime.** Route technology, race, religion, building, room,
   climate and government modifiers into live production, health, work, fulfillment and
   regional formulas.
6. **Remaining room fidelity.** Complete automatic employment, room copy/blueprints,
   exact furniture layouts, special service seats and remaining production states through
   the shared 112-room engine.
7. **Remaining citizen lifecycles.** Complete tourists, retirees, homeless behavior,
   terrain/corpse food fallback and remaining class/race identity integration.
8. **Complete the peaceful strategic map.** Add irregular regions, landmarks, havens,
   fog of war, settlement-site selection, regional problems, health, devastation and
   ownership transitions.
9. **Realms and peaceful diplomacy.** Add NPC courts, emissaries, trust, gifts, deals,
   vassal relations and peaceful faction expansion/collapse without battle simulation.
10. **Connect world and settlement.** Let the selected world region determine settlement
    climate/resources/population and route transport, trade, tourism and events through
    real world entities.
11. **Godot UI alongside every package.** Build only the controls and diagnostic views
    required by active mechanics; never port legacy `view/**` or LWJGL widgets literally.
12. **Final deferred combat package.** Only after stages 1–11: armies, invasions,
    formations, orders, artillery targeting, battles and combat resolution of riots and
    uprisings.

The first implementation macro-batch covers stages 1–3 and should consolidate roughly
120–200 related legacy classes into a small set of settlement runtime components.

## Godot launch checkpoints

- **Checkpoint A — now, before stage 1:** compile and launch `Main.tscn`, create a C#
  world session, select race/region and enter the settlement using procedural placeholders.
  Terrain/walls already use colored `BoxMesh` instances,
  citizens use colored `CapsuleMesh` instances, and UI/maps are drawn by Godot controls.
  This checkpoint fixes only compilation, startup, fatal initialization and camera/input
  blockers; it does not begin model, texture or sprite production.
- **Checkpoint B — after stage 3:** run a longer placeholder simulation to validate dense
  terrain, navigation invalidation, reservations, local logistics and frame time together.
- **Checkpoint C — after stage 7:** verify a complete non-combat settlement lifecycle and
  all required Godot control panels.
- **Checkpoint D — after stage 11:** establish the stable non-combat baseline before the
  final battle package.

Models, texture atlases, animated sprites, audio and visual polish remain replaceable
presentation layers and are not prerequisites for checkpoints A–C.

Updated after the multi-race strategic population, regional religion and dense
strategic terrain/road integration.

## Measured state

- Original converted tree: 2,465 C# files, about 350,130 lines.
- Active runtime: 107 C# files, 25,453 lines (including the restored C# application/world-selection flow, save v15, embassy-to-world diplomacy and peaceful NPC courts).
- Reviewed semantic adaptations: 900 source classes.
- Manifest remainder: 1,221 semantic ports, 159 recovery-required files and
  185 engine/runtime references.
- Original data already available: 890 unchanged `init/text` files, including
  42 resources, 24 floors and 112 room definitions.

## Completion bands

These percentages measure different targets and must not be merged into one
misleading number.

| Target | Current estimate | Remaining |
|---|---:|---:|
| Shared simulation foundation | 78–84% | 16–22% |
| First coherent peaceful settlement slice | 55–62% | 38–45% |
| Broad non-combat mechanics coverage | 38–46% | 54–62% |
| Mechanical parity with the supplied source tree | 24–32% | 68–76% |
| Full game parity including world, battle and UI | 12–16% | 84–88% |

The foundation is far ahead of the class count because map storage, jobs, resources,
citizens, data-driven rooms and data parsing replace many source classes. Remaining
weight is concentrated in path/finder fidelity, physical map entities, boost wiring,
peaceful world depth, Godot UI parity and the explicitly deferred battle layer.

## Acceleration strategy

These rules are mandatory for subsequent implementation batches:

1. Port macro-subsystem batches of 100–250 related source classes behind a few runtime
   interface; never mechanically add individual converted classes to compilation.
2. Prefer generic data-driven room/service/industry engines so the 112 original
   room definitions become content, not 112 independent rewrites.
3. Keep Java as semantic authority and converted C# as the extraction base;
   consult Java only where conversion is damaged or ambiguous.
4. Separate simulation from Godot rendering and UI. Use only the four recorded
   integration checkpoints; otherwise keep verification static across large batches.
5. Generate dependency and source-value audits automatically, then reserve manual
   work for behavior and architecture.
6. Delay low-value screens, debug placers, platform/LWJGL adapters and exact save
   compatibility until the corresponding gameplay subsystem exists.
7. Use the approved 12-stage roadmap above as the only milestone order.
8. Do not spend the current consolidation phase on save compatibility or visual polish.

`Tools/analyze_source_dependencies.py` regenerates
`Porting/source_dependency_map.json`. The map groups Java and converted C# by
subsystem, includes manifest states and ranks cross-subsystem dependencies. It is
the source for selecting large related batches. `Tools/plan_semantic_batch.py`
materializes the next reviewed queue while separating deferred UI/render files.

## Active batch

The active macro batch has reached the peaceful strategic layer: regional populations
are split by all source races, race targets use source climate/terrain weights, exile
moves people into acceptable adjacent regions, realm persecution accumulates and decays,
and the four source religions form a slowly changing mixture with pairwise opposition.
The world now also owns a dense 256×256 terrain grid with height, component oceans/lakes,
rivers, mountains, forest, climate and fertility. Weighted roads connect the regional
graph and caravans follow those routes. F3 exposes the terrain and demographic layers.
Legacy UI, sprite definitions, seasonal terrain details and debug placers remain outside
this consolidation stage.

The 768×768 settlement now owns dense elevation, ground, moisture, fertility,
water/deep-water, mountain/cave and six-type mineral layers. Generator stage order follows
the source coordinator; mineral terrain weights come from the unchanged source data.
Farm output reads area fertility, while mine work positions and quality read matching
deposits. Exact source lake/river shapes, fish, edibles and regional world inputs remain
inside this same terrain milestone rather than being replaced with UI work.

The terrain slice now also stores groundwater distance, salt-water identity,
fish density/spots and seven typed wild growables. Growth and edible placement use
the original moisture exclusion, mineral exclusion, growth-value and climate-bonus
inputs. Ocean entry is represented as explicit strategic-region sides; the neutral
temporary region does not silently invent an ocean.

The source climate catalog and simulation-only weather state are active. Temperature,
downfall, moisture/drought, snow, ice, wind, clouds, growth and ripeness advance from
the settlement clock; moisture feeds the shared agriculture multiplier. Foundation is
stored in its original two-bit 0–3 range, with rock retaining its special 0.5 value.
Rain-event scheduling, thunder presentation and downfall rendering remain deferred.

The population identity milestone is active, not further terrain expansion. All
eight source races load physical/lifecycle properties, population weights, food and
work preferences, inter-race attitudes, traits, pronouns and source name sets. Each
live subject owns stable race/class/type/origin/name/gender/parent/birth identity;
work assignment respects whether its humanoid type works and orders candidates by
their race preferences. Initial bootstrap citizens now use the race selected on the
strategic world screen.

Entry and migration are now an active simulation boundary. Border entry points are
indexed and recompute reachability after navigation changes; isolation or siege closes
both population and trade access. Per-race/type incoming queues spawn one physical
subject at a reachable edge per source-time unit. Immigration attraction retains the
source 0.5 happiness threshold, population/standing blend, auto-admit cap and negative
attraction emigration debt. Emigrants release housing, walk to an entry and leave
without becoming corpses. Happiness supplies its live input in the later standing stage.

Age and childcare now form one live population lifecycle. Each race uses its unchanged
`BABY_DAYS` and `CHILD_DAYS`; a parent produces a separate child after the infant period,
and that child later advances to subject/slave on settlement days. Reproduction projections expose infant, child,
adult and fertile counts plus source-year growth. Nursery capacity is ten children per
workplace per day, while breeder rooms retain fractional eight-day Garthimi incubation
and consume two Meat for each completed child. The parenthood admission hook is live;
Natural conception now uses the source four checks per 16-day year, fertility age window,
base 0.1 speed, population scaling and race multipliers. Children have separate sleep,
play, nursery, school and leaving states and cannot claim ordinary labor jobs.
School now preserves the source childhood education/indoctrination policy, default
16/100 limit, 0.1 Paper consumption, room-quality learning speed and three failed-day
escape from growth delay. Children of an emigrating parent inherit the departure plan.

The active milestone has advanced to happiness/loyalty/fulfillment/standing. Its shared
core now preserves source expectation scaling (`40000 × 0.7143`), fulfillment exponent
2.65, race occurrence adjustment, 100-day loyalty convergence and the eight-day emergency
buff. All eight race `STATS` catalogs now load their class maxima, inversion, multiplier,
exponent, priority, dismissal and child flags. Existing `STORED_*` per-capita values,
home access, starvation and unburied-corpse pressure feed the engine. Every visited
`SERVICE_<room>` blueprint is tracked separately (rather than being collapsed into one
need), so existing room services now provide real fulfillment; environmental and
unported social-stat producers remain deferred.

The sleep/home-resource milestone is complete at the shared-runtime level. `RaceHome`, `RaceHomeClass` and
`RaceHomeSheet` are represented by one catalog that scans all eight unchanged
`init/race/home` definitions and preserves the source limit of eight distinct
resources per race/class. Each resident owns current furniture and wear debt;
targets remain explicit policy values, just as in `StatsHome`, and create physical
hauling jobs through the shared supply pipeline. Wear uses the source 16-day year,
`0.5 / furniture boost` annual rate and home-isolation penalty. Adult subjects now
execute common home/chamber/ground sleep states with the source age/day staggering,
5–9 action cycles, noble eight-cycle bed stay and unhoused fallback. `HOME_HOUSED`
and `HOME_FURNITURE` are calculated per race/class before entering standing.

The special-production milestone is now active in the runtime. Ten unchanged room
definitions cover six pasture species, fishery, two hunter recipes, fruit orchard and
woodcutter. `SpecialProductionRuntime` preserves their distinct work durations,
pasture animal capacity/tending/livestock delivery and multi-output production,
hunter overstaffing and annual 0.6–1.4 luck, fish-spot access, 64-day orchard growth
with the three-day ripe window, and vegetation-dependent chopping. These rooms reuse
the common employment, physical hauling, output storage, equipment, degradation and
terrain multipliers instead of becoming ten independent implementations.

The logistics/economy milestone is now active as one shared runtime rather than six
independent room ports. Stockpiles use the original 80/300/500 units per crate by
upgrade; haulers, export/import depots, markets and transports retain their source
capacities, pull radii, worker formulas, resource selection, priorities and bounded
physical transfers. Market visits consume real race-home wares. Transport carts require
400 loaded units and 576 source seconds of preparation. The settlement trade layer
preserves the 400 reference price, price caps, 25% default export reserve, tariffs and
distance tolls; it remains safely dormant until a future strategic-faction layer injects
real partner quotes instead of inventing trade partners.

The knowledge/technology milestone is also active. All 403 technology nodes from the
11 unchanged source trees load with direct and transitive requirements, multi-currency
costs, increasing level costs, content locks and additive boosts. Laboratories and
libraries share one AdminData/RoomConsumption runtime with physical Clay/Leather input,
45-second work, 16-day degradation and squared saturation slowdown. Universities reuse
the personal-stat education tracks and apply the source `learningSpeed/16` update in 16
daily steps. Technology maintenance retains allocated/frozen currency, 0.8 forgetting
threshold and weighted penalties without importing the old UI/boost graph.

The service runtime is now connected to citizen AI: source daily attempt limits,
priority ordering, weighted selection, radius filtering, physical visits, reservation
cleanup and access/quality/proximity reporting are active generically. Room-specific
animations and effects remain separate follow-up subsystems.

The generic citizen-needs layer now loads all 20 source rates, maintains the
four 16-point chunks and applies one-chunk service fixes. Source BOOST keys,
access, quality and proximity are retained without hard-coding each room. Bath,
well and hearth exposure/cleanliness effects are connected; disease, religion,
combat and law consumers will read the shared state in their own milestone batches.

Health now has a source-data disease catalog, individual incubation/sickness/immunity,
fatality, regular sickness, injuries, hospital seeking/treatment and epidemic state.
Hospital Fabric/Opiates are physically supplied and consumed, affecting the original
recovery formula. Workplace accidents use the original employment debt scheduler,
shift window, radius and damage falloff. Death creates a reservable corpse and the
burial AI physically drags it to a formal grave or tomb first and uses a corpse dump
only as fallback. Slots remain occupied for the source 20/40/16 days, undertakers use
the source 0.1 worker-per-grave ratio, and disturbed graves decay on the source cadence.
Lavatories now become dirty after four uses and create 45-second cleaning work from
the third use. Race/class burial permissions, climate/terrain occurrence weights and
visual sick/in-bed/corpse states remain later layers. The next generated macro queue
contains 108 service/spirit/health/stat classes, including 17 recovery targets.

The practical near-term goal is to raise broad settlement coverage by completing
generic systems, not by maximizing the raw count of copied files.
