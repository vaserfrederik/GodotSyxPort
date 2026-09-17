using System;
using settlement.entity.humanoid.ai.work;
using static settlement.main.SETT.ROOMS;
using static settlement.main.SETT.THINGS;

using game.GAME;
using init.constant.C;
using init.resources.RESOURCE;
using init.resources.RES_AMOUNT;
using init.type.CAUSE_LEAVES;
using init.type.HTYPES;
using settlement.entity.ENTITY;
using settlement.entity.humanoid.Humanoid;
using settlement.entity.humanoid.ai.main.AI;
using settlement.entity.humanoid.ai.main.AIManager;
using settlement.entity.humanoid.ai.main.AISUB.AISubActivation;
using settlement.main.SETT;
using settlement.misc.job.SETT_JOB;
using settlement.room.food.cannibal.Cage;
using settlement.room.food.cannibal.CannibalInstance;
using settlement.room.food.cannibal.ROOM_CANNIBAL;
using settlement.stats.STATS;
using settlement.thing.ThingsCorpses.Corpse;
using snake2d.util.datatypes.DIR;

namespace settlement.entity.humanoid.ai.work
{
    final class WorkCannibal : PlanBlueprint
    {
        private readonly ROOM_CANNIBAL b = ROOMS().CANNIBAL;

        protected WorkCannibal(AIModule_Work module, PlanBlueprint[] map) : base(module, ROOMS().CANNIBAL, map) { }

        public override AISubActivation init(Humanoid a, AIManager d)
        {
            CannibalInstance in = (CannibalInstance)work(a);
            SETT_JOB j = in.getWork();

            if (j == null)
            {
                GAME.Notify("Weird " + in.mX() + " " + in.mY());
                return null;
            }

            d.planTile.set(j.jobCoo());

            Cage c = b.getWorkCage(in);

            if (c == null)
                return null;

            AISubActivation s = AI.SUBS().walkTo.coo(a, d, c.coo());

            if (s != null)
            {
                in.getWork(d.planTile).jobReserve(null);
                b.cage(d.path.destX(), d.path.destY()).grab();
                fetch1.set(a, d);
                return s;
            }
            return null;
        }

        private SETT_JOB job(Humanoid a, AIManager d)
        {
            if (work(a) == null)
                return null;
            CannibalInstance in = (CannibalInstance)work(a);
            return in.getWork(d.planTile);
        }

        final Resumer fetch1 = new Resumer(b.employment().verb)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d) => null;

            public override AISubActivation res(Humanoid a, AIManager d) => kill.set(a, d);

            public override bool con(Humanoid a, AIManager d)
            {
                if (job(a, d) == null || !job(a, d).jobReservedIs(null))
                    return false;
                Cage c = b.cage(d.path.destX(), d.path.destY());
                if (c == null)
                    return false;
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                if (job(a, d) != null && job(a, d).jobReservedIs(null))
                    job(a, d).jobReserveCancel(null);
                Cage c = b.cage(d.path.destX(), d.path.destY());
                if (c != null)
                    c.grabCancel();
            }
        };

        final Resumer kill = new Resumer(b.employment().verb)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d) => AI.SUBS().WORK_HANDS.activate(a, d, 3.0);

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                b.cage(d.path.destX(), d.path.destY()).grabCancel();

                Corpse c = victim(d);
                if (c == null)
                {
                    can(a, d);
                    return null;
                }
                c.findableReserve();
                d.planObject = c.index();
                b.setRace(d.planTile.x(), d.planTile.y(), c.race());

                return drag_back2.set(a, d);
            }

            private Corpse victim(AIManager d)
            {
                ENTITY e = SETT.ENTITIES().getAtTileSingle(d.path.destX(), d.path.destY());
                if (e == null)
                    return null;
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    if (a.indu().hType() != HTYPES.PRISONER())
                        return null;

                    a.kill(false, CAUSE_LEAVES.EXECUTED());

                    return SETT.PATH().finders.corpses.getReservable(d.path.destX(), d.path.destY());
                }
                return null;
            }

            public override bool con(Humanoid a, AIManager d) => fetch1.con(a, d);

            public override void can(Humanoid a, AIManager d) => fetch1.can(a, d);
        };

        final Resumer drag_back2 = new Resumer(b.employment().verb)
        {
            public override AISubActivation res(Humanoid a, AIManager d) => butcher2.set(a, d);

            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Corpse prey = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                return AI.SUBS().walkTo.drag(a, d, SETT.THINGS().corpses.draggable, prey.index(), d.planTile);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                if (job(a, d) == null || !job(a, d).jobReservedIs(null))
                    return false;
                return SETT.THINGS().corpses.getByIndex((short)d.planObject) != null;
            }

            public override void can(Humanoid a, AIManager d)
            {
                Corpse prey = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (prey != null)
                {
                    prey.findableReserveCancel();
                    d.planObject = -1;
                }
                if (job(a, d) != null && job(a, d).jobReservedIs(null))
                    job(a, d).jobReserveCancel(null);
            }
        };

        final Resumer butcher2 = new Resumer(b.employment().verb)
        {
            public override AISubActivation res(Humanoid a, AIManager d)
            {
                Corpse prey = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (prey == null)
                {
                    can(a, d);
                    return null;
                }

                produce(prey, a, d);

                if (prey.isRemoved())
                {
                    can(a, d);
                    return null;
                }
                b.employment().sound().rnd(a);
                return AI.SUBS().WORK_HANDS.activate(a, d, 5);
            }

            private void produce(Corpse corpse, Humanoid a, AIManager d)
            {
                SETT.ROOMS().CANNIBAL.reportCannibal(corpse.race());

                if (corpse.resLeft() <= 0)
                {
                    if (corpse.indu().race().resources().size() > 0)
                    {
                        RES_AMOUNT rr = corpse.indu().race().resources().rnd();
                        produce(rr.resource(), 1, a, d);
                    }
                    corpse.remove();
                    return;
                }

                int goreAmount = 10; // Example value, adjust as needed
                if (goreAmount > 0)
                {
                    corpse.gore(DIR.N, goreAmount);
                }
            }

            public override bool con(Humanoid a, AIManager d)
            {
                CannibalInstance in = (CannibalInstance)work(a);
                if (in == null)
                    return false;
                if (in.getWork(d.planTile) == null || !in.getWork(d.planTile).jobReservedIs(null))
                    return false;
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                Corpse prey = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (prey != null)
                {
                    prey.findableReserveCancel();
                    d.planObject = -1;
                }
                CannibalInstance in = (CannibalInstance)work(a);
                if (in == null)
                    return;
                if (in.getWork(d.planTile) == null || !in.getWork(d.planTile).jobReservedIs(null))
                    return;
                in.getWork(d.planTile).jobReserveCancel(null);
            }

            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Corpse prey = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                prey.drag(DIR.N, d.planTile.x() * C.TILE_SIZE + C.TILE_SIZEH, d.planTile.y() * C.TILE_SIZE + C.TILE_SIZEH, 0);
                CannibalInstance in = (CannibalInstance)work(a);
                in.resetGore(d.planTile);
                return AI.SUBS().WORK_HANDS.activate(a, d, 12);
            }
        };

        protected override bool shouldContinue(Humanoid a, AIManager d)
        {
            bool be = base.shouldContinue(a, d);
            if (!be)
                GAME.Notify("here");
            return be;
        }
    }
}