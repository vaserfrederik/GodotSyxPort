using System;
using System.Collections.Generic;
using game;
using game.boosting;
using game.faction;
using game.raiding.RaidingMap;
using init.sprite.UI;
using init.type;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.info;
using util.text;
using world.army;
using world.entity.army;
using world.region;

namespace view.ui.raider
{
    public sealed class Info : GuiSection
    {
        private static readonly string ¤¤defences = "Defence";
        private static readonly string ¤¤defencesD = "The power of forces defending your city. This deters raiders.";

        private static readonly string ¤¤armiesD = "Additional armies encamped near your city.";
        private static readonly string ¤¤armiesD2 = "Armies encamped in your realm.";
        private static readonly string ¤¤suprise = "Possible surprise attack";

        private static readonly string ¤¤Ransom = "Potential Ransom";
        private static readonly string ¤¤RansomD = "The potential ransom a raider sees fit to extort you with. Based on population and credits, and lowered by raid security.";

        private static readonly string ¤¤entry = "Attack Route";
        private static readonly string ¤¤entryA = "Surprise!";
        private static readonly string ¤¤entryB = "Realm";
        private static readonly string ¤¤entryAD = "Your capital is surrounded by regions where raiders can sneak in and surprise attack you. This makes them take more risk, and as a result, they perceive your deterrent power as low.";
        private static readonly string ¤¤entryBD = "Raiders can't surprise attack your city directly. This is good. They must pass through your regions. This will buy you a lot of time if they choose to attack. Your regional defences and armies can deter raiders as well.";

        static Info()
        {
            D.ts(typeof(Info));
        }

        public Info()
        {
            int gi = 0;
            int mx = 180;

            AddGridD(new GStat
            {
                Update = (GText text) =>
                {
                    GFORMAT.i(text, (int)GAME.raiders().util.playerPow());
                },
                HoverInfoGet = (GBox b) =>
                {
                    b.text(¤¤defencesD);

                    b.NL(16);
                    b.textLL(Dic.¤¤garrison);
                    b.NL();
                    b.add(GFORMAT.iIncr(b.text(), (int)RD.MILITARY().power.getD(FACTIONS.player().capitolRegion())));
                    b.NL(8);

                    double pow = 0;
                    double pow2 = 0;
                    foreach (WArmy a in FACTIONS.player().armies().all())
                    {
                        if (a.region() == FACTIONS.player().capitolRegion())
                            pow += AD.power().get(a);
                        if (a.region() != null && a.region().faction() == FACTIONS.player())
                            pow2 += AD.power().get(a);
                    }

                    b.textLL(¤¤armiesD);
                    b.NL();
                    b.add(GFORMAT.iIncr(b.text(), (int)pow));
                    b.NL(8);

                    b.textLL(¤¤armiesD2);
                    b.NL();
                    b.add(GFORMAT.iIncr(b.text(), (int)pow2));
                    b.NL(8);

                    b.textLL(¤¤suprise);
                    b.NL();
                    if (GAME.raiders().entry.get(FACTIONS.player().capitolRegion()).points() > 0)
                    {
                        int pp = (int)(0.75 * (RD.MILITARY().power.getD(FACTIONS.player().capitolRegion()) + pow));
                        pp += pow2;
                        b.add(GFORMAT.iIncr(b.text(), -pp));
                    }
                    else
                    {
                        b.add(GFORMAT.iIncr(b.text(), (int)0));
                    }
                    b.NL(8);
                }
            }.hv(¤¤defences), gi++, 8, mx, 64, DIR.N);

            AddGridD(new GStat
            {
                Update = (GText text) =>
                {
                    if (GAME.raiders().entry.get(FACTIONS.player().capitolRegion()).points() > 0)
                    {
                        text.errorify().add(¤¤entryA);
                    }
                    else
                    {
                        text.normalify().add(¤¤entryB);
                    }
                },
                HoverInfoGet = (GBox b) =>
                {
                    if (GAME.raiders().entry.get(FACTIONS.player().capitolRegion()).points() > 0)
                    {
                        b.text(¤¤entryAD);
                    }
                    else
                    {
                        b.text(¤¤entryBD);
                        b.NL(8);
                        foreach (RaidRegion r in GAME.raiders().entry.entryRegions())
                        {
                            b.textLL(r.r().info.name());
                            b.tab(6);
                            b.add(UI.icons().s.sword);
                            b.add(GFORMAT.iIncr(b.text(), (int)RD.MILITARY().power.getD(r.r())));
                            b.NL();
                        }

                        b.NL(8);

                        foreach (WArmy a in FACTIONS.player().armies().all())
                        {
                            if (a.region() != null && a.region().faction() == FACTIONS.player())
                            {
                                b.textLL(a.name);
                                b.tab(6);
                                b.add(UI.icons().s.sword);
                                b.add(GFORMAT.iIncr(b.text(), (int)AD.power().get(a)));
                                b.NL();
                            }
                        }
                    }
                }
            }.hv(¤¤entry), gi++, 8, mx, 64, DIR.N);

            AddGridD(new GStat
            {
                Update = (GText text) =>
                {
                    double d = BOOSTABLES.CIVICS().RAID_SECURITY.get(HCLASS_RACE.clP());
                    GFORMAT.f1(text, d);
                },
                HoverInfoGet = (GBox b) =>
                {
                    BOOSTABLES.CIVICS().RAID_SECURITY.hover(b, HCLASS_RACE.clP(), true);
                }
            }.hv(BOOSTABLES.CIVICS().RAID_SECURITY.name), gi++, 8, mx, 64, DIR.N);

            AddGridD(new GStat
            {
                Update = (GText text) =>
                {
                    GFORMAT.i(text, (int)GAME.raiders().util.ransomCurrent());
                },
                HoverInfoGet = (GBox b) =>
                {
                    b.text(¤¤RansomD);
                }
            }.hv(¤¤Ransom), gi++, 8, mx, 64, DIR.N);
        }
    }
}