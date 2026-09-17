using System;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty.opinion;
using init.settings;
using init.sprite.UI;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using world;
using world.map.regions;
using world.region;

namespace view.world.ui.region
{
    final class OtherHov
    {
        private readonly GETTER_IMP<Region> g = new GETTER_IMP<>();

        private static readonly CharSequence ¤¤Besieged = "¤Besieged!";

        static
        {
            D.ts(typeof(OtherHov));
        }

        public void hover(Region reg, GUI_BOX box)
        {
            g.set(reg);
            Str.TMP.clear().add(reg.info.name());
            box.title(Str.TMP);
            GBox b = (GBox)box;

            if (reg.faction() == null)
            {
                b.add(FBanner.rebel.BIG);
                b.text(Dic.¤¤NoRuler);
            }
            else
            {
                FactionNPC f = (FactionNPC)reg.faction();
                if (reg.capitol())
                {
                    b.add(UI.icons().s.crown);
                }
                b.add(f.banner().BIG);
                b.textLL(f.name);
            }

            b.tab(9);
            b.add(RD.RACES().visuals.cRace(reg).appearance().icon);
            b.add(GFORMAT.i(b.text(), RD.RACES().population.get(reg)));

            b.NL();
            b.add(UI.icons().s.sword);
            if (RD.OWNER().affiliation.get(g.get()) >= 0.5)
                b.add(GFORMAT.i(b.text(), RD.MILITARY().garrison.get(reg)));
            else
                b.add(b.text().add('?'));
            b.tab(3);
            b.add(UI.icons().s.flag);
            b.add(GFORMAT.perc(b.text(), RD.OWNER().affiliation.getD(reg)));
            b.tab(6);
            b.add(UI.icons().s.flags);
            b.add(GFORMAT.i(b.text(), FACTIONS.player().emissaries.assimilate.get(reg)));

            if (reg.faction() != null)
            {
                b.tab(9);
                FactionNPC f = (FactionNPC)reg.faction();
                if (DIP.WAR().is(FACTIONS.player(), reg.faction()))
                    b.error(Dic.¤¤AtWar);
                else
                {
                    b.add(UI.icons().s.heart);
                    b.add(GFORMAT.perc(b.text(), ROPINION.get(f.court().king().roy())));
                }
            }

            hovSiege(reg, box);

            if (S.get().developer)
            {
                box.NL();
                b.add(b.text().add(1).s().add(FACTIONS.player().realm().regions()));
                box.NL();
                b.add(b.text().add(2).s().add(RD.DIST().reachable(reg)));
                box.NL();
                if (reg.faction() != null)
                {
                    b.add(b.text().add(3).s().add(((FactionNPC)reg.faction()).sanctified));
                }

                box.NL();
                b.add(b.text().add("pop ").s().add(RD.RACES().popSize(reg)));
                b.add(b.text().add("popT ").s().add(RD.RACES().popSizeTarget(reg)));
                b.add(b.text().add("popD ").s().add(RD.RACES().popSizeD(reg)));
                b.add(b.text().add("capT ").s().add(RD.RACES().capacity(reg)));
                box.NL();
                b.add(b.text().add(2).s().add(RD.DIST().reachable(reg)));
                box.NL();
            }
        }

        public static void hovSiege(Region reg, GUI_BOX box)
        {
            GBox b = (GBox)box;
            if (reg.besieged())
            {
                b.NL(8);
                b.error(¤¤Besieged);
                b.NL();
                GText t = b.text();
                DicTime.setYearDay(t, WORLD.BATTLES().besigedTime(reg));
                b.add(t);
                b.NL();
                b.textL(Dic.¤¤Defences);
                b.tab(6);
                b.add(GFORMAT.f0(b.text(), RD.MILITARY().defensePower(reg)));
                b.NL();
            }
        }
    }
}