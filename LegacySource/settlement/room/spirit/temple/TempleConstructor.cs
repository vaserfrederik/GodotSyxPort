using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.floor.Floors;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.spirit.temple
{
    sealed class TempleConstructor : Furnisher
    {
        private readonly ROOM_TEMPLE blue;
        public readonly FurnisherStat priests;
        public readonly FurnisherStat worshippers;
        public readonly FurnisherStat decor;
        public readonly FurnisherStat grandure;
        public readonly FurnisherStat space;

        private readonly FurnisherItemTile ca;
        public readonly FurnisherItemTile ap;
        public readonly FurnisherItemTile al;
        public readonly FurnisherItemTile es;
        public readonly FurnisherItemTile wo;

        protected TempleConstructor(ROOM_TEMPLE blue, RoomInitData init)
            : base(init, 5, 5, 304, 240)
        {
            this.blue = blue;

            priests = new FurnisherStat.FurnisherStatEmployees(this);
            worshippers = new FurnisherStat.FurnisherStatServices(this, blue);
            decor = new FurnisherStat.FurnisherStatRelative(this, worshippers, 0.7);
            grandure = new FurnisherStat(this)
            {
                public override double get(AREA area, double acc)
                {
                    double d = 1.5 * area.area() / Room.MAX_SIZE;
                    return Math.Pow(CLAMP.d(d, 0, 1), 0.5);
                }

                public override GText format(GText t, double value)
                {
                    GFORMAT.perc(t, value);
                    return t;
                }
            };
            space = new FurnisherStat(this)
            {
                public override double get(AREA area, double acc)
                {
                    return acc;
                }

                public override double get(AREA area, double[] fromItems)
                {
                    double p = fromItems[priests.index()];
                    if (p == 0)
                        return 1;

                    double d = area.area() / (p * 38);
                    return Math.Pow(CLAMP.d(d, 0, 1), 0.5);
                }

                public override GText format(GText t, double value)
                {
                    GFORMAT.perc(t, value);
                    return t;
                }
            };

            Floor path = SETT.FLOOR().map.read("FLOOR_PATH", init.seed);
            Json json = init.json;

            RoomSprite sprite = new RoomSprite(json.get("sprite"));
            sprite.init(5, 5, 304, 240, new Json(), null);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { FurnisherItemTile.Empty, wo, wo, FurnisherItemTile.Empty, wo, wo, FurnisherItemTile.Empty },
                new FurnisherItemTile[] { FurnisherItemTile.Empty, FurnisherItemTile.Empty, al, ap, al, FurnisherItemTile.Empty, FurnisherItemTile.Empty },
                new FurnisherItemTile[] { wo, ca, al, es, al, ca, wo },
                new FurnisherItemTile[] { FurnisherItemTile.Empty, FurnisherItemTile.Empty, al, ap, al, FurnisherItemTile.Empty, FurnisherItemTile.Empty },
                new FurnisherItemTile[] { FurnisherItemTile.Empty, wo, wo, FurnisherItemTile.Empty, wo, wo, FurnisherItemTile.Empty },
            }, 1, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { wo, wo, wo, FurnisherItemTile.Empty, wo, FurnisherItemTile.Empty, wo, wo, wo },
                new FurnisherItemTile[] { wo, FurnisherItemTile.Empty, al, ap, al, ap, al, FurnisherItemTile.Empty, wo },
                new FurnisherItemTile[] { FurnisherItemTile.Empty, ca, al, es, al, es, al, ca, FurnisherItemTile.Empty },
                new FurnisherItemTile[] { wo, FurnisherItemTile.Empty, al, ap, al, ap, al, FurnisherItemTile.Empty, wo },
                new FurnisherItemTile[] { wo, wo, wo, FurnisherItemTile.Empty, wo, FurnisherItemTile.Empty, wo, wo, wo },
            }, 1.8, 1.8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { wo, wo, wo, FurnisherItemTile.Empty, wo, FurnisherItemTile.Empty, wo, FurnisherItemTile.Empty, wo, wo, wo },
                new FurnisherItemTile[] { wo, FurnisherItemTile.Empty, al, ap, al, ap, al, ap, al, FurnisherItemTile.Empty, wo },
                new FurnisherItemTile[] { wo, ca, al, es, al, es, al, es, al, ca, wo },
                new FurnisherItemTile[] { wo, FurnisherItemTile.Empty, al, ap, al, ap, al, ap, al, FurnisherItemTile.Empty, wo },
                new FurnisherItemTile[] { wo, wo, wo, FurnisherItemTile.Empty, wo, FurnisherItemTile.Empty, wo, FurnisherItemTile.Empty, wo, wo, wo },
            }, 2.2, 2.2);

            flush(3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { b3, b1, b0, b1, b3 },
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { b3, b1, b2, b0, b1, b2, b3 },
            }, 7);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { b3, b1, b2, b1, b0, b1, b2, b1, b3 },
            }, 9);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { b3, b1, b2, b1, b1, b0, b1, b1, b2, b1, b3 },
            }, 11);

            flush(3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { cs },
            }, 1);

            flush(3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { eb, eb },
                new FurnisherItemTile[] { eb, eb },
            }, 1);

            flush(3);

            FurnisherItemTools.makeFloor(this, path);
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return true;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            TempleInstance ins = new TempleInstance(blue, area, init);
            foreach (COORDINATE c in ins.body())
            {
                if (ins.is(c) && SETT.ROOMS().fData.tile.get(c) == ca)
                {
                    SETT.LIGHTS().candle(c.x(), c.y(), 0);
                }
            }
            return ins;
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }
    }
}