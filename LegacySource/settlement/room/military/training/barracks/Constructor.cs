using System;
using System.IO;
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

namespace settlement.room.military.training.barracks
{
    internal sealed class Constructor : Furnisher
    {
        private readonly FurnisherItemTile manikin;
        private readonly FurnisherItemTile work;
        private readonly ROOM_BARRACKS blue;

        private readonly FurnisherStat men = new FurnisherStat(this, 0)
        {
            Get = (area, fromItems) => fromItems,
            Format = (t, value) => GFORMAT.i(t, (int)value)
        };

        public Constructor(ROOM_BARRACKS blue, RoomInitData init) : base(init, 1, 1, 88, 44)
        {
            this.blue = blue;
            Json js = init.Data().Json("SPRITES");
            RoomSpriteBoxN sPedi = new RoomSpriteBoxN(js, "PODEUM_BOX")
            {
                Joins = (tx, ty, rx, ry, d, item) => item.Sprite(rx, ry) != null,
                RenderBelow = (r, s, data, it, degrade) => base.Render(r, s, data, it, degrade, false),
                Render = (r, s, data, it, degrade, isCandle) => false
            };

            RoomSprite1x1 sMani = new RoomSprite1x1(js, "MANAKIN_A_1X1")
            {
                sMani2 = new RoomSprite1x1(js, "MANAKIN_B_1X1"),
                RenderBelow = (r, s, data, it, degrade) => sPedi.RenderBelow(r, s, GetData2(it), it, degrade),
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    int rot = (it.Ran() >> 4);
                    if (blue.Is(it.Tile()) && BarracksThing.Used.Is(ROOMS().Data.Get(it.Tile())))
                        rot += GAME.Intervals().Get05();
                    rot &= 0x07;
                    if ((rot & 1) == 1)
                        return sMani2.Render(r, s, rot >> 1, it, degrade, false);
                    else
                        return base.Render(r, s, rot >> 1, it, degrade, false);
                },
                GetData2 = (tx, ty, rx, ry, item, itemRan) => sPedi.GetData(tx, ty, rx, ry, item, itemRan)
            };

            RoomSprite1x1 sCandle = new RoomSprite1x1(js, "TABLE_1X1")
            {
                RenderBelow = (r, s, data, it, degrade) => sPedi.RenderBelow(r, s, GetData2(it), it, degrade),
                GetData2 = (tx, ty, rx, ry, item, itemRan) => sPedi.GetData(tx, ty, rx, ry, item, itemRan)
            };

            FurnisherItemTile ma = new FurnisherItemTile(this, false, sMani, AVAILABILITY.ROOM_SOLID, false);
            FurnisherItemTile ca = new FurnisherItemTile(this, false, sCandle, AVAILABILITY.ROOM_SOLID, true);
            FurnisherItemTile st = new FurnisherItemTile(this, true, sPedi, AVAILABILITY.AVOID_PASS, false);
            FurnisherItemTile ee = new FurnisherItemTile(this, false, sPedi, AVAILABILITY.AVOID_PASS, false);

            manikin = ma;
            work = st;

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ma, ca },
                { st, ee }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ma, ca, ma },
                { st, ee, st }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ma, ca, ma, ma },
                { st, ee, st, st }
            }, 3.0);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ma, ma, ca, ma, ma },
                { st, st, ee, st, st }
            }, 4.0);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ma, ma, ma, ca, ma, ma },
                { st, st, st, ee, st, st }
            }, 5.0);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ma, ma, ma, ca, ma, ma, ma },
                { st, st, st, ee, st, st, st }
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { st, st },
                { ma, ma },
                { ma, ca },
                { st, ee }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { st, st, st },
                { ma, ma, ma },
                { ma, ca, ma },
                { st, ee, st }
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { st, st, st, st },
                { ma, ma, ma, ma },
                { ma, ca, ma, ma },
                { st, ee, st, st }
            }, 7);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { st, st, st, st, st },
                { ma, ma, ma, ma, ma },
                { ma, ma, ca, ma, ma },
                { st, st, ee, st, st }
            }, 9);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { st, st, st, st, st, st },
                { ma, ma, ma, ma, ma, ma },
                { ma, ma, ma, ca, ma, ma },
                { st, st, st, ee, st, st }
            }, 11);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { st, st, st, st, st, st, st },
                { ma, ma, ma, ma, ma, ma, ma },
                { ma, ma, ma, ca, ma, ma, ma },
                { st, st, st, ee, st, st, st }
            }, 13);

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

        public override ROOM_BARRACKS Blue()
        {
            return blue;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new BarracksInstance(blue, area, init);
        }

        public override bool IsHeavy()
        {
            return true;
        }
    }
}