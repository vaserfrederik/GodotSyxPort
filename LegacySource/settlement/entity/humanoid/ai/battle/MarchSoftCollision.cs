using System;
using settlement.entity.humanoid.ai.battle;
using game;
using game.boosting;
using game.time;
using init.constant;
using settlement.entity;
using settlement.entity.humanoid.HEvent;
using settlement.entity.humanoid.HPoll;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.spirte;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.battle
{
    public sealed class MarchSoftCollision : AISUB.Resumable
    {
        public AISubActivation InitReady(AIManager d, Humanoid a, ENTITY other, double norX, double norY, double faceDot, double momentum)
        {
            d.subPathByte = 5;
            if (other is Humanoid)
            {
                Humanoid o = (Humanoid)other;
                if (o.Indu().Hostile() != a.Indu().Hostile())
                {
                    d.OtherEntitySet(o);
                }
            }
            return Activate(a, d, Gg(a, d));
        }

        public AISubActivation InitCoo(AIManager d, Humanoid a, ENTITY other, int px, int py)
        {
            d.subPathByte = 5;
            if (other is Humanoid)
            {
                Humanoid o = (Humanoid)other;
                if (o.Indu().Hostile() != a.Indu().Hostile())
                {
                    d.OtherEntitySet(o);
                }
            }
            return Activate(a, d, Gg(a, d));
        }

        protected MarchSoftCollision() : base("MarchSoftColl") { }

        protected override AISTATE Init(Humanoid a, AIManager d)
        {
            return null;
        }

        private Resumer Gg(Humanoid a, AIManager d)
        {
            if (a.Division() != null)
                a.Speed.SetDirCurrent(a.Division().Dir());
            if (BattleUtil.ShouldMoveIntoDivPosition(a, d))
            {
                double m = COORDINATE.TileDistance(a.Body().CX(), a.Body().CY(), a.Division().Reporter.GetPixel(a));
                if (m > C.TILE_SIZEH)
                {
                    return Push;
                }
            }
            return Brakes;
        }

        private readonly Resumer Push = new ResumerB
        {
            protected override AISTATE SetAction(Humanoid a, AIManager d)
            {
                d.subPathByte--;
                return AI.STATES().PUSH_TO.Move(a, d, a.Division().Reporter.GetPixel(a).X, a.Division().Reporter.GetPixel(a).Y, 1.0, 0.75);
            },
            protected override AISTATE Res(Humanoid a, AIManager d)
            {
                if (d.subPathByte < 0 && d.subByte >= 0)
                {
                    AIPLAN p = AI.Modules().Battle.Escape.Plan(a, d);
                    if (p != null)
                        return d.ResumeOtherPlanState(a, p);
                }
                return Strike.Set(a, d);
            }
        };

        private readonly Resumer Brakes = new ResumerB
        {
            protected override AISTATE SetAction(Humanoid a, AIManager d)
            {
                d.subByte = 0;
                if (a.Division() != null)
                    a.Speed.SetDirCurrent(a.Division().Dir());
                return AI.STATES().STAND_SWORD.Activate(a, d, RND.rFloat0(0.5f));
            },
            protected override AISTATE Res(Humanoid a, AIManager d)
            {
                return Strike.Set(a, d);
            }
        };

        private readonly Resumer Strike = new ResumerB
        {
            private readonly AISTATE State = AI.STATES().SWORD.Strike;
            private readonly float Time = (float)HSprites.SWORD_OUT.Time;

            public override AISTATE SetAction(Humanoid a, AIManager d)
            {
                a.SpriteTimer = 0;
                d.StateTimer = Time;
                a.Speed.MagnitudeTargetSet(0);
                return State;
            }

            public override AISTATE Res(Humanoid a, AIManager d)
            {
                return Strike2.Set(a, d);
            }
        };

        private readonly Resumer Strike2 = new ResumerB
        {
            private readonly AISTATE State = AI.STATES().SWORD.StrikeIn;
            private readonly float Time = (float)HSprites.SWORD_IN.Time;

            public override AISTATE SetAction(Humanoid a, AIManager d)
            {
                if (d.OtherEntity() != null)
                {
                    int dist = a.Body().GetDistance(d.OtherEntity().Body());
                    if (dist <= (a.Body().Width() + d.OtherEntity().Body().Width()) / 2 + 24)
                    {
                        Humanoid enemy = d.OtherEntity();
                        GAME.Battle().Fight.Attack(a, enemy);
                    }
                }

                a.SpriteTimer = 0;
                d.StateTimer = Time;
                a.Speed.MagnitudeTargetSet(0);
                return State;
            }

            public override AISTATE Res(Humanoid a, AIManager d)
            {
                return null;
            }
        };

        private abstract class ResumerB : Resumer
        {
            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                switch (e.Event)
                {
                    case CollisionType.COLLISION_SOFT:
                        a.Speed.SetPrevDir();
                        Resumer ss = Gg(a, d);
                        if (GetResumer(a, d) != Push && ss != Push)
                            d.Overwrite(a, ss.Set(a, d));
                        else if (a.Division() != null)
                            a.Speed.SetDirCurrent(a.Division().Dir());
                        else
                            a.Speed.SetPrevDir();
                        return true;
                    case EventType.EXHAUST:
                        if (RND.OneIn(BOOSTABLES.PHYSICS().STAMINA.Get(a.Indu()) * 8))
                            STATS.NEEDS().EXHASTION.Indu().Inc(a.Indu(), 1);
                        return false;
                    case EventType.MEET_ENEMY:
                        a.Speed.SetPrevDir();
                        d.OtherEntitySet((Humanoid)e.Other);
                        ss = Gg(a, d);
                        if (GetResumer(a, d) != Push && ss != Push)
                            d.Overwrite(a, ss.Set(a, d));
                        else if (a.Division() != null)
                            a.Speed.SetDirCurrent(a.Division().Dir());
                        else
                            a.Speed.SetPrevDir();
                        return false;
                    case EventType.MEET_HARMLESS:
                        return false;
                    default:
                        return InterBattle.Listener.Event(a, d, e);
                }
            }

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {
                switch (e.Type)
                {
                    case PollType.COLLIDES:
                        return 1;
                    case PollType.WILL_COLLIDE_WITH:
                        if (e.Other is Humanoid && ((Humanoid)e.Other).Indu().Hostile() != a.Indu().Hostile())
                            return 1;
                        if (TIME.CurrentSecond() - d.LastCollision < 3)
                            return 1;
                        return 0;
                    default:
                        return InterBattle.Listener.Poll(a, d, e);
                }
            }
        }
    }
}