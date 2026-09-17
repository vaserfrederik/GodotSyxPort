using System;
using System.Collections.Generic;
using static Settlement.Main.SETT;
using static Settlement.Main.SETT.FLOOR;
using static Settlement.Main.SETT.JOBS;
using static Settlement.Main.SETT.ROOMS;
using static Settlement.Main.SETT.TERRAIN;
using Game;
using Game.Audio;
using Game.Faction;
using Game.Faction.FResources;
using Game.Faction;
using Init.Race;
using Init.Sprite;
using Init.Value;
using Settlement.Entity.Humanoid;
using Settlement.Environment.SettEnvMap;
using Settlement.Main;
using Settlement.Path;
using Settlement.Stats.Standing;
using Settlement.Tilemap.Floor;
using Settlement.Tilemap.Terrain;
using Snake2D;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Clickable;
using Snake2D.Util.Sets;
using Util.Gui.Misc;
using Util.Info;
using Util.Rendering;
using Util.Text;
using View.Tool;

namespace Settlement.Job
{
    public sealed class JobBuildRoad : JobBuild
    {
        private static readonly string ¤¤Convert = "Convert";
        private static readonly string ¤¤ConvertD = "Convert existing roads into this type.";
        private static readonly string ¤¤durability = "Durability";

        static JobBuildRoad()
        {
            D.ts(typeof(JobBuildRoad));
        }

        public sealed class JobBuildRoads
        {
            public readonly LIST<JobBuildRoad> all;
            private readonly JobComboPlacer pla;
            private bool convert = false;

            public JobBuildRoads()
            {
                var all = new ArrayList<JobBuildRoad>(SETT.FLOOR().roads.size());
                foreach (Floor f in SETT.FLOOR().roads)
                {
                    all.add(new JobBuildRoad(f));
                }
                this.all = all;
                pla = new JobComboPlacer(this.all, "ROAD_TYPE");
            }

            public Job getPlacable()
            {
                return pla.current();
            }
        }

        private readonly Floor floor;
        private readonly Placer placer;
        private bool showRoads = true;

        private JobBuildRoad(Floor floor) : base($"ROAD_{floor.key}", floor.resource, floor.resAmount, false, floor.name, floor.desc, floor.getIcon())
        {
            this.floor = floor;

            var bs = new LinkedList<CLICKABLE>();
            bs.add(new Panel(SPRITES.icons().m.cog)
            {
                protected override void clickA()
                {
                    showRoads = !showRoads;
                }

                protected override void renAction()
                {
                    selectedSet(showRoads);
                    if (showRoads)
                        SETT.OVERLAY().ROADING.add();
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title(SETT.OVERLAY().ROADING.name);
                    text.text(SETT.OVERLAY().ROADING.desc);
                }
            });

            bs.add(new Panel(SPRITES.icons().m.arrow_right)
            {
                protected override void clickA()
                {
                    SETT.JOBS().roads.convert = !SETT.JOBS().roads.convert;
                }

                protected override void renAction()
                {
                    selectedSet(SETT.JOBS().roads.convert);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title(¤¤Convert);
                    text.text(¤¤ConvertD);
                }
            });

            placer = new Placer(this, floor.resource, floor.resAmount, floor.desc)
            {
                {
                    bs.add(bOverwrite);
                }

                public override void hoverDesc(GBox box)
                {
                    base.hoverDesc(box);
                    box.NL(4);

                    box.textL(STANDINGS.CITIZEN().fullfillment.info().name);
                    box.NL();
                    int ta = 0;
                    foreach (Race race in RACES.all())
                    {
                        box.add(race.appearance().icon.medium);
                        box.add(GFORMAT.perc(box.text(), floor.pref(race)));
                        box.space();
                        if (ta++ > 4)
                        {
                            box.NL();
                            ta = 0;
                        }
                    }
                    box.NL(8);

                    box.textL(Dic.¤¤Speed);
                    box.tab(5);
                    box.add(GFORMAT.percInc(box.text(), (floor.speed.movementSpeed - AVAILABILITY.NORMAL.movementSpeed)));
                    box.NL();

                    foreach (SettEnv e in SETT.ENV().map.all())
                    {
                        if (floor.envValue(e) != 0)
                        {
                            box.textL(e.info.name);
                            box.tab(5);
                            box.add(GFORMAT.perc(box.text(), floor.envValue(e)));
                            box.NL();
                        }
                    }
                    box.NL();
                    box.textL(¤¤durability);
                    box.tab(5);
                    box.add(GFORMAT.perc(box.text(), floor.durability));
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return bs;
                }
            };
        }

