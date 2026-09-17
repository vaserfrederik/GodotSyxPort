using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;

namespace view.ui.profile
{
    public abstract class UIBonus : GuiSection
    {
        private readonly StringInputSprite inInput;
        private readonly GETTER<BOOSTABLE_O> bbb;
        public Faction f;

        public UIBonus(GETTER<BOOSTABLE_O> bbb, GETTER<Faction> f, int height)
        {
            this.bbb = bbb;
            inInput = new StringInputSprite(16, UI.FONT().M).placeHolder(Dic.¤¤Search);

            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            ArrayList<Boostable> all = new ArrayList<Boostable>(BOOSTING.ALL().size());
            foreach (Boostable b in BOOSTING.ALL())
            {
                if (Is(b))
                    all.Add(b);
            }

            AddRelBody(0, DIR.S, new GInput(inInput));

            BoostableCat cat = null;
            Row rr = new Row(BOOSTING.ALL().ElementAt(0));

            all.Sort((arg0, arg1) => ("" + arg0.cat.name).CompareTo("" + arg1.cat.name));

            foreach (Boostable b in all)
            {
                if (b.name == null || b.name.Length == 0)
                    continue;
                if (b.cat != cat)
                {
                    cat = b.cat;
                    rows.Add(new RENDEROBJ.RenderImp(rr.body().width(), rr.body().height())
                    {
                        GText h = new GText(UI.FONT().H2, b.cat.name).lablifySub(),
                        render = (r, ds) =>
                        {
                            h.renderCY(r, body().x1() + 20, body().cY());
                            GCOLOR.UI().border().render(r, body().x1(), body().x2(), body().y2() - 1, body().y2());
                        }
                    });
                }
                double min = b.min(null);
                double max = b.max(null);
                if (min == b.baseValue && max == b.baseValue)
                    continue;
                rows.Add(new Row(b));
            }

            AddRelBody(8, DIR.S, new GScrollRows(rows, height - body().height() - 16, 0)
            {
                passesFilter = (i, o) =>
                {
                    if (inInput.text().Length == 0)
                        return true;
                    if (o is Row)
                    {
                        Row r = (Row)o;
                        if (Str.containsText(r.bo.name, inInput.text()) || Str.containsText(r.bo.desc, inInput.text()))
                            return true;
                        return false;
                    }
                    else
                        return false;
                }
            }.view());

            AddRelBody(8, DIR.E, new Effects(f, height));
        }

        protected abstract bool Is(Boostable bo);

        private class Row : HOVERABLE.HoverableAbs
        {
            private readonly Boostable bo;
            private readonly SPRITE ico;

            public Row(Boostable bo) : base(450, 48)
            {
                this.bo = bo;
                ico = bo.icon.resized(32);
            }

            private readonly GText t = new GText(UI.FONT().S, 16);

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                double min = bo.min(bbb.get().getClass());
                double max = bo.max(bbb.get().getClass());
                if (min == bo.baseValue && max == bo.baseValue)
                    return;

                double d = bo.get(bbb.get());

                GMeter.renderDelta(r, bo.baseValue / max, d / max, body.x1(), body.x2() - 90, body.y1(), body.y2(), GMeter.C_GRAY);

                ico.renderCY(r, body.x1() + 16, body.cY());

                int w = UI.FONT().M.width(bo.name);
                OPACITY.O50.bind();
                COLOR.BLACK.render(r, body.x1() + 46, body.x1() + 50 + w + 8, body.y1() + 10, body.y2() - 10);
                OPACITY.unbind();

                UI.FONT().M.renderCY(r, body.x1() + 50, body.cY(), bo.name);

                t.clear();
                if (min == bo.baseValue && max == bo.baseValue)
                    return;
                if (bo.baseValue == 0)
                    GFORMAT.percInc(t, bo.get(bbb.get()));
                else
                    GFORMAT.percInc(t, bo.get(bbb.get()) / bo.baseValue - 1.0);
                t.renderCY(r, body.x2() - 80, body.cY());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(bo.name);
                text.text(bo.desc);
                text.NL(8);
                bo.hoverDetailed(text, bbb.get(), null, true);
            }
        }

        private class Effects : RENDEROBJ.RenderImp
        {
            private readonly GETTER<Faction> fa;

            public Effects(GETTER<Faction> fa, int height) : base(400, height)
            {
                this.fa = fa;
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                GBox.tmp.clear();
                GBox.tmp.maxWidth = body.width();
                GBox.tmp.maxHeight = body.height();
                GAME.BOOST().hover(GBox.tmp, fa.get());
                GBox.tmp.renderWithout(r, body.x1(), body.y1());
            }
        }
    }
}