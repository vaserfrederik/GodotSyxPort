using init.resources.RBIT;
using init.resources;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.main;
using settlement.room.main;
using settlement.room.main.employment;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    final class PlanFetchEquip : PlanWork
    {
        public PlanFetchEquip(string key) : base(key)
        {
            // TODO Auto-generated constructor stub
        }

        private readonly RBITImp bit = new RBITImp();
        private static readonly CharSequence ¤¤fetch = "Fetching Work Equipment";

        static
        {
            D.ts(typeof(PlanFetchEquip));
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            RoomInstance ins = work(a);
            if (ins == null)
                return null;

            RoomEmploymentSimple ee = ins.blueprintI().employment();
            if (ee == null)
                return null;

            if (ee.tools().Count == 0)
                return null;

            bit.clear();
            foreach (RoomEquip w in ee.tools())
            {
                if (ins.employees().toolsNeeded(w) > 0)
                {
                    bit.or(w.resource);
                }
            }

            RESOURCE r = SETT.PATH().finders.resource.find(bit, a.tc(), d.path, int.MaxValue);

            if (r == null)
                return null;

            foreach (RoomEquip w in ee.tools())
            {
                int am = ins.employees().toolsNeeded(w);
                if (am > 0 && r == w.resource)
                {
                    am = CLAMP.i(am, 0, 15);
                    ins.employees().toolReserve(w, am);
                    return fetch.activateFound(a, d, r, am, true, true);
                }
            }

            throw new RuntimeException();
        }

        private RoomEquip eq(RESOURCE res, RoomInstance ins)
        {
            foreach (RoomEquip w in ins.blueprint().employment().tools())
            {
                if (res == w.resource)
                {
                    return w;
                }
            }
            throw new RuntimeException();
        }

        private readonly AIPlanResourceMany fetch = new AIPlanResourceMany(this, 64)
        {
            protected override AISubActivation next(Humanoid a, AIManager d)
            {
                AISubActivation s = toRoom.set(a, d);
                return s;
            }

            protected override void cancel(Humanoid a, AIManager d)
            {
                RoomInstance ins = work(a);
                if (ins == null)
                    return;
                RESOURCE res = resource(a, d);
                if (res == null)
                    return;
                RoomEquip w = eq(res, ins);
                ins.employees().toolReserve(w, -target(a, d));
            }
        };

        private readonly Resumer toRoom = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                RoomInstance ins = work(a);
                if (ins == null)
                {
                    can(a, d);
                    return null;
                }
                AISubActivation s = AI.SUBS().walkTo.room(a, d, ins);
                if (s != null)
                {
                    return s;
                }
                else
                {
                    res(a, d);
                    return null;
                }
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                fetch.cancel(a, d);
                RoomInstance ins = work(a);
                RoomEquip w = eq(d.resourceCarried(), ins);
                ins.employees().toolDeliver(w, d.resourceA());
                d.resourceCarriedSet(null);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return work(a) != null;
            }

            public override void can(Humanoid a, AIManager d)
            {
                fetch.cancel(a, d);
                d.resourceDrop(a);
            }
        };

        protected override void name(Humanoid a, AIManager d, Str str)
        {
            str.add(¤¤fetch);
        }

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