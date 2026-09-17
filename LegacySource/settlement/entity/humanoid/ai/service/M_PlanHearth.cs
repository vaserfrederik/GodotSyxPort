using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.service.hearth;
using settlement.room.service.module;
using settlement.stats;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace settlement.entity.humanoid.ai.service
{
    internal sealed class M_PlanHearth : MPlan<ROOM_HEARTH>
    {
        public M_PlanHearth() : base("Hearth", new ArrayList<ROOM_HEARTH>(SETT.ROOMS().HEARTH), true)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.Set(a, d);
        }

        private readonly Resumer first = new Resumer("2")
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                Blue(d).Service().Service(d.Path().DestX(), d.Path().DestY()).StartUsing();
                d.PlanByte1 = 0;
                RoomInstance r = Blue(d).Get(a.Tc().X(), a.Tc().Y());
                a.Speed.Turn2(r.Body().CX() - a.Tc().X(), r.Body().CY() - a.Tc().Y());
                return AI.SUBS().STAND.ActivateTime(a, d, 2 + RND.rInt(15));
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.PlanByte1++;
                STATS.NEEDS().EXPOSURE.Fix(a.Indu());
                if (d.PlanByte1 < 8)
                {
                    Room r = SETT.ROOMS().HEARTH.Get(a.Tc());
                    if (r != null)
                    {
                        ROOM_SERVICER ss = (ROOM_SERVICER)r;
                        if (ss.Service().Total() - ss.Service().Reserved() > 4)
                        {
                            if ((d.PlanByte1 & 1) == 1)
                            {
                                Animation s = RND.rBoolean() ? AI.STATES().anima.box : AI.STATES().anima.wave;
                                return AI.SUBS().single.Activate(a, d, s, 1 + RND.rFloat(3));
                            }
                            return AI.SUBS().STAND.ActivateTime(a, d, 2 + RND.rInt(10));
                        }
                    }
                }

                FSERVICE s = Get(a, d);
                if (s != null && s.FindableReservedIs())
                    s.Consume();

                return null;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                FSERVICE s = Get(a, d);
                return s != null && s.FindableReservedIs();
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FSERVICE s = Get(a, d);
                if (s != null)
                    s.FindableReserveCancel();
            }
        };
    }
}