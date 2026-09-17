using System;
using System.IO;
using System.Collections.Generic;
using game.battle.div;
using game.battle.util;
using game.boosting;
using game.faction;
using init.constant;
using init.race;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d.util.gui;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.text;
using world.entity.army;

namespace game.battle.util
{
    public sealed class Power
    {
        public static string ¤¤desc = "The overall power of a battle unit. Divided into different attack and defence types. The total power is an indication of how well the unit will perform in a fight, but in practice each type determines the outcome.";
        private static string ¤¤attack = "attack";
        private static string ¤¤defence = "defence";
        private static string ¤¤morale = "morale";
        private static string ¤¤mass = "mass";
        private static string ¤¤speed = "speed";
        private static string ¤¤charge = "charge";

        private static string ¤¤ranged = "ranged";

        static Power()
        {
            D.ts(typeof(Power));
        }

        public readonly double HIGH_POWER = 5.0;

        private double minPower = -1;
        private double maxPI = -1.0;
        private double bestRanged = 1;

        public Power()
        {
            GAME.saver().onAfterLoad(new ACTION_O<Path>(path =>
            {
                maxPI = -1;
            }));
        }

        public double Get(DIV_SPEC div)
        {
            Init();
            double d = pget(div);
            d -= minPower;
            d *= maxPI;
            return div.men() * (1 + d);
        }

        public void Hover(GUI_BOX box, DIV_SPEC spec)
        {
            GBox b = (GBox)box;

            b.title(Dic.¤¤Power);
            b.text(¤¤desc);

            double att = attack(spec);
            double def = defence(spec);
            GText t;

            b.NL(8);
            b.textLL(¤¤attack);
            b.tab(6);
            t = b.text();
            t.add('+');
            b.add(GFORMAT.f(t, att));
            b.NL();

            b.textLL(¤¤defence);
            b.tab(6);
            t = b.text();
            t.add('+');
            b.add(GFORMAT.f(t, def));
            b.NL();

            b.textLL(¤¤morale);
            b.tab(6);
            t = b.text();
            t.add('*');
            b.add(GFORMAT.f(t, bo(spec, BOOSTABLES.BATTLE().MORALE)));
            b.NL();

            b.textLL(¤¤charge);
            b.tab(6);
            t = b.text();
            t.add('+');
            b.add(GFORMAT.f(t, att * bo(spec, BOOSTABLES.BATTLE().CHARGE) / 2.0));
            b.NL();

            b.textLL(¤¤mass);
            b.tab(6);
            t = b.text();
            t.add('*');
            b.add(GFORMAT.f(t, (1 + 0.1 * bo(spec, BOOSTABLES.PHYSICS().MASS))));
            b.NL();

            b.textLL(¤¤speed);
            b.tab(6);
            t = b.text();
            t.add('*');
            b.add(GFORMAT.f(t, (1 + 0.1 * bo(spec, BOOSTABLES.PHYSICS().SPEED))));
            b.NL();

            b.textLL(¤¤ranged);
            b.tab(6);
            t = b.text();
            t.add('+');
            b.add(GFORMAT.f(t, range(spec)));
            b.NL();

            b.textLL(Dic.¤¤Soldiers);
            b.tab(6);
            t = b.text();
            t.add('*');
            b.add(GFORMAT.i(t, spec.men()));
            b.NL();

            b.tab(6);
            t = b.text();
            b.add(GFORMAT.f0(t, Get(spec)));
            b.NL();

            b.NL();
        }

        private double pget(DIV_SPEC div)
        {
            double attack = attack(div);
            double defence = defence(div) + defenceDir(div) * 0.5;

            double tot = attack + defence;
            tot *= bo(div, BOOSTABLES.BATTLE().MORALE);

            tot += attack * GAME.battle().boost(div, BOOSTABLES.BATTLE().CHARGE) / 2.0;

            tot *= 1 + 0.1 * (bo(div, BOOSTABLES.PHYSICS().MASS) - 1);
            tot *= 1 + 0.25 * (bo(div, BOOSTABLES.PHYSICS().SPEED) - 1);

            tot += range(div);

            return tot;
        }

