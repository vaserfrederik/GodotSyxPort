using System;
using System.IO;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using settlement.tilemap.floor;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.infra.importt
{
    public sealed class Constructor : Furnisher
    {
        private readonly ROOM_IMPORT blue;
        public readonly FurnisherStat crates;
        private readonly FurnisherItemTile cr;
        private readonly Floor floor2;

        public Constructor(ROOM_IMPORT blue, RoomInitData init)
            : base(init, 1, 1, 152, 100)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.read("FLOOR2", init.data());
            crates = new FurnisherStat(this, 1)
            {
                Get = (area, fromItems) => fromItems,
                Format = (t, value) => GFORMAT.i(t, (int)value)
            };

            Json sp = init.data().json("SPRITES");

            RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_1X1")
            {
                Joins = (tx, ty, rx, ry, d, item) => item.rotation == d.orthoID(),
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    ImportInstance ins = ROOMS().IMPORT.getter.get(it.tile());
                    if (ins != null)
                    {
                        if (ins.resource() != null)
                        {
                            int a = blue.crate.amount(it.tx(), it.ty(), ins, ins.sdata);
                            ins.resource().renderLaying(r, it.x(), it.y(), it.ran(), a);
                        }
                    }
                    return true;
                }
            };

            RoomSprite.Imp marker = new RoomSprite.Imp()
            {
                Render = (r, s, data, it, degrade, isCandle) => false,
                RenderBelow = (r, s, data, it, degrade) =>
                {
                    ImportInstance ins = ROOMS().IMPORT.getter.get(it.tile());
                    if (ins == null) return;
                    SPRITE i = ins.resource() == null ? SPRITES.icons().m.cancel : ins.resource().icon();
                    OPACITY.O99.bind();
                    i.render(r, it.x(), it.x() + C.TILE_SIZE, it.y(), it.y() + C.TILE_SIZE);
                    OPACITY.unbind();

                    base.RenderBelow(r, s, data, it, degrade);
                },
                GetData = (tx, ty, rx, ry, item, itemRan) => 0
            };

            cr = new FurnisherItemTile(
                this,
                true,
                spriteCrate,
                AVAILABILITY.ROOM_SOLID,
                false
            );

            FurnisherItemTile ca = new FurnisherItemTile(
                this,
                false,
                spriteCrate,
                AVAILABILITY.ROOM_SOLID,
                true
            );

            FurnisherItemTile ee = new FurnisherItemTile(
                this,
                new SpriteThingy(sp, "POST_1X1"),
                AVAILABILITY.ROOM,
                false
            );

            FurnisherItemTile cc = new FurnisherItemTile(
                this,
                new SpriteThingy(sp, "FENCE_1X1"),
                AVAILABILITY.ROOM,
                false
            );

            FurnisherItemTile __ = new FurnisherItemTile(
                this,
                null,
                AVAILABILITY.ROOM,
                false
            );

            FurnisherItemTile mm = new FurnisherItemTile(
                this,
                marker,
                AVAILABILITY.ROOM,
                false
            );

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, ee },
                { mm, cr, cr, ca, mm },
                { __, __, __, __, __ },
                { mm, cr, cr, ca, mm },
                { ee, cc, cc, cc, ee },
            }, 5, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, cc, ee },
                { mm, cr, cr, cr, ca, mm },
                { __, __, __, __, __, __ },
                { mm, cr, cr, cr, ca, mm },
                { ee, cc, cc, cc, cc, ee },
            }, 6, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, cc, cc, ee },
                { mm, cr, cr, cr, cr, ca, mm },
                { __, __, __, __, __, __, __ },
                { mm, cr, cr, cr, cr, ca, mm },
                { ee, cc, cc, cc, cc, cc, ee },
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, cc, cc, cc, ee },
                { mm, cr, cr, cr, cr, cr, ca, mm },
                { __, __, __, __, __, __, __, __ },
                { mm, cr, cr, cr, cr, cr, ca, mm },
                { ee, cc, cc, cc, cc, cc, cc, ee },
            }, 10);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, cc, cc, cc, cc, ee },
                { mm, cr, cr, cr, cr, cr, cr, ca, mm },
                { __, __, __, __, __, __, __, __, __ },
                { mm, cr, cr, cr, cr, cr, cr, ca, mm },
                { ee, cc, cc, cc, cc, cc, cc, cc, ee },
            }, 12);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, cc, cc, cc, cc, ee },
                { mm, ca, cr, cr, cr, cr, cr, ca, mm },
                { __, __, __, __, __, __, __, __, __ },
                { __, cr, cr, cr, cr, cr, cr, cr, __ },
                { __, cr, cr, cr, cr, cr, cr, cr, __ },
                { __, __, __, __, __, __, __, __, __ },
                { mm, ca, cr, cr, cr, cr, cr, ca, mm },
                { ee, cc, cc, cc, cc, cc, cc, cc, ee },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ee, cc, cc, cc, cc, cc, cc, cc, ee },
                { mm, ca, cr, cr, cr, cr, cr, ca, mm },
                { __, __, __, __, __, __, __, __, __ },
                { __, cr, cr, cr, cr, cr, cr, cr, __ },
                { __, cr, cr, cr, cr, cr, cr, cr, __ },
                { __, __, __, __, __, __, __, __, __ },
                { __, cr, cr, cr, cr, cr, cr, cr, __ },
                { __, cr, cr, cr, cr, cr, cr, cr, __ },
                { __, __, __, __, __, __, __, __, __ },
                { mm, ca, cr, cr, cr, cr, cr, ca, mm },
                { ee, cc, cc, cc, cc, cc, cc, cc, ee },
            }, 1);

            flush(1);
        }

        public override bool usesArea()
        {
            return false;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            foreach (DIR d in DIR.ALL)
            {
                if (!area.is(tx, ty, d))
                {
                    base.putFloor(tx, ty, upgrade, area);
                    return;
                }
            }
            floor2.placeFixed(tx, ty);
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new ImportInstance(blue, area, init);
        }

        private class SpriteThingy : RoomSprite1x1
        {
            public SpriteThingy(Json js, string key) : base(js, key) { }

            public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
            {
                return false;
            }

            public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
            {
                base.render(r, s, data, it, degrade, false);
            }

            protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
            {
                return item.sprite(rx, ry) is SpriteThingy;
            }
        }
    }
}