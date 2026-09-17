using System;
using System.IO;
using System.Collections.Generic;

namespace settlement.room.infra.export
{
    class Constructor : Furnisher
    {
        private readonly ROOM_EXPORT blue;
        public readonly FurnisherStat crates;
        private readonly Floor floor2;

        private const int ICRATE = 1;
        private const int ICANDLE = 2;

        public Constructor(ROOM_EXPORT blue, RoomInitData init) : base(init, 1, 1, 88, 44)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.read("FLOOR2", init.data());
            crates = new FurnisherStat(this, 1)
            {
                Get = (area, fromItems) => fromItems,
                Format = (t, value) => GFORMAT.i(t, (int)value)
            };

            Json sp = init.data().json("SPRITES");

            RoomSprite1x1 sCrate = new RoomSprite1x1(sp, "CRATE_1X1")
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    renderCrate(r, s, data, it, degrade, isCandle);
                    return false;
                }
            };

            RoomSprite sRoof = new RoomSpriteCombo(sp, "ROOF_COMBO")
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    s.setSoft();
                    DIR rot = DIR.ORTHO.Get(SETT.ROOMS().fData.item.Get(it.tile()).rotation).next(-1);
                    it.setOff(rot.x() * C.TILE_SIZEH, rot.y() * C.TILE_SIZEH);
                    base.Render(r, s, data, it, degrade, isCandle);
                    s.setHard();
                    return false;
                },
                Joins = (tx, ty, rx, ry, d, item) => item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1
            };

            RoomSprite sCrateR = new RoomSprite1x1(sp, "CRATE_1X1")
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    renderCrate(r, s, data, it, degrade, isCandle);
                    return false;
                },
                GetData2 = (tx, ty, rx, ry, item, itemRan) => sRoof.GetData(tx, ty, rx, ry, item, itemRan),
                RenderAbove = (r, s, data, it, degrade) => sRoof.Render(r, s, GetData2(it), it, degrade, rotates)
            }.sData(1);

            RoomSprite spriteCrate = new RoomSprite1x1(sCrate)
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    renderCrate(r, s, data, it, degrade, isCandle);
                    return false;
                }
            };

            FurnisherItemTile cc = new FurnisherItemTile(
                this,
                true,
                spriteCrate,
                AVAILABILITY.ROOM_SOLID,
                false).setData(ICRATE);

            FurnisherItemTile ca = new FurnisherItemTile(
                this,
                false,
                spriteCrate,
                AVAILABILITY.ROOM_SOLID,
                true).setData(ICANDLE);

            FurnisherItemTile cr = new FurnisherItemTile(
                this,
                true,
                spriteCrate,
                AVAILABILITY.ROOM_SOLID,
                false).setData(ICRATE);

            FurnisherItemTile rc = new FurnisherItemTile(
                this,
                true,
                sCrateR,
                AVAILABILITY.ROOM_SOLID,
                false).setData(ICRATE);

            FurnisherItemTile ra = new FurnisherItemTile(
                this,
                true,
                sCrateR,
                AVAILABILITY.ROOM_SOLID,
                true).setData(ICANDLE);

            FurnisherItemTile roof = new FurnisherItemTile(
                this,
                true,
                sRoof,
                AVAILABILITY.ROOM_SOLID,
                false).setData(ICRATE);

            FurnisherItem[] items = new FurnisherItem[]
            {
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 1),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 2),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 3),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 4),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 5),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 6),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 7),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 8),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 9),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 10),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 11),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 12),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 13),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 14),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 15),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 16),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 17),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 18),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 19),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 20),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 21),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 22),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 23),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 24),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 25),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 26),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 27),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 28),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 29),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 30),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 31),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 32),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 33),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 34),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 35),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 36),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 37),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 38),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 39),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 40),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 41),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 42),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 43),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 44),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 45),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 46),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 47),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 48),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 49),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 50),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 51),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 52),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 53),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 54),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 55),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 56),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 57),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 58),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 59),
                new FurnisherItem(new FurnisherItemTile[] { cc, ca, cr, rc, ra, roof }, 60),
            };

            flush(1);
        }

        public override bool UsesArea()
        {
            return false;
        }

        public override bool MustBeIndoors()
        {
            return false;
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override void PutFloor(int tx, int ty, int upgrade, AREA area)
        {
            for (int i = 0; i < DIR.ALL.Length; i++)
            {
                if (!area.is(tx, ty, DIR.ALL[i]))
                {
                    base.PutFloor(tx, ty, upgrade, area);
                    return;
                }
            }
            floor2.placeFixed(tx, ty);
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new ExportInstance(blue, area, init);
        }

        private void renderCrate(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
        {
            if (blue().is(it.tile()))
            {
                ExportInstance ins = blue().instances[0];
                if (ins.crate != null)
                {
                    Resource res = ins.crate;
                    if (res.visible)
                    {
                        res.visible = false;
                    }
                    ins.crate = null;
                }
                if (ins.crate == null)
                {
                    ins.crate = new Resource(blue().resource, null);
                    Resource crate = ins.crate;
                    crate.visible = true;
                    crate.transform.setMatrix(it.matrix);
                    crate.transform.setPosition(it.position);
                    crate.transform.setScale(it.scale);
                    crate.transform.setRotation(it.rotation);
                }
            }
        }
    }
}