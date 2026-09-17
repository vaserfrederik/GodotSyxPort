using System;
using System.IO;
using init.resources;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;
using util.rendering.ShadowBatch;

namespace settlement.room.infra.stockpile
{
    internal class Constructor : Furnisher
    {
        private readonly FurnisherStat storage = new FurnisherStat(this)
        {
            Get = (area, fromItems) => fromItems * (blue.upgrades().Boost(0) - 1),
            Format = (t, value) => GFORMAT.i(t, (int)value)
        };

        private readonly ROOM_STOCKPILE blue;
        private readonly FurnisherItemTile cr;

        public Constructor(ROOM_STOCKPILE blue, RoomInitData init)
            : base(init, 1, 1, 88, 44)
        {
            this.blue = blue;

            var sp = init.data().json("SPRITES");
            var spriteCrate = new RoomSprite1x1(sp, "CRATE_BOTTOM_1X1")
            {
                Top = new RoomSprite1x1(sp, "CRATE_TOP_1X1"),
                Topf = new RoomSprite1x1(sp, "CRATE_TOP_FOOD_1X1"),
                RenderAbove = (r, s, data, it, degrade) =>
                {
                    var top = this.Top;
                    if (SETT.ROOMS().STOCKPILE.Is(it.tile()))
                    {
                        var ins = blue.getter.Get(it.tx(), it.ty());
                        var cr = blue.crate.Get(it.tx(), it.ty(), ins, ins.sdata);
                        var res = cr.resource();
                        if (res != null && RESOURCES.EDI().Is(res))
                        {
                            top = topf;
                        }
                    }
                    top.Render(r, s, data, it, degrade, rotates);
                },
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    if (base.Render(r, s, data, it, degrade, false))
                    {
                        if (SETT.ROOMS().STOCKPILE.Is(it.tile()))
                        {
                            var ins = blue.getter.Get(it.tx(), it.ty());
                            var cr = blue.crate.Get(it.tx(), it.ty(), ins, ins.sdata);
                            var res = cr.resource();
                            if (res != null)
                            {
                                var a = cr.amount();
                                res.RenderLayingRel(r, it.x(), it.y(), it.ran(), a / blue.upgrades().Boost(SETT.ROOMS().STOCKPILE.getter.Get(it.tx(), it.ty()).upgrade()));
                            }
                        }
                    }
                    return false;
                }
            };

            var spriteMisc = new RoomSprite1x1(sp, "MISC_1X1");

            cr = new FurnisherItemTile(
                this,
                true,
                spriteCrate,
                AVAILABILITY.ROOM_SOLID,
                false
            );

            var mm = new FurnisherItemTile(
                this,
                false,
                spriteMisc,
                AVAILABILITY.ROOM_SOLID,
                true
            );

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, mm },
            }, 2, 1);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr, mm },
            }, 3, 2);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr, cr, mm },
            }, 4, 3);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr, cr, cr, mm },
            }, 5, 4);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr, cr, cr, cr, mm },
            }, 6, 5);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr, cr, cr, cr, cr, mm },
            }, 7, 6);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr },
                { mm, mm },
            }, 4, 2);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr },
                { cr, cr },
                { mm, mm },
            }, 6, 4);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { mm, mm },
            }, 8, 6);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { mm, mm },
            }, 10, 8);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { mm, mm },
            }, 12, 10);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { cr, cr },
                { mm, mm },
            }, 14, 12);

            Flush(1, 3);
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool IsHeavy()
        {
            return true;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new StockpileInstance(blue, area, init);
        }
    }
}