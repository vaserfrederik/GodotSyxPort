using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;

namespace view.ui.tech
{
    final class InfoBonuses : GuiSection
    {
        private readonly int WIDTH = 300;
        private readonly UITechTree tree;

        public InfoBonuses(UITechTree tree, int height, int width) : base(height, width)
        {
            this.tree = tree;
            int cc = (int)Math.Ceiling(width / (WIDTH + 48.0));

            List<List<Object>> cols = new List<List<Object>>(cc);
            int max = 0;
            while (cols.Count < cc)
                cols.Add(new List<Object>());

            {
                Dictionary<string, BoostableCat> map = new Dictionary<string, BoostableCat>();
                foreach (Boostable b in BOOSTING.ALL())
                {
                    if (!map.ContainsKey(b.cat.prefix))
                        map.Add(b.cat.prefix, b.cat);
                }

                List<BoostableCat> cats = map.Values.ToList();
                cats.Sort((o1, o2) => o2.all().Count - o1.all().Count);

                foreach (BoostableCat c in cats)
                {
                    List<Object> current = null;
                    foreach (List<Object> l in cols)
                    {
                        if (current == null || l.Count < current.Count)
                            current = l;
                    }
                    if (current.Count > 0)
                        current.Add((Object)null);
                    current.Add(c);
                    foreach (Boostable b in c.all())
                        current.Add(b);
                    max = Math.Max(current.Count, max);
                }
            }

            List<RENDEROBJ> rens = new List<RENDEROBJ>();

            for (int i = 0; i < max; i++)
            {
                GuiSection row = new GuiSection();
                int ri = 0;
                foreach (List<Object> l in cols)
                {
                    RENDEROBJ r = null;
                    if (l.Count <= i)
                    {
                        r = new RENDEROBJ.RenderDummy(WIDTH, 18);
                    }
                    else
                    {
                        Object o = l[i];
                        if (o == null)
                        {
                            r = new RENDEROBJ.RenderDummy(WIDTH, 18);
                        }
                        else if (o is BoostableCat)
                        {
                            GTextR R = new GTextR(UI.FONT().S, ((BoostableCat)o).name);
                            R.setColor(GCOLOR.T().H1);
                            r = R;
                        }
                        else
                        {
                            r = new Boo((Boostable)o);
                        }
                    }
                    row.add(r, WIDTH * ri, body().y1());
                    ri++;
                }
                rens.Add(row);
            }

            add(new GScrollRows(rens, height - 8).view());
        }

        private readonly GText t = new GText(UI.FONT().S, 20);

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
        }

        private class Boo : ClickableAbs
        {
            private readonly Boostable bo;
            private List<TECH> techs = new List<TECH>();

            public Boo(Boostable bo) : base(WIDTH, 18)
            {
                this.bo = bo;
                foreach (TECH t in TECHS.ALL())
                {
                    foreach (BoostSpec b in t.boosters.all())
                    {
                        if (b.boostable == bo)
                            techs.Add(t);
                    }
                }
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                if (techs.Count == 0)
                    OPACITY.O50.bind();
                else if (!isHovered)
                    OPACITY.O85.bind();

                bo.icon.render(r, body().x1(), body().y1());

                if (techs.Count > 0)
                    GCOLOR.T().H1.bind();
                UI.FONT().S.render(r, bo.name, body().x1() + 20, body().y1(), 0, bo.name.Length > 15 ? 15 : bo.name.Length, 1);
                COLOR.unbind();
                OPACITY.unbind();
                t.clear();
                double add = 0;
                double mul = 1;
                foreach (TECH t in techs)
                {
                    foreach (BoostSpec b in t.boosters.all())
                    {
                        if (b.boostable == bo)
                        {
                            if (b.booster.isMul)
                                mul *= FACTIONS.player().tech.level(t) * (b.booster.to() - 1) + 1;
                            else
                                add += FACTIONS.player().tech.level(t) * (b.booster.to());
                        }
                    }
                }

                GFORMAT.percInc(t, (add + 1) * mul - 1);
                t.render(r, body.x1() + 220, body.y1());
            }

            protected override void clickA()
            {
                if (techs.Count == 0)
                    return;
                tree.filter.set(bo.name);
                base.clickA();
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(bo.name);
                text.text(bo.desc);
                text.NL(8);

                GBox box = (GBox)text;
                foreach (TECH t in techs)
                {
                    foreach (BoostSpec b in t.boosters.all())
                    {
                        if (b.boostable == bo)
                        {
                            if (b.booster.isMul)
                                b.booster.hover(box, FACTIONS.player().tech.level(t) * (b.booster.to() - 1) + 1);
                            else
                                b.booster.hover(box, FACTIONS.player().tech.level(t) * (b.booster.to()));
                        }
                    }
                    box.NL();
                }
            }
        }
    }
}