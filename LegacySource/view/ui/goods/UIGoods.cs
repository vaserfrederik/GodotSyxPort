using System;
using System.Collections.Generic;
using snake2d;
using util.gui.misc;
using util.gui.table;
using util.text;

namespace view.ui.goods
{
    public sealed class UIGoods : IFullView
    {
        private const int COLS = 2;
        private readonly List<object> all = new List<object>(RESOURCES.ALL().Count * 2 + 1);

        public readonly Icon icon = SPRITES.icons().s.storage;
        private static readonly CharSequence ¤¤Name = "¤Goods";

        static
        {
            D.ts(typeof(UIGoods));
        }

        private readonly GTableBuilder bu;
        private RESOURCE flashRes;
        private double flashTime;
        private readonly GInput filter = new GInput(new StringInputSprite(24, UI.FONT().S).placeHolder(Dic.¤¤Search));

        public UIGoods() : base(¤¤Name, UI.icons().l.crate)
        {
            section = new GuiSection
            {
                Render = (ren, ds) =>
                {
                    update();
                    base.Render(ren, ds);
                }
            };

            section.body().setWidth(WIDTH).setHeight(1);
            section.addRelBody(8, DIR.S, filter);

            bu = new GTableBuilder
            {
                NrOFEntries = () => (int)Math.Ceiling((double)all.Count / COLS)
            };

            Pop pop = new Pop();
            int wi = new Entry(null, 0, pop).body().width();
            bu.column(null, wi, new GRowBuilder
            {
                Build = ier => new Entry(ier, 0, pop)
            });
            bu.column(null, wi, new GRowBuilder
            {
                Build = ier => new Entry(ier, 1, pop)
            });

            section.addRelBody(8, DIR.S, bu.createHeight(HEIGHT - section.body().height() - 8, false));
        }

        private void update()
        {
            all.Clear();

            CharSequence f = filter.text();

            if (f.Length == 0 || Str.containsText(Dic.¤¤Total, f))
                all.Add(null);

            int cat = RESOURCES.ALL()[0].category;
            foreach (RESOURCE r in RESOURCES.ALL())
            {
                if (f.Length == 0 || Str.containsText(r.name, f) || Str.containsText(r.names, f))
                {
                    if (r.category != cat)
                    {
                        if ((all.Count & 1) == 1)
                            all.Add(icon);
                        cat = r.category;
                    }
                    all.Add(r);
                }
            }
        }

        public void detail(RESOURCE res, Faction f)
        {
            activate();
            int ta = all.IndexOf(res);
            if (ta == -1)
                ta = 0;

            bu.set(ta / 2);
            flashRes = res;
            flashTime = VIEW.renderSecond();
        }

        public override void activate()
        {
            flashRes = null;
            filter.text().clear();
            filter.focus();
            update();
            base.activate();
        }

        private class Entry : GuiSection
        {
            private readonly GETTER<int> ier;
            private readonly GETTER<RESOURCE> res;
            private readonly int off;

            public Entry(GETTER<int> ier, int off, Pop pop) : base()
            {
                this.off = off;
                this.ier = ier;
                this.res = new GETTER<RESOURCE>
                {
                    Get = () =>
                    {
                        int i = ier.Get() * COLS + off;
                        if (i >= all.Count || all[i] == null || all[i] == icon)
                            return null;
                        return (RESOURCE)all[i];
                    }
                };

                addRightCAbs(56, new Row(res, pop));

                addRelBody(2, DIR.S, new RENDEROBJ.RenderImp(body().width(), 6)
                {
                    Render = (r, ds) =>
                    {
                        GCOLOR.UI().border().render(r, body().x1(), body().x2(), body().cY(), body().cY() + 1);
                    }
                });

                pad(6, 0);
            }

            public override void render(SPRITE_RENDERER rr, float ds)
            {
                int i = ier.Get() * COLS + off;
                if (i >= all.Count || all[i] == icon)
                    return;

                if (res.Get() != null && flashRes == res.Get() && VIEW.renderSecond() - flashTime < 3)
                {
                    COLOR.WHITE2WHITE.render(rr, body(), -1);
                }
                else if (hoveredIs())
                {
                    OPACITY.O012.bind();
                    COLOR.WHITE100.render(rr, body(), -1);
                    OPACITY.unbind();
                }
                base.render(rr, ds);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(res.Get() == null ? Dic.¤¤Total : res.Get().name);
                base.hoverInfoGet(text);
            }

            public override bool hover(COORDINATE mCoo)
            {
                int i = ier.Get() * COLS + off;
                if (i >= all.Count || all[i] == icon)
                    return false;
                return base.hover(mCoo);
            }
        }
    }
}