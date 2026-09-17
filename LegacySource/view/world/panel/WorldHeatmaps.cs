using System.Collections.Generic;
using init.sprite;
using snake2d;
using util.gui.misc;
using util.text;
using view.main;
using world;
using world.overlay;

namespace view.world.panel
{
    final class WorldHeatmaps : GButt.ButtPanel
    {
        private readonly GuiSection s;
        private OverlayTileNormal selected = null;

        public WorldHeatmaps() : base(SPRITES.icons().s.eye)
        {
            s = new GuiSection();
            hoverInfoSet(Dic.¤¤Overlays);

            {
                GButt.Checkbox c = new GButt.Checkbox(Dic.¤¤name)
                {
                    protected override void clickA()
                    {
                        WORLD.OVERLAY().regNames.active.toggle();
                    }

                    protected override void renAction()
                    {
                        selectedSet(WORLD.OVERLAY().regNames.active.is());
                    }
                };
                s.addDown(0, c);
            }

            foreach (OverlayTileNormal o in WORLD.OVERLAY().togglable)
            {
                GButt.ButtPanel c = new GButt.ButtPanel(o.info.name)
                {
                    protected override void clickA()
                    {
                        if (selected == o)
                            selected = null;
                        else
                            selected = o;
                    }

                    protected override void renAction()
                    {
                        selectedSet(selected == o);
                    }
                };
                c.hoverSet(o.info);
                c.body().setWidth(200);
                s.addDown(0, c);
            }
        }

        protected override void clickA()
        {
            VIEW.inters().popup.show(s, this);
        }

        protected override void renAction()
        {
            if (hoveredIs() && MButt.RIGHT.consumeClick())
            {
                selected = null;
            }

            if (selected != null)
                selected.add();

            selectedSet(selected != null);
        }
    }
}