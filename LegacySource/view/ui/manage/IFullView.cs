using System;
using snake2d.util.gui;
using snake2d.util.sprite;
using view.main;

namespace view.ui.manage
{
    public class IFullView
    {
        public const int TOP_HEIGHT = IManager.TOP_HEIGHT + 8;
        public const int WIDTH = C.WIDTH() - 32;
        public const int HEIGHT = C.HEIGHT() - TOP_HEIGHT - 8;

        public readonly string title;
        public readonly SPRITE icon;
        protected GuiSection section = new GuiSection();

        public IFullView(string name, SPRITE icon)
        {
            this.title = name;
            this.icon = icon;
        }

        public void activate()
        {
            VIEW.UI().manager.show(this);
        }

        public bool back()
        {
            return false;
        }

        public void init()
        {
        }

        public void hoverInfoGet(GUI_BOX text)
        {
            text.title(title);
        }
    }
}