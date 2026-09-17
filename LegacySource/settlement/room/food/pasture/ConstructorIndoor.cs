using System;
using System.IO;

namespace Settlement.Room.Food.Pasture
{
    public class ConstructorIndoor : Constructor
    {
        private ROOM_PASTURE blue;

        public const int STORAGE1 = 100;
        public const int STORAGE2 = 200;
        public const int STORAGE3 = 300;

        public readonly FurnisherItemTile s1;
        public readonly FurnisherItemTile s2;
        public readonly FurnisherItemTile s3;

        protected ConstructorIndoor(ROOM_PASTURE blue, RoomInitData init) : base(blue, init)
        {
            this.blue = blue;

            Json js = init.Data().Json("SPRITES");

            s1 = new FurnisherItemTile(this, true, new SpriteDep(js, blue.s1), AVAILABILITY.ROOM_SOLID, false).SetData(1);
            s1.SetData(STORAGE1);
            s2 = new FurnisherItemTile(this, true, new SpriteDep(js, blue.s2), AVAILABILITY.ROOM_SOLID, false).SetData(1);
            s2.SetData(STORAGE2);
            s3 = new FurnisherItemTile(this, true, new SpriteDep(js, blue.s3), AVAILABILITY.ROOM_SOLID, false).SetData(1);
            s3.SetData(STORAGE3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { s1, s2, s3 },
            }, 1, 1);

            Flush(1, 1, 3);

            MakeAux(js);
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override bool MustBeOutdoors()
        {
            return false;
        }

        public override ROOM_PASTURE Blue()
        {
            return blue;
        }

        public override bool NeedsIsolation()
        {
            return true;
        }

        public override void PutFloor(int tx, int ty, int upgrade, AREA area)
        {
            base.PutFloor(tx, ty, upgrade, area);
        }

        public override bool RemoveFertility()
        {
            return true;
        }

        private class SpriteDep : RoomSprite1x1
        {
            private readonly RoomResStorage st;

            public SpriteDep(Json json, RoomResStorage st) : base(json, "STORAGE_1X1")
            {
                this.st = st;
            }

            public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderData.RenderIterator it, double degrade, bool isCandle)
            {
                bool ret = base.Render(r, s, data, it, degrade, isCandle);
                st.Render(r, s, it.tx(), it.ty(), it.x(), it.y(), it.ran());

                return ret;
            }
        }

        protected override bool FenceJoin(FurnisherItemTile gc)
        {
            return true;
        }
    }
}