using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.room.main;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    public sealed class AIModule_Prisoner : AIModule
    {
        public static PrisonerData DATA()
        {
            return PrisonerData.self;
        }

        private static AIModule_Prisoner self;
        private readonly Jail jailed = new Jail();
        private readonly PlanExecuted executed = new PlanExecuted();
        private readonly Prison prison = new Prison();
        private readonly Judged judged = new Judged();
        private readonly AIPLAN[] executions = new AIPLAN[]
        {
            new ExecuteTemple(),
            new ExecuteArena(),
        };
        private readonly AIPLAN cannibal = new ExecuteCannibal();
        private readonly Stocked stocked = new Stocked();
        public const byte PRISON_DAYS = (byte)(2 * TIME.years().bitConversion(TIME.days()));

        private static readonly string ¤¤name = "Atonement";

        static AIModule_Prisoner()
        {
            D.ts(typeof(AIModule_Prisoner));
        }

        public AIModule_Prisoner() : base(UI.icons().s.bars, ¤¤name, null)
        {
            new PrisonerData();
            self = this;
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            PUNISHMENT pp = punishment(a, d);

            if (pp == CRIME_PUNISHMENTS.PARDON())
                return free.activate(a, d);

            if (DATA().judged.get(d) == 0 && STATS.LAW().prisonerType.get(a.indu()).isJudged)
            {
                AiPlanActivation s = judged.activate(a, d);
                if (s != null)
                    return s;
                if (SETT.ROOMS().COURT.employment().employed() <= 0)
                    DATA().judgeWait.set(d, DATA().judgeWait.max(d));
                if (!DATA().judgeWait.isMax(d))
                {
                    AiPlanActivation p = jailed.activate(a, d);
                    if (p != null)
                        return p;
                }
            }

            if (!TIME.light().nightIs() && (DATA().stocked.get(d) == 0 || ((TIME.days().bitsSinceStart() + STATS.RAN().get(a.indu(), 3)) & 7) == 0))
            {
                DATA().stocked.setMax(d);
                AiPlanActivation s = stocked.activate(a, d);
                if (s != null)
                {
                    return s;
                }
            }

            if (PrisonerData.self.prisonTimeLeft.get(d) == AIModule_Prisoner.PRISON_DAYS + 1)
            {
                PrisonerData.self.prisonTimeLeft.inc(d, -1);
                AiPlanActivation p = jailed.activate(a, d);
                if (p != null)
                {
                    return p;
                }
            }

            AiPlanActivation s = plan(a, d, pp);

            if (s != null)
                return s;
            s = jailed.activate(a, d);
            if (s != null)
                return s;
            return exile.activate(a, d);
        }

        public static PUNISHMENT punishment(Humanoid a, HAI d)
        {
            PUNISHMENT p = DATA().punishmentSet.get(d);
            if (p != null && p != CRIME_PUNISHMENTS.STOCKS())
            {
                return p;
            }
            return STATS.LAW().prisonerType.get(a.indu()).cl == HCLASSES.SLAVE() ? CRIME_PUNISHMENTS.ENSLAVE() : CRIME_PUNISHMENTS.PARDON();
        }

        public override void init(Humanoid a, AIManager d)
        {
            base.init(a, d);
        }

        public override void update(Humanoid a, AIManager d)
        {
            base.update(a, d);
        }

        public override void cancel(Humanoid a, AIManager d)
        {
            base.cancel(a, d);
        }

        private readonly AIPLAN exile = new AIPLAN.PLANRES("prisLeave")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                return start.set(a, d);
            }

            private readonly Resumer start = new Resumer(CRIME_PUNISHMENTS.BANISH().verb)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (SETT.PATH().finders.entryPoints.find(a.tc().x(), a.tc().y(), d.path, int.MaxValue))
                    {
                        return AI.SUBS().walkTo.pathFull(a, d);
                    }
                    return finish(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    return finish(a, d);
                }

                private AISubActivation finish(Humanoid a, AIManager d)
                {
                    DATA().punish(a, d, CRIME_PUNISHMENTS.BANISH());
                    AIManager.dead = CAUSE_LEAVES.EXILED();
                    return AI.SUBS().LAY.activateTime(a, d, 10);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    if (punishment(a, d) != CRIME_PUNISHMENTS.BANISH())
                    {
                        RoomInstance ins = SETT.ROOMS().STOCKADE.registerPrisoner(a.tc());
                        if (ins != null)
                        {
                            SETT.ROOMS().STOCKADE.unregisterPrisoner(ins.mX(), ins.mY());
                            return false;
                        }
                    }

                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    // TODO Auto-generated method stub
                }

                public override bool event(Humanoid a, AIManager d, HEventData e)
                {
                    return base.event(a, d, e);
                }

                public override double poll(Humanoid a, AIManager d, HPollData e)
                {
                    return base.poll(a, d, e);
                }
            };

            protected override void cancel(Humanoid a, AIManager d)
            {
                base.cancel(a, d);
            }
        };

        private readonly AIPLAN free = new AIPLAN.PLANRES("prisFree")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                return start.set(a, d);
            }

            private readonly Resumer start = new Resumer(Dic.¤¤Free)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.activate(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    return free.set(a, d);
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

            private readonly Resumer free = new Resumer(Dic.¤¤Free)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    DATA().punish(a, d, CRIME_PUNISHMENTS.PARDON());
                    if (STATS.LAW().prisonerType.get(a.indu()).cl == HCLASSES.SLAVE())
                    {
                        a.HTypeSet(HTYPES.SLAVE(), null, CAUSE_ARRIVES.PAROLE());
                    }
                    else
                    {
                        a.HTypeSet(HTYPES.SUBJECT(), null, CAUSE_ARRIVES.PAROLE());
                    }
                    STATS.LAW().EX_CON.indu().setD(a.indu(), 1.0);
                    return null;
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
        };

        private readonly AIPLAN slave = new AIPLAN.PLANRES("prisSlave")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                return start.set(a, d);
            }

            private readonly Resumer start = new Resumer(Dic.¤¤Free)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.activate(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    DATA().punish(a, d, CRIME_PUNISHMENTS.ENSLAVE());
                    a.HTypeSet(HTYPES.SLAVE(), null, null);
                    STATS.LAW().EX_CON.indu().setD(a.indu(), 1.0);
                    return null;
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
        };
    }
}