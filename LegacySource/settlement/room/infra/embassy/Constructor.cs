using System;
using System.IO;
using System.Linq;

namespace settlement.room.infra.embassy
{
    class Constructor : Furnisher
    {
        public readonly FurnisherStat workers = new FurnisherStat.FurnisherStatEmployees(this);
        public readonly FurnisherStat efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);
        private readonly ROOM_EMBASSY blue;
        static readonly int IWORK = 3;

        protected Constructor(ROOM_EMBASSY blue, RoomInitData init) : base(init, 3, 2)
        {
            this.blue = blue;

            var sp = init.data().json("SPRITES");

            var sDecor = new RoomSprite1x1(sp, "DECOR_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation)
                    {
                        if (item.get(rx, ry) != null && item.get(rx, ry).sprite is RoomSpriteCombo)
                            return true;
                    }

                    return false;
                }
            };

            var sTable = new RoomSpriteCombo(sp, "TABLE_COMBO");

            var sTableWork = new RoomSpriteCombo(sTable)
            {
                private readonly RoomSprite top = new RoomSprite1x1(sp, "TABLE_TOP_1X1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        if (d.orthoID() == item.rotation || d.perpendicular().orthoID() == item.rotation)
                        {
                            if (d.orthoID() == item.rotation && item.get(rx, ry) == null)
                                return true;
                            if (item.get(rx, ry) != null && item.get(rx, ry).sprite is RoomSpriteCombo)
                                return true;
                        }

                        return false;
                    }
                };

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (SETT.ROOMS().fData.candle.is(it.tile()))
                        return;
                    top.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            var sTableDec = new RoomSpriteCombo(sTable)
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (SETT.ROOMS().fData.candle.is(it.tile()))
                        return;
                    sDecor.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return sDecor.getData(tx, ty, rx, ry, item, itemRan);
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
                {
                    base.renderPlaceholder(r, x, y, data, tx, ty, rx, ry, item);
                    if (item.width() == 1 || item.height() == 1)
                        SPRITES.cons().ICO.arrows.get(item.rotation);
                }
            };

            var sShelf = new RoomSprite1x1(sp, "SHELF_1X1")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (SETT.ROOMS().fData.candle.is(it.tile()))
                        return;
                    sDecor.render(r, s, data, it, degrade, false);
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (item.width() > 1 && item.height() > 1)
                    {
                        if ((DIR.ORTHO.get(item.rotation).x() * d.x() != 0 || DIR.ORTHO.get(item.rotation).y() * d.y() != 0) && item.sprite(rx, ry) == this)
                            return true;
                        return false;
                    }
                    return DIR.ORTHO.get(item.rotation) == d;
                }
            };

            var sRes = new RoomSprite1x1(sp, "TABLE_1X1")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    var ins = blue.getter.get(it.tx(), it.ty());
                    if (ins != null && blue.consumption().ins().size() == 0)
                    {
                        var iran = it.bigRan();
                        var ri = (int)((iran & 15) % blue.consumption().ins().size());
                        iran = iran >> 4;
                        var dam = (iran & 0xFF) / (double)0x0FF;

                        var am = 8 * blue.consumption().stored(blue.consumption().ins().get(ri)).get(ins) / Constructor.this.blue.maxRes(ri, ins);
                        am *= 0.5 + dam;
                        if (am > 0)
                        {
                            blue.consumption().ins().get(ri).resource.renderLaying(r, it.x(), it.y(), it.ran(), am);
                        }
                    }
                }
            };

            var sStool = new RoomSprite1x1(sp, "STOOL_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return false;
                }
            };

            var rr = new FurnisherItemTile(sRes);
            var sh = new FurnisherItemTile(sShelf);
            var stool = new FurnisherItemTile(sStool);
            var tableWork = new FurnisherItemTile(sTableWork);
            var tableDec = new FurnisherItemTile(sTableDec);

            new FurnisherItem(new[] { rr, sh }, 1);
            new FurnisherItem(new[] { rr, sh, sh }, 2);
            new FurnisherItem(new[] { rr, sh, sh, sh }, 3);
            new FurnisherItem(new[] { rr, sh, sh, sh, sh, sh }, 4);
            new FurnisherItem(new[] { rr, sh }, 2);
            new FurnisherItem(new[] { rr, sh, sh }, 4);
            new FurnisherItem(new[] { rr, sh, sh, sh }, 6);
            new FurnisherItem(new[] { rr, sh, sh, sh, sh }, 8);
            new FurnisherItem(new[] { rr, sh, sh, sh, sh, sh }, 10);

            flush(3);

            new FurnisherItem(new[] { rr, stool }, 1);
            new FurnisherItem(new[] { rr, stool, stool }, 2);
            new FurnisherItem(new[] { rr, stool, stool, stool }, 3);
            new FurnisherItem(new[] { rr, stool, stool, stool, stool, stool }, 4);
            new FurnisherItem(new[] { rr, stool }, 2);
            new FurnisherItem(new[] { rr, stool, stool }, 4);
            new FurnisherItem(new[] { rr, stool, stool, stool }, 6);
            new FurnisherItem(new[] { rr, stool, stool, stool, stool }, 8);
            new FurnisherItem(new[] { rr, stool, stool, stool, stool, stool }, 10);

            flush(3);

            FurnisherItemTools.makeUnder(this, sp, "CARPET_COMBO");
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return true;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new EmbassyInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }
    }
}