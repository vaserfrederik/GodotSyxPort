using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.text;
using view.tool;

namespace settlement.job
{
    public class JobBuildFence : JobBuild
    {
        private readonly TFence fence;
        private static CharSequence ¤¤desc = "¤Stops subjects and animals from wandering where you don't desire them.";

        public static LIST<Job> Make()
        {
            D.ts(typeof(JobBuildFence));
            ArrayList<Job> all = new ArrayList<TJob>(TERRAIN().FENCES.all().Count);
            foreach (TFence s in TERRAIN().FENCES.all())
            {
                all.add(new JobBuildFence(s));
            }
            return all;
        }

        public JobBuildFence(TFence fence) : base(
                $"FENCE_{fence.key()}",
                fence.tile.resource,
                1,
                true,
                fence.tile.name(),
                ¤¤desc,
                fence.tile.getIcon())
        {
            needsFerClear = false;
            this.fence = fence;
        }

        public override void RenderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty)
        {
            foreach (DIR d in DIR.ORTHO)
            {
                if (FLOOR().getter.is(tx, ty, d) || JOBS().getter.get(tx, ty, d) is JobBuildFence)
                    mask |= d.mask();
            }
            SPRITES.cons().BIG.dashedThick.render(r, mask, x, y);
        }

        protected override double ConstructionTime(Humanoid skill)
        {
            return 10;
        }

        protected override SoundRace ConstructSound()
        {
            return fence.tile.clearing().sound(coo.x(), coo.y());
        }

        protected override bool Construct(int tx, int ty)
        {
            if (fence.tile.resource != null)
                GAME.player().res().inc(fence.tile.resource, RTYPE.CONSTRUCTION, -fence.tile.resAmount);
            fence.tile.placeFixed(tx, ty);
            return false;
        }

        public override bool IsConstruction()
        {
            return true;
        }

        public override TerrainTile Becomes(int tx, int ty)
        {
            return fence.tile;
        }

        private static JobComboPlacer pla = null;

        public override ToolConfig Config()
        {
            return pp().Get(this);
        }

        public static Job GetPlacable()
        {
            return pp().Current();
        }

        private static JobComboPlacer pp()
        {
            if (pla == null)
                pla = new JobComboPlacer(SETT.JOBS().fences, "FENCE");
            return pla;
        }
    }
}