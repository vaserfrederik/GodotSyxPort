using System;
using System.Collections.Generic;
using settlement.entity.humanoid.ai.util;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.battle;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.service.arena;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.util
{
    public abstract class AIPlanGladiator : AIPLAN.PLANRES
    {
        private readonly ICharSequence ¤¤name;
        private readonly double inj = 0.3;
        private readonly bool toDeath;

        public AIPlanGladiator(string key, bool toDeath, ICharSequence verb) : base(key)
        {
            ¤¤name = verb;
            this.toDeath = toDeath;
        }

        protected override void name(Humanoid a, AIManager d, Str string)
        {
            string.Add(¤¤name);
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            if (!w(a, d).GladiatorInArena(d.PlanTile.X(), d.PlanTile.Y()))
                throw new Exception();
            return walk.Set(a, d);
        }

        protected abstract RoomArenaWork W(Humanoid a, AIManager d);

        private readonly SubFight fightSub = new SubFight(key + "Fighting")
        {
            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                if (!ShouldFight(a, d))
                    return null;
                if (!IsFighter(d.OtherEntity()))
                    return null;
                return base.Resume(a, d);
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (IsFighter(e.Other))
                {
                    if (e.Event == HEvent.MEET_HARMLESS || e.Event == HEvent.COLLISION_SOFT)
                    {
                        e.Event = HEvent.MEET_ENEMY;
                    }
                }

                return base.Event(a, d, e);
            }

            public override void Attack(Humanoid a, AIManager d, Humanoid enemy)
            {
                AI.Modules().Battle.SoundSword.Rnd(a);
            }
        };

        private bool ShouldFight(Humanoid a, AIManager d)
        {
            if (!w(a, d).GladiatorInArena(a.TC().X(), a.TC().Y()))
                return false;
            if (!toDeath && STATS.NEEDS().INJURIES.COUNT.Indu().GetD(a.Indu()) >= inj)
                return false;
            if (!toDeath && STATS.WORK().WORK_TIME.Indu().IsMax(a.Indu()))
                return false;

            return true;
        }

        private readonly Resumer walk = new Resumer()
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.Subs().WalkTo.CooFull(a, d, d.PlanTile);
                if (s != null)
                    return s;
                Cancel(a, d);
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                if (!toDeath)
                    return taunt.Set(a, d);
                else
                    return ready.Set(a, d);
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

        private readonly Resumer taunt = new Resumer()
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                a.Speed.ImpulseBreak(1.0);
                w(a, d).GladiatorDrawMakeSheer(d.PlanTile);
                d.PlanByte1 = 4;
                a.Speed.SetDirCurrent(DIR.ALL.Rnd());
                return AI.Subs().Single.Activate(a, d, AI.STATES().Anima.ArmsOut, 3 + RND.Rnd(3));
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.PlanByte1 -= 1;
                if (d.PlanByte1 <= 0)
                {
                    return null;
                }
                a.Speed.SetDirCurrent(DIR.ALL.Rnd());
                return AI.Subs().Single.Activate(a, d, AI.STATES().Anima.ArmsOut, 3 + RND.Rnd(3));
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

        private readonly Resumer ready = new Resumer()
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                // Implementation for ready state
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                // Implementation for ready state
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

        private readonly Resumer removed = new Resumer()
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return AI.Subs().Stand.Activate(a, d);
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

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.Event == HEvent.ROOM_REMOVED && SETT.ROOMS().Map.Get(d.PlanTile) == e.Room)
            {
                d.PlanTile.Set(-1, -1);
                d.Overwrite(a, removed.Set(a, d));
                return true;
            }
            return base.Event(a, d, e);
        }

        private abstract class ResFigher : Resumer
        {
            protected ResFigher() : base(¤¤name)
            {
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.Type == HPoll.WILL_COLLIDE_WITH)
                    return IsFighter(e.Other) ? 1 : 0;

                return base.Poll(a, d, e);
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.Event == HEvent.COLLISION_HARD || e.Event == HEvent.MEET_HARMLESS || e.Event == HEvent.COLLISION_SOFT)
                {
                    if (IsFighter(e.Other))
                    {
                        Humanoid oo = (Humanoid)e.Other;
                        if (ShouldFight(oo, d) && RND.OneIn(10))
                        {
                            AIManager d2 = (AIManager)oo.AI();
                            double am = 0.1 + RND.Rnd(inj - 0.1);
                            CAUSE_LEAVE ll = CAUSE_LEAVES.SLAYED();
                            if (((AIPlanGladiator)d2.Plan()).toDeath)
                            {
                                ll = CAUSE_LEAVES.EXECUTED();
                            }
                            else
                            {
                                double inj = STATS.NEEDS().INJURIES.COUNT.Indu().GetD(oo.Indu());
                                am = Math.Min(am, AIPlanGladiator.this.inj - inj);
                                if (am < 0)
                                    return false;
                            }

                            if (!oo.InflictDamage(0.1, ll))
                                return true;

                        }

                        if (d.Plansub() == fightSub)
                        {
                            fightSub.Event(a, d, e);
                        }
                        else if (ShouldFight(a, d))
                        {
                            d.OtherEntitySet((Humanoid)e.Other);
                            d.Overwrite(a, fight.Set(a, d));
                        }
                        if (toDeath && RND.OneIn(5))
                        {
                            if (!a.InflictDamage(RND.Rnd(), CAUSE_LEAVES.EXECUTED()))
                                return false;

                        }
                        return true;
                    }
                }
                return base.Event(a, d, e);
            }
        }
    }
}