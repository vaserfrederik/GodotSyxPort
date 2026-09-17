using System;
using System.IO;

namespace Settlement.Room.Sprite
{
    using Init.Sprite.Game;
    using Snake2D;
    using Util.Rendering;
    using Util.Datatypes;

    public class RoomSpriteXxX : RoomSpriteImp
    {
        private int rotation = 0;
        private readonly SheetType.CXxX type;

        public RoomSpriteXxX(Json json, string key, int size) : base(Size(size), json, key)
        {
            this.type = Size(size);
        }

        public RoomSpriteXxX(int size) : base(Size(size))
        {
            this.type = Size(size);
        }

        private static SheetType.CXxX Size(int size)
        {
            if (size == 2)
                return SheetType.S2x2;
            else if (size == 3)
                return SheetType.S3x3;
            throw new Exception();
        }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int t = data & 0b0111111;
            int rot = Rot(data);
            int dx = type.Dx(data);
            int dy = type.Dy(data);

            it.RanOffset(-dx, -dy);
            int ran = it.Ran();
            SheetPair sheet = sheetPair(it, ran);
            if (sheet == null)
                return false;

            sheet.D.Color(ran).Bind();

            if (!sheet.D.Rotates)
            {
                rot = (ran >> 9) & 0b11;
            }

            int tile = type.Tile(sheet.S, sheet.D, t, Frame(sheet, it), rot);

            it.RanOffset(dx, dy);
            sheet.S.Render(sheet.D, it.X(), it.Y(), it, r, tile, ran, degrade);
            COLOR.Unbind();
            sheet.S.RenderShadow(sheet.D, it.X(), it.Y(), it, s, tile, ran);

            return false;
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            type.RenderOverlay(x, y, r, item.Get(rx, ry).Availability, data & 0b0111111, Rotates ? Rot(data) : -1, false);
        }

        public int Rot(int data)
        {
            return (data >> 6) & 0b11;
        }

        public int SetRot(int data, int rot)
        {
            data &= 0b0011_1111;
            data |= (rot & 0b11) << 6;
            return data;
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            int dx = type.Size - 1;
            int dy = type.Size - 1;

            for (int y = 0; y < type.Size; y++)
            {
                if (!Joins(tx, ty - y, rx, ry - y, null, item))
                {
                    dy = y - 1;
                    break;
                }
            }
            for (int x = 0; x < type.Size; x++)
            {
                if (!Joins(tx - x, ty, rx - x, ry, null, item))
                {
                    dx = x - 1;
                    break;
                }
            }

            int i = dx + type.Size * dy;

            i |= ((item.Rotation + rotation) & 0b11) << 6;

            return (byte)i;
        }

        public RoomSpriteXxX Rotate(int rotation)
        {
            this.rotation = rotation;
            return this;
        }

        protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
        {
            return item.Sprite(rx, ry) == this;
        }

        public override SheetType.CXxX Type()
        {
            return type;
        }
    }
}