using System;
using settlement.room.main.construction;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.tilemap.terrain;
using snake2d.util.datatypes;

namespace settlement.room.main.construction
{
    public class ConstructionInit
    {
        public readonly int Upgrade;
        public readonly Furnisher B;
        public readonly TBuilding Structure;
        public readonly int Degrade;
        public readonly RoomState State;

        private static RoomAreaWrapper Wrap = new RoomAreaWrapper();

        public ConstructionInit(int upgrade, Furnisher b, TBuilding structure, int degrade, RoomState state)
        {
            this.Upgrade = upgrade;
            this.Structure = structure;
            this.Degrade = degrade;
            this.State = state;
            this.B = b;
        }

        public ConstructionInit(Room room, int rx, int ry, bool broken)
        {
            this(room, rx, ry, room.Degrader(rx, ry) == null ? 0 : room.Degrader(rx, ry).GetData(), broken);
        }

        private ConstructionInit(Room room, int rx, int ry, int degrade, bool broken)
        {
            this(room.Upgrade(rx, ry), room.Constructor(), FindStructure(rx, ry), degrade, room.MakeState(rx, ry, broken));
        }

        public static TBuilding FindStructure(int rx, int ry)
        {
            Room room = SETT.ROOMS().Map.Get(rx, ry);

            if (room == null)
                return null;

            if (room is ConstructionInstance)
                return ((ConstructionInstance)room).Structure();

            ROOMA a = Wrap.Init(room, rx, ry);
            Wrap.Done();
            foreach (COORDINATE c in a.Body())
            {
                if (a.Is(c))
                {
                    TerrainTile t = TERRAIN().Get(c.X(), c.Y());
                    if (t is BuildingComponent)
                        return ((BuildingComponent)TERRAIN().Get(c.X(), c.Y())).Building();
                }
            }

            return TERRAIN().BUILDINGS.MUD;
        }
    }
}