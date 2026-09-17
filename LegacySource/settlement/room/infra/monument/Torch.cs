using System;
using System.IO;
using JsonFx.Json;
using init.constant;
using init.resources;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.infra.monument
{
    sealed class Torch : ROOM_MONUMENT
    {
        private readonly Constructor2 constructor;

        public Torch(RoomInitData init, int index, string key, RoomCategorySub cat) : base(init, index, key, cat)
        {
            this.constructor = new Constructor2(init);
        }

        public override Constructor2 Constructor()
        {
            return constructor;
        }

        public sealed class Constructor2 : MConstructor
        {
            public readonly RoomSprite small;
            public readonly RoomSprite medium;
            private readonly FurnisherItemTile ss;
            private readonly FurnisherItemTile sm;

            protected Constructor2(RoomInitData init) : base(((Torch)base.room).this, init)
            {
                Json js = init.data().json("SPRITES");

                small = new RoomSprite1x1(js, "SMALL_1X1")
                {
                    Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                    {
                        base.Render(r, s, data, it, degrade, isCandle);
                        int am = (int)((1.0 - SETT.ROOMS().map.get(it.tx(), it.ty()).getDegrade(it.tx(), it.ty())) * 4);
                        RESOURCES.WOOD().renderLaying(r, it.x(), it.y(), it.ran(), am);
                        return false;
                    }
                };

                medium = new RoomSpriteCombo(js, "COMBO")
                {
                    Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                    {
                        base.Render(r, s, data, it, degrade, isCandle);
                        if ((data & DIR.N.mask()) != 0 && (data & DIR.W.mask()) != 0)
                        {
                            int am = (int)((1.0 - SETT.ROOMS().map.get(it.tx(), it.ty()).getDegrade(it.tx(), it.ty())) * 8);
                            RESOURCES.WOOD().renderLaying(r, it.x() - C.TILE_SIZEH, it.y() - C.TILE_SIZEH, it.ran(), am);
                        }
                        return false;
                    }
                };

                ss = new FurnisherItemTile(
                    this,
                    small,
                    AVAILABILITY.SOLID,
                    false
                );

                sm = new FurnisherItemTile(
                    this,
                    medium,
                    AVAILABILITY.SOLID,
                    false
                );

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { ss },
                }, 1, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { sm, sm },
                    new FurnisherItemTile[] { sm, sm },
                }, 4, 4);

                flush(0);
            }

            public override Room create(TmpArea area, RoomInit init)
            {
                if (area.body().width() == 2)
                {
                    SETT.LIGHTS().torchBig(area.body().x1(), area.body().y1(), C.TILE_SIZEH);
                }
                else
                {
                    SETT.LIGHTS().torch(area.body().x1(), area.body().y1(), 0);
                }
                return base.create(area, init);
            }
        }
    }
}