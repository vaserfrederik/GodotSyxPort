using System;
using System.Collections.Generic;
using game.GAME;
using game.debug;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.player;
using game.faction.royalty.opinion;
using game.faction.trade;
using game.time;
using init.sprite;
using settlement.main;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.text;
using util.updating;
using view.interrupter;
using world;
using world.map.regions;
using world.region;
using world.region.pop;

namespace game.faction
{
    public class FACTIONS : GameResource
    {
        private const int MAX = 64;
        private const int NPCS_MAX = MAX - 1;
        private static FACTIONS self;

        private readonly IUpdater updater = new IUpdater(MAX, TIME.days().bitSeconds() / 4)
        {
            protected override void update(int i, double timeSinceLast)
            {
                if (all.get(i).isActive())
                {
                    all.get(i).update(timeSinceLast);
                }
            }
        };

        private readonly ArrayList<Faction> all = new ArrayList<Faction>(MAX);
        private readonly ArrayList<FactionNPC> npcs = new ArrayList<FactionNPC>(MAX - 1);
        private readonly ArrayList<FactionNPC> npcsActive = new ArrayList<FactionNPC>(MAX - 1);
        private readonly ArrayList<Faction> active = new ArrayList<Faction>(MAX);
        private bool dirty = true;

        private readonly Player player;
        private readonly FactionResource npcManager;
        public readonly UpdaterNPC ncpUpdater;
        private FactionNPC otherFaction;
        private readonly ResourcePrices prices;
        private readonly FWorth worth = new FWorth();
        private DIP dip;

        private static readonly CharSequence ¤¤sim = "Simulating factions";
        private static readonly CharSequence ¤¤factionDestroyed = "The faction of {0} has been completely destroyed.";
        private static readonly CharSequence ¤¤newFaction = "A new faction has emerged. They call themselves '{0}'.";

        static FACTIONS()
        {
            D.ts(typeof(FACTIONS));
        }

        public FACTIONS() : base("FACTIONS", false)
        {
            self = this;
            new ROPINION(this);
            this.player = new Player(all);
            ncpUpdater = new UpdaterNPC();
            for (int i = 1; i < MAX; i++)
            {
                npcs.add(new FactionNPC(all, ncpUpdater));
            }
            otherFaction = npcs.get(0);

            npcManager = new TradeManager(this);
            dip = new DIP(this);

            new Initer(all);
            FactionProfileFlusher.load(FACTIONS.player());

            new RD.RDOwnerChanger
            {
                change = (Region reg, Faction oldOwner, Faction newOwner) =>
                {
                    activate(oldOwner);
                    activate(newOwner);
                }
            };

            IDebugPanel.add("Factions Prime", new ACTION
            {
                exe = () =>
                {
                    prime();
                }
            });

            prices = new ResourcePrices();
        }

        private void activate(Faction f)
        {
            if (f == null || f == player)
                return;
            bool a = f.isActive();
            if (f.wasActive != a)
            {
                dirty = true;
                f.wasActive = a;
                if (a)
                {
                    foreach (Faction.FactionActivityListener li in Faction.FactionActivityListener.all)
                        li.add((FactionNPC)f);
                }
                else
                {
                    foreach (Faction.FactionActivityListener li in Faction.FactionActivityListener.all)
                        li.remove((FactionNPC)f);
                }
            }
        }

        protected override void save(FilePutter file)
        {
            foreach (Faction f in all)
            {
                file.mark("" + f.index());
                f.save(file);
                file.mark("" + f.index());
            }
            updater.save(file);
            npcManager.save(file);
            ((FactionResource)dip).save(file);
            file.i(otherFaction.index());
        }

        protected override void load(FileGetter file) throws IOException
        {
            foreach (Faction f in all)
            {
                file.check("" + f.index());
                f.load(file);
                file.check("" + f.index());
            }
            updater.load(file);
            npcManager.load(file);
            ((FactionResource)dip).load(file);

            otherFaction = (FactionNPC)all.get(file.i());
            dirty = true;
            prices.clearCache();
        }

