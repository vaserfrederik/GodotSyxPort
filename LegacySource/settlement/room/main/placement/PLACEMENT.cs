using System;
using System.IO;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.map;
using settlement.main;
using settlement.path.finders;
using settlement.room.main;

namespace settlement.room.main.placement
{
    public class PLACEMENT
    {
        public readonly RoomPlacer placer;
        private readonly Instance instance;

        public readonly MAP_BOOLEAN embryo;

        public PLACEMENT(ROOMS m)
        {
            instance = new Instance(m, factory);

            placer = new RoomPlacer(this, instance);
            embryo = instance;
        }

        public readonly RoomBlueprint factory = new RoomBlueprint("_PLACEMENT")
        {
            protected override void update(double ds)
            {
                placer.update(ds);
            }

            public override SFinderRoomService service(int tx, int ty)
            {
                // TODO Auto-generated method stub
                return null;
            }

            protected override void save(FilePutter saveFile)
            {
            }

            protected override void load(FileGetter saveFile)
            {
                placer.load();
                placer.structure.read();
            }

            protected override void clear()
            {
                placer.init(null, 0);
            }

            public override COLOR miniC(int tx, int ty)
            {
                return COLOR.BLUE50;
            }

            public override COLOR miniCPimped(ColorImp origional, int tx, int ty, bool northern, bool southern)
            {
                return origional;
            }
        };

        public bool canReconstruct(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r.constructor() != null && r.constructor().usesArea())
                return true;
            return false;
        }

        public static CharSequence placable(int tx, int ty, RoomBlueprintImp blue, bool buildOnWalls)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return PlacableMessages.¤¤TERRAIN_BLOCK;
            if (ROOMS().placement.factory.is(tx, ty))
                return PLACABLE.E;
            if (ROOMS().map.is(tx, ty))
                return PlacableMessages.¤¤ROOM_BLOCK;

            // if (blue == null || blue.constructor() == null)
            //     return PLACABLE.E;

            if (TERRAIN().get(tx, ty).clearing().isEasilyCleared())
                return null;

            if (TERRAIN().get(tx, ty).clearing().isStructure())
            {
                if (!blue.constructor().removeTerrain(tx, ty))
                    return null;
                if (!buildOnWalls && !TERRAIN().get(tx, ty).roofIs())
                    return PlacableMessages.¤¤TERRAIN_BLOCK;
                if (blue.constructor().mustBeIndoors())
                {
                    if (TERRAIN().get(tx, ty).roofIs())
                        return null;
                    if ((TERRAIN().get(tx, ty).clearing().can() || TERRAIN().MOUNTAIN.is(tx, ty)) && buildOnWalls)
                        return null;
                    return PlacableMessages.¤¤TERRAIN_BLOCK;
                }
                else if (blue.constructor().mustBeOutdoors())
                {
                    if (!TERRAIN().MOUNTAIN.is(tx, ty) && TERRAIN().get(tx, ty).clearing().can() && buildOnWalls)
                        return null;
                    return PlacableMessages.¤¤TERRAIN_BLOCK;
                }
                else
                {
                    if (TERRAIN().get(tx, ty).roofIs())
                        return null;
                    if ((TERRAIN().get(tx, ty).clearing().can() || TERRAIN().MOUNTAIN.is(tx, ty)) && buildOnWalls)
                        return null;
                }
            }
            else if (TERRAIN().get(tx, ty).clearing().can() && PATH().availability.get(tx, ty).player > 0)
                return null;
            return PlacableMessages.¤¤TERRAIN_BLOCK;
        }
    }
}