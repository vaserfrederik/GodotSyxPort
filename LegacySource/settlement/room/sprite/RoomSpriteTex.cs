using System;
using System.IO;
using Newtonsoft.Json;
using Snake2D;
using Util.Rendering;

namespace Settlement.Room.Sprite
{
    public class RoomSpriteTex : RoomSpriteImp
    {
        public RoomSpriteTex(JsonObject sp, string key) : base(SheetType.STex, sp, key)
        {
        }

        public RoomSpriteTex(RoomSprite other) : base(other)
        {
        }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int ran = it.Ran();
            SheetPair sheet = sheetPair(it, ran);
            if (sheet == null)
                return false;
            sheet.D.Color(ran).Bind();
            ran = ran >> 4;

            int tile = Type().Tile(sheet.S, sheet.D, 0, frame(sheet, it), 0);

            sheet.S.Render(sheet.D, it.X(), it.Y(), it, r, tile, ran, degrade);
            COLOR.Unbind();
            if (s != null)
                sheet.S.RenderShadow(sheet.D, it.X(), it.Y(), it, s, tile, ran);
            return false;
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            SheetType.STex.RenderOverlay(
                x, y, r, item.Get(rx, ry).Availability,
                0, Rotates ? data : -1, item.Width == 1 && item.Height == 1);
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            return 0;
        }

        protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
        {
            return false;
        }

        public override SheetType Type()
        {
            return SheetType.STex;
        }
    }
}