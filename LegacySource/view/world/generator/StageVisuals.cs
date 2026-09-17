using System;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.text;
using view.ui.profile;

namespace view.world.generator
{
    class StageVisuals : GuiSection
    {
        static readonly CharSequence ¤¤title = "Customize Faction";

        static StageVisuals()
        {
            D.ts(typeof(StageVisuals));
        }

        public StageVisuals(WorldViewGenerator stages)
        {
            stages.reset();

            addRelBody(16, DIR.N, new UIProfile(false));

            int p = 650 - body().width();
            if (p > 0)
                pad(p / 2, 0);

            addRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤confirm)
            {
                protected override void clickA()
                {
                    stages.hasProfiled = true;
                    stages.set();
                }
            }.hoverInfoSet(Dic.¤¤confirm));

            pad(0, 8);

            stages.dummy.add(this, ¤¤title);
        }
    }
}