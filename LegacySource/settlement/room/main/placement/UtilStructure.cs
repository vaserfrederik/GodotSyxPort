using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Main.Placement
{
    public sealed class UtilStructure : GETTER_IMP<TBuilding>
    {
        private readonly RoomPlacer p;
        private readonly PropGame saver = PROP.Game("ROOM_CONSTRUCTION");

        public UtilStructure(RoomPlacer p)
        {
            this.p = p;

            Set(SETT.TERRAIN().BUILDINGS.MUD);
        }

        public SETT_JOB GetWallJob(int tx, int ty)
        {
            if (SETT.TERRAIN().CAVE.Is(tx, ty))
                return SETT.JOBS().clearss.caveFill;
            return SETT.JOBS().build_structure.Get(Get().structure.Index()).wall;
        }

        public SETT_JOB GetCeilingJob(int tx, int ty)
        {
            if (SETT.TERRAIN().MOUNTAIN.Is(tx, ty))
                return null;
            if (Get().roof.Is(tx, ty))
                return null;
            if (SETT.TERRAIN().CAVE.Is(tx, ty))
                return null;
            return SETT.JOBS().build_structure.Get(Get().structure.Index()).ceiling;
        }

        public void Read()
        {
            string k = saver.Chars("STRUCTURE");
            if (k != null && STRUCTURES.Map().TryGet(k) != null)
            {
                Set(STRUCTURES.Map().TryGet(k).Terrain());
            }
        }

        private int unroofed = 0;
        private int tick = 0;

        public int Roofs()
        {
            if (GAME.UpdateI() != tick)
            {
                unroofed = 0;
                foreach (COORDINATE c in p.instance.Body())
                {
                    if (!p.instance.Is(c))
                        continue;

                    if (GetCeilingJob(c.X(), c.Y()) != null)
                    {
                        unroofed++;
                    }
                }
                if (p.autoWalls.Is())
                {
                    unroofed += p.door.GetOpenings();
                }
            }
            return unroofed;
        }

        public override void Set(TBuilding t)
        {
            base.Set(t);
            saver.CharsSet("STRUCTURE", t.structure.key);
        }

        public int Walls()
        {
            if (!p.autoWalls.Is())
                return 0;
            return p.door.GetWalls();
        }

        public int MountainWalls()
        {
            if (!p.autoWalls.Is())
                return 0;
            return p.door.GetMountains();
        }

        public void Set(int tx, int ty)
        {
            TerrainTile t = SETT.TERRAIN().Get(tx, ty);
            if (t is TBuilding.BuildingComponent)
            {
                Set(((TBuilding.BuildingComponent)t).Building());
            }
        }
    }
}