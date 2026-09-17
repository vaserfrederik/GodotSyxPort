using settlement.room.main.placement;
using settlement.main;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using snake2d;
using snake2d.util.datatypes;
using util.GUTIL;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.tool;

class PlacerItemArea : PlacerItemSingle
{
    private static CharSequence ¤¤undo = "¤Remove Item";

    static
    {
        D.ts(PlacerItemArea.class);
    }

    private readonly PlacableMulti undo = new PlacableMulti(¤¤undo)
    {
        public void place(int x1, int y1, AREA a, PLACER_TYPE t)
        {
            removeItem(x1, y1);
        }

        public CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (!area.is(tx, ty) || !SETT.ROOMS().fData.item.is(tx, ty))
            {
                return PlacableMessages.¤¤ITEM_MUST;
            }
            return null;
        }

        public bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            if (!area.is(fromX, fromY) || !area.is(toX, toY))
                return false;
            FurnisherItem it = SETT.ROOMS().fData.item.get(fromX, fromY);
            FurnisherItem it2 = SETT.ROOMS().fData.item.get(toX, toY);
            if (it != null && it2 != null)
            {
                COORDINATE c = SETT.ROOMS().fData.itemMaster(fromX, fromY, Coo.TMP);
                int x = c.x();
                int y = c.y();
                return SETT.ROOMS().fData.itemMaster(toX, toY, Coo.TMP).isSameAs(x, y);
            }
            return false;
        }
    };

    public void removeItem(int x1, int y1)
    {
        if (!embryo.instance.is(x1, y1))
            return;

        final FurnisherItem item = SETT.ROOMS().fData.item.get(x1, y1);
        if (item == null)
            return;

        COORDINATE c = SETT.ROOMS().fData.itemX1Y1(x1, y1, Coo.TMP);
        int x11 = c.x();
        int y11 = c.y();

        bool constructed = ConstructionData.dConstructed.is(x1, y1, 1);
        for (int dy = 0; dy < item.height(); dy++)
        {
            for (int dx = 0; dx < item.width(); dx++)
            {
                if (item.get(dx, dy) != null)
                {
                    if (!embryo.instance.is(x11 + dx, y11 + dy))
                    {
                        debug(x1, y1);
                    }
                }
            }
        }

        for (int dy = 0; dy < item.height(); dy++)
        {
            for (int dx = 0; dx < item.width(); dx++)
            {
                if (item.get(dx, dy) != null)
                {
                    ConstructionData.dConstructed.set(embryo.instance, x11 + dx, y11 + dy, 0);
                }
            }
        }
        SETT.ROOMS().fData.itemClear(x1, y1, embryo.instance);

        embryo.history.placeItem(item, x11, y11, -1);

        if (constructed)
        {
            embryo.resources.removeItem(x1, y1, item);
        }
    }

    private void debug(int x1, int y1)
    {
        FurnisherItem item = SETT.ROOMS().fData.item.get(x1, y1);
        COORDINATE c = SETT.ROOMS().fData.itemX1Y1(x1, y1, Coo.TMP);
        int x11 = c.x();
        int y11 = c.y();

        System.err.println("so, here we go again... Item is " + item.group.blueprint.blue().key + " " + item.group.index() + " " + item.width() + " " + item.height());
        System.err.println("coo: " + x1 + " " + y1);
        System.err.println("embrio: " + embryo.instance.body());

        System.err.println("the x1y1: " + c);

        for (int dy = 0; dy < item.height(); dy++)
        {
            for (int dx = 0; dx < item.width(); dx++)
            {
                if (item.get(dx, dy) != null)
                {
                    if (!embryo.instance.is(x11 + dx, y11 + dy))
                    {
                        debug(x1, y1);
                    }
                }
            }
        }

        for (int dy = 0; dy < item.height(); dy++)
        {
            for (int dx = 0; dx < item.width(); dx++)
            {
                if (item.get(dx, dy) != null)
                {
                    ConstructionData.dConstructed.set(embryo.instance, x11 + dx, y11 + dy, 0);
                }
            }
        }
        SETT.ROOMS().fData.itemClear(x1, y1, embryo.instance);

        embryo.history.placeItem(item, x11, y11, -1);

        if (constructed)
        {
            embryo.resources.removeItem(x1, y1, item);
        }
    }

    public override void render(SPRITE_RENDERER r, int x, int y, bool hover, bool selected)
    {
        base.render(r, x, y, hover, selected);
    }

    public override PLACABLE getUndo()
    {
        return undo;
    }

    public AREA getTmpArea(int x1, int y1, FurnisherItem item)
    {
        AreaTmp a = GUTIL.AREA();
        a.clear();

        for (int y = 0; y < item.height(); y++)
        {
            for (int x = 0; x < item.width(); x++)
            {
                if (!item.is(x, y))
                {
                    continue;
                }

                int tx = x + x1;
                int ty = y + y1;

                if (!embryo.instance.is(tx, ty))
                {
                    a.set(tx, ty);
                }
            }
        }

        for (int y = -1; y <= item.height(); y++)
        {
            for (int x = -1; x <= item.width(); x++)
            {
                if (item.is(x, y))
                {
                    continue;
                }

                foreach (DIR d in DIR.ALL)
                {
                    int dx = x + d.x();
                    int dy = y + d.y();
                    if (item.is(dx, dy) && item.get(dx, dy).mustBeReachable)
                    {
                        int tx = x + x1;
                        int ty = y + y1;
                        if (!embryo.instance.is(tx, ty))
                        {
                            a.set(tx, ty);
                        }
                        break;
                    }
                }
            }
        }

        return a;
    }
}