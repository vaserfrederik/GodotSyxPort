using System;
using System.IO;
using System.Text;
using snake2d;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;
using view.sett.ui.minimap;

namespace view.sett.ui.right
{
    public class UIPanelRightSett : Interrupter, SAVABLE
    {
        private static readonly CharSequence ¤¤hotspots = "Hot-spots";
        private static readonly CharSequence ¤¤minipanels = "Mini Panels";

        static
        {
            D.ts(typeof(UIPanelRightSett));
        }

        private readonly UIMiniResources resources;
        private readonly UIMiniHotSpots hs;
        private readonly UIMiniRaces species;
        private readonly Expansion[] all;
        private bool[] visable;
        private int[] widths = Alloc.ii(4);

        public UIPanelRightSett(UIMinimapSett top, InterManager i, GameWindow w)
        {
            desturberSet().persistantSet().pin();

            int y1 = top.y2();

            {
                this.resources = new UIMiniResources(1, y1);
                this.hs = new UIMiniHotSpots(2, y1, w);
                this.species = new UIMiniRaces(3, y1);
            }

            all = new Expansion[] {
                this.species,
                this.resources,
                this.hs,
            };
            visable = new bool[] {
                false,
                false,
                false,
                false,
            };

            this.hs.visableSet(true);
            this.resources.visableSet(true);
            this.species.visableSet(true);

            update(0);
            show(i);

            top.panel().padd(makeButt());
        }

        private CLICKABLE makeButt()
        {
            GuiSection s = new GuiSection();
            s.addDownC(0, exp(species, HCLASSES.CITIZEN().names));
            s.addDownC(0, exp(resources, Dic.¤¤Resource));
            s.addDownC(0, exp(hs, ¤¤hotspots));

            CLICKABLE c = new UIMinimapSett.Butt(SPRITES.icons().s.menu)
            {
                protected override void clickA()
                {
                    VIEW.inters().popup.show(s, this);
                }
            }.hoverInfoSet(¤¤minipanels);
            return c;
        }

        protected override void hoverTimer(GBox text)
        {
            foreach (Expansion e in all)
            {
                e.hoverInfoGet(text);
            }
        }

        protected override bool render(Renderer r, float ds)
        {
            int w = 0;
            foreach (Expansion e in all)
            {
                if (e.visableIs())
                {
                    w += e.body().width();
                    e.render(r, ds);
                }
            }

            if (w > 0)
                manager().viewPort().incrW(-w);

            return true;
        }

        protected override void mouseClick(MButt button)
        {
            hs.click();
            if (button == MButt.LEFT)
            {
                foreach (Expansion e in all)
                {
                    e.click();
                }
            }
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            bool h = false;
            foreach (Expansion e in all)
            {
                if (e.hover(mCoo))
                    h = true;
            }

            return h;
        }

        protected override bool update(float ds)
        {
            bool changed = false;
            int i = 0;
            foreach (RENDEROBJ r in all)
            {
                if (r.visableIs() != visable[i])
                {
                    changed = true;
                    visable[i] = r.visableIs();
                }
                if (widths[i] != r.body().width())
                {
                    widths[i] = r.body().width();
                    changed = true;
                }
                i++;
            }

            if (changed)
            {
                int x2 = C.WIDTH();
                for (i = all.Length - 1; i >= 0; i--)
                {
                    if (all[i].visableIs())
                    {
                        all[i].body().moveX2(x2);
                        x2 = all[i].body().x1();
                    }
                }
            }
            return true;
        }

        public void save(FilePutter file)
        {
            int i = 0;
            foreach (RENDEROBJ r in all)
            {
                i |= r.visableIs() ? 1 : 0;
                i = i << 1;
            }
            file.i(i);
        }

        public void load(FileGetter file) throws IOException
        {
            int i = file.i();
            int k = all.Length;
            foreach (RENDEROBJ r in all)
            {
                r.visableSet(((i >> k) & 1) == 1);
                k--;
            }
        }

        public void clear()
        {
            for (int i = 0; i < visable.Length; i++)
                visable[i] = true;
            hs.clear();
        }

        private abstract class Expansion : GuiSection
        {
            Expansion(int index)
            {
            }
        }

        private RENDEROBJ exp(Expansion s, CharSequence name)
        {
            if (s == null)
                throw new RuntimeException();
            CLICKABLE b = new GButt.ButtPanel(name)
            {
                protected override void clickA()
                {
                    s.visableSet(!s.visableIs());
                }

                protected override void renAction()
                {
                    selectedSet(s.visableIs());
                }
            }.setDim(140, 32);
            return b;
        }
    }
}