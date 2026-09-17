using System;
using view.sett.ui.room;
using settlement.main;
using snake2d;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.interrupter;

class ConstructionList : ISidePanel
{
    private const int WIDTH = 120;
    private const int XX = 4;

    public ConstructionList()
    {
        TitleSet(Dic.¤¤construction);

        GTableBuilder bu = new GTableBuilder
        {
            NrOFEntries = () => (int)Math.Ceiling((double)SETT.ROOMS().construction.instances() / XX)
        };

        for (int i = 0; i < XX; i++)
        {
            final int k = i;
            bu.Column(null, WIDTH, new GRowBuilder
            {
                Build = ier => new Entry(ier, k)
            });
        }

        section.Add(bu.CreateHeight(HEIGHT - 32, false));
    }

    private class Entry : ClickableAbs
    {
        private readonly int col;
        private readonly GETTER<int> ier;

        Entry(GETTER<int> ier, int col)
        {
            this.ier = ier;
            this.col = col;
            body.SetDim(WIDTH, 48);
        }

        protected override void ClickA()
        {
            int i = ier.Get() * XX + col;
            if (i >= SETT.ROOMS().construction.instances())
                return;

            SETT.ROOMS().construction.ClickButt(i);
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            int i = ier.Get() * XX + col;
            if (i >= SETT.ROOMS().construction.instances())
                return;

            SETT.ROOMS().construction.HoverButt((GBox)text, i);

            base.HoverInfoGet(text);
        }

        protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            int i = ier.Get() * XX + col;
            if (i >= SETT.ROOMS().construction.instances())
                return;

            GButt.ButtPanel.RenderBG(r, true, false, isHovered, body);
            SETT.ROOMS().construction.RenderButt(r, body.X1() + 8, body.CY(), i);
            GButt.ButtPanel.RenderFrame(r, body);
        }
    }
}