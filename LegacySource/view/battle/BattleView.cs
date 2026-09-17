using System;
using System.IO;
using game;
using game.battle.state;
using game.save;
using init.constant;
using settlement.entity;
using settlement.main;
using settlement.room.main.throne;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.rendering;
using view.battle.editor;
using view.interrupter;
using view.keyboard;
using view.main;
using view.sett.ui.minimap;
using view.subview;
using view.ui.top;

namespace view.battle
{
    public sealed class BattleView : VIEW.ViewSub
    {
        private readonly GameWindow window = new GameWindow(1, C.DIM(), SETT.PIXEL_BOUNDS, 0);
        private readonly DivSelection selection = new DivSelection();
        private readonly BattlePlacer placer = new BattlePlacer(window, selection);
        private readonly BattleRenderer renderer = new BattleRenderer(selection);
        public readonly ISidePanels panels;
        private readonly BattlePanel panel;
        private readonly UIMinimapSett minimap;
        private BattleState state;
        public readonly BattleViewEditor editor;

        public BattleView()
        {
            UIPanelTop pp = new UIPanelTop(uiManager, true, true);
            panels = new ISidePanels(uiManager, 0);
            minimap = new UIMinimapSett(uiManager, UIPanelTop.HEIGHT, window, null);

            panel = new BattlePanel(panels, window, pp, selection, true);
            new UISelection(uiManager, selection, false);

            window.SetZoomOutMax(3);
            new IDeploy(uiManager);
            GAME.saver().add(new Savable("BATTLE_VIEW")
            {
                protected override void Save(FilePutter file)
                {
                    window.saver.Save(file);
                }

                protected override void Load(FileGetter file)
                {
                    window.saver.Load(file);
                    selection.Clear();
                }
            });

            editor = new BattleViewEditor();
        }

        protected override void Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            if (!uiManager.IsHovered())
                window.Hover();
        }

        protected override void MouseClick(MButt button)
        {
            placer.Click(button);
        }

        protected override void HoverTimer(double mouseTimer, GBox text)
        {
            if (MButt.RIGHT.IsDown())
            {
                ENTITY e = SETT.ENTITIES().GetAtPoint(window.Pixel().X(), window.Pixel().Y());
                if (e != null)
                {
                    e.Hover(text);
                    return;
                }
            }
            placer.HoverTimer(text);
        }

        public BattleState State()
        {
            return state;
        }

        protected override bool Update(float ds, bool should)
        {
            if (state != null)
                state.Update(ds * GAME.SPEED.SpeedTarget());

            window.Update(ds);
            placer.Update(!uiManager.IsHovered());
            if (KEYS.MAIN().THRONE.ConsumeClick())
            {
                window.CentererTile.Set(THRONE.Coo());
            }

            return true;
        }

        protected override void Render(Renderer r, float ds, bool hide)
        {
            window.Crop(uiManager.ViewPort());
            renderer.Add();
            s().Render(r, ds, window, minimap.Config);
            if (VIEW.HideUI())
                return;

            if (window.ConsumeHover())
            {
                SETT.LIGHTS().RenderMouse(window.Pixel().X(), window.Pixel().Y(), -window.Pixels().RelX(), -window.Pixels().RelY(), 5);

                if (window.HasZoomedOutMoreAndConsumeThatMotherFZoom())
                    minimap.Open();
            }
        }

        protected override void AfterTick()
        {
            selection.ClearHover();
            base.AfterTick();
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

        public void Activate(BattleState state)
        {
            this.state = state;
            window.Stop();
            base.Activate();
        }

        public void Clear()
        {
            selection.Clear();
        }

        public override void RenderBelowTerrain(Renderer r, ShadowBatch s, RenderData data)
        {
            renderer.RenderBelow(r, data);
        }

        protected override bool CanSave()
        {
            return false;
        }
    }
}