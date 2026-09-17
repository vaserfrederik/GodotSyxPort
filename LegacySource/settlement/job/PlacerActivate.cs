using System;
using System.Text;
using snake2d;
using util.text;

namespace settlement.job
{
    public class PlacerActivate : PlacableMulti
    {
        private static readonly string ¤¤name = "¤Activate Job";
        private static readonly string ¤¤desc = "¤Activates suspended jobs.";

        static PlacerActivate()
        {
            D.ts(typeof(PlacerActivate));
        }

        public PlacerActivate() : base(¤¤name, ¤¤desc, SPRITES.icons().l.suspend.twin(UI.icons().m.anti, DIR.C, 1))
        {
        }

        static void Place(int tx, int ty)
        {
            int i = tx + ty * SETT.TWIDTH;
            if (SETT.JOBS().getter.Is(i))
            {
                SETT.JOBS().state.Activate(i, SETT.JOBS().getter.Get(i));
            }
            ROOM_JOBBER j = ROOM_JOBBER.Get(tx, ty);
            if (j != null && !j.JobToggleIs())
                j.JobToggle(true);
        }

        public override PLACABLE GetUndo()
        {
            return SETT.JOBS().tool_dormant;
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            ROOM_JOBBER j = ROOM_JOBBER.Get(fromX, fromY);
            return j != null && j.Is(toX, toY);
        }

        public override string IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            int i = tx + ty * SETT.TWIDTH;

            ROOM_JOBBER j = ROOM_JOBBER.Get(tx, ty);

            if (SETT.ROOMS().map.Is(i))
            {
                if (j != null)
                    return null;
            }
            else if (SETT.JOBS().getter.Is(i))
            {
                if (SETT.JOBS().state.Is(i, StateManager.State.DORMANT))
                    return null;
            }
            return "";
        }

        public override void Place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            int i = tx + ty * SETT.TWIDTH;
            if (SETT.JOBS().getter.Is(i))
            {
                SETT.JOBS().state.Activate(i, SETT.JOBS().getter.Get(i));
            }
            ROOM_JOBBER j = ROOM_JOBBER.Get(tx, ty);
            if (j != null && !j.JobToggleIs())
                j.JobToggle(true);
        }
    }
}