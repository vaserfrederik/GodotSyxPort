using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity;
using settlement.entity.humanoid.HEvent;
using settlement.entity.humanoid;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.subwalk
{
    final class SubFlee : AISUB.Simple
    {
        public SubFlee() : base("flee")
        {
        }

        public AISubActivation Activate(Humanoid a, AIManager d, ENTITY other)
        {
            a.speed.TurnTo(other.body(), a.body());
            return Activate(a, d);
        }

        public AISubActivation Activate(Humanoid a, AIManager d, int iterations)
        {
            d.subPathByte = (byte)(iterations + 1);
            return Activate(a, d);
        }

        public override AISubActivation Activate(Humanoid a, AIManager d)
        {
            d.subPathByte = (byte)(2 + RND.rInt(5));
            d.subPathByte2 = (byte)(2 + RND.rInt(15));
            return base.Activate(a, d);
        }

        protected override AISTATE Resume(Humanoid a, AIManager d)
        {
            a.speed.TurnWithAngle(RND.rFloat0(90));
            d.subPathByte--;
            if (SETT.TERRAIN().WATER.DEEP.Is(a.tc()))
            {
                d.subPathByte2--;
                if (d.subPathByte2 <= 0)
                {
                    HumanoidResource.dead = CAUSE_LEAVES.DROWNED();
                }
            }

            if (d.subPathByte > 0)
            {
                if (RND.oneIn(3))
                    return AI.STATES().jogCrazy.Activate(a, d, 2f + RND.rFloat() * 3);
                return AI.STATES().jog.Activate(a, d, 2f + RND.rFloat() * 3);
            }

            return null;
        }

        protected override AISTATE ResumeInterrupted(Humanoid a, AIManager d, HEvent event)
        {
            return null;
        }

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.Event == HEvent.COLLISION_UNREACHABLE)
            {
                DIR dd = a.speed.Dir();
                if (!dd.IsOrtho())
                    dd = dd.Next(1);
                for (int i = 0; i < 4; i++)
                {
                    if (SETT.PATH().connectivity.Is(a.tc(), dd))
                    {
                        break;
                    }
                    dd = dd.Next(2);
                    //a.speed.Turn90();
                }
                if (SETT.PATH().connectivity.Is(a.tc(), dd))
                {
                    a.speed.SetRaw(dd, 0.5f);
                }
            }
            else if (e.Event == HEvent.MEET_ENEMY)
            {
                a.speed.TurnTo(-e.norX, -e.norY);
                d.stateTimer = 10;
            }
            else if (e.Event == HEvent.COLLISION_TILE)
            {
                double dx = e.norX;
                double dy = e.norY;
                if (RND.oneIn(4))
                {
                    for (int i = RND.rInt(4); i >= 1; i--)
                    {
                        double y = dy;
                        dy = -dx;
                        dx = y;
                    }
                }
                a.speed.TurnTo(dx, dy);

                return true;
            }
            else if (e.Event == HEvent.EXHAUST)
            {
                return base.Event(a, d, e);
            }
            else if (e.Event == HEvent.COLLISION_HARD)
            {
                return base.Event(a, d, e);
            }
            return false;
        }
    }
}