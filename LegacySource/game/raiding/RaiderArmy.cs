using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace game.raiding
{
    [Serializable]
    public class RaiderArmy
    {
        private static readonly long serialVersionUID = 1L;
        public readonly int Men;
        public readonly List<RaidDiv> SDivs = new List<RaidDiv>();
        public int[] Artillery;
        public readonly int Power;

        public RaiderArmy(Race race, double totPower, double quality)
        {
            int menTot = 0;
            int divMenTarget = 1;

            double powChunk = totPower / 8;

            {
                RaidDiv stat = new RaidDiv();
                double training = Math.Clamp(quality / 2 + RND.rFloat() * quality, 0, 1);
                double equip = Math.Clamp(quality / 2 + RND.rFloat() * quality, 0, 1);

                stat.Race = race.Index();
                stat.Men = 1;
                stat.Ex = Math.Clamp(RND.rFloat() * quality, 0, 1);
                stat.Name = stat.Race().Info.ArmyNames.Rnd().ToString();
                stat.BannerI = RND.rInt(GAME.ARMIES().Banners.Size());
                stat.CopySettings(GAME.Battle().Types.Rnd(stat.Race(), FACTIONS.Player(), RND.rFloat()), 1, training, equip);

                double p = GAME.Battle().Power.Get(stat);

                double men = totPower / p;
                men /= 2;
                men /= 5;
                men = 5 * (int)Math.Ceiling(men);
                men = Math.Clamp(men, 5, Config.Battle().MenPerDivision);
                divMenTarget = (int)men;

                stat.Men = (int)men;
                totPower -= GAME.Battle().Power.Get(stat);
                SDivs.Add(stat);
            }

            double raceMax = 0;
            foreach (Race r in RACES.All())
            {
                raceMax += Race(race, r, totPower);
            }

            while (totPower > 0 && SDivs.Count < Config.Battle().DivisionsPerArmy)
            {
                RaidDiv stat = new RaidDiv();
                double training = Math.Clamp(quality * 0.25 + RND.rFloat() * quality * 0.75, 0, 1);
                double equip = Math.Clamp(quality * 0.25 + RND.rFloat() * quality * 0.75, 0, 1);

                stat.Race = Race(race, raceMax, totPower).Index;
                stat.Men = 1;
                stat.Ex = Math.Clamp(RND.rFloat() * quality, 0, 1);
                stat.Name = stat.Race().Info.ArmyNames.Rnd().ToString();
                stat.BannerI = RND.rInt(GAME.ARMIES().Banners.Size());
                stat.CopySettings(GAME.Battle().Types.Rnd(stat.Race(), FACTIONS.Player(), RND.rFloat()), 1, training, equip);

                double p = GAME.Battle().Power.Get(stat);
                double dd = totPower / p;
                if (dd > 1)
                {
                    stat.Men *= dd;
                    stat.Men = Math.Clamp(stat.Men, 5, divMenTarget);
                }
                p = GAME.Battle().Power.Get(stat);
                SDivs.Add(stat);
                totPower -= p;
                int am = (int)((0.5 + RND.rFloat() * 3) * (powChunk / p));
                while (am-- > 0 && totPower > 0 && SDivs.Count < Config.Battle().DivisionsPerArmy)
                {
                    SDivs.Add(new RaidDiv(stat));
                    stat.Name = stat.Race().Info.ArmyNames.Rnd().ToString();
                    totPower -= p;
                }
            }

            double dd = 0;
            foreach (RaidDiv d in SDivs)
            {
                dd += GAME.Battle().Power.Get(d);
                menTot += d.Men();
            }
            this.Power = (int)dd;

            this.Men = menTot;

            int art = ADSupplies.ArtilleryMax * menTot / Config.Battle().MenPerArmy;
            while (art-- > 0)
            {
                ADArtillery a = AD.Supplies().Arts().Rnd();
                Artillery[a.Index()]++;
            }
        }

        public WArmy Spawn(int wx, int wy, string name)
        {
            Region reg = WORLD.REGIONS().Map.Get(wx, wy);
            if (reg != null && !IsGoodTile(wx, wy, DIR.C, WORLD.REGIONS().Map.Get(wx, wy)))
            {
                foreach (DIR d in DIR.ALL)
                {
                    if (WORLD.PATH().Map.Can(wx, wy, d) && IsGoodTile(wx, wy, d, reg))
                    {
                        wx += d.X();
                        wy += d.Y();
                        break;
                    }
                }
            }

            for (int i = 0; i < 10; i++)
            {
                if (!RemoveArmies(wx, wy))
                    break;
            }

            WArmy a = WORLD.ENTITIES().Armies.Create(wx, wy, null);

            foreach (RaidDiv div in SDivs)
            {
                WDivRegional d = AD.Regional().Create(div.Race(), (double)div.Men / Config.Battle().MenPerDivision, a);
                d.CopyFrom(div);
                d.MenSet(d.MenTarget());
            }
            a.Name.Clear().Add(name);
            AD.Supplies().FillAll(a);
            AD.UpdateArmy(a);
            return a;
        }

        public int Invade(int wx, int wy, Induvidual raider)
        {
            InvasionSpec sp = new InvasionSpec();
            bool first = true;
            foreach (DIV_SPEC stat in SDivs)
            {
                DivGeneration g = new DivGeneration(stat, stat);
                if (first)
                {
                    first = false;
                    g.Indus[0].CopyFrom(raider);
                }
                sp.Add(g);
            }
            for (int i = 0; i < Artillery.Length; i++)
            {
                sp.Artillery[i] = Artillery[i];
            }
            sp.Wx = wx;
            sp.Wy = wy;
            return SETT.INVADOR().Invade(sp, null);
        }

        private bool IsGoodTile(int tx, int ty, DIR d, Region home)
        {
            tx += d.X();
            ty += d.Y();

            if (!home.Is(tx, ty))
                return false;

            for (int di = 0; di < DIR.ALL.Size; di++)
            {
                d = DIR.ALL.Get(di);
                if (WORLD.PATH().Map.Can(tx, ty, d))
                {
                    Region reg = WORLD.REGIONS().Map.Get(tx, ty, d);
                    if (reg != null && reg != home && reg.Faction() != null && reg.Faction() != FACTIONS.Player())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool RemoveArmies(int wx, int wy)
        {
            for (int di = 0; di < DIR.ALLC.Size; di++)
            {
                DIR d = DIR.ALLC.Get(di);
                if (d == DIR.C || WORLD.PATH().Map.Can(wx, wy, d))
                {
                    int dx = wx + d.X();
                    int dy = wy + d.Y();

                    foreach (WArmy a2 in WORLD.ENTITIES().Armies.FillTile(dx, dy))
                    {
                        if (a2.Ctx() == dx && a2.Cty() == dy)
                        {
                            if (a2.Faction() != FACTIONS.Player() && a2.Faction() != null)
                            {
                                if (a2.Faction() != null && !a2.Faction().Playable)
                                    a2.Faction().Playable = true;

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private double Race(Race leader, Race r, double totPower)
        {
            double d = leader.Pref().Race(r);
            if (leader == r)
                d *= 4;
            else if (!r.Playable)
                d *= 0.2;
            return d;
        }

        private Race Race(Race leader, double tot, double totPower)
        {
            tot *= RND.rFloat();
            foreach (Race r in RACES.All())
            {
                tot -= Race(leader, r, totPower);
                if (tot <= 0)
                    return r;
            }
            return leader;
        }

        [Serializable]
        public class RaidDiv : DIV_SETTINGImp, DIV_SPEC
        {
            private static readonly long serialVersionUID = 1L;
            public int Race;
            public double Ex;
            private string Name;
            private int BannerI;

            public RaidDiv()
            {
            }

            public RaidDiv(RaidDiv o)
            {
                this.Race = o.Race;
                this.Ex = o.Ex;
                this.Name = o.Name;
                this.BannerI = o.BannerI;
                CopySettings(o);
            }

            public override Race Race()
            {
                return RACES.All().GetC(Race);
            }

            public override Faction Faction()
            {
                return null;
            }

            public override double Experience()
            {
                return Ex;
            }

            public override string Name()
            {
                return Name;
            }

            public override int BannerI()
            {
                return BannerI;
            }
        }
    }
}