using System;
using System.IO;
using Init.Sprite.Game;
using Settlement.Room.Main.Furnisher;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Rnd;
using Util.Rendering;

namespace Settlement.Room.Sprite
{
    public class RoomSpriteBoxN : RoomSpriteImp
    {
        public RoomSpriteBoxN(Json json, string key) : base(SheetType.sBox, json, key)
        {
        }

        public RoomSpriteBoxN(RoomSprite other) : base(other)
        {
        }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int k = (data >> 5) & 0b0111;
            int rot = (data >> 4) & 1;
            SheetPair sheet = sheetPair(it, k);
            if (sheet == null)
                return false;
            sheet.d.color(k).bind();
            int ran = it.ran();

            int tile = type().tile(sheet.s, sheet.d, data & 0x0F, frame(sheet, it), rot);

            sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
            COLOR.unbind();
            if (s != null)
                sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
            return false;
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            type().renderOverlay(x, y, r, item.get(rx, ry).availability, data & 0x0F, 0, false);
        }

        public int rotMask(int data)
        {
            return (data & 0x0F);
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            int m = 0;
            int ri = RND.rInt(DIR.ORTHO.size());
            for (int i = 0; i < DIR.ORTHO.size(); i++)
            {
                int rr = (ri + i) % DIR.ORTHO.size();
                DIR d = DIR.ORTHO.get(rr);
                if (joins(tx + d.x(), ty + d.y(), rx + d.x(), ry + d.y(), d, item))
                    m |= d.mask();
            }
            m |= 0b00010000 * (item.rotation & 1);
            return (byte)(m | (itemRan << 5));
        }

        protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
        {
            FurnisherItemTile t = item.get(rx, ry);
            return t != null && t.sprite != null && t.sprite is RoomSpriteBoxN;
        }

        public override SheetType Type()
        {
            return SheetType.sBox;
        }
    }
}