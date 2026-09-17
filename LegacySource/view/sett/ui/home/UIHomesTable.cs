using System;
using System.Collections.Generic;
using init.sprite;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;

namespace view.sett.ui.home
{
    final class UIHomesTable : GuiSection
    {
        private static CharSequence ¤¤Housed = "¤Housed";
        private static CharSequence ¤¤HousedD = "¤Subjects that have a home. There might be a small delay between building new houses and having people move in.";
        private static CharSequence ¤¤Homeless = "¤Homeless";
        private static CharSequence ¤¤HomelessD = "¤Subjects that have looked for housing, yet have not found one. Oddjobbers will search for houses across the whole map. Employed people will search in the vicinity of their workplace.";
        private static CharSequence ¤¤HousingTotal = "¤Total Housing";
        private static CharSequence ¤¤HousingAvailable = "¤Available Housing";
        private static CharSequence ¤¤HousingAvailableD = "¤Available Housing of this type across the map. Note that these houses might be beyond the reach of employed people.";
        private static CharSequence ¤¤ClickToGoToFirstHomeless = "¤Click to go to a subject that has trouble finding a home.";
        private static CharSequence ¤¤FurnishClick = "¤Click to manage furnishing.";

        Humanoid subject;
        private int hi = 0;

        static
        {
            D.ts(typeof(UIHomesTable));
        }

        public UIHomesTable(int HEIGHT)
        {
            Data housed = new Data(¤¤Housed, ¤¤HousedD)
            {
                GText format(GText t, HGROUP h) override
                {
                    return GFORMAT.i(t, STATS.HOME().GETTER.stat().data(h.type).get(h.race));
                }
            };

            Data homeless = new Data(¤¤Homeless, ¤¤HomelessD)
            {
                GText format(GText t, HGROUP h) override
                {
                    int am = STATS.HOME().GETTER.hasSearched.data(h.type).get(h.race);
                    GFORMAT.i(t, am);
                    if (am > 0)
                        t.errorify();
                    return t;
                }
            };

            Data available = new Data(¤¤HousingAvailable, ¤¤HousingAvailableD)
            {
                GText format(GText t, HGROUP h) override
                {
                    int am = SETT.ROOMS().HOME.total(h) - SETT.ROOMS().HOME.used(h);
                    return GFORMAT.i(t, am);
                }
            };

            Data total = new Data(¤¤HousingTotal, "")
            {
                GText format(GText t, HGROUP h) override
                {
                    int am = SETT.ROOMS().HOME.total(h);
                    return GFORMAT.i(t, am);
                }
            };

            Data furnishing = new Data(STATS.HOME().materials.info().name, STATS.HOME().materials.info().desc)
            {
                GText format(GText t, HGROUP h) override
                {
                    return GFORMAT.perc(t, STATS.HOME().materials.data(h.type).getD(h.race));
                }
            };

            GTableBuilder bu = new GTableBuilder()
            {
                int nrOFEntries() override
                {
                    return HGROUP.all().size();
                }

                private void hover(GBox box, HGROUP h, Data d)
                {
                    box.textL(d.name);
                    box.tab(5);
                    box.add(d.format(box.text(), h));
                    box.NL();
                    box.text(d.desc);
                    box.NL(4);
                }

                void hoverInfo(int index, GBox box) override
                {
                    HGROUP h = HGROUP.all().get(index);
                    box.title(h.name);

                    hover(box, h, homeless);
                    hover(box, h, housed);
                    hover(box, h, available);
                    hover(box, h, total);
                    hover(box, h, furnishing);
                }

                bool activeIs(int index) override
                {
                    HGROUP h = HGROUP.all().get(index);
                    return STATS.POP().POP.data(h.type).get(h.race) > 0;
                }
            };

            GRowBuilder b;
            int size = 90;

            b = new GRowBuilder()
            {
                RENDEROBJ build(GETTER<int> ier) override
                {
                    return new HOVERABLE.Sprite(Icon.M + 4)
                    {
                        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                        {
                            HGROUP h = HGROUP.all().get(ier.get());
                            h.icon.render(r, body.x1() + 2, body.y1() + 2);
                        }
                    };
                }
            };
            bu.column(null, 48, b);

            bu.column(homeless.name, size, row(homeless));
            bu.column(housed.name, size, row(housed));

            b = new GRowBuilder()
            {
                RENDEROBJ build(GETTER<int> ier) override
                {
                    GStat a = new GStat()
                    {
                        public override void update(GText text)
                        {
                            HGROUP h = HGROUP.all().get(ier.get());
                            available.format(text, h);
                        }
                    };
                    GStat b = new GStat()
                    {
                        public override void update(GText text)
                        {
                            text.add('(');
                            HGROUP h = HGROUP.all().get(ier.get());
                            total.format(text, h);
                            text.add(')');
                        }
                    };

                    return new RENDEROBJ.RenderImp(20, b.height())
                    {
                        public override void render(SPRITE_RENDERER r, float ds)
                        {
                            a.render(r, body.x1(), body.y1());
                            b.render(r, body.x1() + 60, body.y1());
                        }
                    };
                }
            };
            bu.column(Dic.¤¤Available, size + 40, b);

            b = new GRowBuilder()
            {
                RENDEROBJ build(GETTER<int> ier) override
                {
                    GuiSection s = new GuiSection();

                    GStat a = new GStat()
                    {
                        public override void update(GText text)
                        {
                            HGROUP h = HGROUP.all().get(ier.get());
                            furnishing.format(text, h);
                        }
                    };

                    GButt b = new GButt.Glow(SPRITES.icons().s.cog)
                    {
                        protected override void clickA()
                        {
                            HGROUP h = HGROUP.all().get(ier.get());

                            if (h.type == HCLASSES.CITIZEN())
                            {
                                VIEW.s().ui.standing.openAccess(h.race);
                            }

                            base.clickA();
                        }

                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            HGROUP h = HGROUP.all().get(ier.get());
                            if (h.type != HCLASSES.NOBLE())
                                text.text(¤¤FurnishClick);
                            text.NL(8);

                            base.hoverInfoGet(text);
                        }
                    };

                    s.add(b);
                    s.addRightC(2, a.r());

                    return s;
                }
            };
            bu.column(furnishing.name, size + 20, b);

