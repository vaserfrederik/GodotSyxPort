using System;
using System.Collections.Generic;
using game.faction;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.world.ui;
using world.entity.army;

namespace util.gui.common
{
    public abstract class UIPickerArmy : GuiSection
    {
        protected abstract bool canBePicked(WArmy a);
        protected abstract void pick(WArmy a);

        private readonly GETTER<? extends Faction> g;

        public UIPickerArmy(GETTER<? extends Faction> g, int height)
        {
            this.g = g;
            GTableBuilder builder = new GTableBuilder
            {
                nrOFEntries = () =>
                {
                    if (g.get() == null)
                        return 0;
                    return g.get().armies().all().size();
                }
            };

            builder.column("", 250, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new Button(ier);
                }
            });

            add(builder.createHeight(height, false));
        }

        private class Button : ClickableAbs
        {
            private readonly GETTER<int> ier;

            public Button(GETTER<int> ier)
            {
                this.ier = ier;
                body.setWidth(250);
                body.setHeight(40);
            }

            private WArmy a()
            {
                return g.get().armies().all().get(ier.get());
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                GButt.ButtPanel.renderBG(r, true, false, isHovered, body);
                SPRITES.icons().s.sword.renderCY(r, body().x1() + 6, body().cY());

                GCOLOR.T().H1.bind();

                UI.FONT().H2.render(r, a().name, body().x1() + 24, body().cY() - UI.FONT().H2.height() / 2);
                if (!canBePicked(a()))
                {
                    OPACITY.O35.bind();
                    COLOR.BLACK.render(r, body, -4);
                    OPACITY.unbind();
                }
                GButt.ButtPanel.renderFrame(r, body);
            }

            protected override void clickA()
            {
                if (canBePicked(a()))
                    pick(a());
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                UIPickerArmy.this.hover(text, a());
            }
        }

        public void hover(GUI_BOX text, WArmy a)
        {
            WorldHoverer.hover(text, a);
        }
    }
}