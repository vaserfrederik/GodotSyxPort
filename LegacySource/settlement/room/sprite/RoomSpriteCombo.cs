using System;
using System.IO;
using Newtonsoft.Json;
using Util.Rendering;
using Util.Rendering.Shadows;
using Util.Datatypes;
using Util.Colors;
using Util.Sprites;
using Util.Sheets;
using Settlement.Room.Main.Furnisher;
using Util.Rnd;

namespace Settlement.Room.Sprite
{
    public class RoomSpriteCombo : RoomSpriteImp
    {
        public RoomSpriteCombo(Json json, string key) : base(SheetType.sCombo, json, key) { }

        public RoomSpriteCombo(RoomSprite clone) : base(clone) { }

        public RoomSpriteCombo() : base(SheetType.sCombo) { }

        public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int k = (data >> 4) & 0x0F;
            SheetPair sheet = SheetPair(it, k);
            if (sheet == null)
                return false;

            sheet.d.color(k).bind();
            int ran = it.ran();

            int tile = Type().Tile(sheet.s, sheet.d, data & 0x0F, Frame(sheet, it), 0);

            sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
            COLOR.unbind();
            if (s != null)
                sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);

            return false;
        }

        public TextureCoords Texture(int data, RenderIterator it)
        {
            int k = (data >> 4) & 0x0F;
            SheetPair sheet = SheetPair(it, k);
            if (sheet == null)
                return COLOR.WHITE100.Texture();

            int tile = Type().Tile(sheet.s, sheet.d, data & 0x0F, Frame(sheet, it), 0);

            if (sheet.s is Sheet.Imp)
            {
                Sheet.Imp ii = (Sheet.Imp)sheet.s;
                return ii.sheet.GetTexture(tile);
            }

            return COLOR.WHITE100.Texture();
        }

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            if (item.Get(rx, ry) != null)
                Type().RenderOverlay(x, y, r, item.Get(rx, ry).availability, data & 0x0F, 0, false);
        }

        public int RotMask(int data)
        {
            return (data & 0x0F);
        }

        public SheetPair Sheet(int data, RenderIterator it)
        {
            Sheets a = sheet(it);
            if (a == null)
                return null;

            int k = (data >> 4) & 0x0F;
            SheetPair sheet = a.Get(k);
            return sheet;
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            int m = 0;
            int ri = RND.rInt(DIR.ORTHO.Size);
            for (int i = 0; i < DIR.ORTHO.Size; i++)
            {
                int rr = (ri + i) % DIR.ORTHO.Size;
                DIR d = DIR.ORTHO.Get(rr);
                if (Joins(tx + d.x(), ty + d.y(), rx + d.x(), ry + d.y(), d, item))
                    m |= d.mask();
            }
            return (byte)(m | (itemRan << 4));
        }

        protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
        {
            FurnisherItemTile t = item.Get(rx, ry);
            return t != null && t.sprite != null && t.sprite is RoomSpriteCombo;
        }

        public override SheetType Type()
        {
            return SheetType.sCombo;
        }
    }
}