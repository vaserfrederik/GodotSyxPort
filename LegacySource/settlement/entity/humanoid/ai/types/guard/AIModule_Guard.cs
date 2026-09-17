using System;
using settlement.main;
using game.battle.div;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.path.components;
using settlement.room.law.guard;
using settlement.stats;
using util.text;

public sealed class AIModule_Guard : AIModule
{
    private readonly PlanWork guard;
    private readonly PlanGear gear;
    private readonly PlanPatrol patrol;
    private readonly PlanMop mop;
    private readonly PlanExecute execute = new PlanExecute();

    private static readonly CharSequence ¤¤name = "Guard";
    private static readonly CharSequence ¤¤desc = "Guard duty.";
    static
    {
        D.ts(AIModule_Guard);
    }

    public AIModule_Guard() : base(UI.icons().s.clock, ¤¤name, ¤¤desc)
    {
        guard = new PlanWork();
        gear = new PlanGear();
        patrol = new PlanPatrol();
        mop = new PlanMop();
    }

    public override AiPlanActivation getPlan(Humanoid a, AIManager d)
    {
        AiPlanActivation p = gear.activate(a, d);

        if (p != null)
            return p;

        p = execute.activate(a, d);

        if (p != null)
            return p;

        if (SETT.ROOMS().GUARD.emp.employ(a))
        {
            AI.modules().work.swapInstance(a);
            p = guard.activate(a, d);
            if (p != null)
                return p;
        }

        Humanoid c = SETT.ROOMS().GUARD.reporter.pollCriminal(null);
        if (c != null)
        {
            p = AI.listeners().catchCriminal(c).activate(a, d);
            if (p != null)
                return p;
        }

        p = patrol.activate(a, d);
        if (p != null)
            return p;

        return null;
    }

    protected override void init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
    {
    }

    public static bool ShouldBe(Humanoid a)
    {
        Div div = STATS.BATTLE().DIV.get(a);
        if (div != null && SETT.ROOMS().GUARD.activeDuty.is(div))
            return true;
        return false;
    }

    public override int getPriority(Humanoid a, AIManager d)
    {
        double w = STATS.WORK().WORK_TIME.indu().getD(a.indu());
        if (w >= 1)
            return 0;

        if (w < 0.5)
            return 4;

        GuardInstance ins = PlanWork.work(a);

        if (ins != null && ins.hasPotentialSpots())
            return 4;

        return 0;
    }

    protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
    {
        if (hasEnemies(a, d))
        {
            d.overwrite(a, mop);
        }
    }

    private bool hasEnemies(Humanoid a, AIManager d)
    {
        if (STATS.POP().pop(HTYPES.ENEMY()) == 0 && STATS.POP().pop(HTYPES.RIOTER()) == 0)
            return false;
        SComponent ss = SETT.PATH().comps.levels.get(0).get(a.tc());
        if (ss == null)
            return false;
        if (PATH().comps.data.people(a.indu().hostile()).get(ss) > 0)
            return true;
        SComponentEdge e = ss.edgefirst();
        while (e != null)
        {
            if (PATH().comps.data.people(a.indu().hostile()).get(e.to()) > 0)
                return true;
            e = e.next();
        }
        return false;
    }
}