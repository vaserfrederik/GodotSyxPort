using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Service.Hearth
{
    public class Constructor : Furnisher
    {
        private readonly ROOM_HEARTH blue;

        public readonly FurnisherStat services = new FurnisherStat.FurnisherStatI(this);

        public const int codeService = 1;
        public const int codeFire = 2;

        protected Constructor(ROOM_HEARTH blue, RoomInitData init) : base(init, 1, 1, 88, 44)
        {
            this.blue = blue;

            Json sp = init.data().json("SPRITES");

            RoomSprite sBench = new RoomSprite1x1(sp, "BENCH_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    DIR d2 = DIR.ORTHO.getC(item.rotation + 1);

                    if (d2.x() * d.x() == 0 && d2.y() * d.y() == 0)
                        return false;

                    if (item.get(rx - d.x() * 4, ry - d.y() * 4) == null)
                        return true;
                    return false;
                }
            };

            RoomSprite sHearth = new RoomSpriteCombo(sp, "HEARTH_COMBO");

            RoomSprite sFire = new RoomSpriteCombo(sHearth)
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, false);
                    if (blue.is(it.tile()))
                    {
                        HearthInstance ins = blue.getter.get(it.tile());

                        RESOURCES.WOOD().renderLaying(r, it.x(), it.y(), it.ran(), 5);

                        SETT.LIGHTS().hide(it.tx(), it.ty(), ins.used == 0);
                    }
                    return false;
                }
            };

            FurnisherItemTile ff = new FurnisherItemTile(
                this,
                sFire,
                AVAILABILITY.SOLID,
                false).setData(codeFire);

            FurnisherItemTile fe = new FurnisherItemTile(
                this,
                sHearth,
                AVAILABILITY.SOLID,
                false);

            FurnisherItemTile bb = new FurnisherItemTile(
                this,
                false,
                sBench,
                AVAILABILITY.PENALTY4,
                false).setData(codeService);

            FurnisherItemTile __ = new FurnisherItemTile(
                this,
                null,
                AVAILABILITY.ROOM, false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, bb, __, bb, __ },
                { __, bb, ff, bb, __ },
                { __, bb, __, bb, __ },
            }, 6, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, bb, __, bb, __ },
                { __, bb, __, bb, __ },
                { __, bb, ff, bb, __ },
                { __, bb, __, bb, __ },
                { __, bb, __, bb, __ },
            }, 10, 10);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, bb, __, bb, __ },
                { __, bb, __, bb, __ },
                { __, bb, fe, bb, __ },
                { __, bb, ff, bb, __ },
                { __, bb, fe, bb, __ },
                { __, bb, __, bb, __ },
                { __, bb, __, bb, __ },
            }, 14, 14);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, bb, bb, __, bb, bb, __ },
                { __, bb, bb, fe, bb, bb, __ },
                { __, bb, bb, ff, bb, bb, __ },
                { __, bb, bb, fe, bb, bb, __ },
                { __, bb, bb, ff, bb, bb, __ },
                { __, bb, bb, fe, bb, bb, __ },
                { __, bb, bb, __, bb, bb, __ },
            }, 28, 28);

            flush(1, 3);
        }

        public override bool usesArea()
        {
            return false;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new HearthInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        // private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        // {
        //     {0,0,0,0,0,0,0,0},
        //     {0,0,0,0,0,0,0,0},
        //     {0,0,0,1,1,0,0,0},
        //     {0,1,1,1,1,1,1,0},
        //     {0,1,1,1,1,1,1,0},
        //     {0,1,1,1,1,1,1,0},
        //     {0,0,0,0,0,0,0,0},
        //     {0,0,0,0,0,0,0,0},
        // },
        // miniColor
        // );

        // public override COLOR miniColor(int tx, int ty)
        // {
        //     return miniC.get(tx, ty);
        // }
    }
}