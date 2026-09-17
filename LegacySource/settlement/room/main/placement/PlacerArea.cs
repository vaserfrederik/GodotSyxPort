using System;
using System.Collections.Generic;
using settlement.room.main.placement;
using init.sprite;
using settlement.main;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using snake2d;
using snake2d.util.datatypes;
using util;
using util.gui.misc;
using util.text;
using view.tool;

class PlacerArea : PlacableMulti
{
    private static CharSequence ¤¤name = "¤Expand Room";
    private static CharSequence ¤¤shrink = "¤Shrink Room";
    static
    {
        D.ts(typeof(PlacerArea));
    }

    private readonly RoomPlacer embrio;
    private readonly PlacableMulti undo = new PlacableMulti(¤¤shrink, ¤¤shrink, SPRITES.icons().m.shrink)
    {
        public override void place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (embrio.instance.is(tx, ty))
                clear(tx, ty);
            embrio.history.placeEmbryo(tx, ty, -1);
        }

        public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (!embrio.instance.is(tx, ty))
            {
                return PlacableMessages.¤¤ROOM_MUST;
            }
            return null;
        }

        public override PLACABLE getUndo()
        {
            return PlacerArea.this;
        }

        public override void finishPlacing(AREA placedArea)
        {
            foreach (COORDINATE c in placedArea.body())
                if (placedArea.is(c))
                {
                    validateItems(c.x(), c.y());
                }

            base.finishPlacing(placedArea);
        }

        public override void placeInfo(GBox b, int oktiles, AREA a)
        {
            // TODO Auto-generated method stub
            base.placeInfo(b, oktiles, a);
        }
    };



    void clear(int tx, int ty)
    {
        if (ConstructionData.dFloored.is(tx, ty, 1))
        {
            embrio.resources.removeTile(tx, ty);
            SETT.FLOOR().clearer.clear(tx, ty);

        }

        SETT.MAINTENANCE().isser.is(tx, ty);

        FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
        if (it != null)
        {
            bool constructued = ConstructionData.dConstructed.is(tx, ty, 1);
            COORDINATE c = SETT.ROOMS().fData.itemX1Y1(tx, ty, Coo.TMP);

            int x1 = c.x();
            int y1 = c.y();

            SETT.ROOMS().fData.itemClear(tx, ty, embrio.instance);
            embrio.history.placeItem(it, x1, y1, -1);
            if (constructued)
            {
                embrio.resources.removeItem(x1, y1, it);
            }
        }

        embrio.instance.clear(tx, ty);
    }

    public PlacerArea(RoomPlacer embrio) : base(¤¤name, null, null, null)
    {
        this.embrio = embrio;
    }

    private void validateItems(int tx, int ty)
    {
        for (int i = 0; i < DIR.ALL.size(); i++)
        {
            DIR d = DIR.ALL.get(i);
            int dx = tx + d.x();
            int dy = ty + d.y();
            if (!embrio.instance.is(dx, dy))
                continue;
            FurnisherItem it = SETT.ROOMS().fData.item.get(dx, dy);
            if (it != null)
            {
                bool constructued = ConstructionData.dConstructed.is(dx, dy, 1);

                COORDINATE c = SETT.ROOMS().fData.itemX1Y1(dx, dy, Coo.TMP);
                int x1 = c.x();
                int y1 = c.y();
                SETT.ROOMS().fData.itemClear(dx, dy, embrio.instance);

                if (!replaceItem(x1, y1, it, constructued))
                {
                    embrio.history.placeItem(it, x1, y1, -1);
                    if (constructued)
                        embrio.resources.removeItem(x1, y1, it);
                }
            }
        }
    }

    private bool replaceItem(int x1, int y1, FurnisherItem it, bool constructed)
    {
        for (int y = 0; y < it.height(); y++)
        {
            for (int x = 0; x < it.width(); x++)
            {
                if (embrio.placability.itemPlacable(x1 + x, y1 + y, x, y, it, embrio.instance) != null)
                    return false;
            }
        }
        if (embrio.placability.itemProblem(x1, y1, it.group, it, embrio.instance) != null)
            return false;
        SETT.ROOMS().fData.itemSet(x1, y1, it, embrio.instance);
        if (constructed)
        {
            for (int y = 0; y < it.height(); y++)
            {
                for (int x = 0; x < it.width(); x++)
                {
                    if (it.get(x, y) != null)
                        ConstructionData.dConstructed.set(embrio.instance, x + x1, y + y1, 1);
                }
            }
        }
        return true;

    }

    public override void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA area,
        PLACER_TYPE type, bool isPlacable, bool areaIsPlacable)
    {
        base.renderPlaceHolder(r, mask, x, y, tx, ty, area, type, isPlacable, areaIsPlacable);
        if (isPlacable && embrio.autoWalls.is())
        {
            embrio.door.renderTmpPlaceArea(r, x, y, tx, ty, area);
        }

    }

    int am = 0;

    private readonly Rec rec = new Rec();
    private readonly Rec rec2 = new Rec();

    public override CharSequence isPlacable(AREA area, PLACER_TYPE type)
    {
        return null;
    }

    public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
    {
        if (ty == a.body().y1() && !a.is(tx - 1, ty))
        {
            rec.clear();

            foreach (COORDINATE c in embrio.instance.body())
            {
                if (embrio.instance.is(c))
                    rec.unify(c.x(), c.y());
            }
            am = 0;
        }

        CharSequence s = PLACEMENT.placable(tx, ty, embrio.blueprint(), embrio.buildOnWalls.is());
        if (s != null)
            return s;

        s = embrio.blueprint().constructor().placable(tx, ty, null, null);
        if (s != null)
            return s;

        am++;
        if (embrio.instance.area() + am >= Room.MAX_SIZE)
            return PlacableMessages.¤¤MAX_SIZE_REACHED;

        if (tx - a.body().x1() >= Room.MAX_DIM)
            return PlacableMessages.¤¤MAX_DIMENSION_REACHED;

        if (ty - a.body().y1() >= Room.MAX_DIM)
            return PlacableMessages.¤¤MAX_DIMENSION_REACHED;

        if (embrio.instance.area() > 0)
        {
            rec2.set(rec);
            rec2.unify(tx, ty);
            if (rec2.width() > Room.MAX_DIM)
                return PlacableMessages.¤¤MAX_DIMENSION_REACHED;
            if (rec2.height() > Room.MAX_DIM)
                return PlacableMessages.¤¤MAX_DIMENSION_REACHED;
        }

        return null;
    }

    public override void finishPlacing(AREA placedArea)
    {
        int m = GUTIL.coos().getI();

        for (int i = 0; i < m; i++)
        {
            GUTIL.coos().set(i);
            int tx = GUTIL.coos().get().x();
            int ty = GUTIL.coos().get().y();
            validateItems(tx, ty);
        }

        for (int i = 0; i < m; i++)
        {
            GUTIL.coos().set(i);
            int tx = GUTIL.coos().get().x();
            int ty = GUTIL.coos().get().y();
            embrio.history.placeEmbryo(tx, ty, 1);
        }

        base.finishPlacing(placedArea);
    }

    public override PLACABLE getUndo()
    {
        return undo;
    }
}