            b = new GRowBuilder()
            {
                RENDEROBJ build(GETTER<int> ier) override
                {
                    return new GStat()
                    {
                        public override void update(GText text)
                        {
                            HGROUP h = HGROUP.all().get(ier.get());
                            data.format(text, h);
                        }
                    }.r(DIR.NW);
                }
            };

            add(bu.createHeight(HEIGHT, true));
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
            if (subject != null)
            {
                VIEW.s().getWindow().centerer.set(subject.body().cX(), subject.body().cY());
                SETT.OVERLAY().add(subject);
            }
        }

        private void search(HGROUP t)
        {
            ENTITY[] ee = SETT.ENTITIES().getAllEnts();

            for (int i = 0; i < ee.Length; i++)
            {
                if (hi >= ee.Length)
                    hi = 0;
                ENTITY e = SETT.ENTITIES().getAllEnts()[hi];
                hi++;
                if (e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;

                    if (STATS.HOME().GETTER.hasSearched.indu().get(h.indu()) == 0)
                        continue;

                    if (t == HGROUP.get(h))
                    {
                        subject = h;
                        return;
                    }
                }
            }
        }

        private abstract class Data
        {
            public readonly CharSequence name;
            public readonly CharSequence desc;

            public Data(CharSequence name, CharSequence desc)
            {
                this.name = name;
                this.desc = desc;
            }

            public abstract GText format(GText t, HGROUP h);
        }

        private static GRowBuilder row(Data data)
        {
            return new GRowBuilder()
            {
                RENDEROBJ build(GETTER<int> ier) override
                {
                    return new GStat()
                    {
                        public override void update(GText text)
                        {
                            HGROUP h = HGROUP.all().get(ier.get());
                            data.format(text, h);
                        }
                    }.r(DIR.NW);
                }
            };
        }
    }
}