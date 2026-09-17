using System;
using System.Collections.Generic;
using game.boosting;
using game.faction.player;
using init.sprite.UI;
using init.tech;
using init.value;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using view.main;

namespace view.ui.tech
{
    class TechTest : HoverableAbs
    {
        public static GuiSection Get()
        {
            GuiSection s = new GuiSection();
            TechTest t = new TechTest();
            int i = 0;
            int y1 = 0;
            int x1 = 0;
            foreach (Plot p in t.plots)
            {
                GButt.Checkbox c = new GButt.Checkbox(p.blue.icon.small)
                {
                    ClickA = () =>
                    {
                        p.show = !p.show;
                    },
                    RenAction = () =>
                    {
                        SelectedSet(p.show);
                    },
                    HoverInfoGet = text =>
                    {
                        GBox b = (GBox)text;
                        double bo = 0;
                        double cost = 0;
                        foreach (Line l in p.lines)
                        {
                            b.text(l.name);
                            b.add(GFORMAT.f0(b.text(), l.bo));
                            b.add(GFORMAT.iIncr(b.text(), (int)l.cost));
                            bo += l.bo;
                            cost += l.cost;
                            text.NL();
                        }
                        b.add(GFORMAT.f0(b.text(), bo));
                        b.add(GFORMAT.iIncr(b.text(), (int)cost));
                    }
                };
                s.add(c, x1, y1);
                if (i++ > 10)
                {
                    y1 = s.body().y2();
                    i = 0;
                    x1 = 0;
                }
                else
                    x1 += c.body().width();
            }

            GuiSection ff = new GuiSection();
            ff.add(t);
            ff.addRelBody(80, DIR.E, new GText(UI.FONT().S, "y = boost, x = cost"));
            s.addRelBody(16, DIR.S, ff);

            return s;
        }

        static int width = 600;
        static int height = 600;
        private static readonly double COSTI = 1.0 / 1000.0;
        private static readonly double BI = 1.0 / 6.0;

        private readonly ArrayListGrower<Plot> plots = new ArrayListGrower<>();

        private TechTest()
        {
            foreach (RoomBlueprint b in SETT.ROOMS().all())
            {
                if (b is INDUSTRY_HASER)
                {
                    INDUSTRY_HASER i = (INDUSTRY_HASER)b;
                    if (i.industries().get(0).outs().size() > 0)
                    {
                        Plot p = new Plot(i.industries().get(0).blue);
                        if (p.prevCost > 0)
                            plots.add(p);
                    }
                }
            }

            body().setDim(width, height);
        }

        protected override void Render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            foreach (Plot p in plots)
                p.render(r);
            GButt.ButtPanel.renderFrame(r, body);
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            int dx = VIEW.mouse().x() - body.x1();
            int dy = height - VIEW.mouse().y() - body.y1();
            GBox b = (GBox)text;
            b.add(b.text().add('x').add(dx / COSTI / width));
            b.add(b.text().add('y').add(dy / BI / height));
        }

        private class Plot
        {
            private readonly RoomBlueprintImp blue;
            private double prevBo = 0;
            private double prevCost = 0;
            private double bo = 0;
            private double cost = 0;
            private readonly COLOR col;
            string tt = "";
            private readonly ArrayListGrower<Line> lines = new ArrayListGrower<>();
            private bool show = false;

            Plot(RoomBlueprintImp blue)
            {
                this.blue = blue;
                col = COLOR.UNIQUE.getC(blue.index());
                Boostable bo = blue.bonus();
                if (bo == null)
                    return;

                Bitmap1D map = new Bitmap1D(TECHS.ALL().size(), false);
                Bitmap1D mapN = new Bitmap1D(TECHS.ALL().size(), false);
                bool has = true;
                int ii = 0;
                while (has)
                {
                    mapN.clear();
                    flush();
                    has = false;
                    outer:
                    foreach (TECH t in TECHS.ALL())
                    {
                        if (map.get(t.index()))
                            continue;
                        foreach (TechRequirement re in t.requires())
                        {
                            if (!map.get(re.tech.index()))
                                continue outer;
                        }
                        has = true;
                        mapN.set(t.index(), true);
                        add(t, ii);
                    }
                    foreach (TECH t in TECHS.ALL())
                    {
                        if (mapN.get(t.index()))
                            map.set(t.index(), true);
                    }
                    ii++;
                }
            }

