using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.main;
using settlement.misc.util;
using settlement.room.infra.logistics;
using settlement.room.main;
using settlement.stats;
using snake2d;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    public class WorkDeliveryman : PlanBlueprint
    {
        private readonly bool standAround;

        private static CharSequence ¤¤storing = "Storing Goods";
        private static CharSequence ¤¤waiting = "Waiting for orders";

        static
        {
            D.ts(typeof(WorkDeliveryman));
        }

        protected WorkDeliveryman(AIModule_Work module, PlanBlueprint[] map, RoomBlueprintIns<?> b, bool standAround) : base("work_delivery_" + b.key, module, b, map)
        {
            this.standAround = false;
        }

        public override AISubActivation init(Humanoid a, AIManager d)
        {
            RoomInstance i = (RoomInstance)work(a);
            ROOM_MOVEJOBBER jobber = (ROOM_MOVEJOBBER)i;

            MoveJob j = jobber.moveJob(a);

            if (j == null)
            {
                if (standAround)
                    return standing.set(a, d);
                return null;
            }

            if (!d.path.request(a.tc(), j.source))
            {
                LOG.ln("NAY " + a.tc() + " " + j.source);
                return null;
            }

            Room room = SETT.ROOMS().map.get(j.dest);
            if (room == null)
            {
                fuckup(j, a);
            }
            TILE_STORAGE st = room.storage(j.dest.x(), j.dest.y());
            if (st == null)
            {
                fuckup(j, a);
            }
            if (st.resource() != j.res)
            {
                System.err.println(st.resource() + " " + j.res);
                fuckup(j, a);
            }
            if (j.maxAm <= 0)
            {
                fuckup(j, a);
            }
            if (st.storageReservable() < j.maxAm)
            {
                fuckup(j, a);
            }

            j.maxAm = CLAMP.i(j.maxAm, 0, 0b0011_1111);

            st.storageReserve(j.maxAm);

            if (RESOURCE_TILE.GETTER.reserve(j.stored, j.prio, j.res, j.source.x(), j.source.y(), 1) == 0)
            {
                fuckup(j, a);
            }

            d.planByte1 = (byte)j.maxAm;
            d.planTile.set(j.dest.x(), j.dest.y());
            AISubActivation s = fetch.activateFound(a, d, j.res, d.planByte1, j.stored, j.prio);
            if (s == null)
            {
                unreserve(a, d);
            }
            STATS.WORK().fetchProximityStart(a);
            return s;
        }

        private void fuckup(MoveJob j, Humanoid a)
        {
            System.err.println(j.res);
            System.err.println(j.source);
            System.err.println(j.dest);
            System.err.println(j.maxAm);
            throw new RuntimeException("" + work(a));
        }

        private readonly AIPlanResourceMany fetch = new AIPlanResourceMany(this, 48)
        {
            public override AISubActivation next(Humanoid a, AIManager d)
            {
                d.planByte2 = resource(a, d).bIndex();
                return return_resource.set(a, d);
            }

            public override void cancel(Humanoid a, AIManager d)
            {
                STATS.WORK().fetchProximityEnd(a);
                unreserve(a, d);
            }
        };

        private void unreserve(Humanoid a, AIManager d)
        {
            TILE_STORAGE c = targetStorage(a, d);
            if (c != null)
            {
                int i = CLAMP.i(d.planByte1, 0, c.storageReserved());
                c.storageUnreserve(i);
            }
        }

        private TILE_STORAGE targetStorage(Humanoid a, AIManager d)
        {
            Room r = ROOMS().map.get(d.planTile);
            if (r != null)
                return r.storage(d.planTile.x(), d.planTile.y());
            return null;
        }

        private readonly Resumer return_resource = new Resumer(¤¤storing)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (!con(a, d))
                {
                    can(a, d);
                    return WAIT_AND_EXIT.set(a, d);
                }
                return AI.SUBS().walkTo.coo(a, d, d.planTile);
            }

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                if (!con(a, d))
                {
                    can(a, d);
                    return WAIT_AND_EXIT.set(a, d);
                }
                TILE_STORAGE c = targetStorage(a, d);
                int am = d.resourceA();
                am = CLAMP.i(am, 0, c.storageReserved());
                c.storageDeposit(am);

                int res = d.planByte1 - am;
                if (res > 0)
                    c.storageUnreserve(res);

                if (d.resourceCarried() != null)
                    d.resourceAInc(-am);
                d.resourceDrop(a);

                int i = d.resourceA() - am;
                if (i > 0)
                    d.resourceDrop(a);
                d.resourceCarriedSet(null);
                STATS.WORK().fetchProximityEnd(a);
                return WAIT_AND_EXIT.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                if (work(a) != null)
                {
                    TILE_STORAGE c = targetStorage(a, d);

                    if (c != null && c.storageReserved() > 0 && c.resource() != null && c.resource().bIndex() == d.planByte2)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void can(Humanoid a, AIManager d)
            {
                unreserve(a, d);
                STATS.WORK().fetchProximityEnd(a);
                d.resourceDrop(a);
            }
        };

        private readonly Resumer standing = new Resumer(¤¤waiting)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (work(a).is(a.tc()))
                    return AI.SUBS().STAND.activateRndDir(a, d, 5);
                return AI.SUBS().walkTo.room(a, d, work(a));
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

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.type == HPoll.WORKING)
                    return 0;
                return base.poll(a, d, e);
            }
        };

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
            {
                return getResumer(d) == standing ? 0 : 1;
            }
            return base.poll(a, d, e);
        }
    }
}