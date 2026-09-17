using System;
using System.Collections.Generic;
using game.audio;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.home;
using settlement.stats;
using snake2d.util.datatypes;
using util.text;

namespace settlement.entity.humanoid.ai.danger
{
    class PlanSick : AIPLAN.PLANRES
    {
        private static readonly CharSequence ¤¤name = "Being sick";
        private readonly SubPlanSeekHospital ho = new SubPlanSeekHospital(this);

        public readonly SoundRace sound = AUDIO.race("SICK_MOAN");

        static PlanSick()
        {
            D.ts(typeof(PlanSick));
        }

        public PlanSick(string key) : base(key)
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            if (STATS.DISEASE().shouldHospital(a))
            {
                AISubActivation s = ho.init(a, d);
                if (s != null)
                    return s;
            }
            if (a.indu().hType().isWorks())
                STATS.WORK().EMPLOYED.set(a, null);
            return res.set(a, d);
        }

        private readonly Resumer res = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if ((STATS.RAN().get(a.indu(), 0) & 0x0FF) > 200)
                {
                    HOME h = STATS.HOME().GETTER.get(a, this);
                    if (h != null)
                    {
                        if (!h.is(a.tc().x(), a.tc().y()))
                        {
                            int sx = h.serviceX();
                            int sy = h.serviceY();
                            return AI.SUBS().walkTo.cooFull(a, d, sx, sy);
                        }
                        else if (SETT.ENTITIES().hasAtTileHigher(a, a.tc().x(), a.tc().y()))
                        {
                            foreach (DIR dir in DIR.ORTHO)
                            {
                                int dx = a.tc().x() + dir.x();
                                int dy = a.tc().y() + dir.y();
                                if (h.is(dx, dy) && !SETT.PATH().solidity.is(dx, dy) && !SETT.ENTITIES().hasAtTileHigher(a, dx, dy))
                                {
                                    return AI.SUBS().walkTo.cooFull(a, d, dx, dy);
                                }
                            }
                        }
                    }
                }

                sound.rnd(a);

                return AI.SUBS().LAY.activateTime(a, d, 60);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (STATS.DISEASE().diseaseIsDone(a, 0))
                {
                    if (STATS.DISEASE().shouldDie(a))
                    {
                        AIManager.dead = CAUSE_LEAVES.DISEASE();
                        AIManager.deadGore = false;
                        return AI.SUBS().STAND.activate(a, d);
                    }
                    STATS.DISEASE().cure(a.indu(), false);
                    return null;
                }
                if (STATS.DISEASE().shouldHospital(a))
                {
                    AISubActivation s = ho.init(a, d);
                    if (s != null)
                        return s;
                }

                return set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };
    }
}