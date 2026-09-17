using System;
using System.Collections.Generic;
using game;
using game.audio;
using game.faction;
using game.faction.FResources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.text;
using view.tool;

namespace settlement.job
{
    public sealed class JobBuildFort : JobBuild
    {
        private readonly TFortification fort;
        private static readonly CharSequence ¤¤dStairs = "¤Stairs are used for getting access to the top of fortifications. Should be placed adjacent to one.";

        static
        {
            D.ts(typeof(JobBuildFort));
        }

        public static LIST<Job> make()
        {
            ArrayList<Job> all = new ArrayList<Job>(TERRAIN().FORTIFICATIONS.all().Count);
            foreach (TFortification s in TERRAIN().FORTIFICATIONS.all())
            {
                all.add(new JobBuildFort(s));
            }
            return all;
        }

        public static class JobBuildForts
        {
            public readonly LIST<Job> all;
            private readonly JobComboPlacer pla;
            public readonly Job build_stairs = new JobBuildFort.Stairs();

            public JobBuildForts()
            {
                ArrayList<Job> all = new ArrayList<Job>(TERRAIN().FORTIFICATIONS.all().Count);
                foreach (TFortification s in TERRAIN().FORTIFICATIONS.all())
                {
                    all.add(new JobBuildFort(s));
                }
                this.all = all;
                pla = new JobComboPlacer(all.Join(build_stairs), "FORTIFICATIONS");
            }

            public Job getPlacable()
            {
                return pla.current();
            }
        }

        public JobBuildFort(TFortification fort)
            : base("FORT_" + fort.key(), fort.resource, fort.resAmount, true, fort.tile.name(), fort.desc, fort.tile.getIcon())
        {
            this.fort = fort;
        }

        public override void renderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty)
        {
            foreach (DIR d in DIR.ORTHO)
            {
                if (FLOOR().getter.is(tx, ty, d) || JOBS().getter.get(tx, ty, d) is JobBuildFort)
                    mask |= d.mask();
            }
            SPRITES.cons().BIG.dashed.render(r, mask, x, y);
        }

        public override CharSequence lockText()
        {
            Str.TMP.clear().add(Dic.¤¤Requires);
            Str.TMP.NL();
            bool has = false;
            foreach (Lock<Faction> i in fort.reqs.all())
            {
                if (!i.unlocker.inUnlocked(FACTIONS.player()))
                {
                    Str.TMP.NL();
                    has = true;
                    Str.TMP.add(i.unlocker.name);
                }
            }
            if (has)
                return Str.TMP;
            return null;
        }

        protected override double constructionTime(Humanoid skill)
        {
            return 50;
        }

        protected override SoundRace constructSound()
        {
            return fort.sound;
        }

        protected override bool construct(int tx, int ty)
        {
            if (fort.resource != null)
                GAME.player().res().inc(fort.resource, RTYPE.CONSTRUCTION, -fort.resAmount);
            fort.tile.placeFixed(tx, ty);
            SETT.FLOOR().clearer.clear(tx, ty);
            return false;
        }

        public override bool becomesSolid()
        {
            return true;
        }

        public override bool isConstruction()
        {
            return true;
        }

        private static class Stairs : JobBuild
        {
            public Stairs()
                : base("STAIRS", RESOURCES.STONE(), 2, false, SETT.TERRAIN().FSTAIRS.name(), ¤¤dStairs, SETT.TERRAIN().FSTAIRS.getIcon())
            {
            }

            protected override double constructionTime(Humanoid skill)
            {
                return 50;
            }

            protected override bool construct(int tx, int ty)
            {
                GAME.player().res().inc(res, RTYPE.CONSTRUCTION, -resAmount);
                SETT.TERRAIN().FSTAIRS.placeFixed(tx, ty);
                return false;
            }

            protected override SoundRace constructSound()
            {
                return null;
            }

            protected override CharSequence problem(int tx, int ty, bool overwrite)
            {
                TerrainTile t = TERRAIN().get(tx, ty);
                if (t is TFortification.Normal && SETT.PATH().availability.get(tx, ty).player < 0)
                    return null;

                if (base.problem(tx, ty, overwrite) != null)
                    return base.problem(tx, ty, overwrite);

                if (PATH().solidity.is(tx, ty))
                    return PlacableMessages.¤¤SOLID_BLOCK;
                if (t.clearing().needs() && !t.clearing().can())
                    return PlacableMessages.¤¤MISC;
                return null;
            }

            public override void renderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty)
            {
                SPRITES.cons().BIG.dashed.render(r, 0, x, y);
            }

            public override TerrainTile becomes(int tx, int ty)
            {
                return SETT.TERRAIN().FSTAIRS;
            }

            public override ToolConfig config()
            {
                return SETT.JOBS().build_fort.pla.get(this);
            }
        }

        public override TerrainTile becomes(int tx, int ty)
        {
            return fort.tile;
        }

        public override ToolConfig config()
        {
            return SETT.JOBS().build_fort.pla.get(this);
        }
    }
}