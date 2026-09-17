using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.work;
using settlement.main;
using settlement.room.food.farm;
using settlement.room.food.fish;
using settlement.room.food.hunter;
using settlement.room.infra.stockpile;
using settlement.room.main;
using settlement.room.service.arena.grand;
using settlement.room.service.arena.pit;
using settlement.room.service.pleasure;
using settlement.room.spirit.grave;
using settlement.room.spirit.temple;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.rnd;
using util.text;

public static class AIModule_Work
{
    public static int MAX_FETCH_DISTANCE = 250;
    public static int MAX_FETCH_AMOUNT = ROOM_STOCKPILE.MIN_CARRY - 1;

    private static readonly PlanBlueprint[] map = new PlanBlueprint[ROOMS().all().Count];

    private static readonly AIPLAN hangArround = new PlanHangArround("workHang");

    private readonly PlanOddjobber oddjobber = new PlanOddjobber("workOdd");
    private readonly PlanFetchEquip equip = new PlanFetchEquip("workEquip");

    private static readonly string ¤¤name = "Work";
    private static readonly string ¤¤desc = "Spend time working";

    static AIModule_Work()
    {
        // Initialize map with PlanBlueprint instances
        for (int i = 0; i < map.Length; i++)
        {
            map[i] = new PlanBlueprint(); // Assuming PlanBlueprint has a parameterless constructor
        }
    }

    public AIPLAN GetPlan(Humanoid a, AIManager d)
    {
        STATS.WORK().EMPLOYED.set(a, ROOMS().employment.setWork(a));

        if (work(a) != null && work(a).acceptsWork() && map[work(a).blueprint().index()] != null)
        {
            return true;
        }
        return false;
    }

    public void SwapInstance(Humanoid a)
    {
        swapper.swap(a);
    }

    protected void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
    {
        // Update logic here
    }

    protected void Finish(Humanoid a, AIManager d)
    {
        int i = AIModules.data().byte2.get(d);
        if (i == 1)
        {
            STATS.WORK().slackEnd(a, true);
        }
        else if (i == 2)
        {
            STATS.WORK().slackEnd(a, false);
        }
        base.finish(a, d);
    }

    public int GetPriority(Humanoid a, AIManager d)
    {
        if (work(a) == null && !ROOMS().employment.hasWork(a))
        {
            if (GAME.ARMIES().enemy().men() > 0)
                return 0;
            if (!PlanOddjobber.hasOddjob(a, true))
                return 0;
        }

        if (GAME.events().riot.onStrike(a))
            return 0;

        if (STATS.WORK().getWorkPriority(a) > 0)
        {
            return 5;
        }

        return 0;
    }

    public int GetPriority(Humanoid a)
    {
        return GetPriority(a, (AIManager)a.ai());
    }

    private static RoomInstance work(Humanoid a)
    {
        return STATS.WORK().EMPLOYED.get(a.indu());
    }

    private readonly Swapper swapper = new Swapper();

    private bool validateEmployment(Humanoid a, AIManager d)
    {
        ROOMS().employment.setWork(a);

        if (work(a) != null && work(a).acceptsWork() && map[work(a).blueprint().index()] != null)
        {
            return true;
        }
        return false;
    }

    public bool isLawEnforcement(Humanoid a, AIManager d)
    {
        return a.indu().hType() == HTYPES.RECRUIT() || (a.indu().hType() == HTYPES.GUARD());
    }

    public static double getTransportAmount(Humanoid a)
    {
        AIManager d = (AIManager)a.ai();
        if (d.plan() is PlanBlueprint)
        {
            PlanBlueprint t = (PlanBlueprint)d.plan();
            return t.transportAmount(a, d);
        }
        return -1;
    }

    private class Swapper
    {
        private readonly W[] misplaced = new W[SETT.ROOMS().employment.ALLS().Count];

        public Swapper()
        {
            for (int i = 0; i < SETT.ROOMS().employment.ALLS().Count; i++)
            {
                misplaced[i] = new W(SETT.ROOMS().employment.ALLS()[i]);
            }
        }

