using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class ResFree
    {
        public static Resumer make(AIPLAN.PLANRES res)
        {
            return res.new Resumer(Dic.¤¤Free)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    HTYPE t = STATS.LAW().prisonerType.get(a.indu()).cl == HCLASSES.SLAVE() ? HTYPES.SLAVE() : HTYPES.SUBJECT();
                    a.HTypeSet(t, null, CAUSE_ARRIVES.PAROLE());
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
        }
    }
}