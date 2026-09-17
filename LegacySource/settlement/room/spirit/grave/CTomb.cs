using System;
using System.IO;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.spirit.grave
{
    internal class CTomb : Furnisher
    {
        private readonly ROOM_TOMB blue;

        public readonly FurnisherStat workers;
        public readonly FurnisherStat services;
        public readonly FurnisherStat respekk;

        private readonly RoomSprite sHead;

        protected CTomb(ROOM_TOMB blue, RoomInitData init)
            : base(init, 2, 3, 88, 44)
        {
            this.blue = blue;

            workers = new FurnisherStat.FurnisherStatEmployees(this, 0);
            services = new FurnisherStat.FurnisherStatI(this);
            respekk = new FurnisherStat(this)
            {
                Get = (AREA area, double fromItems) =>
                {
                    fromItems /= area.Area();
                    fromItems *= 2;
                    return CLAMP.d(fromItems, 0, 1.0);
                },
                Format = (GText t, double value) => GFORMAT.perc(t, value)
            };

            Json sData = init.Data().Json("SPRITES");

            sHead = new RoomSprite1xN(sData, "HEAD_BOTTOM_1X1", false)
            {
                Lid = new RoomSprite1xN(sData, "HEAD_TOP_1X1", false),

                Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    if (blue.Is(it.Tile()))
                    {
                        int x = it.Tx() + offX(data);
                        int y = it.Ty() + offY(data);
                        if (Grave.IsUsed(x, y))
                        {
                            Lid.Render(r, s, GetData2(it), it, degrade, isCandle);
                        }
                    }
                    return false;
                },

                GetData2 = (int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) => Lid.GetData(tx, ty, rx, ry, item, itemRan)
            };

            var sFoot = new RoomSprite1xN(sData, "TAIL_BOTTOM_1X1", true)
            {
                Lid = new RoomSprite1xN(sData, "TAIL_TOP_1X1", true),

                Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    if (blue.Is(it.Tile()))
                    {
                        if (Grave.IsUsed(it.Tx(), it.Ty()))
                        {
                            Lid.Render(r, s, GetData2(it), it, degrade, isCandle);
                        }
                    }
                    return false;
                },

                GetData2 = (int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) => Lid.GetData(tx, ty, rx, ry, item, itemRan)
            };

            var sStone = new RoomSprite1x1(sData, "STONE_1X1");

            var sSmall = new RoomSprite1x1(sData, "MONUMENT_1x1");
            var sMedium = new RoomSpriteXxX(sData, "MONUMENT_2x2", 2);
            var sLarge = new RoomSpriteXxX(sData, "MONUMENT_3x3", 3);

            var ss = new FurnisherItemTile(
                this,
                false,
                sSmall,
                AVAILABILITY.SOLID,
                false
            );

            var sm = new FurnisherItemTile(
                this,
                false,
                sMedium,
                AVAILABILITY.SOLID,
                false
            );

            var sl = new FurnisherItemTile(
                this,
                false,
                sLarge,
                AVAILABILITY.SOLID,
                false
            );

            var h1 = new FurnisherItemTile(
                this,
                false,
                sHead,
                AVAILABILITY.SOLID,
                false
            );

            var t1 = new FurnisherItemTile(
                this,
                true,
                sFoot,
                AVAILABILITY.SOLID,
                false).SetData(Grave.ITEM_MARK);

            var st = new FurnisherItemTile(
                this,
                false,
                sStone,
                AVAILABILITY.SOLID,
                true
            );

            var __ = new FurnisherItemTile(
                this,
                false,
                null,
                AVAILABILITY.ROOM,
                false
            );

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { st, h1, st },
                new FurnisherItemTile[] { __, t1, __ }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { st, h1, h1, st },
                new FurnisherItemTile[] { __, t1, t1, __ }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { st, h1, h1, st, h1, st },
                new FurnisherItemTile[] { __, t1, t1, __, t1, __ }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { st, h1, h1, st, h1, h1, st },
                new FurnisherItemTile[] { __, t1, t1, __, t1, t1, st }
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { st, h1, h1, st, h1, h1, st, h1, st },
                new FurnisherItemTile[] { __, t1, t1, __, t1, t1, __, t1, __ }
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { st, h1, h1, st, h1, h1, st, h1, h1, st },
                new FurnisherItemTile[] { __, t1, t1, __, t1, t1, __, t1, t1, __ }
            }, 6);

            Flush(1, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ss }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { sm, sm },
                new FurnisherItemTile[] { sm, sm }
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { sl, sl, sl },
                new FurnisherItemTile[] { sl, sl, sl },
                new FurnisherItemTile[] { sl, sl, sl }
            }, 9);

            Flush(3);
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
            return new GraveInstance(blue, area, init);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }
    }
}