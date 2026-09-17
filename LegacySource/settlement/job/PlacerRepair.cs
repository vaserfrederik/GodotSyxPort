using System;
using settlement.main;
using init.sprite;
using settlement.room.main;
using settlement.tilemap;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using util.text;
using view.tool;

namespace settlement.job
{
    class PlacerRepair : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "Repair";
        private static readonly CharSequence ¤¤desc = "Repair damaged structures and rooms.";

        static PlacerRepair()
        {
            D.ts(typeof(PlacerRepair));
        }

        public PlacerRepair() : base(¤¤name, ¤¤desc, SPRITES.icons().l.repair)
        {
        }

        public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (ROOMS().construction.isRepair(tx, ty))
                return null;
            TerrainTile tt = TERRAIN().get(tx, ty);
            if (!(tt is TILE_FIXABLE))
                return PlacableMessages.¤¤BROKEN_MUST;
            Job j = ((TILE_FIXABLE)tt).fixJob(tx, ty);
            if (j == null)
                return PlacableMessages.¤¤BROKEN_MUST;
            return null;
        }

        public override void place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (ROOMS().construction.isRepair(tx, ty))
            {
                PlacerActivate.place(tx, ty);
            }
            else
            {
                TerrainTile tt = TERRAIN().get(tx, ty);
                if (tt is TILE_FIXABLE ttt)
                {
                    Job j = ttt.fixJob(tx, ty);
                    j.placer().place(tx, ty, a, t);
                }
            }
        }

        public override void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA a, PLACER_TYPE t, bool isPlacable, bool areaIsPlacable)
        {
            SPRITES.cons().ICO.repair.render(r, x, y);
        }

        public override PLACABLE getUndo()
        {
            return JOBS().tool_clear;
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            Room r = ROOMS().map.get(fromX, fromY);
            return (ROOMS().construction.isRepair(fromX, fromY) && r.isSame(fromX, fromY, toX, toY));
        }
    }
}