using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.gui.table;
using util.text;

namespace view.sett.ui.room.priority
{
    class Filter<T> : GButt.ButtPanel
    {
        private static readonly CharSequence ¤¤selectAll = "Select All";
        private static readonly CharSequence ¤¤selectNone = "Select None";
        private static readonly CharSequence ¤¤relavant = "Only Relevant";
        private static readonly CharSequence ¤¤toggle = "Toggle:";

        static Filter()
        {
            D.ts(typeof(Filter<>));
        }

        private bool relavant = true;
        private readonly GuiSection s = new GuiSection();
        public readonly LIST<FilterEntry<T>> all;

        public Filter(SPRITE icon, CharSequence name, LIST<FilterEntry<T>> all, LIST<FilterCombined<T>> combos) : base(icon)
        {
            hoverInfoSet(name);
            setDim(40, 40);
            this.all = all;
            s.add(new GButt.ButtPanel(¤¤selectAll)
            {
                protected override void clickA()
                {
                    relavant = false;
                    foreach (FilterEntry<T> e in all)
                    {
                        e.toggled = true;
                    }
                }
            }.setDim(200));
            s.addDown(0, new GButt.ButtPanel(¤¤selectNone)
            {
                protected override void clickA()
                {
                    relavant = false;
                    foreach (FilterEntry<T> e in all)
                    {
                        e.toggled = false;
                    }
                }
            }.setDim(200));
            s.addDown(0, new GButt.ButtPanel(¤¤relavant)
            {
                protected override void clickA()
                {
                    relavant = !relavant;
                }

                protected override void renAction()
                {
                    selectedSet(relavant);
                }
            }.setDim(200));

            GRows g = new GRows(8);

            foreach (FilterCombined<T> b in combos)
            {
                g.add(new GButt.ButtPanel(b.icon)
                {
                    protected override void clickA()
                    {
                        relavant = false;
                        foreach (FilterEntry<T> f in all)
                        {
                            f.toggled = false;
                        }
                        foreach (FilterEntry<T> f in b.all)
                        {
                            f.toggled = true;
                        }
                    }
                }.setDim(48, 48).hoverTitleSet(¤¤toggle + " " + b.name));
            }

            foreach (FilterEntry<T> b in all)
            {
                g.add(new GButt.ButtPanel(b.icon)
                {
                    protected override void clickA()
                    {
                        relavant = false;
                        b.toggled = !b.toggled;
                    }

                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        isSelected = b.toggled;
                        base.render(r, ds, isActive, isSelected, isHovered);
                        if (!b.isRelavant())
                        {
                            OPACITY.O50.bind();
                            COLOR.BLACK.render(r, body, -4);
                            OPACITY.unbind();
                        }
                    }
                }.setDim(48, 48).hoverTitleSet(¤¤toggle + " " + b.name));
            }

            s.addRelBody(8, DIR.S, new GScrollRows(g.rows(), 500).view());
        }

        protected override void clickA()
        {
            VIEW.inters().popup.show(s, this);
        }

        public bool active(FilterEntry<T> t)
        {
            if (relavant)
                return t.isRelavant();
            return t.toggled;
        }

        public abstract class FilterEntry<T>
        {
            public readonly CharSequence name;
            public readonly SPRITE icon;
            public readonly T o;

            public bool toggled = false;

            public FilterEntry(CharSequence name, SPRITE icon, T o)
            {
                this.name = name;
                this.icon = icon;
                this.o = o;
            }

            public abstract bool isRelavant();
        }

        public class FilterCombined<T>
        {
            public readonly CharSequence name;
            public readonly SPRITE icon;

            public readonly ArrayListGrower<FilterEntry<T>> all = new ArrayListGrower<FilterEntry<T>>();

            public FilterCombined(CharSequence name, SPRITE icon)
            {
                this.name = name;
                this.icon = icon;
            }
        }
    }
}