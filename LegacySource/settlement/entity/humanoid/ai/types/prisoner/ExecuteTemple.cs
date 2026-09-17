using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.spirit.temple;
using settlement.stats;
using snake2d.util.datatypes;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class ExecuteTemple : AIPLAN.PLANRES
    {
        public ExecuteTemple() : base("prisTemple")
        {
            // TODO Auto-generated constructor stub
        }

        private readonly CharSequence ¤¤name = "¤Being Sacrificed";

        private readonly List<ROOM_TEMPLE> temples = new List<ROOM_TEMPLE>(SETT.ROOMS().TEMPLES.ALL.Count);

        static ExecuteTemple()
        {
            foreach (ROOM_TEMPLE t in SETT.ROOMS().TEMPLES.ALL)
            {
                if (t.sacrifices())
                    temples.Add(t);
            }
            D.t(typeof(ExecuteTemple));
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            for (int ti = 0; ti < temples.Count; ti++)
            {
                ROOM_TEMPLE t = temples[ti];
                COORDINATE c = t.sacrificeReserve(a.race());
                if (c == null)
                    continue;
                d.planByte1 = (byte)ti;
                d.planTile.set(c);
                return walk.set(a, d);
            }
            return null;
        }

        private readonly Resumer walk = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, d.planTile);
                if (s != null)
                    return s;
                cancel(a, d);
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return ready.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer ready = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                a.speed.setDirCurrent(DIR.ALL.rnd());
                temple(a, d).sacrificeSetReady(d.planTile);
                return res(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                double k = temple(a, d).sacrificeKillAmount(d.planTile);
                if (k == 1)
                    AIManager.dead = CAUSE_LEAVES.SACRIFICED();
                STATS.NEEDS().INJURIES.COUNT.indu().setD(a.indu(), k);
                return AI.SUBS().LAY.activateTime(a, d, 1);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        };

        private ROOM_TEMPLE temple(Humanoid a, AIManager d)
        {
            return temples[d.planByte1];
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            temple(a, d).sacrificeUnreserve(d.planTile);
            base.cancel(a, d);
        }

        protected override bool shouldContinue(Humanoid a, AIManager d)
        {
            return temple(a, d).sacrificeReserved(d.planTile) && base.shouldContinue(a, d);
        }
    }
}