using System;
using System.IO;
using game;
using init.constant;
using settlement.main;
using settlement.overlay;
using settlement.room.main.throne;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.data;
using util.gui.misc;
using util.rendering;
using view.interrupter;
using view.keyboard;
using view.main;
using view.sett.invasion;
using view.sett.ui;
using view.sett.ui.minimap;
using view.sett.ui.right;
using view.subview;
using view.tool;
using view.ui.top;

namespace view.sett
{
    public class SettView : VIEW.ViewSub
    {
        private readonly GameWindow window = new GameWindow(1, C.DIM(), SETT.PIXEL_BOUNDS, 0);
        public readonly Inters interrupters = new Inters();
        private bool hasPlaced = false;

        static SettView()
        {
            UISettMap.Clear();
        }

        private readonly SettViewStart start = new SettViewStart();
        public readonly UIPanelTopSett panel;
        public readonly SettUI ui = new SettUI(uiManager);
        public readonly ISidePanels panels;
        public readonly ToolManager tools = new ToolManager(uiManager, window);
        public readonly IDebugPanelSett debug;
        public readonly UIMinimapSett mini;
        public readonly UIPanelRightSett right;
        public readonly SBattleView battle;
        public readonly GETTER_IMP<Addable> overlayThing;

        public class Inters
        {
            public readonly InterGuisection section = new InterGuisection(uiManager);
            public readonly InterGuisection debugsection = new InterGuisection(uiManager);

            public Inters()
            {
            }
        }

        public SettView()
        {
            UIPanelTop pan = new UIPanelTop(uiManager);
            panels = new ISidePanels(uiManager, 0);
            panel = new UIPanelTopSett(ui, this, pan);

            window.SetZoomOutMax(3);
            tools.SetDefault(new ToolDefault(tools));
            debug = new IDebugPanelSett(uiManager);
            mini = new UIMinimapSett(uiManager, UIPanelTop.HEIGHT, window, new UIMinimapSettConfigExt("VIEW_SETT"));
            mini.Panel().AddScreenshot("VIEW_SETT");
            overlayThing = mini.Panel().AddOverlays();
            right = new UIPanelRightSett(mini, uiManager, window);
            battle = new SBattleView();

            GAME.saver().Add(new Savable("SETT_VIEW")
            {
                protected override void Save(FilePutter file)
                {
                    window.saver.Save(file);
                    right.Save(file);
                    file.Bool(hasPlaced);
                }

                protected override void Load(FileGetter file)
                {
                    window.saver.Load(file);
                    right.Load(file);
                    uiManager.Clear();
                    hasPlaced = file.Bool();
                    if (!hasPlaced)
                        start.Activate();
                }
            });
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
                window.CentererTile.Set(THRONE.coo());
            }

            return true;
        }

        protected override void Render(Renderer r, float ds, bool hide)
        {
            window.Crop(uiManager.ViewPort());
            s().Render(r, (float)(ds * GAME.SPEED.speed()), window, mini.config);

            if (window.ConsumeHover())
            {
                SETT.LIGHTS().RenderMouse(window.Pixel().X, window.Pixel().Y, -window.Pixels().RelX(), -window.Pixels().RelY(), 5);

                if (window.HasZoomedOutMoreAndConsumeThatMotherFZoom())
                    mini.Open();
            }
        }

        public GameWindow GetWindow()
        {
            return window;
        }

        public void ClearAllInterrupters()
        {
            uiManager.Clear();
        }

        public override void Activate()
        {
            window.Stop();
            base.Activate();
        }

        public void Clear()
        {
            hasPlaced = false;
            start.Activate();
            battle.Clear();
            right.Clear();
        }

        public override void RenderBelowTerrain(Renderer r, ShadowBatch s, RenderData data)
        {
            SETT.JOBS().Render(r, s, data);
        }
    }
}