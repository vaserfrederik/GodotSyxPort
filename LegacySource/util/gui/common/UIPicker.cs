using System;
using System.Collections.Generic;
using init.sprite.UI;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.data.INT;
using util.gui.slider;
using util.gui.table;

namespace util.gui.common
{
    public class UIPicker<T> : GuiSection where T : INDEXED, IconHaser
    {
        private INT_OE<T> g;
        private ArrayListShort all;
        private readonly LIST<T> tot;

        public UIPicker(INT_OE<T> g, int rows, LIST<T> tot)
        {
            this.all = new ArrayListShort(tot.size());
            this.g = g;
            this.tot = tot;

            GTableBuilder builder = new GTableBuilder
            {
                NrOFEntries = () => all.size()
            };

            Button b = new Button(new GETTER.GETTER_IMP<int>(0));
            G gg = new G(new GETTER.GETTER_IMP<int>(0));
            AddToRow(b, gg);

            builder.Column("", b.Body().Width(), new GRowBuilder
            {
                Build = (GETTER<int> ier) =>
                {
                    Button b = new Button(ier);
                    G gg = new G(ier);
                    AddToRow(b, gg);
                    return b;
                }
            });

            Add(builder.Create(rows, false));
        }

        protected void AddToRow(GuiSection row, GETTER<T> g)
        {
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            all.Clear();
            foreach (T res in tot)
            {
                if (g.Max(res) > 0)
                {
                    all.Add(res.Index());
                }
            }
            base.Render(r, ds);
        }

        private class G : GETTER<T>
        {
            private readonly GETTER<int> ier;

            public G(GETTER<int> ier)
            {
                this.ier = ier;
            }

            public T Get()
            {
                return tot.Get(all.Get(ier.Get()));
            }
        }

        private class Button : GuiSection
        {
            private readonly GETTER<int> ier;

            public Button(GETTER<int> ier)
            {
                this.ier = ier;

                Add(new HoverableAbs(Icon.M)
                {
                    Render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                    {
                        r().Icon().Render(r, Body);
                    },

                    HoverInfoGet = (GUI_BOX text) =>
                    {
                        text.Title(r().Name());
                    }
                });

                INTE inTE = new INTE
                {
                    Min = () => g.Min(r()),
                    Max = () => g.Max(r()),
                    Get = () => g.Get(r()),
                    Set = (int t) => g.Set(r(), t)
                };

                AddRightC(4, new GSliderIntInput(inTE));
            }

            private T r()
            {
                return tot.Get(all.Get(ier.Get()));
            }
        }
    }
}