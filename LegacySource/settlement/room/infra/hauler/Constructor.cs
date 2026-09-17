using System;
using System.IO;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sprite;
using util.rendering;

namespace settlement.room.infra.hauler
{
    final class Constructor : Furnisher
    {
        protected Constructor(RoomInitData init) : base(init, 1, 0, 88, 44)
        {
            Json sp = init.data().json("SPRITES");

            RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    HaulerInstance ins = blue().get(it.tx(), it.ty());
                    if (ins != null && blue().crate.get(it.tx(), it.ty(), ins, ins.sdata).resource() != null)
                    {
                        blue().crate.resource().renderLaying(r, it.x(), it.y(), it.ran(), blue().crate.amount());
                    }
                    return false;
                }
            };
            RoomSprite spriteCrateRes = new RoomSprite1x1(spriteCrate)
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);

                    HaulerInstance ins = ROOMS().HAULER.getter.get(it.tile());
                    if (ins == null)
                        return false;

                    SPRITE i = ins.resource() == null ? SPRITES.icons().m.cancel : ins.resource().icon();
                    OPACITY.O99.bind();
                    i.render(r, it.x(), it.x() + C.TILE_SIZE, it.y(), it.y() + C.TILE_SIZE);
                    OPACITY.unbind();

                    return false;
                }
            };

            RoomSpriteCombo spriteFence = new RoomSpriteCombo(sp, "FENCE_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return false;
                }
            };

            FurnisherItemTile tt = new FurnisherItemTile(this, false, spriteCrateRes, AVAILABILITY.AVOID_PASS, false);
            FurnisherItemTile ff = new FurnisherItemTile(this, false, spriteFence, AVAILABILITY.AVOID_PASS, false);
            FurnisherItemTile __ = new FurnisherItemTile(this, false, spriteCrate, AVAILABILITY.ROOM, false).setData(1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt,},
                {__,},
            }, 1);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                {tt,ff},
                {__,__},
            }, 2);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ff,__,ff},
                {__,tt,__},
                {ff,__,ff},
            }, 4);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ff,__,__,__,ff},
                {__,__,tt,__,__},
                {ff,__,__,__,ff},
            }, 11);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ff,__,__,__,ff},
                {__,__,__,__,__},
                {__,__,tt,__,__},
                {__,__,__,__,__},
                {ff,__,__,__,ff},
            }, 20);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ff,__,__,__,__,__,ff},
                {__,__,__,__,__,__,__},
                {__,__,__,__,__,__,__},
                {__,__,__,tt,__,__,__},
                {__,__,__,__,__,__,__},
                {__,__,__,__,__,__,__},
                {ff,__,__,__,__,__,ff},
            }, 44);

            flush(3);
        }

        public override bool usesArea()
        {
            return false;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override bool mustBeOutdoors()
        {
            return false;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new HaulerInstance(blue(), area, init);
        }

        public override ROOM_HAULER blue()
        {
            return SETT.ROOMS().HAULER;
        }
    }
}