using System;
using System.Collections.Generic;

namespace Settlement.Entity.Humanoid.AI.Service
{
    public sealed class S_PlanShrine : S_Plan
    {
        public S_PlanShrine() : base(STATS.RELIGION().SHRINE, 1.0f)
        {
        }

        public override bool HasAccess(Humanoid a, AIManager d)
        {
            return STATS.RELIGION().SHRINE.ACCESS.indu().Get(a.indu()) > 0;
        }

        public override bool Allowed(Humanoid a, AIManager d)
        {
            return STATS.RELIGION().getter.Get(a.indu()).permissionShrine.Has(a);
        }

        public override bool GoodTime(Humanoid a, AIManager d)
        {
            return true;
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            AiPlanActivation p = plan.Activate(a, d);
            if (p == null)
            {
                STATS.RELIGION().SHRINE.ClearAccess(a);
            }
            return p;
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d, int dist)
        {
            return GetPlan(a, d);
        }

        public List<ROOM_SHRINE> Services(Humanoid a, AIManager d)
        {
            return SETT.ROOMS().TEMPLES.shrines(STATS.RELIGION().getter.Get(a.indu()).religion);
        }

        private readonly AIPLAN plan = new AIPLAN.PLANRES("serShrine")
        {
            protected override AISubActivation Init(Humanoid a, AIManager d)
            {
                return walk.Set(a, d);
            }

            private readonly Resumer walk = new Resumer(null)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    d.planByte4 = 0;
                    foreach (ROOM_SHRINE t in Services(a, d))
                    {
                        AISubActivation s = AI.SUBS().walkTo.serviceInclude(a, d, t.service().finder, t.service().radius);
                        if (s != null)
                        {
                            d.planByte4 = (byte)t.typeIndex();
                            return s;
                        }
                    }
                    return null;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    STATS.RELIGION().SHRINE.SetAccess(a);
                    return pray.Set(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }

                protected override void Name(Humanoid a, AIManager d, Str str)
                {
                    str.Add(Temp(d).service().verb);
                }
            };

            private readonly Resumer pray = new Resumer(null)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    d.planTile.Set(d.path.destX(), d.path.destY());
                    d.planByte1 = (byte)(5 + RND.rInt(10));

                    FSERVICE s = Temp(d).service(d.planTile.x(), d.planTile.y()).Get(d.planTile.x(), d.planTile.y());
                    if (s != null)
                        s.StartUsing();

                    FurnisherItem it = SETT.ROOMS().fData.item.Get(a.tc());
                    if (it != null)
                    {
                        COORDINATE c = SETT.ROOMS().fData.itemX1Y1(a.tc(), Coo.TMP);
                        if (c != null)
                        {
                            int dx = c.x() + it.width() / 2;
                            int dy = c.y() + it.height() / 2;

                            DIR dir = DIR.Get(a.tc().x(), a.tc().y(), dx, dy);
                            a.speed.SetDirCurrent(dir);
                        }
                    }

                    return Res(a, d);
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    d.planByte1--;
                    if (d.planByte1 <= 0)
                    {
                        Can(a, d);
                        return null;
                    }
                    if (RND.rBoolean())
                    {
                        return AI.SUBS().single.Activate(a, d, AI.STATES().anima.lay, 4 + RND.rInt(4));
                    }
                    else
                    {
                        if (RND.rBoolean())
                            return AI.SUBS().single.Activate(a, d, AI.STATES().anima.carry, 4 + RND.rInt(4));
                        return AI.SUBS().single.Activate(a, d, AI.STATES().anima.stand, 4 + RND.rInt(4));
                    }
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                    FINDABLE s = Temp(d).service().finder.GetReserved(d.planTile.x(), d.planTile.y());
                    if (s != null)
                        s.findableReserveCancel();
                }

                protected override void Name(Humanoid a, AIManager d, Str str)
                {
                    str.Add(Temp(d).service().verb);
                }
            };

            private ROOM_SHRINE Temp(AIManager d)
            {
                return SETT.ROOMS().TEMPLES.SHRINES.Get(d.planByte4);
            }
        };
    }
}