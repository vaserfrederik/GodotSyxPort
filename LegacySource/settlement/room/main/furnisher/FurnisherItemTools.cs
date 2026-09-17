using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.sprite;
using settlement.tilemap.floor.Floor;
using snake2d;
using util.rendering.RenderData;
using util.rendering;

namespace settlement.room.main.furnisher
{
    public static class FurnisherItemTools
    {
        private FurnisherItemTools()
        {
            // TODO Auto-generated constructor stub
        }

        public static FurnisherItemTile MakeFloorTile(Furnisher f, Floor floor)
        {
            RoomSprite ca = new RoomSprite.Imp()
            {
                Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                {
                    return false;
                },
                RenderBelow = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    SETT.FLOOR().RenderOntop(it, floor, GetRes(it.tx(), it.ty()));
                    base.RenderBelow(r, s, data, it, degrade);
                },
                RenderPlaceholder = (SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item) =>
                {
                    int res = GetRes(tx, ty);
                    SPRITES.cons().BIG.dashedThick.Render(r, res, x, y);
                },
                GetRes = (int tx, int ty) =>
                {
                    int res = 0;

                    if (SETT.ROOMS().placement.factory.Is(tx, ty))
                    {
                        for (int di = 0; di < 4; di++)
                        {
                            DIR d = DIR.ORTHO.Get(di);
                            int dx = tx + d.x();
                            int dy = ty + d.y();
                            if (SETT.ROOMS().placement.factory.Is(dx, dy) && Is(dx, dy))
                                res |= d.Mask();
                        }
                    }
                    else
                    {
                        Room r = SETT.ROOMS().map.Get(tx, ty);
                        if (r == null)
                            return 0;
                        for (int di = 0; di < 4; di++)
                        {
                            DIR d = DIR.ORTHO.Get(di);
                            int dx = tx + d.x();
                            int dy = ty + d.y();
                            if (r.IsSame(tx, ty, dx, dy) && Is(dx, dy))
                                res |= d.Mask();
                        }
                    }
                    return res;
                },
                Is = (int dx, int dy) =>
                {
                    FurnisherItemTile t = f.tiles.Get(SETT.ROOMS().fData.tIndex.Get(dx, dy));
                    return (t != null && t.sprite() == this);
                },
                GetData = (int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) =>
                {
                    return 0;
                }
            };

            return new FurnisherItemTile(f, false, ca, AVAILABILITY.ROOM, false);
        }

        public static void MakeFloor(Furnisher f, Floor floor)
        {
            FurnisherItemTile cc = MakeFloorTile(f, floor);

            MakeArea(f, cc);
        }

        public static FurnisherItemTile MakeUnder(Furnisher f, Json j, string key) throws IOException
        {
            RoomSpriteCombo ca = new RoomSpriteCombo(j, key)
            {
                RenderBelow = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    base.Render(r, s, data, it, degrade, false);
                },
                Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                {
                    return false;
                }
            };
            FurnisherItemTile cc = new FurnisherItemTile(f, false, ca, AVAILABILITY.ROOM, false);

            MakeArea(f, cc);
            return cc;
        }

        public static FurnisherItemTile MakeUnder(Furnisher f, Json j, string key, RoomSprite above) throws IOException
        {
            RoomSpriteCombo ca = new RoomSpriteCombo(j, key)
            {
                RenderBelow = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    base.Render(r, s, data, it, degrade, false);
                },
                Render = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle) =>
                {
                    return above.Render(r, s, data, it, degrade, isCandle);
                },
                RenderAbove = (SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) =>
                {
                    above.RenderAbove(r, s, data, it, degrade);
                }
            };
            FurnisherItemTile cc = new FurnisherItemTile(f, false, ca, AVAILABILITY.ROOM, false);

            MakeArea(f, cc);
            return cc;
        }

        public static void MakeArea(Furnisher f, FurnisherItemTile cc)
        {
            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc}
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc}
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc}
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc}
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc}
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc,cc}
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc},
                {cc,cc},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc},
                {cc,cc,cc},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc},
                {cc,cc,cc,cc},
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc},
            }, 10);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc,cc,cc},
            }, 12);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc},
                {cc,cc,cc},
                {cc,cc,cc},
            }, 9);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc},
                {cc,cc,cc,cc},
                {cc,cc,cc,cc},
            }, 12);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc},
            }, 15);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc},
            }, 18);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {cc,cc,cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc,cc,cc},
                {cc,cc,cc,cc,cc,cc,cc},
            }, 21);

            f.Flush(1);
        }
    }
}