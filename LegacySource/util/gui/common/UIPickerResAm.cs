using System;
using System.Collections.Generic;
using init.resources;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.data.INT;
using util.gui.slider;
using util.gui.table;

namespace util.gui.common
{
    public class UIPickerResAm : GuiSection
    {
        private INT_OE<RESOURCE> g;
        private List<short> all = new List<short>(RESOURCES.ALL().Count);

        public UIPickerResAm(INT_OE<RESOURCE> g, int rows)
        {
            this.g = g;

            GTableBuilder builder = new GTableBuilder
            {
                NrOFEntries = () => all.Count
            };

            Button b = new Button(new GETTER<int>(0));
            G gg = new G(new GETTER<int>(0));
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

        protected void AddToRow(GuiSection row, GETTER<RESOURCE> g)
        {
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            all.Clear();
            foreach (RESOURCE res in RESOURCES.ALL())
            {
                if (g.Max(res) > 0)
                {
                    all.Add(res.Index());
                }
            }
            base.Render(r, ds);
        }

        private class G : GETTER<RESOURCE>
        {
            private readonly GETTER<int> ier;

            public G(GETTER<int> ier)
            {
                this.ier = ier;
            }

            public override RESOURCE Get()
            {
                return RESOURCES.ALL()[all[ier.Get()]];
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
                        r.Icon().Render(r, Body);
                    },

                    HoverInfoGet = (GUI_BOX text) =>
                    {
                        text.Title(R().Name);
                    }
                });

                INTE in = new INTE
                {
                    Min = () => g.Min(R()),
                    Max = () => g.Max(R()),
                    Get = () => g.Get(R()),
                    Set = (int t) => g.Set(R(), t)
                };

                AddRightC(4, new GSliderIntInput(in));
            }

            private RESOURCE R()
            {
                return RESOURCES.ALL()[all[ier.Get()]];
            }
        }
    }
}