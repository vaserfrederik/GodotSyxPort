using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.path;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    final class PlanHangArround : PlanWork
    {
        public PlanHangArround(string key) : base(key)
        {
            // TODO Auto-generated constructor stub
        }

        private static readonly CharSequence ¤¤wait = "¤Waiting for work";

        static
        {
            D.ts(typeof(PlanHangArround));
        }

        public override AISubActivation init(Humanoid a, AIManager d)
        {
            if (!PATH().reachability.is(work(a).mX(), work(a).mY()))
                return null;

            if (work(a).is(a.physics.tileC()))
            {
                if (RND.rInt(25) != 0)
                    return stand.set(a, d);
                return walkIn.set(a, d);
            }

            if (work(a).body().width() <= 1 && work(a).body().height() <= 1)
            {
                if (a.physics.tileC().tileDistanceTo(work(a).mX(), work(a).mY()) < 10)
                {
                    if (RND.rInt(15) != 0)
                        return stand.set(a, d);
                    return walk.set(a, d);
                }
            }

            return walk.set(a, d);
        }

        private readonly Resumer stand = new Resumer(¤¤wait)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (RND.rBoolean())
                    return AI.SUBS().STAND.activate(a, d, AI.STATES().anima.wave.activate(a, d, 2));
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer walk = new Resumer(¤¤wait)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().walkTo.room(a, d, work(a));
            }

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                return stand.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer walkIn = new Resumer(¤¤wait)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                RoomInstance r = work(a);
                int di = RND.rInt(DIR.ORTHO.size());
                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR dd = DIR.ALL.getC(di + i);
                    int dx = a.tc().x() + dd.x();
                    int dy = a.tc().y() + dd.y();
                    if (r.is(dx, dy))
                    {
                        AVAILABILITY av = SETT.PATH().availability.get(dx, dy);
                        if (av.player >= 0 && av.player < AVAILABILITY.Penalty && av.from == 0)
                        {
                            return AI.SUBS().walkTo.cooFull(a, d, dx, dy);
                        }
                    }
                }
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                return stand.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
            {
                return 1.0;
            }
            return base.poll(a, d, e);
        }
    }
}