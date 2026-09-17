using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.gui;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.tool;
using world;

namespace view.world.generator
{
    class StageEdit
    {
        static CharSequence ¤¤name = "Edit terrain";
        static
        {
            D.ts(typeof(StageEdit));
        }

        public StageEdit(WorldViewGenerator stages)
        {
            GuiSection s = new GuiSection()
            {
                public void render(SPRITE_RENDERER r, float ds)
                {
                    WORLD.OVERLAY().landmarks.add();
                    base.render(r, ds);
                }
            };

            PLACABLE first = null;
            foreach (PLACABLE p in WORLD.TERRAIN().saver().makePlacers(stages.tools))
            {
                if (first == null)
                    first = p;
                s.addRightC(0, new B(p, stages));
            }

            s.addRightC(16, new GButt.ButtPanel(UI.icons().m.ok)
            {
                protected override void clickA()
                {
                    stages.set();
                }
            });

            GPanel p = new GPanel();
            p.inner().set(s);
            s.add(p);
            s.moveLastToBack();
            s.body().centerIn(C.DIM());
            s.body().moveY1(5);

            stages.dummy.add(s, null, false);
            s.body().moveY1(10);

            stages.tools.place(first);
        }

        private static class B : GButt.ButtPanel
        {
            private readonly PLACABLE p;
            private readonly WorldViewGenerator stages;

            public B(PLACABLE p, WorldViewGenerator stages) : base(p.getIcon())
            {
                this.p = p;
                this.stages = stages;
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.title(p.name());
            }

            protected override void renAction()
            {
                selectedSet(stages.tools.placer.isActivated() && (stages.tools.placer.getCurrent() == p || stages.tools.placer.getCurrent() == p.getUndo()));
            }

            protected override void clickA()
            {
                stages.tools.place(p);
            }
        }
    }
}