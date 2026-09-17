using System.Collections.Generic;
using game.boosting;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.stats;
using settlement.stats.colls;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;

namespace view.sett.ui.standing
{
    class MenuProp : ISidePanel
    {
        private readonly HCLASS c;
        private readonly GETTER<Race> race;

        public MenuProp(HCLASS c, GETTER<Race> race)
        {
            titleSet(Dic.¤¤Properites);
            this.race = race;
            this.c = c;
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (BoostableCat col in BOOSTABLES.colls())
            {
                rows.Add(new GHeader(col.name));

                foreach (Boostable bo in col.all())
                {
                    rows.Add(new Row(bo));
                }

                rows.Add(new RENDEROBJ.RenderDummy(10, 16));
            }

            rows.Add(new GHeader(STATS.TRAITS().info.names));

            StatsTraits pp = STATS.TRAITS();

            foreach (StatTrait p in pp.all())
            {
                rows.Add(new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.perc(text, p.getD(c, race.get()));
                    }

                    public override void hoverInfoGet(GBox b)
                    {
                        b.title(p.trait.info.name);
                        b.text(p.trait.info.desc);
                        b.NL();
                        b.add(GFORMAT.i(b.text(), p.get(c, race.get())));
                    }
                }.hh(p.trait.info.name, 150));
            }

            section.add(new GScrollRows(rows, HEIGHT).view());
        }

        private class Row : GuiSection
        {
            private readonly Boostable bo;

            public Row(Boostable bo)
            {
                this.bo = bo;

                add(bo.icon, 0, 0);
                addRightC(2, new GText(UI.FONT().M, bo.name));
                addRightCAbs(250, new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.f(text, bo.get(HCLASS_RACE.clP(race.get(), c)));
                    }
                });
                body().incrW(64);
                pad(2, 2);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                if (hoveredIs())
                    COLOR.WHITE15.render(r, body());
                base.render(r, ds);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.title(bo.name);
                b.text(bo.desc);
                b.NL(8);

                bo.hover(b, HCLASS_RACE.clP(race.get(), c), true);
            }
        }

        protected override void update(float ds)
        {
        }
    }
}