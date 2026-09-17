using static Settlement.Main.SETT;
using static Settlement.Main.SETT.GRASS;
using static Settlement.Main.SETT.JOBS;
using static Settlement.Main.SETT.PATH;
using static Settlement.Main.SETT.TERRAIN;
using static Settlement.Main.SETT.THINGS;
using static Settlement.Main.SETT.TWIDTH;

using Game.Audio;
using Init.Resources;
using Settlement.Entity.Humanoid;
using Settlement.Main;
using Settlement.Thing;
using Settlement.Tilemap.Terrain;
using Snake2D;
using Util.Rendering;
using Util.Text;
using View.Tool;

namespace Settlement.Job
{
    public abstract class JobBuild : Job
    {
        private static readonly CharSequence ¤¤clearTerrain = "Clearing Terrain";
        private static readonly CharSequence ¤¤clearVegetation = "Clearing Terrain";
        private static readonly CharSequence ¤¤getting = "Getting Materials";
        private static readonly CharSequence ¤¤constructing = "Constructing";
        private static readonly CharSequence ¤¤removing = "Removing Obstacle";

        static JobBuild()
        {
            D.ts(typeof(JobBuild));
        }

        private enum PSTATE
        {
            CLEAR_TERRAIN(¤¤clearTerrain),
            CLEAR_VEG(¤¤clearVegetation),
            REMOVING(¤¤removing),
            FETCHING(¤¤getting),
            CONSTRUCTING(¤¤constructing);

            public readonly CharSequence name;

            private PSTATE(CharSequence name)
            {
                this.name = name;
            }
        }

        private PSTATE state;

        private readonly bool solid;
        private Placer placer;
        protected bool needsFerClear = true;
        protected readonly RESOURCE res;
        protected readonly int resAmount;

        public JobBuild(string key, RESOURCE res, int resAmount, bool solid, CharSequence name, CharSequence desc, SPRITE icon)
            : base(key, name, icon)
        {
            this.res = res;
            if (res == null)
                resAmount = 0;
            this.resAmount = resAmount;
            this.solid = solid;
            placer = new Placer(this, res, resAmount, desc);
        }

        protected override void init(int tx, int ty)
        {
            JOBS().progress.set(tx + ty * TWIDTH, 0);
            JOBS().wantsRes.set(tx + ty * TWIDTH, false);
            if (res != null)
            {
                foreach (Thing t in THINGS().get(tx, ty))
                {
                    if (t is ScatteredResource)
                    {
                        ScatteredResource tt = (ScatteredResource)t;
                        if (tt.resource() == res)
                        {
                            int a = tt.amount() - tt.amountReserved();
                            if (a >= resAmount)
                            {
                                tt.removeUnreserved(resAmount);
                                JOBS().progress.set(tx + ty * TWIDTH, resAmount);
                                break;
                            }
                            else if (a > 0)
                            {
                                JOBS().progress.set(tx + ty * TWIDTH, a);
                                tt.removeUnreserved(a);
                                break;
                            }
                        }
                    }
                }
            }

            JOBS().wantsRes.set(tx + ty * TWIDTH, getStateP(tx, ty) == PSTATE.FETCHING);
        }

        public override RESOURCE res()
        {
            return res;
        }

        public override RESOURCE resourceCurrentlyNeeded()
        {
            if (state == PSTATE.FETCHING)
                return res;
            return null;
        }

        protected override CharSequence problem(int tx, int ty, bool overwrite)
        {
            if (base.problem(tx, ty, overwrite) != null)
                return base.problem(tx, ty, overwrite);
            if (TERRAIN().get(tx, ty).clearing().isStructure())
                return PlacableMessages.¤¤STRUCTURE_BLOCK;
            if (PATH().solidity.is(tx, ty))
                return PlacableMessages.¤¤SOLID_BLOCK;
            TerrainTile t = TERRAIN().get(tx, ty);

            if (t.clearing().needs() && !t.clearing().can())
                return PlacableMessages.¤¤MISC;

            if (becomesSolid() && !SETT.TERRAIN().MOUNTAIN.isMountain(tx, ty))
                return null;
            else if (t.clearing().isStructure())
                return PlacableMessages.¤¤STRUCTURE_BLOCK;

            return null;
        }

        private PSTATE getState(int tx, int ty)
        {
            PSTATE s = getStateP(tx, ty);
            if (s == PSTATE.FETCHING)
            {
                JOBS().wantsRes.set(tx + ty * TWIDTH, true);
            }
            if (resNeeds(tx, ty) && JOBS().wantsRes.get(tx + ty * TWIDTH))
            {
                s = PSTATE.FETCHING;
            }
            return s;
        }

