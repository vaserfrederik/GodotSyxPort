using settlement.main;
using init.sprite;
using settlement.tilemap.terrain;
using snake2d.util.datatypes;
using util.text;
using view.tool;

namespace settlement.job
{
    internal sealed class PlacerDelete : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "Cancel Jobs";
        private static readonly CharSequence ¤¤desc = "Cancels all jobs and room plans.";

        static PlacerDelete()
        {
            D.ts(typeof(PlacerDelete));
        }

        public PlacerDelete() : base(¤¤name, ¤¤desc, SPRITES.icons().m.cancel)
        {
        }

        internal static void Place(int tx, int ty)
        {
            if (JOBS().getter.Is(tx, ty))
                JOBS().state.Clear(tx, ty);

            TerrainTile t = TERRAIN().Get(tx, ty);
            if (t is TGrowable)
            {
                TGrowable b = (TGrowable)t;
                b.job.Set(tx, ty, false);
            }
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            if (SETT.ROOMS().construction.isser.Is(fromX, fromY))
            {
                return SETT.ROOMS().map.Get(fromX, fromY).IsSame(fromX, fromY, toX, toY);
            }
            return false;
        }

        public override CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (SETT.JOBS().clearss.huntundo.IsPlacable(tx, ty, a, t) == null)
            {
                return null;
            }

            if (SETT.ROOMS().construction.isser.Is(tx, ty))
                return null;

            ROOM_JOBBER j = ROOM_JOBBER.Get(tx, ty);
            if (j != null)
                return PlacableMessages.¤¤JOB_MUST;

            if (!JOBS().getter.Is(tx, ty))
            {
                if (!SETT.TERRAIN().GROWABLES[0].job.Is(tx, ty))
                    return PlacableMessages.¤¤JOB_MUST;
            }
            return null;
        }

        public override void Place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (SETT.JOBS().clearss.huntundo.IsPlacable(tx, ty, a, t) == null)
            {
                SETT.JOBS().clearss.huntundo.Place(tx, ty, a, t);
            }

            if (SETT.ROOMS().construction.isser.Is(tx, ty))
            {
                SETT.ROOMS().map.Get(tx, ty).Remove(tx, ty, true, this, false).Clear();
            }

            Job j = JOBS().getter.Get(tx, ty);
            if (j != null)
                j.Cancel(tx, ty);

            Place(tx, ty);

            if (SETT.TERRAIN().GROWABLES[0].job.Is(tx, ty))
                SETT.TERRAIN().GROWABLES[0].job.Set(tx, ty, false);
        }

        public override bool CanBePlacedAs(PLACER_TYPE t)
        {
            return true;
        }
    }
}