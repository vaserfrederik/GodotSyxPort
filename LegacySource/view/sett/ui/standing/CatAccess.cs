using System;
using System.Collections.Generic;
using snake2d;
using util.gui.slider;
using util.gui.table;
using util.text;
using view.sett.ui.standing.Cats;

class CatAccess : Cat
{
    private static readonly string ¤¤PreferedBy = "¤Preferred By:";
    private static readonly string ¤¤Allowed = "¤Allowed to consume";
    private static readonly string ¤¤AllowedNot = "¤Not allowed to consume";
    private static readonly string ¤¤Yearly = "¤{0} per item per year. Estimation: -{1} with current furnishing, and -{2} if the target is fulfilled.";
    private static readonly string ¤¤FurnitureD = "The amount allowed to furnish a subject's home. More allowed and available will increase happiness from furnishing.";
    private static readonly string ¤¤desc = "¤Stats and settings related to consumable resources";

    static CatAccess()
    {
        D.ts(typeof(CatAccess));
    }

    public CatAccess(HCLASS c, Func<Race> race)
        : base(new StatCollection[]
        {
            STATS.FOOD(), STATS.EQUIP(), STATS.HOME()
        })
    {
        titleSet(Dic.¤¤Access);
        name = Dic.¤¤Access;
        desc = ¤¤desc;
        LinkedList<RENDEROBJ> rens = new LinkedList<RENDEROBJ>();

        {
            StatsFood s = STATS.FOOD();

            rens.Add(new StatRow.Title(s.info));

            foreach (STAT st in s.all())
            {
                rens.Add(new StatRow(st, c, race));
            }

            GuiSection ss = new GuiSection();

            int ww = 7;

            LIST<RESOURCE> ll = RESOURCES.EDI().res().join(RESOURCES.DRINKS().res());
            RBITImp bbb = new RBITImp();
            for (int i = 0; i < ll.size(); i++)
            {
                RESOURCE e = ll.get(i);
                if (bbb.has(e))
                    continue;
                bbb.or(e);
                final int k = i;
                Checkbox cl = new Checkbox(e.icon())
                {
                    protected override void clickA()
                    {
                        s.allowed(k).toggle(c, race());
                    }

                    protected override void renAction()
                    {
                        selectedSet(s.allowed(k).get(c, race()));
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        if (race() != null && race().pref().foodMask.has(e))
                        {
                            COLOR.WHITE100.render(r, body, 1);
                            COLOR.WHITE15.render(r, body, 0);
                        }
                        base.render(r, ds, isActive, isSelected, isHovered);
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        text.title(e.name);
                        if (selectedIs())
                            b.text(¤¤Allowed);
                        else
                            b.error(¤¤AllowedNot);
                        b.NL(4);
                        b.textLL(¤¤PreferedBy);
                        b.NL();
                        if (RESOURCES.EDI().is(e))
                        {
                            foreach (Race r in RACES.all())
                            {
                                if (r.pref().food.contains(RESOURCES.EDI().get(e)))
                                {
                                    b.add(r.appearance().iconBig);
                                }
                            }
                        }
                        if (RESOURCES.DRINKS().is(e))
                        {
                            foreach (Race r in RACES.all())
                            {
                                if (r.pref().drink.contains(RESOURCES.DRINKS().get(e)))
                                {
                                    b.add(r.appearance().iconBig);
                                }
                            }
                        }
                    }
                };
                cl.hoverTitleSet(e.name);
                cl.pad(8, 2);

                ss.add(cl, cl.body().width() * (i % ww), (i / ww) * cl.body().height());
            }

            rens.Add(ss);
        }

        {
            StatsEquip s = STATS.EQUIP();
            rens.Add(new StatRow.Title(s.info));

            foreach (EquipCivic ss in s.civics())
            {
                rens.Add(new StatRowEquip(ss, c, race));
            }
        }

        {
            LIST<RES_AMOUNT> rr = RACES.res().homeResMax(c);

            StatsHome s = STATS.HOME();
            rens.Add(new StatRow.Title(s.info));

            foreach (STAT st in s.all())
            {
                if (st.key() != null)
                    rens.Add(new StatRow(st, c, race));
            }

            GuiSection ss = null;
            for (int ri = 0; ri < rr.size(); ri++)
            {
                if (ri % 3 == 0)
                {
                    ss = new GuiSection();
                    rens.Add(ss);
                }
                ss.add(new StatHomeFurniture(ri, c, race, rr), (ri % 3) * 170, 0);
            }
        }

        section.add(new GScrollRows(rens, HEIGHT, 0).view());
    }