        private PSTATE getStateP(int tx, int ty)
        {
            if (needsFerClear && terrainNeedsClear(tx, ty))
                return PSTATE.CLEAR_TERRAIN;
            else if (needsFerClear && GRASS().current.get(tx, ty) > 0)
                return PSTATE.CLEAR_VEG;
            else if (resNeeds(tx, ty))
                return PSTATE.FETCHING;
            else if (solid && THINGS().resources.has(tx, ty, RBIT.ALL))
                return PSTATE.REMOVING;
            else
                return PSTATE.CONSTRUCTING;
        }

        protected override bool get(int tx, int ty)
        {
            state = getState(tx, ty);
            return base.get(tx, ty);
        }

        private bool terrainNeedsClear(int tx, int ty)
        {
            return TERRAIN().get(tx, ty).clearing().needs() && !TERRAIN().get(tx, ty).clearing().isStructure() && TERRAIN().get(tx, ty).clearing().can();
        }

        private bool resNeeds(int tx, int ty)
        {
            return res != null && JOBS().progress.get(tx + ty * TWIDTH) < resAmount;
        }

        public override int jobResourcesNeeded(Humanoid skill)
        {
            if (res != null)
                return resAmount - JOBS().progress.get(tile);
            return 0;
        }

        public override void jobStartPerforming()
        {
            // TODO Auto-generated method stub
        }

        public override double jobPerformTime(Humanoid skill)
        {
            switch (state)
            {
                case PSTATE.CLEAR_TERRAIN:
                    TerrainTile t = TERRAIN().get(coo);
                    if (t.clearing().isEasilyCleared())
                        return 7;
                    return 15;
                case PSTATE.CLEAR_VEG:
                    return 2.0;
                case PSTATE.REMOVING:
                    return 0;
                case PSTATE.FETCHING:
                    return 0;
                case PSTATE.CONSTRUCTING:
                    return constructionTime(skill);
            }
            throw new System.RuntimeException();
        }

        protected abstract double constructionTime(Humanoid skill);

        public override RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm)
        {
            if (!jobReservedIs(r))
            {
                throw new System.RuntimeException();
            }
            RESOURCE res = null;
            switch (state)
            {
                case PSTATE.CLEAR_TERRAIN:
                    TerrainTile t = TERRAIN().get(tile);
                    res = t.clearing().clear1(coo.x(), coo.y());
                    break;
                case PSTATE.CLEAR_VEG:
                    GRASS().currentI.increment(coo.x(), coo.y(), -4);
                    break;
                case PSTATE.REMOVING:
                    ScatteredResource ress = THINGS().resources.get(coo.x(), coo.y());
                    if (ress == null)
                        break;
                    if (ress.findableReservedCanBe())
                    {
                        ress.findableReserve();
                        ress.resourcePickup();
                    }
                    else
                    {
                        ress.resourcePickup();
                    }
                    res = ress.resource();
                    break;
                case PSTATE.FETCHING:
                    JOBS().progress.set(coo.x() + coo.y() * TWIDTH, JOBS().progress.get(coo.x() + coo.y() * TWIDTH) + rAm);
                    break;
                case PSTATE.CONSTRUCTING:
                    if (!construct(coo.x(), coo.y()))
                        THINGS().resources.create(coo.x(), coo.y(), res, JOBS().progress.get(coo.x() + coo.y() * TWIDTH));
                    break;
            }
            return res;
        }

        public override bool jobFinished()
        {
            return JOBS().progress.get(coo.x() + coo.y() * TWIDTH) >= resAmount;
        }

        public override SoundRace jobSound()
        {
            switch (state)
            {
                case PSTATE.CLEAR_TERRAIN:
                    return TERRAIN().get(coo).clearing().sound(coo.x(), coo.y());
                case PSTATE.CLEAR_VEG:
                    return GRASS().clearSound;
                case PSTATE.REMOVING:
                    return null;
                case PSTATE.FETCHING:
                    return null;
                case PSTATE.CONSTRUCTING:
                    return constructSound();
            }
            throw new System.RuntimeException();
        }

        protected abstract SoundRace constructSound();

        protected override void renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i, int state)
        {
            if (state > 0 && res != null)
            {
                res.renderLaying(r, i.x(), i.y(), i.ran(), state);
                shadowBatch.setHeight(1).setDistance2Ground(0);
                res.renderLaying(shadowBatch, i.x(), i.y(), i.ran(), state);
            }
        }

        protected override void cancel(int tx, int ty)
        {
            if (JOBS().progress.get(tx + ty * TWIDTH) > 0)
                THINGS().resources.create(tx, ty, res, JOBS().progress.get(tx + ty * TWIDTH));
        }

        public override PlacableMulti placer()
        {
            return placer;
        }

        public override int resAmount()
        {
            return resAmount;
        }
    }
}