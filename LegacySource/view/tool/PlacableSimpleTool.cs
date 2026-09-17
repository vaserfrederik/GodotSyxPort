using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using view.main;
using view.subview;
using view.tool;

namespace view.tool
{
    internal sealed class PlacableSimpleTool : placeFunc
    {
        private PlacableSimple placable;
        private bool clicked = false;
        int cx, cy;

        public override void updateHovered(float ds, GameWindow window, bool pressed)
        {
            clicked = MButt.LEFT.isDown();
            if (clicked && !window.pixel().isSameAs(cx, cy))
            {
                cx = window.pixel().x();
                cy = window.pixel().y();
                if (placable.isPlacable(cx, cy) == null)
                {
                    placable.place(cx, cy);
                }
            }
            else
            {
                placable.placeInfo(VIEW.hoverBox(), window.pixel().x(), window.pixel().y());
            }
        }

        public override void update(float ds, GameWindow window, bool pressed)
        {
            // TODO Auto-generated method stub
            base.update(ds, window, pressed);
        }

        public override void render(SPRITE_RENDERER r, float ds, GameWindow window)
        {
            int tx = window.pixel().x();
            int ty = window.pixel().y();

            placable.renderOverlay(tx, ty, r, ds, window);
            CharSequence problem = placable.isPlacable(tx, ty);

            if (problem == null)
            {
                placable.renderPlaceHolder(r, window.pixel().rel().x(), window.pixel().rel().y(), false);
            }
            else
            {
                placable.renderPlaceHolder(r, window.pixel().rel().x(), window.pixel().rel().y(), true);
                VIEW.hoverBox().error(problem);
            }
            placable.renderAction(tx, ty);
            COLOR.unbind();
        }

        public override void click(GameWindow window)
        {
            int tx = window.pixel().x();
            int ty = window.pixel().y();

            CharSequence problem = placable.isPlacable(tx, ty);
            if (problem != null)
                return;

            placable.place(tx, ty);
            clicked = true;
            cx = tx;
            cy = ty;
        }

        public override void activate(PLACABLE placer, GameWindow window)
        {
            placable = (PlacableSimple)placer;
        }

        public override void clickRelease(GameWindow window)
        {
        }

        public override LIST<CLICKABLE> gui()
        {
            return null;
        }
    }
}