using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.main;
using world;
using world.map.regions;

namespace util.gui.common
{
    public abstract class UIPickerRegion : GuiSection
    {
        protected virtual bool active(Region reg)
        {
            return true;
        }

        protected virtual bool selected(Region reg)
        {
            return false;
        }

        protected abstract void toggle(Region reg);

        protected virtual void hoverInfo(GBox b, Region reg)
        {
            VIEW.World().UI.Regions.hover(reg, b);
        }

        private GETTER<Faction> g;

        public UIPickerRegion(GETTER<Faction> g, int height)
        {
            this.g = g;

            GTableBuilder builder = new GTableBuilder
            {
                NrOfEntries = () =>
                {
                    if (g.Get() == null)
                        return 0;

                    return g.Get().Realm.Regions;
                }
            };

            builder.Column("", 264, new GRowBuilder
            {
                Build = (ier) =>
                {
                    return new Button(ier);
                }
            });

            Add(builder.CreateHeight(height, false));
        }

        private class Button : ClickableAbs
        {
            private readonly GETTER<int> ier;

            public Button(GETTER<int> ier)
            {
                this.ier = ier;
                body.setWidth(264);
                body.setHeight(40);
            }

            private Region r()
            {
                return g.Get().Realm.Region(ier.Get());
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                Region reg = r();
                if (reg == null)
                    return;

                isSelected = selected(reg);
                GButt.ButtPanel.RenderBG(r, isActive, isSelected, isHovered, body);

                SPRITE b = FBanner.Rebel.MEDIUM;
                if (reg.Faction != null)
                    b = reg.Faction.Banner.MEDIUM;

                b.RenderCY(r, body().x1() + 8, body().cY());

                GCOLOR.T().H1.Bind();
                UI.FONT().H2.Render(r, reg.Info.Name, body().x1() + 40, body().cY() - UI.FONT().H2.Height / 2);

                if (!active(reg))
                {
                    OPACITY.O35.Bind();
                    COLOR.BLACK.Render(r, body, -4);
                    OPACITY.Unbind();
                }

                GButt.ButtPanel.RenderFrame(r, body);
            }

            protected override void ClickA()
            {
                Region reg = r();
                if (reg == null)
                    return;

                if (active(reg))
                    toggle(reg);
            }

            public override bool Hover(COORDINATE mCoo)
            {
                if (base.Hover(mCoo))
                {
                    Region reg = r();
                    if (reg != null)
                        WORLD.MINIMAP().Hilight(reg);
                    return true;
                }
                return false;
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                Region reg = r();
                if (reg == null)
                    return;
                ((UIPickerRegion)this.hoverInfo((GBox)text, reg);
            }
        }
    }
}