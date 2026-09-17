using System;
using System.Collections.Generic;

namespace Settlement.Entity.Humanoid.AI.Subject
{
    public class PlanBuryCorpse : PLANRES
    {
        private static string ¤¤verb = "burying corpse";

        private int timeout = -1;

        static PlanBuryCorpse()
        {
            D.ts(typeof(PlanBuryCorpse));
        }

        public PlanBuryCorpse() : base("SUBJECT_GRAVE")
        {
        }

        public bool ShouldBury(Humanoid a, AIManager d)
        {
            if (timeout == TIME.hours().bitsSinceStart())
                return false;

            if (GAME.ARMIES().enemy().men > 0)
                return false;

            if (!SETT.PATH().availability.is(a.tc()))
                return false;

            if (!SETT.PATH().reachability.is(a.tc()))
                return false;

            return true;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            if (GAME.ARMIES().enemy().men > 0)
                return null;

            if (!SETT.PATH().reachability.is(a.tc()))
                return null;

            bool has = false;

            if (!SETT.ROOMS().DUMP.service().finder.has(a.tc()))
            {
                foreach (GRAVE_DATA_HOLDER b in SETT.ROOMS().GRAVES)
                {
                    if (b.graveData().available.get(null) > 0)
                    {
                        has = true;
                        break;
                    }
                }
            }
            else
            {
                has = true;
            }

            if (!has)
            {
                timeout = TIME.hours().bitsSinceStart();
                return null;
            }

            if (!SETT.PATH().finders.corpses.reserve(a.tc(), d.path, int.MaxValue))
            {
                timeout = TIME.hours().bitsSinceStart();
                return null;
            }
            Corpse c = SETT.PATH().finders.corpses.getResult();
            d.planObject = c.index();

            HCLASS cl = c.indu().hType().parentClass();
            foreach (StatGrave g in c.indu().race().service().GRAVES.get(cl.index()))
            {
                if (g.grave().permission().get(cl, c.indu().race()))
                {
                    GRAVE_JOB grave = g.grave().requestAccessTile();
                    if (grave != null)
                    {
                        d.planTile.set(grave.jobCoo());
                        grave.jobReserve(null);
                        AISubActivation s = fetchCorpse.set(a, d);
                        if (s == null)
                        {
                            c.findableReserveCancel();
                        }
                        return s;
                    }
                }
            }

            return dumpStart.set(a, d);
        }

        private readonly Resumer dumpStart = new Resumer(¤¤verb)
        {
            private ROOM_DUMP dump = SETT.ROOMS().DUMP,

            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                COORDINATE coo = dump.service().finder.reserve(a.tc(), int.MaxValue);
                if (coo != null)
                {
                    d.planTile.set(coo);
                    Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                    AISubActivation s = AI.SUBS().walkTo.coo(a, d, c);
                    if (s != null)
                    {
                        return s;
                    }
                    can(a, d);
                }
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (c != null)
                    c.findableReserveCancel();
                return null;
            },

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                return dumpRet.set(a, d);
            },

            public override bool con(Humanoid a, AIManager d)
            {
                return SETT.THINGS().corpses.getByIndex((short)d.planObject) != null && SETT.THINGS().corpses.getByIndex((short)d.planObject).canBeDragged();
            },

            public override void can(Humanoid a, AIManager d)
            {
                FSERVICE s = dump.service().service(d.planTile.x(), d.planTile.y());
                if (s != null)
                    s.findableReserveCancel();
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (c != null)
                    c.findableReserveCancel();
            }
        };

        private readonly Resumer dumpRet = new Resumer(¤¤verb)
        {
            private ROOM_DUMP dump = SETT.ROOMS().DUMP,

            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                return AI.SUBS().walkTo.drag(a, d, SETT.THINGS().corpses.draggable, c.index(), d.planTile);
            },

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (c != null)
                {
                    dump.burry(c, d.planTile.x(), d.planTile.y());
                    HCLASS cl = c.indu().hType().parentClass();
                    foreach (StatGrave g in c.indu().race().service().GRAVES.get(cl.index()))
                    {
                        if (g.grave().permission().get(cl, c.indu().race()))
                        {
                            g.grave().get(cl).fail(c, 1);
                        }
                    }
                    c.remove();
                }
                else
                {
                    can(a, d);
                }
                return null;
            },

            public override bool con(Humanoid a, AIManager d)
            {
                return dump.service().service(d.planTile.x(), d.planTile.y()) != null;
            },

            public override void can(Humanoid a, AIManager d)
            {
                FSERVICE s = dump.service().service(d.planTile.x(), d.planTile.y());
                if (s != null)
                    s.findableReserveCancel();
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (c != null)
                    c.findableReserveCancel();
            }
        };

        private readonly Resumer fetchCorpse = new Resumer(¤¤verb)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);

                RoomBlueprintImp gg = SETT.ROOMS().map.blueprintImp.get(d.planTile);
                if (gg is ROOM_GRAVEYARD)
                {
                    ROOM_GRAVEYARD g = (ROOM_GRAVEYARD)gg;
                    foreach (DIR dir in DIR.ORTHO)
                    {
                        if (g.isGraveHead(d.planTile.x() + dir.x(), d.planTile.y() + dir.y()))
                        {
                            AISubActivation s = AI.SUBS().walkTo.drag(a, d, THINGS().corpses.draggable, c.index(), d.planTile.x() + dir.x(), d.planTile.y() + dir.y());
                            if (s != null)
                                return s;
                        }
                    }
                }

                AISubActivation s = AI.SUBS().walkTo.drag(a, d, THINGS().corpses.draggable, c.index(), d.planTile);
                if (s == null)
                {
                    can(a, d);
                    c.findableReserveCancel();
                }
                return s;
            },

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                return work.set(a, d);
            },

            public override bool con(Humanoid a, AIManager d)
            {
                GRAVE_JOB job = job(d);
                return job != null && job.jobReservedIs(null);
            },

            public override void can(Humanoid a, AIManager d)
            {
                GRAVE_JOB job = job(d);
                if (job != null)
                    job.jobReserveCancel(null);
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (c != null)
                    c.findableReserveCancel();
            }
        };

        private readonly Resumer work = new Resumer(¤¤verb)
        {
            public override AISubActivation res(Humanoid a, AIManager d)
            {
                GRAVE_JOB job = job(d);
                job.buryAndPerform(SETT.THINGS().corpses.getByIndex((short)d.planObject));
                return null;
            },

            public override bool con(Humanoid a, AIManager d)
            {
                GRAVE_JOB job = job(d);
                return job != null && job.jobReservedIs(null);
            },

            public override void can(Humanoid a, AIManager d)
            {
                GRAVE_JOB job = job(d);
                if (job != null)
                    job.jobReserveCancel(null);
                Corpse c = SETT.THINGS().corpses.getByIndex((short)d.planObject);
                if (c != null)
                    c.findableReserveCancel();
            },

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                GRAVE_JOB job = job(d);
                job.jobStartPerforming();
                return AI.SUBS().WORK.activate(a, d, 25);
            }
        };
    }
}