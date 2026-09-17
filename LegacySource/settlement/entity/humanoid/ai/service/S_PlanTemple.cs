using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.spirit.temple;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.service
{
    internal sealed class S_PlanTemple : S_Plan
    {
        public S_PlanTemple() : base(STATS.RELIGION().TEMPLE, 0.125)
        {
            // TODO Auto-generated constructor stub
        }

        public override bool hasAccess(Humanoid a, AIManager d)
        {
            return STATS.RELIGION().TEMPLE.ACCESS.indu().Get(a.indu()) > 0;
        }

        public override bool allowed(Humanoid a, AIManager d)
        {
            return STATS.RELIGION().getter.Get(a.indu()).permissionTemple.Has(a);
        }

        public override bool goodTime(Humanoid a, AIManager d)
        {
            return true;
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            AiPlanActivation pp = plan.Activate(a, d);
            if (pp == null)
                STATS.RELIGION().TEMPLE.ClearAccess(a);
            return pp;
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d, int dist)
        {
            return GetPlan(a, d);
        }

        public LIST<ROOM_TEMPLE> Services(Humanoid a, AIManager d)
        {
            return SETT.ROOMS().TEMPLES.Temple(STATS.RELIGION().getter.Get(a.indu()).religion);
        }

        private readonly Bitmap1D aa = new Bitmap1D(100000, false);

        private readonly AIPLAN plan = new AIPLAN.PLANRES("ser_temple")
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
                    foreach (ROOM_TEMPLE t in services(a, d))
                    {
                        AISubActivation s = AI.SUBS().walkTo.ServiceInclude(a, d, t.service().finder, t.service().radius);
                        if (s != null)
                        {
                            d.planByte4 = (byte)t.typeIndex();
                            aa.Set(a.id(), true);
                            return s;
                        }
                    }
                    return null;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    STATS.RELIGION().TEMPLE.SetAccess(a);
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
                    if (d.planByte1 <= 0 || Temp(d).service().available() < Temp(d).service().total() / 4)
                    {
                        FINDABLE s = Temp(d).service().finder.GetReserved(d.planTile.x(), d.planTile.y());
                        if (s != null)
                            s.findableReserveCancel();
                        return next.Set(a, d);
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

            private readonly Resumer next = new Resumer(null)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    Room r = SETT.ROOMS().map.Get(a.tc());
                    d.planByte1 = (byte)(5 + RND.rInt(10));
                    if (r != null)
                        return AI.SUBS().walkTo.Room(a, d, a.tc().x(), a.tc().y());
                    return null;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    d.planByte1--;
                    if (d.planByte1 <= 0)
                    {
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
                }

                protected override void Name(Humanoid a, AIManager d, Str str)
                {
                    str.Add(Temp(d).service().verb);
                }
            };

            private ROOM_TEMPLE Temp(AIManager d)
            {
                return SETT.ROOMS().TEMPLES.ALL.Get(d.planByte4);
            }
        };
    }
}