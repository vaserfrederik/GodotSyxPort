using System;
using System.IO;
using init.constant;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.military.training;
using settlement.stats;
using settlement.thing.projectiles;
using snake2d.util.datatypes;

namespace settlement.room.military.training.archery
{
    public sealed class ROOM_ARCHERY : ROOM_M_TRAINER<ArcheryInstance>
    {
        private readonly Constructor constructor;
        private readonly ArcheryThing thing = new ArcheryThing(this);
        private readonly Trajectory[] trajs = new Trajectory[4];

        public ROOM_ARCHERY(int typeIndex, RoomInitData data, string key) : base(typeIndex, data, key)
        {
            constructor = new Constructor(this, data)
            {
                Create = (TmpArea area, RoomInit init) => new ArcheryInstance(this, area, init),
                IsHeavy = () => true
            };

            double dist = constructor.Item(1).Height() - 1;
            dist *= C.TILE_SIZE;
            int i = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                Trajectory t = new Trajectory();
                t.CalcLow(0, 0, 0, (int)(d.X() * dist), (int)(d.Y() * dist), 45, 40 * C.TILE_SIZE);
                trajs[i++] = t;
            }
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public DIR FaceCoo(int tx, int ty)
        {
            FurnisherItem t = SETT.ROOMS().fData.item.Get(tx, ty);
            if (t != null)
                return DIR.ORTHO.Get(t.Rotation);
            return DIR.C;
        }

        public void FireArrow(int tx, int ty, int x, int y)
        {
            if (Is(tx, ty))
            {
                FurnisherItem it = SETT.ROOMS().fData.item.Get(tx, ty);
                if (it != null)
                {
                    Trajectory t = trajs[it.Rotation];
                    SETT.PROJS().LaunchDummy(x, y, 0, t, STATS.EQUIP().RANGED().Get(0).projectile, 0, null);
                }
            }
        }
    }
}