using System;
using System.Collections.Generic;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.text;
using view.main;
using view.tool;

namespace view.sett.ui.room.prints
{
    class PlacerSave : PlacableSingle
    {
        private static readonly string ¤¤not = "¤A room that can be furnished must be selected.";

        static
        {
            D.ts(typeof(PlacerSave));
        }

        private readonly UISavedPrints pp;

        public ToolConfig config = new ToolConfig()
        {
            AddUI = (LISTE<RENDEROBJ> uis) =>
            {
            },

            ActivateAction = () =>
            {
            },

            Update = (bool UIHovered) =>
            {
                if (!VIEW.s().panels.added(pp))
                    VIEW.s().tools.place(null, null, false);
            },

            Back = () =>
            {
                if (pp.placing != null)
                {
                    pp.placing = null;
                    return false;
                }
                VIEW.s().panels.remove(pp);
                return true;
            }
        };

        public PlacerSave(UISavedPrints panel) : base("")
        {
            this.pp = panel;
        }

        public override string IsPlacable(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r == null)
                return PlacableMessages.¤¤ROOM_MUST;
            if (!(r is ROOMA))
                return ¤¤not;

            if (!SETT.ROOMS().copy.prints.canAdd(r))
                return ¤¤not;
            return null;
        }

        public override void PlaceFirst(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            SavedPrint p = SETT.ROOMS().copy.prints.push(r, tx, ty);
            if (p != null)
                pp.set(p);
        }

        public override SPRITE GetIcon()
        {
            return SPRITES.icons().m.crossair;
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            return IsPlacable(toX, toY) == null && SETT.ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
        }
    }
}