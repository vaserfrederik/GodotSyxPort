using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc.stockpile;
using game.tourism;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.gui;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.stats.colls
{
    public class StatsGovern : StatCollection
    {
        public readonly STAT tourismFriend;
        public readonly STAT tourismEnemy;
        public readonly STAT RICHES;

        private static readonly string ¤¤name = "Government";
        private static readonly string ¤¤desc = "Government stats";

        static StatsGovern()
        {
            D.ts(typeof(StatsGovern));
        }

        public StatsGovern(StatsInit init) : base(init, "GOVERN", ¤¤name, ¤¤desc)
        {
            tourismFriend = new STATFakeRace("TOURISM_FRIEND", init)
            {
                protected override double getDD(Race r)
                {
                    double res = 0;
                    double tot = 0;
                    foreach (Race other in RACES.all())
                    {
                        tot += TOURISM.race(other);
                        res += TOURISM.race(other) * r.pref().race(other);
                    }
                    if (tot == 0)
                        return 0;

                    return res / tot;
                }
            };
            tourismFriend.info().icon = UI.icons().m.citizen;

            tourismEnemy = new STATFakeRace("TOURISM_ENEMY", init)
            {
                protected override double getDD(Race r)
                {
                    double res = 0;
                    double tot = 0;
                    foreach (Race other in RACES.all())
                    {
                        tot += TOURISM.race(other);
                        res += TOURISM.race(other) * (1.0 - r.pref().race(other));
                    }
                    if (tot == 0)
                        return 0;

                    return (int)(res / tot);
                }
            };
            tourismEnemy.info().icon = UI.icons().m.citizen.twin(UI.icons().m.anti);

            RICHES = new STATFakeRace("RICHES", init)
            {
                protected override double getDD(Race r)
                {
                    int pop = (POP.tot(HCLASSES.CITIZEN(), null) + POP.tot(HCLASSES.NOBLE(), null)) * NPCStockpile.AVERAGE_PRICE * 4;
                    double d = FACTIONS.player().credits().credits();
                    if (pop == 0)
                        return d > 0 ? 1 : 0;

                    return d / pop;
                }

                public override void hover(GUI_BOX text, HCLASS cl, Race type)
                {
                    GBox b = (GBox)text;
                    b.NL();
                    b.textLL(Dic.¤¤Currs);
                    int tot = (POP.tot(HCLASSES.CITIZEN(), null) + POP.tot(HCLASSES.NOBLE(), null)) * NPCStockpile.AVERAGE_PRICE * 4;
                    b.add(GFORMAT.iofk(b.text(), (int)FACTIONS.player().credits().credits(), tot));
                    b.NL();
                    base.hover(text, cl, type);
                }
            };
            RICHES.info().setMatters(true, false);
            RICHES.info().icon = UI.icons().m.coins;
        }
    }
}