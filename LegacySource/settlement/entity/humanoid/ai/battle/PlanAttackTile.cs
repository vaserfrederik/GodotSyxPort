using System;
using System.Text;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.battle;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.battle
{
    internal class PlanAttackTile : AIPLAN.PLANRES
    {
        public PlanAttackTile(string key) : base(key)
        {
            // TODO Auto-generated constructor stub
        }

        private static readonly CharSequence ¤¤name = "¤attacking terrain";
        static PlanAttackTile()
        {
            D.ts(typeof(PlanAttackTile));
        }

        private static int tx, ty;

        public AIPLAN Init(AIManager d, Humanoid a, int tx, int ty)
        {
            PlanAttackTile.tx = tx;
            PlanAttackTile.ty = ty;
            return this;
        }

        public bool ShouldAttackTile(AIManager d, Humanoid a, int tx, int ty)
        {
            if (!GAME.ARMIES().Map.AttackableI.Is(tx, ty, a.Indu()))
                return false;

            if (a.Division() == null)
                return true;
            if (!a.Division().Reporter.PosHas(a))
                return true;
            if (!a.Division().Settings().Mustering() || a.Division().Settings().MoppingUp())
                return true;
            if (COORDINATE.TileDistance(a.Division().Reporter.GetTile(a), tx, ty) < 2)
                return true;
            return false;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            d.planByte1 = 0;
            d.planTile.Set(tx, ty);
            return stop.Set(a, d);
        }

        private readonly Resumer stop = new Resumer(¤¤name)
        {
            SetAction = (Humanoid a, AIManager d) =>
            {
                return AI.SUBS.STAND.ActivateTime(a, d, 0);
            },
            Res = (Humanoid a, AIManager d) =>
            {
                return moveToEdge.Set(a, d);
            },
            Con = (Humanoid a, AIManager d) =>
            {
                return true;
            },
            Can = (Humanoid a, AIManager d) =>
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer moveToEdge = new Resumer(¤¤name)
        {
            SetAction = (Humanoid a, AIManager d) =>
            {
                if (!ShouldAttackTile(d, a, d.planTile.X, d.planTile.Y))
                    return null;
                int x = (d.planTile.X << C.T_SCROLL) + C.TILE_SIZEH;
                int y = (d.planTile.Y << C.T_SCROLL) + C.TILE_SIZEH;
                DIR dir = DIR.Get(a.Body().CX(), a.Body().CY(), x, y);
                x = (a.Tc().X << C.T_SCROLL) + C.TILE_SIZEH;
                y = (a.Tc().Y << C.T_SCROLL) + C.TILE_SIZEH;

                int dist = (C.TILE_SIZE - a.Body().Width() - 1) / 2;

                x += dir.X() * dist;
                y += dir.Y() * dist;

                AISTATE s = AI.STATES.WALK2_SWORD.Free(a, d, x, y);
                a.Speed.SetDirCurrent(dir);
                return AI.SUBS.single.Activate(a, d, s);
            },
            Res = (Humanoid a, AIManager d) =>
            {
                return wait.Set(a, d);
            },
            Con = (Humanoid a, AIManager d) =>
            {
                return true;
            },
            Can = (Humanoid a, AIManager d) =>
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer wait = new Resumer(¤¤name)
        {
            Res = (Humanoid a, AIManager d) =>
            {
                return attack.Set(a, d);
            },
            SetAction = (Humanoid a, AIManager d) =>
            {
                int tx = (d.planTile.X << C.T_SCROLL) + C.TILE_SIZEH;
                int ty = (d.planTile.Y << C.T_SCROLL) + C.TILE_SIZEH;
                DIR dir = DIR.Get(a.Body().CX(), a.Body().CY(), tx, ty);
                a.Speed.SetDirCurrent(dir);
                a.Speed.MagnitudeInit(0);
                a.Speed.MagnitudeTargetSet(0);
                return AI.SUBS.single.Activate(a, d, AI.STATES.anima.sword.Activate(a, d, 2 + RND.rFloat() * 2));
            },
            Con = (Humanoid a, AIManager d) =>
            {
                return true;
            },
            Can = (Humanoid a, AIManager d) =>
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer attack = new Resumer(¤¤name)
        {
            Res = (Humanoid a, AIManager d) =>
            {
                if (!ShouldAttackTile(d, a, d.planTile.X, d.planTile.Y))
                    return null;

                double mom = C.TILE_SIZE * BOOSTABLES.BATTLE().BLUNT_ATTACK.Get(a.Indu());

                double str = GAME.ARMIES().Map.Strength.Get(d.planTile);

                while (mom > 0)
                {
                    if (mom > RND.rFloat() * str)
                        d.planByte1++;
                    mom -= str;
                }

                if (d.planByte1 >= 4)
                {
                    GAME.ARMIES().Map.BreakIt(d.planTile.X, d.planTile.Y);
                    return null;
                }

                return wait.Set(a, d);
            },
            SetAction = (Humanoid a, AIManager d) =>
            {
                return AI.SUBS.single.Activate(a, d, AI.STATES.anima.stab.Activate(a, d));
            },
            Con = (Humanoid a, AIManager d) =>
            {
                return true;
            },
            Can = (Humanoid a, AIManager d) =>
            {
                // TODO Auto-generated method stub
            }
        };

        public override double Poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.Type == HPoll.COLLIDES)
                return 1;
            if (e.Type == HPoll.WILL_COLLIDE_WITH)
                return 0;
            return InterBattle.listener.Poll(a, d, e);
        }

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            switch (e.Event)
            {
                case COLLISION_TILE:
                    if (ShouldAttackTile(d, a, e.Tx, e.Ty))
                    {
                        if (!d.planTile.IsSameAs(e.Tx, e.Ty))
                        {
                            d.planTile.Set(e.Tx, e.Ty);
                            d.planByte1 = 0;
                        }
                        d.Overwrite(a, stop.Set(a, d));
                        a.Speed.SetPrevDir();
                    }
                    return false;
                case EXHAUST:
                    if (RND.OneIn(BOOSTABLES.PHYSICS().STAMINA.Get(a.Indu()) * 8))
                    {
                        STATS.NEEDS().EXHASTION.Indu().Inc(a.Indu(), 1);
                    }
                    return false;
                case COLLISION_SOFT:
                    {
                        if (ShouldAttackTile(d, a, e.Tx, e.Ty))
                        {
                            d.Overwrite(a, stop.Set(a, d));
                            a.Speed.SetPrevDir();
                        }
                    }
                    break;
                default:
                    return InterBattle.listener.Event(a, d, e);
            }
            return false;
        }
    }
}