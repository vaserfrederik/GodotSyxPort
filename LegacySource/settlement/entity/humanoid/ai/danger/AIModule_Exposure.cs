using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AIData;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.hygine.well;
using settlement.stats;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.danger
{
    public sealed class AIModule_Exposure : AIModule
    {
        private readonly AIDataSuspender suspender = AI.suspender("Exposure");

        private static readonly CharSequence ¤¤name = "Find Shelter";
        private static readonly CharSequence ¤¤desc = "Find Shelter or services to alleviate heat or cold.";
        private static readonly CharSequence ¤¤freezing = "Freezing!";
        private static readonly CharSequence ¤¤cover = "Cooling down";
        private static readonly CharSequence ¤¤nearDeath = "(Near Death!)";

        static AIModule_Exposure()
        {
            D.ts(typeof(AIModule_Exposure));
        }

        public AIModule_Exposure() : base(UI.icons().s.heat, ¤¤name, ¤¤desc)
        {
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            if (STATS.NEEDS().EXPOSURE.COUNT.indu().get(a.indu()) == 0)
                return null;

            if (!suspender.is(d))
            {
                if (!STATS.NEEDS().EXPOSURE.isCold(a.indu()))
                {
                    AiPlanActivation p = getHot(a, d);
                    if (p != null)
                        return p;
                }
                else
                {
                    AiPlanActivation p = getCold(a, d);
                    if (p != null)
                        return p;
                }
                suspender.suspend(d);
            }

            AiPlanActivation p = inside.activate(a, d);
            if (p != null)
                return p;

            return null;
        }

        public AiPlanActivation getHot(Humanoid a, AIManager d)
        {
            int dist = a.indu().hType() == HTYPES.DERANGED() ? int.MaxValue : 200;

            AiPlanActivation p = AI.modules().needs.get(a, d, NEEDS.TYPES().SKINNYDIP, dist);
            if (p != null)
                return p;

            foreach (ROOM_WELL w in SETT.ROOMS().WELLS)
            {
                p = AI.modules().needs.get(a, d, w.service().need, dist);
                if (p != null)
                    return p;
            }
            return null;
        }

        public AiPlanActivation getCold(Humanoid a, AIManager d)
        {
            int dist = a.indu().hType() == HTYPES.DERANGED() ? int.MaxValue : 200;
            AiPlanActivation p = AI.modules().needs.get(a, d, SETT.ROOMS().HEARTH.service().need, dist);
            if (p != null)
                return p;
            return null;
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
            suspender.update(d);
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            if (STATS.NEEDS().EXPOSURE.critical(a.indu()))
            {
                return 10;
            }

            if (!suspender.is(d) && STATS.NEEDS().EXPOSURE.COUNT.indu().get(a.indu()) > 0)
                return 8;

            return 0;
        }

        private readonly AIPLAN inside = new AIPLAN.PLANRES("dangerExposeure")
        {
            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                return find.set(a, d);
            }

            private readonly Resumer find = new Resumer
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    exit.set(a, d);
                    FINDABLE f = PATH().finders.indoor.getReservable(a.tc().x(), a.tc().y());
                    d.planByte2 = 0;
                    if (f != null)
                    {
                        d.planTile.set(a.tc());
                        f.findableReserve();
                        d.planByte2 = 1;
                        return AI.SUBS().STAND.activateRndDir(a, d);
                    }

                    AISubActivation s = AI.SUBS().walkTo.serviceInclude(a, d, PATH().finders.indoor, 64);
                    if (s != null)
                    {
                        d.planTile.set(d.path.destX(), d.path.destY());
                        d.planByte2 = 1;
                        return s;
                    }

                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    return exit.set(a, d);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    if (d.planByte2 == 1)
                    {
                        FINDABLE f = PATH().finders.indoor.getReserved(d.planTile.x(), d.planTile.y());
                        if (f != null)
                            f.findableReserveCancel();
                        d.planByte2 = 0;
                    }
                }
            };

            private readonly Resumer exit = new Resumer
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    d.planByte1 = (byte)(5 + RND.rInt(5));
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    d.planByte1--;

                    if (d.planByte1 > 0 && STATS.NEEDS().EXPOSURE.COUNT.indu().get(a.indu()) > 0 && moduleCanContinue(a, d))
                    {
                        if (STATS.NEEDS().EXPOSURE.critical(a.indu()))
                        {
                            return exit2.set(a, d);
                        }

                        if (RND.rBoolean())
                        {
                            return AI.SUBS().STAND.activate(a, d, AI.STATES().anima.wave.activate(a, d, 2 + RND.rFloat() * 2));
                        }
                        return AI.SUBS().STAND.activateRndDir(a, d);
                    }
                    can(a, d);
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    if (d.planByte2 == 1)
                    {
                        FINDABLE f = PATH().finders.indoor.getReserved(d.planTile.x(), d.planTile.y());
                        if (f != null)
                            f.findableReserveCancel();
                        d.planByte2 = 0;
                    }
                }
            };

            private readonly Resumer exit2 = new Resumer
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().LAY.activateTime(a, d, 8);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (STATS.NEEDS().EXPOSURE.COUNT.indu().isMax(a.indu()) && SETT.WEATHER().temp.getEntityTemp() != 0)
                    {
                        AIManager.dead = STATS.NEEDS().EXPOSURE.isCold(a.indu()) ? CAUSE_LEAVES.COLD() : CAUSE_LEAVES.HEAT();
                        return AI.SUBS().LAY.activateTime(a, d, 8);
                    }
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    if (d.planByte2 == 1)
                    {
                        FINDABLE f = PATH().finders.indoor.getReserved(d.planTile.x(), d.planTile.y());
                        if (f != null)
                            f.findableReserveCancel();
                        d.planByte2 = 0;
                    }
                }
            };

            protected override void name(Humanoid a, AIManager d, Str string)
            {
                if (STATS.NEEDS().EXPOSURE.isCold(a.indu()))
                {
                    string.add(¤¤freezing);
                }
                else
                    string.add(¤¤cover);
                if (STATS.NEEDS().EXPOSURE.critical(a.indu()))
                    string.s().add(¤¤nearDeath);
            }
        };
    }
}