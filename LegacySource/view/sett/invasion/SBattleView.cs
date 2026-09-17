using System;
using System.IO;
using game;
using init.constant;
using settlement.main;
using settlement.room.main.throne;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using util.gui.misc;
using util.rendering;
using view.battle;
using view.interrupter;
using view.keyboard;
using view.main;
using view.sett.ui.minimap;
using view.subview;
using view.ui.top;

namespace view.sett.invasion
{
    public sealed class SBattleView : VIEW.ViewSub
    {
        private readonly GameWindow window = new GameWindow(1, C.DIM(), SETT.PIXEL_BOUNDS, 0);
        private readonly DivSelection selection = new DivSelection();
        private readonly BattlePlacer placer = new BattlePlacer(window, selection);
        public readonly BattleRenderer renderer = new BattleRenderer(selection);
        private readonly ISidePanels panels;
        private readonly BattlePanel panel;
        private readonly UIMinimapSett minimap;

        public SBattleView()
        {
            UIPanelTop pp = new UIPanelTop(uiManager, false, true);
            {
                GuiSection s = new GuiSection();
                s.addRightC(0, UIPanelTop.bToggle());
                pp.addRightRight(s);
            }

            panels = new ISidePanels(uiManager, 0);
            minimap = new UIMinimapSett(uiManager, UIPanelTop.HEIGHT, window, null);

            panel = new BattlePanel(panels, window, pp, selection, false);
            new UISelection(uiManager, selection, true);

            window.setzoomoutMax(3);
            GAME.saver().add(new Savable("S_BATTLEVIEW")
            {
                protected override void save(FilePutter file)
                {
                    window.saver.save(file);
                }

                protected override void load(FileGetter file)
                {
                    window.saver.load(file);
                    selection.clear();
                }
            });
        }

        protected override void hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            if (!uiManager.isHovered())
                window.hover();
        }

        protected override void mouseClick(MButt button)
        {
            placer.click(button);
        }

        protected override void hoverTimer(double mouseTimer, GBox text)
        {
            placer.hoverTimer(text);
        }

        protected override bool update(float ds, bool should)
        {
            window.update(ds);
            placer.update(!uiManager.isHovered());
            if (KEYS.MAIN().THRONE.consumeClick())
            {
                window.centererTile.set(THRONE.coo());
            }

            return true;
        }

        protected override void render(Renderer r, float ds, bool hide)
        {
            window.crop(uiManager.viewPort());
            renderer.add();

            s().render(r, ds, window, minimap.config);
            if (VIEW.hideUI())
            {
                return;
            }

            if (window.consumeHover())
            {
                SETT.LIGHTS().renderMouse(window.pixel().x(), window.pixel().y(), -window.pixels().relX(), -window.pixels().relY(), 5);

                if (window.hasZoomedOutMoreandConsumeThatMotherFZoom())
                    minimap.open();
            }
        }

        public GameWindow getWindow()
        {
            return window;
        }

        public void clearAllInterrupters()
        {
            uiManager.clear();
        }

        public override void activate()
        {
            window.stop();
            window.copy(VIEW.s().getWindow());
            base.activate();
        }

        public override void deactivate()
        {
            VIEW.s().getWindow().copy(window);
            base.deactivate();
        }

        public void clear()
        {
            selection.clear();
        }

        protected override void afterTick()
        {
            selection.clearHover();
        }

        public override void renderBelowTerrain(Renderer r, ShadowBatch s, RenderData data)
        {
            renderer.renderBelow(r, data);
        }
    }
}