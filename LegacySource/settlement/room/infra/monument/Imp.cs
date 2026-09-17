using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using util.rendering;
using view.tool;

namespace settlement.room.infra.monument
{
    public class Imp : ROOM_MONUMENT
    {
        private readonly Furnisher constructor;

        public Imp(RoomInitData init, int tindex, string key, RoomCategorySub cat) : base(init, tindex, key, cat)
        {
            this.constructor = new Constructor(this, init);
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        private class Constructor : MConstructor
        {
            public Constructor(Imp blue, RoomInitData init) : base(blue, init)
            {
                foreach (var sData in init.data().jsons("SPRITES"))
                {
                    RoomSprite floor = new RoomSpriteCombo(sData, "FLOOR_COMBO")
                    {
                        protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                        {
                            return item.get(rx, ry) != null;
                        }
                    };

                    if (sData.has("1x1"))
                    {
                        RoomSprite ssmall = new RoomSprite1x1(sData, "1x1")
                        {
                            public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                            {
                                return floor.getData(tx, ty, rx, ry, item, itemRan);
                            }

                            public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                            {
                                floor.render(r, s, getData2(it), it, degrade, false);
                                return false;
                            }

                            public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                            {
                                animate(1.0 - SETT.ROOMS().map.get(it.tile()).getDegrade(it.tx(), it.ty()));
                                base.render(r, s, data, it, degrade, false);
                            }
                        };
                        FurnisherItemTile ss = new FurnisherItemTile(
                            this,
                            false,
                            ssmall,
                            blue.avail,
                            false
                        );
                        new FurnisherItem(new FurnisherItemTile[][] {
                            {ss},
                        }, 1, 1);
                    }

                    if (sData.has("2x2"))
                    {
                        RoomSprite sp = new RoomSpriteXxX(sData, "2x2", 2)
                        {
                            public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                            {
                                return floor.getData(tx, ty, rx, ry, item, itemRan);
                            }

                            public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                            {
                                floor.render(r, s, getData2(it), it, degrade, false);
                                return false;
                            }

                            public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                            {
                                animate(1.0 - SETT.ROOMS().map.get(it.tile()).getDegrade(it.tx(), it.ty()));
                                base.render(r, s, data, it, degrade, false);
                            }
                        };
                        FurnisherItemTile tt = new FurnisherItemTile(
                            this,
                            false,
                            sp,
                            blue.avail,
                            false
                        );
                        new FurnisherItem(new FurnisherItemTile[][] {
                            {tt, tt},
                            {tt, tt},
                        }, 6, 2);
                    }

                    if (sData.has("3x3"))
                    {
                        RoomSprite sp = new RoomSpriteXxX(sData, "3x3", 3)
                        {
                            public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                            {
                                return floor.getData(tx, ty, rx, ry, item, itemRan);
                            }

                            public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                            {
                                floor.render(r, s, getData2(it), it, degrade, false);
                                return false;
                            }

                            public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                            {
                                animate(1.0 - SETT.ROOMS().map.get(it.tile()).getDegrade(it.tx(), it.ty()));
                                base.render(r, s, data, it, degrade, false);
                            }
                        };
                        FurnisherItemTile tt = new FurnisherItemTile(
                            this,
                            false,
                            sp,
                            blue.avail,
                            false
                        );
                        new FurnisherItem(new FurnisherItemTile[][] {
                            {tt, tt, tt},
                            {tt, tt, tt},
                            {tt, tt, tt},
                        }, 12, 4);
                    }

                    flush(3);
                }
            }

            public override bool removeTerrain(int tx, int ty)
            {
                if (SETT.TERRAIN().get(tx, ty) is TFortification.Normal && SETT.PATH().availability.get(tx, ty).player >= 0)
                    return false;
                return base.removeTerrain(tx, ty);
            }

            public override CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile)
            {
                if (SETT.TERRAIN().get(tx, ty) is TFortification.Normal && SETT.PATH().availability.get(tx, ty).player < 0)
                    return PlacableMessages.¤¤STRUCTURE_BLOCK;
                return base.placable(tx, ty, item, tile);
            }

            public override void putFloor(int tx, int ty, int upgrade, AREA area)
            {
                // TODO Auto-generated method stub
                base.putFloor(tx, ty, upgrade, area);
            }
        }
    }
}