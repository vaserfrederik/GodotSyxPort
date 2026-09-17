using System;
using System.IO;
using init.resources;
using init.sprite;
using init.sprite.game;
using settlement.main;
using settlement.path;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.terrain;
using snake2d;
using util.rendering;
using view.tool;

namespace settlement.room.military.artillery
{
    abstract class Constructor : Furnisher
    {
        private readonly ROOM_ARTILLERY blue;
        private const int SERVICE = 1;

        protected Constructor(RoomInitData init, ROOM_ARTILLERY blue) : base(init, 1, 0)
        {
            this.blue = blue;
            Json js = init.data().json("SPRITES");

            RoomSpriteXxX sArm = new RoomSpriteXxX(js, "ARM_2X2", 2)
            {
                RoomSprite srot = new RoomSpriteXxX(js, "ARM_ROT_2X2", 2);

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    int rot = 0;
                    ArtilleryInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        rot = ins.dirCurrent().id();
                    }

                    data = setRot(data, rot / 2);

                    if ((rot & 1) == 1)
                    {
                        return srot.render(r, s, data, it, degrade, isCandle);
                    }
                    else
                    {
                        return base.render(r, s, data, it, degrade, isCandle);
                    }
                }

                public override int frame(SheetPair a, RenderIterator it)
                {
                    ArtilleryInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        if (ins.isLoaded)
                            return 0;
                    }
                    return 1;
                }
            };

            RoomSpriteXxX sBase = new RoomSpriteXxX(js, "BASE_2X2", 2)
            {
                RoomSprite srot = new RoomSpriteXxX(js, "BASE_ROT_2X2", 2);

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    int rot = 0;
                    ArtilleryInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        rot = ins.dirCurrent().id();
                    }

                    data = setRot(data, rot / 2);

                    if ((rot & 1) == 1)
                    {
                        srot.render(r, s, data, it, degrade, isCandle);
                    }
                    else
                    {
                        base.render(r, s, data, it, degrade, isCandle);
                    }
                    sArm.render(r, s, data, it, degrade, isCandle);
                    return false;
                }

                public override int frame(SheetPair a, RenderIterator it)
                {
                    ArtilleryInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        return ((int)(ins.progress() * 32) & 1);
                    }
                    return 0;
                }
            };

            RoomSprite sDep = new RoomSprite1x1(js, "STORAGE_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    RESOURCES.STONE().renderLaying(r, it.x(), it.y(), it.ran(), 64);
                    return false;
                }
            };

            RoomSprite sca = new RoomSprite1x1(js, "TORCH_1X1")
            {
                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
                {
                    SPRITES.cons().ICO.arrows.get(rot(data).orthoID()).render(r, x, y);
                }
            };

            RoomSprite sDummy = new RoomSprite.Dummy();

            FurnisherItemTile xx = new FurnisherItemTile(this, false, sBase, AVAILABILITY.SOLID, false);
            FurnisherItemTile ca = new FurnisherItemTile(this, false, sca, AVAILABILITY.SOLID, true);
            FurnisherItemTile dp = new FurnisherItemTile(this, false, sDep, AVAILABILITY.ROOM, false);
            FurnisherItemTile __ = new FurnisherItemTile(this, false, sDummy, AVAILABILITY.ROOM, false);
            FurnisherItemTile ee = new FurnisherItemTile(this, true, sDummy, AVAILABILITY.ROOM, false);
            __.setData(SERVICE);
            ee.setData(SERVICE);

            new FurnisherItem(
                new FurnisherItemTile[][]
                {
                    { ca, xx, xx, dp },
                    { __, xx, xx, __ },
                    { __, ee, ee, __ },
                },
                1.0, 1.0);

            flush(1, 3);
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
            return true;
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
            return null;
        }

        public override ROOM_ARTILLERY blue()
        {
            return blue;
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
        }
    }
}