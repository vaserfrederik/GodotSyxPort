using System;
using System.IO;
using game;
using game.faction;
using game.save;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using view.interrupter;
using view.keyboard;
using view.main;
using view.subview;
using view.tool;
using view.ui.top;
using view.world.editor;
using view.world.panel;
using world;

namespace view.world
{
    public class WorldView : VIEW.ViewSub
    {
        public readonly GameWindow window = CreateWindow();
        public static GameWindow CreateWindow()
        {
            return new GameWindow(
                1,
                C.DIM(),
                PIXELS(),
                C.TILE_SIZE * 5
            );
        }

        public readonly ToolManager tools;
        public readonly WorldUI UI;
        public readonly ISidePanels panels = new ISidePanels(uiManager, 0);
        public readonly IDebugPanelWorld debug;
        public readonly WorldViewEditor editor;

        public WorldView()
        {
            UIPanelTop p = new UIPanelTop(uiManager);
            new UIPanelTopWorld(this, p);

            tools = new ToolManager(uiManager, window);
            UI = new WorldUI(uiManager, panels, tools);
            tools.SetDefault(new ToolDefault(tools));
            editor = new WorldViewEditor(window);
            GAME.saver().Add(new Savable("VIEW_WORLD")
            {
                protected override void Save(FilePutter file)
                {
                    window.saver.Save(file);
                }

                protected override void Load(FileGetter file)
                {
                    window.saver.Load(file);
                    WORLD.MINIMAP().Repaint();
                }
            });

            foreach (PLACABLE pl in WORLD.TERRAIN().saver().MakePlacers(tools))
            {
                IDebugPanelWorld.Add(pl, "terrain");
            }
            debug = new IDebugPanelWorld(uiManager);
        }

        public override void Activate()
        {
            if (VIEW.current() == this)
                return;
            base.Activate();

            window.Stop();
            tools.Set(null, null, true);
        }

        protected override void Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
        }

        protected override void MouseClick(MButt button)
        {
        }

        protected override void HoverTimer(double mouseTimer, GBox text)
        {
        }

        protected override bool Update(float ds, bool should)
        {
            if (KEYS.MAIN().THRONE.ConsumeClick())
            {
                window.centererTile.Set(FACTIONS.player().capitolRegion().cx(), FACTIONS.player().capitolRegion().cy());
            }
            return true;
        }

        protected override void Render(Renderer r, float ds, bool hide)
        {
            window.Crop(uiManager.viewPort());
            GAME.world().Render(r, ds, window.zoomout(), window.pixels(), window.view().x1() << window.zoomout(), window.view().y1() << window.zoomout());
        }

        protected override void AfterTick()
        {
        }
    }
}