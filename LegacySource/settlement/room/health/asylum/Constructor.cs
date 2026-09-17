using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.floor.Floors;
using snake2d;
using util.rendering;

namespace settlement.room.health.asylum
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_ASYLUM blue;

        public readonly FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this);
        public readonly FurnisherStat guards = new FurnisherStat.FurnisherStatI(this);
        private readonly RoomSprite1x1 sCandle;
        private readonly Floor floor2;
        private readonly RoomSprite sWalls;
        private readonly RoomSprite sBars;
        private readonly RoomSprite sOpening;

        public static readonly int CODE_ENTRANCE = 1;
        public static readonly int CODE_FOOD = 2;

        protected Constructor(ROOM_ASYLUM blue, RoomInitData init)
            : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.get(init.data().value("FLOOR2"), init.data());

            Json sjson = init.data().json("SPRITES");

            sWalls = new RoomSpriteCombo(sjson, "WALLS_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry) != sCandle;
                }
            };

            sBars = new RoomSprite1x1(sjson, "BARS_1X1");

            sOpening = new RoomSprite1x1(sjson, "OPENING_1X1");

            RoomSprite sDummy = new RoomSprite.Dummy()
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    prenderAbove(r, s, it, degrade, true);
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data)
                {
                    SP.cons().BIG.filled().draw(r, x, y);
                }
            };

            RoomSprite sbucket = new RoomSprite1x1(sjson, "BUCKET_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.get(rx, ry) != null;
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data)
                {
                    SP.cons().BIG.filled().draw(r, x, y);
                }
            };

            RoomSprite stablemisc = new RoomSpriteCombo(sjson, "TABLE_MISC_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.get(rx, ry) != null;
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data)
                {
                    SP.cons().BIG.filled().draw(r, x, y);
                }
            };

            RoomSprite stablefood = new RoomSpriteCombo(sjson, "TABLE_FOOD_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.get(rx, ry) != null;
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data)
                {
                    SP.cons().BIG.filled().draw(r, x, y);
                }
            };

            sCandle = new RoomSprite1x1(sjson, "CANDLE_HOLDER_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.get(rx, ry) != null;
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data)
                {
                    SP.cons().BIG.filled().draw(r, x, y);
                }
            };

            FurnisherItemTile b1 = new FurnisherItemTile(this, sbedA, AVAILABILITY.NOT_ACCESSIBLE, false);
            FurnisherItemTile b2 = new FurnisherItemTile(this, sbedB, AVAILABILITY.NOT_ACCESSIBLE, false);

            FurnisherItemTile c1 = new FurnisherItemTile(this, sCandle, AVAILABILITY.AVOID_PASS, true);
            FurnisherItemTile oo = new FurnisherItemTile(this, sDummy, AVAILABILITY.AVOID_PASS, false);
            FurnisherItemTile ni = new FurnisherItemTile(this, sbucket, AVAILABILITY.ROOM_SOLID, false);
            FurnisherItemTile ta = new FurnisherItemTile(this, stablemisc, AVAILABILITY.ROOM_SOLID, false);
            FurnisherItemTile fo = new FurnisherItemTile(this, stablefood, AVAILABILITY.ROOM_SOLID, false).setData(CODE_FOOD);

            FurnisherItemTile ss = new FurnisherItemTile(this, true, sOpening, AVAILABILITY.AVOID_PASS, false).setData(CODE_ENTRANCE);
            FurnisherItemTile __ = null;

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ni, oo, b1 },
                { ta, oo, b2 },
                { fo, ss, ni },
                { __, __, c1 },
            }, 1);

            flush(1, 3);
        }

        bool isWithinCell(int nx, int ny, int cx, int cy)
        {
            if (SETT.ROOMS().fData.item.get(nx, ny) != null && SETT.ROOMS().fData.sprite.get(nx, ny) != sCandle && SETT.ROOMS().fData.item.get(cx, cy) != null)
            {
                COORDINATE c = SETT.ROOMS().fData.itemX1Y1(nx, ny, Coo.TMP);
                nx = c.x();
                ny = c.y();
                return SETT.ROOMS().fData.itemX1Y1(cx, cy, Coo.TMP).isSameAs(nx, ny);
            }
            return false;
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
            return new AsylumInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            FurnisherItemTile t = SETT.ROOMS().fData.tile.get(tx, ty);
            if (t != null && t.sprite() != sCandle)
                floor2.placeFixed(tx, ty);
            else
                base.putFloor(tx, ty, upgrade, area);
        }

        private void prenderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade, bool wall)
        {
            FurnisherItem i = SETT.ROOMS().fData.item.get(it.tx(), it.ty());
            sBars.render(r, s, i.rotation, it, degrade, false);

            if (!wall)
            {
                sOpening.render(r, s, (i.rotation + 2) & 0b011, it, degrade, false);
                return;
            }

            COORDINATE coo = SETT.ROOMS().fData.itemMaster(it.tx(), it.ty(), Coo.TMP);
            int mX = coo.x();
            int mY = coo.y();

            RoomInstance ro = SETT.ROOMS().map.instance.get(it.tx(), it.ty());
            if (ro == null)
                return;
            int m = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (ro.is(it.tx(), it.ty(), d) && SETT.ROOMS().fData.sprite.get(it.tx(), it.ty(), d) != sCandle && SETT.ROOMS().fData.item.get(it.tx(), it.ty(), d) != null)
                {
                    coo = SETT.ROOMS().fData.itemMaster(it.tx() + d.x(), it.ty() + d.y(), Coo.TMP);
                    if (coo != null && coo.isSameAs(mX, mY))
                        m |= d.mask();
                }
            }

            sWalls.render(r, s, m, it, degrade, false);
        }

        public override bool isHeavy()
        {
            return true;
        }
    }
}