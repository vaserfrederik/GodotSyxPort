using System;
using Init.Constant;
using Init.Sprite;
using Init.Sprite.UI;
using Snake2D;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Clickable;
using Snake2D.Util.Gui.Renderable;
using Util.Gui.Misc;
using View.Interrupter;
using View.World.Panel;

namespace View.World.Generator
{
    class IMinimap : Interrupter
    {
        private readonly UIMinimapW map;
        private readonly GuiSection buttons = new GuiSection();
        private readonly WorldViewGenerator v;

        public IMinimap(WorldViewGenerator v)
        {
            this.v = v;
            map = new UIMinimapW(v.window);
            Pin();
            CLICKABLE b;

            b = new GButt.Panel(SPRITES.icons().m.citizen)
            {
                protected override void ClickA()
                {
                    v.reset();
                    new StagePickRace(v);
                }

                protected override void RenAction()
                {
                    activeSet(v.canSelectRace);
                }
            }.hoverInfoSet(StagePickRace.¤¤title);
            buttons.addRight(0, b);

            b = new GButt.Panel(SPRITES.icons().m.city)
            {
                protected override void ClickA()
                {
                    v.reset();
                    new StageVisuals(v);
                }
            }.hoverInfoSet(StageVisuals.¤¤title);
            buttons.addRight(8, b);

            b = new GButt.Panel(SPRITES.icons().m.arrow_up)
            {
                protected override void ClickA()
                {
                    v.reset();
                    new StagePickTitles(v);
                }
            }.hoverInfoSet(StagePickTitles.¤¤title);
            buttons.addRightC(8, b);

            b = new GButt.Panel(SPRITES.icons().m.terrain)
            {
                protected override void ClickA()
                {
                    new StageTerrain(v);
                }
            }.hoverInfoSet(StageTerrain.¤¤title);
            buttons.addRightC(8, b);

            b = new GButt.Panel(SPRITES.icons().m.plus)
            {
                protected override void ClickA()
                {
                    if (v.window.zoomout() > 0)
                        v.window.setZoomout(v.window.zoomout() - 1);
                }

                protected override void RenAction()
                {
                    activeSet(v.window.zoomout() > 0);
                }
            };
            buttons.addRightC(32, b);

            b = new GButt.Panel(SPRITES.icons().m.minus)
            {
                protected override void ClickA()
                {
                    if (v.window.zoomout() < 3)
                        v.window.setZoomout(v.window.zoomout() + 1);
                }

                protected override void RenAction()
                {
                    activeSet(v.window.zoomout() < 3);
                }
            };
            buttons.addRightC(0, b);

            RENDEROBJ pan = new RENDEROBJ.RenderImp(map.body().width(), 32)
            {
                public override void Render(SPRITE_RENDERER r, float ds)
                {
                    UI.PANEL().butt.render(r, body, 0, DIR.S, DIR.W);
                }
            };

            buttons.body().moveX2(C.WIDTH() - 4);
            buttons.body().moveY1(0);
            pan.body().moveX2(C.WIDTH());
            buttons.add(pan);
            buttons.moveLastToBack();

            map.body().moveY1(buttons.body().y2());
            map.body().moveX2(C.DIM().width());
            show(v.uiManager);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return buttons.hover(mCoo) | map.hover(mCoo) | mCoo.isWithinRec(map.body());
        }

        protected override void mouseClick(MButt button)
        {
            if (button != MButt.LEFT)
                return;
            if (buttons.click())
                return;
            else
                map.click();
        }

        protected override bool render(Renderer r, float ds)
        {
            buttons.render(r, 0);
            map.render(r, 0);

            return true;
        }

        protected override bool update(float ds)
        {
            // TODO Auto-generated method stub
            return true;
        }

        protected override void hoverTimer(GBox text)
        {
            buttons.hoverInfoGet(text);
        }

        protected override void hide()
        {
            base.hide();
        }

        public void show()
        {
            show(v.uiManager);
        }
    }
}