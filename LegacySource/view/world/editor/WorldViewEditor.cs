using game;
using game.faction;
using game.faction.player;
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
using world;

namespace view.world.editor
{
    public class WorldViewEditor : VIEW.ViewSubSimple
    {
        private readonly GameWindow window;
        private readonly ToolManager tools;
        private readonly ISidePanels panels;
        public static readonly ACTION loadPrint = new ACTION
        {
            exe = () =>
            {
                if (!SPRITES.loader().isMini())
                    SPRITES.loader().minify(true, Dic.¤¤Generating);
                SPRITES.loader().print(Dic.¤¤Generating);
            }
        };

        private bool hasSeletedRace = false;
        private bool hasSelectedTitles = true;

        public WorldViewEditor(GameWindow window)
        {
            foreach (PTitle t in FACTIONS.player().titles.all())
            {
                if (t.unlocked())
                    hasSelectedTitles = false;
            }

            this.window = window;
            tools = new ToolManager(uiManager, window);
            window.setZoomout(2);
            window.centererTile.set(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2);
            panels = new ISidePanels(uiManager, 0);
            reset();

            new TopPanel(this);
        }

        public override void activate()
        {
            base.activate();
            window.stop();
            WORLD.FOW().toggled.set(false);
            WORLD.GEN().isEditing = true;
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
            GAME.world().render(r, ds, window.zoomout(), window.pixels(), window.view().x1() << window.zoomout(), window.view().y1() << window.zoomout());
        }

        private void reset()
        {
            uiManager.clear();
            tools.place(null);
        }
    }
}