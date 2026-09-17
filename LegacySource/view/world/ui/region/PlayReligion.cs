using System;
using System.Collections.Generic;
using Util.Data.Getter;
using Util.Gui.Misc;
using Util.Gui.Renderable;
using Util.Info;
using Util.Text;
using World.Map.Regions;
using World.Region;
using World.Region.RDReligions;

namespace View.World.UI.Region
{
    final class PlayReligion : GuiSection
    {
        private GetterImp<Region> g;

        public PlayReligion(GetterImp<Region> g, int W)
        {
            this.g = g;
            int i = 0;

            int w = 90;
            int cols = W / w;

            foreach (RDReligion reg in RD.RELIGION().All())
                AddGridD(Rel(reg), i++, cols, w, 34, DIR.W);

            i++;
            AddGridD(new GStat
            {
                Update = (GText text) =>
                {
                    GFORMAT.PercInv(text, RD.RELIGION().opposition.Get(g.Get()));
                },
                HoverInfoGet = (GBox b) =>
                {
                    STATS.RELIGION().OPPOSITION.Info().Hover(b);
                }
            }.Hh(UI.Icons().M.Cancel), i++, cols, w, 34, DIR.W);

            Pad((W - Body().Width()) / 2, 0);
        }

        private RenderObj Rel(RDReligion rel)
        {
            GuiSection s = new GuiSection
            {
                HoverInfoGet = (GUI_BOX text) =>
                {
                    rel.Religion.Info.Hover(text);
                    GBox b = (GBox)text;
                    b.Sep();
                    rel.Boost.Hover(b, g.Get(), Dic.¤¤Spread, false);
                    b.Sep();

                    rel.Boosts.Hover(text, g.Get());

                    b.Sep();
                    b.TextSLL(STATS.RELIGION().OPPOSITION.Info().Name);
                    b.NL();
                    int tab = 0;
                    foreach (Religion o in RELIGIONS.ALL())
                    {
                        b.Tab(tab);
                        b.Text(o.Info.Name);
                        b.Tab(tab + 6);
                        b.Add(GFORMAT.PercInv(b.Text(), rel.Religion.Opposition(o)));
                        tab += 8;
                        if (tab > 8)
                        {
                            b.NL();
                            tab = 0;
                        }
                    }

                    base.HoverInfoGet(text);
                }
            };

            s.Add(new SPRITE.Imp(48, 14)
            {
                Render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                {
                    double now = rel.Current.Get(g.Get());
                    double t = rel.Target(g.Get());
                    GMeter.Render(r, GMeter.C_REDPURPLE, now, t, X1, X2, Y1, Y2);
                }
            }, 0, 0);

            s.AddCentredY(rel.Religion.Icon, -30);

            return s;
        }
    }
}