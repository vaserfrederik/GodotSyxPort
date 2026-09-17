using System;
using System.Collections.Generic;
using settlement.room.main.placement;
using init.sprite;
using settlement.main;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sprite;
using util.gui.misc;
using util.text;
using view.tool;

class PlacerItemSingle : PlacableFixed
{
    protected readonly RoomPlacer embryo;
    private RoomBlueprintImp blueprint;
    protected FurnisherItemGroup group;
    protected readonly UtilStats res;
    protected readonly Instance area;
    private int upgrade;

    private int[] sizes;

    private static readonly CharSequence ¤¤undo = "¤Remove Item";
    static
    {
        D.ts(typeof(PlacerItemSingle));
    }

    private readonly PlacableMulti undo = new PlacableMulti(¤¤undo)
    {
        Place = (tx, ty, a, t) =>
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r.constructor() == blueprint.constructor())
                r.remove(tx, ty, true, this, false).clear();
        },
        IsPlacable = (tx, ty, a, t) =>
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r.constructor() == blueprint.constructor())
                return null;
            return E;
        },
        ExpandsTo = (fromX, fromY, toX, toY) =>
        {
            Room r = SETT.ROOMS().map.get(fromX, fromY);
            if (r != null && r.constructor() == blueprint.constructor() && r.isSame(fromX, fromY, toX, toY))
                return true;
            return false;
        }
    };

    public PlacerItemSingle(RoomPlacer embryo)
    {
        this.embryo = embryo;
        this.res = embryo.resources;
        this.area = embryo.instance;
    }

    public void Set(RoomBlueprintImp b, int group, int upgrade)
    {
        if (SETT.ROOMS() != null)
        {
            if (sizes == null)
            {
                sizes = Alloc.ii(SETT.ROOMS().AMOUNT_OF_BLUEPRINTS);
            }
            if (this.blueprint != null)
            {
                sizes[this.blueprint.index()] = Size();
            }
        }

        this.blueprint = b;
        this.group = b.constructor().pgroups().Get(group);
        this.upgrade = upgrade;
        if (sizes != null)
        {
            SizeSet(sizes[b.index()]);
        }
    }

    public override CharSequence Name()
    {
        return group.name();
    }

    public override void Place(int tx, int ty, int rx, int ry)
    {
        FurnisherItem it = group.item(Size(), rot());

        if (rx == 0 && ry == 0)
        {
            TBuilding s = blueprint.constructor().mustBeIndoors() ? embryo.structure.Get() : null;

            if (s != null && embryo.autoWalls.Is())
            {
                embryo.instance.Clear(blueprint);
                for (int y = 0; y < it.height(); y++)
                {
                    for (int x = 0; x < it.width(); x++)
                    {
                        if (it.Get(x, y) != null)
                            embryo.instance.Set(tx + x, ty + y);
                    }
                }

                embryo.door.Build(s);
                embryo.instance.Clear(blueprint);

                for (int y = 0; y < it.height(); y++)
                {
                    for (int x = 0; x < it.width(); x++)
                    {
                        if (it.Get(x, y) != null && it.Get(x, y).mustBeReachable)
                        {
                            // Handle reachability
                        }
                    }
                }
            }

            FurnisherItem secret = blueprint.secret;
            if (secret != null)
            {
                for (int y = 0; y < secret.height(); y++)
                {
                    for (int x = 0; x < secret.width(); x++)
                    {
                        if (secret.Get(x, y) != null)
                        {
                            // Handle secret item placement
                        }
                    }
                }
            }
        }
    }

    public override void PlaceInfo(GBox box, int x1, int y1)
    {
        box.Add(box.text().add(width()).add('x').add(height()));
        box.NL();
        for (int i = 0; i < group.blueprint.resources(); i++)
        {
            if (group.item(Size(), rot()).cost2(i, upgrade) > 0)
            {
                box.SetResource(group.blueprint.resource(i), Math.Ceiling(group.item(Size(), rot()).cost2(i, upgrade)));
                box.Space();
            }
        }

        if (blueprint.constructor().mustBeIndoors() && ROOMS().placement.placer.autoWalls.Is() && embryo.structure.Get() != null)
        {
            FurnisherItem it = group.item(Size(), rot());
            int roofs = 0;
            int walls = 0;
            for (int y = -1; y <= it.height(); y++)
            {
                for (int x = -1; x <= it.width(); x++)
                {
                    if (it.Get(x, y) != null)
                    {
                        roofs++;
                    }
                    else if (UtilWallPlacability.wallCanBe.Is(x1 + x, y1 + y))
                    {
                        bool roof = false;
                        foreach (DIR d in DIR.ORTHO)
                        {
                            roof |= it.Get(x, y, d) != null && it.Get(x, y, d).mustBeReachable;
                        }

                        foreach (DIR d in DIR.ALL)
                        {
                            if (it.Get(x, y, d) != null)
                            {
                                if (roof)
                                    roofs++;
                                else
                                    walls++;
                                break;
                            }
                        }
                    }
                }
            }

            int am = roofs * SETT.JOBS().build_structure.Get(embryo.structure.Get().structure.index()).ceiling.resAmount();
            am += walls * SETT.JOBS().build_structure.Get(embryo.structure.Get().structure.index()).wall.resAmount();
            box.SetResource(embryo.structure.Get().structure.resource, am);
            box.Space();
        }

        foreach (FurnisherStat s in group.blueprint.stats())
        {
            double am = group.item(Size(), rot()).stat(s);
            if (am != 0)
            {
                box.NL();
                box.Add(box.text().lablify().add(s.name()));
                box.Tab(7);
                box.Add(s.format(box.text(), am));
            }
        }

        box.NL(8);
        group.blueprint.placeInfo(box, group.item(Size(), rot()), x1, y1);
    }

    public override void HoverDesc(GBox box)
    {
        box.Title(group.name);
        box.Text(group.desc);
        box.NL();
        for (int i = 0; i < group.blueprint.resources(); i++)
        {
            if (group.item(0, 0).cost2(i, upgrade) > 0)
            {
                box.SetResource(group.blueprint.resource(i), Math.Ceiling(group.item(0, 0).cost2(i, upgrade)));
                box.Space();
            }
        }

        foreach (FurnisherStat s in group.blueprint.stats())
        {
            if (group.item(0, 0).stat(s) > 0)
            {
                box.NL();
                box.Add(box.text().lablify().add(s.name()));
                box.Add(s.format(box.text(), group.item(0, 0).stat(s)));
            }
        }
    }

    public override PLACABLE GetUndo()
    {
        return undo;
    }

    public override int Rotations()
    {
        return group.rotations();
    }

    public override int Sizes()
    {
        return group.size();
    }

    public override SPRITE GetIcon()
    {
        return null;
    }

    private readonly Area itemArea = new Area();
    AREA itemAreaCurrent = null;

    private class Area : AREA
    {
        private readonly Rec area = new Rec();
        private int size = 0;
        private FurnisherItem item;

        public Area Set(FurnisherItem item, int x1, int y1)
        {
            this.item = item;
            area.SetDim(item.width(), item.height());
            area.MoveX1Y1(x1, y1);
            size = area.width() * area.height();
            return this;
        }

        public override RECTANGLE body()
        {
            return area;
        }

        public override bool is(int tile)
        {
            return false;
        }

        public override bool is(int tx, int ty)
        {
            return area.holdsPoint(tx, ty) && item.Get(tx - body().x1(), ty - body().y1()) != null;
        }

        public override int area()
        {
            return size;
        }
    }
}