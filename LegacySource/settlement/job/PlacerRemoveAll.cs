using System;
using settlement.job;
using game;
using init.sprite;
using settlement.main;
using settlement.room.main;
using settlement.room.military.artillery;
using snake2d.util.datatypes;
using util.text;
using view.tool;

namespace settlement.job
{
    public class PlacerRemoveAll : PlacableMulti
    {
        private static readonly CharSequence ¤¤remove = "¤Dismantle";
        private static readonly CharSequence ¤¤desc = "¤Dismantles structures and rooms.";

        static PlacerRemoveAll()
        {
            D.ts(typeof(PlacerRemoveAll));
        }

        public PlacerRemoveAll() : base(¤¤remove, ¤¤desc, SPRITES.icons().l.demolish)
        {
        }

        public override PLACABLE GetUndo()
        {
            return SETT.JOBS().tool_clear;
        }

        public override CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            Room r = SETT.ROOMS().map.Get(tx, ty);
            if (r is ArtilleryInstance artilleryInstance)
            {
                return artilleryInstance.army() == GAME.ARMIES().player() ? null : PlacableMessages.¤¤ROOM_MUST;
            }
            CharSequence ro = r != null && !SETT.ROOMS().THRONE.Is(tx, ty) ? null : PlacableMessages.¤¤ROOM_MUST;
            CharSequence st = SETT.JOBS().clearss.structure.problem(tx, ty, false);
            if (ro == null || st == null)
                return null;
            return PlacableMessages.¤¤ROOM_OR_STRUCTURE_MUST;
        }

        public override void Place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (SETT.ROOMS().map.Is(tx, ty))
            {
                TmpArea aa = SETT.ROOMS().map.Get(tx, ty).Remove(tx, ty, true, this, false);
                if (aa != null)
                    aa.Clear();
            }
            if (SETT.JOBS().clearss.structure.problem(tx, ty, false) == null)
                SETT.JOBS().clearss.structure.placer().Place(tx, ty, a, t);
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            Room r = SETT.ROOMS().map.Get(fromX, fromY);
            if (r == null)
                return false;
            if (SETT.ROOMS().THRONE.Is(fromX, fromY))
                return false;
            return r.IsSame(fromX, fromY, toX, toY);
        }
    }
}