using System;
using System.IO;
using init.constant;
using init.type;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.thing.ThingsCadavers;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.food.hunter
{
    internal sealed class Constructor : Furnisher
    {
        private readonly ROOM_HUNTER blue;

        internal readonly FurnisherStat workers = new FurnisherStat(this, 1)
        {
            Get = (area, fromItems) => fromItems,
            Format = (t, value) => GFORMAT.i(t, (int)value)
        };

        internal readonly FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);

        internal readonly FurnisherStat output = new FurnisherStat(this, 0)
        {
            Get = (area, fromItems) =>
            {
                double a = workers.Get(area, fromItems);
                return a * blue.indus.get(0).outs().get(0).rate * blue.bonus().get(HCLASS_RACE.clP()) * blue.eBonus((int)Math.Ceiling(a));
            },
            Format = (t, value) => GFORMAT.i(t, (int)value)
        };

        internal readonly FurnisherItemTile ww;
        internal readonly FurnisherItemTile rr;

        protected Constructor(ROOM_HUNTER blue, RoomInitData init) : base(init, 2, 3)
        {
            this.blue = blue;

            Json j = init.data().json("SPRITES");

            RoomSpriteCombo table = new RoomSpriteCombo(j, "TABLE_COMBO")
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    bool ret = base.Render(r, s, data, it, degrade, isCandle);
                    if (isCandle)
                        return ret;
                    Cadaver ca = SETT.THINGS().cadavers.tGet.get(it.tx(), it.ty());
                    if (ca != null)
                    {
                        ca.spec().blood.bind();
                        long ran = it.bigRan();
                        int a = Job.gore.get(SETT.ROOMS().data.get(it.tile()));
                        int cx = it.x() + C.TILE_SIZEH;
                        int cy = it.y() + C.TILE_SIZEH;
                        for (int i = 0; i < a; i++)
                        {
                            int xx = (int)(cx + (-4 + (ran & 0x07)) * C.SCALE);
                            ran = ran >> 3;
                            int yy = (int)(cy + (-4 + (ran & 0x07)) * C.SCALE);
                            ran = ran >> 3;

                            SETT.THINGS().sprites.bloodPool.render(r, (int)(ran & 0x0F), xx, yy);
                            ran = ran >> 4;
                        }
                        COLOR.unbind();
                    }
                    return ret;
                }
            };

            RoomSprite sNick = new RoomSprite1x1(j, "NICKNACK_1X1");

            RoomSprite stable2 = new RoomSpriteCombo(table)
            {
                RenderAbove = (r, s, data, it, degrade) =>
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                        sNick.render(r, s, getData2(it), it, degrade, false);
                },
                GetData2 = (tx, ty, rx, ry, item, itemRan) => sNick.getData2(tx, ty, rx, ry, item, itemRan)
            };

            RoomSprite sstorage = new RoomSprite1x1(j, "STORAGE_1X1")
            {
                RenderAbove = (r, s, data, it, degrade) =>
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                        sNick.render(r, s, getData2(it), it, degrade, false);
                },
                GetData2 = (tx, ty, rx, ry, item, itemRan) => sNick.getData2(tx, ty, rx, ry, item, itemRan)
            };

            ww = new FurnisherItemTile(this, true, table, AVAILABILITY.ROOM_SOLID, false);
            rr = new FurnisherItemTile(this, true, table, AVAILABILITY.ROOM_SOLID, false);
            FurnisherItemTile mm = new FurnisherItemTile(this, false, stable2, AVAILABILITY.ROOM_SOLID, true);
            FurnisherItemTile nn = new FurnisherItemTile(this, false, sstorage, AVAILABILITY.ROOM_SOLID, false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ww, rr, mm },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ww, rr, mm },
                { ww, rr, mm },
            }, 2);

            flush(3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { mm },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm },
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, nn },
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, mm, nn },
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, mm, mm, nn },
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm },
                { nn, mm },
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, nn },
                { nn, mm, nn },
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, mm, nn },
                { nn, mm, mm, nn },
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, mm, mm, nn },
                { nn, mm, mm, mm, nn },
            }, 10);

            flush(3);
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new HunterInstance(blue, area, init);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool IsHeavy()
        {
            return true;
        }

        // private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        // {
        //     { 0, 0, 0, 0, 0, 0, 0, 0 },
        //     { 0, 1, 1, 1, 0, 0, 0, 0 },
        //     { 0, 0, 1, 0, 1, 0, 0, 0 },
        //     { 0, 0, 1, 0, 1, 0, 0, 0 },
        //     { 0, 0, 1, 0, 1, 0, 0, 0 },
        //     { 0, 0, 1, 0, 1, 0, 0, 0 },
        //     { 0, 1, 1, 1, 0, 0, 0, 0 },
        //     { 0, 0, 0, 0, 0, 0, 0, 0 },
        // },
        // miniColor
        // );

        // public override COLOR MiniColor(int tx, int ty)
        // {
        //     return miniC.get(tx, ty);
        // }
    }
}