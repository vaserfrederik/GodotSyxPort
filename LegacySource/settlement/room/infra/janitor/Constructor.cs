using System;
using System.IO;
using Newtonsoft.Json;

namespace Settlement.Room.Infra.Janitor
{
    public class Constructor : Furnisher
    {
        private readonly ROOM_JANITOR blue;
        public FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
        public FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
        public FurnisherItemTile ta;

        protected Constructor(ROOM_JANITOR blue, RoomInitData init)
            : base(init, 2, 2, 88, 44)
        {
            this.blue = blue;

            var sp = init.Data().Json("SPRITES");

            RoomSprite res = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                public void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (SETT.ROOMS().fData.candle.Is(it.Tile()))
                        return;
                    JanitorInstance ins = blue.Getter.Get(it.Tile());
                    if (ins != null)
                    {
                        int ri = (int)(ins.tableRes >> (8 * ((it.Tx() + it.Ty()) % 8)));
                        ri &= 0x0FF;
                        if (ri != 0)
                            RESOURCES.ALL().Get(ri - 1).RenderLaying(r, it.X(), it.Y(), it.Ran(), ins.bits.resAm(RESOURCES.ALL().GetC(ri - 1)));
                    }
                }
            };

            RoomSprite table = new RoomSpriteCombo(res)
            {
                public RoomSprite1x1 top = new RoomSprite1x1(sp, "TABLE_MISC_1X1");
                public void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.RenderRandom(r, s, it, it.Ran(), degrade);
                }
            };

            RoomSprite1x1 top = new RoomSprite1x1(sp, "MISC_1X1");

            RoomSprite nickGround = new RoomSprite1x1(sp, "STORAGE_1X1")
            {
                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    if (!isCandle)
                    {
                        top.RenderRandom(r, s, it, it.Ran(), degrade);
                    }
                    return false;
                }
            };

            FurnisherItemTile tc = new FurnisherItemTile(
                this,
                table,
                AVAILABILITY.ROOM_SOLID,
                true);

            ta = new FurnisherItemTile(
                this,
                true,
                res,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(1);

            FurnisherItemTile ng = new FurnisherItemTile(
                this,
                nickGround,
                AVAILABILITY.ROOM_SOLID,
                true);

            FurnisherItemTile nn = new FurnisherItemTile(
                this,
                top,
                AVAILABILITY.ROOM_SOLID,
                false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tc, ta, ta, ta, tc},
            }, 1);

            Flush(1, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {nn, ng},
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {nn, ng, nn},
            }, 1.5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {nn, ng, ng, nn},
            }, 2);

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

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new JanitorInstance(blue, area, init);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool IsHeavy()
        {
            return true;
        }

        //private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        //{
        //    {0, 0, 0, 0, 0, 0, 0, 0},
        //    {0, 1, 0, 0, 0, 0, 0, 0},
        //    {0, 1, 0, 0, 0, 0, 0, 0},
        //    {0, 1, 1, 1, 1, 1, 1, 0},
        //    {0, 1, 1, 1, 1, 1, 1, 0},
        //    {0, 1, 1, 1, 1, 1, 1, 0},
        //    {0, 0, 0, 0, 0, 0, 0, 0},
        //    {0, 0, 0, 0, 0, 0, 0, 0},
        //},
        //miniColor);

        //public override COLOR MiniColor(int tx, int ty)
        //{
        //    return miniC.Get(tx, ty);
        //}
    }
}