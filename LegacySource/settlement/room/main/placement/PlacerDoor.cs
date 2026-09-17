using System;
using System.Collections.Generic;
using System.Linq;
using settlement.room.main.placement;
using game;
using init.constant;
using init.sprite;
using settlement.job;
using settlement.main;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;
using util.text;
using view.tool;

namespace settlement.room.main.placement
{
    final class PlacerDoor
    {
        protected readonly Room.RoomInstanceImp a;
        private readonly UtilHistory history;

        final UICons cWall = SPRITES.cons().BIG.filled;
        final UICons cDoor = SPRITES.cons().BIG.outline;

        private static readonly string ¤¤name = "¤Place Doorway";
        private static readonly string ¤¤shrink = "¤Remove Doorway";
        private static readonly string ¤¤cp = "Room will be blocked! Place doorways so that the room can be entered from the outside";

        static
        {
            D.ts(PlacerDoor.class);
        }

        final PlacableMulti undo = new PlacableMulti(¤¤shrink)
        {
            private SPRITE icon = new SPRITE.Twin(SPRITES.icons().m.wall_opening, SPRITES.icons().m.anti);

            public override void place(int tx, int ty, AREA a, PLACER_TYPE t)
            {
                if (removeWithoutHistory(tx, ty))
                    history.placeDoor(tx, ty, -1);
            }

            public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE t)
            {
                if (!isEdge.is(tx, ty))
                    return E;

                if (!UtilWallPlacability.wallCanBe.is(tx, ty))
                    return E;

                if (!isOpening.is(tx, ty))
                {
                    return E;
                }
                return null;
            }

            public override SPRITE getIcon()
            {
                return icon;
            }
        };

        final PlacableMulti placer = new PlacableMulti(¤¤name)
        {
            public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                if (placeWithoutHistory(tx, ty))
                {
                    history.placeDoor(tx, ty, 1);
                }
            }

