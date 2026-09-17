using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using game.boosting;
using game.faction;
using init.sprite.UI;
using init.tech;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;

namespace view.ui.tech
{
    internal class Search : GuiSection
    {
        private static readonly string ¤¤no = "No results";

        static Search()
        {
            D.ts(typeof(Search));
        }

        private readonly Node[] nodes;
        private readonly RENDEROBJ no;
        private readonly int width;
        private readonly int height;

        public Search(int height, int width)
        {
            this.width = width;
            this.height = height;
            NodeBoosts bos = new NodeBoosts();
            nodes = new Node[TECHS.ALL().Count];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node(TECHS.ALL()[i], bos);
            }
            body().setDim(width, height);
        }

        public GuiSection Set(string s)
        {
            int x1 = body().x1();
            int y1 = body().y1();
            clear();

            int x = 0;
            int y = 0;

            foreach (TECH t in TECHS.ALL())
            {
                if (!Contains(t, s))
                    continue;

                Node n = nodes[t.index()];

                if (x + 8 + Node.WIDTH > width)
                {
                    y += 8 + Node.HEIGHT();
                    if (y + 8 + Node.HEIGHT() > height)
                        break;
                }

                n.body().moveX1Y1(x, y);
                add(n);
                x += 16 + Node.WIDTH;
            }

            body().moveX1Y1(x1, y1);
            body().setDim(width, height);

            if (x == 0 && y == 0)
            {
                add(no, body().cX(), body().cY());
            }

            return this;
        }

        private bool Contains(TECH t, string s)
        {
            if (Str.containsText(t.name(), s))
                return true;
            if (Str.containsText(t.desc(), s))
                return true;

            foreach (BoostSpec b in t.boosters.all())
            {
                if (Str.containsText(s, b.boostable.name))
                    return true;
            }

            //for (Lock<Faction> b : t.requires.all()) {
            //    if (Str.containsText(s, b.lockable.name))
            //        return true;
            //}

            return false;
        }
    }
}