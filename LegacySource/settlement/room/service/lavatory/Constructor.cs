using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using util;
using util.rendering;

namespace settlement.room.service.lavatory
{
    class Constructor : Furnisher
    {
        private readonly ROOM_LAVATORY blue;

        public FurnisherStat Latrines { get; }
        public FurnisherStat Workers { get; }
        public FurnisherStat Basins { get; }

        protected Constructor(ROOM_LAVATORY blue, RoomInitData init) : base(init, 2, 3, 88, 44)
        {
            this.blue = blue;

            Latrines = new FurnisherStat.FurnisherStatServices(this, blue);
            Workers = new FurnisherStat.FurnisherStatEmployeesR(this, Latrines, 1.0 / 8.0);
            Basins = new FurnisherStat.FurnisherStatRelative(this, Latrines);

            Json sp = init.data().json("SPRITES");

            RoomSprite sNick = new RoomSprite1x1(sp, "NICKNACK_1X1");

            RoomSprite SToil = new RoomSpriteCombo(sp, "SIT_COMBO")
            {
                private RoomSprite Rim = new RoomSprite1x1(sp, "SHITHOLE_1X1")
                {
                    protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return item.sprite(rx, ry) == null && (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation);
                    }
                };
                private RoomSprite Lid = new RoomSprite1x1(sp, "SHIT_LID_1X1");
                private RoomSprite Shit = new RoomSprite1x1(sp, "SHIT_1X1");

                public override byte GetData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return Rim.GetData(tx, ty, rx, ry, item, itemRan);
                }

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    if (blue.Is(it.tile()) && Lavatory.IsOpen(SETT.ROOMS().data.Get(it.tile())))
                    {
                        Shit.Render(r, s, GetData2(it), it, degrade, isCandle);
                        Rim.Render(r, s, GetData2(it), it, degrade, isCandle);
                    }
                    else
                    {
                        Lid.Render(r, s, GetData2(it), it, degrade, isCandle);
                    }

                    return false;
                }
            };

            RoomSprite SCentre = new RoomSpriteCombo(SToil)
            {
                private readonly RoomSprite Top = new RoomSpriteCombo(sp, "SIT_ONTOP_COMBO")
                {
                    protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                    }
                };

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    return false;
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    Top.Render(r, s, GetData2(it), it, degrade, rotates);
                    if (!SETT.ROOMS().fData.candle.Is(it.tile()))
                    {
                        if ((GUTIL.Ran2().Get(it.tile()) & 0b11) == 0)
                            sNick.Render(r, s, 0, it, degrade, false);
                    }
                }

                public override byte GetData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return Top.GetData(tx, ty, rx, ry, item, itemRan);
                }
            }.sData(1);

            RoomSprite sCentreEdge = new RoomSpriteCombo(SToil)
            {
                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.Is(it.tile()))
                        sNick.Render(r, s, 0, it, degrade, false);
                }
            };

            RoomSprite sSink = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                private RoomSprite Water = new RoomSprite1x1(sp, "BASIN_WATER_1X1");
                private RoomSprite Basin = new RoomSprite1x1(sp, "BASIN_1X1")
                {
                    protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return item.sprite(rx, ry) == null && (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation);
                    }
                };

                public override byte GetData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return Basin.GetData(tx, ty, rx, ry, item, itemRan);
                }

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    Water.Render(r, s, GetData2(it), it, degrade, isCandle);
                    return false;
                }
            };

            FurnisherItemTile tt = new FurnisherItemTile(SToil);
            FurnisherItemTile ce = new FurnisherItemTile(sCentreEdge);
            FurnisherItemTile ww = new FurnisherItemTile(sSink);
            FurnisherItemTile m1 = new FurnisherItemTile(sSink);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ce, ce, ce, ce, ce, ce, ce, ce },
                { ce, tt, tt, tt, tt, tt, tt, ce },
                { ce, tt, tt, tt, tt, tt, tt, ce },
                { ce, tt, tt, tt, tt, tt, tt, ce },
                { ce, tt, tt, ce, ce, tt, tt, ce },
                { ce, tt, ce, ce, ce, ce, tt, ce },
                { ce, tt, ce, ce, ce, ce, tt, ce },
                { ce, ce, ce, ce, ce, ce, ce, ce },
            }, 1, 3);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, m1 },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, m1, ww },
            }, 2);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, ww, m1, ww },
            }, 3);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, ww, m1, ww, ww },
            }, 4);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, m1 },
                { ww, ww },
            }, 3);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, m1, ww },
                { ww, m1, ww },
            }, 5);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, ww, m1, ww },
                { ww, ww, m1, ww },
            }, 7);

            new FurnisherItem(new FurnisherItemTile[,] {
                { ww, ww, m1, ww, ww },
                { ww, ww, m1, ww, ww },
            }, 9);

            Flush(0, 3);
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
            return new LavatoryInstance(blue, area, init);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool IsHeavy()
        {
            return true;
        }
    }
}