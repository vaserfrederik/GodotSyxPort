using System;
using System.Collections.Generic;
using System.Linq;

namespace view.sett.ui.bottom
{
    public static class UIRoomBuild
    {
        private static readonly string ¤¤cost = "¤Costs";
        private static readonly string ¤¤production = "¤Production";
        private static readonly string ¤¤optional = "¤(Optional)";
        private static readonly string ¤¤Emits = "¤Emits";
        private static readonly string ¤¤CurrentRooms = "¤Current Rooms";
        private static readonly string ¤¤CurrentBelievers = "¤Current Worshippers";

        static UIRoomBuild()
        {
            D.ts(typeof(UIRoomBuild));
        }

        private UIRoomBuild()
        {
        }

        public static void hoverRoomBuild(RoomBlueprintImp b, GUI_BOX text)
        {
            GBox box = (GBox)text;
            box.title(b.info.name);
            box.text(b.info.desc);
            box.NL();

            if (b is ROOM_TEMPLE || b is ROOM_SHRINE)
            {
                Religion rel = null;
                if (b is ROOM_TEMPLE)
                {
                    rel = ((ROOM_TEMPLE)b).religion;
                }
                else
                {
                    rel = ((ROOM_SHRINE)b).religion;
                }

                box.textLL(¤¤CurrentBelievers);
                box.add(GFORMAT.perc(box.text(), STATS.RELIGION().ALL.get(rel.index).followers.data().getD(null)));
                box.NL();
            }

            b.reqs.hover(text, FACTIONS.player());

            box.sep();

            if (b is RoomBlueprintIns<?>)
            {
                RoomBlueprintIns<?> ins = (RoomBlueprintIns<?>)b;
                box.NL(2);
                box.textLL(¤¤CurrentRooms);
                box.add(GFORMAT.i(box.text(), ins.instancesSize()));
            }

            box.NL(8);

            bool e = false;
            foreach (SettEnv en in SETT.ENV().map.all())
            {
                if (b.constructor().envValue(en))
                {
                    if (!e)
                    {
                        box.textLL(¤¤Emits);
                        e = true;
                    }
                    box.text(en.info.name);
                }
            }
            box.NL();

            if (b.employment() != null)
            {
                box.textLL(Dic.¤¤AccidentRate);
                box.add(GFORMAT.perc(box.text(), b.employment().accidentsPerYear / (1 + BOOSTABLES.CIVICS().ACCIDENT.get(HCLASS_RACE.clP(null, null))), 4));
                box.NL();
            }

            if (b.constructor().resources() > 0)
            {
                box.NL(8);
                box.textLL(¤¤cost);
                int o = 0;
                for (int ri = 0; ri < b.constructor().resources(); ri++)
                {
                    if (b.upgrades().resMask(0, ri) == 0)
                        continue;
                    if (optional(b.constructor(), ri))
                    {
                        o++;
                        continue;
                    }
                    box.add(b.constructor().resource(ri).icon());
                }

                if (o > 0)
                {
                    box.space().space();
                    box.add(box.text().lablifySub().add(¤¤optional));
                    for (int ri = 0; ri < b.constructor().resources(); ri++)
                    {
                        if (b.upgrades().resMask(0, ri) == 0)
                            continue;
                        if (optional(b.constructor(), ri))
                        {
                            box.add(b.constructor().resource(ri).icon());
                        }
                    }
                }
                box.NL();
            }

            if (b is INDUSTRY_HASER)
            {
                box.NL(8);
                box.textLL(¤¤production);
                box.NL();

                foreach (Industry i in ((INDUSTRY_HASER)b).industries())
                {
                    if (i.outs().size() == 0)
                    {
                        foreach (IndustryResource r in i.ins())
                        {
                            box.add(r.resource.icon()).add(GFORMAT.f0(box.text(), -r.rate * (i.bonus() == null ? 1 : i.bonus().get(FACTIONS.player()))));
                            box.space();
                        }
                    }
                    else
                    {
                        for (int ri = 0; ri < i.ins().size(); ri++)
                        {
                            IndustryResource r = i.ins().get(ri);
                            box.add(r.resource.icon()).add(GFORMAT.f0(box.text(), -r.rate * i.bonus().get(FACTIONS.player())));
                            if (ri < i.ins().size() - 1)
                                box.add(box.text().add('&'));
                        }

                        box.add(SPRITES.icons().s.arrow_right);

                        for (int ri = 0; ri < i.outs().size(); ri++)
                        {
                            IndustryResource r = i.outs().get(ri);
                            box.add(r.resource.icon()).add(GFORMAT.fRel(box.text(), (r.rate * i.bonus().get(FACTIONS.player())), r.rate));
                            if (ri < i.outs().size() - 1)
                                box.add(SPRITES.icons().s.plus);
                        }
                    }

                    if (!i.lockable().passes(FACTIONS.player()))
                    {
                        box.add(SPRITES.icons().m.lock);
                        box.NL();
                        i.lockable().hover(text, FACTIONS.player());
                    }

                    box.sep();
                }

                if (b.bonus() != null)
                {
                    int tab = 0;
                    foreach (Race r in RACES.all())
                    {
                        box.tab(tab * 2);
                        box.add(r.appearance().icon);
                        double d = RACES.boosts().getNorSkill(r, b.employment());
                        GGaugeMutable.bad2Good(ColorImp.TMP, d);
                        int am = (int)Math.Ceiling(0.1 + d * 3);
                        am = CLAMP.i(am, 0, 3);
                        box.rewind(4);
                        for (int i = 0; i < am; i++)
                        {
                            box.add(SPRITES.icons().s.hammer, ColorImp.TMP);
                            box.rewind(8);
                        }

                        tab++;
                        if (tab > 6)
                        {
                            box.NL();
                            tab = 0;
                        }
                    }
                    box.NL();
                }
            }

            STAT stat = null;
            if (b is ROOM_SERVICE_ACCESS_HASER)
                stat = ((ROOM_SERVICE_ACCESS_HASER)b).service().stats().total();
            else if (b is ROOM_MONUMENT)
            {
                ROOM_MONUMENT m = (ROOM_MONUMENT)b;
                stat = STATS.ACCESS().MONUMENTS.ALL().get(m.monumentIndex);
            }
            if (stat != null)
            {
                box.NL(4);
                box.textLL(STANDINGS.CITIZEN().fullfillment.info().name);
                box.NL();
                int tab = 0;
                double min = 0;
                double max = 0;
                foreach (Race r in RACES.all())
                {
                    max = Math.Max(max, d.definition(r).get(HCLASSES.CITIZEN()).max);
                }

                foreach (Race r in RACES.all())
                {
                    double d = d.definition(r).get(HCLASSES.CITIZEN()).max;
                    int k = 1 + (int)(5 * d);
                    if ((r.index & 0b0011) == 0)
                        box.NL();
                    box.tab((r.index & 0b011) * 3);
                    box.add(r.appearance().icon);
                    ColorImp.TMP.interpolate(GCOLOR.UI().BAD.hovered, GCOLOR.UI().GOOD.hovered, d);
                    for (int i = 0; i < k; i++)
                    {
                        box.add(SPRITES.icons().s.heart, ColorImp.TMP);
                        box.rewind(8);
                    }
                    box.space();
                }
            }

            if (b is ROOM_POOL)
            {
                ROOM_POOL pool = (ROOM_POOL)b;
                foreach (Race r in RACES.all())
                {
                    double d = r.pref().pool(pool);
                    int k = 1 + (int)(5 * d);
                    if ((r.index & 0b0011) == 0)
                        box.NL();
                    box.tab((r.index & 0b011) * 3);
                    box.add(r.appearance().icon);
                    ColorImp.TMP.interpolate(GCOLOR.UI().BAD.hovered, GCOLOR.UI().GOOD.hovered, d);
                    for (int i = 0; i < k; i++)
                    {
                        box.add(SPRITES.icons().s.heart, ColorImp.TMP);
                        box.rewind(8);
                    }
                    box.space();
                }
            }
        }

        private static bool optional(Furnisher f, int ri)
        {
            if (f.areaCost(ri, 0) > 0)
                return false;
            if (!f.usesArea())
                return false;
            foreach (FurnisherItemGroup g in f.groups())
            {
                if (needed(f, g) && g.cost(ri, 0) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool needed(Furnisher f, FurnisherItemGroup g)
        {
            if (g.min > 0)
                return true;

            foreach (FurnisherStat s in f.stats())
            {
                if (g.stat(s.index()) > 0 && s.min > 0)
                    return true;
            }
            return false;
        }
    }
}