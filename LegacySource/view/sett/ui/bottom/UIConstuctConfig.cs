using System;
using System.Collections.Generic;
using Util.Gui.Misc;
using Util.Gui.Panel;
using Util.Gui.Renderable;
using Util.Misc;
using Util.Sets;
using Util.Sprite;
using View.Main;
using View.Tool;

namespace View.Sett.Ui.Bottom
{
    class UIConstuctConfig : ToolConfig
    {
        protected CharSequence name;
        protected GuiSection section = new GuiSection();
        private readonly GPanel panel = new GPanel();
        private GuiSection full = new GuiSection();
        ACTION exit = new ACTION()
        {
            public void Exe()
            {
                VIEW.s().tools.placer.deactivate();
            }
        };

        PLACABLE placer;

        protected UIConstuctConfig(CharSequence name)
        {
            this.name = name;
            panel.setButt();
            panel.setTitle(name);
        }

        public void Activate()
        {
            VIEW.s().tools.place(placer, this);
        }

        public bool IsActive()
        {
            return VIEW.s().tools.configCurrent() == this;
        }

        public void AddUI(LISTE<RENDEROBJ> uis)
        {
            full.Clear();

            VIEW.s().tools.placer.stealButtons(full);
            if (placer.getAdditionalButt() != null)
                foreach (CLICKABLE p in placer.getAdditionalButt())
                    full.addRightC(0, p);
            full.body().centerX(C.DIM());
            full.addRelBody(C.SG * 8, DIR.N, section);

            panel.setButt();
            panel.inner().set(full);
            panel.clickActionSet(exit);
            full.add(panel);
            full.moveLastToBack();
            full.body().moveY1(75);
            uis.add(full);
        }

        protected class Butt : GButt.Panel
        {
            private readonly PLACABLE p;

            Butt(PLACABLE p) : base(p.getIcon())
            {
                this.p = p;
                if (placer == null)
                    placer = p;
            }

            Butt(PLACABLE p, SPRITE icon) : base(icon)
            {
                this.p = p;
                if (placer == null)
                    placer = p;
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                p.hoverDesc((GBox)text);
            }

            protected override void clickA()
            {
                placer = p;
                Activate();
            }

            protected override void renAction()
            {
                selectedSet(p == placer);
            }
        }
    }
}