        private double attack(DIV_SPEC div)
        {
            double baseValue = bo(div, BOOSTABLES.BATTLE().OFFENCE) + bo(div, BOOSTABLES.BATTLE().DEXTERITY) * 0.5;
            double blunt = bo(div, BOOSTABLES.BATTLE().BLUNT_ATTACK);
            double res = blunt;
            for (int di = 0; di < BOOSTABLES.BATTLE().DAMAGES.size(); di++)
            {
                res += blunt * bo(div, BOOSTABLES.BATTLE().DAMAGES.get(di).attack) / BOOSTABLES.BATTLE().DAMAGES.size();
            }
            res += baseValue;

            return res;
        }

        private double defence(DIV_SPEC div)
        {
            double baseValue = bo(div, BOOSTABLES.BATTLE().DEFENCE) + bo(div, BOOSTABLES.BATTLE().DEXTERITY) * 0.5;
            double blunt = bo(div, BOOSTABLES.BATTLE().BLUNT_DEFENCE);
            double res = blunt;
            for (int di = 0; di < BOOSTABLES.BATTLE().DAMAGES.size(); di++)
            {
                res += blunt * bo(div, BOOSTABLES.BATTLE().DAMAGES.get(di).defence) / BOOSTABLES.BATTLE().DAMAGES.size();
            }
            res += baseValue;

            return res;
        }

        private double defenceDir(DIV_SPEC div)
        {
            double baseValue = bo(div, BOOSTABLES.BATTLE().DEFENCE_DIR) + bo(div, BOOSTABLES.BATTLE().DEXTERITY) * 0.5;
            double blunt = bo(div, BOOSTABLES.BATTLE().BLUNT_DEFENCE_DIR);
            double res = blunt;
            for (int di = 0; di < BOOSTABLES.BATTLE().DAMAGES.size(); di++)
            {
                res += blunt * bo(div, BOOSTABLES.BATTLE().DAMAGES.get(di).defenceDir) / BOOSTABLES.BATTLE().DAMAGES.size();
            }
            res += baseValue;

            return res;
        }

        private double bo(DIV_SPEC div, Boostable b)
        {
            return b.stat.div().getD(div);
        }

        private double range(EquipRange r, double refValue)
        {
            return r.range(refValue);
        }

        public double BestRangedPower()
        {
            return bestRanged;
        }

        private void Init()
        {
            if (maxPI >= 0)
                return;

            DIV_SPECImp spec = new DIV_SPECImp();

            double minAverage = 0;
            for (int ri = 0; ri < RACES.playable().size(); ri++)
            {
                Race r = RACES.playable().get(ri);
                spec.clear(r);
                spec.menSet(1);
                minAverage += pget(spec);
            }
            minAverage /= RACES.playable().size();
            minPower = minAverage;

            double highAverage = 0;
            for (int ri = 0; ri < RACES.playable().size(); ri++)
            {
                Race r = RACES.playable().get(ri);
                spec.clear(r);
                spec.menSet(1);
                spec.experienceSet(0.5);

                double am = 0;
                double pp = 0;
                for (int si = 0; si < GAME.battle().types.ALL().size(); si++)
                {
                    DivType t = GAME.battle().types.ALL().get(si);
                    if (t.valid(r))
                    {
                        am += t.occurence;
                        spec.copySettings(t);
                        pp += pget(spec) * t.occurence;
                    }
                }

                double m = minAverage;
                if (am > 0)
                {
                    m = Math.Max(m, pp / am);
                }

                highAverage += m;
            }
            highAverage /= RACES.playable().size();
            double delta = highAverage - minAverage;
            maxPI = HIGH_POWER / delta;

            bestRanged = 0;
            foreach (EquipRange r in STATS.EQUIP().RANGED())
            {
                bestRanged = Math.Max(bestRanged, range(r, r.ref(1.0, GAME.battle().boostMax(r.boostable))));
            }
        }

        private Div sDiv;
        private readonly DIV_SPEC dstats = new DIV_SPEC()
        {
            training = (tr) => tr.stat.div().getD(sDiv),
            equip = (e) => e.stat().div().getD(sDiv),
            race = () => sDiv.info.race(),
            men = () => sDiv.menNrOf(),
            faction = () => sDiv.army().faction(),
            experience = () => STATS.BATTLE().COMBAT_EXPERIENCE.div().getD(sDiv),
            name = () => null,
            bannerI = () => 0
        };

        public double Get(Div div)
        {
            sDiv = div;
            return Get(dstats);
        }

        public double Get(WArmy a)
        {
            int am = 0;
            for (int di = 0; di < a.divs().size(); di++)
            {
                am += Get(a.divs().get(di));
            }
            return am;
        }
    }
}