using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.GAME;
using game.boosting;
using game.debug;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc.stockpile;
using game.raiding.RaidingMap;
using game.time;
using init.constant;
using settlement.entry;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using view.interrupter;
using world.map.regions;

namespace game.raiding
{
    public sealed class RAIDING : GameResource
    {
        public const int AMOUNT = 100;

        private readonly ArrayList<Raider> all = new ArrayList<Raider>(AMOUNT);
        public readonly RaidingCurrent current = new RaidingCurrent();
        public readonly RaidingUtil util = new RaidingUtil(AMOUNT);

        private readonly Updater updater = new Updater(this);
        private readonly UpdaterRegions updaterRegs = new UpdaterRegions();

        public readonly RaidingMap entry = new RaidingMap();

        private readonly ACTION init = new ACTION(() =>
        {
            all.Clear();

            double w = Config.sett().POP_RAIDER_WORTH * Immigration.MAX_POPULATION + 25000000 / Config.sett().POP_RAIDER_WORTH;

            for (int i = 0; i < all.Max; i++)
            {
                double d = (double)i / all.Max;
                double wealth = NPCStockpile.AVERAGE_PRICE * 0.75 * Config.sett().POP_RAIDER_WORTH + Math.Pow(d, 2.1) * w;
                double power = 5 + Config.battle().MEN_PER_ARMY * (1 + (GAME.battle().power.HIGH_POWER - 1) / 2) * Math.Pow(d, 2.75);

                Raider r = new Raider(wealth, power, CLAMP.d(i / 10.0, 0, 1));
                all.Add(r);
            }
            current.Clear();
            updater.Clear();
        });

        public RAIDING() : base("RAIDING", false)
        {
            IDebugPanel.Add("RAIDER event", new ACTION(() =>
            {
                if (util.Active().Size > 0)
                {
                    Raider r = util.Active().Rnd();
                    current.Appear(r);
                }
                else
                    LOG.ln("nope");
            }));

            IDebugPanel.Add("RAIDER spawn", new ACTION(() =>
            {
                LIST<RaidRegion> rr = entry.EntryRegions();
                if (rr.Size == 0)
                    return;

                Region reg = rr.Rnd().R;
                double power = util.Defences(reg);
                power *= (1 + RND.rFloat(1));
                Raider raider = new Raider(FACTIONS.WORTH().raider(), power, RND.rFloat());
                raider.text.Set(raider, true);
                current.Raid(raider);
            }));

            IDebugPanel.Add("RAIDER appear", new ACTION(() =>
            {
                LIST<RaidRegion> rr = entry.EntryRegions();
                if (rr.Size == 0)
                    return;

                Region reg = rr.Rnd().R;
                double power = util.Defences(reg);
                power *= (1 + RND.rFloat(0.5));
                Raider raider = new Raider(FACTIONS.WORTH().raider(), power, RND.rFloat());
                raider.text.Set(raider, true);
                if (!current.Appear(raider))
                    LOG.ln("nope");
            }));

            IDebugPanel.Add("RAIDER appear cap", new ACTION(() =>
            {
                Raider raider = new Raider(FACTIONS.WORTH().raider(), util.Defences(FACTIONS.player().capitolRegion()) * (1 + RND.rFloat(0.5)), RND.rFloat());
                raider.text.Set(raider, true);
                current.Appear(raider, FACTIONS.player().cx(), FACTIONS.player().cy());
            }));

            IDebugPanel.Add("RAIDER clear", new ACTION(() =>
            {
                current.Clear();
            }));

            IDebugPanel.Add("RAIDER mess", new ACTION(() =>
            {
                Raider raider = new Raider(FACTIONS.WORTH().raider(), util.Defences(FACTIONS.player().capitolRegion()) * (1 + RND.rFloat(0.5)), RND.rFloat());
                raider.text.Set(raider, RND.rBoolean());
                new MessArmyAppear(raider, RND.rInt0(100), RND.rInt0(100)).Send();
                new MessCustom(raider, "hello").Send();
                new MessDefeated(raider).Send();
                new MessDemand(raider).Send();
                new MessDemandRejected(raider).Send();
                new MessDemandTY(raider).Send();
                new MessGoingAway(raider).Send();
                new MessVictory(raider).Send();
            }));

            IDebugPanel.Add("RAIDERS reset", init);

            GAME.AddOnInit(new ACTION(() =>
            {
                BValueAll vv = new BValueAll(() =>
                {
                    if (DIP.overlord(FACTIONS.player()) != null)
                        return 1;
                    return 0;
                });

                BSourceInfo s = new BSourceInfo(DIP.VASSAL().name, DIP.VASSAL().icon);
                new BoosterValue(vv, s, 0, 4, false).Add(BOOSTABLES.CIVICS().RAID_SECURITY);
            }));

            GAME.AddOnInit(init);
        }

        protected override void Save(FilePutter file)
        {
            foreach (Raider r in all)
            {
                file.Object(r);
            }
            current.Save(file);
            updater.Save(file);
            updaterRegs.Save(file);
        }

        protected override void Load(FileGetter file) => throw new NotImplementedException();

        void LoadFix()
        {
            int powMax = 0;
            int pp = 0;
            foreach (Raider r in all)
            {
                if (r.defeated)
                {
                    Defeat(r);
                    powMax = Math.Max(powMax, r.army.power);
                }
                if (r.raids > 0)
                    pp = Math.Max(powMax, pp);
            }
            init.Exe();
            foreach (Raider r in ALL())
            {
                if (r.army.power <= powMax)
                    r.defeated = true;
            }
        }

        protected override void Update(double ds, Profiler prof)
        {
            prof.LogStart(this);
            current.Update(ds, prof);
            updater.Update(ds);
            updaterRegs.Update(ds);
            prof.LogEnd(this);
        }

        public LIST<Raider> ALL() => all;

        void Defeat(Raider raider)
        {
            raider.defeated = true;
            raider.secondDefeated = TIME.currentSecond();
        }

        public LIST<Raider> Active() => util.Active();

        public void Reset() => init.Exe();

        public void Raid()
        {
            if (Active().Size > 0)
            {
                Raider r = Active().Rnd();
                r.text.Set(r, r.raids == 0);
                GAME.raiders().current.Raid(r);
            }
        }
    }
}