            public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                if (!isEdge.is(tx, ty))
                    return E;
                if (!UtilWallPlacability.wallCanBe.is(tx, ty))
                    return E;
                if (isOpening.is(tx, ty))
                    return E;
                return null;
            }

            public override void renderTmpPlaceArea(SPRITE_RENDERER r, int x, int y, int tx, int ty, AREA area)
            {
                if (a.is(tx, ty))
                    return;

                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    int dx = tx + d.x();
                    int dy = ty + d.y();

                    if (!UtilWallPlacability.wallShouldBuild.is(dx, dy))
                        continue;

                    if (area.is(dx, dy))
                        continue;

                    if (isEdge.is(dx, dy))
                        continue;

                    cWall.render(r, 0, x + C.TILE_SIZE * d.x(), y + C.TILE_SIZE * d.y());
                }
            }

            public override void renderWall(SPRITE_RENDERER r, RenderData.RenderIterator it)
            {
                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    int dx = it.tx() + d.x();
                    int dy = it.ty() + d.y();
                    if (a.is(dx, dy))
                        continue;
                    if (UtilWallPlacability.wallisReal.is(dx, dy))
                    {
                        if (isOpening.is(dx, dy))
                            cDoor.render(r, 0, it.x() + C.TILE_SIZE * d.x(), it.y() + C.TILE_SIZE * d.y());
                        continue;
                    }

                    if (!UtilWallPlacability.wallShouldBuild.is(dx, dy))
                    {
                        continue;
                    }

                    if (isOpening.is(dx, dy))
                        cDoor.render(r, 0, it.x() + C.TILE_SIZE * d.x(), it.y() + C.TILE_SIZE * d.y());
                    else
                        cWall.render(r, 0, it.x() + C.TILE_SIZE * d.x(), it.y() + C.TILE_SIZE * d.y());
                }
            }

            public override void renderWall(SPRITE_RENDERER r, FurnisherItem a, int tx, int ty, int rx, int ry, int x, int y)
            {
                if (!a.is(rx, ry) || a.get(rx, ry).mustBeReachable)
                    return;

                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    int dx = tx + d.x();
                    int dy = ty + d.y();

                    if (a.is(rx + d.x(), ry + d.y()))
                        continue;

                    if (UtilWallPlacability.wallisReal.is(dx, dy))
                    {
                        continue;
                    }

                    if (!UtilWallPlacability.wallCanBe.is(dx, dy))
                        continue;

                    if (!UtilWallPlacability.wallShouldBuild.is(dx, dy))
                    {
                        continue;
                    }

                    getType(a, rx + d.x(), ry + d.y()).render(r, 0, x + C.TILE_SIZE * d.x(), y + C.TILE_SIZE * d.y());
                }
            }

            private UICons getType(FurnisherItem a, int rx, int ry)
            {
                for (int i = 0; i < DIR.ORTHO.size(); i++)
                {
                    DIR d = DIR.ORTHO.get(i);
                    if (a.get(rx, ry, d) != null && a.get(rx, ry, d).mustBeReachable)
                        return cDoor;
                }
                return cWall;
            }
        };

        public PlacerDoor(Room.RoomInstanceImp a, UtilHistory history)
        {
            this.a = a;
            this.history = history;
        }

        public bool isOpening(int tx, int ty)
        {
            for (int i = 0; i < DIR.ALL.size(); i++)
            {
                DIR d = DIR.ALL.get(i);
                int dx = tx + d.x();
                int dy = ty + d.y();
                if (a.is(dx, dy))
                {
                    int m = d.isOrtho() ? d.mask() : (d.mask() << 4);
                    return (ConstructionData.dWall.get(dx, dy) & m) != 0;
                }
            }
            return false;
        }

        public void build(TBuilding structure)
        {
            foreach (COORDINATE c in a.body())
            {
                if (!a.is(c))
                    continue;
                FurnisherItemTile tile = ROOMS().fData.tile.get(c);
                if (tile != null && tile.noWalls)
                    continue;

                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    int dx = c.x() + d.x();
                    int dy = c.y() + d.y();
                    build(structure, dx, dy);
                }
            }
        }

        private void build(TBuilding structure, int tx, int ty)
        {
            if (isOpening(tx, ty))
            {
                UtilWallPlacability.openingBuild(tx, ty, structure);
            }
            else if (UtilWallPlacability.wallShouldBuild.is(tx, ty))
            {
                if (isOpening(tx, ty))
                {
                    UtilWallPlacability.openingBuild(tx, ty, structure);
                }
                else
                {
                    UtilWallPlacability.wallBuild(tx, ty, structure);
                }
            }
        }

        private readonly MAP_BOOLEAN isolationMap = new MAP_BOOLEAN()
        {
            public override bool is(int tile)
            {
                throw new RuntimeException();
            }

            public override bool is(int tx, int ty)
            {
                return UtilWallPlacability.wallCanBe.is(tx, ty) && !isOpening.is(tx, ty);
            }
        };

        public double isolation(RoomBlueprint blue, AREA area, bool wallOn)
        {
            return SETT.ROOMS().isolation.getProspect(blue, area, wallOn ? isolationMap : UtilWallPlacability.wallisReal);
        }

        private readonly MAP_BOOLEAN isEdge = new MAP_BOOLEAN()
        {
            public override bool is(int tx, int ty)
            {
                if (a.is(tx, ty))
                    return false;
                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    if (a.is(tx, ty, d))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override bool is(int tile)
            {
                return false;
            }
        };

        private MAP_BOOLEAN isOpening = new MAP_BOOLEAN()
        {
            public override bool is(int tx, int ty)
            {
                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.get(i);
                    int dx = tx + d.x();
                    int dy = ty + d.y();
                    if (a.is(dx, dy))
                    {
                        int m = d.isOrtho() ? d.mask() : (d.mask() << 4);
                        return (ConstructionData.dWall.get(dx, dy) & m) != 0;
                    }
                }
                return false;
            }

            public override bool is(int tile)
            {
                return false;
            }
        };
    }
}