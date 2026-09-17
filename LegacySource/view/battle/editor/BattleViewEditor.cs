using System;
using game;
using init.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;
using view.subview;
using view.tool;
using view.world;
using world;

namespace view.battle.editor
{
    public class BattleViewEditor : VIEW.ViewSubSimple
    {
        private readonly GameWindow window;
        private readonly ToolManager tools;
        private readonly ISidePanels panels;

        public static ACTION loadPrint = new ACTION
        {
            exe = () =>
            {
                if (!SPRITES.loader().isMini())
                    SPRITES.loader().minify(true, Dic.¤¤Generating);
                SPRITES.loader().print(Dic.¤¤Generating);
            }
        };

        public BattleViewEditor()
        {
            window = WorldView.createwindow();
            tools = new ToolManager(uiManager, window);
            window.setZoomout(2);
            window.centererTile.set(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2);
            panels = new ISidePanels(uiManager, 0);

            new Inter(uiManager);
        }

        public override void activate()
        {
            base.activate();
            window.stop();
            WORLD.FOW().toggled.set(false);
        }

        public override void deactivate()
        {
            WORLD.FOW().toggled.set(true);
        }

        protected override void hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            window.hover();
        }

        protected override void mouseClick(MButt button)
        {
        }

        protected override void hoverTimer(double mouseTimer, GBox text)
        {
        }

        protected override bool update(float ds, bool should)
        {
            return true;
        }

        protected override void render(Renderer r, float ds, bool hide)
        {
            window.crop(uiManager.viewPort());
            GAME.world().render(r, ds, window.zoomout(), window.pixels(), window.view().x1(), window.view().y1());
        }

        protected override bool canSave()
        {
            return false;
        }
    }
}