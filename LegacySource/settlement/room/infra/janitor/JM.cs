using System;
using game;
using game.audio;
using game.faction;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using snake2d.util.datatypes;

namespace settlement.room.infra.janitor
{
    internal class JM : JOB_MANAGER
    {
        private JanitorInstance ins;
        private readonly ROOM_JANITOR b;
        private readonly Coo coo = new Coo();

        public JM(ROOM_JANITOR b)
        {
            this.b = b;
        }

        public JOB_MANAGER Get(JanitorInstance ins)
        {
            this.ins = ins;
            return this;
        }

        public void ReportResourceFound(RESOURCE res)
        {
        }

        public bool ResourceReachable(RESOURCE res)
        {
            return !ins.bits.ResMissing(res);
        }

        public bool ResourceShouldSearch(RESOURCE res)
        {
            return SETT.PATH().finders.maintenance.mask(ins.mX(), ins.mY()).Has(res);
        }

        public SETT_JOB GetReservableJob(COORDINATE prefered)
        {
            if (prefered == null)
            {
                if (!ins.bits.ResMaskWorker(ins).IsClear())
                {
                    coo.Set(ins.rx, ins.ry);
                    return res;
                }

                if (!ins.searchForJobs)
                    return null;

                SETT_JOB j = Search(ins.mX(), ins.mY(), ROOM_JANITOR.radius);

                if (j == null)
                    ins.searchForJobs = false;

                return j;
            }

            if (!ins.bits.ResMaskFetcherMust(ins).IsClear())
            {
                coo.Set(ins.rx, ins.ry);
                return res;
            }

            if (ins.Is(prefered) && !ins.bits.ResMaskFetcher(ins).IsClear())
            {
                coo.Set(ins.rx, ins.ry);
                return res;
            }

            int tx = prefered.X();
            int ty = prefered.Y();

            coo.Set(prefered);

            if (SETT.MAINTENANCE().reservable.Is(tx, ty))
            {
                RESOURCE res = SETT.MAINTENANCE().resource.Get(tx, ty);
                if (res == null || ins.bits.ResAm(res) > 0)
                    return work;
            }
            if (!ins.Is(prefered))
            {
                SETT_JOB j = Search(prefered.X(), prefered.Y(), 32);
                if (j != null)
                    return j;
            }

            if (!ins.searchForJobs)
                return null;

            return Search(ins.mX(), ins.mY(), ROOM_JANITOR.radius);
        }

        public SETT_JOB ReportResourceMissing(RBIT resourceMask, int jx, int jy)
        {
            ins = b.Get(jx, jy);
            if (ins != null)
            {
                ins.bits.ResSetMissing(resourceMask);
                coo.Set(jx, jy);
                SETT_JOB j = GetReservableJob(coo);
                return j;
            }
            return null;
        }

        public SETT_JOB GetJob(COORDINATE c)
        {
            coo.Set(c);

            if (coo.IsSameAs(ins.rx, ins.ry))
            {
                return res;
            }

            if (SETT.MAINTENANCE().isser.Is(coo))
            {
                return work;
            }

            return null;
        }

        private SETT_JOB Search(int sx, int sy, int distance)
        {
            COORDINATE c = SETT.PATH().finders.maintenance.FindWithin(ins.bits.ResHave(), sx, sy, ROOM_JANITOR.radius, ins.mX(), ins.mY());

            if (c != null)
            {
                coo.Set(c);
                return work;
            }

            return null;
        }

        private readonly SETT_JOB work = new Job();

        int lx, ly;

        private class Job : SETT_JOB
        {
            private int wt = 20;

            public bool JobUseTool()
            {
                return true;
            }

            public void JobStartPerforming()
            {
            }

            public SoundRace JobSound()
            {
                return b.employment().sound();
            }

            public RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public bool JobReservedIs(RESOURCE r)
            {
                return SETT.MAINTENANCE().reserved.Is(coo.X(), coo.Y());
            }

            public void JobReserveCancel(RESOURCE r)
            {
                SETT.MAINTENANCE().reserved.Set(coo, false);
                r = SETT.MAINTENANCE().resource.Get(coo);
                if (r != null && ins.bits.ResAm(r) > 0)
                {
                    ins.bits.ResInc(ins, r, 1);
                    FACTIONS.player().res().Inc(r, RTYPE.MAINTENANCE, 1);
                }
            }

            public bool JobReserveCanBe()
            {
                return SETT.MAINTENANCE().reservable.Is(coo);
            }

            public void JobReserve(RESOURCE r)
            {
                if (coo.IsSameAs(ins.rx, ins.ry))
                    GAME.Notify("FUCKFUCK");
                r = SETT.MAINTENANCE().resource.Get(coo);
                if (r != null && ins.bits.ResAm(r) > 0)
                {
                    ins.bits.ResInc(ins, r, -1);
                    FACTIONS.player().res().Inc(r, RTYPE.MAINTENANCE, -1);
                }
                SETT.MAINTENANCE().reserved.Set(coo, true);
            }

            public double JobPerformTime(Humanoid skill)
            {
                if (coo.IsSameAs(lx, ly) || SETT.MAINTENANCE().resource.Get(coo) != null)
                    return 1;

                return SETT.MAINTENANCE().pFreeFetch.Is(coo) ? 1 : wt;
            }

            public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
            {
                SETT.MAINTENANCE().reserved.Set(coo, false);
                SETT.MAINTENANCE().maintain(coo.X(), coo.Y());

                lx = coo.X();
                ly = coo.Y();
                SETT.MAINTENANCE().pFreeFetch.Set(coo, ins.employees().fetchBonusConsume(wt + 1));
                return null;
            }

            public CharSequence JobName()
            {
                return b.employment().verb;
            }

            public COORDINATE JobCoo()
            {
                return coo;
            }
        }

        internal readonly SETT_JOB res = new SETT_JOB()
        {
            public bool JobUseTool()
            {
                return false;
            }

            public void JobStartPerforming()
            {
            }

            public SoundRace JobSound()
            {
                return null;
            }

            public RBIT JobResourceBitToFetch()
            {
                return ins.bits.ResMaskFetcher(ins);
            }

            public bool JobReservedIs(RESOURCE r)
            {
                return ins.bits.ResReserved(r);
            }

            public void JobReserveCancel(RESOURCE r)
            {
                ins.bits.ResReserve(ins, r, false);
            }

            public bool JobReserveCanBe()
            {
                return !ins.bits.ResMaskFetcher(ins).IsClear();
            }

            public void JobReserve(RESOURCE r)
            {
                ins.bits.ResReserve(ins, r, true);
            }

            public double JobPerformTime(Humanoid skill)
            {
                return 0;
            }

            public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
            {
                ins.bits.ResReserve(ins, r, false);
                ins.bits.ResInc(ins, r, ram);

                bool view = false;

                for (int i = 0; i < 8; i++)
                {
                    if (((ins.tableRes >> (i * 8)) & 0x0FF) == r.Index() + 1)
                    {
                        view = true;
                        break;
                    }
                }

                if (!view)
                {
                    ins.tableRes = ins.tableRes << 8;
                    ins.tableRes |= r.Index() + 1;
                }

                return null;
            }

            public CharSequence JobName()
            {
                return b.employment().verb;
            }

            public COORDINATE JobCoo()
            {
                return coo;
            }
        };

        public void ResetResourceSearch()
        {
            ins.bits.Update();
        }
    }
}