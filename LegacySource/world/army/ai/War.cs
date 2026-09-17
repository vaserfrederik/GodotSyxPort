using System;
using System.Collections.Generic;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.time;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.updating;
using world;
using world.army;
using world.entity.army;
using world.map.pathing;
using world.map.regions;
using world.region;

namespace world.army.ai
{
    final class War : IUpdater
    {
        private readonly Recruiter recruiter = new Recruiter();
        private readonly Defender defender = new Defender();
        private readonly Attacker attacker = new Attacker();
        private readonly Chiller chiller = new Chiller();

        private readonly Bitsmap1D hasSentMessage = new Bitsmap1D(0, 4, FACTIONS.MAX());

        private readonly ArrayList<WArmy> armies = new ArrayList<>(32);

        public War() : base(FACTIONS.MAX(), TIME.secondsPerDay() / 2)
        {
        }

        protected override void update(int i, double timeSinceLast)
        {
            Faction f = FACTIONS.getByIndex(i);
            if (!f.isActive())
                return;
            if (f == FACTIONS.player())
                return;
            recruiter.recruit((FactionNPC)f);
            planForWar(f);
        }

        public override void save(FilePutter file)
        {
            hasSentMessage.save(file);
            base.save(file);
        }

        public override void load(FileGetter file)
        {
            hasSentMessage.load(file);
            base.load(file);
        }

        public override void clear()
        {
            hasSentMessage.clear();
            base.clear();
        }

        void planForWar(Faction f)
        {
            if (f == null || !f.isActive())
                return;
            if (f == FACTIONS.player())
                return;
            armies.clearSloppy();

            for (int ai = 0; ai < f.armies().all().size(); ai++)
            {
                WArmy a = f.armies().all().get(ai);
                if (f != FACTIONS.player() && a.raiding() && RD.DEVASTATION().current.get(a.region()) > 0.9)
                    a.stop();
                if (AD.men(null).get(a) > 0 && armies.hasRoom())
                    armies.add(a);
            }

            foreach (WArmy a in armies)
            {
                if (a.intercepting() != null && !DIP.WAR().is(a.intercepting().faction(), f))
                    a.stop();
                else if (a.state() == WArmyState.besieging)
                {
                    Region reg = a.besieging();

                    if (reg == null || !DIP.WAR().is(reg.faction(), f))
                    {
                        a.stop();
                    }
                    armies.remove(a);
                }

                if (a.state() == WArmyState.fortified || a.state() == WArmyState.fortifying)
                {
                    if (a.region() == null || (a.region().faction() != f && !DIP.WAR().is(f, a.region().faction())))
                    {
                        allyFaction = f;
                        RegDist d = WORLD.PATH().regFinder.single(a.ctx(), a.cty(), Treaty.FACTION, ally);
                        armies.remove(a);
                        if (d != null)
                        {
                            COORDINATE c = WORLD.PATH().rnd(d.reg);
                            a.setDestination(c.x(), c.y());
                            continue;
                        }
                        d = WORLD.PATH().regFinder.single(a.ctx(), a.cty(), Treaty.DUMMY, ally);

                        if (d != null)
                        {
                            COORDINATE c = WORLD.PATH().rnd(d.reg);
                            a.teleport(c.x(), c.y());
                            continue;
                        }
                        a.disband();
                    }
                }
            }

            if (DIP.WAR().all(f).size() == 0)
            {
                chiller.chill(f, armies);
                return;
            }

            if (logging)
                log(f, "has " + armies.size() + " armies to use");

            defender.defend(f, armies);
            if (logging)
                log(f, "has " + armies.size() + " armies for offence");

            attacker.attack(f, armies);
            if (logging)
                log(f, "has " + armies.size() + " armies without orders");
        }

        private Faction allyFaction;
        private WRegSel ally = new WRegSel()
        {
            public bool is(Region t)
            {
                return t.faction() == allyFaction;
            }
        };

        public void init(Faction f)
        {
            update(f.index(), 0);

            foreach (WArmy a in f.armies().all())
            {
                for (int i = 0; i < a.divs().size(); i++)
                {
                    a.divs().get(i).menSet(a.divs().get(i).menTarget());
                }

                foreach (ADSupply s in AD.supplies().all)
                {
                    s.current().set(a, s.targetAmount(a));
                }
            }
        }

        static bool logging = false;

        public static void log(WArmy d, string message)
        {
            string s = d.faction().ToString() + " " + d.ctx() + " " + d.cty() + ": " + message;
            LOG.ln(s);
        }

        public static void log(Faction f, string message)
        {
            string s = f.ToString() + ": " + message;
            LOG.ln(s);
        }
    }
}