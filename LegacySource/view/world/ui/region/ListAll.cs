using System;
using System.Collections.Generic;
using System.Linq;

namespace View.World.UI.Region
{
    class ListAll : ISidePanel
    {
        private readonly int width = C.SG * 400;
        private readonly int height = C.SG * 30;

        static ListAll()
        {
            D.gInit(new ListAll());
        }

        private readonly GTableSorter<Region> sorter = new GTableSorter<Region>(WREGIONS.MAX)
        {
            GetUnsorted = index =>
            {
                Region f = WORLD.REGIONS().GetByIndex(index);
                if (f.info.Area() > 0 && !(f.faction() == FACTIONS.Player() && f.Capitol()))
                    return f;
                return null;
            }
        };

        private readonly StringInputSprite s;

        private readonly GTFilter<Region> filterName = new GTFilter<Region>(D.g("search"))
        {
            Passes = h => h.info.Name().StartsWithIgnoreCase(s.text())
        };

        private readonly GTSort<Region> sort = new GTSort<Region>("blabla")
        {
            Cmp = (current, cmp) => get(current) - get(cmp),

            get = current =>
            {
                int m = WREGIONS.MAX * FACTIONS.MAX();
                int res = current.Index();
                Faction f = current.faction();
                if (f == null)
                {
                    res += 3 * m;
                }
                else if (f != FACTIONS.Player())
                {
                    res += m;
                    res += WREGIONS.MAX * f.Index();
                }
                return res;
            },

            Format = (h, text) =>
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly GTableBuilder builder;

        public ListAll()
        {
            sorter.SetSort(sort);

            s = new StringInputSprite(8, UI.FONT().H2)
            {
                Change = () => sorter.SetFilter(filterName)
            }.PlaceHolder(filterName.Name);

            GInput input = new GInput(s);

            section.Add(input);

            builder = new GTableBuilder
            {
                NrOFEntries = () => sorter.Size()
            };

            builder.Column(null, width, new GRowBuilder
            {
                Build = ier => new Button(ier)
            });

            GuiSection section = builder.CreateHeight(HEIGHT - input.Body.Height() - C.SG * 4, true);
            this.section.AddDown(2, section);

            titleSet(D.g("Regions"));
        }

        protected override void update(float ds)
        {
            sorter.Sort();
        }

        private readonly GText textH = new GText(UI.FONT().H2, 30);
        private readonly GText textS = new GText(UI.FONT().S, 30);

        private class Button : ClickableAbs
        {
            private readonly GETTER<Integer> ier;

            public Button(GETTER<Integer> ier)
            {
                this.ier = ier;
                body.setWidth(width);
                body.setHeight(height);
            }

            protected override void clickA()
            {
                Region reg = sorter.Get(ier);
                VIEW.world().window.centererTile.set(reg.cx(), reg.cy());
                ISidePanel p = VIEW.world().UI.regions.Get(reg);
                VIEW.world().panels.Add(this, true);
                VIEW.world().panels.Add(p, false);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                Region f = sorter.Get(ier);
                selectedSet(VIEW.world().UI.regions.active(f));
                GCOLOR.UI().border().render(r, body());

                GButt.ButtPanel.RenderBG(r, isActive, isSelected, isHovered, body);

                COLOR col = COLOR.WHITE85;
                if (f.faction() == null)
                {
                    FBanner.rebel.MEDIUM.renderCY(r, 8, body().cY());
                }
                else
                {
                    f.faction().banner().MEDIUM.renderCY(r, 8, body().cY());
                    if (f.capitol())
                    {
                        UI.icons().s.crown.renderCY(r, 6, body.cY() - 8);
                    }
                    if (f.faction() == null)
                    {
                        col = GCOLOR.T().INORMAL;
                    }
                    else if (DIP.WAR().is(FACTIONS.Player(), f.faction()))
                    {
                        col = GCOLOR.T().IBAD;
                    }
                    else
                    {
                        col = GCOLOR.T().IGOOD;
                    }
                }

                textH.Clear();
                textH.color(col);
                textH.add(f.info.name());
                textH.setMaxWidth(width - 60);
                textH.setMultipleLines(false);

                textH.renderCY(r, 40, body().cY());

                textS.Clear();
                GFORMAT.i(textS, RD.RACES().population.get(f));
                textS.adjustWidth();
                textS.renderCY(r, body.x2() - 8 - textS.width(), body.cY());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                Region f = sorter.Get(ier);
                VIEW.world().UI.regions.hover(f, text);
            }
        }

        public ISidePanel Get(Region f)
        {
            sorter.SortForced();
            if (f != null)
            {
                builder.Set(sorter.GetIndex(f));
            }

            return this;
        }
    }
}