        public override PlacableMulti placer()
        {
            return placer;
        }

        public override void renderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty)
        {
            foreach (DIR d in DIR.ORTHO)
            {
                if (FLOOR().getter.is(tx, ty, d) || JOBS().getter.get(tx, ty, d) == this)
                    mask |= d.mask();
            }
            SPRITES.cons().BIG.dashed.render(r, mask, x, y);
        }

        protected override CharSequence problem(int tx, int ty, bool overwrite)
        {
            if (SETT.JOBS().roads.convert)
            {
                if (SETT.FLOOR().getter.get(tx, ty) == floor)
                    return PLACABLE.E;
                if (SETT.FLOOR().getter.get(tx, ty) == null)
                    return PLACABLE.E;
            }

            if (ROOMS().map.is(tx, ty))
                return PlacableMessages.¤¤ROOM_BLOCK;

            if (SETT.FLOOR().getter.get(tx, ty) == floor)
                return PlacableMessages.¤¤ROAD_ALREADY;

            if (!overwrite)
            {
                if (JOBS().getter.is(tx, ty))
                    return PlacableMessages.¤¤JOB_BLOCK;
            }

            if (SETT.TERRAIN().WATER.BRIDGE.is(tx, ty) || SETT.TERRAIN().WATER.DEEP.is(tx, ty))
                return lockText();

            if (SETT.PATH().solidity.is(tx, ty))
                return PlacableMessages.¤¤BLOCKED;

            if (!SETT.TERRAIN().get(tx, ty).roofIs() && !SETT.TERRAIN().get(tx, ty).clearing().can())
                return PlacableMessages.¤¤BLOCKED;

            if (SETT.FLOOR().getter.get(tx, ty) == floor)
                return PlacableMessages.¤¤ROAD_ALREADY;

            return lockText();
        }

        public static CharSequence problem(int tx, int ty)
        {
            if (ROOMS().map.is(tx, ty))
                return PlacableMessages.¤¤ROOM_BLOCK;

            if (SETT.FLOOR().getter.get(tx, ty) != null)
                return PlacableMessages.¤¤ROAD_ALREADY;

            if (SETT.PATH().solidity.is(tx, ty))
                return PlacableMessages.¤¤BLOCKED;

            if (!SETT.TERRAIN().get(tx, ty).roofIs() && !SETT.TERRAIN().get(tx, ty).clearing().can())
                return PlacableMessages.¤¤BLOCKED;

            return null;
        }

        public override CharSequence lockText()
        {
            if (floor.resource != null)
            {
                if (GAME.player().res().amount(floor.resource) < floor.resAmount)
                    return $"Not enough {floor.resource.name}";
            }
            return null;
        }

        protected override bool terrainNeedsClear(int tx, int ty)
        {
            if (TERRAIN().get(tx, ty).roofIs())
                return false;
            return base.terrainNeedsClear(tx, ty);
        }

        protected override double constructionTime(Humanoid skill)
        {
            return 25;
        }

        private readonly SoundRace sound = AUDIO.race("BUILD_ROAD");

        protected override SoundRace constructSound()
        {
            return sound;
        }

        protected override void renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i, int state)
        {
            if (!FLOOR().getter.is(i.tile()))
                base.renderBelow(r, shadowBatch, i, state);
        }

        public override void doSomethingExtraRender()
        {
            if (showRoads)
                SETT.OVERLAY().ROADING.add();
        }

        protected override bool construct(int tx, int ty)
        {
            if (FLOOR().getter.get(tx, ty) != floor)
            {
                if (floor.resource != null)
                    GAME.player().res().inc(floor.resource, RTYPE.CONSTRUCTION, -floor.resAmount);
                floor.placeFixed(tx, ty);
                FLOOR().degrade.set(tx, ty, 0);
            }
            else
                FLOOR().degrade.set(tx, ty, FLOOR().degrade.get(tx, ty) - 0.25);
            return FLOOR().degrade.get(tx, ty) != 0;
        }

        public override bool isConstruction()
        {
            return true;
        }

        public override TerrainTile becomes(int tx, int ty)
        {
            return TERRAIN().NADA;
        }

        public override ToolConfig config()
        {
            return SETT.JOBS().roads.pla.get(this);
        }

        public static Job getPlacable()
        {
            return SETT.JOBS().roads.pla.current();
        }
    }
}