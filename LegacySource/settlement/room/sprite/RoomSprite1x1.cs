using System;
using System.IO;
using Newtonsoft.Json;
using Util.Gutil;
using Util.Rendering;
using Util.Colors;
using Util.Datatypes;

namespace Settlement.Room.Sprite
{
    public class RoomSprite1x1 : RoomSpriteImp
    {
        public RoomSprite1x1(Json json, string key) : base(SheetType.S1x1, json, key) { }

        public RoomSprite1x1(RoomSprite other) : base(other) { }

        public override bool Render(SpriteRenderer r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            int ran = it.Ran();
            SheetPair sheet = sheetPair(it, ran);
            if (sheet == null)
                return false;
            sheet.d.color(ran).Bind();
            ran = ran >> 4;

            int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), this.rotates ? (data & 0x03) : ran & 0b11);

            sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
            Color.Unbind();
            if (s != null)
                sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
            return false;
        }

        public void RenderRandom(SpriteRenderer r, ShadowBatch s, RenderIterator it, int ran, double degrade)
        {
            SheetPair sheet = sheetPair(it, ran);
            if (sheet == null)
                return;
            sheet.d.color(ran).Bind();

            int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), -1);

            sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
            Color.Unbind();
            sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
        }

        public void RenderRandom(SpriteRenderer r, ShadowBatch s, RenderIterator it, int ran, double degrade, Color col)
        {
            SheetPair sheet = sheetPair(it, ran);
            if (sheet == null)
                return;
            col.Bind();

            int tile = type().tile(sheet.s, sheet.d, 0, frame(sheet, it), -1);

            sheet.s.render(sheet.d, it.x(), it.y(), it, r, tile, ran, degrade);
            Color.Unbind();
            sheet.s.renderShadow(sheet.d, it.x(), it.y(), it, s, tile, ran);
        }

        public override void RenderPlaceholder(SpriteRenderer r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            SheetType.S1x1.renderOverlay(
                x, y, r, item.get(rx, ry).availability,
                0, rotates ? data : -1, item.width() == 1 && item.height() == 1);
        }

        public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            int ri = Gutil.ran2().get(tx, ty) & 0b011;
            for (int i = 0; i < DIR.ORTHO.size(); i++)
            {
                int rr = (ri + i) % DIR.ORTHO.size();
                DIR d = DIR.ORTHO.get(rr);
                if (joins(tx + d.x(), ty + d.y(), rx + d.x(), ry + d.y(), d, item))
                    return (byte)rr;
            }
            return (byte)item.rotation;
        }

        public int GetRot(int data)
        {
            return data & 0x03;
        }

        public DIR Rot(int data)
        {
            return DIR.ORTHO.get(data & 0x03);
        }

        protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
        {
            return DIR.ORTHO.get(item.rotation) == d;
        }

        public override SheetType.C1X1 Type()
        {
            return SheetType.S1x1;
        }

        public override int Rotation(int data, FurnisherItem item)
        {
            if (rotates)
                return GetRot(data) + 1;
            return 0;
        }
    }
}