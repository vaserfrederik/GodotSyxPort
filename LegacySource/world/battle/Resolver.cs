using System;
using System.Collections.Generic;

namespace World.Battle
{
    public sealed class Resolver
    {
        private readonly ResolverSide a;
        private readonly ResolverSide b;
        private readonly ResolverPlayer iplayer;
        private readonly RCount count = new RCount();

        public Resolver()
        {
            iplayer = new ResolverPlayer();
            a = new ResolverSide();
            b = new ResolverSide();
        }

        public void Init(Side A, Side B)
        {
            Init(A, 0, B, 0);

            ResolverSide winner = a;
            ResolverSide looser = b;
            if (b.powerBalance > a.powerBalance)
            {
                winner = b;
                looser = a;
            }

            if (looser.player)
            {
                if (looser.us[0].unit.a != null)
                {
                    COORDINATE ret = Util.RetTile(looser.us[0].unit.a);
                    if (ret == null)
                    {
                        looser.us[0].Count(count.Clear(), 1.0, true);
                    }
                    else
                    {
                        looser.retreatCoo.Set(ret);

                        looser.us[0].Count(count.Clear(), RetreatValue(looser), true);
                    }
                }
                looser.Count(count.Clear(), AutoValue(looser), false);
                winner.Count(count.Clear(), AutoValue(winner), false);
                iplayer.Battle(looser, winner);
                return;
            }

            if (looser.side.us[0].r != null)
            {
                return;
            }

            WArmy retreater = looser.side.us[0].a;

            double retValue = RetreatValue(looser);
            if (retValue < 1)
            {
                COORDINATE ret = Util.RetTile(retreater);

                if (ret != null)
                {
                    retreater.Teleport(ret.x, ret.y);
                    if (winner.player)
                    {
                        iplayer.EnemyWithdraws(winner, looser);
                        return;
                    }
                    looser.us[0].Extract(retValue);
                    BattleListener.Notify(winner, looser);
                    return;
                }
            }

            looser.Count(count.Clear(), AutoValue(looser), false);
            winner.Count(count.Clear(), AutoValue(winner), false);

            if (winner.player)
            {
                if (looser.us[0].unit.a != null)
                {
                    COORDINATE ret = Util.RetTile(looser.us[0].unit.a);
                    if (ret == null)
                    {
                        looser.us[0].Count(count.Clear(), 1.0, true);
                    }
                    else
                    {
                        looser.us[0].Count(count.Clear(), RetreatValue(looser), true);

                        looser.retreatCoo.Set(ret);
                    }
                }
                if (winner.us[0].unit.a != null)
                {
                    COORDINATE ret = Util.RetTile(winner.us[0].unit.a);
                    if (ret != null)
                    {
                        winner.retreatCoo.Set(ret);
                    }
                }
                iplayer.Battle(winner, looser);
                return;
            }

            BattleListener.Notify(winner, looser);

            looser.Extract(1.0);
            winner.Extract((1 - winner.powerBalance));
        }

        public static double RetreatValue(ResolverSide retreater)
        {
            double d = (1 - retreater.powerBalance);
            d = CLAMP.d(d, 0, 1);
            return d;
        }

        public static double AutoValue(ResolverSide side)
        {
            if (side.powerBalance < 0.5)
                return 1.0;
            return CLAMP.d(1.0 - side.powerBalance, 0, 1);
        }

        public bool Besiege(Side besieger, Side besieged, bool first)
        {
            Region reg = besieged.us[0].r;
            double extra = RD.MILITARY().power.GetD(reg) * RD.MILITARY().fort.GetD(reg);

            Init(besieger, 0, besieged, extra);
            b.us[0].defences = extra;

            if (a.player)
            {
                if (b.powerBalance > a.powerBalance)
                {
                    a.Count(count, 1.0, false);
                    b.Count(count, 1 - b.powerBalance, false);
                }
                else
                {
                    b.Count(count, 1.0, false);
                    a.Count(count, 1 - a.powerBalance, false);
                }
                iplayer.Besiege(a, b);
                return true;
            }
            if (b.player && first)
            {
                b.Count(count, 1 - b.powerBalance, false);
                //b.count(count, 0, true);
                a.Count(count, 1 - a.powerBalance, false);
                if (b.Men() > 0)
                {
                    iplayer.SallyOut(b, a);
                    return true;
                }
            }

            if (b.powerBalance > a.powerBalance)
            {
                return false;
            }

            if (b.player && reg.Capitol())
            {
                iplayer.InvadeCapitol(a);
                return true;
            }

            a.us[0].Extract((1 - a.powerBalance));
            b.us[0].Extract(1.0);
            Util.Conquer(a.side, RND.rFloat(), RND.rFloat(), reg, besieger.us[0].faction);
            return true;
        }

        private void Init(Side A, double powA, Side B, double powB)
        {
            if (A.us.Count == 0)
                throw new RuntimeException();

            if (B.us.Count == 0)
                throw new RuntimeException();

            if (!Util.Enemies(A.us[0].faction, B.us[0].faction))
                throw new RuntimeException();

            foreach (SideUnit u in A.us)
            {
                powA += u.Power();
            }

            foreach (SideUnit u in B.us)
            {
                powB += u.Power();
            }

            double pI = 1.0 / (powA + powB);

            a.Init(A, powA * pI);
            b.Init(B, powB * pI);
        }
    }
}