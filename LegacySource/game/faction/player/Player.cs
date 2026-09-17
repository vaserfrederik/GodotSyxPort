using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.boosting;
using game.debug;
using game.faction;
using game.faction.FBanner;
using game.faction.FCredits;
using game.faction.FResources;
using game.faction.player.emmi;
using game.time;
using init.race;
using init.trade;
using init.type;
using settlement.main;
using settlement.stats;
using settlement.trade;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using view.interrupter;
using world.army;
using world.region;
using world.region.pop;

namespace game.faction.player
{
    public sealed class Player : Faction, BOOSTABLE_O
    {
        public readonly PTech tech = new PTech();
        public readonly PTitles titles = new PTitles();
        public readonly PlayerRaces races;
        private readonly FResources resources = new FResources(48, TIME.Days())
        {
            public override int GetAvailable(TRADABLE t)
            {
                int a = t.Ps().PlayerOwned();
                return (int)Math.Ceiling(a * 0.9);
            }
        };

        private readonly FBanner banner = new FBanner(this);
        private readonly PLevels level = new PLevels();
        private readonly PCredits credits = new PCredits();
        public readonly Emissaries emissaries = new Emissaries();
        public readonly PBonusSetting bonusesCustom;
        private int ri;
        public readonly PTrade trade = new PTrade();
        public readonly Str rulerName = new Str(24).Add("bob");
        public readonly Str desc = new Str(24);

        public Player(LISTE<Faction> all) : base(all)
        {
            races = new PlayerRaces();
            ri = races.Get(0).Index();

            IDebugPanel.Add("add credits", new ACTION
            {
                public override void Exe()
                {
                    credits.Inc(50, CTYPE.MISC);
                }
            });

            IDebugPanel.Add("add credits+", new ACTION
            {
                public override void Exe()
                {
                    credits.Inc(10000000, CTYPE.MISC);
                }
            });

            bonusesCustom = new PBonusSetting();
        }

        public void SetRace(Race race)
        {
            ri = race.Index;
            races.Set(race);
        }

        public override Race Race()
        {
            return RACES.All().Get(ri);
        }

        protected override void Save(FilePutter file)
        {
            base.Save(file);
            file.I(ri);
            tech.Saver.Save(file);
            titles.Saver.Save(file);
            level.Saver.Save(file);
            races.Saver.Save(file);
            emissaries.Saver.Save(file);
            bonusesCustom.Save(file);
            trade.Saver.Save(file);
            rulerName.Save(file);
            PlayerColors.Saver.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            base.Load(file);
            ri = file.I();
            ri %= RACES.All().Size();
            tech.Saver.Load(file);
            titles.Saver.Load(file);
            level.Saver.Load(file);
            races.Saver.Load(file);
            emissaries.Saver.Load(file);
            bonusesCustom.Load(file);
            trade.Saver.Load(file);
            rulerName.Load(file);
            PlayerColors.Saver.Load(file);
        }

        protected override void Clear()
        {
            base.Clear();
            tech.Saver.Clear();
            titles.Saver.Clear();
            level.Saver.Clear();
            races.Saver.Clear();
            emissaries.Saver.Clear();
            bonusesCustom.Clear();
            trade.Saver.Clear();
            PlayerColors.Saver.Clear();
        }

        public override PBuyer Buyer(TRADABLE tr)
        {
            return SETT.TRADE().Buyer(tr);
        }

        public override PSeller Seller(TRADABLE t)
        {
            return SETT.TRADE().Seller(t);
        }

        protected override void Update(double ds)
        {
            base.Update(ds);
        }

