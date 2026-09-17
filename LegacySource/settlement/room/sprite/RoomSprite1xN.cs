using System;
using System.IO;
using settlement.room.main.furnisher;
using snake2d;
using util.rendering;
using util.file;

namespace settlement.room.sprite
{
    public class RoomSprite1xN : RoomSprite1x1
    {
        private readonly bool master;

        public RoomSprite1xN(Json json, string key, bool master) : base(json, key)
        {
            this.master = master;
        }

        public RoomSprite1xN(RoomSprite other, bool master) : base(other)
        {
            this.master = master;
        }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            it.RanOffset(OffX(data), OffY(data));
            return base.Render(r, s, GetRot(data), it, degrade, isCandle);
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            DIR d = Rot(data);
            int m = d.Mask();
            if ((data & 0b0100) != 0)
            {
                m |= d.Perpendicular().Mask();
            }
            SheetType.sCombo.RenderOverlay(x, y, r, item.Get(rx, ry).Availability, m, 0, false);
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            int m = Math.Max(item.Width(), item.Height());
            int res = -1;

            for (int dist = 1; dist < m; dist++)
            {
                for (int di = 0; di < DIR.ORTHO.Size; di++)
                {
                    DIR d = DIR.ORTHO.Get(di);

                    if (IsMaster(rx + d.X() * dist, ry + d.Y() * dist, item))
                    {
                        if (res != -1)
                        {
                            res |= 0b0100;
                            return (byte)res;
                        }
                        res = (di | (dist << 3));
                    }
                }
            }

            if (res != -1)
                return (byte)res;
            
            for (int dist = 1; dist < m; dist++)
            {
                for (int di = 0; di < DIR.ORTHO.Size; di++)
                {
                    DIR d = DIR.ORTHO.Get(di);

                    if (Joins(rx + d.X() * dist, ry + d.Y() * dist, item, !master))
                    {
                        if (res != -1)
                        {
                            res |= 0b0100;
                            return (byte)res;
                        }
                        res = (di | (dist << 3));
                    }
                }
            }
            
            
            if (res != -1)
                return (byte)res;
            return (byte)item.Rotation;
        }

        protected bool IsMaster(int rx, int ry, FurnisherItem item)
        {
            RoomSprite s = item.Sprite(rx, ry);
            if (s == null)
                return false;
            if (s is RoomSprite1xN)
            {
                return master != ((RoomSprite1xN)s).master;
            }
            return false;
        }

        protected bool Joins(int rx, int ry, FurnisherItem item, bool master)
        {
            RoomSprite s = item.Sprite(rx, ry);
            if (s == null)
                return false;
            if (s is RoomSprite1xN)
            {
                return master != ((RoomSprite1xN)s).master;
            }
            return false;
        }

        protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
        {
            RoomSprite s = item.Sprite(rx, ry);
            if (s == null)
                return false;
            if (s is RoomSprite1xN)
            {
                return master != ((RoomSprite1xN)s).master;
            }
            return false;
        }

        public int OffX(int data)
        {
            if (master)
                return 0;
            DIR d = Rot(data);
            return d.X() * ((data >> 3) & 0b011111);
        }

        public int OffY(int data)
        {
            if (master)
                return 0;
            DIR d = Rot(data);
            return d.Y() * ((data >> 3) & 0b011111);
        }
    }
}