            private void add(TECH t, int ii)
            {
                if (t.AIAmount == 0)
                    return;

                double b = boost(t) + tool(t) + upgrade(t);

                bool ll = false;
                foreach (Lock l in t.lockers.all())
                {
                    if (l.lockable.key.Equals("ROOM_" + blue.key))
                    {
                        ll = true;
                    }
                }

                if (b > 0 || ll)
                {
                    if (tt.Length > 0)
                        tt += " + ";
                    else
                        tt += " ";

                    tt += " " + t.name() + "(" + cost(t) + "," + (int)(b * 100) / 100.0 + ")";
                    bo += b;
                    cost += cost(t);
                }
            }

            private double cost(TECH t)
            {
                double c = 0;
                foreach (TechCost tc in t.costs)
                {
                    c += PTech.costTotal(tc, t, t.levelMax);
                }
                return c;
            }

            private double boost(TECH t)
            {
                foreach (BoostSpec s in t.boosters.all())
                {
                    if (SETT.ROOMS().bonus.get(s.boostable) == blue)
                    {
                        return s.booster.to() * t.levelMax;
                    }
                }

                return 0;
            }

            private double tool(TECH t)
            {
                foreach (BoostSpec s in t.boosters.all())
                {
                    if (SETT.ROOMS().employment.equip.boostToTarget(s.boostable) != null)
                    {
                        Target ta = SETT.ROOMS().employment.equip.boostToTarget(s.boostable);
                        if (ta.blue == blue)
                        {
                            return 0.25 * 0.75 * t.levelMax * s.booster.to();
                        }
                    }
                }
                return 0;
            }

            private double upgrade(TECH t)
            {
                foreach (Lock l in t.lockers.all())
                {
                    string n = l.lockable.key;

                    if (n.Contains(blue.key + "_UPGRADE_"))
                    {
                        string[] nn = n.Split("_UPGRADE_");
                        if (nn.Length == 2)
                        {
                            RoomBlueprintImp b = (RoomBlueprintImp)SETT.ROOMS().collection.tryGet(nn[0].Replace("ROOM_", ""));
                            if (b == blue)
                            {
                                return 1; // Assuming some value for upgrade
                            }
                        }
                    }
                }
                return 0;
            }

            private void flush()
            {
                if (tt.Length > 0)
                {
                    Line l = new Line(tt, startX, startY, bo, cost);
                    tt = "";
                    lines.add(l);

                    prevBo += bo;
                    prevCost += cost;
                    bo = 0;
                    cost = 0;
                }
            }

            private void render(SPRITE_RENDERER r)
            {
                if (!show)
                    return;
                foreach (Line l in lines)
                    l.render(r, body.x1(), body.y2(), col);
            }
        }

        private static class Line
        {
            private readonly VectorImp vec = new VectorImp();
            private readonly int mag;
            private readonly double startX;
            private readonly double startY;
            private readonly string name;
            public readonly double bo;
            public readonly double cost;

            public Line(string name, double startX, double startY, double bo, double cost)
            {
                this.name = name;
                this.startX = startX;
                this.startY = startY;
                mag = (int)Math.Ceiling(vec.set(cost * COSTI * width, bo * BI * height));
                this.bo = bo;
                this.cost = cost;
            }

            public void render(SPRITE_RENDERER r, int x1, int y1, COLOR col)
            {
                for (int i = 0; i < mag; i++)
                {
                    int x = (int)(x1 + startX + vec.nX() * i);
                    int y = (int)(y1 - (startY + vec.nY() * i));
                    if (i == 0)
                    {
                        col.render(r, x, x + 3, y, y + 3);
                    }
                    else
                        col.render(r, x, x + 1, y, y + 1);
                }
            }
        }
    }
}