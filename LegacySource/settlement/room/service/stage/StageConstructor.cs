using System;
using System.IO;
using snake2d.util.datatypes;
using snake2d.util.file;

namespace settlement.room.service.stage
{
    final class StageConstructor : Furnisher
    {
        private readonly ROOM_STAGE blue;

        public const int STATION = 1;
        public readonly FurnisherStatI workers;
        public readonly FurnisherStat spectators;

        protected StageConstructor(ROOM_STAGE blue, RoomInitData init)
            : base(init, 1, 2)
        {
            this.blue = blue;

            workers = new FurnisherStatI(this);
            spectators = new FurnisherStat.FurnisherStatServices(this, blue);

            Json sp = init.data().json("SPRITES");

            RoomSpriteBoxN first = new RoomSpriteBoxN(sp, "A_BOX")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() >= 0;
                }
            };
            first.sData(0);

            RoomSpriteBoxN second = new RoomSpriteBoxN(sp, "B_BOX")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() >= 1;
                }
            };
            second.sData(1);

            RoomSpriteBoxN third = new RoomSpriteBoxN(sp, "C_BOX")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() >= 2;
                }
            };
            third.sData(2);

            FurnisherItemTile AA = new FurnisherItemTile(
                this,
                first,
                AVAILABILITY.PENALTY4,
                false
            );
            AA.setData(STATION);
            final FurnisherItemTile aa = new FurnisherItemTile(
                this,
                false,
                first,
                AVAILABILITY.ROOM,
                false
            );
            final FurnisherItemTile ai = new FurnisherItemTile(
                this,
                first,
                AVAILABILITY.SOLID,
                true
            );

            FurnisherItemTile BB = new FurnisherItemTile(
                this,
                second,
                AVAILABILITY.PENALTY4,
                false
            );
            BB.setData(STATION);
            final FurnisherItemTile bb = new FurnisherItemTile(
                this,
                second,
                AVAILABILITY.ROOM,
                false
            );

            FurnisherItemTile CC = new FurnisherItemTile(
                this,
                third,
                AVAILABILITY.PENALTY4,
                false
            );
            CC.setData(STATION);
            final FurnisherItemTile cc = new FurnisherItemTile(
                this,
                third,
                AVAILABILITY.PENALTY4,
                false
            );

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ai, aa, aa, aa, aa, aa, aa, aa, ai},
                {aa, bb, bb, bb, bb, bb, bb, bb, aa},
                {aa, bb, cc, cc, cc, cc, cc, bb, aa},
                {aa, bb, cc, CC, CC, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, CC, CC, cc, bb, aa},
                {aa, bb, cc, cc, cc, cc, cc, bb, aa},
                {aa, bb, bb, bb, bb, bb, bb, bb, aa},
                {ai, aa, aa, aa, aa, aa, aa, aa, ai}
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ai, aa, aa, aa, aa, aa, aa, ai},
                {aa, bb, bb, bb, bb, bb, bb, aa},
                {aa, bb, cc, cc, cc, cc, bb, aa},
                {aa, bb, cc, CC, CC, cc, bb, aa},
                {aa, bb, cc, CC, CC, cc, bb, aa},
                {aa, bb, cc, CC, CC, cc, bb, aa},
                {aa, bb, cc, CC, CC, cc, bb, aa},
                {aa, bb, cc, CC, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {ai, aa, aa, aa, aa, aa, aa, ai}
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ai, aa, aa, aa, aa, aa, ai},
                {aa, bb, bb, bb, bb, bb, aa},
                {aa, bb, cc, cc, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, CC, cc, bb, aa},
                {aa, bb, cc, cc, cc, bb, aa},
                {aa, bb, bb, bb, bb, bb, aa},
                {ai, aa, aa, aa, aa, aa, ai}
            }, 1);

            flush(1, 1);
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
            return new StageInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        //private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        //{
        //    {0,0,0,0,0,0,0,0},
        //    {0,1,0,0,0,0,0,0},
        //    {0,1,0,0,0,0,0,0},
        //    {0,1,1,1,1,1,1,0},
        //    {0,1,1,1,1,1,1,0},
        //    {0,1,1,1,1,1,1,0},
        //    {0,0,0,0,0,0,0,0},
        //    {0,0,0,0,0,0,0,0},
        //},
        //miniColor);

        //public override COLOR miniColor(int tx, int ty)
        //{
        //    return miniC.get(tx, ty);
        //}
    }
}