        public void UpdateSpecial(double ds, Profiler prof)
        {
            if (CapitolRegion() != null)
            {
                int ex = 0;
                foreach (Race r in RACES.All())
                {
                    if (RD.RACES().Get(r) == null)
                        ex += STATS.POP().POP.Data(null).Get(r, 0);
                }

                ex /= (RACES.All().Size() - RD.RACES().All.Size() + 1);

                foreach (RDRace rr in RD.RACES().All)
                {
                    rr.Pop.Set(CapitolRegion(), STATS.POP().POP.Data(null).Get(rr.Race, 0) + ex);
                }
            }

            prof.LogStart(tech.GetType());
            tech.Update(ds);
            prof.LogEnd(tech.GetType());

            prof.LogStart(titles.GetType());
            titles.Update(ds);
            prof.LogEnd(titles.GetType());

            prof.LogStart(level.GetType());
            level.Update(ds);
            prof.LogEnd(level.GetType());

            prof.LogStart(emissaries.GetType());
            emissaries.Update(ds);
            prof.LogEnd(emissaries.GetType());

            prof.LogStart(trade.GetType());
            trade.Update(ds);
            prof.LogEnd(trade.GetType());
        }

        public override FResources Res()
        {
            return resources;
        }

        public override FBanner Banner()
        {
            return banner;
        }

        public override PCredits Credits()
        {
            return credits;
        }

        public PLevels Level()
        {
            return level;
        }

        public PTech Tech()
        {
            return tech;
        }

        public override string RulerName()
        {
            return rulerName.ToString();
        }

        public static class PlayerRaces
        {
            private readonly int[] order;

            public readonly SAVABLE Saver = new SAVABLE
            {
                public void Save(FilePutter file)
                {
                    file.IsE(order);
                }

                public void Load(FileGetter file)
                {
                    if (!file.IsE(order))
                    {
                        Set(FACTIONS.Player().Race());
                    }
                }

                public void Clear()
                {
                }
            };

            void Set(Race player)
            {
                order[0] = player.Index;
                int playable = 1;

                for (int ri = 0; ri < RACES.All().Size(); ri++)
                {
                    Race r = RACES.All().Get(ri);
                    if (r != player && r.Playable)
                    {
                        playable++;
                    }
                }
                int i = 1;
                for (int ri = 0; ri < RACES.All().Size(); ri++)
                {
                    Race r = RACES.All().Get(ri);
                    if (r != player && r.Playable)
                    {
                        order[i++] = r.Index;
                    }
                    else if (r != player)
                    {
                        order[playable++] = r.Index;
                    }
                }
            }

            public PlayerRaces()
            {
                int playable = 0;
                foreach (Race r in RACES.All())
                {
                    if (r.Playable)
                    {
                        playable++;
                    }
                }
                int i = 0;
                foreach (Race r in RACES.All())
                {
                    if (r.Playable)
                    {
                        order[i++] = r.Index;
                    }
                    else
                    {
                        order[playable++] = r.Index;
                    }
                }
            }

            public void Order(Race r, int index)
            {
                int i = 0;
                for (; i < order.Length; i++)
                    if (order[i] == r.Index)
                        break;

                for (; i < order.Length - 1; i++)
                    order[i] = order[i + 1];

                for (i = order.Length - 1; i > index; i--)
                    order[i] = order[i - 1];

                order[index] = r.Index;
            }

            public Race Get(int index)
            {
                if (index < 0)
                    return null;
                return RACES.All().Get(order[index]);
            }

            public int Size()
            {
                return order.Length;
            }
        }

        public override double BoostableValue(BValue v)
        {
            return v.VGet(this);
        }

        public override double OffensivePower()
        {
            double p = AD.Power().Get(this);
            int creds = (int)FACTIONS.Player().Credits().GetD();
            for (int i = 0; i < AD.Mercenaries().Max() && creds > 0; i++)
            {
                WDivMercenary d = AD.Mercenaries().Get(i);
                if (d.Army() == null && !d.Disbanded())
                {
                    int c = AD.Mercenaries().SigningCost(i) + AD.Mercenaries().UpkeepCost(i) * 16;
                    double dd = (double)creds / c;
                    dd = CLAMP.D(dd, 0, 1);
                    p += GAME.Battle().Power.Get(d);
                    creds -= (int)Math.Ceiling(c * dd);
                }
            }

            p += RD.MILITARY().Power.GetD(CapitolRegion()) - SETT.INVADOR().InvadingPower();

            return Math.Max(p, 0);
        }

        public override int Citizens(Race race)
        {
            return STATS.POP().POP.Data(HCLASSES.CITIZEN()).Get(race);
        }
    }
}