using System;
using settlement.room.main.furnisher;
using snake2d;
using util.rendering;
using util.sprite;

namespace settlement.room.sprite
{
    public class RoomSpriteSimple : RoomSprite.Imp
    {
        private readonly int tileStart;
        public readonly int tileEnd;
        private readonly TILE_SHEET sheet;
        private readonly int variations;

        public RoomSpriteSimple(TILE_SHEET sheet, int startTile, int variations) : this(sheet, startTile, variations, 0, 3)
        {
        }

        private RoomSpriteSimple(TILE_SHEET sheet, int startTile, int variations, int shadHeight, int shadLength)
        {
            this.sheet = sheet;
            this.tileStart = startTile;
            this.variations = variations;
            this.tileEnd = startTile + variations;
            this.shadowHeight = shadHeight;
            this.shadowDist = shadLength;
        }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int x = it.x();
            int y = it.y();
            data += GetTileOffset(it, data);
            int tile = data + tileStart + (it.ran() % variations);
            sheet.Render(r, tile, x, y);
            RenderDegrade(sheet, r, tile, it, degrade);

            if (shadowHeight > 0 || shadowDist > 0)
            {
                s.SetDistance2Ground(shadowHeight).SetHeight(shadowDist);
                sheet.Render(s, tile, x, y);
            }

            return false;
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            if (item.Get(rx, ry) != null && item.Get(rx, ry).IsBlocker())
                SPRITES.cons().BIG.filled.Render(r, 0, x, y);
            else
                SPRITES.cons().BIG.dashedThick.Render(r, 0, x, y);
        }

        protected virtual int GetTileOffset(RenderData.RenderIterator it, int data)
        {
            return 0;
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            return 0;
        }
    }
}