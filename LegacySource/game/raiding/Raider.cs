using System;
using System.Collections.Generic;
using game;
using game.faction;
using init.race;
using settlement.main;
using settlement.stats;
using snake2d;
using util.gui.misc;
using util.info;
using util.text;

namespace game.raiding
{
    [Serializable]
    public class Raider
    {
        private static readonly long serialVersionUID = 1L;
        public readonly Individual indu;
        public readonly string name;
        public RaiderArmy army;
        public int raids;
        public int bounty;
        public bool defeated = false;
        public double worth;
        bool hasAttacked = false;
        public readonly RaiderText text;
        public double secondDefeated;

        public Raider(double wealth, double power, double quality)
        {
            double ri = 0;
            foreach (Race r in RACES.all())
            {
                ri += r.physics.raiding;
            }
            ri *= RND.rFloat();
            Race rr = RACES.playable().rnd();
            foreach (Race r in RACES.all())
            {
                ri -= r.physics.raiding;
                if (ri <= 0)
                {
                    rr = r;
                    break;
                }
            }

            indu = new Individual(HTYPES.SOLDIER(), rr);
            Str.TMP.clear().add(indu.race().info.raiderNames[RND.rInt(indu.race().info.raiderNames.Length)]);
            Str.TMP.insert(0, STATS.APPEARANCE().nameFirst.name(indu));
            name = "" + Str.TMP;
            adjust(wealth, power, quality);
            text = new RaiderText();
        }

        public Raider(Race race, double power)
        {
            indu = new Individual(HTYPES.SOLDIER(), race);
            Str.TMP.clear().add(indu.race().info.raiderNames[RND.rInt(indu.race().info.raiderNames.Length)]);
            Str.TMP.insert(0, STATS.APPEARANCE().nameFirst.name(indu));
            name = "" + Str.TMP;
            adjust(0, power, RND.rFloat());
            text = new RaiderText();
        }

        public void adjust(double wealth, double power, double quality)
        {
            worth = wealth;
            army = new RaiderArmy(indu.race(), power, quality);
            bounty = (int)worth;
        }

        public void hover(GUI_BOX text)
        {
            GBox b = (GBox)text;
            b.title(name);

            b.text(Dic.¤¤Soldiers);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), army.men));
            b.NL();

            b.text(Dic.¤¤Power);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), army.power));
            b.NL();

            b.text(Dic.¤¤Currs);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), (int)worth));
            b.NL();
        }

        public bool hasInterrest()
        {
            if (!SETT.ROOMS().BARRACKS.get(0).reqs.passes(FACTIONS.player()))
                return false;
            if (STATS.POP().POP.data(null).get(null) < 200)
                return false;
            return GAME.raiders().util.ransomCurrent() > worth;
        }

        public bool isScared()
        {
            return army.power < GAME.raiders().util.weakestRegionPow() || army.power < GAME.raiders().util.playerPow();
        }
    }
}