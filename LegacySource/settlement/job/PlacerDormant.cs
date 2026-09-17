using System;
using settlement.main;
using init.sprite;
using settlement.job.StateManager;
using snake2d.util.datatypes;
using util.text;
using view.tool;

namespace settlement.job
{
    internal class PlacerDormant : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "¤Suspend Jobs";
        private static readonly CharSequence ¤¤desc = "¤Suspended jobs will not be performed. Useful for planning out your city and controlling which areas will be worked first.";

        static PlacerDormant()
        {
            D.ts(typeof(PlacerDormant));
        }

        public PlacerDormant() : base(¤¤name, ¤¤desc, SPRITES.icons().l.suspend)
        {
        }

        public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            ROOM_JOBBER j = ROOM_JOBBER.get(tx, ty);
            if (j != null)
            {
                return null;
            }
            int i = tx + ty * TWIDTH;
            if (JOBS().getter.is(i) && !JOBS().state.is(i, State.DORMANT))
                return null;
            return "";
        }

        public override void place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            place(tx, ty);
        }

        public static void place(int tx, int ty)
        {
            ROOM_JOBBER r = ROOM_JOBBER.get(tx, ty);
            if (r != null && r.jobToggleIs())
                r.jobToggle(false);
            int i = tx + ty * TWIDTH;
            Job j = JOBS().getter.get(i);
            if (j == null)
                return;
            JOBS().state.set(State.DORMANT, j);
        }

        public override PLACABLE getUndo()
        {
            return JOBS().tool_activate;
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            ROOM_JOBBER j = ROOM_JOBBER.get(fromX, fromY);
            return j != null && j.is(toX, toY);
        }
    }
}