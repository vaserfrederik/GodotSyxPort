using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.barber;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.service
{
    internal sealed class M_PlanBarber : MPlan<ROOM_BARBER>
    {
        public M_PlanBarber() : base("Barber", SETT.ROOMS().BARBERS, true)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.Set(a, d);
        }

        private readonly Resumer first = new Resumer("")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.planByte2 = (byte)(5 + RND.rInt(15));
                Get(a, d).StartUsing();
                return Res(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (d.planByte2-- < 0)
                {
                    Get(a, d).Consume();
                    for (int di = 0; di < DIR.ORTHO.size; di++)
                    {
                        if (!SETT.PATH().solidity.Is(d.planTile, DIR.ORTHO.Get(di)))
                        {
                            foreach (RES_AMOUNT ra in a.Race().resourcesGroom())
                            {
                                FACTIONS.Player().res.Inc(ra.Resource, RTYPE.PRODUCED, ra.Amount);
                                SETT.THINGS().resources.Create(d.planTile.x, d.planTile.y, ra.Resource, ra.Amount);
                            }
                        }
                    }
                    return null;
                }

                DIR dir = Blue(d).Dir(d.planTile.x, d.planTile.y);

                if (Blue(d).Service.usageSound != null && RND.OneIn(5))
                    Blue(d).Service.usageSound.Rnd(a);

                if (RND.rBoolean())
                {
                    a.Speed.SetDirCurrent(dir);
                    if (RND.rBoolean())
                    {
                        return AI.SUBS.STAND.ActivateRndDir(a, d, 6);
                    }
                    else
                    {
                        return AI.SUBS.STAND.Activate(a, d, AI.STATES().anima.box.Activate(a, d, 3));
                    }
                }
                else
                {
                    dir = dir.Next((int)RND.rSign());
                    return AI.SUBS.STAND.ActivateRndDir(a, d, 6);
                }
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                FINDABLE s = Get(a, d);
                return s != null && s.FindableReservedIs();
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FINDABLE s = Get(a, d);
                if (s != null)
                    s.FindableReserveCancel();
            }
        };
    }
}