        private void swap(Humanoid h)
        {
            RoomInstance w = work(h);

            WGROUP group = group(h);
            double p = stayPriority(group, w);
            if (p >= 1)
                return;

            W wer = misplaced[w.blueprintI().employment().eindex()];

            RoomInstance w2 = updateBestRoom(wer, group);

            if (w2 != null && w != w2 && stayPriority(group, w2) > p)
            {
                STATS.WORK().EMPLOYED.set(h, w2);
                return;
            }

            Humanoid h2 = wer.get(group);
            if (h2 != null)
            {
                RoomInstance w2 = work(h2);
                if (w != w2)
                {
                    double swap = stayPriority(group(h2), w) + stayPriority(group, w2);
                    swap -= stayPriority(group, w) + stayPriority(group(h2), w2);

                    if (swap > 0)
                    {
                        h2.interrupt();

                        STATS.WORK().EMPLOYED.set(h2, w);
                        STATS.WORK().EMPLOYED.set(h, w2);
                        wer.set(null, group);
                        return;
                    }
                }
            }

            for (int i = 0; i < WGROUP.all().Count; i++)
            {
                WGROUP g = WGROUP.all()[i];
                if (group == g)
                    continue;
                Humanoid nn = wer.get(g);
                if (nn == null)
                {
                    wer.set(h, g);
                }
                else
                {
                    double p2 = stayPriority(group(nn), work(nn));
                    if (p < p2)
                    {
                        wer.set(h, g);
                    }
                    else if (p == p2 && RND.oneIn(w.blueprintI().employment().employed()))
                    {
                        wer.set(h, g);
                    }
                }
            }
        }

        private RoomInstance updateBestRoom(W wer, WGROUP group)
        {
            if (wer.roomCounts[group.index] >= wer.emp.blueprint().instancesSize())
            {
                wer.roomCounts[group.index] = 0;
            }
            RoomInstance nextBestWork = getEmployableRoom(wer.emp.blueprint(), wer.roomCounts[group.index]);
            wer.roomCounts[group.index]++;

            RoomInstance currentBestWork = getEmployableRoom(wer.emp.blueprint(), wer.currentRoom[group.index]);

            if (nextBestWork != null)
            {
                if (currentBestWork == null)
                {
                    wer.currentRoom[group.index] = wer.roomCounts[group.index];
                    currentBestWork = nextBestWork;
                }
                else
                {
                    double cp = stayPriority(group, currentBestWork);
                    double np = stayPriority(group, nextBestWork);
                    if (np > cp || (np == cp && nextBestWork.employees().target() - nextBestWork.employees().employed() > currentBestWork.employees().target() - currentBestWork.employees().employed()))
                    {
                        wer.currentRoom[group.index] = wer.roomCounts[group.index];
                        currentBestWork = nextBestWork;
                    }
                }
            }

            return currentBestWork;
        }

        private RoomInstance getEmployableRoom(RoomBlueprintIns current, int ri)
        {
            if (ri < current.instancesSize())
            {
                RoomInstance ins = current.getInstance(ri);
                if (ins.active() && ins.employees().employed() < ins.employees().target())
                {
                    return ins;
                }
            }
            return null;
        }

        public double stayPriority(WGROUP group, RoomInstance work)
        {
            if (work == null)
                return 1.0;

            if (work.employees().preferred().is(group))
                return 1.0;
            return 0.5 * group.race.pref().structure(BUILDING_PREFS.get(work.mX(), work.mY()));
        }

        private class W
        {
            public readonly RoomEmploymentSimple emp;
            private readonly Humanoid[] as = new Humanoid[WGROUP.all().Count];
            private readonly int[] roomCounts = Alloc.ii(WGROUP.all().Count);
            private readonly int[] currentRoom = Alloc.ii(WGROUP.all().Count);

            public W(RoomEmploymentSimple emp)
            {
                this.emp = emp;
            }

            public Humanoid get(WGROUP g)
            {
                Humanoid a = as[g.index];
                if (a != null)
                {
                    if (!a.isRemoved() && group(a) != g)
                    {
                        RoomInstance ins = work(a);
                        if (ins != null && ins.blueprintI().employment() == emp)
                            return a;
                    }
                    as[g.index] = null;
                }
                return null;
            }

            private void set(Humanoid h, WGROUP g)
            {
                as[g.index] = h;
            }
        }

        private static WGROUP group(Humanoid h)
        {
            return WGROUP.get(h) == null ? WGROUP.get(HTYPES.SUBJECT(), h.race()) : WGROUP.get(h);
        }
    }
}