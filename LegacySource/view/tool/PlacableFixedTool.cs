using System;
using System.Collections.Generic;
using init.constant;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.text;
using view.keyboard;
using view.main;
using view.subview;
using view.tool;

namespace view.tool
{
    internal sealed class PlacableFixedTool : placeFunc
    {
        private PlacableFixed placable;

        private readonly ArrayList<CLICKABLE> butts = new ArrayList<CLICKABLE>(3);
        private readonly CLICKABLE bIncrease = KeyButt.Wrap(new GButt.Panel(SPRITES.icons().m.plus)
        {
            { hoverInfoSet(KEYS.MAIN().MOD.Repr() + Dic.¤¤MouseWheelAdd); }
            protected override void RenAction()
            {
                activeSet(placable.size() < placable.sizes() - 1);
            }
            protected override void ClickA()
            {
                if (placable.size() < placable.sizes() - 1)
                    placable.sizeSet(placable.size() + 1);
            }
        }, KEYS.MAIN().GROW);
        private readonly CLICKABLE bDecrease = KeyButt.Wrap(new GButt.Panel(SPRITES.icons().m.minus)
        {
            { hoverInfoSet(KEYS.MAIN().MOD.Repr() + Dic.¤¤MouseWheelAdd); }
            protected override void RenAction()
            {
                activeSet(placable.size() > 0);
            }
            protected override void ClickA()
            {
                if (placable.size() > 0)
                    placable.sizeSet(placable.size() - 1);
            }
        }, KEYS.MAIN().SHRINK);
        private readonly CLICKABLE bRotate = KeyButt.Wrap(new GButt.Panel(SPRITES.icons().m.rotate)
        {
            protected override void ClickA()
            {
                int r = placable.rot() + 1;
                r %= placable.rotations();
                placable.rotSet(r);
            }
        }, KEYS.MAIN().ROTATE);

        public override void UpdateHovered(float ds, GameWindow window, bool pressed)
        {
            double s = MButt.PeekWheel();
            if (KEYS.MAIN().MOD.IsPressed() && s != 0)
            {
                if (s > 0 && placable.size() < placable.sizes() - 1)
                {
                    placable.sizeSet(placable.size() + 1);
                }
                else if (s < 0 && placable.size() > 0)
                {
                    placable.sizeSet(placable.size() - 1);
                }
                MButt.ClearWheelSpin();
            }

            if (KEYS.MAIN().GROW.ConsumeClick() && placable.size() < placable.sizes() - 1)
            {
                placable.sizeSet(placable.size() + 1);
            }
            else if (KEYS.MAIN().SHRINK.ConsumeClick() && placable.size() > 0)
            {
                placable.sizeSet(placable.size() - 1);
            }

            if (KEYS.MAIN().ROTATE.ConsumeClick())
            {
                int r = placable.rot() + 1;
                r %= placable.rotations();
                placable.rotSet(r);
            }

            if (pressed)
            {
                Click(window);
            }
        }

        public override void Update(float ds, GameWindow window, bool pressed)
        {
            placable.updateRegardless(window);
            base.Update(ds, window, pressed);
        }

        public override void Render(SPRITE_RENDERER r, float ds, GameWindow window)
        {
            placable.init(window.tile().x(), window.tile().y());

            int w = placable.width();
            int h = placable.height();

            int x1 = window.tile().x() - w / 2;
            int y1 = window.tile().y() - h / 2;

            CharSequence pError = placable.placableWhole(x1, y1);
            CharSequence e = null;

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    CharSequence e2 = placable.placable(x1 + dx, y1 + dy, dx, dy);
                    if (e2 != null)
                    {
                        e = e2;
                    }
                }
            }

            COLOR normal = pError == null && e == null ? GCOLOR.MAP().OK : GCOLOR.MAP().SOSO;

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    CharSequence e2 = placable.placable(x1 + dx, y1 + dy, dx, dy);
                    if (e2 != null)
                    {
                        GCOLOR.MAP().BAD.Bind();
                    }
                    else
                    {
                        normal.Bind();
                    }
                    int x = window.tile().rel().x() + (-w / 2 + dx) * C.TILE_SIZE;
                    int y = window.tile().rel().y() + (-h / 2 + dy) * C.TILE_SIZE;

                    int m = 0;
                    if (dx == 0)
                        m |= DIR.W.mask();
                    if (dx == w - 1)
                        m |= DIR.E.mask();
                    if (dy == 0)
                        m |= DIR.N.mask();
                    if (dy == h - 1)
                        m |= DIR.S.mask();

                    m = ~m;
                    m &= 0x0F;

                    placable.renderPlaceHolder(r, m, x, y, x1 + dx, y1 + dy, dx, dy, e2 == null, pError == null);
                }
            }
            COLOR.Unbind();
            int dist = (int)Math.Ceiling(h / 2.0 + 1);
            dist *= C.TILE_SIZE;
            dist = dist >> CORE.renderer().getZoomout();
            VIEW.hoverBoxDistance(dist);
            if (pError != null && pError.Length > 0)
                VIEW.hoverBox().error(pError);
            else if (e != null && e.Length > 0)
                VIEW.hoverBox().error(e);
            else
                placable.placeInfo(VIEW.hoverBox(), x1, y1);
        }

        public override void Click(GameWindow window)
        {
            placable.init(window.tile().x(), window.tile().y());

            int w = placable.width();
            int h = placable.height();

            int x1 = window.tile().x() - w / 2;
            int y1 = window.tile().y() - h / 2;

            if (placable.placableWhole(x1, y1) != null)
                return;

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    if (placable.placable(x1 + dx, y1 + dy, dx, dy) != null)
                        return;
                }
            }

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    placable.place(x1 + dx, y1 + dy, dx, dy);
                }
            }

            placable.afterPlaced(x1, y1);
        }

        public override void Activate(PLACABLE placer, GameWindow window)
        {
            placable = (PlacableFixed)placer;
        }

        public override void ClickRelease(GameWindow window)
        {
        }

        public override LIST<CLICKABLE> Gui()
        {
            butts.Clear();
            if (placable.sizes() > 1)
            {
                butts.Add(bDecrease);
                butts.Add(bIncrease);
            }

            if (placable.rotations() > 1)
                butts.Add(bRotate);

            return butts;
        }
    }
}