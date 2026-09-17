using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.gui.panel;

namespace util.gui.misc
{
    public sealed class GButtablePanel : ClickableAbs
    {
        private readonly GuiSection s = new GuiSection();
        private bool visable = false;
        private GPanel box = new GPanel();
        private int buttI;
        
        public GButtablePanel()
        {
            box.setButt();
        }

        public void addTitle(ICharSequence s)
        {
            box.setTitle(s);
            body.set(box.body());
        }
        
        public void addButton(CLICKABLE button, int margin)
        {
            if (buttI++ >= 10)
            {
                button.body().moveX1(s.body().x1()).moveY1(s.body().y2());
                s.add(button);
                buttI = 0;
            }
            else
            {
                s.addRight(margin, button);
            }
            visable = true;

            s.body().centerX(C.DIM());
            s.body().moveY1(120);
            box.inner().set(s);
            body.set(box.body());
        }
        
        public void nl()
        {
            buttI = 100;
        }
        
        public void addButton(CLICKABLE button)
        {
            addButton(button, 0);
        }
        
        public void addButtons(params CLICKABLE[] buttons)
        {
            foreach (CLICKABLE bu in buttons)
                addButton(bu);
        }


        public void clear()
        {
            s.clear();
            visable = false;
            buttI = 0;
            box.title().clear();
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            if (visable)
            {
                box.render(r, ds);
                s.render(r, ds);
            }
        }
        
        protected override void clickA()
        {
            s.click();
        }
        
        public override bool hover(COORDINATE mCoo)
        {
            s.hover(mCoo);
            return base.hover(mCoo);
        }
        
        public override void hoverInfoGet(GUI_BOX text)
        {
            s.hoverInfoGet(text);
        }
    }
}