        protected override void update(double ds, Profiler prof)
        {
            prof.logStart(updater.GetType());
            updater.update(ds);
            prof.logEnd(updater.GetType());

            prof.logStart(npcManager.GetType());
            npcManager.update(ds, null);
            prof.logEnd(npcManager.GetType());

            prof.logStart(player.GetType());
            player.updateSpecial(ds, prof);
            prof.logEnd(player.GetType());

            prof.logStart(dip.GetType());
            ((FactionResource)dip).update(ds, null);
            prof.logEnd(dip.GetType());
        }

        public static Player player()
        {
            return self.player;
        }

        public static Faction getByIndex(int index)
        {
            return self.all.get(index);
        }

        public void prime()
        {
            SPRITES.loader().print(¤¤sim);

            int a = 50;

            for (int i = 0; i < a; i++)
            {
                SPRITES.loader().print(¤¤sim + ": " + (int)(100 * ((i * 2 + a * 2) / (double)(a * 4))) + "%");

                foreach (FactionNPC f in FACTIONS.NPCs())
                {
                    RD.UPDATER().shipAll(f, 1.0);
                    f.stockpile.update(f, TIME.secondsPerDay());
                }

                if (i % 4 == 0)
                {
                    ((TradeManager)self.npcManager).prime();
                }
            }
            ((TradeManager)self.npcManager).prime();
            prices.clearCache();
        }

        public static FactionNPC activateNext(Region capitol, RDRace prefRace, bool log)
        {
            if (capitol.realm() != null)
                throw new RuntimeException();

            FactionNPC ff = self.free();
            if (ff == null)
                return null;
            ff.clear();
            capitol.fationSet(ff, log);

            capitol.info.name().clear().add(ff.name);

            ff.generate(prefRace, true);

            foreach (Faction.FactionActivityListener li in Faction.FactionActivityListener.all)
                li.add(ff);
            ((Faction)ff).wasActive = true;

            if (log && SETT.exists())
            {
                Str.TMP.clear().add(¤¤newFaction);
                Str.TMP.insert(0, ff.name);
                WORLD.LOG().log(null, ff, UI.icons().s.crown, Str.TMP, ff.cx(), ff.cy());
            }

            return ff;
        }

        public static bool canActivateNext()
        {
            return self.free() != null;
        }

        public static int frees()
        {
            int am = 0;
            foreach (FactionNPC f in self.npcs)
            {
                if (!f.isActive())
                {
                    am++;
                }
            }
            return am;
        }

        public FactionNPC free()
        {
            foreach (FactionNPC f in npcs)
            {
                if (!f.isActive())
                {
                    ((Faction)f).wasActive = false;
                    return f;
                }
            }
            self.dirty = true;
            return null;
        }

        public static LIST<FactionNPC> NPCs()
        {
            active();
            return self.npcsActive;
        }

        public static LIST<Faction> active()
        {
            if (self.dirty)
            {
                self.active.clearSloppy();
                self.active.add(self.player);
                self.npcsActive.clearSloppy();
                self.dirty = false;
                for (int i = 0; i < self.npcs.size(); i++)
                {
                    if (self.npcs.get(i).realm().capitol() != null)
                    {
                        self.npcsActive.add(self.npcs.get(i));
                        self.active.add(self.npcs.get(i));
                    }
                }
            }
            return self.active;
        }

        public static LIST<Faction> all()
        {
            return self.all;
        }

        public static void remove(FactionNPC faction, bool log)
        {
            if (log)
            {
                Str.TMP.clear().add(¤¤factionDestroyed);
                Str.TMP.insert(0, faction.name);
                WORLD.LOG().log(null, faction, UI.icons().s.crown, Str.TMP, faction.cx(), faction.cy());
            }

            faction.armies().disbandAll();
            RD.clearFaction(faction);
            self.dirty = true;
        }

        public static void otherFactionSet(FactionNPC faction)
        {
            self.otherFaction = faction;
        }

        public static FactionNPC otherFaction()
        {
            return self.otherFaction;
        }

        public static CharSequence name(Faction f)
        {
            if (f == null)
                return Dic.¤¤Rebels;
            return f.name;
        }

        public static ResourcePrices PRICE()
        {
            return self.prices;
        }

        public static FWorth WORTH()
        {
            return self.worth;
        }

        public static int MAX()
        {
            return MAX;
        }

        public static int NPC_MAX()
        {
            return NPCS_MAX;
        }
    }
}