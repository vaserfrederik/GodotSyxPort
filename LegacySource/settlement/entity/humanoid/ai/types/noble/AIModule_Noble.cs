using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.types.noble;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;
using game.nobility;
using init.sprite.UI;
using settlement.entity.humanoid.HPoll;

namespace settlement.entity.humanoid.ai.types.noble
{
    public sealed class AIModule_Noble : AIModule
    {
        private static readonly CharSequence ¤¤name = "Be Noble";
        private static readonly CharSequence ¤¤verb = "Inspecting";

        static
        {
            D.ts(typeof(AIModule_Noble));
        }

        private readonly AIData.AIDataBit sus = AI.bit("noble");

        public AIModule_Noble()
            : base(UI.icons().s.noble, ¤¤name, null)
        {
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            return beNoble.Activate(a, d);
        }

        protected override void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int upI)
        {
            if (newDay)
                sus.Set(d, false);
        }

        public override int GetPriority(Humanoid a, AIManager d)
        {
            if (STATS.WORK().WORK_TIME.indu().IsMax(a.indu()))
                return 0;
            return 3;
        }

        private readonly AIPLAN beNoble = new AIPLAN.PLANRES("noble")
        {
            protected override AISubActivation Init(Humanoid a, AIManager d)
            {
                NobleOffice o = a.noble().office();
                if (o != null && o.room() != null && !sus.Is(d))
                {
                    RoomBlueprintIns b = o.room();
                    if (b.instancesSize() > 0)
                    {
                        int ii = RND.rInt(b.instancesSize());
                        for (int k = 0; k < b.instancesSize(); k++)
                        {
                            RoomInstance ins = b.getInstance(((ii + k) % b.instancesSize()));
                            if (ins.employees().employed() > 0)
                            {
                                AISubActivation s = AI.SUBS().walkTo.room(a, d, ins);
                                if (s != null)
                                {
                                    inspectRoom.Set(a, d);
                                    return s;
                                }
                            }
                        }
                        sus.Set(d, true);
                    }
                }
                return other.Set(a, d);
            }

            private readonly Resumer other = new Resumer(¤¤verb)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    inspect.Set(a, d);
                    COORDINATE c = SETT.PATH().finders.randomDistanceAway.Get(THRONE.coo().x(), THRONE.coo().y(), 100, SFinderRND.value);
                    if (c != null)
                        return AI.SUBS().walkTo.coo(a, d, c);

                    return AI.SUBS().STAND.activate(a, d);
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                    // TODO Auto-generated method stub
                }
            };

            private readonly Resumer inspect = new Resumer(¤¤verb)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    d.planByte1 = 8;
                    return null;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    d.planByte1--;
                    if (d.planByte1 <= 0)
                        return null;
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }
            };

            private readonly Resumer inspectRoom = new Resumer(¤¤verb)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    d.planByte1 = 16;
                    return null;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    d.planByte1--;
                    if (d.planByte1 <= 0)
                        return null;
                    if (RND.oneIn(4))
                    {
                        RoomInstance r = SETT.ROOMS().map.instance.Get(a.tc());
                        if (r != null)
                        {
                            return AI.SUBS().walkTo.room(a, d, r);
                        }
                    }
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }
            };

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.type == HPoll.WORKING)
                {
                    return 1.0;
                }
                return base.Poll(a, d, e);
            }
        };
    }
}