using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.common;
using util.gui.slider;
using util.gui.table;

namespace util.gui.common
{
    public class UIPickerRaceAm : GuiSection
    {
        private INT_OE<Race> g;
        private List<short> all = new List<short>(RACES.all().Count);

        public UIPickerRaceAm(INT_OE<Race> g, int rows)
        {
            this.g = g;

            GTableBuilder builder = new GTableBuilder
            {
                NrOFEntries = () => all.Count
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

        protected void AddToRow(GuiSection row, GETTER<Race> g)
        {
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            all.Clear();
            foreach (Race res in RACES.all())
                if (g.Max(res) > 0)
                    all.Add(res.Index());
            base.Render(r, ds);
        }

        private class G : GETTER<Race>
        {
            private readonly GETTER<int> ier;

            public G(GETTER<int> ier)
            {
                this.ier = ier;
            }

            public override Race Get()
            {
                return RACES.all()[all[ier.Get()]];
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
                        r().Appearance().Icon.Render(r, Body);
                    },
                    HoverInfoGet = (GUI_BOX text) =>
                    {
                        text.Title(r().Info.Names);
                    }
                });

                INTE @in = new INTE
                {
                    Min = () => g.Min(r()),
                    Max = () => g.Max(r()),
                    Get = () => g.Get(r()),
                    Set = (int t) => g.Set(r(), t)
                };

                AddRightC(4, new GSliderIntInput(@in));
            }

            private Race r()
            {
                return RACES.all()[all[ier.Get()]];
            }
        }
    }
}