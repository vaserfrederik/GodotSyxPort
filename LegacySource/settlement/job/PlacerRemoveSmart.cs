using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Job
{
    using static Settlement.Main.SETT.JOBS;

    using Init.Sprite;
    using Init.Sprite.UI;
    using Settlement.Main;
    using Snake2D.Util.DataTypes;
    using Util.Text;
    using View.Tool;

    public sealed class PlacerRemoveSmart : PlacableMulti
    {
        private static readonly string ¤¤remove = "¤Smart Remove";
        private static readonly string ¤¤desc = "¤Removes jobs if any selected, else removes structures and rooms.";

        static PlacerRemoveSmart()
        {
            D.ts(typeof(PlacerRemoveSmart));
        }

        private int stage = -1;

        public PlacerRemoveSmart() : base(¤¤remove, ¤¤desc, SPRITES.icons().l.demolish.twin(UI.icons().s.allRight, DIR.NE, 2))
        {
        }

        public override PLACABLE GetUndo()
        {
            return SETT.JOBS().tool_clear;
        }

        public override string IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (stage == 1)
            {
                if (SETT.ROOMS().map.get(tx, ty) == null && SETT.JOBS().getter.get(tx, ty) != null)
                {
                    return null;
                }
                if (SETT.ROOMS().construction.isser.is(tx, ty) && SETT.ROOMS().DELETE.isPlacable(tx, ty, null, null) == null)
                {
                    return null;
                }
            }
            else if (stage == 2)
            {
                if (SETT.ROOMS().DELETE.isPlacable(tx, ty, a, t) == null)
                    return null;
                if (SETT.JOBS().clearss.structure.problem(tx, ty, false) == null)
                {
                    return null;
                }
            }

            return E;
        }

        public override string IsPlacable(AREA area, PLACER_TYPE type)
        {
            stage = -1;
            foreach (COORDINATE c in area.body())
            {
                if (area.is(c))
                {
                    if (SETT.ROOMS().map.get(c) == null && SETT.JOBS().getter.get(c) != null)
                    {
                        stage = 1;
                        return null;
                    }
                    if (SETT.ROOMS().construction.isser.is(c.x(), c.y()) && SETT.ROOMS().DELETE.isPlacable(c.x(), c.y(), null, null) == null)
                    {
                        stage = 1;
                        return null;
                    }
                    if (SETT.JOBS().clearss.structure.problem(c.x(), c.y(), false) == null)
                    {
                        stage = 2;
                        return null;
                    }
                    if (SETT.ROOMS().DELETE.isPlacable(c.x(), c.y(), null, null) == null)
                    {
                        stage = 2;
                        return null;
                    }
                }
            }
            return E;
        }

        public override void Place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (stage == 1)
            {
                if (SETT.ROOMS().map.get(tx, ty) == null && SETT.JOBS().getter.get(tx, ty) != null)
                {
                    Job j = JOBS().getter.get(tx, ty);
                    if (j != null)
                        j.cancel(tx, ty);
                    SETT.JOBS().clearer.set(tx, ty);
                }
                if (SETT.ROOMS().construction.isser.is(tx, ty) && SETT.ROOMS().DELETE.isPlacable(tx, ty, null, null) == null)
                {
                    SETT.ROOMS().DELETE.place(tx, ty, a, t);
                }
            }
            else if (stage == 2)
            {
                if (SETT.ROOMS().DELETE.isPlacable(tx, ty, a, t) == null)
                {
                    SETT.ROOMS().DELETE.place(tx, ty, a, t);
                }
                if (SETT.JOBS().clearss.structure.problem(tx, ty, false) == null)
                {
                    SETT.JOBS().clearss.structure.placer().place(tx, ty, a, t);
                }
            }
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            if (stage == 1)
            {
                return SETT.ROOMS().construction.isser.is(fromX, fromY) && SETT.ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
            }
            if (stage == 2)
            {
                return SETT.ROOMS().DELETE.isPlacable(fromX, fromY, null, null) == null && SETT.ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
            }
            return false;
        }
    }
}