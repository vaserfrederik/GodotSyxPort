using System;
using init.constant;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.health.physician;
using settlement.stats;
using snake2d.util.datatypes;

namespace settlement.entity.humanoid.ai.service
{
    internal sealed class M_PlanPhysician : MPlan<ROOM_PHYSICIAN>
    {
        public M_PlanPhysician() : base("Phys", SETT.ROOMS().PHYSICIANS, false)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.Set(a, d);
        }

        private readonly Resumer first = new Resumer("2")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.Set(a.indu(), 1);
                DIR dir = Blue(d).GetLayDir(d.planTile.x(), d.planTile.y());
                a.speed.SetDirCurrent(dir);
                int x = d.planTile.x() * C.TILE_SIZE + C.TILE_SIZEH + dir.x() * (C.TILE_SIZEH - 2);
                int y = d.planTile.y() * C.TILE_SIZE + C.TILE_SIZEH + dir.y() * (C.TILE_SIZEH - 2);
                a.physics.body().MoveC(x, y);
                return AI.SUBS().LAY.ActivateTime(a, d, 25);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                Get(a, d).Consume();
                Can(a, d);
                foreach (DIR dir in DIR.ORTHO)
                {
                    if (!SETT.PATH().solidity.Is(a.tc(), dir))
                    {
                        int x = a.tc().x() * C.TILE_SIZE + C.TILE_SIZEH + dir.x() * (C.TILE_SIZE);
                        int y = a.tc().y() * C.TILE_SIZE + C.TILE_SIZEH + dir.y() * (C.TILE_SIZE);
                        a.physics.body().MoveC(x, y);
                        break;
                    }
                }

                return null;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return Get(a, d) != null;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.Set(a.indu(), 0);
                if (Blue(d).service().finder.GetReserved(d.planTile.x(), d.planTile.y()) != null)
                    Blue(d).service().finder.GetReserved(d.planTile.x(), d.planTile.y()).FindableReserveCancel();
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.COLLISION_UNREACHABLE)
                    return true;
                return base.Event(a, d, e);
            }
        };
    }
}