    private class StatHomeFurniture : GuiSection
    {
        private readonly int ri;
        private readonly HCLASS cl;
        private readonly Func<Race> race;

        public StatHomeFurniture(int ri, HCLASS cl, Func<Race> race, LIST<RES_AMOUNT> rr)
        {
            this.ri = ri;
            this.cl = cl;
            this.race = race;

            INTE inTarget = new INTE()
            {
                min = () => 0,
                max = () => STATS.HOME().max(ri),
                get = () => STATS.HOME().target(cl, race(), ri),
                set = (t) => STATS.HOME().targetSet(cl, race(), ri, t)
            };

            add(new GTarget(40, false, true, inTarget).hoverInfoSet(() => "Set target for " + res().resource().name));
            add(new GStat()
            {
                update = (text) => StatRow.format(text, STATS.HOME().stat(ri), STATS.HOME().stat(ri).data(cl).getD(race()), cl, race())
            }, 230, 0);
            add(new StatRow.Meter(STATS.HOME().stat(ri), cl, race), 320, 0);
            pad(2, 4);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            activeSet(res() != null);
            if (activeIs())
                base.render(r, ds);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            RES_AMOUNT res = res();
            if (res == null)
                return;
            GBox b = (GBox)text;
            b.title(res.resource().name);

            b.text(¤¤FurnitureD);
            b.NL(8);

            int tot = STATS.HOME().needed(cl, race(), ri);
            int am = STATS.HOME().current(cl, race(), ri);
            b.add(GFORMAT.iofkInv(b.text(), am, tot));
            b.NL(8);

            b.textL(Dic.¤¤ConsumptionRate);
            b.NL();
            GText t = b.text();
            t.add(¤¤Yearly);
            t.insert(0, STATS.HOME().rate(cl, race()), 2);
            t.insert(1, (int)(STATS.HOME().rate(cl, race()) * STATS.HOME().current(cl, race(), ri)));
            t.insert(2, (int)(STATS.HOME().rate(cl, race()) * STATS.POP().POP.data(cl).get(race())) * STATS.HOME().target(cl, race(), res.resource()));
            b.add(t);

            if (race() == null)
            {
                b.NL(8);
                b.textL(Dic.¤¤Used);
                foreach (Race r in RACES.all())
                {
                    if (r.home().clas(cl).amount(res.resource()) > 0)
                        b.add(r.appearance().icon);
                }
                b.NL();
            }

            base.hoverInfoGet(text);
        }

        private RES_AMOUNT res()
        {
            return RACES.res().homeResMax(cl)[ri];
        }
    }

    private class StatRowEquip : GuiSection
    {
        private readonly EquipCivic ss;
        private readonly HCLASS cl;
        private readonly Func<Race> race;

        public StatRowEquip(EquipCivic ss, HCLASS cl, Func<Race> race)
        {
            this.ss = ss;
            this.cl = cl;
            this.race = race;

            add(new StatRow.Arrow(ss.stat(), cl, race));
            addRightC(4, ss.resource.icon());

            INTE inTarget = new INTE()
            {
                min = () => 0,
                max = () => ss.max(),
                get = () => ss.target(cl, race()),
                set = (t) => ss.targetSet(t, cl, race())
            };

            addRightC(16, new GTarget(40, false, true, inTarget).hoverInfoSet(ss.sTarget));

            add(new GStat()
            {
                update = (text) => StatRow.format(text, ss.stat(), ss.stat().data(cl).getD(race()), cl, race())
            }, 230, 0);

            add(new StatRow.Meter(ss.stat(), cl, race), 320, 0);
            pad(2, 4);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (!isHoveringAHoverElement())
            {
                ss.hover(text, cl, race());
                text.NL();
                ss.stat().hover(text, cl, race());
            }
            else
            {
                base.hoverInfoGet(text);
            }
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
            GCOLOR.UI().border().render(r, body().x1(), body().x2(), body().y2() - 1, body().y2());
        }
    }
}